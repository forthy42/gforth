\ CROSS.FS     The Cross-Compiler                      06oct92py
\ Idea and implementation: Bernd Paysan (py)

\ Authors: Bernd Paysan, Anton Ertl, Jens Wilke, David Kühling, Neal Crook
\ Copyright (C) 1995,1996,1997,1998,1999,2000,2003,2004,2005,2006,2007,2009,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

0 
[IF]

ToDo:
- Crossdoc destination ./doc/crossdoc.fd makes no sense when
  cross.fs is used seperately. jaw
- Do we need this char translation with >address and in branchoffset? 
  (>body also affected) jaw

[THEN]

s" compat/strcomp.fs" included

hex

\ debugging for compiling

\ print stack at each colon definition
\ : : save-input cr bl word count type restore-input throw .s : ;

\ print stack at each created word
\ : create save-input cr bl word count type restore-input throw .s create ;


\ \ -------------  Setup Vocabularies

\ Remark: Vocabulary is not ANS, but it should work...

Vocabulary Cross
Vocabulary Target
Vocabulary Ghosts
Vocabulary Minimal
only Forth also Target also also
definitions Forth

: T  previous Ghosts also Target ; immediate
: G  Ghosts ; immediate
: H  previous Forth also Cross ; immediate

forth definitions

: T  previous Ghosts also Target ; immediate
warnings @ 0 warnings !
: G  Ghosts ; immediate
warnings !

: >cross  also Cross definitions previous ;
: >target also Target definitions previous ;
: >minimal also Minimal definitions previous ;

H

>CROSS

\ Test against this definitions to find out whether we are cross-compiling
\ may be useful for assemblers
0 Constant gforth-cross-indicator

[IFUNDEF] +place
: +place ( adr len adr )
    2dup c@ dup >r  + over c!  r> char+ +  swap move ;
[THEN]

[IFUNDEF] place
: place ( c-addr1 u c-addr2 )
    2dup c! char+ swap move ;
[THEN]

\ find out whether we are compiling with gforth

: bl-word ( -- addr )
    parse-name here place here ;
: defined? parse-name find-name 0<> ;
defined? emit-file defined? toupper and \ drop 0
[IF]
\ use this in a gforth system
: \GFORTH ; immediate
: \ANSI postpone \ ; immediate
[ELSE]
: \GFORTH postpone \ ; immediate
: \ANSI ; immediate
[THEN]

