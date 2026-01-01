\ Number IO

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook, Jens Wilke
\ Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007,2010,2015,2016,2019,2022,2024,2025 Free Software Foundation, Inc.

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
    [ image-header 2 cells + ] ALiteral @ word-pno-size + aligned ;

: +hold ( n -- addr )
    \G Reserve space for n chars in the pictured numeric buffer.
    \G -17 THROW if no space
    negate holdptr +!
    holdptr @ dup holdbuf u< -&17 and throw ;

: hold    ( char -- ) \ core
    \G Used between @code{<<#} and @code{#>}. Prepend the ASCII character
    \G @var{char} to the pictured numeric output string.  Use
    \G @word{holds} for prepending non-ASCII characters.
    1 +hold c! ;

: <# ( -- ) \ core	less-number-sign
    \G Initialise/clear the pictured numeric output string.
    holdbuf-end dup holdptr ! holdend ! ;

: #>      ( xd -- addr u ) \ core	number-sign-greater
    \G Complete the pictured numeric output string by discarding
    \G @var{xd} and returning @var{addr u}; the address and length of
    \G the formatted string. A Standard program may modify characters
    \G within the string.  Does not release the hold area; use
    \G @code{#>>} to release a hold area started with @code{<<#}, or
    \G @code{<#} to release all hold areas.
    2drop holdptr @ holdend @ over - ;

: <<# ( -- ) \ gforth	less-less-number-sign
    \G Start a hold area that ends with @code{#>>}. Can be nested in
    \G each other and in @code{<#}.  Note: if you do not match up the
    \G @code{<<#}s with @code{#>>}s, you will eventually run out of
    \G hold area; you can reset the hold area to empty with @code{<#}.
    holdend @ holdptr @ - hold
    holdptr @ holdend ! ;

: #>> ( -- ) \ gforth	number-sign-greater-greater
    \G Release the hold area started with @code{<<#}.
    holdend @ dup holdbuf-end u>= -&11 and throw
    count chars bounds holdptr ! holdend ! ;

: sign    ( n -- ) \ core
    \G Used between @code{<<#} and @code{#>}. If @var{n} (a
    \G @var{single} number) is negative, prepend ``@code{-}'' to the
    \G pictured numeric output string.
    0< IF '-' hold THEN ;

: digit ( n --  c )
    dup 9 u> [ char A char 9 1+ - ] Literal and + '0' + ;

: # ( ud1 -- ud2 ) \ core		number-sign
    \G Used between @code{<<#} and @code{#>}. Prepend the
    \G least-significant digit (according to @code{base}) of @var{ud1}
    \G to the pictured numeric output string.  @var{ud2} is
    \G @var{ud1/base}, i.e., the number representing the remaining
    \G digits.
    \ special-casing base=#10 does not pay off:
    \ <2022Mar11.130937@mips.complang.tuwien.ac.at>
    base @ ud/mod rot digit hold ;

: #s      ( ud -- 0 0 ) \ core	number-sign-s
    \G Used between @code{<<#} and @code{#>}.  Prepend all digits of
    \G @var{ud} to the pictured numeric output string.  @code{#s} will
    \G convert at least one digit. Therefore, if @var{ud} is 0,
    \G @code{#s} will prepend a ``0'' to the pictured numeric output
    \G string.
    dup if
        begin
            #
        dup 0= until
    then
    drop
    base @ #10 = if
        begin
            #10 u/mod swap '0' + hold
            \ optimizing #10 u/mod further slows down Rocket Lake
            \ <2025Nov23.103631@mips.complang.tuwien.ac.at>
        dup 0= until
    else
        begin
            base @ u/mod swap digit hold
        dup 0= until
    then
    0 ;

: holds ( addr u -- ) \ core-ext
    \G Used between @code{<<#} and @code{#>}. Prepend the string @code{addr u}
    \G to the pictured numeric output string.
    dup +hold swap move ;

\ print numbers                                        07jun92py

: type-r ( c-addr u u2 -- ) \ gforth-experimental
    \G Type string @i{c-addr u} right-aligned in field of width \i{u2}.
    over - spaces type ;

: d.r ( d n -- ) \ double	d-dot-r
    \G Display @var{d} right-aligned in a field @var{n} characters
    \G wide. If more than @var{n} characters are needed to display the
    \G number, all digits and, if necessary, the sign ``-'', are
    \G displayed.
    >r tuck dabs <<# #s rot sign #>
    r> type-r #>> ;

: ud.r ( ud n -- ) \ gforth	u-d-dot-r
    \G Display @var{ud} right-aligned in a field @var{n} characters wide. If more than
    \G @var{n} characters are needed to display the number, all digits are displayed.
    >r <<# #s #> r> type-r #>> ;

: .r ( n1 n2 -- ) \ core-ext	dot-r
    \G Display @var{n1} right-aligned in a field @var{n2} characters
    \G wide. If more than @var{n2} characters are needed to display
    \G the number, all digits and, if necessary, the sign ``-'', are
    \G displayed.
    >r s>d r> d.r ;

: u.r ( u n -- )  \ core-ext	u-dot-r
    \G Display @var{u} right-aligned in a field @var{n} characters wide. If more than
    \G @var{n} characters are needed to display the number, all digits are displayed.
    0 swap ud.r ;

base @ decimal \ suppress warning about d. being a literal
: d. ( d -- ) \ double	d-dot
    \G Display (the signed double number) @var{d} in free-format. followed by a space.
    0 d.r space ;
base !

: ud. ( ud -- ) \ gforth	u-d-dot
    \G Display (the signed double number) @var{ud} in free-format, followed by a space.
    0 ud.r space ;

: . ( n -- ) \ core	dot
    \G Display (the signed single number) @var{n} in free-format, followed by a space.
    s>d d. ;

: u. ( u -- ) \ core	u-dot
    \G Display (the unsigned single number) @var{u} in free-format, followed by a space.
    0 ud. ;

