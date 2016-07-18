\ source location handling

\ Copyright (C) 1995,1997,2003,2004,2007,2009,2011,2014 Free Software Foundation, Inc.

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

: loadfilename#>str ( n -- addr u )
    dup 0< IF  drop s" *terminal*"  EXIT  THEN
    included-files $[]@ ;

\ we encode line and character in one cell to keep the interface the same

: decode-pos ( npos -- nline nchar )
    dup 8 rshift swap $ff and ;

: decode-pos1 ( xpos -- nfile nline nchar )
    dup 23 arshift swap $7fffff and decode-pos ;

: .sourcepos3 (  nfile nline nchar -- )
    rot loadfilename#>str type ': emit
    base @ decimal
    rot 0 .r ': emit swap 0 .r
    base ! ;

: .sourcepos1 ( xpos -- )
    decode-pos1 .sourcepos3 ;
    
: compile-sourcepos ( compile-time: -- ; run-time: -- xpos )
    \ compile the current source position as literals: nfile is the
    \ source file index, nline the line number within the file.
    current-sourcepos1
    postpone literal ;

: .sourcepos ( nfile npos -- )
    \ print source position
    decode-pos .sourcepos3 ;

: save-source-filename ( c-addr1 u1 -- c-addr2 u2 )
    \ c-addr1 u1 is a temporary string for a file name, c-addr2 u2 is
    \ a permanent one.  Reuses strings for the same file names and
    \ adds them to the included files (not sure if that's a good idea)
    2dup str>loadfilename# dup 0< if
	drop save-mem 2dup add-included-file
    else
	nip nip loadfilename#>str
    then ;

: #line ( "u" "["file"]" -- )
    \g Set the line number to @i{u} and (if present) the file name to @i{file}.  Consumes the rest of the line.
    \g 
    parse-name ['] evaluate 10 base-execute 1- loadline !
    '"' parse 2drop '"' parse dup if
	save-source-filename loadfilename 2!
    else
	2drop
    then
    postpone \ ;
