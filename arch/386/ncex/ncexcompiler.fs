\ NCEX compiler patch

\ ------------------------------------ tools -----------------------------------

\ Print a number as an gdb address.
: .addr ( x -- ) 
  base @ >R
  hex
  ." 0x" 0 <# # # # # # # # # #> type space 
  r> base ! ;

\ Same as ' but return the compiling xt if restricted.
: my-' ( "name" -- xt )
  name find-name dup 0= -13 ?throw \ nt
  dup name>int \ nt name-xt
  ['] compile-only-error over = if \ nt name-xt
    drop name>comp drop \ name-xt
  else \ nt name-xt
    nip
  then \ name-xt
  ;

\ Before we an do something, we need to revector a few words, that would mess
\ up the cross compiler. This method was advised by Anton Ertl. I don't think
\ it's elegant, but it works.
\ Make name a defered word and let it execute xt.
: redefer ( xt "name" -- )
  my-' tuck \ name-xt xt name-xt
  >body ! \ name-xt
  dodefer: swap code-address! 
  ;

\ Same as normal is, but works on restricted words too.
: my-is ( "name" -- )
  my-' >body postpone aliteral postpone !
  ; IMMEDIATE restrict

\ ------------------------------ itc -> nc wrapper -----------------------------
\ Compile code to pretend xt is a regular code definition. The code has to
\ load sp and rp from the variables which are either registers or in a stack
\ frame. Since this is system and CPU dependent, the code for the generation
\ of that is in ncexcpu1.fs . 
: generate-wrapper ( xt -- )
  wrapper \ xt code-addr
  swap code-address! \ xt
  ;

\ --------------------------- threaded code compiler ---------------------------
: (tc-literal) postpone lit , ;
: (tc-aliteral) postpone lit a, ;

: (tc-:) ( "name" -- colon-sys )
  Header docol: cfa, defstart ] :-hook ;

: (tc-;) ( compilation colon-sys -- ; run-time nest-sys )
  ;-hook ?struc postpone exit reveal postpone [ ;

: (tc-:noname) ( -- xt colon-sys )
  0 last ! cfalign here docol: cfa, 0 ] :-hook ;

: (tc-compile,) ( xt -- )
  , ;

: (tc-SLiteral) ( Compilation c-addr1 u ; run-time -- c-addr2 u ) \ string
    postpone (S") here over char+ allot  place align ;

' (tc-literal) redefer literal
' (tc-aliteral) redefer aliteral
' (tc-;) redefer ;
' (tc-:) redefer :
' (tc-:noname) redefer :noname
' (tc-compile,) redefer compile,
' (tc-SLiteral) redefer sliteral

' tc-ahead redefer ahead
' tc-if redefer if
' tc-else redefer else
' tc-then redefer then
  
\ ---------------------------- native code compiler ----------------------------
: (nc-literal) ( x -- )
  (opt-add-const) ;

: (nc-compile,) ( xt -- )
  (opt-add-xt) 
  ;

: ((nc-:)) ( -- xt colon-sys )
  here \ xt
  dup >body cfa,
  defstart ] 
  regalloc-init
  (nc-:),
  ;

: (nc-:) ( "name" -- colon-sys ) 
  header ((nc-:)) nip ;

: (nc-:noname) ( -- xt colon-sys )
  0 last ! cfalign ((nc-:)) ;

: (nc-;) ( compilation colon-sys -- ; run-time nest-sys )
  (opt-flush) 
  ?struc 
  postpone exit (opt-flush)
  lastxt 
  generate-wrapper
  reveal postpone [ 
  ; 

: (nc-sliteral) ( ca1 u -- )
  (opt-flush)
  0 (opt-add-const) 
  (opt-flush) asm-here >R \ ca u / r: fix-str
  TUCK (opt-add-const) \ u ca / r: fix-str
  (opt-flush) \ u ca / r: fix-str
  0 ## jmp, 
  asm-here R> 1 cells - ! \ u ca
  asm-here dup >R \ u ca here / r: fix-jmp
  rot dup allot move \ / r: fix-jmp
  r> dup 1 cells - swap \ store orig
  asm-here swap - swap !
  ; 

\ ------------------------------- compiler choice ------------------------------
: threaded ( -- )
  ['] (tc-literal) my-is literal
  ['] (tc-literal) my-is aliteral
  ['] (tc-compile,) is compile,
  ['] (tc-:) is :
  ['] (tc-:noname) is :noname
  ['] (tc-;) my-is ;
  ['] (tc-sliteral) my-is sliteral
  ['] tc-if my-is if
  ['] tc-ahead my-is ahead
  ['] tc-then my-is then
  ['] tc-else my-is else
  ;

: native ( -- )
  ['] (nc-literal) my-is literal
  ['] (nc-literal) my-is aliteral
  ['] (nc-compile,) is compile,
  ['] (nc-:) is :
  ['] (nc-:noname) is :noname
  ['] (nc-;) my-is ;
  ['] (nc-sliteral) my-is sliteral
  ['] nc-if my-is if
  ['] nc-ahead my-is ahead
  ['] nc-then my-is then
  ['] nc-else my-is else
  ;

: ." postpone S" postpone type ; immediate restrict

: init-ncex ( -- )
  threaded (asm-reset)
  threading-method 1 <> abort" NCEX supportss only ITC."
  ; 
