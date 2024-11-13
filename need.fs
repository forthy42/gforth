\ need stuff

\ Authors: Bernd Paysan
\ Copyright (C) 2024 Free Software Foundation, Inc.

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

\ create database for what sources provide

2variable last-loadfilename
0 Value provides.fd

: provides-header ( -- )
    provides.fd 0= latest 0= or get-current >voc xt? 0= or ?EXIT
    [:  sourcefilename last-loadfilename 2@ d<>
	IF
	    sourcefilename last-loadfilename 2!
	    cr sourcefilename type ':' emit
	THEN
	space
	get-current forth-wordlist <> IF
	    get-current >voc name>string type ':' emit
	THEN
	latest name>string type
    ;] provides.fd outfile-execute ;

: provides-file ( -- addr u )
    ${XDG_DATA_HOME} dup 0= IF  2drop "~/.local/share"  THEN
    [: type ." /gforth/provides" ;] $tmp ;

\ to generate the database, call
\ gforth need.fs -e 'provides' <files>
\ and to add more infos to the data base
\ gforth need.fs -e 'provides+' <more-files>

: provides ( -- )
    provides-file w/o create-file throw to provides.fd
    ['] provides-header IS header-extra ;
: provides+ ( -- )
    provides-file w/o open-file throw to provides.fd
    ['] provides-header IS header-extra ;

\ read said database matching need query

$[]variable $need[]
$[]variable $require[]

: init-need ( -- )
    $need[] $[]free
    $require[] $[]free ;

$Variable last-require

: rec-require ( addr u -- addr u xt | 0 )
    over source drop = >r \ start of line
    2dup + 1- c@ ':' = r> and IF
	[: 1- last-require $! ;]
    ELSE  2drop false  THEN ;
: rec-provide ( addr u -- addr u xt | 0 )
    [: $need[] [: 2over str= IF
		last-require @ IF
		    last-require $@ $require[] $+[]!
		    postpone \ \ no need to look at the rest of the line
		THEN
	    THEN ;] $[]map 2drop
    ;] ;

' rec-provide ' rec-require 2 recognizer-sequence: rec-needs
' rec-scope ' rec-nt 2 recognizer-sequence: rec-checkneeds

: checkneeds ( addr u -- nt )
    rec-checkneeds dup IF drop THEN ;

: read-needs ( -- )
    provides-file r/o open-file throw
    [: ['] rec-needs ['] forth-recognize ['] read-loop wrap-xt ;]
    execute-parsing-file ;

: parse-needs ( "need1" .. "needn" "close-paren" -- )
    BEGIN   parse-name 2dup ")" str= 0= WHILE
	    dup 0= IF
		2drop
		source-id 0= IF
		    success-color ." need" default-color cr
		    input-color  THEN
		refill 0=
	    ELSE
		2dup checkneeds IF  2drop  ELSE  $need[] $+[]!  THEN false
	    THEN
	UNTIL  ELSE  2drop  THEN ;

: require-needs ( -- )
    $require[] ['] required $[]map ;

: check-needs ( -- )
    \ checks don't work if words are burried in other
    \ vocabularies
    $need[] [: checkneeds 0= -13 and throw ;] $[]map ;

: need( ( "need1" .. "needn" "close-paren" -- )
    init-need
    parse-needs
    read-needs require-needs ;
: need ( "need" -- )
    init-need
    parse-name name-too-short? $need[] $+[]!
    read-needs require-needs ;

