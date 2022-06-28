\ Minos2 Buttons Examples

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2022 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

require minos2/widgets.fs

also minos

require minos2/font-style.fs

44e update-size#

require minos2/text-style.fs
require minos2/presentation-support.fs

dark-gui
$0000EECC new-color, fvalue button-color#
light-gui
$FFFFAAFF re-color button-color#

' }}i18n-text is }}text'

{{
    $777777FF $888888FF pres-frame
    {{
	{{
	    l" 双头龙" }}text' glue*l }}glue
	    l" 雙頭龍" }}text' glue*l }}glue \normal 
	    l" 🇨🇳 scify 🇲🇾" button-color# }}button
	    [: ." simpliefied" cr ['] translators:scify is translator +lang ;] over click[] glue*l }}glue
	    l" 🇹🇼 tcify 🇭🇰" button-color# }}button
	    [: ." traditional" cr ['] translators:tcify is translator +lang ;] over click[] glue*l }}glue
	    l" as is" button-color# }}button
	    [: ." as is" cr ['] noop  is translator +lang ;] over click[] glue*l }}glue  glue*l }}glue 
	}}h box[]
	glue*l }}glue
    }}v box[] >bdr
}}z box[] to top-widget

dark-gui
presentation
bye

\\\
Local Variables:
forth-local-words:
    (
     (("x\"" "l\"") immediate (font-lock-string-face . 1)
      "[\"\n]" nil string (font-lock-string-face . 1))
    )
forth-local-indent-words:
    (
     (("{{" "vt{{") (0 . 2) (0 . 2) immediate)
     (("}}h" "}}v" "}}z" "}}vp" "}}p" "}}vt") (-2 . 0) (-2 . 0) immediate)
    )
End:

	    
