\ ShBoom compiler

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
ToDo:
Get constant optimization working. Split docon: / doval:

[THEN]


hex

[IFUNDEF] X 
unlock >TARGET
.( Jo!)
\ Cond: X ;Cond 
: Cond: T : H  ;
Cond: ;Cond compile ; T immediate H ;Cond
: \C postpone \ ;
: \S ;
Cond: \C postpone \ ;Cond
Cond: \S ;Cond

lock
: X ;
[ELSE]
: \C ; immediate
: \S postpone \ ; immediate
[THEN]

\C >CROSS
4c Constant _not_reached
ea Constant _nop
08 Constant _call
00 Constant _br
10 Constant _bz
6e Constant _ret
90 Constant _push.b
4f Constant _push.l
30 Constant _skip

ea Constant _filler

\ new ones with latch!

Variable IG 0 IG !
Variable I-Latch
Variable I-Nr
Variable I-Max

: igflush

    IG @ 0= IF EXIT THEN
    4 0 DO  I I-Nr @ u< I-Max @ 4 <> I 3 = and or
            IF   I-Latch I + c@
            ELSE _filler THEN
            IG @ I + X c!
    LOOP
    0 I-Nr !
    4 I-Max !
    0 IG ! ;

: igalloc

    igflush
    X here IG !
    X cell X allot ;

: prim,

    I-Nr @ I-Max @ = IG @ 0= or
    IF igalloc THEN
    I-Latch I-Nr @ + c!
    1 I-Nr +!
    I-Nr @ I-Max @ =
    IF igflush THEN ;

: byte,

    I-Nr @ 2 u> ABORT" byte, should 2 left"
    I-Max @ 4 <> ABORT" byte, not free"

    -1 I-Max +!
    I-Latch 3 + c! ;

: bytefree?
    I-Max @ 4 = ;

: long,
     X , ;

: rest
     I-Max @ I-Nr @ - ;

: group
     IG @ dup 0= IF drop X here THEN ;

: filler,   _filler prim, ;

: quads                                 \ fills with nops
  0 ?DO filler, LOOP ;

: fillup
        rest 4 <> IF igflush THEN ;

: needed rest swap u< IF fillup THEN ;

: $num
  base @ hex
  0 0 bl word count >number 2drop drop
  swap base ! ;

\C >TARGET
\ \S : .quad2 2 needed ; immediate
\ \S : .quad3 3 needed ; immediate
\ \S : .quad4 4 needed ; immediate

Cond: .quad2 2 needed ;Cond
Cond: .quad3 3 needed ;Cond 
Cond: .quad4 4 needed ;Cond 

\C >CROSS

\ --------------------- Nesting

: offset ( adr -- off )
  group - 2/ 2/ ;

: bits? ( offset nr -- offset flag )
  1 swap lshift over u> ;

decimal
: range? ( offset -- nr )
  dup 0<
  IF abs 1- ELSE THEN
  2 bits? IF drop 1 EXIT THEN
  10 bits? IF drop 2 EXIT THEN
  18 bits? IF drop 3 EXIT THEN
  26 bits? IF drop 4 EXIT THEN
  -1 ABORT" offset out of range" ;
hex

: GenericBranch, ( adr opcode -- )
  >r dup offset dup range? dup >r rest u> bytefree? 0= or
  IF 	fillup
        \ recalculate offset
        rdrop drop
        offset dup range? >r
  ELSE  nip THEN
  \ rest r@ - quads
  rdrop rest >r
  ( offset R: opcode restbytes ) \ extract offset bytes
  r@ 0 ?DO dup 0ff and swap 8 rshift LOOP drop
  \ get len and or call-opcode into the first byte
  r> swap 07 and r> or swap 
  0 ?DO prim, LOOP ;

\ automatic inline detection				15jul97jaw

Variable filler-cnt

