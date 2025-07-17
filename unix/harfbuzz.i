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
#undef hb_glyph_info_get_glyph_flags
#define HB_MIN_VER(x, y) (HB_VERSION_MAJOR > x || (HB_VERSION_MAJOR == x && HB_VERSION_MINOR >= y))
%}

%apply unsigned int { hb_codepoint_t, hb_tag_t, hb_color_t };
%apply unsigned char { uint8_t };
%apply int { hb_position_t };
%apply SWIGTYPE * { FT_Face };

// prep: sed -e 's/^\(.*\(hb_set_invert\).*\)$/#if HB_MIN_VER(3,0)\n\1\n#endif/g' -e 's/^\(.*\(hb_buffer_get_not_found_glyph\|hb_buffer_set_not_found_glyph\).*\)$/#if HB_MIN_VER(3,1)\n\1\n#endif/g' -e 's/^\(.*\(hb_buffer_create_similar\|hb_font_get_synthetic_slant\|hb_font_get_var_coords_design\|hb_font_set_synthetic_slant\|hb_segment_properties_overlay\).*\)$/#if HB_MIN_VER(3,3)\n\1\n#endif/g' -e 's/^\(.*\(hb_font_funcs_set_glyph_shape_func\|hb_font_get_glyph_shape\).*\)$/#if HB_MIN_VER(4,0)\n\1\n#endif/g' -e 's/^\(.*\(hb_set_add_sorted_array\).*\)$/#if HB_MIN_VER(4,1)\n\1\n#endif/g' -e 's/^\(.*\(hb_set_next_many\).*\)$/#if HB_MIN_VER(4,2)\n\1\n#endif/g' -e 's/^\(.*\(hb_font_changed\|hb_font_get_serial\|hb_ft_hb_font_changed\|hb_set_hash\).*\)$/#if HB_MIN_VER(4,4)\n\1\n#endif/g' -e 's/^\(.*\(hb_language_matches\).*\)$/#if HB_MIN_VER(5,0)\n\1\n#endif/g' -e 's/^\(.*\(hb_face_builder_sort_tables\).*\)$/#if HB_MIN_VER(5,3)\n\1\n#endif/g' -e 's,^\(.*hb_var_num_t.*\)$,/* \1 */,g' -e 's/^\(.*\(hb_face_collect_nominal_glyph_mapping\|hb_font_funcs_set_draw_glyph_func\|hb_font_funcs_set_paint_glyph_func\|hb_font_draw_glyph\|hb_font_paint_glyph\|hb_font_set_synthetic_bold\|hb_font_get_synthetic_bold\|hb_font_set_variation\|hb_font_get_var_named_instance\|hb_set_is_inverted\|hb_shape_justify\).*\)$/#if HB_MIN_VER(7,0)\n\1\n#endif/g' -e 's/^\(.*\(hb_buffer_set_random_state\|hb_buffer_get_random_state\).*\)$/#if HB_MIN_VER(8,4)\n\1\n#endif/g' -e 's/^\(.*\(hb_buffer_set_not_found_variation_selector_glyph\|hb_buffer_get_not_found_variation_selector_glyph\|hb_face_set_get_table_tags_func\).*\)$/#if HB_MIN_VER(10,0)\n\1\n#endif/g' -e 's/^\(.*\(hb_face_create_or_fail\|hb_face_create_from_file_or_fail\|hb_ft_face_create_from_file_or_fail\).*\)$/#if HB_MIN_VER(10,1)\n\1\n#endif/g' -e 's/^\(.*\(hb_ft_font_get_ft_face\).*\)$/#if HB_MIN_VER(10,4)\n\1\n#endif/g' -e 's/^\(.*\(hb_malloc\|hb_calloc\|hb_realloc\|hb_free\|hb_face_list_loaders\|hb_font_set_funcs_using\|hb_ft_face_create_from_blob_or_fail\).*\)$/#if HB_MIN_VER(11,0)\n\1\n#endif/g' -e 's/^\(.*\(hb_font_funcs_set_draw_glyph_or_fail_func\|hb_font_funcs_set_paint_glyph_or_fail_func\|hb_font_draw_glyph_or_fail\|hb_font_paint_glyph_or_fail\|hb_font_list_funcs\|hb_font_is_synthetic\).*\)$/#if HB_MIN_VER(11,2)\n\1\n#endif/g'
// exec: sed -e 's/^c-library \(.*\)/cs-vocabulary \1\nget-current >r also \1 definitions\n\nc-library \1/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g' -e 's/ s n / a n /g' -e 's/s" harfbuzz" add-lib/e? os-type s" linux-android" string-prefix? [IF] s" typeset" [ELSE] s" harfbuzz" [THEN] add-lib/g' -e 's/\(^c-function hb_shape_justify \)/\\ \1/g'

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
