// this file is in the public domain
%module freetype_gl
%insert("include")
%{
#include <freetype-gl.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

%apply short { wchar_t };
%apply int { size_t };
%apply unsigned int { uint32_t };
%apply SWIGTYPE * { unsigned char const *const };

#define SWIG_FORTH_GFORTH_LIBRARY "freetype-gl"

#define __THREAD

// exec: sed -e 's/^\(c-library .*\)/cs-vocabulary freetype-gl\nget-current >r also freetype-gl definitions\n\n\1\ns" freetype-gl" open-fpath-file 0= [IF] rot close-file throw add-incdir [THEN]/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g' -e 's/c-function texture_font_get_glyph texture_font_get_glyph a s -- a/c-function texture_font_get_glyph texture_font_get_glyph a a -- a/g' -e 's/c-function texture_glyph_get_kerning texture_glyph_get_kerning a s -- r/c-function texture_glyph_get_kerning texture_glyph_get_kerning a a -- r/g' -e 's/\(c-function FTGL_Error_String .*$\)/\1\nc-value freetype_gl_errno freetype_gl_errno -- n\nc-variable freetype_gl_warnings freetype_gl_warnings ( -- addr )/g' -e 's/s" freetype-gl" add-lib/e? os-type s" linux-android" string-prefix? [IF] s" typeset -lm -lz '$FREETYPESVG'" [ELSE] s" freetype -lm -lz '$FREETYPESVG'" [THEN] add-lib\n\\c #define IMPLEMENT_FREETYPE_GL/g' -e 's,#include <freetype-gl.h>,#include "freetype-gl/freetype-gl.h",g'



%include "freetype-gl.h"
%include "vec234.h"
%include "vector.h"
%include "texture-atlas.h"
%include "texture-font.h"
%include "ftgl-utils.h"
