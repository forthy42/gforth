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
require mkdir.fs

translator: config-translator

Vocabulary config
' config >wordlist Value config-wl

' rec-string ' rec-num ' rec-float 3 recognizer-sequence: config-recognize
\G The config recognizer

s" Config error" exception Value config-throw
\ if you don't want an exception, set config-throw to 0

: .config-err ( -- )
    ." can't parse config line " sourceline# 0 .r ." : '" source type ." '" cr
    config-throw throw ;
: exec-config ( .. addr u char xt1 xt2 -- ) >r >r
    [: >r type r> emit ;] $tmp config-wl find-name-in
    ?dup-IF  execute r> execute rdrop
    ELSE rdrop r> execute .config-err THEN ;

: eval-config ( .. rec addr u -- )  rot config-translator execute ;

:noname '$' ['] $! [: drop free throw ;] exec-config ;
' translate-string to config-translator
:noname '#' ['] !  ['] drop exec-config ;
' translate-num    to config-translator
:noname '&' ['] 2! ['] 2drop exec-config ;
' translate-dnum   to config-translator
:noname '%' ['] f! ['] fdrop exec-config ;
' translate-float  to config-translator
' .config-err ' notfound to config-translator

: config-line ( -- )
    source nip 0= ?EXIT
    '=' parse -trailing 2>r
    parse-name config-recognize 2r> eval-config
    postpone \ ;

: read-config-loop ( -- )
    BEGIN  refill  WHILE  config-line  REPEAT ;

: read-config ( addr u wid -- )  to config-wl
    >included throw ['] read-config-loop execute-parsing-named-file ;

: write-config ( addr u wid -- )  to config-wl
    force-open >r
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
