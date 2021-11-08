\ source location handling

\ Authors: Anton Ertl, Bernd Paysan, Gerald Wodni
\ Copyright (C) 1995,1997,2003,2004,2007,2009,2011,2014,2016,2017,2018,2019 Free Software Foundation, Inc.

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

\ related stuff can be found in kernel.fs

\ this stuff is used by (at least) assert.fs and debugs.fs

\ 1-cell encoded position: filenameno9b:lineno15b:charno8b

require string.fs

-1 #23 rshift Constant *terminal*#

Defer filename>display  ' noop is filename>display

\ filename>display variants

Variable lastfile

: shorten-file ( addr u -- addr' u' ) { d: fn }
    fpath path>string BEGIN
	next-path dup  WHILE
	    fn 2over string-prefix?
	    over fn rot safe/string
	    IF  c@ '/' = and  ELSE  2drop false  THEN
	    IF  fn rot 1+ /string  rot drop 2nip  EXIT
	    ELSE  2drop  THEN
    REPEAT  2drop 2drop fn ;

: expand-file ( addr u -- addr' u' ) { d: fn }
    fn open-fpath-file IF  fn  ELSE  rot close-file drop  THEN
    over c@ '/' <> IF
	{ | pwd[ $1000 ] }
	pwd[ $1000 get-dir [: type '/' emit type ;] $tmp compact-filename
    THEN ;

: prepend-file ( addr u -- addr 0 )
    over lastfile @ <> IF
	over lastfile !
	[:  status-color 2dup expand-file
	    1 1 2over cols +x-lines+rest drop 1 > IF
		2drop 2dup shorten-file  THEN
	    type ':' emit default-color cr ;] ['] execute-theme-color
	do-debug \ side-emit it on debugging output
    THEN  drop 0 ;

0 Value source-line#
0 Value source-pos#

: plain~~ ( -- )
    \G uses the file as is (default)
    ['] noop is filename>display
    0 to source-line# 0 to source-pos# ;
: short~~ ( -- )
    \G uses a short file format (default)
    plain~~ ['] shorten-file is filename>display ;
: expand~~ ( -- )
    \G uses a fully expanded file format (to pass to e.g. editors)
    plain~~ ['] expand-file is filename>display ;
: prepend~~ ( -- )
    \G prepends the file to a list of locations in that file (like SwiftForth)
    ['] prepend-file is filename>display
    3 to source-line# 2 to source-pos# ;
plain~~

: loadfilename#>str ( n -- addr u )
    dup *terminal*# and *terminal*# = IF  drop s" *terminal*"  EXIT  THEN
    included-files $[]@ filename>display ;

\ we encode line and character in one cell to keep the interface the same

: decode-pos ( npos -- nline nchar )
    dup 8 rshift swap $ff and ;

: decode-view ( view -- nfile nline nchar )
    dup 23 rshift swap $7fffff and decode-pos ;

: view>char ( view -- u )
    $ff and ;

: .sourcepos3 (  nfile nline nchar -- )
    rot loadfilename#>str type ': emit
    base @ decimal
    rot source-line# .r ': emit swap 1+ source-pos# .r
    base ! ;

: .sourceview ( view -- )
    decode-view .sourcepos3 ;

: compile-sourcepos ( compile-time: -- ; run-time: -- view )
    \ compile the current source position as literals: nfile is the
    \ source file index, nline the line number within the file.
    current-sourceview
    postpone literal ;

: .sourcepos ( nfile npos -- )
    \ print source position
    decode-pos .sourcepos3 ;

: save-source-filename# ( c-addr1 u1 -- index )
    \ adds a permanent copy of c-addr1 u1 to the included file names,
    \ returning the index into the included-files
    2dup str>loadfilename# dup 0< if
	drop add-included-file included-files $[]# 1-
    else
	nip nip
    then ;

: #line ( "u" "["file"]" -- )
    \g Set the line number to @i{u} and (if present) the file name to @i{file}.  Consumes the rest of the line.
    \g 
    parse-name ['] evaluate 10 base-execute 1- loadline !
    '"' parse 2drop '"' parse dup if
	save-source-filename# loadfilename# !
    else
	2drop
    then
    postpone \ ;
