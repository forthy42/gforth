\ CROSS.FS     The Cross-Compiler                      06oct92py
\ Idea and implementation: Bernd Paysan (py)

\ Copyright (C) 1995 Free Software Foundation, Inc.

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

\ Log:
\       changed in ; [ to state off           12may93jaw
\       included place +place                 12may93jaw
\       for a created word (variable, constant...)
\       is now an alias in the target voabulary.
\       this means it is no longer necessary to
\       switch between vocabularies for variable
\       initialization                        12may93jaw
\       discovered error in DOES>
\       replaced !does with (;code)           16may93jaw
\       made complete redesign and
\       introduced two vocs method
\       to be asure that the right words
\       are found                             08jun93jaw
\       btw:  ! works not with 16 bit
\             targets                         09jun93jaw
\       added: 2user and value                11jun93jaw

\ 	needed? works better now!!!		01mar97jaw
\	mach file is only loaded into target
\	cell corrected


\ include other.fs       \ ansforth extentions for cross

: string, ( c-addr u -- )
    \ puts down string as cstring
    dup c, here swap chars dup allot move ;
' falign Alias cfalign
: comment? ( c-addr u -- c-addr u )
        2dup s" (" compare 0=
        IF    postpone (
        ELSE  2dup s" \" compare 0= IF postpone \ THEN
        THEN ;

decimal

\ Begin CROSS COMPILER:

\ GhostNames                                            9may93jaw
\ second name source to search trough list

VARIABLE GhostNames
0 GhostNames !
: GhostName ( -- addr )
    here GhostNames @ , GhostNames ! here 0 ,
    bl word count
    \ 2dup type space
    string, cfalign ;

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

\ Parameter for target systems                         06oct92py

>TARGET
mach-file count included

[IFUNDEF] has-interpreter true CONSTANT has-interpreter [THEN]

also Forth definitions

[IFDEF] asm-include asm-include [THEN]

previous
hex

>CROSS

\ Create additional parameters                         19jan95py

T
NIL		   Constant TNIL
cell               Constant tcell
cell<<             Constant tcell<<
cell>bit           Constant tcell>bit
bits/byte          Constant tbits/byte
float              Constant tfloat
1 bits/byte lshift Constant maxbyte
H

\ Variables                                            06oct92py

Variable image
Variable tlast    TNIL tlast !  \ Last name field
Variable tlastcfa \ Last code field
Variable tdoes    \ Resolve does> calls
Variable bit$
Variable tdp
: there  tdp @ ;


>TARGET

\ Byte ordering and cell size                          06oct92py

: cell+         tcell + ;
: cells         tcell<< lshift ;
: chars         ;
: char+		1 + ;
: floats	tfloat * ;
    
>CROSS
: cell/         tcell<< rshift ;
>TARGET
20 CONSTANT bl
TNIL Constant NIL

>CROSS

bigendian
[IF]
   : T!  ( n addr -- )  >r s>d r> tcell bounds swap 1-
     DO  maxbyte ud/mod rot I c!  -1 +LOOP  2drop ;
   : T@  ( addr -- n )  >r 0 0 r> tcell bounds
     DO  maxbyte * swap maxbyte um* rot + swap I c@ + swap  LOOP d>s ;
[ELSE]
   : T!  ( n addr -- )  >r s>d r> tcell bounds
     DO  maxbyte ud/mod rot I c!  LOOP  2drop ;
   : T@  ( addr -- n )  >r 0 0 r> tcell bounds swap 1-
     DO  maxbyte * swap maxbyte um* rot + swap I c@ + swap  -1 +LOOP d>s ;
[THEN]

\ Memory initialisation                                05dec92py
\ Fixed bug in else part                               11may93jaw

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

>MINIMAL
: makekernel ( targetsize -- targetsize )
  bit$  over 1- tcell>bit rshift 1+ initmem
  image over initmem tdp off ;

>CROSS
\ Bit string manipulation                               06oct92py
\                                                       9may93jaw
CREATE Bittable 80 c, 40 c, 20 c, 10 c, 8 c, 4 c, 2 c, 1 c,
: bits ( n -- n ) chars Bittable + c@ ;

: >bit ( addr n -- c-addr mask ) 8 /mod rot + swap bits ;
: +bit ( addr n -- )  >bit over c@ or swap c! ;
: -bit ( addr n -- )  >bit invert over c@ and swap c! ;
: relon ( taddr -- )  bit$ @ swap cell/ +bit ;
: reloff ( taddr -- )  bit$ @ swap cell/ -bit ;

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

>CROSS
: >image ( taddr -- absaddr )  image @ + ;
>TARGET
: @  ( taddr -- w )     >image t@ ;
: !  ( w taddr -- )     >image t! ;
: c@ ( taddr -- char )  >image c@ ;
: c! ( char taddr -- )  >image c! ;
: 2@ ( taddr -- x1 x2 ) T dup cell+ @ swap @ H ;
: 2! ( x1 x2 taddr -- ) T swap over ! cell+ ! H ;

\ Target compilation primitives                        06oct92py
\ included A!                                          16may93jaw

: here  ( -- there )    there ;
: allot ( n -- )        tdp +! ;
: ,     ( w -- )        T here H tcell T allot  ! H ;
: c,    ( char -- )     T here    1 allot c! H ;
: align ( -- )          T here H align+ 0 ?DO  bl T c, H LOOP ;
: cfalign ( -- )
    T here H cfalign+ 0 ?DO  bl T c, H LOOP ;

: A!                    dup relon T ! H ;
: A,    ( w -- )        T here H relon T , H ;

>CROSS

\ threading modell                                     13dec92py

>TARGET
: >body   ( cfa -- pfa ) T cell+ cell+ H ;
>CROSS

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

: >magic ;
: >link cell+ ;
: >exec cell+ cell+ ;
: >end 3 cells + ;

Variable last-ghost
: Make-Ghost ( "name" -- ghost )
  >in @ GhostName swap >in !
  <T Create atonce @ IF immediate atonce off THEN
  here tuck swap ! ghostheader T>
  DOES> dup last-ghost ! >exec @ execute ;

variable cfalist 0 cfalist !

: markcfa
  cfalist here over @ , swap ! , ;

\ ghost words                                          14oct92py
\                                          changed:    10may93py/jaw

: gfind   ( string -- ghost true/1 / string false )
\ searches for string in word-list ghosts
  dup count [ ' ghosts >body ] ALiteral search-wordlist
  dup IF >r >body nip r>  THEN ;

VARIABLE Already

: ghost   ( "name" -- ghost )
  Already off
  >in @  bl word gfind   IF  Already on nip EXIT  THEN
  drop  >in !  Make-Ghost ;

\ resolve                                              14oct92py

: resolve-loop ( ghost tcfa -- ghost tcfa )
  >r dup >link @
  BEGIN  dup  WHILE  dup T @ H r@ rot T ! H REPEAT  drop r> ;

\ exists                                                9may93jaw

: exists ( ghost tcfa -- )
  over GhostNames
  BEGIN @ dup
  WHILE 2dup cell+ @ =
  UNTIL
        2 cells + count cr ." CROSS: Exists: " type 4 spaces drop
        swap cell+ !
  ELSE  true abort" CROSS: Ghostnames inconsistent "
  THEN ;

: resolve  ( ghost tcfa -- )
  over >magic @ <fwd> <>  IF  exists EXIT THEN
  resolve-loop  over >link ! <res> swap >magic ! ;

\ gexecute ghost,                                      01nov92py

: do-forward   ( ghost -- )
  >link dup @  there rot !  T  A,  H ;
: do-resolve   ( ghost -- )
  >link @                   T  A,  H ;

: gexecute   ( ghost -- )   dup @
             <fwd> = IF  do-forward  ELSE  do-resolve  THEN ;
: ghost,     ghost  gexecute ;

\ .unresolved                                          11may93jaw

variable ResolveFlag

\ ?touched                                             11may93jaw

: ?touched ( ghost -- flag ) dup >magic @ <fwd> = swap >link @
                               0 <> and ;

: ?resolved  ( ghostname -- )
  dup cell+ @ ?touched
  IF  cell+ cell+ count cr type ResolveFlag on ELSE drop THEN ;

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
>CROSS

\ ALIAS2 ansforth conform alias                          9may93jaw

: ALIAS2 create here 0 , DOES> @ execute ;
\ usage:
\ ' <name> alias2 bla !

\ Target Header Creation                               01nov92py

: string,  ( addr count -- )
  dup T c, H bounds  ?DO  I c@ T c, H  LOOP ; 
: name,  ( "name" -- )  bl word count string, T cfalign H ;
: view,   ( -- ) ( dummy ) ;

\ Target Document Creation (goes to crossdoc.fd)       05jul95py

s" doc/crossdoc.fd" r/w create-file throw value doc-file-id
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
    ghost >magic @ <fwd> <> ;

: needed? ( -- flag ) \ name
\G returns a false flag when
\G a word is not defined
\G a forward reference exists
\G so the definition is not skipped!
    bl word gfind
    IF dup >magic @ <fwd> = 
	\ swap >link @ 0<> and 
	nip
	0=
    ELSE  drop true  THEN ;

: doer? ( -- flag ) \ name
    ghost >magic @ <do:> = ;

: skip-defs ( -- )
    BEGIN  refill  WHILE  source -trailing nip 0= UNTIL  THEN ;

\ Target header creation

VARIABLE CreateFlag CreateFlag off

: (Theader ( "name" -- ghost )
\  >in @ bl word count type 2 spaces >in !
  T align H view,
  tlast @ dup 0> IF  T 1 cells - THEN  A, H  there tlast !
  >in @ name, >in ! T here H tlastcfa !
  CreateFlag @ IF
       >in @ alias2 swap >in !         \ create alias in target
       >in @ ghost swap >in !
       swap also ghosts ' previous swap !     \ tick ghost and store in alias
       CreateFlag off
  ELSE ghost THEN
  dup >magic ^imm !     \ a pointer for immediate
  Already @ IF  dup >end tdoes !
  ELSE 0 tdoes ! THEN
  80 flag!
  cross-doc-entry cross-tag-entry ;

VARIABLE ;Resolve 1 cells allot

: Theader  ( "name" -- ghost )
  (THeader dup there resolve 0 ;Resolve ! ;

>TARGET
: Alias    ( cfa -- ) \ name
    >in @ skip? IF  2drop  EXIT  THEN  >in !
    dup 0< has-prims 0= and
    IF
	." needs prim: " >in @ bl word count type >in ! cr
    THEN
    (THeader over resolve T A, H 80 flag! ;
: Alias:   ( cfa -- ) \ name
    >in @ skip? IF  2drop  EXIT  THEN  >in !
    dup 0< has-prims 0= and
    IF
	." needs doer: " >in @ bl word count type >in ! cr
    THEN
    ghost tuck swap resolve <do:> swap >magic ! ;
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
ghost over      ghost =         ghost drop      2drop drop

\ compile                                              10may93jaw

: compile  ( -- ) \ name
  restrict?
  bl word gfind dup 0= ABORT" CROSS: Can't compile "
  0> ( immediate? )
  IF    >exec @ compile,
  ELSE  postpone literal postpone gexecute  THEN ;
                                        immediate

\ generic threading modell
: docol,  ( -- ) compile :docol T 0 , H ;

: dodoes, ( -- ) T cfalign H compile :doesjump T 0 , H ;

[IFUNDEF] (code) 
Defer (code)
Defer (end-code)
[THEN]

[IFUNDEF] ca>native
defer ca>native
[THEN]

>TARGET
: Code
  (THeader there resolve
  [ has-prims 0= [IF] ITC [ELSE] true [THEN] ] [IF]
  there 2 T cells H + ca>native T a, 0 , H
  [THEN]
  depth (code) ;

: Code:
    ghost dup there ca>native resolve  <do:> swap >magic !
    depth (code) ;

: end-code
    depth ?dup IF   1- <> ABORT" CROSS: Stack changed"
    ELSE true ABORT" CROSS: Stack empty" THEN
    (end-code) ;
               
: '  ( -- cfa ) bl word gfind 0= ABORT" CROSS: undefined "
  dup >magic @ <fwd> = ABORT" CROSS: forward " >link @ ;

Cond: [']  compile lit ghost gexecute ;Cond

Cond: chars ;Cond

>CROSS
\ tLiteral                                             12dec92py

: lit, ( n -- )   compile lit T  ,  H ;
: alit, ( n -- )  compile lit T A,  H ;

>TARGET
Cond: \G  T-\G ;Cond

Cond:  Literal ( n -- )   restrict? lit, ;Cond
Cond: ALiteral ( n -- )   restrict? alit, ;Cond

: Char ( "<char>" -- )  bl word char+ c@ ;
Cond: [Char]   ( "<char>" -- )  restrict? Char  lit, ;Cond

\ some special literals					27jan97jaw

Cond: MAXU
 restrict? compile lit 
 tcell 0 ?DO FF T c, H LOOP ;Cond

Cond: MINI
 restrict? compile lit
 bigendian IF
 80 T c, H tcell 1 ?DO 0 T c, H LOOP 
 ELSE
 tcell 1 ?DO 0 T c, H LOOP 80 T c, H
 THEN
 ;Cond
 
Cond: MAXI
 restrict? compile lit
 bigendian IF
 7F T c, H tcell 1 ?DO FF T c, H LOOP 
 ELSE
 tcell 1 ?DO FF T c, H LOOP 7F T c, H
 THEN
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
  >in @ skip? IF  drop skip-defs  EXIT  THEN  >in !
  (THeader ;Resolve ! there ;Resolve cell+ !
  docol, depth T ] H ;

: :noname ( -- colon-sys )
  T cfalign H there docol, depth T ] H ;

Cond: EXIT ( -- )  restrict?  compile ;S  ;Cond

Cond: ?EXIT ( -- ) 1 abort" CROSS: using ?exit" ;Cond

Cond: ; ( -- ) restrict?
               depth ?dup IF   1- <> ABORT" CROSS: Stack changed"
                          ELSE true ABORT" CROSS: Stack empty" THEN
               compile ;S state off
               ;Resolve @
               IF ;Resolve @ ;Resolve cell+ @ resolve THEN
               ;Cond
Cond: [  restrict? state off ;Cond

>CROSS
: !does
    tlastcfa @ dup there >r tdp ! compile :dodoes r> tdp ! T cell+ ! H ;

>TARGET
Cond: DOES> restrict?
        compile (does>) dodoes, tdoes @ ?dup IF  @ T here H resolve THEN
        ;Cond
: DOES> dodoes, T here H !does depth T ] H ;

>CROSS
\ Creation                                             01nov92py

\ Builder                                               11may93jaw

: Builder    ( Create do: "name" -- )
  >in @ alias2 swap dup >in ! >r >r
  Make-Ghost rot swap >exec ! ,
  r> r> >in !
  also ghosts ' previous swap ! ;
\  DOES>  dup >exec @ execute ;

: gdoes,  ( ghost -- )  >end @ dup >magic @ <fwd> <>
    IF
	dup >magic @ <do:> =
	IF  gexecute T 0 , H  EXIT THEN
    THEN
    compile :dodoes gexecute T here H tcell - reloff ;

: TCreate ( -- )
  last-ghost @
  CreateFlag on
  Theader >r dup gdoes,
  >end @ >exec @ r> >exec ! ;

: Build:  ( -- [xt] [colon-sys] )
  :noname  postpone TCreate ;

: gdoes>  ( ghost -- addr flag )
  last-ghost @
  state @ IF  gexecute true EXIT  THEN
  cell+ @ T >body H false ;

\ DO: ;DO                                               11may93jaw
\ changed to ?EXIT                                      10may93jaw

: DO:     ( -- addr [xt] [colon-sys] )
  here ghostheader
  :noname postpone gdoes> postpone ?EXIT ;

: by:     ( -- addr [xt] [colon-sys] ) \ name
  ghost
  :noname postpone gdoes> postpone ?EXIT ;

: ;DO ( addr [xt] [colon-sys] -- )
  postpone ;    ( S addr xt )
  over >exec ! ; immediate

: by      ( -- addr ) \ Name
  ghost >end @ ;

>TARGET
\ Variables and Constants                              05dec92py

Build:  ;
by: :dovar ( ghost -- addr ) ;DO
Builder Create

Build: T 0 , H ;
by Create
Builder Variable

Build: T 0 A, H ;
by Create
Builder AVariable

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

Build: T , H ;
by (Constant)
Builder Value

Build: T A, H ;
by (Constant)
Builder AValue

Build:  ( -- ) compile noop ;
by: :dodefer ( ghost -- ) ABORT" CROSS: Don't execute" ;DO
Builder Defer

Build:  ( inter comp -- ) swap T immediate A, A, H ;
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

Build: 	>r rot r@ nalign  dup T , H  ( align1 size offset )
	+ swap r> nalign ;
by (Field)
Builder Field

: struct  T 0 1 chars H ;
: end-struct  T 2Constant H ;

: cells: ( n -- size align )
    T cells 1 cells H ;

\ ' 2Constant Alias2 end-struct
\ 0 1 T Chars H 2Constant struct

\ structural conditionals                              17dec92py

>CROSS
: ?struc      ( flag -- )       ABORT" CROSS: unstructured " ;
: sys?        ( sys -- sys )    dup 0= ?struc ;
: >mark       ( -- sys )        T here  0 , H ;
: >resolve    ( sys -- )        T here over - swap ! H ;
: <resolve    ( sys -- )        T here - , H ;
>TARGET

\ Structural Conditionals                              12dec92py

Cond: BUT       restrict? sys? swap ;Cond
Cond: YET       restrict? sys? dup ;Cond

>CROSS
Variable tleavings
>TARGET

Cond: DONE   ( addr -- )  restrict? tleavings @
      BEGIN  2dup u> 0=  WHILE  dup T @ H swap >resolve REPEAT
      tleavings ! drop ;Cond

>CROSS
: (leave  T here H tleavings @ T , H  tleavings ! ;
>TARGET

Cond: LEAVE     restrict? compile branch (leave ;Cond
Cond: ?LEAVE    restrict? compile 0=  compile ?branch (leave  ;Cond

\ Structural Conditionals                              12dec92py

Cond: AHEAD     restrict? compile branch >mark ;Cond
Cond: IF        restrict? compile ?branch >mark ;Cond
Cond: THEN      restrict? sys? dup T @ H ?struc >resolve ;Cond
Cond: ELSE      restrict? sys? compile AHEAD swap compile THEN ;Cond

Cond: BEGIN     restrict? T here H ;Cond
Cond: WHILE     restrict? sys? compile IF swap ;Cond
Cond: AGAIN     restrict? sys? compile branch <resolve ;Cond
Cond: UNTIL     restrict? sys? compile ?branch <resolve ;Cond
Cond: REPEAT    restrict? over 0= ?struc compile AGAIN compile THEN ;Cond

Cond: CASE      restrict? 0 ;Cond
Cond: OF        restrict? 1+ >r compile over compile = compile IF compile drop
                r> ;Cond
Cond: ENDOF     restrict? >r compile ELSE r> ;Cond
Cond: ENDCASE   restrict? compile drop 0 ?DO  compile THEN  LOOP ;Cond

\ Structural Conditionals                              12dec92py

Cond: DO        restrict? compile (do)   T here H ;Cond
Cond: ?DO       restrict? compile (?do)  T (leave here H ;Cond
Cond: FOR       restrict? compile (for)  T here H ;Cond

>CROSS
: loop]   dup <resolve tcell - compile DONE compile unloop ;
>TARGET

Cond: LOOP      restrict? sys? compile (loop)  loop] ;Cond
Cond: +LOOP     restrict? sys? compile (+loop) loop] ;Cond
Cond: NEXT      restrict? sys? compile (next)  loop] ;Cond

\ String words                                         23feb93py

: ,"            [char] " parse string, T align H ;

Cond: ."        restrict? compile (.")     T ," H ;Cond
Cond: S"        restrict? compile (S")     T ," H ;Cond
Cond: ABORT"    restrict? compile (ABORT") T ," H ;Cond

Cond: IS        T ' >body H compile ALiteral compile ! ;Cond
: IS            T ' >body ! H ;
Cond: TO        T ' >body H compile ALiteral compile ! ;Cond
: TO            T ' >body ! H ;

\ LINKED ERR" ENV" 2ENV"                                18may93jaw

\ linked list primitive
: linked        T here over @ A, swap ! H ;

: err"   s" ErrLink linked" evaluate T , H
         [char] " parse string, T align H ;

: env"  [char] " parse s" EnvLink linked" evaluate
        string, T align , H ;

: 2env" [char] " parse s" EnvLink linked" evaluate
        here >r string, T align , , H
        r> dup T c@ H 80 and swap T c! H ;

\ compile must be last                                 22feb93py

Cond: compile ( -- ) restrict? \ name
      bl word gfind dup 0= ABORT" CROSS: Can't compile"
      0> IF    gexecute
         ELSE  dup >magic @ <imm> =
               IF   gexecute
               ELSE compile (compile) gexecute THEN THEN ;Cond

Cond: postpone ( -- ) restrict? \ name
      bl word gfind dup 0= ABORT" CROSS: Can't compile"
      0> IF    gexecute
         ELSE  dup >magic @ <imm> =
               IF   gexecute
               ELSE compile (compile) gexecute THEN THEN ;Cond

>MINIMAL
also minimal
\ Usefull words                                        13feb93py

: KB  400 * ;

\ define new [IFDEF] and [IFUNDEF]                      20may93jaw

: defined? defined? ;
: needed? needed? ;
: doer? doer? ;

: [IFDEF] defined? postpone [IF] ;
: [IFUNDEF] defined? 0= postpone [IF] ;

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

\G interprets the line if word is not defined
: \- defined? IF postpone \ THEN ;

\G interprets the line if word is defined
: \+ defined? 0= IF postpone \ THEN ;

Cond: \- \- ;Cond
Cond: \+ \+ ;Cond

: ?? bl word find IF execute ELSE drop 0 THEN ;

: needed:
\G defines ghost for words that we want to be compiled
  BEGIN >in @ bl word c@ WHILE >in ! ghost drop REPEAT drop ;

: [IF]   postpone [IF] ;
: [THEN] postpone [THEN] ;
: [ELSE] postpone [ELSE] ;

Cond: [IF]      [IF] ;Cond
Cond: [IFDEF]   [IFDEF] ;Cond
Cond: [IFUNDEF] [IFUNDEF] ;Cond
Cond: [THEN]    [THEN] ;Cond
Cond: [ELSE]    [ELSE] ;Cond

previous

\ save-cross                                           17mar93py

>CROSS
Create magic  s" Gforth10" here over allot swap move

char 1 bigendian + tcell + magic 7 + c!

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
  image @ there r@ write-file throw \ write image
  TNIL IF
      bit$  @ there 1- tcell>bit rshift 1+
                r@ write-file throw \ write tags
  THEN
  r> close-file throw ;

\ words that should be in minimal
>MINIMAL
also minimal

bigendian Constant bigendian
: save-cross save-cross ;
: here there ;
also forth 
[IFDEF] Label : Label Label ; [THEN] 
[IFDEF] start-macros : start-macros start-macros ; [THEN]
previous

: + + ;
: or or ;
: 1- 1- ;
: - - ;
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
: const ;

\ mach-file count included

: all-words    ['] false    IS skip? ;
: needed-words ['] needed?  IS skip? ;
: undef-words  ['] defined? IS skip? ;

: \  postpone \ ;  immediate
: \G T-\G ; immediate
: (  postpone ( ;  immediate
: include bl word count included ;
: .( [char] ) parse type ;
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

minimal

\ for debugging...
: order         order ;
: words         words ;
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

unlock definitions also minimal
: lock   lock ;
lock
