: p: opt( '' )opt: ;
: (remove-prim) 0 1 opt-remove ;
: p; postpone (remove-prim) postpone ;opt ; immediate

p: ;s ( -- )
   regalloc-reset
   regalloc-flush
   ret,
p;

p: @                     ( addr -- n )
( OK )
  regalloc-reset
  #tos-cache #USEREGS = IF
    req-any 
    0 [tos0] tos0 mov, 
  ELSE
    req-any req-free
    0 [tos0] free0 mov,
    1 reg-free
    0 free>tos
  THEN 
  p;

p: !                     ( n addr -- )
( OK )
  regalloc-reset
  req-any
  req-any
  tos1 0 [tos0] mov, 
  2 reg-free p;

p: 1+                    ( n -- n+1 )
( OK )
  regalloc-reset
  req-any 
  tos0 inc, p;

p: CHAR+  			( addr -- addr+char)
( OK )
  regalloc-reset
  req-any 
  tos0 inc, p; 

p: 1-
( OK )
  regalloc-reset
  req-any 
  tos0 dec, p;

p: 2*
( OK )
  regalloc-reset
  req-any 
  1 ## tos0 shl, p;

p: 2/
( OK )
  regalloc-reset
  req-any 
  1 ## tos0 sar, p;

p: AND
( OK )
  regalloc-reset
  req-any
  req-any
  0 1 tos-swap
  tos0 tos1 and, 
  1 reg-free p;

p: OR
( OK )
  regalloc-reset
  req-any
  req-any
  0 1 tos-swap
  tos0 tos1 or, 
  1 reg-free p;

p: XOR
( OK )
  regalloc-reset
  req-any
  req-any
  0 1 tos-swap
  tos0 tos1 xor, 
  1 reg-free p;

p: * 					( t1 t0 -- t1*t0 )
( OK )
  regalloc-reset
  req-edx 			\ tos0
  req-eax 			\ tos1
  tos0 mul, 
  1 reg-free p;

p: +
( OK )
  regalloc-reset
  req-any req-any
  0 1 tos-swap
  tos0 tos1 add, 
  1 reg-free p;

p: -
( OK )
  regalloc-reset
  req-any req-any
  tos0 tos1 sub, 
  1 reg-free p;

p: INVERT
( OK )
  regalloc-reset
  req-any 
  tos0 not, p;

p: NEGATE
( OK )
  regalloc-reset
  req-any 
  tos0 neg, p;

p: C@
( OK )
  regalloc-reset
  req-any
  a-d-free
  free0 free0 xor,
  0 [tos0] free0l mov,
  1 reg-free 
  0 free>tos p;

p: C! ( c addr -- )
( OK )
  regalloc-reset
  req-any req-a-d
  tos1l 0 [tos0] mov, 
  2 reg-free p;

p: +!
( OK )
  regalloc-reset
  req-any
  req-any
  tos1 0 [tos0] add, 
  2 reg-free p;

p: 2!
( OK )
  regalloc-reset
  req-any
  req-any
  req-any
  tos1 0       [tos0] mov,
  tos2 1 CELLS [tos0] mov, 
  3 reg-free p;

p: 2@
( OK )
  regalloc-reset
  req-any
  req-free
  req-free
  0       [tos0] free0 mov,
  1 CELLS [tos0] free1 mov,
  1 reg-free
  1 free>tos 
  0 free>tos p;

p: 2OVER 		( n1 n2 n3 n4 --- n1 n2 n3 n4 n1 n2 )
( OK )
  regalloc-reset
  req-any req-any req-any req-any
  req-free req-free
  tos3 free0 mov,
  tos2 free1 mov,
  0 free>tos 
  1 free>tos p;

p: 2SWAP 			( n1 n2 n3 n4 --- n3 n4 n1 2n )
( OK )
  regalloc-reset
  req-any req-any req-any req-any
  1 3 tos-swap 
  0 2 tos-swap p;

