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

$variable provider-file
10 stack: providers

?: >abspath ( addr u -- addr' u' )
    over c@ '/' <> IF
	[: {: | pwd[ $1000 ] :} pwd[ $1000 get-dir
	    type '/' emit type ;] $tmp
	compact-filename
    THEN ;
: <no-provides ( -- )
    \G lines that aren't provided by some upper level
    0 provider-file !@ providers >stack ;
: <provides ( -- )
    \G lines that are provided by the current file
    <no-provides  sourcefilename >abspath provider-file $! ;
: provides> ( -- )
    \G end of a provides/no-provides block
    provider-file $free providers stack> provider-file ! ;
: source-provider ( -- addr u )
    \G what's the source provider's file name?
    provider-file $@ 2dup d0= IF  2drop sourcefilename >abspath  THEN ;

[IFUNDEF] provides-file
    : provides-file ( -- addr u )
	${XDG_DATA_HOME} dup 0= IF  2drop "~/.local/share"  THEN
	[: type ." /gforth/provides" ;] $tmp ;
[THEN]

$variable last-provider
0 Value provides.fd

: provides-header ( -- )
    provides.fd 0= latest 0= or get-current >voc xt? 0= or ?EXIT
    [:  source-provider 2dup last-provider $@ str= 0=
	IF
	    2dup last-provider $!
	    cr type ':' emit
	ELSE  2drop
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