: noinline? ( byte -- flag )
  dup $BB = IF EXIT THEN	\ add_pc
  dup $9A = IF EXIT THEN    \ r>
  dup _push.b = IF EXIT THEN \ push.b ?
  dup _push.l = IF EXIT THEN \ push.l ?
  \ we count for fillers
  \ too much fillers means we have a forward reference!!
  dup _filler = IF 1 filler-cnt +! THEN
  filler-cnt @ 2 u> IF drop true EXIT THEN 
  020 u<    \ branch?
  ;

: term? ( byte -- flag )
  _ret = ;

\ inline detection					15jul97jaw

\ an inline function is marked when the word begins with a skip

: definline? ( adr -- flag )
  X c@ _skip = ;

: @inlineflag ( adr -- flag )
  1+ X c@ 0<> ;

: forced-inline? ( adr -- flag )
  dup definline?
  IF @inlineflag
  ELSE drop false THEN ;

: autoinline? ( adr -- flag )
  \ flag detection
  dup definline? IF @inlineflag EXIT THEN
  \ no of bytes to compile inline
  4 
  \ because for forward references there is only 4 bytes room
\C  comp-state @ Resolving = IF 4 min THEN
  0 filler-cnt !
  1+ >r \ maximum bytes for automatic inline declarations
  BEGIN dup X c@ noinline? IF drop rdrop false EXIT THEN
	dup X c@ term? IF drop rdrop true EXIT THEN
	1+ r> 1- dup >r
  	0=
  UNTIL rdrop drop false ;

: SkipInlineMark ( adr -- adr2 )
  dup definline? IF X cell+ THEN ;

: inline?
  autoinline? ;

: compile-inline ( adr -- )
  BEGIN dup X c@ dup term? 0=
  WHILE prim, 1+
  REPEAT 2drop ;

: smart-colon ( xt -- )
  dup inline? 
  IF    \ dup gdiscover .sourcepos ." Inline: " IF @name type ELSE . THEN
        SkipInlineMark compile-inline
  ELSE  _call GenericBranch,
  THEN ;

[IFDEF] colonmark,
:noname (  -- addr )
  4 needed X here 4 quads ;	IS colonmark,
[THEN]

\ --------------------- Literals

\ lit optimization (gets long)                          06aug97jaw

: nibble-lit? ( n -- n false | true )
  dup -7 9 within dup
  IF swap $f and $20 or prim, THEN ;

: byte-lit?   ( n -- n false | true )
  dup $100 u< bytefree? and rest 1 u> and dup
  IF swap _push.b prim, I-Latch 3 + c!
     3 I-Max !
  THEN ;

: opt-lit?
  nibble-lit? ?dup IF EXIT THEN
  byte-lit? ?dup IF EXIT THEN
  false ;

: (lit,)
  opt-lit? IF EXIT THEN
  4f prim, 
  long, ;               ' (lit,) IS lit,

\ Wordinfo for cross