p: 2DUP 		( n1 n2 -- n1 n2 n1 n2 )
( OK )
  regalloc-reset
  req-any req-any 
  req-free req-free
  tos1 free0 mov,
  tos0 free1 mov,
  0 free>tos
  1 free>tos p;

p: DROP 			( n -- )
( OK )
  regalloc-reset
  req-any 
  1 reg-free p;

p: NIP 				( a b -- b )
( OK )
  regalloc-reset
  req-any req-any 
  0 1 tos-swap 1 reg-free p;

p: 2DROP 			( n1 n2 -- )
( OK )
  regalloc-reset
  req-any req-any
  2 reg-free p;

p: DUP 				( n -- n  n)
( OK )
  regalloc-reset
  req-any
  req-free
  tos0 free0 mov, 
  0 free>tos 
  p;

p: OVER 			( n1 n2 -- n1 n2 n1 )
( OK )
  regalloc-reset
  req-any
  req-any
  req-free
  tos1 free0 mov, 
  0 free>tos p;

p: ROT 				( n1 n2 n3 --- n2 n3 n1 )
( OK )
  regalloc-reset
  req-any req-any req-any
  0 1 tos-swap 		\ t2 t0 t1 
  0 2 tos-swap p; 	\ t1 t0 t2 

\ Put the top of stack value below the two next values. Inverse operation
\ to ROT.
p: -ROT 			( n1 n2 n3 --- n3 n1 n2 )
( OK )
  regalloc-reset
  req-any req-any 
  req-any 		\ t2 t1 t0
  0 1 tos-swap 		\ t2 t0 t1 
  1 2 tos-swap p; 	\ t0 t2 t1

p: TUCK 			( t1 t0 -- t0 t1 t0 )
( OK )
  regalloc-reset
  req-any req-any 
  req-free
  tos0 free0 mov,
  0 1 tos-swap 		\ t0 t1
  0 free>tos 		\ t0 t1 f0
p;

p: D0=
( OK )
  regalloc-reset
  req-any req-any
  tos0 tos1 or,
  tos1l setz,
  31 ## tos1 shl, 
  31 ## tos1 sar, 
  1 reg-free p;
  
p: SWAP
( OK )
  regalloc-reset
  req-any
  req-any 
  0 1 tos-swap p;

\ The following, return stack manipulating, words need an explanation. Since
\ the top of the return stack must be the address of the c stack frame, we
\ need to get the top item from the stack, manipulate the stack and put it
\ back.
p: 2>R 			
( OK )
  regalloc-reset
  req-any req-any req-free
  free0 pop,
  tos1 push,
  tos0 push,
  free0 push,
  2 reg-free
p;

p: 2R>
( OK )
  regalloc-reset
  req-free req-free req-free
  free2 pop,
  free0 pop,
  free1 pop,
  free2 push,
  1 free>tos
  0 free>tos
p;

p: 2R@
  regalloc-reset
  req-free req-free
  4 [esp] free0 mov,
  8 [esp] free1 mov,
  1 free>tos
  0 free>tos p;

p: >R
( OK )
  regalloc-reset
  req-any req-free
  free0 pop,
  tos0 push, 
  free0 pop,
  1 reg-free 
  p;

p: R>
( OK )
  regalloc-reset
  req-free req-free
  free1 pop,
  free0 pop,
  free1 push, 
  0 free>tos p;

p: R@
  regalloc-reset
  req-free req-free
  free1 pop,
  0 [esp] free0 mov,
  free1 push,
  0 free>tos 
  p;

opt( '' DUP '' >R )opt:
  regalloc-reset
  req-any req-free
  free0 pop,
  tos0 push,
  free0 push,
  0 2 opt-remove
;opt

opt( '' R> '' DROP )opt:
  regalloc-reset
  req-free req-free
  free1 pop,
  free0 pop,
  free1 push,
  0 2 opt-remove
;opt

opt( '' DROP '' R> )opt:
  regalloc-reset
  req-any req-free
  free0 pop,
  tos0 pop,
  free0 push,
  0 2 opt-remove
