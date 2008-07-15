\ paths.fs path file handling                                    03may97jaw

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008 Free Software Foundation, Inc.

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
\ create mypath	100 path,
\ mypath path+ 
\ mypath path=
\ mypath .path

\ do a open with the search path:
\ open-path-file ( adr len path -- fd adr len ior )
\ the file is opened read-only; if the file is not found an error is generated

\ questions to: wilke@jwdt.com

: path-allot ( umax -- ) \ gforth
    \G @code{Allot} a path with @i{umax} characters capacity, initially empty.
    chars dup , 0 , allot ;

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

\ create sourcepath 1024 chars , 0 , 1024 chars allot \ !! make this dynamic
0 avalue fpath ( -- path-addr ) \ gforth

: make-path ( -- addr )
    $400 chars dup 2 cells + allocate throw >r
    0 swap r@ 2! r> ;

: os-cold ( -- )
    make-path to fpath
    pathstring 2@ fpath only-path 
    init-included-files ;

\ The path Gforth uses for @code{included} and friends.

: also-path ( c-addr len path-addr -- ) \ gforth
    \G add the directory @i{c-addr len} to @i{path-addr}.
  >r
  \ len check
  r@ cell+ @ over + r@ @ u> ABORT" path buffer too small!" \ !! grow it
  \ copy into
  tuck r@ cell+ dup @ cell+ + swap cmove
  \ make delimiter
  0 r@ cell+ dup @ cell+ + 2 pick + c! 1 + r> cell+ +!
;

: clear-path ( path-addr -- ) \ gforth
    \G Set the path @i{path-addr} to empty.
    0 swap cell+ ! ;

: only-path ( adr len path^ -- )
    dup clear-path also-path ;

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

: path>string ( path -- c-addr u )
    \ string contains NULs to separate/terminate components
    cell+ dup cell+ swap @ ;

: next-path ( addr u -- addr1 u1 addr2 u2 )
    \ addr2 u2 is the first component of the path, addr1 u1 is the rest
    2dup 0 scan
    dup 0= IF     2drop 0 -rot 0 -rot EXIT THEN
    >r 1+ -rot r@ 1- -rot
    r> - ;

: previous-path ( path^ -- )
    \ !! "fpath previous-path" doesn't work
  dup path>string
  BEGIN tuck dup WHILE repeat ;

: .path ( path-addr -- ) \ gforth
    \G Display the contents of the search path @var{path-addr}.
    path>string
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
    \ 2dup S" ../" string-prefix? r> or >r \ not catered for in expandtopic
    S" ./" string-prefix?
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
    ofile count s" ~+/" string-prefix?
    IF
	ofile count 3 /string ofile place
    THEN ;

: expandtopic ( -- ) \ stack effect correct? - anton
    \ expands "./" into an absolute name
    ofile count s" ./" string-prefix?
    IF
	ofile count 1 /string tfile place
	0 ofile c! includefilename 2@ extractpath ofile place
	\ care of / only if there is a directory
	ofile c@ IF need/ THEN
	tfile count over c@ pathsep? IF 1 /string THEN
	ofile +place
    THEN ;

: del-string ( addr u u1 -- addr u2 )
    \ delete u1 characters from string by moving stuff from further up
    2 pick >r /string r@ over >r swap cmove 2r> ;

: del-./s ( addr u -- addr u2 )
    \ deletes (/*./)* at the start of the string
    BEGIN ( current-addr u )
	BEGIN ( current-addr u )
	    over c@ '/ = WHILE
		1 del-string
	REPEAT
	2dup s" ./" string-prefix? WHILE
	    2 del-string
    REPEAT ;

: preserve-root ( addr1 u1 -- addr2 u2 )
    over c@ '/ = if \ preserve / at start
	1 /string
    endif ;


: skip-..-prefixes ( addr1 u1 -- addr2 u2 )
    \ deal with ../ at start
    begin ( current-addr u )
	del-./s 2dup s" ../" string-prefix? while
	    3 /string
    repeat ;
    
: compact-filename ( addr u1 -- addr u2 )
    \ rewrite filename in place, eliminating multiple slashes, "./", and "x/.."
    over swap preserve-root skip-..-prefixes
    ( start current-addr u )
    over swap '/ scan dup if ( start addr3 addr4 u4 )
	1 /string del-./s recurse
	2dup s" ../" string-prefix? if ( start addr3 addr4 u4 )
	    3 /string ( start to from count )
	    >r swap 2dup r@ cmove r>
	endif
    endif
    + nip over - ;

\ test cases:
\ s" z/../../../a" compact-filename type cr
\ s" ../z/../../../a/c" compact-filename type cr
\ s" /././//./../..///x/y/../z/.././..//..//a//b/../c" compact-filename type cr

: reworkdir ( -- )
  remove~+
  ofile count compact-filename
  nip ofile c! ;

: open-ofile ( -- fid ior )
    \G opens the file whose name is in ofile
    expandtopic reworkdir
    ofile count r/o open-file ;

: check-path ( adr1 len1 adr2 len2 -- fid 0 | 0 ior )
  0 ofile ! >r >r ofile place need/
  r> r> ofile +place
  open-ofile ;

\ !! allow arbitrary FAMs, not just R/O
: open-path-file ( addr1 u1 path-addr -- wfileid addr2 u2 0 | ior ) \ gforth
\G Look in path @var{path-addr} for the file specified by @var{addr1
\G u1}.  If found, the resulting path and and (read-only) open file
\G descriptor are returned. If the file is not found, @var{ior} is
\G what came back from the last attempt at opening the file (in the
\G current implementation).
    >r
    2dup absolut-path? IF
        rdrop
        ofile place open-ofile
        dup 0= IF
            >r ofile count r> THEN
        EXIT
    ELSE
        r> -&37 >r path>string BEGIN
            next-path dup WHILE
                r> drop
                5 pick 5 pick check-path dup 0= IF
                    drop >r 2drop 2drop r> ofile count 0 EXIT
                ELSE
                    >r drop
                THEN
        REPEAT
        2drop 2drop 2drop r>
  THEN ;

: open-fpath-file ( addr1 u1 -- wfileid addr2 u2 0 | ior ) \ gforth
    \G Look in the Forth search path for the file specified by @var{addr1 u1}.
    \G If found, the resulting path and an open file descriptor
    \G are returned. If the file is not found, @var{ior} is non-zero.
    fpath open-path-file ;
