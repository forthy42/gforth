// this file is in the public domain
%module harfbuzz
%insert("include")
%{
#include <harfbuzz/hb.h>
#include <harfbuzz/hb-ft.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

%apply unsigned int { hb_codepoint_t, hb_tag_t, hb_color_t };
%apply unsigned char { uint8_t };
%apply int { hb_position_t };
%apply SWIGTYPE * { FT_Face };

// prep: sed -e 's/^\(.*not_found_glyph.*\)$//g'
// exec: sed -e 's/^c-library \(.*\)/cs-vocabulary \1``get-current also \1 definitions``c-library \1/g' -e 's/^end-c-library/end-c-library`previous set-current/g' -e 's/ s n / a n /g' -e 's/\(c-function \(hb_glyph_info_get_glyph_flags\|hb_segment_properties_overlay\|hb_buffer_create_similar\|hb_font_funcs_set_glyph_shape_func\|hb_font_funcs_set_glyph_name_func\|hb_font_get_glyph_shape\|hb_font_get_serial\|hb_font_changed\|hb_font_set_synthetic_slant\|hb_font_get_synthetic_slant\|hb_font_set_var_coords_design\|hb_set_add_sorted_array\|hb_set_hash\|hb_set_next_many\|hb_ft_hb_font_changed\) .*$\)/\\ \1/g' -e 's/s" harfbuzz" add-lib/e? os-type s" linux-android" string-prefix? [IF] s" typeset" [ELSE] s" harfbuzz" [THEN] add-lib/g' | tr '`' '\n'

#define HB_EXTERN extern
#define HB_BEGIN_DECLS
#define HB_END_DECLS

%include "harfbuzz/hb.h"
%include "harfbuzz/hb-blob.h"
%include "harfbuzz/hb-buffer.h"
%include "harfbuzz/hb-common.h"
 // %include "harfbuzz/hb-deprecated.h"
%include "harfbuzz/hb-face.h"
%include "harfbuzz/hb-font.h"
%include "harfbuzz/hb-set.h"
%include "harfbuzz/hb-shape.h"
%include "harfbuzz/hb-shape-plan.h"
%include "harfbuzz/hb-unicode.h"
%include "harfbuzz/hb-version.h"
%include "harfbuzz/hb-ft.h"
