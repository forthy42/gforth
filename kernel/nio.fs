\ Number IO

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

: pad    ( -- addr ) \ core-ext
    here word-pno-size + aligned ;

\ hold <# #> sign # #s                                 25jan92py

: hold    ( char -- ) \ core
    \G Used within @code{<#} and @code{#>}. Append the character char
    \G to the pictured numeric output string.
    -1 chars holdptr +!
    holdptr @ dup holdbuf u< -&17 and throw
    c! ;

: <# ( -- ) \ core	less-number-sign
    \G Initialise/clear the pictured numeric output string.
    holdbuf-end dup holdptr ! holdend ! ;

: #>      ( xd -- addr u ) \ core	number-sign-greater
    \G Complete the pictured numeric output string by
    \G discarding xd and returning addr u; the address and length
    \G of the formatted string. A Standard program may modify characters
    \G within the string.
    2drop holdptr @ holdend @ over - ;

: <<# ( -- ) \ gforth	less-less-number-sign
    \G starts a hold area that ends with @code{#>>}. Can be nested in
    \G each other and in @code{<#}.  Note: if you do not match up the
    \G @code{<<#}s with @code{#>>}s, you will eventually run out of
    \G hold area; you can reset the hold area to empty with @code{<#}.
    holdend @ holdptr @ - hold
    holdptr @ holdend ! ;

: #>> ( -- ) \ gforth	number-sign-greater-greater
    \G releases the hold area started with @code{<<#}.
    holdend @ dup holdbuf-end u>= -&11 and throw
    count chars bounds holdptr ! holdend ! ;

: sign    ( n -- ) \ core
    \G Used within @code{<#} and @code{#>}. If n (a @var{single} number)
    \G is negative, append the display code for a minus sign to the pictured
    \G numeric output string. Since the string is built up "backwards"
    \G this is usually used immediately prior to @code{#>}, as shown in
    \G the examples below.
    0< IF  [char] - hold  THEN ;

: #       ( ud1 -- ud2 ) \ core		number-sign
    \G Used within @code{<#} and @code{#>}. Add the next least-significant
    \G digit to the pictured numeric output string. This is achieved by dividing ud1
    \G by the number in @code{base} to leave quotient ud2 and remainder n; n
    \G is converted to the appropriate display code (eg ASCII code) and appended
    \G to the string. If the number has been fully converted, ud1 will be 0 and
    \G @code{#} will append a "0" to the string.
    base @ 2 max ud/mod rot 9 over <
    IF
	[ char A char 9 - 1- ] Literal +
    THEN
    [char] 0 + hold ;

: #s      ( ud -- 0 0 ) \ core	number-sign-s
    \G Used within @code{<#} and @code{#>}. Convert all remaining digits
    \G using the same algorithm as for @code{#}. @code{#s} will convert
    \G at least one digit. Therefore, if ud is 0, @code{#s} will append
    \G a "0" to the pictured numeric output string.
    BEGIN
	# 2dup or 0=
    UNTIL ;

\ print numbers                                        07jun92py

: d.r ( d n -- ) \ double	d-dot-r
    \G Display d right-aligned in a field n characters wide. If more than
    \G n characters are needed to display the number, all digits are displayed.
    \G If appropriate, n must include a character for a leading "-".
    >r tuck  dabs  <<# #s  rot sign #>
    r> over - spaces  type #>> ;

: ud.r ( ud n -- ) \ gforth	u-d-dot-r
    \G Display ud right-aligned in a field n characters wide. If more than
    \G n characters are needed to display the number, all digits are displayed.
    >r <<# #s #> r> over - spaces type #>> ;

: .r ( n1 n2 -- ) \ core-ext	dot-r
    \G Display n1 right-aligned in a field n2 characters wide. If more than
    \G n2 characters are needed to display the number, all digits are displayed.
    \G If appropriate, n2 must include a character for a leading "-".
    >r s>d r> d.r ;

: u.r ( u n -- )  \ core-ext	u-dot-r
    \G Display u right-aligned in a field n characters wide. If more than
    \G n characters are needed to display the number, all digits are displayed.
    0 swap ud.r ;

: d. ( d -- ) \ double	d-dot
    \G Display (the signed double number) d in free-format. followed by a space.
    0 d.r space ;

: ud. ( ud -- ) \ gforth	u-d-dot
    \G Display (the signed double number) ud in free-format, followed by a space.
    0 ud.r space ;

: . ( n -- ) \ core	dot
    \G Display (the signed single number) n in free-format, followed by a space.
    s>d d. ;

: u. ( u -- ) \ core	u-dot
    \G Display (the unsigned single number) u in free-format, followed by a space.
    0 ud. ;

