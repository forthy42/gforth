\ builttag.fs

\ Copyright (C) 1998,2000,2003,2004,2007 Free Software Foundation, Inc.

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

0 [IF]

This is a cross compiler extension.

[THEN]

base @ decimal

variable builtnr
create linebuf 200 chars allot
create filename 200 chars allot
0 value btfd

: s'
  [char] ' parse postpone sliteral ; immediate

[IFDEF] project-name
: extractproject ( -- adr len ) project-name ;
[ELSE]

defined? sourcefilename 0= [IF]
	  cr ." I need project-name defined for builttag" abort
[THEN]

: extractproject ( -- adr len )
  sourcefilename 2dup >r >r
  BEGIN dup WHILE 1-
        2dup + c@ [char] . = IF r> drop r> drop EXIT THEN
  REPEAT 2drop r> r> ;

[THEN]

get-current >MINIMAL

: builttag
  base @ >r decimal
  extractproject filename place
  s" .n" filename +place
  filename count r/o open-file 
  IF   drop 0 builtnr !
  ELSE 	>r linebuf 100 r@ read-line drop drop
	linebuf swap 0 -rot 0 -rot >number 2drop drop 1+
	builtnr ! r> close-file throw
  THEN
  filename count r/w create-file throw to btfd
  builtnr @ s>d <# #S #> btfd write-file throw
  s"  constant built#" btfd write-line throw
  s' const create builtdate ," ' btfd write-file throw
  time&date >r >r >r
  s>d <# [char] : hold # # #> btfd write-file throw
  s>d <# bl hold # # #> btfd write-file throw
  drop
  r> s>d <# [char] . hold # # #> btfd write-file throw
  r> s>d <# [char] . hold # # #> btfd write-file throw
  r> s>d <# # # # # #> btfd write-file throw
  s' "' btfd write-line throw
  s' : .built cr ." Built #" built# . ." Date " builtdate count type cr ;'
  btfd write-line throw
  btfd close-file throw
  filename count included 
  r> base ! ;

set-current
base !
