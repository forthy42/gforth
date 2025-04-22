\ Presentation of Wayland integration in ΜΙΝΩΣ2

\ Author: Bernd Paysan
\ Copyright (C) 2025 Bernd Paysan

\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU Affero General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

require minos2/widgets.fs

[IFDEF] android
    also jni hidekb also android >changed hidestatus >changed previous previous
[THEN]

also minos

ctx 0= [IF]  window-init  [THEN]

require minos2/font-style.fs
require minos2/text-style.fs
require minos2/presentation-support.fs
require unix/open-url.fs

:noname 44e update-size# ; is rescaler
rescaler

m2c:animtime% f@ 3e f* m2c:animtime% f!

tex: gforth-logo
' gforth-logo "logo.png" 0.5e }}image-file Constant gforth-logo-glue drop

: logo-img ( xt xt -- o o-img ) 2>r
    baseline# 0e to baseline#
    {{ 2r> }}image-tex dup >r /right
    glue*l }}glue
    }}v >o font-size# f2/ to border o o>
    to baseline# r> ;

' }}i18n-text is }}text'

{{
{{ glue-left @ }}glue

\ page 0
{{
    $000000FF $FFFFFFFF pres-frame
    {{
	glue*l }}glue \ ) $CCDDDD3F color, 4e }}frame dup .button1
	l" Wayland in ΜΙΝΩΣ2" /title
	l" Notation Matters" /subtitle
	{{
	    {{
		glue*l }}glue
		tex: burladingen-wappen
		' burladingen-wappen "Wappen_Burladingen.svg.png" 0.5e }}image-file
		Constant wappen-burladingen-glue /center
		glue*l }}glue
	    }}v
	    glue*2 }}glue
	}}z
	l" Bernd Paysan" /author
	l" Forth–Tagung 2025 in Burladingen" /location
	glue*l }}glue \ ) $CCDDDD3F color, 4e }}frame dup .button1
    }}v box[] >o font-size# to border o Value title-page o o>
}}z box[] dup >slides

\ page 6
{{
    $440044FF $FFBFFFFF pres-frame
    {{
	l" Was ist Wayland?" /title
	l" Ein objektbasiertes RPC zur Kommunikation mit dem Compositor" /subsection
	vt{{
	    l" Name -c-4 " l" Name comes first" b\\
	    l" flags+counts -4 " l" Flags: up to 8 bits, count rest of the cell" b\\
	    l" Link Field -3 " l" To next header" b\\
	    l" Code Field -2 " l" Moved here" b\\
	    l" Name–HM -1 " l" Header method table, see next page" b\\
	    l" Body  0 " l" This is where the xt points to" b\\
	    glue*l }}glue \ ) $CCDDDD3F 4e }}frame dup .button1
	}}vt
    }}v box[] >bdr
}}z box[] /flip dup >slides

\ page 17
{{
    $000000FF $FFFFFFFF pres-frame
    {{
	l" Literatur & Links" /title
	vt{{
	    l" Gforth Team " l" Gforth Homepage" bi\\
	    l" 🔗" l" https://gforth.org/" bm\\
	    [: "https://gforth.org/" open-url ;] 0 click[]
	}}vt
	glue*l }}glue
    }}v box[] >bdr
}}z box[] /flip dup >slides

' }}text is }}text'

\ end
glue-right @ }}glue
}}h box[]
{{
    ' gforth-logo     gforth-logo-glue logo-img solid-frame
}}z
}}z slide[]
to top-widget

also opengl

[IFDEF] writeout-en
    lsids ' .lsids s" ef2018/en" r/w create-file throw
    dup >r outfile-execute r> close-file throw
[THEN]

previous

script? [IF]
    next-arg s" time" str= [IF]  +db time( \ ) [THEN]
    presentation bye
[ELSE]
    presentation
[THEN]

\\\
Local Variables:
forth-local-words:
    (
     ("[a-z0-9]+(" immediate (font-lock-comment-face . 1)
      ")" nil comment (font-lock-comment-face . 1))
     (("x\"" "l\"") immediate (font-lock-string-face . 1)
      "[\"\n]" nil string (font-lock-string-face . 1))
    )
forth-local-indent-words:
    (
     (("{{" "vt{{") (0 . 2) (0 . 2) immediate)
     (("}}h" "}}v" "}}z" "}}vp" "}}p" "}}vt") (-2 . 0) (-2 . 0) immediate)
    )
End:
