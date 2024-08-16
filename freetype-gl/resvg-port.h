/****************************************************************************
 *
 * resvg-port.h
 *
 *   Libresvg based hook functions for OT-SVG rendering in FreeType
 *   (headers).
 * 
 * Copyright (C) 2024 by Bernd Paysan
 *
 * This is based on rsvg-port.h by heavily editing it. rsvg-port.h is
 *
 * Copyright (C) 2022-2024 by
 * David Turner, Robert Wilhelm, Werner Lemberg, and Moazin Khatti.
 *
 * This file is part of the FreeType project, and may only be used,
 * modified, and distributed under the terms of the FreeType project
 * license, FTL.TXT.  By continuing to use, modify, or distribute
 * this file you indicate that you have read the license and
 * understand and accept it fully.
 *
 */

#ifndef RSVG_PORT_H
#define RSVG_PORT_H

#include <ft2build.h>
#include <freetype/otsvg.h>

#ifdef HAVE_LIBRESVG

#include <resvg.h>
#include <freetype/freetype.h>


  /*
   * Different hook functions can access persisting data by creating a state
   * structure and putting its address in `library->svg_renderer_state`.
   * Functions can then store and retrieve data from this structure.
   */
  typedef struct Resvg_Port_StateArray_
  {
    unsigned int start_id, end_id;
    resvg_render_tree *tree;
  } Resvg_Port_StateArray;
  typedef struct Resvg_Port_StateRec_
  {
    unsigned int len;
    Resvg_Port_StateArray* array;
  } Resvg_Port_StateRec;

  typedef struct Resvg_Port_StateRec_*  Resvg_Port_State;


  FT_Error
  resvg_port_init( FT_Pointer  *state );

  void
  resvg_port_free( FT_Pointer  *state );

  FT_Error
  resvg_port_render( FT_GlyphSlot  slot,
		     FT_Pointer   *state );

  FT_Error
  resvg_port_preset_slot( FT_GlyphSlot  slot,
			  FT_Bool       cache,
			  FT_Pointer   *state );

#endif /* HAVE_LIBRSVG */


  extern SVG_RendererHooks  resvg_hooks;

#endif /* RSVG_PORT_H */


/* End */
