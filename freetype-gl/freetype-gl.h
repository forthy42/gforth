/* Freetype GL - A C OpenGL Freetype engine
 *
 * Distributed under the OSI-approved BSD 2-Clause License.  See accompanying
 * file `LICENSE` for more details.
 */
#ifndef __FREETYPE_GL_H__
#define __FREETYPE_GL_H__

#ifndef SOURCE_PATH_SIZE
#define SOURCE_PATH_SIZE 0
#endif

/* Mandatory */
#include "platform.h"
#include "opengl.h"
#include "vec234.h"
#include "vector.h"
#include "texture-atlas.h"
#include "texture-font.h"
#include "freetype-gl-err.h"

#ifdef IMPLEMENT_FREETYPE_GL
#include "platform.c"
#include "texture-atlas.c"
#include "texture-font.c"
#include "vector.c"
#include "utf8-utils.c"
#include "freetype-gl-err.c"
#include "distance-field.c"
#endif

#ifdef __cplusplus
#ifndef NOT_USING_FT_GL_NAMESPACE
using namespace ftgl;
#endif /* NOT_USING_FT_GL_NAMESPACE */
#endif /* __cplusplus */

#endif /* FREETYPE_GL_H */
