\ Quoted string recognizer

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2012,2013,2014,2015,2016,2017,2018,2019,2021,2022,2023,2024 Free Software Foundation, Inc.

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

s" Scanned string not in input buffer" exception >r

: ?in-inbuf ( addr u -- )
    bounds source bounds swap 1+ 2tuck within >r within r> and
    0= IF  [ r> ]L throw  THEN ;

: scan-string ( addr u -- addr' u' )
    2dup ?in-inbuf
    drop source drop - 1+ >in !
    ['] multiline-string \"-parse  save-mem ;

: slit, ( c-addr1 u -- ) \ gforth
    \G This is a non-immediate variant of @word{sliteral}@*
    \G Execution semantics: Copy the string described by @i{c-addr1 u}
    \G to @i{c-addr2 u} and Compile the following semantis:@*
    \G Compiled semantics: ( @i{ -- c-addr2 u} ).
    postpone sliteral ;

' scan-string
:noname scan-string slit, ;
:noname scan-string slit, postpone 2lit, ;
translate: scan-translate-string (  -- translator ) \ gforth-experimental
\G Additional data: @code{( @i{c-addr1 u1 'ccc"'} )}.@*
\G Every translator action also parses until the first non-escaped
\G @code{"}.  The string @i{c-addr u} and the parsed input are
\G concatenated, then the @code{\}-escapes are translated, giving
\G @i{c-addr2 u2}.@*
\G Interpreting run-time: @code{( @i{ -- c-addr2 u2} )}

' noop
' slit,
:noname slit, postpone 2lit, ;
translate: translate-string ( -- translator ) \ gforth-experimental
\G Additional data: @code{( @i{c-addr1 u1} )}.@*
\G Interpreting run-time: @code{( @i{ -- c-addr2 u2} )}@*
\G @i{c-addr2 u2} is the result of translating the @code{\}-escapes in
\G @i{c-addr1 u1}.

: ?scan-string ( addr u scan-translate-string -- addr' u' translate-string  |  ... translator -- ... translator ) \ gforth-experimental
    \G Check if the token is an incomplete (side effect free) string,
    \G and scan the string to complete it.
    case
	scan-translate-string of  scan-string translate-string  endof
	0
    endcase ;

: rec-string ( addr u -- addr u' scan-translate-string | 0 ) \ gforth-experimental
    \G Convert strings enclosed in double quotes into string literals,
    \G escapes are treated as in @code{S\"}.
    2dup s\" \"" string-prefix?
    IF    scan-translate-string
    ELSE  2drop 0  THEN ;

' rec-string action-of forth-recognize >back

0 [IF] \ dot-quoted strings, we don't need them
: .slit slit, postpone type ;
:noname scan-string type ;
:noname scan-string .slit ;
:noname scan-string slit, ]] 2lit, type [[ ; >postponer translate: translate-."
' translate-." Constant rectype-." \ gforth-obsolete

: rec-."  ( addr u -- addr u' translate-." | 0 )
    2dup ".\"" string-prefix?
    IF    ['] scan-translate-."
    ELSE  2drop 0  THEN ;

' rec-." action-of forth-recognize >back
[THEN]
