\ config file reader/writer

\ Author: Bernd Paysan
\ Copyright (C) 2016   Bernd Paysan

\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

require rec-scope.fs
require recognizer-ext.fs

rec-method rectype-config

Vocabulary config
' config >wordlist Value config-wl

3 stack: config-recognizer
\G The config recognizer

' rec-string ' rec-num ' rec-float 3 config-recognizer set-stack

s" Config error" exception Value config-throw
\ if you don't want an exception, set config-throw to 0

: .config-err ( -- )
    ." can't parse config line " sourceline# 0 .r ." : '" source type ." '" cr
    config-throw throw ;
: exec-config ( .. addr u char xt1 xt2 -- ) >r >r
    [: >r type r> emit ;] $tmp config-wl find-name-in
    ?dup-IF  execute r> execute rdrop
    ELSE rdrop r> execute .config-err THEN ;

: eval-config ( .. rec addr u -- )  rot rectype-config ;

:noname '$' ['] $! [: drop free throw ;] exec-config ;
rectype-string is rectype-config
:noname '#' ['] !  ['] drop exec-config ;
rectype-num    is rectype-config
:noname '&' ['] 2! ['] 2drop exec-config ;
rectype-dnum   is rectype-config
:noname '%' ['] f! ['] fdrop exec-config ;
rectype-float  is rectype-config
' .config-err rectype-null is rectype-config

: config-line ( -- )
    '=' parse 2>r
    parse-name config-recognizer recognize 2r> eval-config
    postpone \ ;

: read-config-loop ( -- )
    BEGIN  refill  WHILE  config-line  REPEAT ;

: read-config ( addr u wid -- )  to config-wl
    >included throw ['] read-config-loop execute-parsing-named-file ;

: write-config ( addr u wid -- )  to config-wl
    r/w create-file throw >r
    [: config-wl
	[: dup name>string 1- 2dup + c@ >r type .\" ="
	    execute r>
	    case
		'$' of  $@ [: '"' emit see-voc:c-\type '"' emit ;] $tmp type cr  endof
		'#' of  @ 0 .r cr  endof
		'&' of  '#' emit 2@ 0 d.r '.' emit cr  endof
		'%' of  f@ fe. cr  endof
		drop
	    endcase
	;] map-wordlist ;] r@ outfile-execute
    r> close-file throw ;
