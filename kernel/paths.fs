\ paths.fs path file handling                                    03may97jaw

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ -Changing the search-path:
\ fpath+ <path> 	adds a directory to the searchpath
\ fpath= <path>|<path>	makes complete now searchpath
\ 			seperator is |
\ .fpath		displays the search path
\ remark I: 
\ a ./ in the beginning of filename is expanded to the directory the
\ current file comes from. ./ can also be included in the search-path!
\ ~+/ loads from the current working directory

\ remark II:
\ if there is no sufficient space for the search path increase it!


\ -Creating custom paths:

\ It is possible to use the search mechanism on yourself.

\ Make a buffer for the path:
\ create mypath	100 chars , 	\ maximum length (is checked)
\ 		0 ,		\ real len
\ 		100 chars allot \ space for path
\ use the same functions as above with:
\ mypath path+ 
\ mypath path=
\ mypath .path

\ do a open with the search path:
\ open-path-file ( adr len path -- fd adr len ior )
\ the file is opened read-only; if the file is not found an error is generated

\ questions to: wilke@jwdt.com

[IFUNDEF] +place
: +place ( adr len adr )
        2dup >r >r
        dup c@ char+ + swap move
        r> r> dup c@ rot + swap c! ;
[THEN]

[IFUNDEF] place
: place ( c-addr1 u c-addr2 )
        2dup c! char+ swap move ;
[THEN]

create sourcepath 1024 chars , 0 , 1024 chars allot \ !! make this dynamic
sourcepath avalue fpath

: also-path ( adr len path^ -- )
  >r
  \ len check
  r@ cell+ @ over + r@ @ u> ABORT" path buffer too small!"
  \ copy into
  tuck r@ cell+ dup @ cell+ + swap cmove
  \ make delimiter
  0 r@ cell+ dup @ cell+ + 2 pick + c! 1 + r> cell+ +!
  ;

: only-path ( adr len path^ -- )
  dup 0 swap cell+ ! also-path ;

: path+ ( path-addr  "dir" -- ) \ gforth
    \G Add the directory @var{dir} to the search path @var{path-addr}.
    name rot also-path ;

: fpath+ ( "dir" ) \ gforth
    \G Add directory @var{dir} to the Forth search path.
    fpath path+ ;

: path= ( path-addr "dir1|dir2|dir3" ) \ gforth
    \G Make a complete new search path; the path separator is |.
    name 2dup bounds ?DO i c@ '| = IF 0 i c! THEN LOOP
    rot only-path ;

: fpath= ( "dir1|dir2|dir3" ) \ gforth
    \G Make a complete new Forth search path; the path separator is |.
    fpath path= ;

: path>counted  cell+ dup cell+ swap @ ;

: next-path ( adr len -- adr2 len2 )
  2dup 0 scan
  dup 0= IF     2drop 0 -rot 0 -rot EXIT THEN
  >r 1+ -rot r@ 1- -rot
  r> - ;

: previous-path ( path^ -- )
  dup path>counted
  BEGIN tuck dup WHILE repeat ;

: .path ( path-addr -- ) \ gforth
    \G Display the contents of the search path @var{path-addr}.
    path>counted
    BEGIN next-path dup WHILE type space REPEAT 2drop 2drop ;

: .fpath ( -- ) \ gforth
    \G Display the contents of the Forth search path.
    fpath .path ;

: absolut-path? ( addr u -- flag ) \ gforth
    \G A path is absolute if it starts with a / or a ~ (~ expansion),
    \G or if it is in the form ./*, extended regexp: ^[/~]|./, or if
    \G it has a colon as second character ("C:...").  Paths simply
    \G containing a / are not absolute!
    2dup 2 u> swap 1+ c@ ': = and >r \ dos absoulte: c:/....
    over c@ '/ = >r
    over c@ '~ = >r
    \ 2dup 3 min S" ../" compare 0= r> or >r \ not catered for in expandtopic
    2 min S" ./" compare 0=
    r> r> r> or or or ;

Create ofile 0 c, 255 chars allot
Create tfile 0 c, 255 chars allot

: pathsep? dup [char] / = swap [char] \ = or ;

: need/   ofile dup c@ + c@ pathsep? 0= IF s" /" ofile +place THEN ;

: extractpath ( adr len -- adr len2 )
  BEGIN dup WHILE 1-
        2dup + c@ pathsep? IF EXIT THEN
  REPEAT ;

: remove~+ ( -- )
    ofile count 3 min s" ~+/" compare 0=
    IF
	ofile count 3 /string ofile place
    THEN ;

: expandtopic ( -- ) \ stack effect correct? - anton
    \ expands "./" into an absolute name
    ofile count 2 min s" ./" compare 0=
    IF
	ofile count 1 /string tfile place
	0 ofile c! sourcefilename extractpath ofile place
	\ care of / only if there is a directory
	ofile c@ IF need/ THEN
	tfile count over c@ pathsep? IF 1 /string THEN
	ofile +place
    THEN ;

: compact.. ( adr len -- adr2 len2 )
    \ deletes phrases like "xy/.." out of our directory name 2dec97jaw
    over swap
    BEGIN  dup  WHILE
        dup >r '/ scan 2dup 4 min s" /../" compare 0=
        IF
            dup r> - >r 4 /string over r> + 4 -
            swap 2dup + >r move dup r> over -
        ELSE
            rdrop dup 1 min /string
        THEN
    REPEAT  drop over - ;

: reworkdir ( -- )
  remove~+
  ofile count compact..
  nip ofile c! ;

: open-ofile ( -- fid ior )
    \G opens the file whose name is in ofile
    expandtopic reworkdir
    ofile count r/o open-file ;

: check-path ( adr1 len1 adr2 len2 -- fd 0 | 0 <>0 )
  0 ofile ! >r >r ofile place need/
  r> r> ofile +place
  open-ofile ;

: open-path-file ( addr1 u1 path-addr -- wfileid addr2 u2 0 | ior ) \ gforth
    \G Look in path @var{path-addr} for the file specified by @var{addr1 u1}.
    \G If found, the resulting path and an open file descriptor
    \G are returned. If the file is not found, @var{ior} is non-zero.
  >r
  2dup absolut-path?
  IF    rdrop
        ofile place open-ofile
	dup 0= IF >r ofile count r> THEN EXIT
  ELSE  r> path>counted
        BEGIN  next-path dup
        WHILE  5 pick 5 pick check-path
        0= IF >r 2drop 2drop r> ofile count 0 EXIT ELSE drop THEN
  REPEAT
        2drop 2drop 2drop -&38
  THEN ;

: open-fpath-file ( addr1 u1 -- wfileid addr2 u2 0 | ior ) \ gforth
    \G Look in the Forth search path for the file specified by @var{addr1 u1}.
    \G If found, the resulting path and an open file descriptor
    \G are returned. If the file is not found, @var{ior} is non-zero.
    fpath open-path-file ;
