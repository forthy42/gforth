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

// exec: sed -e 's/c-function texture_atlas_get_region.*/\\c #define texture_atlas_get_regionp(self, w, h, x) { ivec4* _x=x; *_x=texture_atlas_get_region(self,w,h); }`c-function texture_atlas_get_region texture_atlas_get_regionp a n n a -- void/g' -e 's/c-function texture_font_get_glyph texture_font_get_glyph a s -- a/c-function texture_font_get_glyph texture_font_get_glyph a a -- a/g' -e 's/c-function texture_glyph_get_kerning texture_glyph_get_kerning a s -- r/c-function texture_glyph_get_kerning texture_glyph_get_kerning a a -- r/g' -e 's/\(c-function freetype_gl_errstr .*$\)/\1`c-value freetype_gl_errno freetype_gl_errno -- n/g' -e 's/s" freetype-gl" add-lib/e? os-type s" linux-android" string-prefix? \[IF] s" typeset" \[ELSE] s" freetype-gl" \[THEN] add-lib/g' | tr '`' '\n'



%include "freetype-gl.h"
%include "vec234.h"
%include "vector.h"
%include "texture-atlas.h"
%include "texture-font.h"
%include "freetype-gl-err.h"