;opt

p: LSHIFT 				( x1 u -- x2 )
( OK )
  regalloc-reset
  req-ecx
  req-any
  tos0 tos1 shl, 
  1 reg-free p;

p: RSHIFT 				( x1 u -- x2 )
( OK )
  regalloc-reset
  req-ecx 				\ tos0
  req-any 				\ tos1
  tos0 tos1 shr, 
  1 reg-free p;

p: 0= 					( n -- f )
( OK )
  regalloc-reset
  req-a-d
  tos0 tos0 or,
  tos0l setz,
  31 ## tos0 shl, 
  31 ## tos0 sar, p;

p: 0< 					( n -- f )
( OK )
  regalloc-reset
  req-a-d
  tos0 tos0 or,
  tos0l setl,
  31 ## tos0 shl, 
  31 ## tos0 sar, p;

p: 0> 					( n -- f )
( OK )
  regalloc-reset
  req-a-d
  tos0 tos0 or,
  tos0l setg,
  31 ## tos0 shl, 
  31 ## tos0 sar, p;

p: 0<> 					( n -- f )
( OK )
  regalloc-reset
  req-a-d
  tos0 tos0 or,
  tos0l setnz,
  31 ## tos0 shl, 
  31 ## tos0 sar, p;
 
p: = 					( n1 n2 -- f )
( OK )
  regalloc-reset
  req-any
  req-any
  a-d-free
  free0 free0 xor,
  tos0 tos1 cmp,
  free0l setne,
  free0 dec,
  2 reg-free 
  0 free>tos p;

p: <> 					( n1 n2 -- f )
( OK )
  regalloc-reset
  req-any
  req-any
  a-d-free
  free0 free0 xor,
  tos0 tos1 cmp,
  free0l sete,
  free0 dec,
  2 reg-free 
  0 free>tos p;

p: < 					( n1 n2 -- f )
( OK )
  regalloc-reset
  req-any
  req-any
  a-d-free
  free0 free0 xor,
  tos0 tos1 cmp,
  free0l setge,
  free0 dec,
  2 reg-free 
  0 free>tos p;

\ Perform a less or equal comparison. Equivalent to > INVERT
p: <= 					( n1 n2 -- f )
( OK )
  regalloc-reset
  req-any
  req-any
  a-d-free
  free0 free0 xor,
  tos0 tos1 cmp,
  free0l setg,
  free0 dec,
  2 reg-free 
  0 free>tos p;

p: > 					( n1 n2 -- f )
( OK )
  regalloc-reset
  req-any
  req-any
  a-d-free
  free0 free0 xor,
  tos0 tos1 cmp,
  free0l setle,
  free0 dec,
  2 reg-free 
  0 free>tos p;

\ Perform a greater or equal comparison. Equivalent to < INVERT
p: >= 					( n1 n2 -- f )
( OK )
  regalloc-reset
  req-any
  req-any
  a-d-free
  free0 free0 xor,
  tos0 tos1 cmp,
  free0l setl,
  free0 dec,
  2 reg-free 
  0 free>tos p;

p: U< 					( n1 n2 -- f )
( OK )
  regalloc-reset
  req-any
  req-a-d
  tos0 tos1 cmp,
  tos1l setb,
  31 ## tos1 shl,
  31 ## tos1 sar,
  1 reg-free p;

p: U> 					( n1 n2 -- f )
( OK )
  regalloc-reset
  req-any
  req-a-d
  tos0 tos1 cmp,
  tos1l seta,
  31 ## tos1 shl,
  31 ## tos1 sar,
  1 reg-free p;

p: 2ROT ( x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2 )
( OK )
  regalloc-reset
  req-any req-any req-any
  req-any req-any req-any 		\ t5 t4 t3 t2 t1 t0
  0 4 tos-swap 				\ t5 t0 t3 t2 t1 t4
  1 5 tos-swap 				\ t1 t0 t3 t2 t5 t4
  2 4 tos-swap 				\ t1 t2 t3 t0 t5 t4
  3 5 tos-swap p; 			\ t3 t2 t1 t0 t5 t4

