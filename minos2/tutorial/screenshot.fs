\ ΜΙΝΩΣ2 Screenshot Examples

\ Authors: Bernd Paysan
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

[IFUNDEF] button-color#
    dark-gui
    $0000CCFF new-color, fvalue button-color#
    light-gui
    $FFFFAAFF re-color button-color#
[THEN]
[IFUNDEF] edit-color#
    dark-gui
    $AAAAAAFF new-color, fvalue edit-color#
    light-gui
    $CCCCCCFF re-color edit-color#
[THEN]

{{
    $000000FF $FFFFFFFF pres-frame
    {{
	l" ΜΙΝΩΣ2 Screenshot Example" /title \skip
	\normal
	{{
	    {{
		l" Filename: " }}text'
		\mono
		{{
		    glue*l edit-color# font-size# 40% f* }}frame dup .button3
		    {{
			s" screenshot-file" }}edit 25%b dup Value shot-filename
			glue*ll }}glue
		    }}h box[]
		}}z box[]
		\sans
	    }}h shot-filename ' true edit[] >bl
	}}h box[] >bl
	\skip
	{{
	    glue*ll }}glue
	    l" Save JPEG" button-color# }}button
	    [: [: shot-filename .text$ type ." .jpg" ;] $tmp screenshot>jpg ;] over click[]
	    em-space
	    l" Save PNG" button-color# }}button
	    [: [: shot-filename .text$ type ." .png" ;] $tmp screenshot>png ;] over click[]
	    glue*ll }}glue
	}}h box[] >bl
	glue*l }}glue
    }}v box[] >bdr
}}z box[]

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
