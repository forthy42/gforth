\ Presentation on ΜΙΝΩΣ2 made in ΜΙΝΩΣ2

\ Author: Bernd Paysan
\ Copyright (C) 2018 Bernd Paysan

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

tex: minos2
' minos2 "net2o-minos2.png" 0.666e }}image-file Constant minos2-glue drop

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
	l" ΜΙΝΩΣ2 GUI" /title
	l" Lightweight GUI library" /subtitle
	glue*2 }}glue
	l" Bernd Paysan" /author
	l" EuroForth 2018, Edinburgh" /location
	glue*l }}glue \ ) $CCDDDD3F color, 4e }}frame dup .button1
    }}v box[] >o font-size# to border o Value title-page o o>
}}z box[] dup >slides

\ page 6
{{
    $440044FF $FFBFFFFF pres-frame
    {{
	l" ΜΙΝΩΣ2 Widgets" /title
	l" Design principle is a Lego–style combination of many extremely simple objects" \\
	{{ {{ vt{{
		    l" actor " l" base class that reacts on all actions (clicks, touchs, keys)" b\\
		    l" animation " l" action for animations" b\\
		    l" widget " l" base class for all visible objects" b\\
		    l" glue " l" base class for flexible objects" b\\
		    l" tile " l" colored rectangle" b\\
		    l" frame " l" colored rectangle with border" b\\
		    l" icon " l" icon from an icon texture" b\\
		    l" image " l" larger image" b\\
		    {{ l" edit " b0 blackish l" editable text: " }}text'
		    "中秋节快乐！ Happy autumn festival! 🌙🌕" }}edit dup Value edit-field glue*l }}glue }}h edit-field ' true edit[] >bl
		    \sans \latin \normal \regular
		    l" text " l" text element/Emoji/Icons/中文/… 😀🤭😁😂😇😈🙈🙉🙊💓💔💕💖💗💘🍺🍻🎉🎻🎺🎷" b\\
		    l" part-text " l" pseudo–element for paragraph breaking" b\\
		    l" canvas " l" vector graphics (TBD)" b\\
		    l" video " l" video player (TBD)" b\\
		}}vt
		glue*l }}glue
	    tex: vp0 glue*lll ' vp0 }}vp vp[]
	    $FFBFFFFF color, fdup to slider-color to slider-fgcolor
	    dup font-size# f2/ fdup vslider
	}}h box[]
    }}v box[] >bdr
}}z box[] /flip dup >slides

\ page 7
{{
$004444FF $BFFFFFFF pres-frame
{{
    l" ΜΙΝΩΣ2 Boxes" /title
    {{
    l" Just like " }}text' \LaTeX l" , boxes arrange widgets/text" }}text' glue*l }}h box[]
    >bl
    \skip
    vt{{
	l" hbox " l" Horizontal box, common baseline" b\\
	l" vbox " l" Vertical box, minimum distance a baselineskip (of the hboxes below)" b\\
	l" zbox " l" Overlapping several boxes" b\\
	l" slider " l" horizontal and vertical sliders (composite object)" b\\
	l" parbox " l" box for breaking paragraphs" b\\
	l" grid " l" Free widget placements (TBD)" b\\
	\skip
	l" Tables uses helper glues, no special boxes needed" \\
    }}vt
    {{ {{ glue*l }}glue
	    {{ \tiny l"  Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. " }}i18n-text \bold "Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit," }}text \regular " sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui " }}text \italic "dolorem ipsum quia dolor sit amet," }}text \regular " consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum" }}text \bold-italic " qui dolorem eum fugiat" }}text \regular " quo voluptas nulla pariatur?" }}text glue*l }}glue }}p cbl dpy-w @ s>f font-size# 140% f* f- 1e text-shrink% f2/ f- f/ dup .par-split unbox
	glue*l }}glue }}v
    glue*2 }}glue }}z  \ ) $CCDDDD3F 4e }}frame dup .button1
}}v box[] >bdr
}}z box[] /flip dup >slides

\ page 8
{{
    $444400FF $FFFFBFFF pres-frame
    {{
	l" ΜΙΝΩΣ2 Displays" /title
	l" Render into different kinds of displays" \\
	\skip
	vt{{
	    l" viewport " l" Into a texture, used as viewport" b\\
	    l" display " l" To the actual display (no class, just the default)" b\\
	}}vt
	glue*l }}glue
    }}v box[] >bdr
}}z box[] /flip dup >slides

\ page 9
{{
    $002244FF $BFDFFFFF pres-frame
    {{
	l" Minimize Draw Calls" /title
	l" OpenGL wants as few draw–calls per frame, so different contexts are drawn in stacks with a draw–call each" p\\
	\skip
	vt{{
	    l" init " l" Initialization round" b\\
	    l" bg " l" background round" b\\
	    l" text " l" text round (same draw call as bg round, just different code)" b\\
	    l" image " l" draw images with one draw–call per image" b\\
	}}vt
	glue*l }}glue
    }}v box[] >bdr
}}z box[] /flip dup >slides

\ page 17
{{
    $000000FF $FFFFFFFF pres-frame
    {{
	l" Literatur & Links" /title
	vt{{
	    l" Bernd Paysan " l" net2o fossil repository" bi\\
	    l" 🔗" l" https://net2o.de/" bm\\
	    [: s" xdg-open https://net2o.de/" system ;] 0 click[]
	}}vt
	glue*l }}glue
    }}v box[] >bdr
}}z box[] /flip dup >slides

' }}text is }}text'

\ end
glue-right @ }}glue
}}h box[]
{{
    ' minos2     minos2-glue logo-img solid-frame
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