p: FILL 				( addr cnt char -- )
( OK )
  regalloc-reset
  req-eax 				\ eax=char
  req-ecx 				\ ecx=cnt
  req-edi 				\ edi=addr
  rep, stosb,
  3 reg-free p;

p: D+ 					( d1 d2 -- d1+d2 )
( OK )
  regalloc-reset
  req-any req-any req-any req-any
  tos1 tos3 add,
  tos0 tos2 adc,
  2 reg-free p;

p: D- 					( d1 d2 -- d1-d2 )
( OK )
  regalloc-reset
  req-any req-any req-any req-any
  tos1 tos3 sub,
  tos0 tos2 sbb,
  2 reg-free p;

p: D2* 					( d1 -- d2 )
( OK )
  regalloc-reset
  req-any req-any
  clc,
  1 ## tos1 rcl,
  1 ## tos0 rcl, p;

p: D2/ 					( d1l d1h -- d2l d2h )
( OK )
  regalloc-reset
  req-any req-any req-free
  tos0 free0 mov,
  1 ## free0 rcl,
  1 ## tos0 rcr,
  1 ## tos1 rcr, p;

p: D0< 					( dl dh -- flag )
( OK )
  regalloc-reset
  req-any req-any
  tos0 tos1 mov,
  31 ## tos1 sar,
  1 reg-free p;

p: DNEGATE 				( dl dh -- d1l d1h )
( OK )
  regalloc-reset
  req-any req-any
  tos0 neg,
  tos1 neg,
  0 ## tos0 sbb, p;

p: CMOVE 				( a1 a2 cnt -- )
( OK )
  regalloc-reset
  req-ecx 				\ ecx=cnt
  req-edi 				\ edi=a2
  req-esi 				\ esi=a1
  rep, movsb,
  3 reg-free p;

p: CMOVE> 				( a1 a2 cnt -- )
( OK )
  regalloc-reset
  req-ecx 				\ ecx=cnt
  req-edi 				\ edi=a2
  req-esi 				\ esi=a1
  ecx edi add,
  ecx esi add,
  edi dec,
  esi dec,
  std,
  rep, movsb,
  cld,
  3 reg-free p;

p: M* 					( n1 n2 -- d )
( OK )
  regalloc-reset
  req-edx
  req-eax
  edx imul, p;

p: UM* 					( u1 u2 -- ud )
( OK )
  regalloc-reset
  req-edx
  req-eax
  edx mul, p;

p: UM/MOD 				( ud un -- ur uq )
( OK )
  regalloc-reset
  req-any 				\ un=tos0
  req-edx 				\ udh=edx=tos1=rem
  req-eax 				\ udl=eax=tos2=quot
  tos0 div,
  1 2 tos-swap
  1 reg-free p;

p: SM/REM 				( d1l d1h n1 -- nrem nquot )
( OK )
  regalloc-reset
  req-ebx
  req-edx
  req-eax
  ebx idiv,
  1 reg-free
  0 1 tos-swap p;

p: SP@ 				( -- sp )
( OK )
  regalloc-reset
  req-free
  ebp free0 mov,
  offs-ebp ## free0 add,
  0 free>tos p;

p: SP! 				( sp -- )
( OK )
  regalloc-reset
  regalloc-flush
  req-any
  1 reg-free
  0 TO offs-ebp
  eax ebp mov, p;

\ Retrieve the current return stack pointer.
p: RP@ 				( -- rp )
( OK )
  regalloc-reset
  req-free
  esp free0 mov,
  0 free>tos p;

\ Set the return stack pointer. Attention: A wrong value does not lead to a
\ segmentation fault immediate, but at the next call or return.
p: RP! 				( rp -- )
( OK )
  regalloc-reset
  req-any
  tos0 esp mov,
  1 reg-free p;

