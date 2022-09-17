\ Presentation of Gforth 1.0 headers and recognizers

\ Author: Bernd Paysan
\ Copyright (C) 2021 Bernd Paysan

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

require widgets.fs

[IFDEF] android
    also jni hidekb also android >changed hidestatus >changed previous previous
[THEN]

also minos

ctx 0= [IF]  window-init  [THEN]

require minos2/font-style.fs

44e update-size#

require minos2/text-style.fs
require presentation-support.fs

m2c:animtime% f@ 0e f* m2c:animtime% f!

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
	l" Gforth 1.0" /title
	l" Header & Recognizers" /subtitle
	glue*2 }}glue
	l" Bernd Paysan" /author
	l" EuroForth 2022, Video–Konferenz" /location
	glue*l }}glue \ ) $CCDDDD3F color, 4e }}frame dup .button1
    }}v box[] >o font-size# to border o Value title-page o o>
}}z box[] dup >slides

\ page 6
{{
    $440044FF $FFBFFFFF pres-frame
    {{
	l" New Headers 1️" /title
	l" The Big Header Unification nt = xt = body" /subsection
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

\ page 6
{{
    $440044FF $FFBFFFFF pres-frame
    {{
	l" New Header 2️" /title
	l" Header Method Table" /subsection
	vt{{
	    l" Link " l" Pointer to previous VTable" b\\
	    l" compile, " l" method to compile the word" b\\
	    l" to " l" method to apply IS or TO" b\\
	    l" defer@ " l" method for DEFER@" b\\
	    l" extra " l" method for DOES> (or other extras)" b\\
	    \skip
	    l" name>int " l" convert name token to interpretation semantics" b\\
	    l" name>comp " l" convert name token to compilation semantics" b\\
	    l" name>string " l" convert name token to string (if any)" b\\
	    l" name>link " l" follow link field (if any)" b\\	    glue*l }}glue \ ) $CCDDDD3F 4e }}frame dup .button1
	}}vt
    }}v box[] >bdr
}}z box[] /flip dup >slides

\ page 6
{{
    $200030FF $EECCFFFF pres-frame
    {{
	l" Recognizer" /title
	l" Minimalistic Core & Sequences & Unification" /subsection
	vt{{
	    l" forth-recognize " l" Default–Recognizer as deferred Word" b\\
	    l" recognizer-sequence: " l" Sequence of recognizers" b\\
	    l" wordlists " l" are executable and recognizers" b\\
	    l" search order " l" is a rec-sequence:" b\\
	    \skip
	    l" translators " l" are executable, take ( data -- ... )" b\\
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
	    [: s" xdg-open https://gforth.org/" system ;] 0 click[]
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
