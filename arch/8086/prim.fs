\ **************************************************************
\ File:         PRIMS.FS
\                 Lowlevel routines for GFORTH on 8086 (PC)
\ Autor:        Klaus Kohl
\ Log:          30.07.97 KK:    file generated (from KK-FORTH)
\
\ * Register using for 8086 on PC (like KK-FORTH):
\  Intel  Forth    used for                       8bit-Register
\   BX     TOS   oberstes Stackelement             TOSH   TOSL
\   BP     FRP   Returnstack Pointer
\   SP     FSP   Stack Pointer
\   SI     FIP   Instruction Pointer
\   DI      W    Arbeitsregister
\
\ * Memory ( use only one 64K-Page ):
\    $0080-$00FF : TIB
\    $0100-$F800 : program
\    $F800-$FC00 : datastack
\    $FC00-$FFFF : returnstack
\ **************************************************************


start-macros
 \ register definition
  ' sp Alias fsp
  ' bp Alias frp
  ' bx Alias tos        ' bl Alias tosl  ' bh Alias tosh
  ' si Alias fip
  ' di Alias w

 \ system depending macros
  : next,
    lods,
    ax w xchg,
    w ) jmp, ;
\ note that this is really for 8086 and 286, and _not_ intented to run
\ fast on a Pentium (Pro). These old chips load their code from real RAM
\ and do it slow, anyway.
\ If you really want to have a fast 16 bit Forth on modern processors,
\ redefine it as
\ : next,  fip ) w mov,  2 # fip add,  w ) jmp, ;

end-macros

  unlock
    $0100 $f000 region dictionary
    setup-target
  lock

\ ==============================================================
\ rom starts with jump to GFORTH-kernel (must be at $0000 !!!)
\ ==============================================================
  Label into-forth
    $ffff # fip mov,            \ ip will be patched
    $fef0 # fsp mov,            \ sp at $FD80...$FEF0
    $fd80 # frp mov,            \ rp at $F.00...$FD80
    next,
  End-Label


\ output debug information
  Label (dout)   \ like (emit) with character in dl
    6 # ah mov,
    $21 int,
    ret,
  End-Label

Start-Macros
\  : dout,       ( char -- )
\     # dl byte mov,
\    (dout) # call, ;
 : dout,  drop ;               \ no debug output
end-macros


\ ==============================================================
\ GFORTH minimal primitive set
\ ==============================================================
 \ inner interpreter
  Code: :docol
     ': dout,                    \ only for debugging
     frp dec,   frp dec,   fip frp ) mov,        \ save ip
     4 w d) fip lea,                             \ calc pfa
     next,
   End-Code

  Code: :dovar
    '2 dout,                    \ only for debugging
    tos push,
    4 w d) tos lea,
    next,
  End-Code

  Code: :dodoes  ( -- pfa ) \ get pfa and execute DOES> part
    '6 dout,                    \ only for debugging
     frp dec,   frp dec,   fip frp ) mov,        \ save ip
     2 # w add,                                  \
     w ) fip mov,                                \ get does> address
     tos push,                                   \ save tos
     2 # w add,
     w tos mov,                                  \ copy pfa to tos
     next,                                       \ execute does> part
   End-Code


 \ program flow
  Code ;s       ( -- ) \ exit colon definition
    '; dout,                    \ only for debugging
    frp ) fip mov,   frp inc,  frp inc,         \ get ip
    next,
  End-Code

  Code execute   ( xt -- ) \ execute colon definition
    'E dout,                    \ only for debugging
    tos w mov,                                  \ copy tos to w
    tos pop,                                    \ get new tos
    w ) jmp,                                    \ execute
  End-Code

  Code ?branch   ( f -- ) \ jump on f<>0
    tos tos or,   tos pop,                      \ check and get new tos
    0= IF,  fip ) fip mov,      next,           \ jump
    ELSE,   fip inc,  fip inc,  next,  THEN,    \ skip
   End-Code


 \ memory access
  Code @        ( addr -- n ) \ read cell
    tos ) tos mov,
    next,
   End-Code

  Code !        ( n addr -- ) \ write cell
    tos ) pop,
    tos pop,
    next,
   End-Code


 \ datastack and returnstack address
  Code sp@      ( -- sp ) \ get stack address
    tos push,
    fsp tos mov,
    next,
   End-Code

  Code sp!      ( sp -- ) \ set stack address
    tos fsp mov,
    tos pop,
    next,
  End-Code

  Code rp@      ( -- rp ) \ get returnstack address
    tos push,
    frp tos mov,
    next,
  End-Code

  Code rp!      ( rp -- ) \ set returnstack address
    tos frp mov,
    tos pop,
    next,
  End-Code


 \ arithmetic and logic
  Code +        ( n1 n2 -- n3 ) \ addition
    ax pop,
    ax tos add,
    next,
   End-Code

  Code xor      ( n1 n2 -- n3 ) \ logic XOR
    ax pop,
    ax tos xor,
    next,
   End-Code

  Code and      ( n1 n2 -- n3 ) \ logic AND
    ax pop,
    ax tos and,
    next,
   End-Code


 \ i/o
  Variable lastkey      \ Flag und Zeichencode der letzen Taste

  Code (key)    ( -- char ) \ get character
    tos push,
    lastkey #) ax mov,
    ah ah or,  0= IF, 7 # ah mov,  $21 int, THEN,
    0 # lastkey #) mov,
    ah ah xor,
    ax tos mov,
    next,
   End-Code

  Code (emit)     ( char -- ) \ output character
    tosl dl mov,
    6 # ah mov,
    $ff # dl cmp,  0= IF, dl dec, THEN,
    $21 int,
    tos pop,
    next,
  End-Code

