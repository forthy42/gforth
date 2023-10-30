// this file is in the public domain
%module xkbcommon
%insert("include")
%{
#include <xkbcommon/xkbcommon.h>
#include <xkbcommon/xkbcommon-keysyms.h>
%}

%apply unsigned int { uint32_t, xkb_keycode_t, xkb_keysym_t, xkb_layout_index_t, xkb_layout_mask_t, xkb_level_index_t, xkb_mod_index_t, xkb_mod_mask_t, xkb_led_index_t, xkb_led_mask_t }
%apply unsigned long { size_t }
%apply SWIGTYPE * { va_list }

// exec: sed -e 's/" xkbcommon" add-lib/" xkbcommon" add-lib/g' -e 's/^c-library/cs-vocabulary xkbcommon``get-current also xkbcommon definitions``c-library/g' -e 's/^end-c-library/end-c-library`previous set-current/g' | tr '`' '\n'

%include <xkbcommon/xkbcommon.h>
%include <xkbcommon/xkbcommon-keysyms.h>
