\ Number IO

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007,2010,2012,2013 Free Software Foundation, Inc.

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

require ./io.fs

: pad    ( -- c-addr ) \ core-ext
    \G @var{c-addr} is the address of a transient region that can be
    \G used as temporary data storage. At least 84 characters of space
    \G is available.
    [ has? flash [IF] ] normal-dp @ [ [ELSE] ] here [ [THEN] ]
    word-pno-size + aligned ;

\ hold <# #> sign # #s                                 25jan92py

: hld  ( -- addr )  pad cell - ;
: hold  ( char -- )  hld -1 over +! @ c! ;
: <#    hld dup ! ;
: #>   ( d -- addr +n )  2drop hld dup @ tuck - ;

: sign    ( n -- ) \ core
    \G Used within @code{<#} and @code{#>}. If @var{n} (a @var{single}
    \G number) is negative, append the display code for a minus sign
    \G to the pictured numeric output string. Since the string is
    \G built up ``backwards'' this is usually used immediately prior
    \G to @code{#>}, as shown in the examples below.
    0< IF  [char] - hold  THEN ;

: #       ( ud1 -- ud2 ) \ core		number-sign
    \G Used within @code{<#} and @code{#>}. Add the next
    \G least-significant digit to the pictured numeric output
    \G string. This is achieved by dividing @var{ud1} by the number in
    \G @code{base} to leave quotient @var{ud2} and remainder @var{n};
    \G @var{n} is converted to the appropriate display code (eg ASCII
    \G code) and appended to the string. If the number has been fully
    \G converted, @var{ud1} will be 0 and @code{#} will append a ``0''
    \G to the string.
    base @ ud/mod rot 9 over <
    IF
	[ char A char 9 - 1- ] Literal +
    THEN
    [char] 0 + hold ;

: #s      ( ud -- 0 0 ) \ core	number-sign-s
    \G Used within @code{<#} and @code{#>}. Convert all remaining digits
    \G using the same algorithm as for @code{#}. @code{#s} will convert
    \G at least one digit. Therefore, if @var{ud} is 0, @code{#s} will append
    \G a ``0'' to the pictured numeric output string.
    BEGIN
	# 2dup or 0=
    UNTIL ;

: holds ( addr u -- )
    BEGIN  dup  WHILE  1- 2dup + c@ hold  REPEAT  2drop ;

\ print numbers                                        07jun92py

: d.r ( d n -- ) \ double	d-dot-r
    \G Display @var{d} right-aligned in a field @var{n} characters wide. If more than
    \G @var{n} characters are needed to display the number, all digits are displayed.
    \G If appropriate, @var{n} must include a character for a leading ``-''.
    >r tuck  dabs  <# #s  rot sign #>
    r> over - spaces  type ;

: ud.r ( ud n -- ) \ gforth	u-d-dot-r
    \G Display @var{ud} right-aligned in a field @var{n} characters wide. If more than
    \G @var{n} characters are needed to display the number, all digits are displayed.
    >r <# #s #> r> over - spaces type ;

: .r ( n1 n2 -- ) \ core-ext	dot-r
    \G Display @var{n1} right-aligned in a field @var{n2} characters wide. If more than
    \G @var{n2} characters are needed to display the number, all digits are displayed.
    \G If appropriate, @var{n2} must include a character for a leading ``-''.
    >r s>d r> d.r ;

: u.r ( u n -- )  \ core-ext	u-dot-r
    \G Display @var{u} right-aligned in a field @var{n} characters wide. If more than
    \G @var{n} characters are needed to display the number, all digits are displayed.
    0 swap ud.r ;

: d. ( d -- ) \ double	d-dot
    \G Display (the signed double number) @var{d} in free-format. followed by a space.
    0 d.r space ;

: ud. ( ud -- ) \ gforth	u-d-dot
    \G Display (the signed double number) @var{ud} in free-format, followed by a space.
    0 ud.r space ;

: . ( n -- ) \ core	dot
    \G Display (the signed single number) @var{n} in free-format, followed by a space.
    s>d d. ;

: u. ( u -- ) \ core	u-dot
    \G Display (the unsigned single number) @var{u} in free-format, followed by a space.
    0 ud. ;

