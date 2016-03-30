// this file is in the public domain
%module freetype_gl
%insert("include")
%{
#include "freetype-gl.h"
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

%apply short { wchar_t };
%apply int { size_t };
%apply SWIGTYPE * { unsigned char const *const };

// exec: sed -e 's/c-function texture_atlas_get_region.*/\\c #define texture_atlas_get_regionp(self, w, h, x) { ivec4* _x=x; *_x=texture_atlas_get_region(self,w,h); }\nc-function texture_atlas_get_region texture_atlas_get_regionp a n n a -- void/g' -e 's/c-function texture_font_get_glyph texture_font_get_glyph a s -- a/c-function texture_font_get_glyph texture_font_get_glyph a a -- a/g' -e 's/c-function texture_glyph_get_kerning texture_glyph_get_kerning a s -- r/c-function texture_glyph_get_kerning texture_glyph_get_kerning a a -- r/g'


%include "freetype-gl.h"
%include "vec234.h"
%include "vector.h"
%include "texture-atlas.h"
%include "texture-font.h"