p: DU< 				( d1l d1h d2l d2h -- flag )
( OK )
  regalloc-reset
  req-any 			\ tos0=d2h
  req-any 			\ tos1=d2l
  req-any 			\ tos2=d1h
  req-any 			\ tos3=d1l
  tos1 tos3 sub,
  tos0 tos2 sbb,
  tos3 tos3 sbb,
  3 reg-free p;

p: PICK 			( n -- tos+n )
( OK )
  regalloc-reset
  regalloc-flush
  req-any
  tos0 inc,
  2 ## tos0 shl,
  ebp tos0 add,
  offs-ebp [tos0] tos0 mov, p;

p: COUNT 				( c-addr1 -- c-addr2 u )
( OK )
  regalloc-reset
  req-any
  a-d-free
  free0 free0 xor,
  0 [tos0] free0l mov,
  tos0 inc,
  0 free>tos p;

p: CELLS 				( x -- x*4 )
( OK )
  regalloc-reset
  req-any
  2 ## tos0 shl, p;

p: CELL+ 				( x -- x+4)
( OK )
  regalloc-reset
  req-any
  4 ## tos0 add, p;

p: / 					( n1 n2 -- n3 )
( OK )
  regalloc-reset
  req-edx 				\ n2=tos0=edx
  req-eax 				\ n1=tos1=eax
  req-free
  eax free0 mov,
  31 ## free0 sar,
  edx free0 xchg,
  free0 idiv, 				\ eax=quot edx=rem
  1 reg-free p;
  
p: MOD 					( n1 n2 -- n3 )
( OK )
  regalloc-reset
  req-edx 				\ n2=tos0=edx
  req-eax 				\ n1=tos1=eax
  req-free
  eax free0 mov,
  31 ## free0 sar,
  edx free0 xchg,
  free0 idiv, 				\ eax=quot edx=rem
  0 1 tos-swap
  1 reg-free p;
  
p: M+ 					( dl dh n -- dl dh )
( OK )
   regalloc-reset
   req-any 				\ tos0=n
   req-any 				\ tos1=dh
   req-any 				\ tos2=dl
   tos0 tos2 add,
   0 ## tos1 adc,
   1 reg-free p;

p: ALIGNED 				( addr -- addr2 )
( OK )
   regalloc-reset
   req-any
   3 ## tos0 add,
   3 INVERT ## tos0 and, p;

p: MAX 					( n1 n2 -- n3 )
( OK )
   regalloc-reset
   req-any req-any
   tos0 tos1 cmp,
   0 jg,
   tos0 tos1 mov,
0 $:
   1 reg-free p;

p: MIN 					( n1 n2 -- n3 )
( OK )
   regalloc-reset
   req-any req-any
   tos0 tos1 cmp,
   0 jl,
   tos0 tos1 mov,
0 $:
   1 reg-free p;

p: ABS 					( n1 -- n2 )
( OK )
   regalloc-reset
   req-any
   tos0 tos0 or,
   0 jns,
   tos0 neg,
0 $: p;

: (_sizing) 				( prim-xt -- )
  0 opt-getlit 				\ pxt x
  SWAP EXECUTE \ x'
  0 opt-setlit
  1 1 opt-remove ;