\ ==============================================================
\ additional words (for awaitable response)
\ ==============================================================
 \ memory character access
  Code c@       ( addr -- c ) \ read character
    tos ) tosl mov,
    tosh tosh xor,
    next,
   End-Code

  Code c!       ( c addr -- ) \ write character
    ax pop,
    al tos ) mov,
    tos pop,
    next,
   End-Code


 \ moving datas between stacks
  Code r>       ( -- n ; R: n -- )
    tos push,
    frp ) tos mov,  frp inc,  frp inc,
    next,
   End-Code

  Code >r       ( n -- ; R: -- n )
    frp dec,  frp dec,  tos frp ) mov,
    tos pop,
    next,
   End-Code

\ ==============================================================
\ usefull lowlevel words
\ ==============================================================
 \ word definitions

  Code: :docon
    '1 dout,                    \ only for debugging
    tos push,
    4 w d) tos lea,
    tos ) tos mov,
    next,
  End-Code

  Code: :dodefer
    '4 dout,                    \ only for debugging
    4 w d) w mov,
    w ) jmp,
  End-Code


 \ branch and literal
  Code branch   ( -- ) \ unconditional branch
    fip ) fip mov,
    next,
   End-Code

  Code lit     ( -- n ) \ inline literal
    tos push,
    lods,
    ax tos mov,
    next,
   End-Code


 \ data stack words
  Code dup      ( n -- n n )
    tos push,
    next,
   End-Code

  Code 2dup     ( d -- d d )
    ax pop,
    ax push,
    tos push,
    ax push,
    next,
   End-Code

  Code drop     ( n -- )
    tos pop,
    next,
   End-Code

  Code 2drop    ( d -- )
    2 # fsp add,
    tos pop,
    next,
   End-Code

  Code swap     ( n1 n2 -- n2 n1 )
    ax pop,
    tos push,
    ax tos mov,
    next,
   End-Code

  Code over     ( n1 n2 -- n1 n2 n1 )
    tos ax mov,
    tos pop,
    tos push,
    ax push,
    next,
   End-Code

  Code rot      ( n1 n2 n3 -- n2 n3 n1 )
    tos ax mov,
    cx pop,
    tos pop,
    cx push,
    ax push,
    next,
   End-Code

  Code -rot     ( n1 n2 n3 -- n3 n1 n2 )
    tos ax mov,
    tos pop,
    cx pop,
    ax push,
    cx push,
    next,
   End-Code


 \ return stack
  Code r@       ( -- n ; R: n -- n )
    tos push,
    frp ) tos mov,
    next,
  End-Code


 \ arithmetic
  Code -        ( n1 n2 -- n3 ) \ Subtraktion
    ax pop,
    tos ax sub,
    ax tos mov,
    next,
   End-Code

  Code um*      ( u1 u2 -- ud ) \ unsigned multiply
    tos ax mov,
    cx pop,
    cx mul,
    ax push,
    dx tos mov,
    next,
   End-Code

  Code um/mod   ( ud u -- r q ) \ unsiged divide
    tos cx mov,
    dx pop,
    ax pop,
    cx div,
    dx push,
    ax tos mov,
    next,
   End-Code


 \ logic
  Code or       ( n1 n2 -- n3 ) \ logic OR
    ax pop,   ax tos or,   next,
   End-Code


 \ shift
  Code 2/       ( n1 -- n2 ) \ arithmetic shift right
     tos sar,
     next,
   End-Code

  Code lshift   ( n1 n2 -- n3 ) \ shift n1 left n2 bits
     tos cx mov,
     tos pop,
     cx cx or,  0<> IF, tos c* shl, THEN,
     next,
   End-Code

  Code rshift   ( n1 n2 -- n3 ) \ shift n1 right n2 bits
     tos cx mov,
     tos pop,
     cx cx or,  0<> IF, tos c* shr, THEN,
     next,
   End-Code


 \ compare
  Code 0=       ( n -- f ) \ Test auf 0
    tos tos or,
    0 # tos mov,
    0= IF, tos dec, THEN,
    next,
   End-Code

  Code =        ( n1 n2 -- f ) \ Test auf Gleichheit
    ax pop,
    ax tos sub,
    0= IF,  -1 # tos mov,   next,
    ELSE,   0  # tos mov,   next,
    THEN,
   End-Code


 \ additon io routines
  Code (key?)     ( -- f ) \ check for read sio character
    tos push, lastkey # tos mov,
    1 tos d) ah mov,   ah ah or,
    0= IF,  $ff # dl mov,  6 # ah mov,  $21 int,
            0 # ah mov,
            0<> IF, dl ah mov,   ax tos ) mov, THEN,
    THEN,  ah tosl mov,   ah tosh mov,
    next,
   End-Code

  Code emit?    ( -- f ) \ check for write character to sio
    tos push,
    -1 # tos mov,             \ output always possible
    next,
   End-Code

\ ======================== not ready ============================
0 [IF]  \ not jet adapted

\ ======================== not ready ============================
[ENDIF]

  Code (bye)     ( -- ) \ back to DOS
     ax pop,  $4c # ah mov,  $21 int,
    End-Code

: bye ( -- )  0 (bye) ;
    
Code: :doesjump
end-code