\C 1
\S 0
[IF]
: con? ( xt -- flag )
  X @
  [G'] :docon
  dup forward? IF 2drop false EXIT THEN
  ghost>cfa SkipInlineMark
  X @ = ;

: var? ( xt -- flag )
  X @
  [G'] :dovar
  dup forward? IF 2drop false EXIT THEN
  ghost>cfa SkipInlineMark
  X @ = ;
[ELSE]
: con? ( xt -- flag )
  >code-address docon: = ;

: var? ( xt -- flag )
  >code-address dovar: = ;

[THEN]

: nocolon? ( xt -- xt false | true )
\ constant optimization is currently switched off 
\ because we cannot detect the difference between constant and value
\ T dup con?
\ T IF xt>body + X @ lit, true EXIT THEN
  dup var?
  IF ." V" xt>body + lit, true EXIT THEN
  false ;

: (compile,) ( xt -- )
  dup -1 = IF ABORT" -1 compile,!" EXIT THEN
  dup 0100 u< 
  IF 	\ cpu-primitive
	prim,
  ELSE	\ colon definition call
        \ is this a constant?
        nocolon? IF EXIT THEN
        \ compile call or inline
	smart-colon
  THEN ;

Cond: M $num (compile,) ;Cond

Cond: _inline rest 4 <> ABORT" inline statement not at beginning!"
	 _skip (compile,) 1 prim,  4 needed ;Cond	 

Cond: _noinline rest 4 <> ABORT" noinline statement not at beginning!"
	 _skip (compile,) 0 prim, 4 needed ;Cond	 

: (bd) 4 needed ;                       ' (bd) is branchto,

' igflush IS comp[

\C :noname compile ;s _not_reached prim, igflush ;  is fini,
\S :noname postpone ;s _not_reached prim, igflush ;  is fini,

:noname
  ig @ IF ." ig left!!!!!" THEN
  ; is docol,

' (compile,) IS colon,



\C 1
\S 0 
[IF]
\ inline support for doers                              07aug97jaw
\ cross version

: doinline, ( xt -- )
  X here >r
  docol,
  ]comp
  gexecute
  fini,
  comp[ 
  X here r> - 1- X cell / 1+
  fillcfa ;

:noname
  dup [G'] :douser =
  IF doinline, EXIT THEN
  dup [G'] :dovar = over [G'] :docon = or 
  IF doinline, EXIT THEN
  (doer,) ; IS doer,
[ELSE]

: doinline2,
  ]comp
  colon,
  postpone ;s
  comp[ 2 fillcfa ;

: doinline,
  ]comp
  colon,
  postpone ;s
  comp[ 1 fillcfa ;

:noname ( ghost -- )
  dup douser: =
  IF doinline2, EXIT THEN
  dup dovar: = over docon: = or 
  IF doinline, EXIT THEN
  (doer,) ; IS doer,

[THEN]

\ Conditionals                                       07aug97   

:noname _br GenericBranch, ; IS branch,
:noname _bz GenericBranch, ; IS ?branch,
:noname 2 needed Group I-Nr @ + _br prim, IGFlush ; IS branchmark,
:noname 2 needed Group I-Nr @ + _bz prim, IGFlush ; IS ?branchmark,
:noname 4 needed Group ; IS branchtomark,

: restbytes ( adr -- )
\G returns the left bytes in our quad
   1 cells tuck 1- and - ;
\  align+ dup 0= IF drop 4 THEN ;

:noname
  \ check whether there is a branch instruction
  dup X c@ dup dup _br <> swap _bz <> or 0= ?struc
  4 needed              \ align destination to ig
  ( srcbranch opcode )
  swap >r r@ restbytes r@ + X cell - \ addr of source ig
  offset negate dup range?
  r@ restbytes u> ABORT" CROSS: 2 byte forward, not enough?!"
  ( opcode offset R: srcadr )
  r@ restbytes rot >r >r
  ( offset R: opcode restbytes ) \ extract offset bytes
  r@ 0 ?DO dup 0ff and swap 8 rshift LOOP drop
  \ get len and or call-opcode into the first byte
  r> swap 07 and r> or swap 
  0 ?DO J I + X c! LOOP rdrop ; IS branchtoresolve,

\ assembler extentions                                  09aug97jaw

: parsenum
  postpone [
  name evaluate ] ;

: (popg,)
  dup 0 16 within 0=
  ABORT" popg, not a register"
  $50 + prim, ;

: (popr,)
  dup 0 15 within 0=
  ABORT" popr, not a register"
  $a0 + prim, ;

: (pushg,)
  dup 0 16 within 0=
  ABORT" pushg, not a register"
  $50 + prim, ;

: (pushr,)
  dup 0 15 within 0=
  ABORT" pushg, not a register"
  $50 + prim, ;

Cond: popg, parsenum (popg,) ;Cond

Cond: popr, parsenum (popr,) ;Cond

Cond: pushg, parsenum (pushg,) ;Cond

Cond: pushr, parsenum (pushr,) ;Cond

