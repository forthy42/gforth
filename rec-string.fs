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
    \G Execution semantics: ( @i{ c-addr1 u --} )
    \G Copy the string described by @i{c-addr1 u}
    \G to @i{c-addr2 u} and compile the following semantics:@*
    \G Compiled semantics: ( @i{ -- c-addr2 u} ).
    postpone sliteral ;

' scan-string
:noname scan-string slit, ;
:noname scan-string slit, postpone 2lit, ;
translate: scan-translate-string ( c-addr1 u1 'ccc"' -- translation ) \ gforth-experimental
\G Every translation action also parses until the first non-escaped
\G @code{"}.  The string @i{c-addr u} and the parsed input are
\G concatenated, then the @code{\}-escapes are translated, giving
\G @i{c-addr2 u2}.@*
\G Interpreting run-time: @code{( @i{ -- c-addr2 u2} )}

' noop
' slit,
:noname slit, postpone 2lit, ;
translate: translate-string ( c-addr1 u1 -- translation ) \ gforth-experimental
\G Interpreting run-time: @code{( @i{ -- c-addr2 u2} )}@*
\G @i{c-addr2 u2} is the result of translating the @code{\}-escapes in
\G @i{c-addr1 u1}.

: ?scan-string ( addr u 'ccc"' scan-translate-string -- addr' u' translate-string  |  translation -- translation ) \ gforth-experimental
    \G Check if the token is an incomplete (side effect free) string,
    \G and scan the string to complete it.
    case
	scan-translate-string of  scan-string translate-string  endof
	0
    endcase ;

: rec-string ( c-addr u -- translation ) \ gforth-experimental
    \G A string starts and ends with @code{"} and may contain escaped
    \G characters, including @code{\"} (@pxref{String and character
    \G literals}). If @i{c-addr u} is the start of a string, the
    \G translation represents parsing the rest of the string, if
    \G necessary, converting the escaped characters, and pushing the
    \G string at run-time (@word{translate-string}
    \G @word{scan-translate-string}).  If @i{c-addr u} is not
    \G recognized as the start of a string, translation is
    \G @word{translate-none}.
    2dup s\" \"" string-prefix?
    IF    scan-translate-string
    ELSE  rec-none  THEN ;

' rec-string action-of rec-forth >back

0 [IF] \ dot-quoted strings, we don't need them
: .slit slit, postpone type ;
:noname scan-string type ;
:noname scan-string .slit ;
:noname scan-string slit, ]] 2lit, type [[ ; >postponer translate: translate-."
' translate-." Constant rectype-." \ gforth-obsolete

: rec-."  ( addr u -- addr u' translate-." | 0 )
    2dup ".\"" string-prefix?
    IF    ['] scan-translate-."
    ELSE  rec-none  THEN ;

' rec-." action-of rec-forth >back
[THEN]
