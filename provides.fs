\ need database generator

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

2variable last-provider
2variable provider-file
0 Value provides.fd

: <provides ( -- ) ." <provides " sourcefilename type cr
    sourcefilename provider-file 2! ;
: provides> ( -- ) ." provides>" cr #0. provider-file 2! ;
: source-provider ( -- addr u )
    provider-file 2@ 2dup d0= IF  2drop sourcefilename  THEN ;

[IFUNDEF] provides-file
    : provides-file ( -- addr u )
	${XDG_DATA_HOME} dup 0= IF  2drop "~/.local/share"  THEN
	[: type ." /gforth/provides" ;] $tmp ;
[THEN]

: provides-header ( -- )
    provides.fd 0= latest 0= or get-current >voc xt? 0= or ?EXIT
    [:  source-provider last-provider 2@ d<>
	IF
	    source-provider last-provider 2!
	    cr source-provider type ':' emit
	THEN
	space
	get-current forth-wordlist <> IF
	    get-current >voc name>string type ':' emit
	THEN
	latest name>string type
    ;] provides.fd outfile-execute ;

: provides-file ( -- addr u )
    ${GFORTH_PROVIDES} dup ?EXIT
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
