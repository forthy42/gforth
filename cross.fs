\ CROSS.FS     The Cross-Compiler                      06oct92py
\ $Id: cross.fs,v 1.21 1995-02-02 18:13:02 pazsan Exp $
\ Idea and implementation: Bernd Paysan (py)
\ Copyright 1992-94 by the GNU Forth Development Group

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

\ include other.fs       \ ansforth extentions for cross

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
\        2dup type space
        dup c, here over chars allot swap move align ;

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

\ Variables                                            06oct92py

-1 Constant NIL
Variable image
Variable tlast    NIL tlast !  \ Last name field
Variable tlastcfa \ Last code field
Variable tdoes    \ Resolve does> calls
Variable bit$
Variable tdp
: there  tdp @ ;

\ Parameter for target systems                         06oct92py

included

\ Create additional parameters                         19jan95py

T
cell               Constant tcell
cell<<             Constant tcell<<
cell>bit           Constant tcell>bit
bits/byte          Constant tbits/byte
float              Constant tfloat
1 bits/byte lshift Constant maxbyte
H

>TARGET

\ Byte ordering and cell size                          06oct92py

: cell+         tcell + ;
: cells         tcell<< lshift ;
: chars         ;
: floats	tfloat * ;
    
>CROSS
: cell/         tcell<< rshift ;
>TARGET
20 CONSTANT bl
-1 Constant NIL
-2 Constant :docol
-3 Constant :docon
-4 Constant :dovar
-5 Constant :douser
-6 Constant :dodefer
-7 Constant :dodoes
-8 Constant :doesjump

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
: makekernal ( targetsize -- targetsize )
  bit$  over 1- cell>bit rshift 1+ initmem
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
    cell tuck 1- and - [ cell 1- ] Literal and ;

>TARGET
: aligned ( taddr -- ta-addr )  dup align+ + ;
\ assumes cell alignment granularity (as GNU C)

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
: ,     ( w -- )        T here H cell T allot  ! H ;
: c,    ( char -- )     T here    1 allot c! H ;
: align ( -- )          T here H align+ 0 ?DO  bl T c, H LOOP ;

: A!                    dup relon T ! H ;
: A,    ( w -- )        T here H relon T , H ;

>CROSS

\ threading modell                                     13dec92py

\ generic threading modell
: docol,  ( -- ) :docol T A, 0 , H ;

>TARGET
: >body   ( cfa -- pfa ) T cell+ cell+ H ;
>CROSS

: dodoes, ( -- ) T :doesjump A, 0 , H ;

\ Ghost Builder                                        06oct92py

\ <T T> new version with temp variable                 10may93jaw

VARIABLE VocTemp

: <T  get-current VocTemp ! also Ghosts definitions ;
: T>  previous VocTemp @ set-current ;

4711 Constant <fwd>             4712 Constant <res>
4713 Constant <imm>

\ iForth makes only immediate directly after create
\ make atonce trick! ?

Variable atonce atonce off

: NoExec true ABORT" CROSS: Don't execute ghost" ;

