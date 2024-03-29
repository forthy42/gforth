/* Freetype GL - A C OpenGL Freetype engine
 *
 * Distributed under the OSI-approved BSD 2-Clause License.  See accompanying
 * file `LICENSE` for more details.
 */
#ifndef __FREETYPE_GL_H__
#define __FREETYPE_GL_H__

/* Mandatory */
#include "platform.h"
#include "vec234.h"
#include "vector.h"
#include "texture-atlas.h"
#include "texture-font.h"
#include "ftgl-utils.h"
#ifdef VERTEX_BUFFER
#include "opengl.h"
#include "vertex-buffer.h"
#endif
#ifdef HAVE_LIBRESVG
#include "resvg-port.h"
#endif

#ifdef IMPLEMENT_FREETYPE_GL
#include "platform.c"
#include "texture-atlas.c"
#include "texture-font.c"
#include "vector.c"
#include "utf8-utils.c"
#include "distance-field.c"
#include "edtaa3func.c"
#include "ftgl-utils.c"
#ifdef VERTEX_BUFFER
#include "vertex-attribute.c"
#include "vertex-buffer.c"
#endif
#ifdef HAVE_LIBRESVG
#include "resvg-port.c"
#endif
#endif

#ifdef __cplusplus
#ifndef NOT_USING_FT_GL_NAMESPACE
using namespace ftgl;
#endif /* NOT_USING_FT_GL_NAMESPACE */
#endif /* __cplusplus */

#endif /* FREETYPE_GL_H */
