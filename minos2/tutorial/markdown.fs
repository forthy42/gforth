\ ΜΙΝΩΣ2 Markdown Examples

\ Authors: Bernd Paysan
\ Copyright (C) 2022,2025 Free Software Foundation, Inc.

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

require minos2/md-viewer.fs

fpath+ ~+

: string-postfix? ( addr1 u1 addr2 u2 -- flag )
    tuck 2>r >r
    2dup d0= IF  rdrop  ELSE  dup r> - safe/string  THEN
    2r> str= ;

1 arg s" .md" string-postfix?
[IF] next-arg [ELSE] "doc/README.md" file>fpath save-mem [THEN]
markdown-parse

dpy-w @ s>f font-size# fover 5% f* f+ f2* f- p-format

{{
    $000000FF $FFFFFFFF pres-frame
    {{
	md-box
	glue*ll }}glue
	tex: vp-md
    glue*l ' vp-md }}vp vp[] >o font-size# dpy-w @ s>f 5% f* fdup fnegate to borderv f+ to border o o>
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