: GhostHeader <fwd> , 0 , ['] NoExec , ;

: >magic ; : >link cell+ ; : >exec cell+ cell+ ;
: >end 3 cells + ;

Variable last-ghost
: Make-Ghost ( "name" -- ghost )
  >in @ GhostName swap >in !
  <T Create atonce @ IF immediate atonce off THEN
  here tuck swap ! ghostheader T>
  DOES> dup last-ghost ! >exec @ execute ;

\ ghost words                                          14oct92py
\                                          changed:    10may93py/jaw

: gfind   ( string -- ghost true/1 / string false )
\ searches for string in word-list ghosts
\ !! wouldn't it be simpler to just use search-wordlist ? ae
  dup count [ ' ghosts >body ] ALiteral search-wordlist
  dup IF  >r >body nip r>  THEN ;

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
  ELSE true ABORT" CROSS: Ghostnames inconsistent"
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
      abort" Unresolved words!"
  ELSE
      ." Nothing!"
  THEN
  cr ;

>CROSS
\ Header states                                        12dec92py

: flag! ( 8b -- )   tlast @ dup >r T c@ xor r> c! H ;

VARIABLE ^imm

>TARGET
: immediate     20 flag!
                ^imm @ @ dup <imm> = IF  drop  EXIT  THEN
                <res> <> ABORT" CROSS: Cannot immediate a unresolved word"
                <imm> ^imm @ ! ;
: restrict      40 flag! ;
>CROSS

\ ALIAS2 ansforth conform alias                          9may93jaw

: ALIAS2 create here 0 , DOES> @ execute ;
\ usage:
\ ' <name> alias2 bla !

\ Target Header Creation                               01nov92py

: string,  ( addr count -- )
  dup T c, H bounds  DO  I c@ T c, H  LOOP ; 
: name,  ( "name" -- )  bl word count string, T align H ;
: view,   ( -- ) ( dummy ) ;

VARIABLE CreateFlag CreateFlag off

: (Theader ( "name" -- ghost ) T align H view,
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
  80 flag! ;

VARIABLE ;Resolve 1 cells allot

: Theader  ( "name" -- ghost )
  (THeader dup there resolve 0 ;Resolve ! ;

>TARGET
: Alias    ( cfa -- ) \ name
  (THeader over resolve T A, H 80 flag! ;
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
ghost (;code)   ghost noop                      2drop
ghost (.")      ghost (S")      ghost (ABORT")  2drop drop
ghost '

\ compile                                              10may93jaw

: compile  ( -- ) \ name
  restrict?
  bl word gfind dup 0= ABORT" CROSS: Can't compile "
  0> ( immediate? )
  IF    >exec @ compile,
  ELSE  postpone literal postpone gexecute  THEN ;
                                        immediate

>TARGET
: '  ( -- cfa ) bl word gfind 0= ABORT" CROSS: undefined "
  dup >magic @ <fwd> = ABORT" CROSS: forward " >link @ ;

Cond: [']  compile lit ghost gexecute ;Cond

Cond: chars ;Cond

>CROSS
\ tLiteral                                             12dec92py

: lit, ( n -- )   compile lit T  ,  H ;
: alit, ( n -- )  compile lit T A,  H ;

>TARGET
Cond:  Literal ( n -- )   restrict? lit, ;Cond
Cond: ALiteral ( n -- )   restrict? alit, ;Cond

: Char ( "<char>" -- )  bl word char+ c@ ;
Cond: [Char]   ( "<char>" -- )  restrict? Char  lit, ;Cond

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
  (THeader ;Resolve ! there ;Resolve cell+ !
  docol, depth T ] H ;

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
: !does  :dodoes tlastcfa @ tuck T ! cell+ ! H ;

>TARGET
Cond: DOES> restrict?
        compile (;code) dodoes, tdoes @ ?dup IF  @ T here H resolve THEN
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
  IF dup >link @ dup 0< IF T A, 0 , H drop EXIT THEN drop THEN
  :dodoes T A, H gexecute T here H cell - reloff ;

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

: ;DO ( addr [xt] [colon-sys] -- )
  postpone ;    ( S addr xt )
  over >exec ! ; immediate

: by      ( -- addr ) \ Name
  ghost >end @ ;

>TARGET
\ Variables and Constants                              05dec92py

Build:  ;
DO: ( ghost -- addr ) ;DO
Builder Create
by Create :dovar resolve

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
DO: ( ghost -- up-addr )  T @ H tup @ + ;DO
Builder User
by User :douser resolve

Build: T 0 u, , 0 u, drop H ;
by User
Builder 2User

Build: T 0 au, , H ;
by User
Builder AUser

Build:  ( n -- ) T , H ;
DO: ( ghost -- n ) T @ H ;DO
Builder Constant
by Constant :docon resolve

Build:  ( n -- ) T A, H ;
by Constant
Builder AConstant

Build: T 0 , H ;
by Constant
Builder Value

Build:  ( -- ) compile noop ;
DO: ( ghost -- ) ABORT" CROSS: Don't execute" ;DO
Builder Defer
by Defer :dodefer resolve

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

\ Structural Conditionals                              12dec92py

Cond: DO        restrict? compile (do)   T here H ;Cond
Cond: ?DO       restrict? compile (?do)  (leave T here H ;Cond
Cond: FOR       restrict? compile (for)  T here H ;Cond

>CROSS
: loop]   dup <resolve cell - compile DONE compile unloop ;
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

: there? bl word gfind IF >magic @ <fwd> <> ELSE drop false THEN ;

: [IFDEF] there? postpone [IF] ;
: [IFUNDEF] there? 0= postpone [IF] ;

\ C: \- \+ Conditional Compiling                         09jun93jaw

: C: >in @ there? 0=
     IF    >in ! T : H
     ELSE drop
        BEGIN bl word dup c@
              IF   count comment? s" ;" compare 0= ?EXIT
              ELSE refill 0= ABORT" CROSS: Out of Input while C:"
              THEN
        AGAIN
     THEN ;

also minimal

: \- there? IF postpone \ THEN ;
: \+ there? 0= IF postpone \ THEN ;

: [IF]   postpone [IF] ;
: [THEN] postpone [THEN] ;
: [ELSE] postpone [ELSE] ;

Cond: [IF]      [IF] ;Cond
Cond: [IFDEF]   [IFDEF] ;Cond
Cond: [IFUNDEF] [IFUNDEF] ;Cond
Cond: [THEN]    [THEN] ;Cond
Cond: [ELSE]    [ELSE] ;Cond

\ save-cross                                           17mar93py

\ i'm not interested in bigforth features this time    10may93jaw
\ [IFDEF] file
\ also file
\ [THEN]
\ included throw after create-file                     11may93jaw

bigendian Constant bigendian

: save-cross ( "name" -- )
  bl parse ." Saving to " 2dup type
  w/o bin create-file throw >r
  s" gforth00"  r@ write-file throw \ write magic
  image @ there r@ write-file throw \ write image
  bit$  @ there 1- cell>bit rshift 1+
                r@ write-file throw \ write tags
  r> close-file throw ;

\ words that should be in minimal

: + + ;         : 1- 1- ;
: - - ;         : 2* 2* ;
: * * ;         : / / ;
: dup dup ;     : over over ;
: swap swap ;   : rot rot ;
: drop drop ;   : =   = ;
: lshift lshift ; : 2/ 2/ ;
: . . ;
cell constant cell

\ include bug5.fs
\ only forth also minimal definitions

: \ postpone \ ;
: ( postpone ( ;
: include bl word count included ;
: .( [char] ) parse type ;
: cr cr ;

: times 0 ?DO dup T c, H LOOP drop ; \ used for space table creation
only forth also minimal definitions

\ cross-compiler words

: decimal       decimal ;
: hex           hex ;

: tudp          T tudp H ;
: tup           T tup H ;  minimal

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