opt( ''# '' CELLS )opt: ['] CELLS (_sizing) ;opt
opt( ''# '' 1+    )opt: ['] 1+    (_sizing) ;opt
opt( ''# '' 1-    )opt: ['] 1-    (_sizing) ;opt
opt( ''# '' CHAR+ )opt: ['] 1+    (_sizing) ;opt
opt( ''# '' 2*    )opt: ['] 2*    (_sizing) ;opt
opt( ''# '' 2/    )opt: ['] 2/    (_sizing) ;opt

\ General arithmetic optimizer. Precalculates or produces faster code.
: (#arith) 				( xt -- )
  0 opt-getlit 				\ xt x
  0 2 opt-remove
  regalloc-reset
  req-any
  ## tos0 EXECUTE ;
  
: (##arith) 				( xt -- )
  0 opt-getlit
  1 opt-getlit 			\ xt x1 x0
  rot EXECUTE  			\ x
  0 opt-setlit
  1 2 opt-remove 
  ;
   
opt( ''# '' +   )opt: ['] add, (#arith) ;opt
opt( ''# '' -   )opt: ['] sub, (#arith) ;opt
opt( ''# '' OR  )opt: ['] or,  (#arith) ;opt
opt( ''# '' AND )opt: ['] and, (#arith) ;opt
opt( ''# '' XOR )opt: ['] xor, (#arith) ;opt

opt( ''# ''# '' +   )opt: ['] +   (##arith) ;opt
opt( ''# ''# '' -   )opt: ['] -   (##arith) ;opt
opt( ''# ''# '' OR  )opt: ['] OR  (##arith) ;opt
opt( ''# ''# '' AND )opt: ['] AND (##arith) ;opt
opt( ''# ''# '' XOR )opt: ['] XOR (##arith) ;opt

\ @ optimizer.
opt( ''# '' @ )opt:
   0 opt-getlit  			\ addr
   0 2 opt-remove 
   regalloc-reset
   req-free
   #[] free0 mov,
   0 free>tos ;opt
 
\ ! optimizer.
opt( ''# '' ! )opt:
   0 opt-getlit  			\ addr
   0 2 opt-remove 
   regalloc-reset
   req-any
   tos0 #[] mov,
   1 reg-free ;opt

\ +! optimizer.
opt( ''# '' +! )opt:
   0 opt-getlit  			\ addr
   0 2 opt-remove 
   regalloc-reset
   req-any
   tos0 #[] add,
   1 reg-free ;opt

\ Optimizer of + @ sequence.
opt( ''# '' + '' @ )opt: 
    0 opt-getlit 			\ x
   regalloc-reset
    req-any 				\ tos0=offs
    [tos0] tos0 mov, 
    0 3 opt-remove
    ;opt
    
opt( '' + '' @ )opt:     
    regalloc-reset
    req-any 				\ tos0=offs
    req-any 				\ tos1=addr
    0 [tos0+tos1] tos1 mov,
    1 reg-free 
    0 2 opt-remove
    ;opt

opt( '' OVER '' @ )opt: 
  regalloc-reset
  req-any
  req-any
  req-free
  0 [tos1] free0 mov,
  0 free>tos
  0 2 opt-remove ;opt

opt( '' OVER '' ! )opt:
  regalloc-reset
  req-any 				\ tos0=x
  req-any 				\ tos1=addr
  tos0 0 [tos1] mov,
  1 reg-free
  0 2 opt-remove
;opt

opt( '' DUP '' 1- )opt:
  regalloc-reset
  req-any
  req-free
  -1 [tos0] free0 lea,
  0 free>tos
  0 2 opt-remove
;opt

opt( '' DUP '' CELL+ '' @ )opt:
  regalloc-reset
  req-any
  req-free
  4 [tos0] free0 mov,
  0 free>tos
  0 3 opt-remove
;opt

\ opt( '' 0= '' ?branch )opt:
\   regalloc-reset
\   req-any
\   tos0 tos0 test,
\   1 reg-free
\   ['] n-jnz, (ahead)
\   0 2 opt-remove
\ ;opt
\ 
\ : (#-rel-IF) 				( jmp-xt n-free -- orig )
\   0 opt-getlit 				\ xt nf x rel?
\   regalloc-reset
\   req-any
\   ## tos0 cmp,
\   reg-free
\   (ahead)
\   0 3 opt-remove 
\ ;
\ 
\ : (rel-IF) 				( jmp-xt nfree -- orig )
\   regalloc-reset
\   req-any req-any
\   tos0 tos1 cmp,
\   1+ reg-free
\   (ahead)
\   0 2 opt-remove
\ ;
\ 
\ opt( '' =  '' ?branch )opt: ['] n-jnz,  1 (rel-IF) ;opt
\ opt( '' <> '' ?branch )opt: ['] n-jz,   1 (rel-IF) ;opt
\ opt( '' <  '' ?branch )opt: ['] n-jnl,  1 (rel-IF) ;opt
\ opt( '' >  '' ?branch )opt: ['] n-jng,  1 (rel-IF) ;opt
\ opt( '' <= '' ?branch )opt: ['] n-jnle, 1 (rel-IF) ;opt
\ opt( '' >= '' ?branch )opt: ['] n-jnge, 1 (rel-IF) ;opt
\ 
\ opt( ''# '' =  '' ?branch )opt: ['] n-jnz,  1 (#-rel-IF) ;opt
\ opt( ''# '' <> '' ?branch )opt: ['] n-jz,   1 (#-rel-IF) ;opt
\ opt( ''# '' <  '' ?branch )opt: ['] n-jnl,  1 (#-rel-IF) ;opt
\ opt( ''# '' >  '' ?branch )opt: ['] n-jng,  1 (#-rel-IF) ;opt
\ opt( ''# '' <= '' ?branch )opt: ['] n-jnle, 1 (#-rel-IF) ;opt
\ opt( ''# '' >= '' ?branch )opt: ['] n-jnge, 1 (#-rel-IF) ;opt
\ 
\ opt( '' OVER '' =  '' ?branch )opt: ['] n-jnz,  0 (rel-IF) 0 1 opt-remove ;opt
\ opt( '' OVER '' <> '' ?branch )opt: ['] n-jz,   0 (rel-IF) 0 1 opt-remove ;opt
\ opt( '' OVER '' <  '' ?branch )opt: ['] n-jng,  0 (rel-IF) 0 1 opt-remove ;opt
\ opt( '' OVER '' >  '' ?branch )opt: ['] n-jnl,  0 (rel-IF) 0 1 opt-remove ;opt
\ opt( '' OVER '' <= '' ?branch )opt: ['] n-jnge, 0 (rel-IF) 0 1 opt-remove ;opt
\ opt( '' OVER '' >= '' ?branch )opt: ['] n-jnle, 0 (rel-IF) 0 1 opt-remove ;opt
\ 
\ opt( ''# '' OVER '' =  '' ?branch )opt: ['] n-jnz,  0 (#-rel-IF) 0 1 opt-remove ;opt
\ opt( ''# '' OVER '' <> '' ?branch )opt: ['] n-jz,   0 (#-rel-IF) 0 1 opt-remove ;opt
\ opt( ''# '' OVER '' <  '' ?branch )opt: ['] n-jng,  0 (#-rel-IF) 0 1 opt-remove ;opt
\ opt( ''# '' OVER '' >  '' ?branch )opt: ['] n-jnl,  0 (#-rel-IF) 0 1 opt-remove ;opt
\ opt( ''# '' OVER '' <= '' ?branch )opt: ['] n-jnge, 0 (#-rel-IF) 0 1 opt-remove ;opt
\ opt( ''# '' OVER '' >= '' ?branch )opt: ['] n-jnle, 0 (#-rel-IF) 0 1 opt-remove ;opt
\ 
\ 
\ : (2DUP-rel-IF) 			( jmp-xt -- )
\   regalloc-reset
\   req-any
\   req-any
\   tos0 tos1 cmp, 			\ jxt
\   (ahead)
\   0 3 opt-remove
\ ;
\ 
\ opt( '' 2DUP '' =  '' ?branch )opt: ['] n-jnz,  (2DUP-rel-IF) ;opt
\ opt( '' 2DUP '' <> '' ?branch )opt: ['] n-jz,   (2DUP-rel-IF) ;opt
\ opt( '' 2DUP '' <  '' ?branch )opt: ['] n-jnl,  (2DUP-rel-IF) ;opt
\ opt( '' 2DUP '' >  '' ?branch )opt: ['] n-jng,  (2DUP-rel-IF) ;opt
\ opt( '' 2DUP '' <= '' ?branch )opt: ['] n-jnle, (2DUP-rel-IF) ;opt
\ opt( '' 2DUP '' >= '' ?branch )opt: ['] n-jnge, (2DUP-rel-IF) ;opt
\ 
\ : (rel-WHILE) 				( dest jmp-xt nr-free -- orig dest)
\   (rel-IF) 				\ dest orig
\   1 (CS-ROLL) ;
\ 
\ opt( '' =  '' WHILE )opt: ['] n-jnz,  1 (rel-WHILE) ;opt
\ opt( '' <> '' WHILE )opt: ['] n-jz,   1 (rel-WHILE) ;opt
\ opt( '' <  '' WHILE )opt: ['] n-jnl,  1 (rel-WHILE) ;opt
\ opt( '' >  '' WHILE )opt: ['] n-jng,  1 (rel-WHILE) ;opt
\ opt( '' <= '' WHILE )opt: ['] n-jnle, 1 (rel-WHILE) ;opt
\ opt( '' >= '' WHILE )opt: ['] n-jnge, 1 (rel-WHILE) ;opt
\ 
\ opt( '' OVER '' =  '' WHILE )opt: ['] n-jnz,  0 (rel-WHILE) 0 1 opt-remove ;opt
\ opt( '' OVER '' <> '' WHILE )opt: ['] n-jz,   0 (rel-WHILE) 0 1 opt-remove ;opt
\ opt( '' OVER '' <  '' WHILE )opt: ['] n-jng,  0 (rel-WHILE) 0 1 opt-remove ;opt
\ opt( '' OVER '' >  '' WHILE )opt: ['] n-jnl,  0 (rel-WHILE) 0 1 opt-remove ;opt
\ opt( '' OVER '' <= '' WHILE )opt: ['] n-jnge, 0 (rel-WHILE) 0 1 opt-remove ;opt
\ opt( '' OVER '' >= '' WHILE )opt: ['] n-jnle, 0 (rel-WHILE) 0 1 opt-remove ;opt
\ 
\ : (2DUP-rel-WHILE) 			( dest jmp-xt nr-free -- orig dest )
\   (2DUP-rel-IF) 1 (CS-ROLL) ;
\ 
\ opt( '' 2DUP '' =  '' WHILE )opt: ['] n-jnz,  (2DUP-rel-WHILE) ;opt
\ opt( '' 2DUP '' <> '' WHILE )opt: ['] n-jz,   (2DUP-rel-WHILE) ;opt
\ opt( '' 2DUP '' <  '' WHILE )opt: ['] n-jnl,  (2DUP-rel-WHILE) ;opt
\ opt( '' 2DUP '' >  '' WHILE )opt: ['] n-jng,  (2DUP-rel-WHILE) ;opt
\ opt( '' 2DUP '' <= '' WHILE )opt: ['] n-jnle, (2DUP-rel-WHILE) ;opt
\ opt( '' 2DUP '' >= '' WHILE )opt: ['] n-jnge, (2DUP-rel-WHILE) ;opt
\ 
\ 
\ : (#_log_IF) 				( log-xt -- )
\   0 opt-getlit 				\ xt x rel?
\   regalloc-reset
\   req-any
\   ?+relocate
\   ## tos0 EXECUTE
\   1 reg-free
\   ['] n-jz, (ahead)
\   0 3 opt-remove 
\   ;
\ 
\ : (_log_IF) 				( log-xt -- )
\   regalloc-reset 			\ xt
\   req-any
\   req-any
\   tos0 tos1 EXECUTE
\   2 reg-free
\   ['] n-jz, (ahead)
\   0 2 opt-remove
\ ; 
\  
\ opt( '' OR  '' ?branch )opt: ['] or,   (_log_IF) ;opt
\ opt( '' AND '' ?branch )opt: ['] test, (_log_IF) ;opt
\ opt( ''# '' OR  '' ?branch )opt: ['] or,   (#_log_IF) ;opt
\ opt( ''# '' AND '' ?branch )opt: ['] test, (#_log_IF) ;opt
