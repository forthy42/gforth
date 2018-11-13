// this file is in the public domain
%module harfbuzz
%insert("include")
%{
#include <harfbuzz/hb.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

%apply unsigned int { hb_codepoint_t, hb_tag_t };
%apply int { hb_position_t };

// exec: sed -e 's/ s n / a n /g' -e 's/c-function hb_glyph_info_get_glyph_flags/\\ c-function hb_glyph_info_get_glyph_flags/g'

#define HB_EXTERN extern
#define HB_BEGIN_DECLS
#define HB_END_DECLS

%include "harfbuzz/hb.h"
%include "harfbuzz/hb-blob.h"
%include "harfbuzz/hb-buffer.h"
%include "harfbuzz/hb-common.h"
%include "harfbuzz/hb-deprecated.h"
%include "harfbuzz/hb-face.h"
%include "harfbuzz/hb-font.h"
%include "harfbuzz/hb-set.h"
%include "harfbuzz/hb-shape.h"
%include "harfbuzz/hb-shape-plan.h"
%include "harfbuzz/hb-unicode.h"
%include "harfbuzz/hb-version.h"
