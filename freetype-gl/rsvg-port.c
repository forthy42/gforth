/****************************************************************************
 *
 * rsvg-port.c
 *
 *   Librsvg-based hook functions for OT-SVG rendering in FreeType
 *   (implementation).
 *
 * Copyright (C) 2022-2024 by
 * David Turner, Robert Wilhelm, Werner Lemberg, and Moazin Khatti.
 *
 * This file is part of the FreeType project, and may only be used,
 * modified, and distributed under the terms of the FreeType project
 * license, LICENSE.TXT.  By continuing to use, modify, or distribute
 * this file you indicate that you have read the license and
 * understand and accept it fully.
 *
 */

#include <ft2build.h>
#include <freetype/otsvg.h>

#ifdef HAVE_LIBRSVG

#include <cairo.h>
#include <librsvg/rsvg.h>
#include <stdlib.h>
#include <math.h>

#include <freetype/freetype.h>
#include <freetype/ftbbox.h>

#include "rsvg-port.h"


  /*
   * The init hook is called when the first OT-SVG glyph is rendered.  All
   * we do is to allocate an internal state structure and set the pointer in
   * `library->svg_renderer_state`.  This state structure becomes very
   * useful to cache some of the results obtained by one hook function that
   * the other one might use.
   */
  FT_Error
  rsvg_port_init( FT_Pointer  *state )
  {
    /* allocate the memory upon initialization */
    *state = malloc( sizeof( Rsvg_Port_StateRec ) ); /* XXX error handling */

    return FT_Err_Ok;
  }


  /*
   * Deallocate the state structure.
   */
  void
  rsvg_port_free( FT_Pointer  *state )
  {
    free( *state );
  }


  /*
   * The render hook.  The job of this hook is to simply render the glyph in
   * the buffer that has been allocated on the FreeType side.  Here we
   * simply use the recording surface by playing it back against the
   * surface.
   */
  FT_Error
  rsvg_port_render( FT_GlyphSlot  slot,
                    FT_Pointer   *_state )
  {
    FT_Error  error = FT_Err_Ok;

    Rsvg_Port_State   state;
    cairo_status_t    status;
    cairo_t          *cr;
    cairo_surface_t  *surface;


    state = *(Rsvg_Port_State*)_state;

    /* Create an image surface to store the rendered image.  However,   */
    /* don't allocate memory; instead use the space already provided in */
    /* `slot->bitmap.buffer`.                                           */
    surface = cairo_image_surface_create_for_data( slot->bitmap.buffer,
                                                   CAIRO_FORMAT_ARGB32,
                                                   (int)slot->bitmap.width,
                                                   (int)slot->bitmap.rows,
                                                   slot->bitmap.pitch );
    status = cairo_surface_status( surface );

    if ( status != CAIRO_STATUS_SUCCESS )
    {
      if ( status == CAIRO_STATUS_NO_MEMORY )
        return FT_Err_Out_Of_Memory;
      else
        return FT_Err_Invalid_Outline;
    }

    cr     = cairo_create( surface );
    status = cairo_status( cr );

    if ( status != CAIRO_STATUS_SUCCESS )
    {
      if ( status == CAIRO_STATUS_NO_MEMORY )
        return FT_Err_Out_Of_Memory;
      else
        return FT_Err_Invalid_Outline;
    }

    /* Set a translate transform that translates the points in such a way */
    /* that we get a tight rendering with least redundant white spac.     */
    cairo_translate( cr, -state->x, -state->y );

    /* Replay from the recorded surface.  This saves us from parsing the */
    /* document again and redoing what was already done in the preset    */
    /* hook.                                                             */
    cairo_set_source_surface( cr, state->rec_surface, 0.0, 0.0 );
    cairo_paint( cr );

    cairo_surface_flush( surface );

    slot->bitmap.pixel_mode = FT_PIXEL_MODE_BGRA;
    slot->bitmap.num_grays  = 256;
    slot->format            = FT_GLYPH_FORMAT_BITMAP;

    /* Clean up everything. */
    cairo_surface_destroy( surface );
    cairo_destroy( cr );
    cairo_surface_destroy( state->rec_surface );

    return error;
  }


  /*
   * This hook is called at two different locations.  Firstly, it is called
   * when presetting the glyphslot when `FT_Load_Glyph` is called.
   * Secondly, it is called right before the render hook is called.  When
   * `cache` is false, it is the former, when `cache` is true, it is the
   * latter.
   *
   * The job of this function is to preset the slot setting the width,
   * height, pitch, `bitmap.left`, and `bitmap.top`.  These are all
   * necessary for appropriate memory allocation, as well as ultimately
   * compositing the glyph later on by client applications.
   */
  FT_Error
  rsvg_port_preset_slot( FT_GlyphSlot  slot,
                         FT_Bool       cache,
                         FT_Pointer   *_state )
  {
    /* FreeType variables. */
    FT_Error  error = FT_Err_Ok;

    FT_SVG_Document  document = (FT_SVG_Document)slot->other;
    FT_Size_Metrics  metrics  = document->metrics;

    FT_UShort  units_per_EM   = document->units_per_EM;
    FT_UShort  end_glyph_id   = document->end_glyph_id;
    FT_UShort  start_glyph_id = document->start_glyph_id;

    /* Librsvg variables. */
    GError   *gerror = NULL;
    gboolean  ret;

    gboolean  out_has_width;
    gboolean  out_has_height;
    gboolean  out_has_viewbox;

    RsvgHandle         *handle;
    RsvgLength         out_width;
    RsvgLength         out_height;
    RsvgRectangle      out_viewbox;
    RsvgDimensionData  dimension_svg;

    cairo_t        *rec_cr;
    cairo_matrix_t  transform_matrix;

    /* Rendering port's state. */
    Rsvg_Port_State     state;
    Rsvg_Port_StateRec  state_dummy;

    /* General variables. */
    double  x, y;
    double  xx, xy, yx, yy;
    double  x0, y0;
    double  width, height;
    double  x_svg_to_out, y_svg_to_out;
    double  tmpd;

    float metrics_width, metrics_height;
    float horiBearingX, horiBearingY;
    float vertBearingX, vertBearingY;
    float tmpf;

    char  *id;
    char  str[32];


    /* If `cache` is `TRUE` we store calculations in the actual port */
    /* state variable, otherwise we just create a dummy variable and */
    /* store there.  This saves us from too many 'if' statements.    */
    if ( cache )
      state = *(Rsvg_Port_State*)_state;
    else
      state = &state_dummy;

    /* Form an `RsvgHandle` by loading the SVG document. */
    handle = rsvg_handle_new_from_data( document->svg_document,
                                        document->svg_document_length,
                                        &gerror );
    if ( handle == NULL )
    {
      error = FT_Err_Invalid_SVG_Document;
      goto CleanLibrsvg;
    }

    /* Get attributes like `viewBox` and `width`/`height`. */
    rsvg_handle_get_intrinsic_dimensions( handle,
                                          &out_has_width,
                                          &out_width,
                                          &out_has_height,
                                          &out_height,
                                          &out_has_viewbox,
                                          &out_viewbox );

    /*
     * Figure out the units in the EM square in the SVG document.  This is
     * specified by the `ViewBox` or the `width`/`height` attributes, if
     * present, otherwise it should be assumed that the units in the EM
     * square are the same as in the TTF/CFF outlines.
     *
     * TODO: I'm not sure what the standard says about the situation if
     * `ViewBox` as well as `width`/`height` are present; however, I've
     * never seen that situation in real fonts.
     */
    if ( out_has_viewbox == TRUE )
    {
      dimension_svg.width  = (int)out_viewbox.width; /* XXX rounding? */
      dimension_svg.height = (int)out_viewbox.height;
    }
    else if ( out_has_width == TRUE && out_has_height == TRUE )
    {
      dimension_svg.width  = (int)out_width.length; /* XXX rounding? */
      dimension_svg.height = (int)out_height.length;

      /*
       * librsvg 2.53+ behavior, on SVG doc without explicit width/height.
       * See `rsvg_handle_get_intrinsic_dimensions` section in
       * the `librsvg/rsvg.h` header file.
       */
      if ( out_width.length  == 1 &&
           out_height.length == 1 )
      {
        dimension_svg.width  = units_per_EM;
        dimension_svg.height = units_per_EM;
      }
    }
    else
    {
      /*
       * If neither `ViewBox` nor `width`/`height` are present, the
       * `units_per_EM` in SVG coordinates must be the same as
       * `units_per_EM` of the TTF/CFF outlines.
       *
       * librsvg up to 2.52 behavior, on SVG doc without explicit
       * width/height.
       */
      dimension_svg.width  = units_per_EM;
      dimension_svg.height = units_per_EM;
    }

    /* Scale factors from SVG coordinates to the needed output size. */
    x_svg_to_out = (double)metrics.x_ppem / dimension_svg.width;
    y_svg_to_out = (double)metrics.y_ppem / dimension_svg.height;

    /*
     * Create a cairo recording surface.  This is done for two reasons.
     * Firstly, it is required to get the bounding box of the final drawing
     * so we can use an appropriate translate transform to get a tight
     * rendering.  Secondly, if `cache` is true, we can save this surface
     * and later replay it against an image surface for the final rendering.
     * This saves us from loading and parsing the document again.
     */
    state->rec_surface =
      cairo_recording_surface_create( CAIRO_CONTENT_COLOR_ALPHA, NULL );

    rec_cr = cairo_create( state->rec_surface );

    /*
     * We need to take into account any transformations applied.  The end
     * user who applied the transformation doesn't know the internal details
     * of the SVG document.  Thus, we expect that the end user should just
     * write the transformation as if the glyph is a traditional one.  We
     * then do some maths on this to get the equivalent transformation in
     * SVG coordinates.
     */
    xx =  (double)document->transform.xx / ( 1 << 16 );
    xy = -(double)document->transform.xy / ( 1 << 16 );
    yx = -(double)document->transform.yx / ( 1 << 16 );
    yy =  (double)document->transform.yy / ( 1 << 16 );

    x0 =  (double)document->delta.x / 64 *
            dimension_svg.width / metrics.x_ppem;
    y0 = -(double)document->delta.y / 64 *
            dimension_svg.height / metrics.y_ppem;

    /* Cairo stores both transformation and translation in one matrix. */
    transform_matrix.xx = xx;
    transform_matrix.yx = yx;
    transform_matrix.xy = xy;
    transform_matrix.yy = yy;
    transform_matrix.x0 = x0;
    transform_matrix.y0 = y0;

    /* Set up a scale transformation to scale up the document to the */
    /* required output size.                                         */
    cairo_scale( rec_cr, x_svg_to_out, y_svg_to_out );
    /* Set up a transformation matrix. */
    cairo_transform( rec_cr, &transform_matrix );

    /* If the document contains only one glyph, `start_glyph_id` and */
    /* `end_glyph_id` have the same value.  Otherwise `end_glyph_id` */
    /* is larger.                                                    */
    if ( start_glyph_id < end_glyph_id )
    {
      /* Render only the element with its ID equal to `glyph<ID>`. */
      sprintf( str, "#glyph%u", slot->glyph_index );
      id = str;
    }
    else
    {
      /* NULL = Render the whole document */
      id = NULL;
    }

#if LIBRSVG_CHECK_VERSION( 2, 52, 0 )
    {
      RsvgRectangle  viewport =
      {
        .x = 0,
        .y = 0,
        .width  = (double)dimension_svg.width,
        .height = (double)dimension_svg.height,
      };


      ret = rsvg_handle_render_layer( handle,
                                      rec_cr,
                                      id,
                                      &viewport,
                                      NULL );
    }
#else
    ret = rsvg_handle_render_cairo_sub( handle, rec_cr, id );
#endif

    if ( ret == FALSE )
    {
      error = FT_Err_Invalid_SVG_Document;
      goto CleanCairo;
    }

    /* Get the bounding box of the drawing. */
    cairo_recording_surface_ink_extents( state->rec_surface, &x, &y,
                                         &width, &height );

    /* We store the bounding box's `x` and `y` values so that the render */
    /* hook can apply a translation to get a tight rendering.            */
    state->x = x;
    state->y = y;

    /* Preset the values. */
    slot->bitmap_left = (FT_Int) state->x;  /* XXX rounding? */
    slot->bitmap_top  = (FT_Int)-state->y;

    /* Do conversion in two steps to avoid 'bad function cast' warning. */
    tmpd               = ceil( height );
    slot->bitmap.rows  = (unsigned int)tmpd;
    tmpd               = ceil( width );
    slot->bitmap.width = (unsigned int)tmpd;

    slot->bitmap.pitch = (int)slot->bitmap.width * 4;

    slot->bitmap.pixel_mode = FT_PIXEL_MODE_BGRA;

    /* Compute all the bearings and set them correctly.  The outline is */
    /* scaled already, we just need to use the bounding box.            */
    metrics_width  = (float)width;
    metrics_height = (float)height;

    horiBearingX = (float) state->x;
    horiBearingY = (float)-state->y;

    vertBearingX = slot->metrics.horiBearingX / 64.0f -
                     slot->metrics.horiAdvance / 64.0f / 2;
    vertBearingY = ( slot->metrics.vertAdvance / 64.0f -
                       slot->metrics.height / 64.0f ) / 2; /* XXX parentheses correct? */

    /* Do conversion in two steps to avoid 'bad function cast' warning. */
    tmpf                 = roundf( metrics_width * 64 );
    slot->metrics.width  = (FT_Pos)tmpf;
    tmpf                 = roundf( metrics_height * 64 );
    slot->metrics.height = (FT_Pos)tmpf;

    slot->metrics.horiBearingX = (FT_Pos)( horiBearingX * 64 ); /* XXX rounding? */
    slot->metrics.horiBearingY = (FT_Pos)( horiBearingY * 64 );
    slot->metrics.vertBearingX = (FT_Pos)( vertBearingX * 64 );
    slot->metrics.vertBearingY = (FT_Pos)( vertBearingY * 64 );

    if ( slot->metrics.vertAdvance == 0 )
      slot->metrics.vertAdvance = (FT_Pos)( metrics_height * 1.2f * 64 );

    /* If a render call is to follow, just destroy the context for the */
    /* recording surface since no more drawing will be done on it.     */
    /* However, keep the surface itself for use by the render hook.    */
    if ( cache )
    {
      cairo_destroy( rec_cr );
      goto CleanLibrsvg;
    }

    /* Destroy the recording surface as well as the context. */
  CleanCairo:
    cairo_surface_destroy( state->rec_surface );
    cairo_destroy( rec_cr );

  CleanLibrsvg:
    /* Destroy the handle. */
    g_object_unref( handle );

    return error;
  }


  SVG_RendererHooks  rsvg_hooks = {
                       (SVG_Lib_Init_Func)rsvg_port_init,
                       (SVG_Lib_Free_Func)rsvg_port_free,
                       (SVG_Lib_Render_Func)rsvg_port_render,
                       (SVG_Lib_Preset_Slot_Func)rsvg_port_preset_slot
                     };

#else /* !HAVE_LIBRSVG */

  SVG_RendererHooks  rsvg_hooks = { NULL, NULL, NULL, NULL };

#endif /* !HAVE_LIBRSVG */


/* End */
