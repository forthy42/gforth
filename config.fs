\ config file reader/writer

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

Vocabulary config
' config >body Value config-wl

3 stack: config-recognizer
\G The config recognizer

' rec:string ' rec:num ' rec:float 3 config-recognizer set-stack

: .config-err ( -- )
    ." unknown config variable: '" source type ." '" cr ;
: exec-config ( .. addr u char xt1 xt2 -- ) >r >r
    [: >r type r> emit ;] $tmp config-wl find-name-in
    ?dup-IF  execute r> execute rdrop
    ELSE rdrop r> execute .config-err THEN ;

: eval-config ( .. rec addr u -- )  rot
    case
	r:string of  '$' ['] $! [: drop free throw ;] exec-config  endof
	r:num    of  '#' ['] !  ['] drop exec-config               endof
	r:dnum   of  '&' ['] 2! ['] 2drop exec-config              endof
	r:float  of  '%' ['] f! ['] fdrop exec-config              endof
	2drop ." can't parse config line: '" source type ." '" cr
    endcase ;

: config-line ( -- )
    '=' parse 2>r
    parse-name config-recognizer map-recognizer 2r> eval-config
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

:noname defers 'cold config-recognizer $boot ; is 'cold
:noname config-recognizer $save defers 'image ; is 'image
