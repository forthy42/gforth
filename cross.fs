\ CROSS.FS     The Cross-Compiler                      06oct92py
\ Idea and implementation: Bernd Paysan (py)

\ Copyright (C) 1995,1996,1997,1998,1999 Free Software Foundation, Inc.

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

0 
[IF]

ToDo:
Crossdoc destination ./doc/crossdoc.fd makes no sense when
cross.fs is uses seperately. jaw
Do we need this char translation with >address and in branchoffset? 
(>body also affected) jaw
Clean up mark> and >resolve stuff jaw

[THEN]


hex     \ the defualt base for the cross-compiler is hex !!
Warnings off

\ words that are generaly useful

: KB  400 * ;
: >wordlist ( vocabulary-xt -- wordlist-struct )
  also execute get-order swap >r 1- set-order r> ;

: umax 2dup u< IF swap THEN drop ;
: umin 2dup u> IF swap THEN drop ;

: string, ( c-addr u -- )
    \ puts down string as cstring
    dup c, here swap chars dup allot move ;

: SetValue ( n -- <name> )
\G Same behaviour as "Value" if the <name> is not defined
\G Same behaviour as "to" if <name> is defined
\G SetValue searches in the current vocabulary
  save-input bl word >r restore-input throw r> count
  get-current search-wordlist
  IF	drop >r
	\ we have to set current to be topmost context wordlist
	get-order get-order get-current swap 1+ set-order
	r> ['] to execute
	set-order order
  ELSE Value THEN ;

: DefaultValue ( n -- <name> )
\G Same behaviour as "Value" if the <name> is not defined
\G DefaultValue searches in the current vocabulary
 save-input bl word >r restore-input throw r> count
 get-current search-wordlist
 IF bl word drop 2drop ELSE Value THEN ;

hex

Vocabulary Cross
Vocabulary Target
Vocabulary Ghosts
VOCABULARY Minimal
only Forth also Target also also
definitions Forth

: T  previous Cross also Target ; immediate
: G  Ghosts ; immediate
: H  previous Forth also Cross ; immediate

forth definitions

: T  previous Cross also Target ; immediate
: G  Ghosts ; immediate

: >cross  also Cross definitions previous ;
: >target also Target definitions previous ;
: >minimal also Minimal definitions previous ;

H

>CROSS

\ 1 Constant Cross-Flag	\ to check whether assembler compiler plug-ins are
			\ for cross-compiling
\ No! we use "[IFUNDEF]" there to find out whether we are target compiling!!!

: comment? ( c-addr u -- c-addr u )
        2dup s" (" compare 0=
        IF    postpone (
        ELSE  2dup s" \" compare 0= IF postpone \ THEN
        THEN ;

\ Begin CROSS COMPILER:



\ \ --------------------        Error Handling                  05aug97jaw

\ Flags

also forth definitions  \ these values may be predefined before
                        \ the cross-compiler is loaded

false DefaultValue stack-warn   	 \ check on empty stack at any definition
false DefaultValue create-forward-warn   \ warn on forward declaration of created words

[IFUNDEF] DebugMaskSrouce Variable DebugMaskSource 0 DebugMaskSource ! [THEN]
[IFUNDEF] DebugMaskCross  Variable DebugMaskCross  0 DebugMaskCross  ! [THEN]

previous >CROSS

: .dec
  base @ decimal swap . base ! ;

: .sourcepos
  cr sourcefilename type ." :"
  sourceline# .dec ;

: warnhead
\G display error-message head
\G perhaps with linenumber and filename
  .sourcepos ." Warning: " ;

: empty? depth IF .sourcepos ." Stack not empty!"  THEN ;

stack-warn [IF]
: defempty? empty? ;
[ELSE]
: defempty? ; immediate
[THEN]



\ \ GhostNames Ghosts                                  9may93jaw

\ second name source to search trough list

VARIABLE GhostNames
0 GhostNames !

: GhostName ( -- addr )
    here GhostNames @ , GhostNames ! here 0 ,
    bl word count
    \ 2dup type space
    string, \ !! cfalign ?
    align ;

\ Ghost Builder                                        06oct92py

\ <T T> new version with temp variable                 10may93jaw

VARIABLE VocTemp

: <T  get-current VocTemp ! also Ghosts definitions ;
: T>  previous VocTemp @ set-current ;

hex
4711 Constant <fwd>             4712 Constant <res>
4713 Constant <imm>             4714 Constant <do:>

\ iForth makes only immediate directly after create
\ make atonce trick! ?

Variable atonce atonce off

: NoExec true ABORT" CROSS: Don't execute ghost" ;

: GhostHeader <fwd> , 0 , ['] NoExec , ;

: >magic ;		\ type of ghost
: >link cell+ ;		\ pointer where ghost is in target, or if unresolved
			\ points to the where we have to resolve (linked-list)
: >exec cell+ cell+ ;	\ execution symantics (while target compiling) of ghost
: >end 3 cells + ;	\ room for additional tags
			\ for builder (create, variable...) words the
			\ execution symantics of words built are placed here

Variable executed-ghost \ last executed ghost, needed in tcreate and gdoes>
Variable last-ghost	\ last ghost that is created
Variable last-header-ghost \ last ghost definitions with header

: Make-Ghost ( "name" -- ghost )
  >in @ GhostName swap >in !
  <T Create atonce @ IF immediate atonce off THEN
  here tuck swap ! ghostheader T>
  dup last-ghost !
  DOES> dup executed-ghost ! >exec @ execute ;

\ ghost words                                          14oct92py
\                                          changed:    10may93py/jaw

: gfind   ( string -- ghost true/1 / string false )
\ searches for string in word-list ghosts
  dup count [ ' ghosts >wordlist ] ALiteral search-wordlist
  dup IF >r >body nip r>  THEN ;

: gdiscover ( xt -- ghost true | xt false )
  GhostNames
  BEGIN @ dup
  WHILE 2dup
        cell+ @ dup >magic @ <fwd> <>
        >r >link @ = r> and
        IF cell+ @ nip true EXIT THEN
  REPEAT
  drop false ;

VARIABLE Already

: ghost   ( "name" -- ghost )
  Already off
  >in @  bl word gfind   IF  Already on nip EXIT  THEN
  drop  >in !  Make-Ghost ;

: >ghostname ( ghost -- adr len )
  GhostNames
  BEGIN @ dup
  WHILE 2dup cell+ @ =
  UNTIL nip 2 cells + count
  ELSE  2drop 
	\ true abort" CROSS: Ghostnames inconsistent"
	s" ?!?!?!"
  THEN ;

' >ghostname ALIAS @name

: forward? ( ghost -- flag )
  >magic @ <fwd> = ;

\ Predefined ghosts                                    12dec92py

ghost 0=                                        drop
ghost branch    ghost ?branch                   2drop
ghost (do)      ghost (?do)                     2drop
ghost (for)                                     drop
ghost (loop)    ghost (+loop)                   2drop
ghost (next)                                    drop
ghost unloop    ghost ;S                        2drop
ghost lit       ghost (compile) ghost !         2drop drop
ghost (does>)   ghost noop                      2drop
ghost (.")      ghost (S")      ghost (ABORT")  2drop drop
ghost '                                         drop
ghost :docol    ghost :doesjump ghost :dodoes   2drop drop
ghost :dovar					drop
ghost over      ghost =         ghost drop      2drop drop
ghost - drop
ghost 2drop drop
ghost 2dup drop

\ \ Parameter for target systems                         06oct92py

\ we define it ans like...
wordlist Constant target-environment

VARIABLE env-current \ save information of current dictionary to restore with environ>

: >ENVIRON get-current env-current ! target-environment set-current ;
: ENVIRON> env-current @ set-current ; 

>TARGET

: environment? ( adr len -- [ x ] true | false )
  target-environment search-wordlist 
  IF execute true ELSE false THEN ;

: e? bl word count T environment? H 0= ABORT" environment variable not defined!" ;

: has? 	bl word count T environment? H 
	IF 	\ environment variable is present, return its value
	ELSE	\ environment variable is not present, return false
		\ !! JAW abort is just for testing
		false true ABORT" arg" 
	THEN ;

: $has? T environment? H IF ELSE false THEN ;

>ENVIRON get-order get-current swap 1+ set-order
true SetValue compiler
true  SetValue cross
true SetValue standard-threading
>TARGET previous


mach-file count included hex

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
[THEN]

true DefaultValue interpreter
true DefaultValue ITC
false DefaultValue rom

>TARGET
s" relocate" T environment? H 
[IF]	SetValue NIL
[ELSE]	>ENVIRON T NIL H SetValue relocate
[THEN]

>CROSS

\ \ Create additional parameters                         19jan95py

\ currently cross only works for host machines with address-unit-bits
\ eual to 8 because of s! and sc!
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
\ this sets byte size for the target machine, an (probably right guess) jaw

T
NIL		   	Constant TNIL
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
tbits/byte bits/byte /	Constant tbyte


\ Variables                                            06oct92py

Variable image
Variable tlast    TNIL tlast !  \ Last name field
Variable tlastcfa \ Last code field
Variable tdoes    \ Resolve does> calls
Variable bit$

\ statistics						10jun97jaw

Variable headers-named 0 headers-named !
Variable user-vars 0 user-vars !

\ Memory initialisation                                05dec92py

[IFDEF] Memory \ Memory is a bigFORTH feature
   also Memory
   : initmem ( var len -- )
     2dup swap handle! >r @ r> erase ;
   toss
[ELSE]
   : initmem ( var len -- )
     tuck allocate abort" CROSS: No memory for target"
     ( len var adr ) dup rot !
     ( len adr ) swap erase ;
[THEN]

\ MakeKernal                                           12dec92py

: makekernel ( targetsize -- targetsize )
  bit$  over 1- tcell>bit rshift 1+ initmem
  image over initmem ;

>MINIMAL
: makekernel makekernel ;
>CROSS

: target>bitmask-size ( u1 -- u2 )
  1- tcell>bit rshift 1+ ;

: allocatetarget ( size --- adr )
  dup allocate ABORT" CROSS: No memory for target"
  swap over swap erase ;



\ \ memregion.fs


Variable last-defined-region    \ pointer to last defined region
Variable region-link            \ linked list with all regions
Variable mirrored-link          \ linked list for mirrored regions
0 dup mirrored-link ! region-link !


: >rname 6 cells + ;
: >rbm   5 cells + ;
: >rmem  4 cells + ;
: >rlink 3 cells + ;
: >rdp 2 cells + ;
: >rlen cell+ ;
: >rstart ;


: region ( addr len -- )                \G create a new region
  \ check whether predefined region exists 
  save-input bl word find >r >r restore-input throw r> r> 0= 
  IF	\ make region
	drop
	save-input create restore-input throw
	here last-defined-region !
	over ( startaddr ) , ( length ) , ( dp ) ,
	region-link linked 0 , 0 , bl word count string,
  ELSE	\ store new parameters in region
        bl word drop
	>body >r r@ last-defined-region !
	r@ >rlen ! dup r@ >rstart ! r> >rdp !
  THEN ;

: borders ( region -- startaddr endaddr ) \G returns lower and upper region border
  dup >rstart @ swap >rlen @ over + ;

: extent  ( region -- startaddr len )   \G returns the really used area
  dup >rstart @ swap >rdp @ over - ;

: area ( region -- startaddr totallen ) \G returns the total area
  dup >rstart swap >rlen @ ;

: mirrored                              \G mark a region as mirrored
  mirrored-link
  align linked last-defined-region @ , ;

: .addr ( u -- )
\G prints a 16 or 32 Bit nice hex value
  base @ >r hex
  tcell 2 u>
  IF s>d <# # # # # '. hold # # # # #> type
  ELSE s>d <# # # # # # #> type
  THEN r> base ! ;

: .regions                      \G display region statistic

  \ we want to list the regions in the right order
  \ so first collect all regions on stack
  0 region-link @
  BEGIN dup WHILE dup @ REPEAT drop
  BEGIN dup
  WHILE cr
        0 >rlink - >r
        r@ >rname count tuck type
        12 swap - 0 max spaces space
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


: setup-target ( -- )   \G initialize targets memory space
  s" rom" T $has? H
  IF  \ check for ram and rom...
      address-space area nip 0<>
      ram-dictionary area nip 0<>
      rom-dictionary area nip 0<>
      and and 0=
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
        0 >rlink - >r
        r@ >rlen @
        IF      \ allocate mem
                r@ >rlen @ dup

                allocatetarget dup image !
                r@ >rmem !

                target>bitmask-size allocatetarget
                dup
                bit$ !
                r> >rbm !

        ELSE    r> drop THEN
   REPEAT ;

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


0 value tdp
variable fixed		\ flag: true: no automatic switching
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
  >rdp to tdp ;

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
   : S!  ( n addr -- )  >r s>d r> tcell bounds swap 1-
     DO  maxbyte ud/mod rot I c!  -1 +LOOP  2drop ;
   : S@  ( addr -- n )  >r 0 0 r> tcell bounds
     DO  maxbyte * swap maxbyte um* rot + swap I c@ + swap  LOOP d>s ;
   : Sc!  ( n addr -- )  >r s>d r> tchar bounds swap 1-
     DO  maxbyte ud/mod rot I c!  -1 +LOOP  2drop ;
   : Sc@  ( addr -- n )  >r 0 0 r> tchar bounds
     DO  maxbyte * swap maxbyte um* rot + swap I c@ + swap  LOOP d>s ;
[ELSE]
   : S!  ( n addr -- )  >r s>d r> tcell bounds
     DO  maxbyte ud/mod rot I c!  LOOP  2drop ;
   : S@  ( addr -- n )  >r 0 0 r> tcell bounds swap 1-
     DO  maxbyte * swap maxbyte um* rot + swap I c@ + swap  -1 +LOOP d>s ;
   : Sc!  ( n addr -- )  >r s>d r> tchar bounds
     DO  maxbyte ud/mod rot I c!  LOOP  2drop ;
   : Sc@  ( addr -- n )  >r 0 0 r> tchar bounds swap 1-
     DO  maxbyte * swap maxbyte um* rot + swap I c@ + swap  -1 +LOOP d>s ;
[THEN]

: taddr>region ( taddr -- region | 0 )
\G finds for a target-address the correct region
\G returns 0 if taddr is not in range of a target memory region
  region-link
  BEGIN @ dup
  WHILE dup >r
        0 >rlink - >r
        r@ >rlen @
        IF      dup r@ borders within
                IF r> r> drop nip EXIT THEN
        THEN
        r> drop
        r>
  REPEAT
  2drop 0 ;

: (>regionimage) ( taddr -- 'taddr )
  dup
  \ find region we want to address
  taddr>region dup 0= ABORT" Address out of range!"
  >r
  \ calculate offset in region
  r@ >rstart @ -
  \ add regions real address in our memory
  r> >rmem @ + ;

\ Bit string manipulation                               06oct92py
\                                                       9may93jaw
CREATE Bittable 80 c, 40 c, 20 c, 10 c, 8 c, 4 c, 2 c, 1 c,
: bits ( n -- n ) chars Bittable + c@ ;

: >bit ( addr n -- c-addr mask ) 8 /mod rot + swap bits ;
: +bit ( addr n -- )  >bit over c@ or swap c! ;
: -bit ( addr n -- )  >bit invert over c@ and swap c! ;

: (relon) ( taddr -- )  bit$ @ swap cell/ +bit ;
: (reloff) ( taddr -- ) bit$ @ swap cell/ -bit ;

: (>image) ( taddr -- absaddr ) image @ + ;

DEFER >image
DEFER relon
DEFER reloff
DEFER correcter

T has? relocate H
[IF]
' (relon) IS relon
' (reloff) IS reloff
' (>image) IS >image
[ELSE]
' drop IS relon
' drop IS reloff
' (>regionimage) IS >image
[THEN]

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
: !  ( w taddr -- )     >image S! ;
: c@ ( taddr -- char )  >image Sc@ ;
: c! ( char taddr -- )  >image Sc! ;
: 2@ ( taddr -- x1 x2 ) T dup cell+ @ swap @ H ;
: 2! ( x1 x2 taddr -- ) T swap over ! cell+ ! H ;

\ Target compilation primitives                        06oct92py
\ included A!                                          16may93jaw

: here  ( -- there )    there ;
: allot ( n -- )        tdp +! ;
: ,     ( w -- )        T here H tcell T allot  ! H T here drop H ;
: c,    ( char -- )     T here    tchar allot c! H ;
: align ( -- )          T here H align+ 0 ?DO  bl T c, tchar H +LOOP ;
: cfalign ( -- )
    T here H cfalign+ 0 ?DO  bl T c, tchar H +LOOP ;

: >address		dup 0>= IF tbyte / THEN ; \ ?? jaw 
: A!                    swap >address swap dup relon T ! H ;
: A,    ( w -- )        >address T here H relon T , H ;

>CROSS

: tcmove ( source dest len -- )
\G cmove in target memory
  tchar * bounds
  ?DO  dup T c@ H I T c! H 1+
  tchar +LOOP  drop ;

\ \ Load Assembler

>TARGET
H also Forth definitions \ ." asm: " order

: X 	also target bl word find
	IF	state @ IF compile,
		ELSE execute THEN
	ELSE	previous ABORT" Cross: access method not supported!"
	THEN 
	previous ; immediate

[IFDEF] asm-include asm-include [THEN] hex

previous
>CROSS H

\ \ --------------------        Compiler Plug Ins               01aug97jaw

\  Compiler States

Variable comp-state
0 Constant interpreting
1 Constant compiling
2 Constant resolving
3 Constant assembling

Defer lit, ( n -- )
Defer alit, ( n -- )

Defer branch, ( target-addr -- )	\ compiles a branch
Defer ?branch, ( target-addr -- )	\ compiles a ?branch
Defer branchmark, ( -- branch-addr )	\ reserves room for a branch
Defer ?branchmark, ( -- branch-addr )	\ reserves room for a ?branch
Defer ?domark, ( -- branch-addr )	\ reserves room for a ?do branch
Defer branchto, ( -- )			\ actual program position is target of a branch (do e.g. alignment)
Defer branchtoresolve, ( branch-addr -- ) \ resolves a forward reference from branchmark
Defer branchfrom, ( -- )		\ ?!
Defer branchtomark, ( -- target-addr )	\ marks a branch destination

Defer colon, ( tcfa -- )		\ compiles call to tcfa at current position
Defer colonmark, ( -- addr )		\ marks a colon call
Defer colon-resolve ( tcfa addr -- )

Defer addr-resolve ( target-addr addr -- )
Defer doer-resolve ( ghost res-pnt target-addr addr -- ghost res-pnt )

Defer do,	( -- do-token )
Defer ?do,	( -- ?do-token )
Defer for,	( -- for-token )
Defer loop,	( do-token / ?do-token -- )
Defer +loop,	( do-token / ?do-token -- )
Defer next,	( for-token )

[IFUNDEF] ca>native
defer ca>native	
[THEN]

>TARGET
DEFER >body             \ we need the system >body
			\ and the target >body
>CROSS
T 2 cells H VALUE xt>body
DEFER doprim,	\ compiles start of a primitive
DEFER docol,   	\ compiles start of a colon definition
DEFER doer,		
DEFER fini,      \ compiles end of definition ;s
DEFER doeshandler,
DEFER dodoes,

DEFER ]comp     \ starts compilation
DEFER comp[     \ ends compilation

: (cc) T a, H ;					' (cc) IS colon,

: (cr) >tempdp ]comp colon, comp[ tempdp> ; 	' (cr) IS colon-resolve
: (ar) T ! H ;					' (ar) IS addr-resolve
: (dr)  ( ghost res-pnt target-addr addr )
	>tempdp drop over 
	dup >magic @ <do:> =
	IF 	doer,
	ELSE	dodoes,
	THEN 
	tempdp> ;				' (dr) IS doer-resolve

: (cm) ( -- addr )
    T here align H
    -1 colon, ;					' (cm) IS colonmark,

>TARGET
: compile, colon, ;
>CROSS

\ file loading

: >fl-id   1 cells + ;
: >fl-name 2 cells + ;

Variable filelist 0 filelist !
Create NoFile ," #load-file#"
0 Value	 filemem
: loadfile  FileMem ?dup IF >fl-name ELSE NoFile THEN ;

1 [IF] \ !! JAW WIP

: add-included-file ( adr len -- )
	dup char+ >fl-name allocate throw >r
	r@ >fl-name place
	filelist @ r@ !
	r> dup filelist ! to FileMem
	;

: included? ( c-addr u -- f )
  	filelist
	BEGIN	@ dup
	WHILE	>r r@ 1 cells + count compare 0=
		IF rdrop 2drop true EXIT THEN
		r>
	REPEAT
	2drop drop false ;	

: included 
\	cr ." Including: " 2dup type ." ..."
	FileMem >r
	2dup add-included-file included 
	r> to FileMem ;

: include bl word count included ;

: require bl word count included ;

[THEN]

\ resolve structure

: >next ;		\ link to next field
: >tag cell+ ;		\ indecates type of reference: 0: call, 1: address, 2: doer
: >taddr cell+ cell+ ;	
: >ghost 3 cells + ;
: >file 4 cells + ;
: >line 5 cells + ;

: (refered) ( ghost addr tag -- )
\G creates a reference to ghost at address taddr
    rot >r here r@ >link @ , r> >link ! 
    ( taddr tag ) ,
    ( taddr ) , 
    last-header-ghost @ , 
    loadfile , 
    sourceline# , 
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
  over cr ." Resolving " >ghostname type dup ."  in " >ghost @ >ghostname type ;

: reswarn-forward ( ghost res-struct -- ghost res-struct )
  over warnhead >ghostname type dup ."  is referenced in " 
  >ghost @ >ghostname type ;

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

Variable TWarnings
TWarnings on
Variable Exists-Warnings
Exists-Warnings on

: exists ( ghost tcfa -- )
  over GhostNames
  BEGIN @ dup
  WHILE 2dup cell+ @ =
  UNTIL
        2 cells + count
        TWarnings @ Exists-Warnings @ and
        IF warnhead type ."  exists"
        ELSE 2drop THEN
        drop swap >link !
  ELSE  true abort" CROSS: Ghostnames inconsistent "
  THEN ;

: resolve  ( ghost tcfa -- )
\G resolve referencies to ghost with tcfa
    \ is ghost resolved?, second resolve means another definition with the
    \ same name
    over forward? 0= IF  exists EXIT THEN
    \ get linked-list
    swap >r r@ >link @ swap \ ( list tcfa R: ghost )
    \ mark ghost as resolved
    dup r@ >link ! <res> r@ >magic !
    \ loop through forward referencies
    r> -rot 
    comp-state @ >r Resolving comp-state !
    resolve-loop 
    r> comp-state !

    ['] noop IS resolve-warning 
  ;

\ gexecute ghost,                                      01nov92py

: is-forward   ( ghost -- )
  colonmark, 0 (refered) ; \ compile space for call

: is-resolved   ( ghost -- )
  >link @ colon, ; \ compile-call

: gexecute   ( ghost -- )
  dup @ <fwd> = IF  is-forward  ELSE  is-resolved  THEN ;

: addr,  ( ghost -- )
  dup @ <fwd> = IF  1 refered 0 T a, H ELSE >link @ T a, H THEN ;

\ !! : ghost,     ghost  gexecute ;

\ .unresolved                                          11may93jaw

variable ResolveFlag

\ ?touched                                             11may93jaw

: ?touched ( ghost -- flag ) dup forward? swap >link @
                               0 <> and ;

: .forwarddefs ( ghost -- )
	."  appeared in:"
	>link
	BEGIN	@ dup
	WHILE	cr 5 spaces
		dup >ghost @ >ghostname type
		."  file " dup >file @ ?dup IF count type ELSE ." CON" THEN
		."  line " dup >line @ .dec
	REPEAT 
	drop ;

: ?resolved  ( ghostname -- )
  dup cell+ @ ?touched
  IF  	dup 
	cell+ cell+ count cr type ResolveFlag on 
	cell+ @ .forwarddefs
  ELSE 	drop 
  THEN ;

>MINIMAL
: .unresolved  ( -- )
  ResolveFlag off cr ." Unresolved: "
  Ghostnames
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

>CROSS
\ Header states                                        12dec92py

: flag! ( 8b -- )   tlast @ dup >r T c@ xor r> c! H ;

VARIABLE ^imm

>TARGET
: immediate     40 flag!
                ^imm @ @ dup <imm> = IF  drop  EXIT  THEN
                <res> <> ABORT" CROSS: Cannot immediate a unresolved word"
                <imm> ^imm @ ! ;
: restrict      20 flag! ;

: isdoer	
\G define a forth word as doer, this makes obviously only sence on
\G forth processors such as the PSC1000
		<do:> last-header-ghost @ >magic ! ;
>CROSS

\ ALIAS2 ansforth conform alias                          9may93jaw

: ALIAS2 create here 0 , DOES> @ execute ;
\ usage:
\ ' <name> alias2 bla !

\ Target Header Creation                               01nov92py

>TARGET
: string,  ( addr count -- )
  dup T c, H bounds  ?DO  I c@ T c, H  LOOP ; 
: name,  ( "name" -- )  bl word count T string, cfalign H ;
: view,   ( -- ) ( dummy ) ;
>CROSS

\ Target Document Creation (goes to crossdoc.fd)       05jul95py

s" ./doc/crossdoc.fd" r/w create-file throw value doc-file-id
\ contains the file-id of the documentation file

: T-\G ( -- )
    source >in @ /string doc-file-id write-line throw
    postpone \ ;

Variable to-doc  to-doc on

: cross-doc-entry  ( -- )
    to-doc @ tlast @ 0<> and	\ not an anonymous (i.e. noname) header
    IF
	s" " doc-file-id write-line throw
	s" make-doc " doc-file-id write-file throw
	tlast @ >image count $1F and doc-file-id write-file throw
	>in @
	[char] ( parse 2drop
	[char] ) parse doc-file-id write-file throw
	s"  )" doc-file-id write-file throw
	[char] \ parse 2drop					
	T-\G
	>in !
    THEN ;

\ Target TAGS creation

s" kernel.TAGS" r/w create-file throw value tag-file-id
\ contains the file-id of the tags file

Create tag-beg 2 c,  7F c, bl c,
Create tag-end 2 c,  bl c, 01 c,
Create tag-bof 1 c,  0C c,

2variable last-loadfilename 0 0 last-loadfilename 2!
	    
: put-load-file-name ( -- )
    loadfilename 2@ last-loadfilename 2@ d<>
    IF
	tag-bof count tag-file-id write-line throw
	sourcefilename 2dup
	tag-file-id write-file throw
	last-loadfilename 2!
	s" ,0" tag-file-id write-line throw
    THEN ;

: cross-tag-entry  ( -- )
    tlast @ 0<>	\ not an anonymous (i.e. noname) header
    IF
	put-load-file-name
	source >in @ min tag-file-id write-file throw
	tag-beg count tag-file-id write-file throw
	tlast @ >image count $1F and tag-file-id write-file throw
	tag-end count tag-file-id write-file throw
	base @ decimal sourceline# 0 <# #s #> tag-file-id write-file throw
\	>in @ 0 <# #s [char] , hold #> tag-file-id write-line throw
	s" ,0" tag-file-id write-line throw
	base !
    THEN ;

\ Check for words

Defer skip? ' false IS skip?

: defined? ( -- flag ) \ name
    ghost forward? 0= ;

: needed? ( -- flag ) \ name
\G returns a false flag when
\G a word is not defined
\G a forward reference exists
\G so the definition is not skipped!
    bl word gfind
    IF dup forward?
	nip
	0=
    ELSE  drop true  THEN ;

: doer? ( -- flag ) \ name
    ghost >magic @ <do:> = ;

: skip-defs ( -- )
    BEGIN  refill  WHILE  source -trailing nip 0= UNTIL  THEN ;

\ Target header creation

Variable CreateFlag
CreateFlag off

Variable NoHeaderFlag
NoHeaderFlag off

: 0.r ( n1 n2 -- ) 
    base @ >r hex 
    0 swap <# 0 ?DO # LOOP #> type 
    r> base ! ;
: .sym
  bounds 
  DO I c@ dup
	CASE	'/ OF drop ." \/" ENDOF
		'\ OF drop ." \\" ENDOF
		dup OF emit ENDOF
	ENDCASE
    LOOP ;

: (Theader ( "name" -- ghost )
    \  >in @ bl word count type 2 spaces >in !
    \ wordheaders will always be compiled to rom
    switchrom
    \ build header in target
    NoHeaderFlag @
    IF  NoHeaderFlag off
    ELSE
	T align H view,
	tlast @ dup 0> IF  T 1 cells - H THEN T A, H  there tlast !
	1 headers-named +!	\ Statistic
	>in @ T name, H >in !
    THEN
    T cfalign here H tlastcfa !
    \ Symbol table
\    >in @ cr ." sym:s/CFA=" there 4 0.r ." /"  bl word count .sym ." /g" cr >in !
    CreateFlag @
    IF
	>in @ alias2 swap >in !         \ create alias in target
	>in @ ghost swap >in !
	swap also ghosts ' previous swap !     \ tick ghost and store in alias
	CreateFlag off
    ELSE ghost
    THEN
    dup Last-Header-Ghost !
    dup >magic ^imm !     \ a pointer for immediate
    Already @
    IF  dup >end tdoes !
    ELSE 0 tdoes !
    THEN
    80 flag!
    cross-doc-entry cross-tag-entry ;

VARIABLE ;Resolve 1 cells allot
\ this is the resolver information from ":"
\ resolving is done by ";"

: Theader  ( "name" -- ghost )
  (THeader dup there resolve 0 ;Resolve ! ;

>TARGET
: Alias    ( cfa -- ) \ name
    >in @ skip? IF  2drop  EXIT  THEN  >in !
    dup 0< s" prims" T $has? H 0= and
    IF
	.sourcepos ." needs prim: " >in @ bl word count type >in ! cr
    THEN
    (THeader over resolve T A, H 80 flag! ;
: Alias:   ( cfa -- ) \ name
    >in @ skip? IF  2drop  EXIT  THEN  >in !
    dup 0< s" prims" T $has? H 0= and
    IF
	.sourcepos ." needs doer: " >in @ bl word count type >in ! cr
    THEN
    ghost tuck swap resolve <do:> swap >magic ! ;

Variable prim#
: first-primitive ( n -- )  prim# ! ;
: Primitive  ( -- ) \ name
    prim# @ T Alias H  -1 prim# +! ;
>CROSS

\ Conditionals and Comments                            11may93jaw

: ;Cond
  postpone ;
  swap ! ;  immediate

: Cond: ( -- ) \ name {code } ;
  atonce on
  ghost
  >exec
  :NONAME ;

: restrict? ( -- )
\ aborts on interprete state - ae
  state @ 0= ABORT" CROSS: Restricted" ;

: Comment ( -- )
  >in @ atonce on ghost swap >in ! ' swap >exec ! ;

Comment (       Comment \

\ compile                                              10may93jaw

: compile  ( -- ) \ name
  restrict?
  bl word gfind dup 0= ABORT" CROSS: Can't compile "
  0> ( immediate? )
  IF    >exec @ compile,
  ELSE  postpone literal postpone gexecute  THEN ;
                                        immediate

: [G'] 
\G ticks a ghost and returns its address
  bl word gfind 0= ABORT" CROSS: Ghost don't exists"
  state @
  IF   postpone literal
  THEN ; immediate

: ghost>cfa
  dup forward? ABORT" CROSS: forward " >link @ ;
               
>TARGET

: '  ( -- cfa ) 
\ returns the target-cfa of a ghost
  bl word gfind 0= ABORT" CROSS: Ghost don't exists"
  ghost>cfa ;

Cond: [']  T ' H alit, ;Cond

>CROSS

: [T']
\ returns the target-cfa of a ghost, or compiles it as literal
  postpone [G'] state @ IF postpone ghost>cfa ELSE ghost>cfa THEN ; immediate

\ \ threading modell					13dec92py
\ modularized						14jun97jaw

: fillcfa   ( usedcells -- )
  T cells H xt>body swap - 0 ?DO 0 T c, tchar H +LOOP ;

: (>body)   ( cfa -- pfa ) xt>body + ;		' (>body) T IS >body H

: (doer,)   ( ghost -- ) ]comp gexecute comp[ 1 fillcfa ;   ' (doer,) IS doer,

: (docol,)  ( -- ) [G'] :docol doer, ;		' (docol,) IS docol,

: (doprim,) ( -- )
  there xt>body + ca>native T a, H 1 fillcfa ;	' (doprim,) IS doprim,

: (doeshandler,) ( -- ) 
  T cfalign H compile :doesjump T 0 , H ; 	' (doeshandler,) IS doeshandler,

: (dodoes,) ( does-action-ghost -- )
  ]comp [G'] :dodoes gexecute comp[
  addr,
  T here H tcell - reloff 2 fillcfa ;		' (dodoes,) IS dodoes,

: (lit,) ( n -- )   compile lit T  ,  H ;	' (lit,) IS lit,

\ if we dont produce relocatable code alit, defaults to lit, jaw
has? relocate
[IF]
: (alit,) ( n -- )  compile lit T  a, H ;	' (alit,) IS alit,
[ELSE]
: (alit,) ( n -- )  lit, ;			' (alit,) IS alit,
[THEN]

: (fini,)         compile ;s ;                ' (fini,) IS fini,

[IFUNDEF] (code) 
Defer (code)
Defer (end-code)
[THEN]

>TARGET
: Code
  defempty?
  (THeader there resolve
  [ T e? prims H 0= [IF] T e? ITC H [ELSE] true [THEN] ] [IF]
  doprim, 
  [THEN]
  depth (code) ;

: Code:
  defempty?
    ghost dup there ca>native resolve  <do:> swap >magic !
    depth (code) ;

: end-code
    (end-code)
    depth ?dup IF   1- <> ABORT" CROSS: Stack changed"
    ELSE true ABORT" CROSS: Stack empty" THEN
    ;

( Cond ) : chars tchar * ; ( Cond )

>CROSS

\ tLiteral                                             12dec92py

>TARGET
Cond: \G  T-\G ;Cond

Cond:  Literal ( n -- )   restrict? lit, ;Cond
Cond: ALiteral ( n -- )   restrict? alit, ;Cond

: Char ( "<char>" -- )  bl word char+ c@ ;
Cond: [Char]   ( "<char>" -- )  restrict? Char  lit, ;Cond

\ some special literals					27jan97jaw

\ !! Known Bug: Special Literals and plug-ins work only correct
\ on 16 and 32 Bit Targets and 32 Bit Hosts!

Cond: MAXU
  restrict? 
  tcell 1 cells u> 
  IF	compile lit tcell 0 ?DO FF T c, H LOOP 
  ELSE	$ffffffff lit, THEN
  ;Cond

Cond: MINI
  restrict?
  tcell 1 cells u>
  IF	compile lit bigendian 
  	IF	80 T c, H tcell 1 ?DO 0 T c, H LOOP 
  	ELSE  	tcell 1 ?DO 0 T c, H LOOP 80 T c, H
  	THEN
  ELSE	tcell 2 = IF $8000 ELSE $80000000 THEN lit, THEN
  ;Cond
 
Cond: MAXI
 restrict?
 tcell 1 cells u>
 IF	compile lit bigendian 
	IF 	7F T c, H tcell 1 ?DO FF T c, H LOOP
	ELSE 	tcell 1 ?DO FF T c, H LOOP 7F T c, H
 	THEN
 ELSE	tcell 2 = IF $7fff ELSE $7fffffff THEN lit, THEN
 ;Cond

>CROSS
\ Target compiling loop                                12dec92py
\ ">tib trick thrown out                               10may93jaw
\ number? defined at the top                           11may93jaw

\ compiled word might leave items on stack!
: tcom ( in name -- )
  gfind  ?dup  IF    0> IF    nip >exec @ execute
                        ELSE  nip gexecute  THEN EXIT THEN
  number? dup  IF    0> IF swap lit,  THEN  lit,  drop
               ELSE  2drop >in !
               ghost gexecute THEN  ;

>TARGET
\ : ; DOES>                                            13dec92py
\ ]                                                     9may93py/jaw

: ] state on
    Compiling comp-state !
    BEGIN
        BEGIN >in @ bl word
              dup c@ 0= WHILE 2drop refill 0=
              ABORT" CROSS: End of file while target compiling"
        REPEAT
        tcom
        state @
        0=
    UNTIL ;

\ by the way: defining a second interpreter (a compiler-)loop
\             is not allowed if a system should be ans conform

: : ( -- colon-sys ) \ Name
  defempty?
  constflag off \ don't let this flag work over colon defs
		\ just to go sure nothing unwanted happens
  >in @ skip? IF  drop skip-defs  EXIT  THEN  >in !
  (THeader ;Resolve ! there ;Resolve cell+ !
  docol, ]comp depth T ] H ;

: :noname ( -- colon-sys )
  T cfalign H there docol, 0 ;Resolve ! depth T ] H ;

Cond: EXIT ( -- )  restrict?  compile ;S  ;Cond

Cond: ?EXIT ( -- ) 1 abort" CROSS: using ?exit" ;Cond

>CROSS
: LastXT ;Resolve @ 0= abort" CROSS: no definition for LastXT"
         ;Resolve cell+ @ ;

>TARGET

Cond: recurse ( -- ) Last-Ghost @ gexecute ;Cond

Cond: ; ( -- ) restrict?
               depth ?dup IF   1- <> ABORT" CROSS: Stack changed"
                          ELSE true ABORT" CROSS: Stack empty" THEN
               fini,
               comp[
               state off
               ;Resolve @
               IF ;Resolve @ ;Resolve cell+ @ resolve THEN
		Interpreting comp-state !
               ;Cond
Cond: [  restrict? state off Interpreting comp-state ! ;Cond

>CROSS

Create GhostDummy ghostheader
<res> GhostDummy >magic !

: !does ( does-action -- )
\ !! zusammenziehen und dodoes, machen!
    tlastcfa @ [G'] :dovar killref
\    tlastcfa @ dup there >r tdp ! compile :dodoes r> tdp ! T cell+ ! H ;
\ !! geht so nicht, da dodoes, ghost will!
    GhostDummy >link ! GhostDummy 
    tlastcfa @ >tempdp dodoes, tempdp> ;

>TARGET
Cond: DOES> restrict?
        compile (does>) doeshandler, 
	\ resolve words made by builders
	tdoes @ ?dup IF  @ T here H resolve THEN
        ;Cond
: DOES> switchrom doeshandler, T here H !does depth T ] H ;

>CROSS
\ Creation                                             01nov92py

\ Builder                                               11may93jaw

: Builder    ( Create-xt do:-xt "name" -- )
\ builds up a builder in current vocabulary
\ create-xt is executed when word is interpreted
\ do:-xt is executet when the created word from builder is executed
\ for do:-xt an additional entry after the normal ghost-enrys is used

  >in @ alias2 swap dup >in ! >r >r
  Make-Ghost 
  rot swap >exec dup @ ['] NoExec <>
  IF 2drop ELSE ! THEN
  ,
  r> r> >in !
  also ghosts ' previous swap ! ;
\  DOES>  dup >exec @ execute ;

: gdoes,  ( ghost -- )
\ makes the codefield for a word that is built
  >end @ dup forward? 0=
  IF
	dup >magic @ <do:> =
	IF 	 doer, 
	ELSE	dodoes,
	THEN
	EXIT
  THEN
\  compile :dodoes gexecute
\  T here H tcell - reloff 
  2 refered 
  0 fillcfa
  ;

: TCreate ( <name> -- )
  executed-ghost @
  CreateFlag on
  create-forward-warn
  IF ['] reswarn-forward IS resolve-warning THEN
  Theader >r dup gdoes,
\ stores execution symantic in the built word
  >end @ >exec @ r> >exec ! ;

: RTCreate ( <name> -- )
\ creates a new word with code-field in ram
  executed-ghost @
  CreateFlag on
  create-forward-warn
  IF ['] reswarn-forward IS resolve-warning THEN
  \ make Alias
  (THeader there 0 T a, H 80 flag! ( S executed-ghost new-ghost )
  \ store  poiter to code-field
  switchram T cfalign H
  there swap T ! H
  there tlastcfa ! 
  dup there resolve 0 ;Resolve !
  >r dup gdoes,
  >end @ >exec @ r> >exec ! ;

: Build:  ( -- [xt] [colon-sys] )
  :noname postpone TCreate ;

: BuildSmart:  ( -- [xt] [colon-sys] )
  :noname
  [ T has? rom H [IF] ]
  postpone RTCreate
  [ [ELSE] ]
  postpone TCreate 
  [ [THEN] ] ;

: gdoes>  ( ghost -- addr flag )
  executed-ghost @
  state @ IF  gexecute true EXIT  THEN
  >link @ T >body H false ;

\ DO: ;DO                                               11may93jaw
\ changed to ?EXIT                                      10may93jaw

: DO:     ( -- addr [xt] [colon-sys] )
  here ghostheader
  :noname postpone gdoes> postpone ?EXIT ;

: by:     ( -- addr [xt] [colon-sys] ) \ name
  ghost
  :noname postpone gdoes> postpone ?EXIT ;

: ;DO ( addr [xt] [colon-sys] -- addr )
  postpone ;    ( S addr xt )
  over >exec ! ; immediate

: by      ( -- addr ) \ Name
  ghost >end @ ;

>TARGET
\ Variables and Constants                              05dec92py

Build:  ( n -- ) ;
by: :docon ( ghost -- n ) T @ H ;DO
Builder (Constant)

Build:  ( n -- ) T , H ;
by (Constant)
Builder Constant

Build:  ( n -- ) T A, H ;
by (Constant)
Builder AConstant

Build:  ( d -- ) T , , H ;
DO: ( ghost -- d ) T dup cell+ @ swap @ H ;DO
Builder 2Constant

BuildSmart: ;
by: :dovar ( ghost -- addr ) ;DO
Builder Create

T has? rom H [IF]
Build: ( -- ) T here 0 , H switchram T align here swap ! 0 , H ( switchrom ) ;
by (Constant)
Builder Variable
[ELSE]
Build: T 0 , H ;
by Create
Builder Variable
[THEN]

T has? rom H [IF]
Build: ( -- ) T here 0 , H switchram T align here swap ! 0 , 0 , H ( switchrom ) ;
by (Constant)
Builder 2Variable
[ELSE]
Build: T 0 , 0 , H ;
by Create
Builder 2Variable
[THEN]

T has? rom H [IF]
Build: ( -- ) T here 0 , H switchram T align here swap ! 0 , H ( switchrom ) ;
by (Constant)
Builder AVariable
[ELSE]
Build: T 0 A, H ;
by Create
Builder AVariable
[THEN]

\ User variables                                       04may94py

>CROSS
Variable tup  0 tup !
Variable tudp 0 tudp !
: u,  ( n -- udp )
  tup @ tudp @ + T  ! H
  tudp @ dup T cell+ H tudp ! ;
: au, ( n -- udp )
  tup @ tudp @ + T A! H
  tudp @ dup T cell+ H tudp ! ;
>TARGET

Build: T 0 u, , H ;
by: :douser ( ghost -- up-addr )  T @ H tup @ + ;DO
Builder User

Build: T 0 u, , 0 u, drop H ;
by User
Builder 2User

Build: T 0 au, , H ;
by User
Builder AUser

BuildSmart: T , H ;
by (Constant)
Builder Value

BuildSmart: T A, H ;
by (Constant)
Builder AValue

BuildSmart:  ( -- ) [T'] noop T A, H ;
by: :dodefer ( ghost -- ) ABORT" CROSS: Don't execute" ;DO
Builder Defer

BuildSmart:  ( inter comp -- ) swap T immediate A, A, H ;
DO: ( ghost -- ) ABORT" CROSS: Don't execute" ;DO
Builder interpret/compile:

\ Sturctures                                           23feb95py

>CROSS
: nalign ( addr1 n -- addr2 )
\ addr2 is the aligned version of addr1 wrt the alignment size n
 1- tuck +  swap invert and ;
>TARGET

Build: ;
by: :dofield T @ H + ;DO
Builder (Field)

Build: ( align1 offset1 align size "name" --  align2 offset2 )
    rot dup T , H ( align1 align size offset1 )
    + >r nalign r> ;
by (Field)
Builder Field

: struct  T 1 chars 0 H ;
: end-struct  T 2Constant H ;

: cell% ( n -- size align )
    T 1 cells H dup ;

\ ' 2Constant Alias2 end-struct
\ 0 1 T Chars H 2Constant struct

\ structural conditionals                              17dec92py

>CROSS
: ?struc      ( flag -- )       ABORT" CROSS: unstructured " ;
: sys?        ( sys -- sys )    dup 0= ?struc ;
: >mark       ( -- sys )        T here  ( dup ." M" hex. ) 0 , H ;

: branchoffset ( src dest -- ) - tchar / ; \ ?? jaw

: >resolve    ( sys -- )        T here ( dup ." >" hex. ) over branchoffset swap ! H ;

: <resolve    ( sys -- )        T here ( dup ." <" hex. ) branchoffset , H ;

:noname compile branch T here branchoffset , H ;
  IS branch, ( target-addr -- )
:noname compile ?branch T here branchoffset , H ;
  IS ?branch, ( target-addr -- )
:noname compile branch T here 0 , H ;
  IS branchmark, ( -- branchtoken )
:noname compile ?branch T here 0 , H ;
  IS ?branchmark, ( -- branchtoken )
:noname T here 0 , H ;
  IS ?domark, ( -- branchtoken )
:noname dup T @ H ?struc T here over branchoffset swap ! H ;
  IS branchtoresolve, ( branchtoken -- )
:noname branchto, T here H ;
  IS branchtomark, ( -- target-addr )

>TARGET

\ Structural Conditionals                              12dec92py

Cond: BUT       restrict? sys? swap ;Cond
Cond: YET       restrict? sys? dup ;Cond

0 [IF]
>CROSS
Variable tleavings
>TARGET

Cond: DONE   ( addr -- )  restrict? tleavings @
      BEGIN  2dup u> 0=  WHILE  dup T @ H swap >resolve REPEAT
      tleavings ! drop ;Cond

>CROSS
: (leave)  T here H tleavings @ T , H  tleavings ! ;
>TARGET

Cond: LEAVE     restrict? compile branch (leave) ;Cond
Cond: ?LEAVE    restrict? compile 0=  compile ?branch (leave)  ;Cond

[ELSE]
    \ !! This is WIP
    \ The problem is (?DO)!
    \ perhaps we need a plug-in for (?DO)
    
>CROSS
Variable tleavings 0 tleavings !
: (done) ( addr -- )
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

Cond: DONE   ( addr -- )  restrict? (done) ;Cond

>CROSS
: (leave) ( branchtoken -- )
    3 cells allocate throw >r
    T here H r@ cell+ !
    r@ 2 cells + !
    tleavings @ r@ !
    r> tleavings ! ;
>TARGET

Cond: LEAVE     restrict? branchmark, (leave) ;Cond
Cond: ?LEAVE    restrict? compile 0=  ?branchmark, (leave)  ;Cond

[THEN]

>CROSS
\ !!JW ToDo : Move to general tools section

: to1 ( x1 x2 xn n -- addr )
\G packs n stack elements in a allocated memory region
   dup dup 1+ cells allocate throw dup >r swap 1+
   0 DO tuck ! cell+ LOOP
   drop r> ;
: 1to ( addr -- x1 x2 xn )
\G unpacks the elements saved by to1
    dup @ swap over cells + swap
    0 DO  dup @ swap 1 cells -  LOOP
    free throw ;

: loop]     branchto, dup <resolve tcell - (done) ;

: skiploop] ?dup IF branchto, branchtoresolve, THEN ;

>TARGET

\ Structural Conditionals                              12dec92py

>TARGET
Cond: AHEAD     restrict? branchmark, ;Cond
Cond: IF        restrict? ?branchmark, ;Cond
Cond: THEN      restrict? sys? branchto, branchtoresolve, ;Cond
Cond: ELSE      restrict? sys? compile AHEAD swap compile THEN ;Cond

Cond: BEGIN     restrict? branchtomark, ;Cond
Cond: WHILE     restrict? sys? compile IF swap ;Cond
Cond: AGAIN     restrict? sys? branch, ;Cond
Cond: UNTIL     restrict? sys? ?branch, ;Cond
Cond: REPEAT    restrict? over 0= ?struc compile AGAIN compile THEN ;Cond

Cond: CASE      restrict? 0 ;Cond
Cond: OF        restrict? 1+ >r compile over compile =
                compile IF compile drop r> ;Cond
Cond: ENDOF     restrict? >r compile ELSE r> ;Cond
Cond: ENDCASE   restrict? compile drop 0 ?DO  compile THEN  LOOP ;Cond

\ Structural Conditionals                              12dec92py

:noname \ ?? i think 0 is too much! jaw
    0 compile (do)
    branchtomark,  2 to1 ;
  IS do, ( -- target-addr )

\ :noname
\     compile 2dup compile = compile IF
\     compile 2drop compile ELSE
\     compile (do) branchtomark, 2 to1 ;
\   IS ?do,
    
:noname
    0 compile (?do)  ?domark, (leave)
    branchtomark,  2 to1 ;
  IS ?do, ( -- target-addr )
:noname compile (for) branchtomark, ;
  IS for, ( -- target-addr )
:noname 1to compile (loop)  loop] compile unloop skiploop] ;
  IS loop, ( target-addr -- )
:noname 1to compile (+loop)  loop] compile unloop skiploop] ;
  IS +loop, ( target-addr -- )
:noname compile (next)  loop] compile unloop ;
  IS next, ( target-addr -- )

Cond: DO      	restrict? do, ;Cond
Cond: ?DO     	restrict? ?do, ;Cond
Cond: FOR	restrict? for, ;Cond

Cond: LOOP	restrict? sys? loop, ;Cond
Cond: +LOOP	restrict? sys? +loop, ;Cond
Cond: NEXT	restrict? sys? next, ;Cond

\ String words                                         23feb93py

: ,"            [char] " parse T string, align H ;

Cond: ."        restrict? compile (.")     T ," H ;Cond
Cond: S"        restrict? compile (S")     T ," H ;Cond
Cond: ABORT"    restrict? compile (ABORT") T ," H ;Cond

Cond: IS        T ' >body H compile ALiteral compile ! ;Cond
: IS            T >address ' >body ! H ;
Cond: TO        T ' >body H compile ALiteral compile ! ;Cond
: TO            T ' >body ! H ;

Cond: defers	T ' >body @ compile, H ;Cond
: on		T -1 swap ! H ; 
: off   	T 0 swap ! H ;

\ LINKED ERR" ENV" 2ENV"                                18may93jaw

\ linked list primitive
: linked        T here over @ A, swap ! H ;
: chained	T linked A, H ;

: err"   s" ErrLink linked" evaluate T , H
         [char] " parse T string, align H ;

: env"  [char] " parse s" EnvLink linked" evaluate
        T string, align , H ;

: 2env" [char] " parse s" EnvLink linked" evaluate
        here >r T string, align , , H
        r> dup T c@ H 80 and swap T c! H ;

\ compile must be last                                 22feb93py

Cond: compile ( -- ) restrict? \ name
      bl word gfind dup 0= ABORT" CROSS: Can't compile"
      0> IF    gexecute
         ELSE  dup >magic @ <imm> =
               IF   gexecute
               ELSE compile (compile) addr, THEN THEN ;Cond

Cond: postpone ( -- ) restrict? \ name
      bl word gfind dup 0= ABORT" CROSS: Can't compile"
      0> IF    gexecute
         ELSE  dup >magic @ <imm> =
               IF   gexecute
	       ELSE compile (compile) addr, THEN THEN ;Cond
	   
\ \ minimal definitions
	   
>MINIMAL
also minimal
\ Usefull words                                        13feb93py

: KB  400 * ;

\ \ [IF] [ELSE] [THEN] ...				14sep97jaw

\ it is useful to define our own structures and not to rely
\ on the words in the compiler
\ The words in the compiler might be defined with vocabularies
\ this doesn't work with our self-made compile-loop

Create parsed 20 chars allot	\ store word we parsed

: upcase
    parsed count bounds
    ?DO I c@ toupper I c! LOOP ;

: [ELSE]
    1 BEGIN
	BEGIN bl word count dup WHILE
	    comment? parsed place upcase parsed count
	    2dup s" [IF]" compare 0= >r 
	    2dup s" [IFUNDEF]" compare 0= >r
	    2dup s" [IFDEF]" compare 0= r> or r> or
	    IF   2drop 1+
	    ELSE 2dup s" [ELSE]" compare 0=
		IF   2drop 1- dup
		    IF 1+
		    THEN
		ELSE
		    2dup s" [ENDIF]" compare 0= >r
		    s" [THEN]" compare 0= r> or
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

: defined? defined? ;
: needed? needed? ;
: doer? doer? ;

\ we want to use IFDEF on compiler directives (e.g. E?) in the source, too

: directive? 
  bl word count [ ' target >wordlist ] aliteral search-wordlist 
  dup IF nip THEN ;

: [IFDEF]  >in @ directive? swap >in !
	   0= IF defined? ELSE name 2drop true THEN
	   postpone [IF] ;

: [IFUNDEF] defined? 0= postpone [IF] ;

Cond: [IFDEF]   postpone [IFDEF] ;Cond

Cond: [IFUNDEF] postpone [IFUNDEF] ;Cond

\ C: \- \+ Conditional Compiling                         09jun93jaw

: C: >in @ defined? 0=
     IF    >in ! T : H
     ELSE drop
        BEGIN bl word dup c@
              IF   count comment? s" ;" compare 0= ?EXIT
              ELSE refill 0= ABORT" CROSS: Out of Input while C:"
              THEN
        AGAIN
     THEN ;

also minimal

\G doesn't skip line when bit is set in debugmask
: \D name evaluate debugmasksource @ and 0= IF postpone \ THEN ;

\G interprets the line if word is not defined
: \- defined? IF postpone \ THEN ;

\G interprets the line if word is defined
: \+ defined? 0= IF postpone \ THEN ;

Cond: \- \- ;Cond
Cond: \+ \+ ;Cond
Cond: \D \D ;Cond

: ?? bl word find IF execute ELSE drop 0 THEN ;

: needed:
\G defines ghost for words that we want to be compiled
  BEGIN >in @ bl word c@ WHILE >in ! ghost drop REPEAT drop ;

previous

\ save-cross                                           17mar93py

>CROSS
Create magic  s" Gforth2x" here over allot swap move

bigendian 1+ \ strangely, in magic big=0, little=1
tcell 1 = 0 and or
tcell 2 = 2 and or
tcell 4 = 4 and or
tcell 8 = 6 and or
tchar 1 = $00 and or
tchar 2 = $28 and or
tchar 4 = $50 and or
tchar 8 = $78 and or
magic 7 + c!

: save-cross ( "image-name" "binary-name" -- )
  bl parse ." Saving to " 2dup type cr
  w/o bin create-file throw >r
  TNIL IF
      s" #! "   r@ write-file throw
      bl parse  r@ write-file throw
      s"  -i"   r@ write-file throw
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
  image @ there 
  r@ write-file throw \ write image
  TNIL IF
      bit$  @ there 1- tcell>bit rshift 1+
                r@ write-file throw \ write tags
  THEN
  r> close-file throw ;

: save-region ( addr len -- )
  bl parse w/o bin create-file throw >r
  swap >image swap r@ write-file throw
  r> close-file throw ;

\ words that should be in minimal

create s-buffer 50 chars allot

>MINIMAL
also minimal

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
: warnings name 3 = 0= twarnings ! drop ;
: | ;
\ : | NoHeaderFlag on ; \ This is broken (damages the last word)

: save-cross save-cross ;
: save-region save-region ;
: tdump swap >image swap dump ;

also forth 
[IFDEF] Label           : Label defempty? Label ; [THEN] 
[IFDEF] start-macros    : start-macros defempty? start-macros ; [THEN]
[IFDEF] builttag	: builttag builttag ;	[THEN]
previous

: s" [char] " parse s-buffer place s-buffer count ; \ for environment?
: + + ;
: 1+ 1 + ;
: 2+ 2 + ;
: or or ;
: 1- 1- ;
: - - ;
: and and ;
: or or ;
: 2* 2* ;
: * * ;
: / / ;
: dup dup ;
: over over ;
: swap swap ;
: rot rot ;
: drop drop ;
: =   = ;
: 0=   0= ;
: lshift lshift ;
: 2/ 2/ ;
: . . ;

: all-words    ['] false    IS skip? ;
: needed-words ['] needed?  IS skip? ;
: undef-words  ['] defined? IS skip? ;

: \  postpone \ ;  immediate
: \G T-\G ; immediate
: (  postpone ( ;  immediate
: include bl word count included ;
: require require ;
: .( [char] ) parse type ;
: ." [char] " parse type ;
: cr cr ;

: times 0 ?DO dup T c, H LOOP drop ; \ used for space table creation
only forth also minimal definitions

\ cross-compiler words

: decimal       decimal ;
: hex           hex ;

: tudp          T tudp H ;
: tup           T tup H ;

: doc-off       false T to-doc H ! ;
: doc-on        true  T to-doc H ! ;
[IFDEF] dbg : dbg dbg ; [THEN]

minimal

\ for debugging...
: order         order ;
: hwords         words ;
: words 	also ghosts words previous ;
: .s            .s ;
: bye           bye ;

\ turnkey direction
: H forth ; immediate
: T minimal ; immediate
: G ghosts ; immediate

: turnkey  0 set-order also Target definitions
           also Minimal also ;

\ these ones are pefered:

: lock   turnkey ;
: unlock forth also cross ;

: [[ also unlock ;
: ]] previous previous ;

unlock definitions also minimal
: lock   lock ;
lock
