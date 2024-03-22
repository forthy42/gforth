/****************************************************************************
 *
 * rsvg-port.h
 *
 *   Librsvg based hook functions for OT-SVG rendering in FreeType
 *   (headers).
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

#ifndef RSVG_PORT_H
#define RSVG_PORT_H

#include <ft2build.h>
#include <freetype/otsvg.h>

#ifdef HAVE_LIBRSVG

#include <cairo.h>
#include <librsvg/rsvg.h>
#include <freetype/freetype.h>


  /*
   * Different hook functions can access persisting data by creating a state
   * structure and putting its address in `library->svg_renderer_state`.
   * Functions can then store and retrieve data from this structure.
   */
  typedef struct  Rsvg_Port_StateRec_
  {
    cairo_surface_t  *rec_surface;

    double  x;
    double  y;

  } Rsvg_Port_StateRec;

  typedef struct Rsvg_Port_StateRec_*  Rsvg_Port_State;


  FT_Error
  rsvg_port_init( FT_Pointer  *state );

  void
  rsvg_port_free( FT_Pointer  *state );

  FT_Error
  rsvg_port_render( FT_GlyphSlot  slot,
                    FT_Pointer   *state );

  FT_Error
  rsvg_port_preset_slot( FT_GlyphSlot  slot,
                         FT_Bool       cache,
                         FT_Pointer   *state );

#endif /* HAVE_LIBRSVG */


  extern SVG_RendererHooks  rsvg_hooks;

#endif /* RSVG_PORT_H */


/* End */
