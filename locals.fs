\ Local primitives                                      17jan92py

Variable loffset   0 loffset !
Variable locals  here locals !  100 ( some) cells allot
: local, ( offset -- )  postpone rp@ loffset @ swap -
  postpone Literal postpone + ;
: delocal, ( offset -- ) local, postpone rp! ;
: (local  DOES>  @ local, postpone @ ;

: <local ( -- sys1 )  current @ @ loffset @ locals @ ;  immediate
: local: ( -- )  postpone >r  last @ lastcfa @ here locals @ dp !
  cell loffset +! Create  loffset @ , immediate (local
  here locals !  dp !  lastcfa ! last ! ;               immediate
: local> ( sys1 -- sys2 ) ;                             immediate
: local; ( sys2 -- ) locals ! dup delocal,
  loffset ! current @ ! ;                               immediate
: TO  >in @ ' dup @ [ ' (local >body cell+ ] ALiteral =
  IF    >body @ local, postpone ! drop
  ELSE  drop >in ! postpone to  THEN  ;                 immediate
: EXIT  loffset @ IF  0 delocal,  THEN  postpone EXIT ; immediate

: DO      2 cells loffset +!  postpone DO     ; immediate restrict
: ?DO     2 cells loffset +!  postpone ?DO    ; immediate restrict
: FOR     2 cells loffset +!  postpone FOR    ; immediate restrict
: LOOP   -2 cells loffset +!  postpone LOOP   ; immediate restrict
: +LOOP  -2 cells loffset +!  postpone +LOOP  ; immediate restrict
: NEXT   -2 cells loffset +!  postpone NEXT   ; immediate restrict
: >R      1 cells loffset +!  postpone >R     ; immediate restrict
: R>     -1 cells loffset +!  postpone R>     ; immediate restrict

\ High level locals                                    19aug93py

: { postpone <local  -1
  BEGIN  >in @ name dup c@ 1 = swap 1+ c@ '| = and  UNTIL
  drop >in @ >r
  BEGIN  dup 0< 0= WHILE  >in ! postpone local:  REPEAT  drop
  r> >in ! postpone local> ;                  immediate restrict

' local; alias } immediate restrict

\ ANS Locals                                           19aug93py

Create inlocal  5 cells allot  inlocal off
: (local)  ( addr u -- )  inlocal @ 0=
  IF  postpone <local inlocal on
      inlocal 3 cells + 2!  inlocal cell+ 2! THEN
  dup IF    linestart @ >r loadline @ >r loadfile @ >r
            blk @ >r >tib @ >r  #tib @ dup >r  >in @ >r

            >tib +! dup #tib ! >tib @ swap move
            >in off blk off loadfile off -1 linestart !

            postpone local:

            r> >in !  r> #tib !  r> >tib ! r> blk !
            r> loadfile ! r> loadline ! r> linestart !
      ELSE  2drop  inlocal cell+ 2@  inlocal 3 cells + 2@
            postpone local>
            inlocal 2 cells + 2! inlocal cell+ ! THEN ;

: ?local;  inlocal @
  IF  inlocal cell+ @ inlocal 2 cells + 2@
      postpone local; inlocal off  THEN ;

: ;      ?local; postpone ; ;                 immediate restrict
: DOES>  ?local; postpone DOES> ;             immediate

: locals|
  BEGIN  name dup c@ 1 = over 1+ c@ '| = and 0=  WHILE
         count (local)  REPEAT  0 (local) ;   immediate restrict
