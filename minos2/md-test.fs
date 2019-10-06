\ Presentation on ΜΙΝΩΣ2 made in ΜΙΝΩΣ2

\ Author: Bernd Paysan
\ Copyright (C) 2018 Bernd Paysan

\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU Affero General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU Affero General Public License for more details.

\ You should have received a copy of the GNU Affero General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

require minos2/widgets.fs

[IFDEF] android
    also jni hidekb also android >changed hidestatus >changed previous previous
[THEN]

also minos

ctx 0= [IF]  window-init  [THEN]

require minos2/font-style.fs

84e update-size#

require minos2/text-style.fs
require minos2/md-viewer.fs

fpath+ ~+

next-arg markdown-parse

dpy-w @ s>f font-size# fover 25% f* f+ f2* f- p-format

: slide-frame ( glue color -- o )
    font-size# 70% f* }}frame ;
: pres-frame ( color -- o1 o2 ) \ drop $FFFFFFFF
    color, glue*wh slide-frame dup .button1 simple[] ;

{{
    $FFFFFFFF pres-frame
    {{
	v-box
	glue*ll }}glue
	tex: vp-md
    glue*l ' vp-md }}vp vp[] >o font-size# dpy-w @ s>f 25% f* fdup fnegate to borderv f+ to border o o>
}}z box[] to top-widget

: !widgets ( -- )
    top-widget .htop-resize
    1e ambient% sf! set-uniforms ;

[IFDEF] android also android also jni [THEN]

: presentation ( -- )
    1config
    [IFDEF] hidestatus hidekb hidestatus [THEN]
    !widgets widgets-loop ;

[IFDEF] android previous previous [THEN]

previous

script? [IF]
    presentation bye
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
