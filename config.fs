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

translate-method: configuring

Vocabulary config
' config >wordlist Value config-wl

' rec-string ' rec-number ' rec-float 3 rec-sequence: config-recognize
\ The config recognizer

s" Config error" exception Value config-throw
\ if you don't want an exception, set config-throw to 0

: .config-err ( -- )
    current-sourceview .sourceview
    ." : can't parse config line: '" source type ." '" cr
    config-throw throw ;
: exec-config ( .. addr u char xt1 xt2 -- ) >r >r
    [: >r type r> emit ;] $tmp config-wl find-name-in
    ?dup-IF  execute r> execute rdrop
    ELSE rdrop r> execute .config-err THEN ;

:noname '$' ['] $! [: drop free throw ;] exec-config ;
translate-string is configuring
:noname '#' ['] !  ['] drop exec-config ;
translate-cell   is configuring
:noname '&' ['] 2! ['] 2drop exec-config ;
translate-dcell  is configuring
:noname '%' ['] f! ['] fdrop exec-config ;
translate-float  is configuring
translate-none [IF]
    ' .config-err ' translate-none is configuring
[THEN]

: config-line ( -- )
\    current-sourceview .sourceview ." : config line='" source type ." '" cr
    source nip 0= ?EXIT
    source bl skip ";" string-prefix? ?EXIT
    '=' parse -trailing 2>r
    parse-name config-recognize ?scan-string 2r> rot
    [ translate-none ] [IF]  configuring
    [ELSE]
	?dup-IF  configuring  ELSE  2drop .config-err  THEN
    [THEN]
    postpone \ ;

: read-config-loop ( -- )
    BEGIN  refill  WHILE  config-line  REPEAT ;

: read-config ( addr u wid -- )  to config-wl
    >included throw add-included-file  included-files $@ + cell-
    $@ ['] read-config-loop execute-parsing-named-file
    included-files stack> { w^ file } file $free ;

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