\ANSI : [IFUNDEF] defined? 0= postpone [IF] ; immediate
\ANSI : [IFDEF] defined? postpone [IF] ; immediate
0 \ANSI drop 1
[IF]
: \G postpone \ ; immediate
: rdrop postpone r> postpone drop ; immediate
: parse-name bl-word count ;
: bounds over + swap ;
: scan >r BEGIN dup WHILE over c@ r@ <> WHILE 1 /string REPEAT THEN rdrop ;
: linked here over @ , swap ! ;
: alias create , DOES> @ EXECUTE ;
: defer ['] noop alias ;
: is state @ 
  IF ' >body postpone literal postpone ! 
  ELSE ' >body ! THEN ; immediate
: 0>= 0< 0= ;
: d<> rot <> -rot <> or ;
: toupper dup 'a' 'z' 1+ within IF 'A' 'a' - + THEN ;
Variable ebuf
: emit-file ( c fd -- ior ) swap ebuf c! ebuf 1 chars rot write-file ;
0a Constant #lf
0d Constant #cr

[IFUNDEF] Warnings Variable Warnings [THEN]

\ \ Number parsing					23feb93py

\ number? number                                       23feb93py

Variable dpl

hex
Create bases   10 ,   2 ,   A , 100 ,
\              16     2    10   character

\ !! protect BASE saving wrapper against exceptions
: getbase ( addr u -- addr' u' )
    over c@ '$' - dup 4 u<
    IF
	cells bases + @ base ! 1 /string
    ELSE
	drop
    THEN ;

: sign? ( addr u -- addr u flag )
    over c@ '-' =  dup >r
    IF
	1 /string
    THEN
    r> ;

: s>unumber? ( addr u -- ud flag )
    over ''' =
    IF 	\ a ' alone is rather unusual :-)
	drop char+ c@ 0 true EXIT 
    THEN
    base @ >r  dpl on  getbase
    0. 2swap
    BEGIN ( d addr len )
	dup >r >number dup
    WHILE \ there are characters left
	dup r> -
    WHILE \ the last >number parsed something
	dup 1- dpl ! over c@ '.' =
    WHILE \ the current char is '.'
	1 /string
    REPEAT  THEN \ there are unparseable characters left
	2drop false
    ELSE
	rdrop 2drop true
    THEN
    r> base ! ;

\ ouch, this is complicated; there must be a simpler way - anton
: s>number? ( addr len -- d f )
    \ converts string addr len into d, flag indicates success
    sign? >r
    s>unumber?
    0= IF
        rdrop false
    ELSE \ no characters left, all ok
	r>
	IF
	    dnegate
	THEN
	true
    THEN ;

: s>number ( addr len -- d )
    \ don't use this, there is no way to tell success
    s>number? drop ;

: snumber? ( c-addr u -- 0 / n -1 / d 0> )
    s>number? 0=
    IF
	2drop false  EXIT
    THEN
    dpl @ dup 0< IF
	nip
    ELSE
	1+
    THEN ;

: (number?) ( string -- string 0 / n -1 / d 0> )
    dup >r count snumber? dup if
	rdrop
    else
	r> swap
    then ;

: number ( string -- d )
    (number?) ?dup 0= abort" ?"  0<
    IF
	s>d
    THEN ;

[THEN]

[IFUNDEF] (number?) : (number?) number? ; [THEN]

\ this provides assert( and struct stuff
\GFORTH [IFUNDEF] assert1(
\GFORTH also forth definitions require assert.fs previous
\GFORTH [THEN]
[IFUNDEF] $@
    : $@ @ dup IF  dup cell+ swap @  ELSE  0  THEN ;
[THEN]
[IFUNDEF] >stack
    : $@len ( $addr -- u ) \ gforth-string string-fetch-len
	\G returns the length of the stored string.
	@ dup IF  @  THEN ;
    : >pow2 ( n -- pow2 )
	1-
	dup 2/ or \ next power of 2
	dup 2 rshift or
	dup 4 rshift or
	dup 8 rshift or
	dup #16 rshift or
	[ cell 8 = [IF] ]
	    dup #32 rshift or
	    [ [THEN] ] 1+ ;
    : $padding ( n -- n' ) \ gforth-string
	[ 6 cells ] Literal +  >pow2  [ -4 cells ] Literal and ;
    : $!len ( u $addr -- ) \ gforth-string string-store-len
	\G changes the length of the stored string.  Therefore we must
	\G change the memory area and adjust address and count cell as
	\G well.
	over $padding  over @ IF  \ fast path for unneeded size change
	    over @ @ $padding over = IF  drop @ !  EXIT  THEN
	THEN
	over @ swap resize throw over ! @ ! ;
    : >stack ( x stack -- )
	\G push to top of stack
	dup >r $@len cell+ r@ $!len
	r> $@ + 1 cells - ! ;
[THEN]

>CROSS

hex     \ the defualt base for the cross-compiler is hex !!
\ Warnings off

\ words that are generaly useful

: KB  400 * ;
: >wordlist ( vocabulary-xt -- wordlist-struct )
  also execute get-order swap >r 1- set-order r> ;

: umax 2dup u< IF swap THEN drop ;
: umin 2dup u> IF swap THEN drop ;

: string, ( c-addr u -- )
    \ puts down string as cstring
    dup c, here swap chars dup allot move ;

: ," '"' parse string, ;

: SetValue ( n -- <name> )
\G Same behaviour as "Value" if the <name> is not defined
\G Same behaviour as "to" if the <name> is defined
\G SetValue searches in the current vocabulary
  save-input bl-word >r restore-input throw r> count
  get-current search-wordlist
  IF	drop >r
	\ we have to set current to be topmost context wordlist
	get-order get-order get-current swap 1+ set-order
	r> ['] to execute
	set-order
  ELSE Value THEN ;

: DefaultValue ( n -- <name> )
\G Same behaviour as "Value" if the <name> is not defined
\G DefaultValue searches in the current vocabulary
 save-input bl-word >r restore-input throw r> count
 get-current search-wordlist
 IF bl-word drop 2drop ELSE Value THEN ;

hex

\ FIXME delete`
\ 1 Constant Cross-Flag	\ to check whether assembler compiler plug-ins are
			\ for cross-compiling
\ No! we use "[IFUNDEF]" there to find out whether we are target compiling!!!

\ FIXME move down
: comment? ( c-addr u -- c-addr u )
        2dup s" (" str=
        IF    postpone (
        ELSE  2dup s" \" str= IF postpone \ THEN
        THEN ;

: X ( -- <name> )
\G The next word in the input is a target word.
\G Equivalent to T <name> but without permanent
\G switch to target dictionary. Used as prefix e.g. for @, !, here etc.
  parse-name [ ' target >wordlist ] Literal search-wordlist
  IF state @ IF compile, ELSE execute THEN
  ELSE	-1 ABORT" Cross: access method not supported!"
  THEN ; immediate

\ Begin CROSS COMPILER:

\ debugging

0 [IF]

This implements debugflags for the cross compiler and the compiled
images. It works identical to the has-flags in the environment.
The debugflags are defined in a vocabluary. If the word exists and
its value is true, the flag is switched on.

[THEN]

>CROSS

Vocabulary debugflags	\ debug flags for cross
also debugflags get-order over
Constant debugflags-wl
set-order previous

: DebugFlag
  get-current >r debugflags-wl set-current
  SetValue
  r> set-current ;

: Debug? ( adr u -- flag )
\G return true if debug flag is defined or switched on
  debugflags-wl search-wordlist
  IF EXECUTE
  ELSE false THEN ;

: D? ( <name> -- flag )
\G return true if debug flag is defined or switched on
\G while compiling we do not return the current value but
  parse-name debug? ;

: [d?]
\G compile the value-xt so the debug flag can be switched
\G the flag must exist!
  parse-name debugflags-wl search-wordlist
  IF 	compile,
  ELSE  -1 ABORT" unknown debug flag"
	\ POSTPONE false 
  THEN ; immediate

: symentry ( adr len taddr -- )
\G Produce a symbol table (an optional symbol address
\G map) if wanted
    [ [IFDEF] fd-symbol-table ]
      base @ swap hex s>d <# 8 0 DO # LOOP #> fd-symbol-table write-file throw base !
      s" :" fd-symbol-table write-file throw
      fd-symbol-table write-line throw
    [ [ELSE] ]
      2drop drop
    [ [THEN] ] ;


\ \ --------------------	source file

decimal

Variable cross-file-list
0 cross-file-list !
Variable target-file-list
0 target-file-list !
Variable host-file-list
0 host-file-list !

cross-file-list Value file-list
0 Value source-desc

\ file loading

: >fl-id   1 cells + ;
: >fl-name 2 cells + ;

Variable filelist 0 filelist !
Create NoFile ," #load-file#"

: loadfile ( -- adr )
  source-desc ?dup IF >fl-name ELSE NoFile THEN ;

: sourcefilename ( -- adr len ) 
  loadfile count ;

\ANSI : sourceline# 0 ;

\ \ --------------------	path handling from kernel/paths.fs

\ paths.fs path file handling                                    03may97jaw

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
\ if there is not enough space for the search path increase it!


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

\ if we have path handling, use this and the setup of it
[IFUNDEF] open-fpath-file

create sourcepath 1024 chars , 0 , 1024 chars allot \ !! make this dynamic
sourcepath value fpath

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
    parse-name rot also-path ;

: fpath+ ( "dir" ) \ gforth
    \G Add directory @var{dir} to the Forth search path.
    fpath path+ ;

: path= ( path-addr "dir1|dir2|dir3" ) \ gforth
    \G Make a complete new search path; the path separator is |.
    parse-name 2dup bounds ?DO i c@ '|' = IF 0 i c! THEN LOOP
    rot only-path ;

: fpath= ( "dir1|dir2|dir3" ) \ gforth
    \G Make a complete new Forth search path; the path separator is |.
    fpath path= ;

: path>string  cell+ dup cell+ swap @ ;

: next-path ( adr len -- adr2 len2 )
  2dup 0 scan
  dup 0= IF     2drop 0 -rot 0 -rot EXIT THEN
  >r 1+ -rot r@ 1- -rot
  r> - ;

: previous-path ( path^ -- )
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
    2dup 2 u> swap 1+ c@ ':' = and >r \ dos absoulte: c:/....
    over c@ '/' = >r
    over c@ '~' = >r
    \ 2dup S" ../" string-prefix? r> or >r \ not catered for in expandtopic
    S" ./" string-prefix?
    r> r> r> or or or ;

Create ofile 0 c, 255 chars allot
Create tfile 0 c, 255 chars allot

: pathsep? dup '/' = swap '\' = or ;

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
	0 ofile c! sourcefilename extractpath ofile place
	ofile c@ IF need/ THEN
	tfile count over c@ pathsep? IF 1 /string THEN
	ofile +place
    THEN ;

: compact.. ( adr len -- adr2 len2 )
    \ deletes phrases like "xy/.." out of our directory name 2dec97jaw
    over swap
    BEGIN  dup  WHILE
        dup >r '/ scan 2dup s" /../" string-prefix?
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
  ELSE  r> path>string
        BEGIN  next-path dup
        WHILE  5 pick 5 pick check-path
        0= IF >r 2drop 2drop r> ofile count 0 EXIT ELSE drop THEN
  REPEAT
        2drop 2drop 2drop -38
  THEN ;

: open-fpath-file ( addr1 u1 -- wfileid addr2 u2 0 | ior ) \ gforth
    \G Look in the Forth search path for the file specified by @var{addr1 u1}.
    \G If found, the resulting path and an open file descriptor
    \G are returned. If the file is not found, @var{ior} is non-zero.
    fpath open-path-file ;

fpath= ~+

[THEN]

\ \ --------------------	include require			13may99jaw

[IFDEF] add-included-file
    ' add-included-file alias h-add-included-file
[THEN]

>CROSS

: add-included-file ( adr len -- adr )
  dup >fl-name char+ allocate throw >r
  file-list @ r@ ! r@ file-list !
  r@ >fl-name place r> ;

: included? ( c-addr u -- f )
  file-list
  BEGIN	@ dup
  WHILE	>r 2dup r@ >fl-name count str=
	IF rdrop 2drop true EXIT THEN
	r>
  REPEAT
  2drop drop false ;	

false DebugFlag showincludedfiles

: included1 ( fd adr u -- )
\ include file adr u / fd
\ we don't use fd with include-file, because the forth system
\ doesn't know the name of the file to get a nice error report
  [d?] showincludedfiles
  IF	cr ." Including: " 2dup type ." ..." THEN
  rot close-file throw
  source-desc >r
  add-included-file to source-desc
  sourcefilename ['] included catch
  r> to source-desc 
  throw ;

: included ( adr len -- )
	cross-file-list to file-list
	open-fpath-file throw 
        included1 ;

: required ( adr len -- )
	cross-file-list to file-list
	open-fpath-file throw \ 2dup cr ." R:" type
	2dup included?
	IF 	2drop close-file throw
	ELSE	included1
	THEN ;

: include parse-name included ;

: require parse-name required ;

0 [IF]

also forth definitions previous

: included ( adr len -- ) included ;

: required ( adr len -- ) required ;

: include include ;

: require require ;

[THEN]

>CROSS
hex

\ \ --------------------        Error Handling                  05aug97jaw

\ Flags

also forth definitions  \ these values may be predefined before
                        \ the cross-compiler is loaded

false DefaultValue stack-warn   	 \ check on empty stack at any definition
false DefaultValue create-forward-warn   \ warn on forward declaration of created words

previous >CROSS

: .sourcepos
  cr sourcefilename type ." :"
  sourceline# dec. ;

: warnhead
\G display error-message head
\G perhaps with linenumber and filename
  .sourcepos ." Warning: " ;

: empty? depth IF .sourcepos ." Stack not empty!"  THEN ;

stack-warn [IF]
: defempty? empty? ;
[ELSE]
: defempty? ; immediate
\ : defempty? .sourcepos ; 
[THEN]

\ \ --------------------        Compiler Plug Ins               01aug97jaw

>CROSS

\ Compiler States

Variable comp-state
0 Constant interpreting
1 Constant compiling
2 Constant resolving
3 Constant assembling

: compiling? comp-state @ compiling = ;

: pi-undefined -1 ABORT" Plugin undefined" ;

[IFDEF] value! ' value! alias value-to [THEN]

: Plugin ( -- : pluginname )
  Create 
  \ for normal cross-compiling only one action
  \ exists, this fields are identical. For the instant
  \ simulation environment we need, two actions for each plugin
  \ the target one and the one that generates the simulation code
  ['] pi-undefined , \ action
  ['] pi-undefined , \ target plugin action
  8765 ,     \ plugin magic
[IFDEF] set-to
  ['] defer-is set-to
[THEN]
  DOES> perform ;

Plugin DummyPlugin

: 'PI ( -- addr : pluginname )
  ' >body dup 2 cells + @ 8765 <> ABORT" not a plugin" ;

: plugin-of ( xt -- : pluginname )
  dup 'PI 2! ;

: action-of ( xt -- : plunginname )
  'PI cell+ ! ;

: TPA ( -- : plugin )
\ target plugin action
\ executes current target action of plugin
  'PI cell+ POSTPONE literal POSTPONE perform ; immediate

Variable ppi-temp 0 ppi-temp !

: pa:
\g define plugin action
  ppi-temp @ ABORT" pa: definition not closed"
  'PI ppi-temp ! :noname ;

: ;pa
\g end a definition for plugin action
  POSTPONE ; ppi-temp @ ! 0 ppi-temp ! ; immediate


Plugin dlit, ( d -- )			\ compile numerical value the target
Plugin lit, ( n -- )
Plugin alit, ( n -- )

Plugin branch, ( target-addr -- )	\ compiles a branch
Plugin ?branch, ( target-addr -- )	\ compiles a ?branch
Plugin branchmark, ( -- branch-addr )	\ reserves room for a branch
Plugin ?branchmark, ( -- branch-addr )	\ reserves room for a ?branch
Plugin ?dup-?branchmark, ( -- branch-addr )	\ reserves room for a ?branch
Plugin ?domark, ( -- branch-addr )	\ reserves room for a ?do branch
Plugin branchto, ( -- )			\ actual program position is target of a branch (do e.g. alignment)
' NOOP plugin-of branchto, 
Plugin branchtoresolve, ( branch-addr -- ) \ resolves a forward reference from branchmark
Plugin branchtomark, ( -- target-addr )	\ marks a branch destination

Plugin colon, ( tcfa -- )		\ compiles call to tcfa at current position
Plugin prim, ( tcfa -- )		\ compiles primitive invocation
Plugin colonmark, ( -- addr )		\ marks a colon call
Plugin colon-resolve ( tcfa addr -- )

Plugin addr-resolve ( target-addr addr -- )
Plugin doer-resolve ( ghost res-pnt target-addr addr -- ghost res-pnt )

Plugin ncontrols? ( [ xn ... x1 ] n -- ) \ checks wheter n control structures are open
Plugin if, 	( -- if-token )
Plugin ?dup-if, 	( -- if-token )
Plugin else,	( if-token -- if-token )
Plugin then,	( if-token -- )
Plugin ahead,
Plugin begin,
Plugin while,
Plugin until,
Plugin again,
Plugin repeat,
Plugin cs-swap	( x1 x2 -- x2 x1 )

Plugin case,	( -- n )
Plugin of,	( n -- x1 n )
Plugin endof,	( x1 n -- x2 n )
Plugin endcase,	( x1 .. xn n -- )

Plugin do,	( -- do-token )
Plugin ?do,	( -- ?do-token )
Plugin +do,	( -- ?do-token )
Plugin -do,	( -- ?do-token )
Plugin u-do,	( -- ?do-token )
Plugin for,	( -- for-token )
Plugin loop,	( do-token / ?do-token -- )
Plugin +loop,	( do-token / ?do-token -- )
Plugin -loop,	( do-token / ?do-token -- )
Plugin next,	( for-token )
Plugin leave,	( -- )
Plugin ?leave, 	( -- )

Plugin ca>native  \ Convert a code address to the processors
                  \ native address. This is used in doprim, and
                  \ code/code: primitive definitions word to
                  \ convert the addresses.
                  \ The only target where we need this is the misc
                  \ which is a 16 Bit processor with word addresses
                  \ but the forth system we build has a normal byte
                  \ addressed memory model    

Plugin doprim,	\ compiles start of a primitive
Plugin docol,   	\ compiles start of a colon definition
Plugin doer,		
Plugin fini,      \ compiles end of definition ;s
Plugin doeshandler,
Plugin dodoes,

Plugin colon-start
' noop plugin-of colon-start
Plugin colon-end
' noop plugin-of colon-end

Plugin ]comp     \ starts compilation
' noop plugin-of ]comp
Plugin comp[     \ ends compilation
' noop plugin-of comp[

Plugin t>body             \ we need the system >body
			\ and the target >body

>TARGET
: >body t>body ;


\ Ghost Builder                                        06oct92py

>CROSS
hex
\ Values for ghost magic
4711 Constant <fwd>             4712 Constant <res>
4713 Constant <imm>             4714 Constant <do:>
4715 Constant <skip>

\ Bitmask for ghost flags
1 Constant <unique>
2 Constant <primitive>

\ FXIME: move this to general stuff?
: set-flag ( addr flag -- )
  over @ or swap ! ;

: reset-flag ( addr flag -- )
  invert over @ and swap ! ;

: get-flag ( addr flag -- f )
  swap @ and 0<> ;
  

Struct

  \ link to next ghost (always the first element)
  cell% field >next-ghost

  \ type of ghost
  cell% field >magic
		
  \ pointer where ghost is in target, or if unresolved
  \ points to the where we have to resolve (linked-list)
  cell% field >link

  \ execution semantics (while target compiling) of ghost
  cell% field >exec

  \ compilation action of this ghost; this is what is
  \ done to compile a call (or whatever) to this definition.
  \ E.g. >comp contains the semantic of postpone s"
  \ whereas >exec-compile contains the semantic of s"
  cell% field >comp

  \ Compilation semantics (while parsing) of this ghost. E.g. 
  \ "\" will skip the rest of line.
  \ These semantics are defined by Cond: and
  \ if a word is made immediate in instant, then the >exec2 field
  \ gets copied to here
  cell% field >exec-compile

  \ Additional execution semantics of this ghost. This is used
  \ for code generated by instant and for the doer-xt of created
  \ words
  cell% field >exec2

  cell% field >created

  \ the xt of the created ghost word itself
  cell% field >ghost-xt

  \ pointer to the counted string of the assiciated
  \ assembler label
  cell% field >asm-name

  \ mapped primitives have a special address, so
  \ we are able to detect them
  cell% field >asm-dummyaddr
			
  \ for builder (create, variable...) words
  \ the execution symantics of words built are placed here
  \ this is a doer ghost or a dummy ghost
  cell% field >do:ghost

  cell% field >ghost-flags

  cell% field >ghost-name

\  cell% field >ghost-hm

End-Struct ghost-struct

Variable ghost-list
0 ghost-list !

Variable executed-ghost \ last executed ghost, needed in tcreate and gdoes>
\ Variable last-ghost	\ last ghost that is created
Variable last-header-ghost \ last ghost definitions with header

\ space for ghosts resolve structure
\ we create ghosts in a separate space
\ and not to the current host dp, because this
\ gives trouble with instant while compiling and creating
\ a ghost for a forward reference
\ BTW: we cannot allocate another memory region
\ because allot will check the overflow!!
Variable cross-space-dp
Create cross-space 250000 allot here 100 allot align 
Constant cross-space-end
cross-space cross-space-dp !
Variable cross-space-dp-orig

: cross-space-used cross-space-dp @ cross-space - ;

: >space ( -- )
  dp @ cross-space-dp-orig !
  cross-space-dp @ dp ! ;

: space> ( -- )
  dp @ dup cross-space-dp !
  cross-space-end u> ABORT" CROSS: cross-space overflow"
  cross-space-dp-orig @ dp ! ;

\ this is just for debugging, to see this in the backtrace
: execute-exec execute ;
: execute-exec2 execute ;
: execute-exec-compile execute ;

: NoExec
  executed-ghost @ >exec2 @
  ?dup 
  IF   execute-exec2
  ELSE true ABORT" CROSS: Don't execute ghost, or immediate target word"
  THEN ;

Defer is-forward

: (ghostheader) ( -- )
    ghost-list linked <fwd> , 0 , ['] NoExec , ['] is-forward ,
    0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , ;

: ghostheader ( -- ) (ghostheader) 0 , ;

' Ghosts >wordlist Constant ghosts-wordlist

\ the current wordlist for ghost definitions in the host
ghosts-wordlist Value current-ghosts

: Make-Ghost ( "name" -- ghost )
  >space
  \ save current and create in ghost vocabulary
  get-current >r current-ghosts set-current
  >in @ Create >in !
  \ some forth systems like iForth need the immediate directly
  \ after the word is created
  \ restore current
  r> set-current
  here (ghostheader)
  parse-name string, align
  space>
  \ set ghost-xt field by doing a search
  dup >ghost-name count 
  current-ghosts search-wordlist
  0= ABORT" CROSS: Just created, must be there!"
  over >ghost-xt !
  DOES> 
      dup executed-ghost !
      >exec @ execute-exec ;

\ ghost words                                          14oct92py
\                                          changed:    10may93py/jaw

Defer search-ghosts

: (search-ghosts) ( adr len -- cfa true | 0 )
  current-ghosts search-wordlist ; 

  ' (search-ghosts) IS search-ghosts

: gsearch ( addr len -- ghost true | 0 )
  search-ghosts
  dup IF swap >body swap THEN ;

Variable cross-wheres

0
cell +field gwhere-nt
cell +field gwhere-loc
constant gwhere-struct

: tsourceview ( -- view )
    [IFDEF] loadfilename# loadfilename# @
    [ELSE] sourcefilename str>loadfilename# [THEN]
    sourceline#
    input-lexeme 2@ drop source drop -
    $ff min swap 8 lshift + $7fffff min swap #23 lshift or ;

: gwhere-duplicate? ( ghost -- flag )
    cross-wheres $@ dup if
	gwhere-struct - + >r
	dup r@ gwhere-nt @ =
	r> gwhere-loc @ tsourceview = and if
	    drop true exit then
    else
	2drop then
    drop false ;

[IFUNDEF] $+!len
    : $+!len ( n $addr -- addr )
	dup >r $@len tuck + r@ $!len r> @ cell+ + ;
[THEN]

: gwhere, ( ghost -- )
    dup gwhere-duplicate? 0= IF
	gwhere-struct cross-wheres $+!len >r
	dup r@ gwhere-nt !
	tsourceview r> gwhere-loc !
    THEN  drop ;

\ locations

0 Value glocs-start
Variable cross-locs[]

[IFUNDEF] $[]
: $room ( u $addr -- )
    \G generate room for at least u bytes, erase when expanding
    >r dup r@ $@len tuck u<= IF  rdrop 2drop EXIT  THEN
    - dup r> $+!len swap 0 fill ;

: $[] ( u $[]addr -- addr' )
    \G index into the string array and return the address at index @var{u}
    \G The array will be resized as needed
    >r cells dup cell+ r@ $room  r> $@ drop + ;
[THEN]

: tsourcepos1 ( -- xpos )
    [IFDEF] replace-sourceview
	replace-sourceview  0 to replace-sourceview ?dup ?EXIT
    [THEN]
    tsourceview ;
: gxt-location ( addr -- addr )
\ note that an xt was compiled at addr, for backtrace-locate functionality
    dup glocs-start - T 1 cells H / >r
    tsourcepos1 dup r> 1+ cross-locs[] $[] 1 cells - 2! ;

\ search for ghosts

: gfind   ( string -- ghost true / string false )
\ searches for string in word-list ghosts
  \ dup count type space
  dup >r count gsearch
  dup IF rdrop over gwhere, ELSE r> swap THEN ;

: gdiscover ( xt -- ghost true | xt false )
  >r ghost-list
  BEGIN @ dup
  WHILE dup >magic @ <fwd> <>
        IF dup >link @ r@ =
           IF rdrop true EXIT THEN
        THEN
  REPEAT
  drop r> false ;

: xt>ghost ( xt -- ghost )
  gdiscover 0= ABORT" CROSS: ghost not found for this xt" ;

: Ghost   ( "name" -- ghost )
  >in @ bl-word gfind IF  nip EXIT  THEN
  drop  >in !  Make-Ghost ;

: >ghostname ( ghost -- adr len )
  >ghost-name count ;

: forward? ( ghost -- flag )
  >magic @ <fwd> = ;

: undefined? ( ghost -- flag )
  >magic @ dup <fwd> = swap <skip> = or ;

: immediate? ( ghost -- flag )
  >magic @ <imm> = ;

Variable TWarnings
TWarnings on
Variable Exists-Warnings
Exists-Warnings on

: exists-warning ( ghost -- ghost )
  TWarnings @ Exists-Warnings @ and
  IF dup >ghostname warnhead type ."  exists " THEN ;

\ : HeaderGhost Ghost ;

Variable reuse-ghosts reuse-ghosts off

: HeaderGhost ( "name" -- ghost )
  >in @ 
  parse-name 
\  2dup type space
  current-ghosts search-wordlist
  IF  >body dup undefined? reuse-ghosts @ or
      IF   nip EXIT
      ELSE exists-warning 
      THEN
      drop >in ! 
  ELSE >in ! 
  THEN 
  \ we keep the execution semantics of the previously
  \ defined words, this is a workaround
  \ for the redefined \ until vocs work
  Make-Ghost ;
 
: .ghost ( ghost -- ) >ghostname type ;

\ ' >ghostname ALIAS @name

: findghost ( "ghostname" -- ghost ) 
  bl-word gfind 0= ABORT" CROSS: Ghost don't exists" ;

: [G'] ( -- ghost : name )
\G ticks a ghost and returns its address
  findghost
  state @ IF postpone literal THEN ; immediate

: g>xt ( ghost -- xt )
\G Returns the xt (cfa) of a ghost. Issues a warning if undefined.
  dup undefined? ABORT" CROSS: forward " >link @ ;
   
: g>body ( ghost -- body )
\G Returns the body-address (pfa) of a ghost. 
\G Issues a warning if undefined (a forward-reference).
  g>xt X >body ;

1 Constant <label>

Struct
  \ bitmask of address type (not used for now)
  cell% field addr-type
  \ if this address is an xt, this field points to the ghost
  cell% field addr-xt-ghost
  \ a bit mask that tells as what part of the cell
  \ is refenced from an address pointer, used for assembler generation
  cell% field addr-refs
End-Struct addr-struct

: %allocerase ( align size -- addr )
  dup >r %alloc dup r> erase ;

\ returns the addr struct, define it if 0 reference
: define-addr-struct ( addr -- struct-addr )
  dup @ ?dup IF nip EXIT THEN
  addr-struct %allocerase tuck swap ! ;

>cross

\ Predefined ghosts                                    12dec92py

Ghost - drop \ need a ghost otherwise "-" would be treated as a number

Ghost 0=                                        drop
Ghost branch    Ghost ?branch                   2drop
Ghost ?dup-?branch drop
Ghost unloop    Ghost ;S                        2drop
Ghost lit       Ghost !                         2drop
Ghost noop                                      drop
Ghost over      Ghost =         Ghost drop      2drop drop
Ghost 2drop drop
Ghost 2dup drop
Ghost call drop
Ghost @ drop
Ghost up@ drop
Ghost execute drop
Ghost + drop
Ghost decimal drop
Ghost hex drop
Ghost lit@ drop
Ghost lit-perform drop
Ghost lit+ drop
Ghost does-xt drop
Ghost n/a drop
Ghost refill drop

Ghost :docol    drop
Ghost :dodoes   drop
Ghost :dodoes1   drop
Ghost :dodoes2   drop
Ghost :dodoes3   drop
Ghost :dodoes4   drop
Ghost :dodoes5   drop
Ghost :dodoes6   drop
Ghost :dodoes7   drop
Ghost :dodoes8   drop
Ghost :dodoes9   drop
Ghost :dovar	drop

\ \ Parameter for target systems                         06oct92py


\ we define it ans like...
wordlist Constant target-environment

\ save information of current dictionary to restore with environ>
Variable env-current 

: >ENVIRON get-current env-current ! target-environment set-current ;
: ENVIRON> env-current @ set-current ; 

>TARGET

: environment? ( addr len -- [ x ] true | false )
\G returns the content of environment variable and true or
\G false if not present
   target-environment search-wordlist 
   IF EXECUTE true ELSE false THEN ;

: $has? ( addr len -- x | false )
\G returns the content of environment variable 
\G or false if not present
   T environment? H dup IF drop THEN ;

: e? ( "name" -- x )
\G returns the content of environment variable. 
\G The variable is expected to exist. If not, issue an error.
   parse-name T environment? H 
   0= ABORT" environment variable not defined!" ;

: has? ( "name" --- x | false )
\G returns the content of environment variable 
\G or false if not present
   parse-name T $has? H ;


>ENVIRON get-order get-current swap 1+ set-order
true SetValue compiler
true SetValue cross
true SetValue standard-threading
>TARGET previous

0
[IFDEF] mach-file drop mach-file count 1 [THEN]
[IFDEF] machine-file drop machine-file 1 [THEN]
[IF] 	included hex
[ELSE]  cr ." No machine description!" ABORT 
[THEN]

>ENVIRON

T has? ec H
[IF]
false DefaultValue relocate
false DefaultValue file
false DefaultValue OS
false DefaultValue prims
false DefaultValue floating
false DefaultValue glocals
false DefaultValue dcomps
false DefaultValue hash
false DefaultValue xconds
false DefaultValue header
false DefaultValue backtrace
false DefaultValue new-input
false DefaultValue peephole
false DefaultValue primcentric
false DefaultValue abranch
true DefaultValue f83headerstring
true DefaultValue control-rack
[THEN]

true DefaultValue gforthcross
true DefaultValue interpreter
true DefaultValue ITC
false DefaultValue rom
false DefaultValue flash
true DefaultValue standardthreading

\ ANSForth environment  stuff
8 DefaultValue ADDRESS-UNIT-BITS
255 DefaultValue MAX-CHAR
255 DefaultValue /COUNTED-STRING

>TARGET
s" relocate" T environment? H 
\ JAW why set NIL to this?!
[IF]	drop \ SetValue NIL
[ELSE]  >ENVIRON X NIL SetValue relocate
[THEN]
>CROSS

\ \ Create additional parameters                         19jan95py

\ currently cross only works for host machines with address-unit-bits
\ equal to 8 because of s! and sc!
\ but I start to query the environment just to modularize a little bit

: check-address-unit-bits ( -- )	
\	s" ADDRESS-UNIT-BITS" environment?
\	IF 8 <> ELSE true THEN
\	ABORT" ADDRESS-UNIT-BITS unknown or not equal to 8!"

\	shit, this doesn't work because environment? is only defined for 
\	gforth.fi and not kernl???.fi
	;

check-address-unit-bits
8 Constant bits/byte	\ we define: byte is address-unit

1 bits/byte lshift Constant maxbyte 
\ this sets byte size for the target machine, (probably right guess) jaw

T
0		   	Constant TNIL
cell               	Constant tcell
cell<<             	Constant tcell<<
cell>bit           	Constant tcell>bit
bits/char          	Constant tbits/char
bits/char H bits/byte T /      
			Constant tchar
float             	Constant tfloat
1 bits/char lshift 	Constant tmaxchar
[IFUNDEF] bits/byte
8			Constant tbits/byte
[ELSE]
bits/byte		Constant tbits/byte
[THEN]
H
tbits/char bits/byte /	Constant tbyte

: >signed ( u -- n )
    1 tbits/char tcell * 1- lshift 2dup and
    IF  negate or  ELSE  drop  THEN ;

\ Variables                                            06oct92py

Variable (tlast)    
(tlast) Value tlast TNIL tlast !  \ Last name field
Variable tlastcfa \ Last code field

\ statistics						10jun97jaw

Variable headers-named 0 headers-named !
Variable user-vars 0 user-vars !

: target>bitmask-size ( u1 -- u2 )
  1- tcell>bit rshift 1+ ;

: allocatetarget ( size -- adr )
  dup allocate ABORT" CROSS: No memory for target"
  swap over swap erase ;

\ \ memregion.fs


Variable last-defined-region    \ pointer to last defined region
Variable region-link            \ linked list with all regions
Variable mirrored-link          \ linked list for mirrored regions
0 dup mirrored-link ! region-link !

: >rname 9 cells + ;
: >rtouch 8 cells + ; \ executed when region is accessed
: >rbm   4 cells + ; \ bitfield per cell witch indicates relocation
: >rmem  5 cells + ;
: >rtype 6 cells + ; \ field per cell witch points to a type struct
: >rrom 7 cells + ;  \ a -1 indicates that this region is rom
: >rlink 3 cells + ;
: >rdp 2 cells + ;
: >rlen cell+ ;
: >rstart ;

: (region) ( addr len region -- )
\G change startaddress and length of an existing region
  dup >r last-defined-region !
  r@ >rlen ! dup r@ >rstart ! r> >rdp ! ;

: uninitialized -1 ABORT" CROSS: Region is uninitialized" ;

: region ( addr len -- "name" )                
\G create a new region
  \ check whether predefined region exists 
  save-input bl-word find >r >r restore-input throw r> r> 0= 
  IF	\ make region
	drop
	save-input create restore-input throw
	here last-defined-region !
	over ( startaddr ) , ( length ) , ( dp ) ,
	region-link linked 0 , 0 , 0 , 0 , 
        ['] uninitialized ,
        parse-name string,
  ELSE	\ store new parameters in region
        bl-word drop
	>body (region)
  THEN ;

: borders ( region -- startaddr endaddr ) 
\G returns lower and upper region border
  dup >rstart @ swap >rlen @ over + ;

: extent  ( region -- startaddr len )   
\G returns the really used area
  dup >rstart @ swap >rdp @ over - ;

: area ( region -- startaddr totallen ) 
\G returns the total area
  dup >rstart @ swap >rlen @ ;

: dp@ ( region -- dp )
  >rdp @ ;

: mirrored ( -- )                              
\G mark last defined region as mirrored
  mirrored-link
  align linked last-defined-region @ , ;

: writeprotected
\G mark a region as write protected
  -1 last-defined-region @ >rrom ! ;

: .addr ( u -- )
\G prints a 16 or 32 Bit nice hex value
  base @ >r hex
  tcell 2 u>
  IF s>d <# # # # # '.' hold # # # # #> type
  ELSE s>d <# # # # # # #> type
  THEN r> base ! space ;

: .regions                      \G display region statistic

  \ we want to list the regions in the right order
  \ so first collect all regions on stack
  0 region-link @
  BEGIN dup WHILE dup @ REPEAT drop
  BEGIN dup
  WHILE cr
        0 >rlink - dup
        >r >rname count tuck type
        12 swap - spaces space
        ." Start: " r@ >rstart @ dup .addr space
        ." End: " r@ >rlen @ + .addr space
        ." DP: " r> >rdp @ .addr
  REPEAT drop
  s" rom" T $has? H 0= ?EXIT
  cr ." Mirrored:"
  mirrored-link @
  BEGIN dup
  WHILE space dup cell+ @ >rname count type @
  REPEAT drop cr
  ;

\ -------- predefined regions

0 0 region address-space
\ total memory addressed and used by the target system

0 0 region user-region
\ data for user variables goes here
\ this has to be defined before dictionary or ram-dictionary

0 0 region dictionary
\ rom area for the compiler

T has? rom H
[IF]
0 0 region ram-dictionary mirrored
\ ram area for the compiler
[ELSE]
' dictionary ALIAS ram-dictionary
[THEN]

0 0 region return-stack

0 0 region data-stack

0 0 region tib-region

' dictionary ALIAS rom-dictionary

: setup-region ( region -- )
  >r
  \ allocate mem
  r@ >rlen @ allocatetarget
  r@ >rmem !

  r@ >rlen @
  target>bitmask-size allocatetarget
  r@ >rbm !

  r@ >rlen @
  tcell / 1+ cells allocatetarget r@ >rtype !

  ['] noop r@ >rtouch !
  rdrop ;

: setup-target ( -- )   \G initialize target's memory space
  s" rom" T $has? H
  IF  \ check for ram and rom...
      \ address-space area nip 0<>
      ram-dictionary area nip 0<>
      rom-dictionary area nip 0<>
      and 0=
      ABORT" CROSS: define address-space, rom- , ram-dictionary, with rom-support!"
  THEN
  address-space area nip
  IF
      address-space area
  ELSE
      dictionary area
  THEN
  nip 0=
  ABORT" CROSS: define at least address-space or dictionary!!"

  \ allocate target for each region
  region-link
  BEGIN @ dup
  WHILE dup
        0 >rlink - dup
        >r >rlen @
        IF      r@ setup-region
        THEN    rdrop
   REPEAT drop ;

\ MakeKernel                                           		22feb99jaw

: makekernel ( start targetsize -- )
\G convenience word to setup the memory of the target
\G used by main.fs of the c-engine based systems
  dictionary (region) setup-target ;

>MINIMAL
: makekernel makekernel ;
>CROSS

\ \ switched tdp for rom support				03jun97jaw

\ second value is here to store some maximal value for statistics
\ tempdp is also embedded here but has nothing to do with rom support
\ (needs switched dp)

variable tempdp	0 ,	\ temporary dp for resolving
variable tempdp-save

0 [IF]
variable romdp 0 ,      \ Dictionary-Pointer for ramarea
variable ramdp 0 ,      \ Dictionary-Pointer for romarea

\
variable sramdp		\ start of ram-area for forth
variable sromdp		\ start of rom-area for forth

[THEN]

0 Value current-region
0 Value tdp
Variable fixed		\ flag: true: no automatic switching
			\	false: switching is done automatically

\ Switch-Policy:
\
\ a header is always compiled into rom
\ after a created word (create and variable) compilation goes to ram
\
\ Be careful: If you want to make the data behind create into rom
\ you have to put >rom before create!

variable constflag constflag off

: activate ( region -- )
\G next code goes to this region
  dup to current-region >rdp to tdp ;

: (switchram)
  fixed @ ?EXIT s" rom" T $has? H 0= ?EXIT
  ram-dictionary activate ;

: switchram
  constflag @
  IF constflag off ELSE (switchram) THEN ;

: switchrom
  fixed @ ?EXIT rom-dictionary activate ;

: >tempdp ( addr -- ) 
  tdp tempdp-save ! tempdp to tdp tdp ! ;
: tempdp> ( -- )
  tempdp-save @ to tdp ;

: >ram  fixed off (switchram) fixed on ;
: >rom  fixed off switchrom fixed on ;
: >auto fixed off switchrom ;



\ : romstart dup sromdp ! romdp ! ;
\ : ramstart dup sramdp ! ramdp ! ;

\ default compilation goes to rom
\ when romable support is off, only the rom switch is used (!!)
>auto

: there  tdp @ ;

>TARGET

\ \ Target Memory Handling

\ Byte ordering and cell size                          06oct92py

: cell+         tcell + ;
: cells         tcell<< lshift ;
: chars         tchar * ;
: char+		tchar + ;
: floats	tfloat * ;
    
>CROSS
: cell/         tcell<< rshift ;
>TARGET
20 CONSTANT bl
\ TNIL Constant NIL

>CROSS

bigendian
[IF]
    : DS!  ( d addr -- )
	tcell bounds swap 1-
	DO  maxbyte ud/mod rot I c!  -1 +LOOP  2drop ;
    : DS@  ( addr -- d )
	>r 0 0 r> tcell bounds
	DO  maxbyte * swap maxbyte um* under+ I c@ + swap  LOOP ;
    : Sc!  ( n addr -- )
	>r s>d r> tchar bounds swap 1-
	DO  maxbyte ud/mod rot I c!  -1 +LOOP  2drop ;
    : Sc@  ( addr -- n )
	>r 0 0 r> tchar bounds
	DO  maxbyte * swap maxbyte um* under+ I c@ + swap  LOOP d>s ;
[ELSE]
    : DS!  ( d addr -- )
	tcell bounds
	DO  maxbyte ud/mod rot I c!  LOOP  2drop ;
    : DS@  ( addr -- n )
	>r 0 0 r> tcell bounds swap 1-
	DO  maxbyte * swap maxbyte um* under+ I c@ + swap  -1 +LOOP ;
    : Sc!  ( n addr -- )
	>r s>d r> tchar bounds
	DO  maxbyte ud/mod rot I c!  LOOP  2drop ;
    : Sc@  ( addr -- n )
	>r 0 0 r> tchar bounds swap 1-
	DO  maxbyte * swap maxbyte um* under+ I c@ + swap  -1 +LOOP d>s ;
[THEN]

: S! ( n addr -- ) >r s>d r> DS! ;
: S@ ( addr -- n ) DS@ d>s ;

: taddr>region ( taddr -- region | 0 )
\G finds for a target-address the correct region
\G returns 0 if taddr is not in range of a target memory region
  region-link
  BEGIN @ dup
  WHILE dup >r
        0 >rlink - dup
        >r >rlen @
        IF      dup r@ borders within
                IF r> r> drop nip 
                   dup >rtouch @ EXECUTE EXIT 
                THEN
        THEN
        r> drop
        r>
  REPEAT
  2drop 0 ;

: taddr>region-abort ( taddr -- region | 0 )
\G Same as taddr>region but aborts if taddr is not
\G a valid address in the target address space
  dup taddr>region dup 0= 
  IF    drop cr ." Wrong address: " .addr
        -1 ABORT" Address out of range!"
  THEN nip ;

: (>regionimage) ( taddr -- 'taddr )
  dup
  \ find region we want to address
  taddr>region-abort
  >r
  \ calculate offset in region
  r@ >rstart @ -
  \ add regions real address in our memory
  r> >rmem @ + ;

: (>regionramimage) ( taddr -- 'taddr )
\G same as (>regionimage) but aborts if the region is rom
  dup
  \ find region we want to address
  taddr>region-abort
  dup
  >r >rrom @ ABORT" CROSS: region is write-protected!"
  \ calculate offset in region
  r@ >rstart @ -
  \ add regions real address in our memory
  r> >rmem @ + ;

: (>regionbm) ( taddr -- 'taddr bitmaskbaseaddr )
  dup
  \ find region we want to address
  taddr>region-abort
  >r
  \ calculate offset in region
  r@ >rstart @ -
  \ add regions real address in our memory
  r> >rbm @ ;

: (>regiontype) ( taddr -- 'taddr )
  dup
  \ find region we want to address
  taddr>region-abort
  >r
  \ calculate offset in region
  r@ >rstart @ - tcell / cells
  \ add regions real address in our memory
  r> >rtype @ + ;
  
\ Bit string manipulation                               06oct92py
\                                                       9may93jaw
CREATE Bittable 80 c, 40 c, 20 c, 10 c, 8 c, 4 c, 2 c, 1 c,
: bits ( n -- n ) chars Bittable + c@ ;

: >bit ( addr n -- c-addr mask ) 8 /mod under+ bits ;
: +bit ( addr n -- )  >bit over c@ or swap c! ;
: -bit ( addr n -- )  >bit invert over c@ and swap c! ;

: @relbit ( taddr -- f ) (>regionbm) swap cell/ >bit swap c@ and ;

: (relon) ( taddr -- )  
  [ [IFDEF] fd-relocation-table ]
  s" +" fd-relocation-table write-file throw
  dup s>d <# #s #> fd-relocation-table write-line throw
  [ [THEN] ]
  (>regionbm) swap cell/ +bit ;

: (reloff) ( taddr -- ) 
  [ [IFDEF] fd-relocation-table ]
  s" -" fd-relocation-table write-file throw
  dup s>d <# #s #> fd-relocation-table write-line throw
  [ [THEN] ]
  (>regionbm) swap cell/ -bit ;

DEFER >image
DEFER >ramimage
DEFER relon
DEFER reloff
DEFER correcter

T has? relocate H
[IF]
' (relon) IS relon
' (reloff) IS reloff
' (>regionimage) IS >image
' (>regionimage) IS >ramimage
[ELSE]
' drop IS relon
' drop IS reloff
' (>regionimage) IS >image
' (>regionimage) IS >ramimage
[THEN]

: enforce-writeprotection ( -- )
  ['] (>regionramimage) IS >ramimage ;

: relax-writeprotection ( -- )
  ['] (>regionimage) IS >ramimage ;

: writeprotection-relaxed? ( -- )
  ['] >ramimage >body @ ['] (>regionimage) = ;

\ Target memory access                                 06oct92py

: align+  ( taddr -- rest )
    tcell tuck 1- and - [ tcell 1- ] Literal and ;
: cfalign+  ( taddr -- rest )
    \ see kernel.fs:cfaligned
    /maxalign tuck 1- and - [ /maxalign 1- ] Literal and ;

>TARGET
: aligned ( taddr -- ta-addr )  dup align+ + ;
\ assumes cell alignment granularity (as GNU C)

: cfaligned ( taddr1 -- taddr2 )
    \ see kernel.fs
    dup cfalign+ + ;

: @  ( taddr -- w )     >image S@ ;
: d@  ( taddr -- d )    >image DS@ ;
: !  ( w taddr -- )     >ramimage S! ;
: d!  ( d taddr -- )    >ramimage DS! ;
: c@ ( taddr -- char )  >image Sc@ ;
: c! ( char taddr -- )  >ramimage Sc! ;
: 2@ ( taddr -- x1 x2 ) T dup cell+ @ swap @ H ;
: 2! ( x1 x2 taddr -- ) T tuck ! cell+ ! H ;

\ Target compilation primitives                        06oct92py
\ included A!                                          16may93jaw

: here  ( -- there )    there ;
: allot ( n -- )        tdp +! ;
: ,     ( w -- )        T here H tcell T allot  ! H ;
: d,    ( d -- )        T here H tcell T allot d! H ;
: c,    ( char -- )     T here H tchar T allot c! H ;
: >align ( n -- )       0 ?DO  bl T c, H tchar +LOOP ;
: align ( -- )          T here H align+ T >align H ;
: cfalign# ( n -- )
	T here H + cfalign+ 0 ?DO  bl T c, H  tchar +LOOP ;
: cfalign ( -- ) 0 T cfalign# H ;

: >address		dup 0>= IF tbyte / THEN ; \ ?? jaw 
: A!                    swap >address swap dup relon T ! H ;
: A,    ( w -- )        >address T here H relon T , H ;
: V,    ( w -- )        >address T here H reloff T , H ;
: dA,   ( d -- )        >address T here H relon T d, H ;
: dV,   ( d -- )        >address T here H reloff T d, H ;

\ high-level ghosts

>CROSS

Ghost (do)      Ghost (?do)                     2drop
Ghost (+do)     Ghost (-do)     Ghost (u-do)    2drop drop
Ghost (for)                                     drop
Ghost (loop)    Ghost (+loop)   Ghost (-loop)   2drop drop
Ghost (next)                                    drop
Ghost set-does> drop
Ghost compile,                                  drop
Ghost (.")      Ghost (S")      Ghost (ABORT")  2drop drop
Ghost (C")      Ghost c(abort") Ghost type      2drop drop
Ghost '         Ghost c(warning")               2drop

\ user ghosts

Ghost state drop

\ \ --------------------        Host/Target copy etc.     	29aug01jaw


>CROSS

: TD! >image DS! ;
: TD@ >image DS@ ;

: th-count ( taddr -- host-addr len )
\G returns host address of target string
  assert1( tbyte 1 = )
  dup X c@ swap X char+ >image swap ;

: ht-move ( haddr taddr len -- )
\G moves data from host-addr to destination in target-addr
\G character by character
  swap -rot bounds ?DO I c@ over X c! X char+ LOOP drop ;

2Variable last-string
X has? rom [IF] $60 [ELSE] $00 [THEN] Constant header-masks

: ht-header,  ( addr count -- )
  dup there swap last-string 2!
    dup header-masks or T c, H bounds  ?DO  I c@ T c, H  LOOP ;
: ht-string,  ( addr count -- )
  dup there swap last-string 2!
    dup T c, H bounds  ?DO  I c@ T c, H  LOOP ;
: ht-mem, ( addr count )
    bounds ?DO  I c@  T c, H  LOOP ;

>TARGET

: count dup X c@ swap X char+ swap ;

: on		>r -1 -1 r> TD!  ; 
: off   	T 0 swap ! H ;

: tcmove ( source dest len -- )
\G cmove in target memory
  tchar * bounds
  ?DO  dup T c@ H I T c! H 1+
  tchar +LOOP  drop ;

: tcallot ( char size -- )
    0 ?DO  dup T c, H  tchar +LOOP  drop ;

: td, ( d -- )
\G Store a host value as one cell into the target
  there tcell X allot TD! ;

\ \ Load Assembler

>TARGET
H also Forth definitions

\ FIXME: should we include the assembler really in the forth 
\ dictionary?!?!?!? This conflicts with the existing assembler 
\ of the host forth system!!
[IFDEF] asm-include asm-include [THEN] hex

previous


>CROSS

: (cc) T a, H ;					' (cc) plugin-of colon,
: (prim) T a, H ;				' (prim) plugin-of prim,

: (cr) >tempdp colon, tempdp> ;                 ' (cr) plugin-of colon-resolve
: (ar) T ! H ;					' (ar) plugin-of addr-resolve
: doer/does, ( ghost -- )
    dup >magic @ <do:> =
    IF  doer,  ELSE  dodoes,  THEN ;
: (dr)  ( ghost res-pnt target-addr addr )
	>tempdp drop over doer/does,
	tempdp> ;				' (dr) plugin-of doer-resolve

: (cm) ( -- addr )
    there -1 colon, ;				' (cm) plugin-of colonmark,

>TARGET
: compile, ( xt -- )
    T here H gxt-location drop
    dup xt>ghost >comp @ EXECUTE ;
>CROSS

\ resolve structure

: >next ;		\ link to next field
: >tag cell+ ;		\ indecates type of reference: 0: call, 1: address, 2: doer
: >taddr cell+ cell+ ;	
: >ghost 3 cells + ;
: >file 4 cells + ;
: >line 5 cells + ;

: (refered) ( ghost addr tag -- )
\G creates a reference to ghost at address taddr
    >space
    rot >link linked
    ( taddr tag ) ,
    ( taddr ) , 
    last-header-ghost @ , 
    loadfile , 
    sourceline# , 
    space>
;

: refered ( ghost tag -- )
\G creates a resolve structure
    T here aligned H swap (refered)
  ;

: killref ( addr ghost -- )
\G kills a forward reference to ghost at position addr
\G this is used to eleminate a :dovar refence after making a DOES>
    dup >magic @ <fwd> <> IF 2drop EXIT THEN
    swap >r >link
    BEGIN dup @ dup  ( addr last this )
    WHILE dup >taddr @ r@ =
 	 IF   @ over !
	 ELSE nip THEN
    REPEAT rdrop 2drop 
  ;

Defer resolve-warning

: reswarn-test ( ghost res-struct -- ghost res-struct )
  over cr ." Resolving " .ghost dup ."  in " >ghost @ .ghost ;

: reswarn-forward ( ghost res-struct -- ghost res-struct )
  over warnhead .ghost dup ."  is referenced in " 
  >ghost @ .ghost ;

\ ' reswarn-test IS resolve-warning
 
\ resolve                                              14oct92py

 : resolve-loop ( ghost resolve-list tcfa -- )
    >r
    BEGIN dup WHILE 
\  	  dup >tag @ 2 = IF reswarn-forward THEN
	  resolve-warning 
	  r@ over >taddr @ 
	  2 pick >tag @
	  CASE	0 OF colon-resolve ENDOF
		1 OF addr-resolve ENDOF
		2 OF doer-resolve ENDOF
	  ENDCASE
	  @ \ next list element
    REPEAT 2drop rdrop 
  ;

\ : resolve-loop ( ghost tcfa -- ghost tcfa )
\  >r dup >link @
\  BEGIN  dup  WHILE  dup T @ H r@ rot T ! H REPEAT  drop r> ;

\ exists                                                9may93jaw

: exists ( ghost tcfa -- )
\G print warning and set new target link in ghost
  swap exists-warning
  >link ! ;

: colon-resolved   ( ghost -- )
\ compiles a call to a colon definition,
\ compile action for >comp field
    >link @ colon, ; 

: prim-resolved  ( ghost -- )
\ compiles a call to a primitive
    >exec2 @ prim, ;

: (is-forward)   ( ghost -- )
    colonmark, 0 (refered) ; \ compile space for call
' (is-forward) IS is-forward

0 Value resolved

: resolve-forward-references ( ghost resolve-list -- )
    \ loop through forward referencies
    comp-state @ >r Resolving comp-state !
    over >link @ resolve-loop 
    r> comp-state !

    ['] noop IS resolve-warning ;


: (resolve) ( ghost tcfa -- ghost resolve-list )
    \ check for a valid address, it is a primitive reference
    \ otherwise
    dup taddr>region 0<> IF
      \ define this address in the region address type table
      2dup (>regiontype) define-addr-struct addr-xt-ghost 
      \ we define new address only if empty
      \ this is for not to take over the alias ghost
      \ (different ghost, but identical xt)
      \ but the very first that really defines it
      dup @ 0= IF ! ELSE 2drop THEN
    THEN
    swap dup
    >r to resolved

\    r@ >comp @ ['] is-forward =
\    ABORT" >comp action not set on a resolved ghost"

    \ copmile action defaults to colon-resolved
    \ if this is not right something must be set before
    \ calling resolve
    r@ >comp @ ['] is-forward = IF
       ['] colon-resolved r@ >comp !
   THEN
    r@ >link @ swap \ ( list tcfa R: ghost )
    \ mark ghost as resolved
    r@ >link ! <res> r@ >magic !
    r> swap ;

: resolve  ( ghost tcfa -- )
\G resolve references to ghost with tcfa
    \ is ghost resolved?, second resolve means another 
    \ definition with the same name
    over undefined? 0= IF  exists EXIT THEN
    (resolve)
    ( ghost resolve-list )
    resolve-forward-references ;

: resolve-noforwards ( ghost tcfa -- )
\G Same as resolve but complain if there are any
\G forward references on this ghost
   \ is ghost resolved?, second resolve means another 
   \ definition with the same name
   over undefined? 0= IF  exists EXIT THEN
   (resolve)
   IF cr ." No forward references allowed on: " .ghost cr
      -1 ABORT" Illegal forward reference"
   THEN
   drop ;

\ gexecute ghost,                                      01nov92py

: (gexecute)   ( ghost -- )
    T here H gxt-location drop
    dup >comp @ EXECUTE ;

: gexecute ( ghost -- )
  dup >magic @ <imm> = ABORT" CROSS: gexecute on immediate word"
  (gexecute) ;

: addr,  ( ghost -- )
    dup 0= IF  T a, H  EXIT  THEN
    dup forward? IF  1 refered 0 T a, H ELSE >link @ T a, H THEN ;

\ .unresolved                                          11may93jaw

variable ResolveFlag

\ ?touched                                             11may93jaw

: ?touched ( ghost -- flag ) dup forward? swap >link @ 0<> and ;

: .forwarddefs ( ghost -- )
  ."  appeared in:"
  >link
  BEGIN	@ dup
  WHILE	cr 5 spaces
	dup >ghost @ .ghost
	."  file " dup >file @ ?dup IF count type ELSE ." CON" THEN
	."  line " dup >line @ dec.
  REPEAT 
  drop ;

: ?resolved  ( ghost -- )
  dup ?touched
  IF  	ResolveFlag on 
	dup cr .ghost .forwarddefs
  ELSE 	drop 
  THEN ;

: .unresolved  ( -- )
  ResolveFlag off cr ." Unresolved: "
  ghost-list
  BEGIN @ dup
  WHILE dup ?resolved
  REPEAT drop ResolveFlag @
  IF
      -1 abort" Unresolved words!"
  ELSE
      ." Nothing!"
  THEN
  cr ;

: .stats
  base @ >r decimal
  cr ." named Headers: " headers-named @ . 
  r> base ! ;

>MINIMAL

: .unresolved .unresolved ;

>CROSS
\ Header states                                        12dec92py

\ new header fields

X has? f83headerstring bigendian or [IF] 0 [ELSE] tcell 1- [THEN]
Constant flag+

X has? f83headerstring [IF]
: t>f+c    tcell + ;
: t>flag   t>f+c flag+ + ; \ points to the flag byte
: t>link   ;
: cfa,     ( cfa -- ) T A, H ;
[ELSE]
X has? new-cfa [IF]
: t>f+c    tcell 4 * - ;
: t>flag   t>f+c flag+ + ; \ points to the flag byte
: t>link   tcell 3 * - ;
: t>cfa    tcell 2* - ;
: cfa,     ( cfa -- ) T here H t>cfa T A! H ;
[ELSE]
: t>f+c    tcell 3 * - ;
: t>flag   t>f+c flag+ + ; \ points to the flag byte
: t>link   tcell 2* - ;
: cfa,     ( cfa -- ) T A, H ;
[THEN]
[THEN]
: t>namehm tcell - ;

Struct
    tcell dup field >next-hm        \ linked list
    tcell dup field >compile,       \ smart compile,
    tcell dup field >extra-code     \ code for DOES> and other purposes
End-Struct name-hm

Create current-name-hm  name-hm %size allot

: flag! ( w -- )   tlast @ t>flag dup >r T c@ xor r> c! H ;

VARIABLE ^imm

\ !! should be target wordsize specific
$80 X has? f83headerstring [IF]
    dup Constant alias-mask 2/
    dup constant immediate-mask 2/
[THEN]
dup constant restrict-mask 2/
constant obsolete-mask

>TARGET
: restrict      restrict-mask flag! ;
: compile-only  restrict-mask flag! ;
: obsolete      obsolete-mask flag! ;
: >code-address t>cfa T @ H ;

: isdoer	
\G define a forth word as doer, this makes obviously only sense on
\G forth processors such as the PSC1000
		<do:> last-header-ghost @ >magic ! ;
>CROSS

\ Target Header Creation                               01nov92py

: ht-lstring, ( addr count -- )
  dup T , H bounds  ?DO  I c@ T c, H  LOOP ;

: ht-nlstring, ( addr count -- )
  tuck bounds  ?DO  I c@ T c, H  LOOP T , H ;

>TARGET
X has? f83headerstring [IF]
    : name,  ( addr u -- )  ht-header, X cfalign ;
[ELSE]
    : name,  ( addr u -- )
	dup /maxalign + T cfalign# H ht-nlstring, ;
[THEN]
: reset-included ( -- )
    [IFDEF] current-sourcepos1    included-files $free
    [ELSE] 0 allocate throw 0 included-files 2! [THEN]
    [IFDEF] loadfilename#  loadfilename# off  [THEN]
    s" kernel/main.fs" h-add-included-file ;
: reset-locs ( -- )  $100 to glocs-start ;
: glocs-start glocs-start ;
: shorten-path ( addr u -- addr' u' )
    2>r
    fpath path>string  BEGIN  next-path dup  WHILE
	    2r@ 2over string-prefix?
	    over 2r@ drop + c@ '/' = and
	    IF  2r> 2 pick 1+ /string 2>r  THEN
	    2drop  REPEAT
    2drop 2drop  2r> compact-filename ;
: included-files, ( -- addr )
    cell allocate throw { w^ array }  0 array @ !
    tsourcepos1 #23 rshift  0 ?DO
	T here H
	array @ I 2 + cells resize throw array !
	cell array @ +!
	array @ dup @ + !
	I loadfilename#>str shorten-path
	ht-lstring, T align H
    LOOP  T here H  array @ dup cell+ swap @
    dup cell / T cells , H bounds ?DO  I @ T A, H  cell +LOOP
    array @ free throw ;
>CROSS

\ Target Document Creation (goes to crossdoc.fd)       05jul95py

s" ./doc/crossdoc.fd.tmp" r/w create-file throw value doc-file-id
\ contains the file-id of the documentation file

: T-\G ( -- )
    source >in @ /string doc-file-id write-line throw
    postpone \ ;

Variable to-doc  to-doc on

require hold-number-line.fs

: cross-doc-entry  ( -- )
    to-doc @ tlast @ 0<> and	\ not an anonymous (i.e. noname) header
    IF
        s" " doc-file-id write-line throw
        sourceline# 0 <<# hold#line #> doc-file-id write-line throw #>>
	s" make-doc " doc-file-id write-file throw
	Last-Header-Ghost @ >ghostname doc-file-id write-file throw
	>in @
	'(' parse 2drop
	')' parse doc-file-id write-file throw
	s"  )" doc-file-id write-file throw
	'\' parse 2drop					
	T-\G
	>in !
    THEN ;

\ Target TAGS creation

s" kernel.TAGS" r/w create-file throw value tag-file-id
s" kernel.tags" r/w create-file throw value vi-tag-file-id
\ contains the file-id of the tags file

Create tag-beg 1 c,  7F c,
Create tag-end 1 c,  01 c,
Create tag-bof 1 c,  0C c,
Create tag-tab 1 c,  09 c,

2variable last-loadfilename 0 0 last-loadfilename 2!
	    
: put-load-file-name ( -- )
    sourcefilename last-loadfilename 2@ d<>
    IF
	tag-bof count tag-file-id write-line throw
	sourcefilename 2dup
	tag-file-id write-file throw
	last-loadfilename 2!
	s" ,0" tag-file-id write-line throw
    THEN ;

: put-cross-gnu-tag-entry  ( addr u -- )
    tlast @ 0<>	\ not an anonymous (i.e. noname) header
    IF
	put-load-file-name
	source >in @ min tag-file-id write-file throw
	tag-beg count tag-file-id write-file throw
	tag-file-id write-file throw
	tag-end count tag-file-id write-file throw
	base @ decimal sourceline# 0 <# #s #> tag-file-id write-file throw
\	>in @ 0 <# #s ',' hold #> tag-file-id write-line throw
	s" ,0" tag-file-id write-line throw
	base !
    ELSE  2drop  THEN ;

: cross-gnu-tag-entry  ( -- )
    Last-Header-Ghost @ >ghostname put-cross-gnu-tag-entry ;

: put-cross-vi-tag-entry ( addr u -- )
    tlast @ 0<>	\ not an anonymous (i.e. noname) header
    IF
	sourcefilename vi-tag-file-id write-file throw
	tag-tab count vi-tag-file-id write-file throw
	vi-tag-file-id write-file throw
	tag-tab count vi-tag-file-id write-file throw
	s" /^" vi-tag-file-id write-file throw
	source vi-tag-file-id write-file throw
	s" $/" vi-tag-file-id write-line throw
    ELSE  2drop  THEN ;

: cross-vi-tag-entry ( -- )
    Last-Header-Ghost @ >ghostname put-cross-vi-tag-entry ;

: cross-tag-entry ( -- )
    cross-gnu-tag-entry
    cross-vi-tag-entry ;

: put-cross-tag-entry ( addr u -- )
    2dup put-cross-gnu-tag-entry
    put-cross-vi-tag-entry ;

: cross-record-name ( -- )
    >in @ parse-name put-cross-tag-entry >in ! ;

\ Check for words

Defer skip? ' false IS skip?

: skipdef ( "name" -- )
\G skip definition of an undefined word in undef-words and
\G all-words mode
    Ghost dup forward?
    IF  >magic <skip> swap !
    ELSE drop THEN ;

: tdefined? ( "name" -- flag ) 
    Ghost undefined? 0= ;

: defined2? ( "name" -- flag ) 
\G return true for anything else than forward, even for <skip>
\G that's what we want
    Ghost forward? 0= ;

: forced? ( "name" -- flag )
\G return true if it is a forced skip with defskip
    Ghost >magic @ <skip> = ;

: needed? ( -- flag ) \ name
\G returns a false flag when
\G a word is not defined
\G a forward reference exists
\G so the definition is not skipped!
    bl-word gfind
    IF dup undefined?
	nip
	0=
    ELSE  drop true  THEN ;

: doer? ( "name" -- 0 | addr ) \ name
    Ghost dup >magic @ <do:> = 
    IF >link @ ELSE drop 0 THEN ;

: skip-defs ( -- )
    BEGIN  refill  WHILE  source -trailing nip 0= UNTIL  THEN ;

\ Target header creation

Variable NoHeaderFlag
NoHeaderFlag off

: 0.r ( n1 n2 -- ) 
    base @ >r hex 
    0 swap <# 0 ?DO # LOOP #> type 
    r> base ! ;

: .sym ( adr len -- )
\G escapes / and \ to produce sed output
  bounds 
  DO I c@ dup
	CASE	'/' OF drop ." \/" ENDOF
		'\' OF drop ." \\" ENDOF
		dup OF emit ENDOF
	ENDCASE
    LOOP ;

Defer setup-execution-semantics  ' noop IS setup-execution-semantics
Defer hm, \ forward rference only
Defer hm-named
Defer hm-noname
0 Value lastghost

: >loc ( -- )
    T here H tcell - gxt-location drop ;

: (THeader ( "name" -- ghost )
    \  >in @ bl-word count type 2 spaces >in !
    \ wordheaders will always be compiled to rom
    switchrom hm,
    \ build header in target
    NoHeaderFlag @
    IF  NoHeaderFlag off
	hm-noname
    ELSE
	[ X has? f83headerstring ] [IF]
	    T align H tlast @ there >r T A, H
	    >in @ parse-name T name, H >in !
        r> tlast !
	[ELSE]
	    >in @ parse-name dup T aligned cfalign# name, H >in !
	    tlast @ T A, H
	    [ X has? new-cfa ] [IF]
		0 T , H
	    [THEN]
	    executed-ghost @ ?dup IF
		>do:ghost @ >exec2 @ execute
	    ELSE
		0 T , H
	    THEN
        there tlast !
	[THEN]
	1 headers-named +!	\ Statistic
	hm-named
    THEN
    T ( cfalign ) here H tlastcfa ! >loc
    \ Old Symbol table sed-script
\    >in @ cr ." sym:s/CFA=" there 4 0.r ." /"  bl-word count .sym ." /g" cr >in !
    HeaderGhost
    \ output symbol table to extra file
    dup >ghostname there symentry
    dup Last-Header-Ghost ! dup to lastghost
    dup >magic ^imm !     \ a pointer for immediate
    input-lexeme 2@ 2>r
    [IFDEF] alias-mask alias-mask flag! [THEN]
    cross-doc-entry cross-tag-entry 
    setup-execution-semantics
    2r> input-lexeme 2!
    ;

\ this is the resolver information from ":"
\ resolving is done by ";"
Variable ;Resolve 1 cells allot

: hereresolve ( ghost -- )
  there resolve ( 0 ;Resolve ! ) ;

: Theader  ( "name" -- ghost )
  (THeader dup hereresolve ;

Variable aprim-nr -20 aprim-nr !

: copy-execution-semantics ( ghost-from ghost-dest -- )
  >r
  dup >exec @ r@ >exec !
  dup >comp @ r@ >comp !
  dup >exec2 @ r@ >exec2 !
  dup >exec-compile @ r@ >exec-compile !
  dup >ghost-xt @ r@ >ghost-xt !
  dup >created @ r@ >created !
  rdrop drop ;

>TARGET

Variable last-prim-ghost
0 last-prim-ghost !

: asmprimname, ( ghost -- : name ) 
  dup last-prim-ghost !
  >r
  here parse-name string, r@ >asm-name !
  aprim-nr @ r> >asm-dummyaddr ! ;

Defer setup-prim-semantics

: mapprim   ( "forthname" "asmlabel" -- ) 
  THeader -1 aprim-nr +! aprim-nr @ cfa,
  asmprimname, 
  setup-prim-semantics ;

: mapprim:   ( "forthname" "asmlabel" -- ) 
  -1 aprim-nr +! aprim-nr @
  Ghost tuck swap resolve-noforwards <do:> swap tuck >magic !
  asmprimname, ;

: Doer:   ( cfa -- ) \ name
  >in @ skip? IF  2drop  EXIT  THEN  >in !
  dup 0< s" prims" T $has? H 0= and
  IF
      .sourcepos ." needs doer: " >in @ parse-name type >in ! cr
  THEN
  Ghost
  tuck swap resolve-noforwards <do:> swap >magic ! ;

Ghost prim-dummy Constant prim-ghost

Variable prim#
: first-primitive ( n -- )  prim# ! ;
: group 0 parse 2drop prim# @ 1- -$200 and prim# ! ;
: groupadd  ( n -- )  drop ;
: #primitive ( n "name" -- )
  prim-ghost executed-ghost !
  (THeader ( S xt ghost )
  2dup >exec2 !
  ['] prim-resolved over >comp !
  dup >ghost-flags <primitive> set-flag
  s" EC" T $has? H 0=
  IF
      T here H resolve-noforwards $8000 invert and cfa,
\      alias-mask flag!
  ELSE
      T here H resolve-noforwards cfa,
  THEN ;
: Primitive  ( -- ) \ name
  >in @ skip? IF  drop  EXIT  THEN  >in !
  s" prims" T $has? H 0=
  IF
     .sourcepos ." needs prim: " >in @ parse-name type >in ! cr
  THEN
  prim# @ #primitive
  -1 prim# +! ;
>CROSS

\ Conditionals and Comments                            11may93jaw

\G saves the existing cond action, this is used for redefining in
\G instant
Variable cond-xt-old

: cond-target ( -- )
\G Compiles semantic of redefined cond into new one
  cond-xt-old @ compile, ; immediate restrict

: ;Cond
  postpone ;
  swap ! ;  immediate

: Cond: ( "name" -- ) 
\G defines a conditional or another word that must
\G be executed directly while compiling
\G these words have no interpretative semantics by default
  Ghost
  >exec-compile
  dup @ cond-xt-old !
  :NONAME ;


: Comment ( -- )
  >in @ Ghost swap >in ! ' swap 
  2dup >exec-compile ! >exec ! ;

Comment (       Comment \

\ compile                                              10may93jaw

: compile  ( "name" -- ) \ name
  findghost
  dup >exec-compile @ ?dup
  IF    nip compile,
  ELSE  postpone literal postpone gexecute  THEN ;  immediate restrict
            
>TARGET

: '  ( -- xt ) 
\G returns the target-cfa of a ghost
  bl-word gfind 0= ABORT" CROSS: Ghost don't exists"
  g>xt ;

\ FIXME: this works for the current use cases, but is not
\ in all cases correct ;-) 
: comp' X ' 0 ;

Cond: [']  T ' H alit, ;Cond

>CROSS

: [T']
\ returns the target-cfa of a ghost, or compiles it as literal
  postpone [G'] 
  state @ IF postpone g>xt ELSE g>xt THEN ; immediate

\ \ threading model					13dec92py
\ modularized						14jun97jaw

X has? new-cfa [IF] 0 [ELSE] T 1 cells H [THEN] Value xt>body

X has? new-cfa [IF]
    : cfaddr, ( ghost -- )
	tcell -2 * T allot H addr, tcell T allot H ;
[ELSE]
    : cfaddr, ( ghost -- ) addr, ;
[THEN]

: (>body)   ( cfa -- pfa ) 
  xt>body + ;						' (>body) plugin-of t>body

: (doer,)   ( ghost -- ) 
  cfaddr, ;   					' (doer,) plugin-of doer,

: (docol,)  ( -- ) [G'] :docol (doer,) ;		' (docol,) plugin-of docol,

                                                        ' NOOP plugin-of ca>native

: (doprim,) ( -- )
  there xt>body + ca>native T a, H ;		' (doprim,) plugin-of doprim,

: (doeshandler,) ( -- ) 
    T H ; 					' (doeshandler,) plugin-of doeshandler,

Defer gset-extra

: (dodoes,) ( does-action-ghost -- )
    ]comp [G'] :dodoes cfaddr, comp[
    gset-extra ;					' (dodoes,) plugin-of dodoes,

: (dlit,) ( n -- ) compile lit td, ;			' (dlit,) plugin-of dlit,

: (lit,) ( n -- )  s>d dlit, ;				' (lit,) plugin-of lit,

\ if we don't produce relocatable code alit, defaults to lit, jaw
\ this is just for convenience, so we don't have to define alit,
\ separately for embedded systems....
T has? relocate H
[IF]
: (alit,) ( n -- )  compile lit T  a, H ;		' (alit,) plugin-of alit,
[ELSE]
: (alit,) ( n -- )  lit, ;				' (alit,) plugin-of alit,
[THEN]

: (fini,)         compile ;s ;				' (fini,) plugin-of fini,

[IFUNDEF] (code) 
Defer (code)
Defer (end-code)
[THEN]

>TARGET
: Code
  defempty?
  (THeader ( ghost )
  ['] prim-resolved over >comp !
  dup >exec2 there swap !
  there resolve-noforwards

  
  [ T e? prims H 0= [IF] T e? ITC H [ELSE] true [THEN] ] [IF]
  doprim, 
  [THEN]
  depth (code) ;

\ FIXME : no-compile -1 ABORT" this ghost is not for compilation" ;

: Code:
  defempty?
    Ghost dup 
    >r >ghostname there symentry
    r@ there ca>native resolve-noforwards
    <do:> r@ >magic !
    r> drop
    depth (code) ;

: end-code
    (end-code)
    depth ?dup IF   1- <> ABORT" CROSS: Stack changed"
    ELSE true ABORT" CROSS: Stack empty" THEN
    ;

>CROSS

\ tLiteral                                             12dec92py

>TARGET
Cond: \G  T-\G ;Cond

Cond: Literal  ( n -- )   lit, ;Cond
Cond: ALiteral ( n -- )   alit, ;Cond

: Char ( "<char>" -- )  bl-word char+ c@ ;
Cond: [Char]   ( "<char>" -- )  Char  lit, ;Cond

: (x#) ( adr len base -- )
  base @ >r base ! 0 0 parse-name >number 2drop drop r> base ! ;

: d# $0a (x#) ;
: h# $010 (x#) ;

Cond: d# $0a (x#) lit, ;Cond
Cond: h# $010 (x#) lit, ;Cond

tchar 1 = [IF]
Cond: chars ;Cond 
[THEN]

\ some special literals					27jan97jaw

Cond: MAXU
  -1 s>d dlit,
  ;Cond

tcell 2 = tcell 4 = or tcell 8 = or 0=
[IF]
.( Warning: MINI and MAXI may not work with this host) cr
[THEN]

Cond: MINI
  tcell 2 = IF $8000 ELSE $80000000 THEN 0
  tcell 8 = IF swap THEN dlit,
  ;Cond
 
Cond: MAXI
  tcell 2 = IF $7fff ELSE $7fffffff THEN 0
  tcell 8 = IF drop -1 swap THEN dlit,
  ;Cond

>CROSS

\ Target compiling loop                                12dec92py
\ ">tib trick thrown out                               10may93jaw
\ number? defined at the top                           11may93jaw
\ replaced >in by save-input				

: discard 0 ?DO drop LOOP ;

\ compiled word might leave items on stack!
: tcom ( x1 .. xn n name -- )
\  dup count type space
  gfind 
  IF    >r ( discard saved input state ) discard r>
	dup >exec-compile @ ?dup
        IF   nip execute-exec-compile ELSE gexecute  THEN 
	EXIT 
  THEN
  (number?) dup  
  IF	0> IF swap lit,  THEN  lit, discard
  ELSE	2drop restore-input throw Ghost gexecute THEN  ;

\ : ; DOES>                                            13dec92py
\ ]                                                     9may93py/jaw

>CROSS

: compiling-state ( -- )
\G set states to compililng
    Compiling comp-state !
    \ if we have a state in target, change it with the compile state
    [G'] state dup undefined? 0= 
    IF >ghost-xt @ execute X on ELSE drop THEN ;

: interpreting-state ( -- )
\G set states to interpreting
   \ if target has a state variable, change it according to our state
   [G'] state dup undefined? 0= 
   IF >ghost-xt @ execute X off ELSE drop THEN
   Interpreting comp-state ! ;

>TARGET

Variable no-loop

: ] no-loop @ IF  no-loop off  EXIT  THEN
    compiling-state
    BEGIN
        compiling? WHILE
        BEGIN save-input bl-word
              dup c@ 0= WHILE drop discard refill 0=
              ABORT" CROSS: End of file while target compiling"
        REPEAT
        tcom
    REPEAT ;

\ by the way: defining a second interpreter (a compiler-)loop
\             is not allowed if a system should be ANS conformant

ghost :-dummy Constant :-ghost

: (:) ( ghost -- ) 
\ common factor of : and :noname. Prepare ;Resolve and start definition
    ;Resolve ! there ;Resolve cell+ !
    >loc
    docol, ]comp  colon-start depth T ] H ;

ghost : drop
ghost :noname drop

: : ( -- colon-sys ) \ Name
  defempty? [g'] : gwhere,
  constflag off \ don't let this flag work over colon defs
		\ just to go sure nothing unwanted happens
  >in @ skip? IF  drop skip-defs  EXIT  THEN  >in !
  :-ghost executed-ghost !  (THeader (:) ;

: gstart-xt ( -- colon-sys xt )
    [ x has? new-cfa ] [IF] T 2 cells allot H [THEN]
    there >r here ghostheader r@ resolve
    docol, ]comp colon-start depth ;Resolve off T ] H r> ;

: :noname ( -- xt colon-sys )
    switchrom hm, [g'] :noname gwhere,
    [ X has? f83headerstring 0= ] [IF]
	[ X has? new-cfa ] [IF] T cfalign 0 A, H [ELSE] T 0 cell+ cfalign# H [THEN]
	:-ghost >do:ghost @ >exec2 @ execute
    [ELSE]
	X cfalign
    [THEN]
    there 
    \ define a nameless ghost
    here ghostheader dup last-header-ghost ! dup to lastghost
    (:) hm-noname ;

Cond: EXIT ( -- )   compile ;S  ;Cond

Cond: ?EXIT ( -- ) 1 abort" CROSS: using ?exit" ;Cond

>CROSS
: LastXT ;Resolve @ 0= abort" CROSS: no definition for LastXT"
         ;Resolve cell+ @ ;

>TARGET

: resolved ( -- )
    ;Resolve @ 
    IF  ['] colon-resolved ;Resolve @ >comp !
	;Resolve @ ;Resolve cell+ @ resolve 
    THEN ;
     
Cond: recurse ( -- ) Last-Header-Ghost @ gexecute ;Cond

: (;) 	depth ?dup 
	IF   1- <> ABORT" CROSS: Stack changed"
	ELSE true ABORT" CROSS: Stack empty" 
	THEN
	colon-end
	fini,
	comp[
	T resolved H interpreting-state ;

Cond: ; ( -- ) (;)
	;Cond

Cond: [ ( -- ) interpreting-state ;Cond

>CROSS

0 Value created

Ghost does, drop
Ghost !does drop

Defer gset-optimizer

: kill-dovar ( -- )
    tlastcfa @ [G'] :dovar killref ;
: !does ( does-action -- )
    kill-dovar
    [G'] does, gset-optimizer
    >space here >r ghostheader space>
    ['] colon-resolved r@ >comp !
    r@ created >do:ghost ! r@ swap resolve
    r> tlastcfa @ >tempdp dodoes, tempdp> ;

Defer !newdoes

Defer instant-interpret-does>-hook  ' noop IS instant-interpret-does>-hook

>TARGET

X has? new-does [IF]
X has? primcentric [IF]
: does-resolved ( ghost -- )
    compile does-xt g>xt T a, H ;
[ELSE]
: does-resolved ( ghost -- )
    g>xt T a, H ;
[THEN]

: resolve-does>-part ( -- )
\ resolve words made by builders
  Last-Header-Ghost @ >do:ghost @ ?dup 
  IF  there resolve  THEN ;

Cond: DOES>
    T here H
[ T has? primcentric H [IF]
    T has? new-cfa H [IF] ] #7 [ [ELSE] ] #6 [ [THEN]
[ELSE] ] #7 [ [THEN] ]
    T cells H + T cfaligned H alit, compile set-does> compile ;
    Last-Header-Ghost @ >do:ghost @ >r
T :noname H
    hm-noname
    r> ?dup IF  swap resolve  ELSE  drop  THEN
;Cond

X has? new-cfa [IF] #10 [ELSE] #10 [THEN] Constant does-xt-off
: DOES>
    ['] does-resolved created >comp !
    T here does-xt-off cells H + T cfaligned H \ includes noname header+vtable
    !newdoes
    T :noname H 2drop
    instant-interpret-does>-hook
    depth ;
[ELSE]
T has? primcentric H [IF]
: does-resolved ( ghost -- )
\    g>xt dup T >body H alit, compile call T cell+ @ a, H ;
    compile does-xt g>xt T a, H ;
[ELSE]
: does-resolved ( ghost -- )
    g>xt T a, H ;
[THEN]

: resolve-does>-part ( -- )
\ resolve words made by builders
  Last-Header-Ghost @ >do:ghost @ ?dup 
  IF  there resolve  THEN ;

Cond: DOES>
        T here H [ T has? primcentric H [IF] ] 5 [ [ELSE] ] 4 [ [THEN] ] T cells
        H + alit, compile !does compile ;s
        doeshandler, resolve-does>-part
        ;Cond

: DOES>
    ['] does-resolved created >comp !
    switchrom doeshandler, T here H !does 
    instant-interpret-does>-hook
    depth T ] H ;
[THEN]

>CROSS
\ Creation                                              01nov92py

\ Builder                                               11may93jaw

0 Value built

: Builder    ( Create-xt do-ghost "name" -- )
\ builds up a builder in current vocabulary
\ create-xt is executed when word is interpreted
\ do:-xt is executed when the created word from builder is executed
\ for do:-xt an additional entry after the normal ghost-entrys is used

  ghost to built 
  built >created @ 0= IF
    built >created on
  THEN ;

: gdoes,  ( ghost -- )
\ makes the codefield for a word that is built
    >do:ghost @ dup undefined? 0=
    IF
	doer/does,
	EXIT
    THEN
\  compile :dodoes gexecute
\  T here H tcell - reloff 
  2 refered 
  ;

: takeover-x-semantics ( S constructor-ghost new-ghost -- )
   \g stores execution semantic and compilation semantic in the built word
   swap >do:ghost @ 2dup swap >do:ghost !
   \ we use the >exec2 field for the semantic of a created word,
   \ using exec or exec2 makes no difference for normal cross-compilation
   \ but is usefull for instant where the exec field is already
   \ defined (e.g. Vocabularies)
   2dup >exec @ swap >exec2 ! 
   >comp @ swap >comp ! ;

0 Value createhere

: create-resolve ( -- )
    created createhere resolve ( 0 ;Resolve ! ) ;
\ : create-resolve-immediate ( -- )
\      create-resolve T immediate H ;

: TCreate ( <name> -- )
  create-forward-warn
  IF ['] reswarn-forward IS resolve-warning THEN
  executed-ghost @ (Theader
  dup >created on  dup to created
  2dup takeover-x-semantics
  there to createhere drop gdoes, ;

: RTCreate ( <name> -- )
\ creates a new word with code-field in ram
  create-forward-warn
  IF ['] reswarn-forward IS resolve-warning THEN
  \ make Alias
  executed-ghost @ (THeader 
  dup >created on  dup to created
  2dup takeover-x-semantics
  there 0 T a, H [IFDEF] alias-mask alias-mask flag! [THEN]
  \ store poiter to code-field
  switchram T cfalign H
  there swap T ! H
  there tlastcfa ! 
  there to createhere drop gdoes, ;

: Build:  ( -- [xt] [colon-sys] )
  :noname postpone TCreate ;

: BuildSmart:  ( -- [xt] [colon-sys] )
  :noname
  [ T has? rom H [IF] ]
  postpone RTCreate
  [ [ELSE] ]
  postpone TCreate 
  [ [THEN] ] ;

: ;Build
  postpone create-resolve postpone ; built >exec ! ; immediate

\ : ;Build-immediate
\     postpone create-resolve-immediate
\     postpone ; built >exec ! ; immediate

: gdoes>  ( ghost -- addr flag )
  executed-ghost @ g>body ;

\ DO: ;DO                                               11may93jaw

: do:ghost! ( ghost -- ) built >do:ghost ! ;
: doexec! ( xt -- ) built >do:ghost @ >exec ! ;

: DO:     ( -- [xt] [colon-sys] )
  here ghostheader do:ghost!
  :noname postpone gdoes> ;

: by:     ( -- [xt] [colon-sys] ) \ name
  Ghost do:ghost!
  :noname postpone gdoes> ;

: hmghost:  ( ghost -- )
    Ghost >r
    :noname r> postpone Literal postpone cfaddr, postpone ;
    built >do:ghost @ >exec2 ! ;

Variable thm-list
Variable ghm-list

Ghost docol-hm drop

>TARGET
8 T cells H Constant hmsize
>CROSS

8 cells Constant ghmsize \ ghost header method vtables for comparison

ghost :,
ghost peephole-compile,
2drop
ghost set-does>
drop
ghost value,
ghost constant,
2drop
ghost 2constant,
ghost 2@
ghost variable,
2drop drop
ghost user,
ghost defer,
2drop
ghost u-compile,
ghost uvalue-to
2drop
ghost field+,
ghost abi-code,
2drop
ghost ;abi-code,
drop
ghost default-name>comp
drop
ghost i/c>int
ghost i/c>comp
2drop
ghost named>string
ghost named>link
2drop
ghost noname>string
ghost noname>link
2drop
ghost value-to
ghost defer-is
ghost umethod,
2drop
ghost is-umethod
drop
ghost u#exec
ghost u#+
ghost uvar,
2drop drop
ghost :loc,
drop
ghost x#exec
ghost noop
ghost no.extensions
2drop drop

Create hmtemplate
0 ,
findghost :, ,
findghost n/a ,
0 ,
findghost noop ,
findghost default-name>comp ,
findghost named>string ,
findghost named>link ,
0 ,

Struct
    cell% field g>hmlink
    cell% field g>hmcompile,
    cell% field g>hmto
    cell% field g>hmextra
    cell% field g>hm>int
    cell% field g>hm>comp
    cell% field g>hm>string
    cell% field g>hm>link
End-Struct vtable-struct

\ stores 8 ghosts and a link

: hm= ( hm1 hm2 -- flag )
    cell+ swap ghmsize cell /string tuck compare 0= ;

: (hm,) ( -- )
    T align  here H thm-list @ T A, H  dup thm-list !
    hmtemplate ghmsize cell /string bounds DO
	I @ addr,
    cell +LOOP
    \ copy table of ghosts for comparison
    dup hmtemplate @ T ! H  hmtemplate off
    align , here ghm-list @ dup , over ! ghm-list !
    hmtemplate ghmsize cell /string bounds DO
	I @ ,
    cell +LOOP ;

:noname ( -- )
    hmtemplate @ 0= IF EXIT THEN
    ghm-list
    BEGIN  @ dup  WHILE
	    dup hmtemplate hm= IF
		1 cells - @ hmtemplate @ T ! H  hmtemplate off  EXIT  THEN
    REPEAT  drop (hm,) ; IS hm,

: hm-template, ( -- )
    T here 0 A, H hmtemplate ! ;
:noname ( -- )
    [G'] noop              hmtemplate g>hm>int !
    [G'] default-name>comp hmtemplate g>hm>comp !
    [G'] named>string      hmtemplate g>hm>string !
    [G'] named>link        hmtemplate g>hm>link ! ; is hm-named
:noname ( -- )
    [G'] noop              hmtemplate g>hm>int !
    [G'] default-name>comp hmtemplate g>hm>comp !
    [G'] noname>string     hmtemplate g>hm>string !
    [G'] noname>link       hmtemplate g>hm>link ! ; is hm-noname
: hm-populate ( -- )
    [G'] :,                hmtemplate g>hmcompile, !
    [G'] n/a               hmtemplate g>hmto !
    TNIL                   hmtemplate g>hmextra !
    hm-named ;

:noname ( ghost -- )  hmtemplate g>hmcompile, ! ; IS gset-optimizer
: gset-to ( ghost -- )        hmtemplate g>hmto ! ;
: gset->int ( ghost -- )      hmtemplate g>hm>int ! ;
: gset->comp ( ghost -- )     hmtemplate g>hm>comp ! ;
:noname ( ghost -- )     hmtemplate g>hmextra ! ; is gset-extra

: set-optimizer ( xt -- ) xt>ghost gset-optimizer ;
: set-to       ( xt -- )  xt>ghost gset-to ;
: set->int     ( xt -- )  xt>ghost gset->int ;
: set->comp    ( xt -- )  xt>ghost gset->comp ;
: set-extra    ( xt -- )  xt>ghost gset-extra ;

: hm: ( -- xt colon-sys )
    :noname postpone hm-template, postpone hm-populate ;
: ;hm ( xt colon-sys -- )
    postpone ;  built >do:ghost @ >exec2 ! ; immediate

>TARGET
ghost imm>comp

: immediate     ( immediate-mask flag! )
    [G'] imm>comp gset->comp
    ^imm @ @ dup <imm> = IF  drop  EXIT  THEN
    <res> <> ABORT" CROSS: Cannot immediate a unresolved word"
    immediate-mask flag!
    <imm> ^imm @ ! ;

ghost a>int drop
ghost a>comp drop
ghost a-to drop
ghost s-to drop
ghost :dodefer drop
ghost ?fold1 drop

: Alias    ( cfa -- ) \ name
    >in @ skip? IF  2drop  EXIT  THEN  >in !
    (THeader ( S xt ghost )
    2dup swap xt>ghost swap copy-execution-semantics
    [G'] a>int  gset->int
    [G'] a>comp gset->comp
    [G'] s-to   gset-to
    over resolve [G'] :dodefer (doer,) T A, H ;

: interpret/compile: ( xt1 xt2 "name" -- )
    (THeader <res> over >magic !  there swap >link !
    [G'] :dodefer (doer,)
    swap T A, A, H 
    hm-populate
    [G'] n/a     gset-to
    [G'] a>int   gset->int
    [G'] i/c>comp hmtemplate g>hm>comp ! ;

: opt: ( -- colon-sys )   gstart-xt set-optimizer ;
: comp: ( -- colon-sys )  gstart-xt set-optimizer ;
: fold1: ( -- colon-sys ) no-loop on gstart-xt set-optimizer
    compile ?fold1 T ] H ;

variable cross-boot$[]
variable cross-boot[][]

>TARGET

: boot$[], ( -- )
    H cross-boot$[] $@ dup cell / T cell * , H bounds ?DO I @ T A, H cell +LOOP ;
: boot[][], ( -- )
    H cross-boot[][] $@ dup cell / T cell * , H bounds ?DO I @ T A, H cell +LOOP ;

: wheres, ( -- ) cr ." Compiling wheres" cr
    H cross-wheres $@ dup cell / T cell * , H bounds ?DO I 2@
	dup undefined? IF  ." undefined " dup >ghostname type cr drop -1
	ELSE  g>xt  THEN
    T A, , H 2 cells +LOOP ;

: wheres-off H 0 cross-wheres $!len ;

: locs[], ( -- )
    \ transfer locations to target
    cross-locs[] $@len cell/ T cells , H
    cross-locs[] $@ bounds DO
	I @ T , H
    cell +LOOP ;

>CROSS

\ instantiate deferred extra, now

:noname ( doesxt -- )
    kill-dovar
    [G'] does, gset-optimizer
    >space here >r ghostheader space>
    ['] colon-resolved r@ >comp !
    r@ created >do:ghost ! r@ swap resolve
    r> tlastcfa @ >tempdp dodoes, tempdp> ;
IS !newdoes

: ;DO ( [xt] [colon-sys] -- )
  postpone ; doexec! ; immediate

: by      ( -- ) \ Name
  Ghost >do:ghost @ do:ghost! ;

: compile: ( --[xt] [colon-sys] )
\G defines a compile time action for created words
\G by this builder
  :noname ;

: ;compile ( [xt] [colon-sys] -- )
  postpone ; built >do:ghost @ >comp ! ; immediate

\ Dummy builder

Builder :-dummy
Build: ;Build
by: :docol ;DO
hm: [G'] :, gset-optimizer ;hm

Builder prim-dummy
Build: ;Build
by: :doprim ;DO \ :doprim is a dummy, we will not use it
hm: [G'] peephole-compile, gset-optimizer ;hm

<do:> Ghost :doprim >magic !

Builder does>-dummy
Build: ;Build
by: :dodoes ;DO
hm: [G'] does, gset-optimizer
    [G'] noop  gset-extra  ;hm
\ hmghost: dodoes-hm

Ghost to:,
Ghost to:exec
2drop

Builder to-class:
Build: T A, A, H ;Build
by: :dodoes1 ;DO
hm: [G'] to:, gset-optimizer
    [G'] to:exec  gset-extra  ;hm
\ hmghost: dodoes-hm

Ghost to-access:,
Ghost to-access:exec
2drop

Builder to-access:
Build: T , H ;Build
by: :dodoes2 ;DO
compile: g>xt compile does-xt T a, H ;compile
hm: [G'] to-access:, gset-optimizer
    [G'] to-access:exec gset-extra  ;hm

\ Variables and Constants                              05dec92py

Builder (Constant)
Build:  ( n -- ) ;Build
by: :docon ( target-body-addr -- n ) T @ H ;DO
compile: g>body compile lit T d@ dV, H ;compile
hm: [G'] constant, gset-optimizer ;hm

Builder (AConstant)
Build:  ( n -- ) ;Build
by: :doacon ( target-body-addr -- n ) T @ H ;DO
compile: g>body compile lit T d@ dA, H ;compile
hm: [G'] constant, gset-optimizer ;hm

Builder Constant
Build:  ( n -- ) T , H ;Build
by (Constant)

Builder dConstant
Build:  ( d -- ) T d, H ;Build
by (Constant)

Builder AConstant
Build:  ( n -- ) T A, H ;Build
by (AConstant)

Builder 2Constant
Build:  ( d -- ) T , , H ;Build
by: :dodoes3 ( ghost -- d ) T dup cell+ @ swap @ H ;DO
hm: [G'] 2constant, gset-optimizer
    [G'] 2@ gset-extra ;hm

Builder Create
BuildSmart: ;Build
by: :dovar ( target-body-addr -- addr ) ;DO
hm: [G'] variable, gset-optimizer ;hm \ hmghost: dovar-hm

Builder <Builds
BuildSmart: ;Build
by: :dodoes4 ( target-body-addr -- addr ) ;DO
hm: [G'] does, gset-optimizer ;hm \ hmghost: dovar-hm

Builder Variable
T has? rom H [IF]
Build: ( -- ) T here 0 A, H switchram T align here swap ! 0 , H ( switchrom ) ;Build
by (Constant)
[ELSE]
Build: T 0 , H ;Build
by Create
[THEN]

Builder 2Variable
T has? rom H [IF]
Build: ( -- ) T here 0 A, H switchram T align here swap ! 0 , 0 , H ( switchrom ) ;Build
by (Constant)
[ELSE]
Build: T 0 , 0 , H ;Build
by Create
[THEN]

Builder AVariable
T has? rom H [IF]
Build: ( -- ) T here 0 A, H switchram T align here swap ! 0 A, H ( switchrom ) ;Build
by (Constant)
[ELSE]
Build: T 0 A, H ;Build
by Create
[THEN]

Builder $Variable
Build: T here 0 A, H cross-boot$[] >stack ;Build
by Create

Builder $[]Variable
Build: T here 0 A, H cross-boot[][] >stack ;Build
by Create

Variable t-theme-color#  1 t-theme-color# !

ghost theme-to drop

Builder theme-color:
Build: t-theme-color# @ T , H  1 t-theme-color# +! ;Build
DO: ;DO
compile: g>xt compile does-xt T a, H ;compile
hm: [G'] does, gset-optimizer
    [G'] theme-to gset-to
;hm

\ User variables                                       04may94py

: tup@ user-region >rstart @ ;

\ Variable tup  0 tup !
\ Variable tudp 0 tudp !

: u,  ( n -- udp )
  current-region >r user-region activate
  X here swap X , tup@ -
  r> activate ;

: au, ( n -- udp )
  current-region >r user-region activate
  X here swap X a, tup@ - 
  r> activate ;

T has? no-userspace H [IF]

: buildby
  ghost >exec @ built >exec ! ;

Builder User
buildby Variable
by Variable

Builder 2User
buildby 2Variable
by 2Variable

Builder AUser
buildby AVariable
by AVariable

[ELSE]
    Builder User
    Build: 0 u, X , ;Build
    by: :douser ( ghost -- up-addr )  X @ tup@ + ;DO
    hm: [G'] user, gset-optimizer ;hm
    
    Builder 2User
    Build: 0 u, X , 0 u, drop ;Build
    by User
    
    Builder AUser
    Build: 0 au, X , ;Build
    by User
[THEN]

T has? rom H [IF]
    Builder (Value)
    Build:  ( n -- ) ;Build
    by: :dovalue ( target-body-addr -- n ) T @ @ H ;DO
    hm: [G'] value, gset-optimizer [G'] value-to gset-to ;hm
    
    Builder Value
    Build: T here 0 A, H switchram T align here swap ! , H ;Build
    by (Value)
    
    Builder AValue
    Build: T here 0 A, H switchram T align here swap ! A, H ;Build
    by (Value)
[ELSE]
    Builder (Value)
    Build:  ( n -- ) ;Build
    by: :dovalue ( target-body-addr -- n ) T @ H ;DO
    hm: [G'] value, gset-optimizer [G'] value-to gset-to ;hm

    Builder Value
    BuildSmart: T , H ;Build
    by (Value)
    
    Builder dValue
    BuildSmart: T d, H ;Build
    by (Value)
    
    Builder AValue
    BuildSmart: T A, H ;Build
    by (Value)
[THEN]

Builder (UValue)
Build: 0 u, T , H ;Build
DO: X @ tup@ + X @ ;DO
hm: [G'] u-compile, gset-optimizer [G'] uvalue-to gset-to ;hm

Defer texecute

Builder Defer
T has? rom H [IF]
    Build: ( -- )  T here 0 A, H switchram T align here swap ! H [T'] noop T A, H ( switchrom ) ;Build
    by: :dodefer ( ghost -- ) X @ X @ texecute ;DO
[ELSE]
    BuildSmart:  ( -- ) [T'] noop T A, H ;Build
    by: :dodefer ( ghost -- ) X @ texecute ;DO
[THEN]
hm:
[G'] defer, gset-optimizer
[G'] defer-is gset-to ;hm

\ Sturctures                                           23feb95py

: nalign ( addr1 n -- addr2 )
\ addr2 is the aligned version of addr1 wrt the alignment size n
 1- tuck +  swap invert and ;

Builder (Field)
Build: ;Build
by: :dofield T @ H + ;DO
hm: [G'] field+, gset-optimizer ;hm

Builder Field
Build: ( align1 offset1 align size "name" --  align2 offset2 )
    rot dup T , H ( align1 align size offset1 )
    + >r nalign r> ;Build
by (Field)

>TARGET
Builder end-struct
Build: T , , H ;Build
by 2Constant
: struct  T 1 chars 0 H ;

: cell% ( n -- size align )
    T 1 cells H dup ;
>CROSS

\ Forth 2012 structures

Builder +field
Build: ( offset size -- offset' )
over T , H + ;Build
by (Field)
Builder field:
Build: ( offset -- offset' )
T aligned dup , cell+ H ;Build
by (Field)
Builder cfield:
Build: ( offset -- offset' )
dup T , char+ H ;Build
by (Field)

\ ABI-CODE support
Builder (ABI-CODE)
Build: ;Build
by: :doabicode noop ;DO
hm: [G'] abi-code, gset-optimizer ;hm

BUILDER (;abi-code)
Build: ;Build
by: :do;abicode noop ;DO
hm: [G'] ;abi-code, gset-optimizer ;hm

\ User Methods                                         22jun13py

Variable class-o

Builder user-o
DO: true abort" not in cross compiler!" ;DO
Build: 0 au, dup class-o ! X , ;Build
by User

Builder uval-o
DO: true abort" not in cross compiler!" ;DO
Build: 0 au, dup class-o ! X , ;Build
by (UValue)

>TARGET
: umethod ( m v -- m' v )
    over >r no-loop on T : H compile u#exec class-o @ T , H
    r> tcell / T , (;) swap cell+ swap H
    [G'] umethod, gset-optimizer
    [G'] is-umethod gset-to
\    [G'] umethod-defer@ gset-defer@
;

: uvar ( m v size -- m v' )
    over >r no-loop on T : H compile u#+ class-o @ T , H
    r> T , (;) H +
    [G'] uvar, gset-optimizer ;
>CROSS

\ Mini-OOF

>TARGET
: method ( m v -- m' v )
    over >r no-loop on T : H compile x#exec
    r> tcell / T , (;) swap cell+ swap H ;
>CROSS

Builder var
Build: ( m v size -- m v+size )  over T , H + ;Build
by (Field)

Builder end-class
Build: ( addr m v -- )
   T here >r , dup , 2 cells H ?DO T ['] noop A, 1 cells H +LOOP
   T cell+ dup cell+ r> rot @ 2 cells /string move H ;Build
by Create

: class ( class -- class methods vars ) dup T 2@ H ;
: defines ( xt class -- )  T ' >body @ + ! H ;

\ rectype

Builder rectype:
Build: ( xtint xtcomp xtpost --- )
    T rot A, swap A, A, H 7 0 DO [T'] no.extensions X A, LOOP ;Build
    by Create

ghost do-translate drop
Builder translate:
Build: ( xtint xtcomp xtpost --- )
    T rot A, swap A, A, H 7 0 DO [T'] no.extensions X A, LOOP ;Build
by: :dodoes5 ;DO
hm: [G'] does, gset-optimizer
[G'] do-translate gset-extra  ;hm

\ Peephole optimization					05sep01jaw

\ this section defines different compilation
\ actions for created words
\ this will help the peephole optimizer
\ I (jaw) took this from bernds latest cross-compiler
\ changes but seperated it from the original
\ Builder words. The final plan is to put this
\ into a seperate file, together with the peephole
\ optimizer for cross


T has? primcentric H [IF]

\ .( loading peephole optimization) cr

>CROSS

: (callc) compile call T >body a, H ;		' (callc) plugin-of colon,
: (callcm) T here 0 a, 0 a, H ;                 ' (callcm) plugin-of colonmark,
: (call-res) >tempdp resolved gexecute tempdp> drop ;
                                                ' (call-res) plugin-of colon-resolve
T has? ec H [IF]
: (pprim) T @ H >signed dup 0< IF  $4000 -  ELSE
    cr ." wrong usage of (prim) "
    dup gdiscover IF  .ghost  ELSE  .  THEN  cr -1 throw  THEN
    T a, H ;					' (pprim) plugin-of prim,
[ELSE]
: (pprim) dup 0< IF  $4000 -  ELSE
    cr ." wrong usage of (prim) "
    dup gdiscover IF  .ghost  ELSE  .  THEN  cr -1 throw  THEN
    T a, H ;					' (pprim) plugin-of prim,
[THEN]

\ if we want this, we have to spilt aconstant
\ and constant!!
\ Builder (Constant)
\ compile: g>body X @ lit, ;compile

Builder (Value)
compile: g>body compile lit@ T a, H ;compile

\ this changes also Variable, AVariable and 2Variable
Builder Create
compile: g>body alit, ;compile

Builder User
compile: g>body compile up@ compile lit+ T @ V, H ;compile

Builder Defer
compile: g>body compile lit-perform T A, H ;compile

Builder (Field)
compile: g>body T @ H compile lit+ T V, H ;compile

Builder (UValue)
compile: g>body compile up@ compile lit+ T @ V, H compile @ ;compile

Builder 2Constant
compile: g>body compile lit T dup cell+ @ V, H compile lit T @ V, H ;compile
[THEN]

\ structural conditionals                              17dec92py

>CROSS
: (ncontrols?) ( n -- ) 
\g We expect n open control structures
  depth over u<= 
  ABORT" CROSS: unstructured, stack underflow"
  0 ?DO I pick 0= 
        ABORT" CROSS: unstructured" 
  LOOP ;					' (ncontrols?) plugin-of ncontrols?

\ : ?struc      ( flag -- )       ABORT" CROSS: unstructured " ;
\ : sys?        ( sys -- sys )    dup 0= ?struc ;

: >mark       ( -- sys )        T here  0 , H ;

X has? abranch [IF]
    : branchoffset ( src dest -- )  drop ;
    : offset, ( n -- )  X A, ;
[ELSE]
    : branchoffset ( src dest -- )  - tchar / ; \ ?? jaw
    : offset, ( n -- )  X , ;
[THEN]

:noname compile branch X here branchoffset offset, ;
  IS branch, ( target-addr -- )
:noname compile ?branch X here branchoffset offset, ;
  IS ?branch, ( target-addr -- )
:noname compile branch T here 0 H offset, ;
  IS branchmark, ( -- branchtoken )
:noname compile ?branch T here 0 H offset, ;
  IS ?branchmark, ( -- branchtoken )
:noname compile ?dup-?branch T here 0 H offset, ;
  IS ?dup-?branchmark, ( -- branchtoken )
:noname T here 0 H offset, ;
  IS ?domark, ( -- branchtoken )
:noname dup X @ 0= defstart and ?struc
    X here over branchoffset swap X ! ;
  IS branchtoresolve, ( branchtoken -- )
:noname branchto, X here ;
  IS branchtomark, ( -- target-addr )

>TARGET

\ Structural Conditionals                              12dec92py

\ CLEANUP Cond: BUT       sys? swap ;Cond
\ CLEANUP Cond: YET       sys? dup ;Cond

>CROSS

Variable tleavings 0 tleavings !

: (done) ( do-addr -- )
\G resolve branches of leave and ?leave and ?do
\G do-addr is the address of the beginning of our
\G loop so we can take care of nested loops
    tleavings @
    BEGIN  dup
    WHILE
	>r dup r@ cell+ @ \ address of branch
	u> 0=	   \ lower than DO?	
    WHILE
	r@ 2 cells + @ \ branch token
	branchtoresolve,
	r@ @ r> free throw
    REPEAT  r>  THEN
    tleavings ! drop ;

>TARGET

\ What for? ANS? JAW Cond: DONE   ( addr -- )  (done) ;Cond

>CROSS
: (leave) ( branchtoken -- )
    3 cells allocate throw >r
    T here H r@ cell+ !
    r@ 2 cells + !
    tleavings @ r@ !
    r> tleavings ! ;
>TARGET

: (leave,) ( -- ) 
  branchmark, (leave) ; 			' (leave,) plugin-of leave,

: (?leave,) ( -- )
  compile 0= ?branchmark, (leave) ; 		' (?leave,) plugin-of ?leave,

Cond: LEAVE     leave, ;Cond
Cond: ?LEAVE    ?leave, ;Cond

>CROSS
\ !!JW ToDo : Move to general tools section

: to1 ( x1 x2 xn n -- addr )
\G packs n stack elements in am allocated memory region
   dup dup 1+ cells allocate throw dup >r swap 1+
   0 DO tuck ! cell+ LOOP
   drop r> ;

: 1to ( addr -- x1 x2 xn )
\G unpacks the elements saved by to1
    dup @ swap over cells + swap
    0 DO  dup @ swap 1 cells -  LOOP
    free throw ;

: loop] ( target-addr -- )
  branchto, 
  dup 	X here branchoffset offset, 
  tcell - (done) ;

: skiploop] ?dup IF branchto, branchtoresolve, THEN ;

>TARGET

\ Structural Conditionals                              12dec92py

: (cs-swap) ( x1 x2 -- x2 x1 )
  swap ;					' (cs-swap) plugin-of cs-swap

: (ahead,) branchmark, ; 			' (ahead,) plugin-of ahead,

: (if,) ?branchmark, ; 				' (if,) plugin-of if,

: (?dup-if,) ?dup-?branchmark, ; 		' (?dup-if,) plugin-of ?dup-if,

: (then,) branchto, branchtoresolve, ; 		' (then,) plugin-of then,

: (else,) ( ahead ) branchmark, 
          swap 
          ( then ) branchto, branchtoresolve, ;	' (else,) plugin-of else,

: (begin,) branchtomark, ; 			' (begin,) plugin-of begin,

: (while,) ( if ) ?branchmark,
           swap ; 				' (while,) plugin-of while,

: (again,) branch, ;				' (again,) plugin-of again,

: (until,) ?branch, ;				' (until,) plugin-of until,

: (repeat,) ( again ) branch,
            ( then ) branchto, branchtoresolve, ; ' (repeat,) plugin-of repeat,

: (case,)   ( -- n )
  0 ;						' (case,) plugin-of case,

: (of,) ( n -- x1 n )
  1+ >r 
  compile over compile = 
  if, compile drop r> ;				' (of,) plugin-of of,

: (endof,) ( x1 n -- x2 n )
  >r 1 ncontrols? else, r> ;			' (endof,) plugin-of endof,

: (endcase,) ( x1 .. xn n -- )
  compile drop 0 ?DO 1 ncontrols? then, LOOP ;	' (endcase,) plugin-of endcase,

>TARGET
: UValue T (UValue) H ;

Cond: AHEAD     ahead, ;Cond
Cond: IF        if,  ;Cond
Cond: ?dup-IF   ?dup-if,  ;Cond
Cond: THEN      1 ncontrols? then, ;Cond
Cond: ENDIF     1 ncontrols? then, ;Cond
Cond: ELSE      1 ncontrols? else, ;Cond

Cond: BEGIN     begin, ;Cond
Cond: WHILE     1 ncontrols? while, ;Cond
Cond: AGAIN     1 ncontrols? again, ;Cond
Cond: UNTIL     1 ncontrols? until, ;Cond
Cond: REPEAT    2 ncontrols? repeat, ;Cond

Cond: CASE      case, ;Cond
Cond: OF        of, ;Cond
Cond: ENDOF     endof, ;Cond
Cond: ENDCASE   endcase, ;Cond

\ Structural Conditionals                              12dec92py

: (do,) ( -- target-addr )
  \ ?? i think 0 is too much! jaw
    0 compile (do)
    branchtomark,  2 to1 ;			' (do,) plugin-of do,

\ alternative for if no ?do
\ : (do,)
\     compile 2dup compile = compile IF
\     compile 2drop compile ELSE
\     compile (do) branchtomark, 2 to1 ;
    
: (?do,) ( -- target-addr )
    0 compile (?do)  ?domark, (leave)
    branchtomark,  2 to1 ;			' (?do,) plugin-of ?do,

: (+do,) ( -- target-addr )
    0 compile (+do)  ?domark, (leave)
    branchtomark,  2 to1 ;			' (+do,) plugin-of +do,

: (-do,) ( -- target-addr )
    0 compile (-do)  ?domark, (leave)
    branchtomark,  2 to1 ;			' (-do,) plugin-of -do,

: (u-do,) ( -- target-addr )
    0 compile (u-do)  ?domark, (leave)
    branchtomark,  2 to1 ;			' (u-do,) plugin-of u-do,

: (for,) ( -- target-addr )
  compile (for) branchtomark, ;			' (for,) plugin-of for,

: (loop,) ( target-addr -- )
  1to compile (loop)  loop] 
  compile unloop skiploop] ;			' (loop,) plugin-of loop,

: (+loop,) ( target-addr -- )
  1to compile (+loop)  loop] 
  compile unloop skiploop] ;			' (+loop,) plugin-of +loop,

: (-loop,) ( target-addr -- )
  1to compile (-loop)  loop] 
  compile unloop skiploop] ;			' (-loop,) plugin-of -loop,

: (next,) 
  compile (next)  loop] compile unloop ;	' (next,) plugin-of next,

Cond: DO      	do, ;Cond
Cond: ?DO     	?do, ;Cond
Cond: +DO     	+do, ;Cond
Cond: -DO     	-do, ;Cond
Cond: U-DO     	u-do, ;Cond
Cond: FOR	for, ;Cond

Cond: LOOP	1 ncontrols? loop, ;Cond
Cond: +LOOP	1 ncontrols? +loop, ;Cond
Cond: -LOOP	1 ncontrols? -loop, ;Cond
Cond: NEXT	1 ncontrols? next, ;Cond

\ String words                                         23feb93py

: ,"            '"' parse ht-string, X align ;

X has? control-rack [IF]
Cond: ."        compile (.")     T ," H ;Cond
Cond: S"        compile (S")     T ," H ;Cond
Cond: C"        compile (C")     T ," H ;Cond
Cond: ABORT"    compile (ABORT") T ," H ;Cond
[ELSE]
Cond: ."        '" parse tuck 2>r ahead, there 2r> ht-mem, X align
                >r then, r> compile ALiteral compile Literal compile type ;Cond
Cond: S"        '" parse tuck 2>r ahead, there 2r> ht-mem, X align
                >r then, r> compile ALiteral compile Literal ;Cond
Cond: C"        ahead, there '"' parse ht-string, X align
                >r then, r> compile ALiteral ;Cond
Cond: ABORT"    if, ahead, there '"' parse ht-string, X align
                >r then, r> compile ALiteral compile c(abort") then, ;Cond
Cond: WARNING"  if, ahead, there '"' parse ht-string, X align
                >r then, r> compile ALiteral compile c(warning") then, ;Cond
[THEN]

X has? rom [IF]
Cond: IS        cross-record-name T ' >body @ H compile ALiteral compile ! ;Cond
: IS            cross-record-name T >address ' >body @ ! H ;
Cond: TO        T ' >body @ H compile ALiteral compile ! ;Cond
: TO            T ' >body @ ! H ;
Cond: CTO       T ' >body H compile ALiteral compile ! ;Cond
: CTO           T ' >body ! H ;
[ELSE]
Cond: IS        cross-record-name T ' >body H compile ALiteral compile ! ;Cond
: IS            cross-record-name T >address ' >body ! H ;
Cond: TO        T ' >body H compile ALiteral compile ! ;Cond
: TO            T ' >body ! H ;
Cond: UTO       compile up@ compile lit+ T ' >body @ , H compile ! ;Cond
Cond: UADDR     compile up@ compile lit+ T ' >body @ , H ;Cond
: UADDR         T ' >body @ H tup@ + ;
: UTO           UADDR X ! ;
[THEN]

Cond: defers	T ' >body @ compile, H ;Cond

\ LINKED ERR" ENV" 2ENV"                                18may93jaw

\ linked list primitive
: linked        X here over X @ X A, swap X ! ;
: chained	T linked A, H ;

: err"   s" ErrLink linked" evaluate T , H
         '"' parse ht-string, X align ;

: env"  '"' parse s" EnvLink linked" evaluate
        ht-string, X align X , ;

: 2env" '"' parse s" EnvLink linked" evaluate
        here >r ht-string, X align X , X ,
        r> dup T c@ H 80 and swap T c! H ;

\ compile must be last                                 22feb93py

Cond: [compile] ( -- ) \ name
\g For immediate words, works even if forward reference
      bl-word gfind 0= ABORT" CROSS: Can't compile"
      (gexecute) ;Cond
	   
Cond: postpone ( -- ) \ name
      bl-word gfind 0= ABORT" CROSS: Can't compile"
      dup >magic @ <fwd> =
      ABORT" CROSS: Can't postpone on forward declaration"
      dup >magic @ <imm> =
      IF   (gexecute)
      ELSE >link @ alit, compile compile,  THEN ;Cond

\ save-cross                                           17mar93py

hex

>CROSS
Create magic  s" Gforth6x" here over allot swap move

bigendian 1+ \ strangely, in magic big=0, little=1
tcell 1 = 0 and or
tcell 2 = 2 and or
tcell 4 = 4 and or
tcell 8 = 6 and or
tchar 1 = 00 and or
tchar 2 = 28 and or
tchar 4 = 50 and or
tchar 8 = 78 and or
magic 7 + c!

: save-cross ( "image-name" "binary-name" -- )
  .regions \  s" ec" X $has? IF  .regions  THEN
  bl parse ." Saving to " 2dup type cr
  w/o bin create-file throw >r
  s" header" X $has? IF
      s" #! "           r@ write-file throw
      bl parse          r@ write-file throw
      s"  --image-file" r@ write-file throw
      #lf       r@ emit-file throw
      r@ dup file-position throw drop 8 mod 8 swap ( file-id limit index )
      ?do
	  bl over emit-file throw
      loop
      drop
      magic 8       r@ write-file throw \ write magic
  ELSE
      bl parse 2drop
  THEN
  >rom dictionary >rmem @ there
  s" rom" X $has? IF  dictionary >rstart @ -  THEN
  r@ write-file throw \ write image
  s" relocate" X $has? IF
      dictionary >rbm @ there 1- tcell>bit rshift 1+
                r@ write-file throw \ write tags
  THEN
  r> close-file throw ;

: save-region ( addr len -- )
  bl parse w/o bin create-file throw >r
  swap >image swap r@ write-file throw
  r> close-file throw ;

\ save-asm-region                         		29aug01jaw

Variable name-ptr
Create name-buf 200 chars allot
: init-name-buf name-buf name-ptr ! ;
: nb, name-ptr @ c! 1 chars name-ptr +! ;
: $nb, ( adr len -- ) bounds ?DO I c@ nb, LOOP ;
: @nb name-ptr @ name-buf tuck - ;

\ stores a usefull string representation of the character
\ in the name buffer
: name-char, ( c -- )
  dup 'a 'z 1+ within IF nb, EXIT THEN
  dup 'A 'Z 1+ within IF $20 + nb, EXIT THEN
  dup '0 '9 1+ within IF nb, EXIT THEN
  CASE '+ OF s" _PLUS" $nb, ENDOF
       '- OF s" _MINUS" $nb, ENDOF
       '* OF s" _STAR" $nb, ENDOF
       '/ OF s" _SLASH" $nb, ENDOF
       '' OF s" _TICK" $nb, ENDOF
       '( OF s" _OPAREN" $nb, ENDOF
       ') OF s" _CPAREN" $nb, ENDOF
       '[ OF s" _OBRACKET" $nb, ENDOF
       '] OF s" _CBRACKET" $nb, ENDOF
       '! OF s" _STORE" $nb, ENDOF
       '@ OF s" _FETCH" $nb, ENDOF
       '> OF s" _GREATER" $nb, ENDOF
       '< OF s" _LESS" $nb, ENDOF
       '= OF s" _EQUAL" $nb, ENDOF
       '# OF s" _HASH" $nb, ENDOF
       '? OF s" _QUEST" $nb, ENDOF
       ': OF s" _COL" $nb, ENDOF
       '; OF s" _SEMICOL" $nb, ENDOF
       ', OF s" _COMMA" $nb, ENDOF
       '. OF s" _DOT" $nb, ENDOF
       '" OF s" _DQUOT" $nb, ENDOF
       dup 
       base @ >r hex s>d <# #s 'X hold '_ hold #> $nb, r> base !
  ENDCASE ;
 
: label-from-ghostname ( ghost -- addr len )
  dup >ghostname init-name-buf 'L nb, bounds 
  ?DO I c@ name-char, LOOP 
  \ we add the address to a name to make it unique
  \ because one name may appear more then once
  \ there are names (e.g. boot) that may be reference from other
  \ assembler source files, so we declare them as unique
  \ and don't add the address suffix
  dup >ghost-flags @ <unique> and 0= 
  IF   s" __" $nb, >link @ base @ >r hex 0 <# #s 'L hold #> r> base ! $nb, 
  ELSE drop 
  THEN
  @nb ;

\ FIXME why disabled?!
: label-from-ghostnameXX ( ghost -- addr len )
\ same as (label-from-ghostname) but caches generated names
  dup >asm-name @ ?dup IF nip count EXIT THEN
 \ dup >r (label-from-ghostname) 2dup
  align here >r string, align
  r> r> >asm-name ! ;

: primghostdiscover ( xt -- ghost true | xt false )
  dup 0= IF false EXIT THEN
  >r last-prim-ghost
  BEGIN @ dup
  WHILE dup >asm-dummyaddr @ r@ =
        IF rdrop true EXIT THEN
  REPEAT
  drop r> false ;

: gdiscover2 ( xt -- ghost true | xt false ) 
  dup taddr>region 0= IF false EXIT THEN
  dup (>regiontype) @ dup 0= IF drop false EXIT THEN
  addr-xt-ghost @ dup 0= IF drop false EXIT THEN
  nip true ;
\  dup >ghost-name @ IF nip true ELSE drop false THEN ;

\ generates a label name for the target address
: generate-label-name ( taddr -- addr len )
  gdiscover2
  IF dup >magic @ <do:> =
     IF >asm-name @ count EXIT THEN
     label-from-ghostname
  ELSE
     primghostdiscover
     IF   >asm-name @ count 
     ELSE base @ >r hex 0 <# #s 'L hold #> r> base !
     THEN
  THEN ;

Variable outfile-fd

: $out ( adr len -- ) outfile-fd @ write-file throw  ;
: nlout newline $out ;
: .ux ( n -- ) 
  base @ hex swap 0 <# #S #> $out base ! ;

: save-asm-region-part-aligned ( taddr len -- 'taddr 'len )
  dup cell/ 0 
  ?DO nlout s"    .word " $out over @relbit 
      IF   over X @ generate-label-name $out
      ELSE over X @ s" 0x0" $out .ux
      THEN
      tcell /string
  LOOP ;

: print-bytes ( taddr len n -- taddr' len' )
  over min dup 0> 
  IF   nlout s"    .byte " $out 0 
       ?DO  I 0> IF s" , " $out THEN
            over X c@ s" 0x0" $out .ux 1 /string 
       LOOP 
  THEN ;

: save-asm-region-part ( addr len -- )
  over dup X aligned swap - ?dup
  IF   print-bytes THEN
  save-asm-region-part-aligned
  dup dup X aligned swap - ?dup
  IF   2 pick @relbit ABORT" relocated field splitted"
       print-bytes
  THEN
  2drop ;

: print-label ( taddr -- )
  nlout generate-label-name $out s" :" $out ;

: snl-calc ( taddr taddr2 -- )
  tuck over - ;

: skip-nolables ( taddr -- taddr2 taddr len )
\G skips memory region where no lables are defined
\G starting from taddr+1
\G Labels will be introduced for each reference mark
\G in addr-refs.
\G This word deals with lables at byte addresses as well.
\G The main idea is to have an intro part which
\G skips until the next cell boundary, the middle part
\G which skips whole cells very efficiently and the third
\G part which skips the bytes to the label in a cell
  dup 1+ dup (>regiontype) 
  ( S taddr taddr-realstart type-addr )
  dup @ dup IF addr-refs @ THEN
  swap >r
  over align+ tuck tcell swap - rshift swap 0
  ?DO dup 1 and 
     IF drop rdrop snl-calc UNLOOP EXIT THEN 
     2/ 1 under+ 
  LOOP
  drop r> cell+
  ( S .. taddr2 type-addr ) dup
  BEGIN dup @ dup IF addr-refs @ THEN 0= WHILE cell+ REPEAT
  dup >r swap - 1 cells / tcell * + r>
  ( S .. taddr2+skiplencells type-addr )
  @ addr-refs @ 1 tcell lshift or
  BEGIN dup 1 and 0= WHILE 1 under+ 2/ REPEAT drop
  ( S .. taddr2+skiplencells+skiplenbytes )
  snl-calc ;

: insert-label ( taddr -- )
  dup 0= IF drop EXIT THEN
  \ ignore everything which points outside our memory regions
  \ maybe a primitive pointer or whatever
  dup taddr>region 0= IF drop EXIT THEN
  dup >r (>regiontype) define-addr-struct addr-refs dup @ 
  r> tcell 1- and 1 swap lshift or swap ! ;

\ this generates a sorted list of addresses which must be labels
\ it scans therefore a whole region
: generate-label-list-region ( taddr len -- )
  BEGIN over @relbit IF over X @ insert-label THEN
        tcell /string dup 0< 
  UNTIL 2drop ;

: generate-label-list ( -- )
  region-link
  BEGIN @ dup WHILE 
        dup 0 >rlink - extent 
        ?dup IF generate-label-list-region ELSE drop THEN
  REPEAT drop ;

: create-outfile ( addr len -- )
  w/o bin create-file throw outfile-fd ! ;

: close-outfile ( -- )
  outfile-fd @ close-file throw ;

: (save-asm-region) ( region -- )
  \ ." label list..."
  generate-label-list
  \ ." ok!" cr
  extent ( S taddr len )
  over insert-label
  2dup + dup insert-label >r ( R end-label )
  ( S taddr len ) drop
  BEGIN
     dup print-label
     dup r@ <> WHILE
     skip-nolables save-asm-region-part
  REPEAT drop rdrop ;

: lineout ( addr len -- )
  outfile-fd @ write-line throw ;  

: save-asm-region ( region adr len -- )
  create-outfile (save-asm-region) close-outfile ;

\ \ minimal definitions
	   
: upcase ( addr u -- )
    bounds ?DO I c@ toupper I c! LOOP ;

>MINIMAL also minimal

\ Useful words                                        13feb93py

: KB  400 * ;

[IFDEF] #loc
    ' #loc alias #loc
[ELSE]
    false Value warned?
    : #loc 2drop parse-name 2drop
	warned? IF  ." #loc not supported" cr true to warned?  THEN ;
[THEN]

\ \ [IF] [ELSE] [THEN] ...				14sep97jaw

\ it is useful to define our own structures and not to rely
\ on the words in the host system
\ The words in the host system might be defined with vocabularies
\ this doesn't work with our self-made compile-loop

: [ELSE]
    1 BEGIN
	BEGIN parse-name dup WHILE
	    comment? 20 umin 2dup upcase
	    2dup s" [IF]" str= >r 
	    2dup s" [IFUNDEF]" str= >r
	    2dup s" [IFDEF]" str= r> or r> or
	    IF   2drop 1+
	    ELSE 2dup s" [ELSE]" str=
		IF   2drop 1- dup
		    IF 1+
		    THEN
		ELSE
		    2dup s" [ENDIF]" str= >r
		    s" [THEN]" str= r> or
		    IF 1- THEN
		THEN
	    THEN
	    ?dup 0= ?EXIT
	REPEAT
	2drop refill 0=
    UNTIL drop ; immediate
  
: [THEN] ( -- ) ; immediate

: [ENDIF] ( -- ) ; immediate

: [IF] ( flag -- )
    0= IF postpone [ELSE] THEN ; immediate 

Cond: [IF]      postpone [IF] ;Cond
Cond: [THEN]    postpone [THEN] ;Cond
Cond: [ELSE]    postpone [ELSE] ;Cond

\ define new [IFDEF] and [IFUNDEF]                      20may93jaw

: defined? tdefined? ;
: needed? needed? ;
: doer? doer? ;

\ we want to use IFDEF on compiler directives (e.g. E?) in the source, too

: directive? 
  parse-name [ ' target >wordlist ] literal search-wordlist 
  dup IF nip THEN ;

: [IFDEF]  >in @ directive? swap >in !
	   0= IF tdefined? ELSE parse-name 2drop true THEN
	   postpone [IF] ;

: [IFUNDEF] tdefined? 0= postpone [IF] ;

Cond: [IFDEF]   postpone [IFDEF] ;Cond

Cond: [IFUNDEF] postpone [IFUNDEF] ;Cond

\ C: \- \+ Conditional Compiling                         09jun93jaw

: C: >in @ tdefined? 0=
     IF    >in ! X :
     ELSE drop
        BEGIN bl-word dup c@
              IF   count comment? s" ;" str= ?EXIT
              ELSE refill 0= ABORT" CROSS: Out of Input while C:"
              THEN
        AGAIN
     THEN ;

: d? d? ;

: \D ( -- "debugswitch" ) 
\G doesn't skip line when debug switch is on
    D? 0= IF postpone \ THEN ;

: \- ( -- "wordname" )
\G interprets the line if word is not defined
   tdefined? IF postpone \ THEN ;

: \+ ( -- "wordname" )
\G interprets the line if word is defined
   tdefined? 0= IF postpone \ THEN ;

: \? ( -- "envorinstring" )
\G Skip line if environmental variable evaluates to false
   X has? 0= IF postpone \ THEN ;

Cond: \- \- ;Cond
Cond: \+ \+ ;Cond
Cond: \D \D ;Cond
Cond: \? \? ;Cond

: ?? bl-word find IF execute ELSE drop 0 THEN ;

: needed:
\G defines ghost for words that we want to be compiled
  BEGIN >in @ bl-word c@ WHILE >in ! Ghost drop REPEAT drop ;

\ words that should be in minimal

[IFUNDEF] save-mem
    create s-buffer 50 chars allot
    : save-mem  s-buffer place s-buffer count ;
[THEN]

bigendian Constant bigendian

: here there ;
: equ constant ;
: mark there constant ;

\ compiler directives
: >ram >ram ;
: >rom >rom ;
: >auto >auto ;
: >tempdp >tempdp ;
: tempdp> tempdp> ;
: const constflag on ;

: Redefinitions-start
\G Starts a redefinition section. Warnings are disabled and
\G existing ghosts are reused. This is used in the kernel
\G where ( and \ and the like are redefined
  twarnings off warnings off reuse-ghosts on ;

: Redefinitions-end
\G Ends a redefinition section. Warnings are enabled again.
  twarnings on warnings on reuse-ghosts off ;

: warnings parse-name 3 = 
  IF twarnings off warnings off ELSE twarnings on warnings on THEN drop ;

: | ;
\ : | NoHeaderFlag on ; \ This is broken (damages the last word)

: save-cross save-cross ;
: save-region save-region ;
: tdump swap >image swap dump ;

also forth 
[IFDEF] Label           : Label defempty? Label ; [THEN] 
[IFDEF] start-macros    : start-macros defempty? start-macros ; [THEN]
\ [IFDEF] builttag	: builttag builttag ;	[THEN]
previous

: s" '"' parse save-mem ; \ for environment?
: + + ;
: - - ;
: d+ d+ ;
: d- d- ;
: 1+ 1 + ;
: 2+ 2 + ;
: 1- 1- ;
: and and ;
: or or ;
: xor xor ;
: 2* 2* ;
: * * ;
: / / ;
: dup dup ;
: ?dup ?dup ;
: over over ;
: swap swap ;
: rot rot ;
: drop drop ;
: 2drop 2drop ;
: =   = ;
: <>  <> ;
: 0=   0= ;
: lshift lshift ;
[IFDEF] dlshift
    : dlshift dlshift ;
[ELSE]
    : dlshift 0 ?DO 2dup d+ LOOP ;
[THEN]
: 2/ 2/ ;
: hex. base @ $10 base ! swap . base ! ;
: invert invert ;
: linkstring ( addr u n addr -- )
    X here over X @ X , swap X ! X , ht-string, X align ;
: %size nip ;
: %alignment drop ;
\ : . . ;

: all-words    ['] forced?    IS skip? ;
: needed-words ['] needed?  IS skip? ;
: undef-words  ['] defined2? IS skip? ;
: skipdef skipdef ;

: \  postpone \ ;  immediate
: \G T-\G ; immediate
: (  postpone ( ;  immediate
: include parse-name included ;
: included swap >image swap included ;
: require require ;
: needs require ;
: .( ')' parse type ;
: ERROR" '"' parse 
  rot 
  IF cr ." *** " type ."  ***" -1 ABORT" CROSS: Target error, see text above" 
  ELSE 2drop 
  THEN ;
: ." '"' parse type ;
: cr cr ;

: c,s 0 ?DO dup X c, LOOP drop ; \ used for space table creation

: set-optimizer set-optimizer ;

\ only forth also cross also minimal definitions order

\ cross-compiler words

: decimal       decimal [g'] decimal >exec2 @ ?dup IF EXECUTE THEN ;
: hex           hex [g'] hex >exec2 @ ?dup IF EXECUTE THEN ;

\ : tudp          X tudp ;
\ : tup           X tup ;

: doc-off       false to-doc ! ;
: doc-on        true  to-doc ! ;

: declareunique ( "name" -- )
\ Sets the unique flag for a ghost. The assembler output
\ generates labels with the ghostname concatenated with the address
\ while cross-compiling. The address is concatenated
\ because we have double occurrences of the same name.
\ If we want to reference the labels from the assembler or C
\ code we declare them unique, so the address is skipped.
  Ghost >ghost-flags dup @ <unique> or swap ! ;

\ [IFDEF] dbg : dbg dbg ; [THEN]

\ for debugging...
\ : dbg dbg ;
: horder         order ;
: hwords        words ;
\ : words 	also ghosts 
\                words previous ;
: .s            .s ;
: depth         depth ;
: bye           bye ;

\ dummy

\ turnkey direction
: H forth ; immediate
: T minimal ; immediate
: G ghosts ; immediate


\ these ones are pefered:

: unlock previous forth also cross ;

\ also minimal
>cross

: turnkey 
   ghosts-wordlist 1 set-order
   also target definitions
   also Minimal also ;

>minimal

: [[+++
  turnkey unlock ;

unlock definitions also minimal

: lock   turnkey ;

Defer +++]]-hook
: +++]] +++]]-hook lock ;

LOCK
\ load cross compiler extension defined in mach file

UNLOCK >CROSS

[IFDEF] extend-cross extend-cross [THEN]

LOCK
