\ ΜΙΝΩΣ2 Plots Examples

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

require minos2/plot.fs

dark-blue x-color FValue plot-x-color

Create plot-x-addr
0e f, 1e f, 3e f, 2e f, 0e f,
here plot-x-addr - Constant plot-x-u
Create plot-x-addr'
1e f,
1e f,
3e f,
2e f,
1e f,
3e f,
1e f,
3e f,
3e f,
Create plot-y-addr'
1e f,
3e f,
3e f,
4e f,
3e f,
1e f,
1e f,
3e f,
1e f,
here plot-y-addr' - Constant plot-xy-u

: plot-x-test ( -- )
    plot-x-addr plot-x-u plot-x-color plot-x ;
: plot-xy-test ( -- )
    plot-x-addr' plot-y-addr' plot-xy-u plot-x-color
    0e 4e 0e 5e plot-xy-minmax ;

{{
    $000000FF $FFFFFFFF pres-frame
    {{
	l" ΜΙΝΩΣ2 Plot Example" /title \skip
	\normal
	{{
	    whitish glue*l x-color ' plot-x-test ' noop }}canvas
	    {{
		em-space
		glue*l }}glue
	    }}v box[]
	    whitish glue*l x-color ' plot-xy-test ' noop }}canvas
	}}h box[] >bl
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
