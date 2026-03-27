// this file is in the public domain
%module xkbcommon
%insert("include")
%{
#include <xkbcommon/xkbcommon.h>
#include <xkbcommon/xkbcommon-keysyms.h>
// unfortunately, there's no version marker in xkbcommon.h, but we extracted it in autoconf
#define XKB_MIN_VER(x, y) (XKBCOMMON_MAJOR > x || (XKBCOMMON_MAJOR == x && XKBCOMMON_MINOR >= y))
%}

%apply unsigned int { uint32_t, xkb_keycode_t, xkb_keysym_t, xkb_layout_index_t, xkb_layout_mask_t, xkb_level_index_t, xkb_mod_index_t, xkb_mod_mask_t, xkb_led_index_t, xkb_led_mask_t }
%apply unsigned long { size_t }
%apply SWIGTYPE * { va_list }

// prep: sed -e 's/^\(.*\(xkb_components_names_from_rules\|xkb_keymap_mod_get_mask\|xkb_state_update_latched_locked\).*\)$/#if XKB_MIN_VER(0,10)`\1`#endif/g' -e 's/^\(.*\(xkb_keymap_get_as_string2\).*\)$/#if XKB_MIN_VER(1,12)`\1`#endif/g'  | tr '`' '\n'
// exec: sed -e 's/" xkbcommon" add-lib/" xkbcommon" add-lib/g' -e 's/^c-library/cs-vocabulary xkbcommon\nget-current >r also xkbcommon definitions\n\nc-library/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g' -e 's/\(_from_buffer .*\) s u /\1 a u /g' | grep -v '^#[1-9][0-9]	constant XKB_KEY' | grep -v '^#[1-9][0-9][0-9]	constant XKB_KEY' | grep -v '^#[1-9][0-9][0-9][0-9]	constant XKB_KEY'

%include <xkbcommon/xkbcommon.h>
%include <xkbcommon/xkbcommon-keysyms.h>
