\ This is not very nice (hard limits, no checking, assumes 1 chars = 1)

\ Optimizations:
\ superfluous stores are removed. GCC removes the superfluous loads by itself
\ TOS and FTOS can be kept in register( variable)s.
\ 
\ Problems:
\ The TOS optimization is somewhat hairy. The problems by example:
\ 1) dup ( w -- w w ): w=TOS; sp-=1; sp[1]=w; TOS=w;
\    The store is not superfluous although the earlier opt. would think so
\    Alternatively:    sp[0]=TOS; w=TOS; sp-=1; TOS=w;
\ 2) ( -- .. ): sp[0] = TOS; ... /* This additional store is necessary */
\ 3) ( .. -- ): ... TOS = sp[0]; /* as well as this load */
\ 4) ( -- ): /* but here they are unnecessary */
\ 5) Words that call NEXT themselves have to be done very carefully.
\
\ To do:
\ add the store optimization for doubles
\ regarding problem 1 above: It would be better (for over) to implement
\ 	the alternative

warnings off

[IFUNDEF] vocabulary    include search-order.fs [THEN]
[IFUNDEF] environment?  include environ.fs      [THEN]
include gray.fs

100 constant max-effect \ number of things on one side of a stack effect
4096 constant batch-size \ no meaning, just make sure it's >0
255 constant maxchar
maxchar 1+ constant eof-char
9 constant tab-char
10 constant nl-char

: read-whole-file ( c-addr1 file-id -- c-addr2 )
\ reads the contents of the file file-id puts it into memory at c-addr1
\ c-addr2 is the first address after the file block
  begin ( c-addr file-id )
    2dup batch-size swap read-file 
    if
      true abort" I/O error"
    endif
    ( c-addr file-id actual-size ) rot over + -rot
    batch-size <>
  until
  drop ;

variable input \ pointer to next character to be parsed
variable endinput \ pointer to the end of the input (the char after the last)

: start ( -- addr )
 input @ ;

: end ( addr -- addr u )
 input @ over - ;

variable output \ xt ( -- ) of output word

: printprim ( -- )
 output @ execute ;

: field
 <builds-field ( n1 n2 -- n3 )
 does>         ( addr1 -- addr2 )
   @ + ;

: const-field
 <builds-field ( n1 n2 -- n3 )
 does>         ( addr -- w )
   @ + @ ;

struct
 2 cells field item-name
 cell field item-d-offset
 cell field item-f-offset
 cell field item-type
constant item-descr

2variable forth-name
2variable wordset
2variable c-name
2variable doc
2variable c-code
2variable forth-code
2variable stack-string
create effect-in  max-effect item-descr * allot
create effect-out max-effect item-descr * allot
variable effect-in-end ( pointer )
variable effect-out-end ( pointer )
2variable effect-in-size
2variable effect-out-size

variable primitive-number -9 primitive-number !

\ for several reasons stack items of a word are stored in a wordlist
\ since neither forget nor marker are implemented yet, we make a new
\ wordlist for every word and store it in the variable items
variable items

\ a few more set ops

: bit-equivalent ( w1 w2 -- w3 )
 xor invert ;

: complement ( set1 -- set2 )
 empty ['] bit-equivalent binary-set-operation ;

\ the parser

eof-char max-member \ the whole character set + EOF

: getinput ( -- n )
 input @
 dup endinput @ =
 if
   drop eof-char
 else
   c@
 endif ;

:noname ( n -- )
 dup bl > if
  emit space
 else
  .
 endif ;
print-token !

: testchar? ( set -- f )
 getinput member? ;
' testchar? test-vector !

: ?nextchar ( f -- )
 ?not? if
   ." syntax error" cr
   getinput . cr
   input @ endinput @ over - 100 min type cr
   abort
 endif
 input @ endinput @ <> if
   1 input +!
 endif ;

: charclass ( set "name" -- )
 ['] ?nextchar terminal ;

: .. ( c1 c2 -- set )
 ( creates a set that includes the characters c, c1<=c<=c2 )
 empty copy-set
 swap 1+ rot do
  i over add-member
 loop ;

: ` ( -- terminal ) ( use: ` c )
 ( creates anonymous terminal for the character c )
 [compile] ascii singleton ['] ?nextchar make-terminal ;

char a char z ..  char A char Z ..  union char _ singleton union  charclass letter
char 0 char 9 ..					charclass digit
bl singleton						charclass blank
tab-char singleton					charclass tab
nl-char singleton eof-char over add-member complement	charclass nonl
nl-char singleton eof-char over add-member char : over add-member complement  charclass nocolonnl
bl 1+ maxchar ..					charclass nowhite
char " singleton eof-char over add-member complement	charclass noquote
nl-char singleton					charclass nl
eof-char singleton					charclass eof


(( letter (( letter || digit )) **
)) <- c-name ( -- )

nowhite ++
<- name ( -- )

(( ` \ nonl ** nl
)) <- comment ( -- )

(( {{ effect-in }} (( {{ start }} c-name {{ end 2 pick item-name 2! item-descr + }} blank ** )) ** {{ effect-in-end ! }}
   ` - ` - blank **
   {{ effect-out }} (( {{ start }} c-name {{ end 2 pick item-name 2! item-descr + }} blank ** )) ** {{ effect-out-end ! }}
)) <- stack-effect ( -- )

(( {{ s" " doc 2! s" " forth-code 2! }}
   (( comment || nl )) **
   (( {{ start }} name {{ end 2dup forth-name 2! c-name 2! }}  tab ++
      {{ start }} stack-effect {{ end stack-string 2! }} tab ++
        {{ start }} name {{ end wordset 2! }} tab **
        (( {{ start }}  c-name {{ end c-name 2! }} )) ??  nl
   ))
   (( ` " ` "  {{ start }} (( noquote ++ ` " )) ++ {{ end 1- doc 2! }} ` " nl )) ??
   {{ start }} (( nocolonnl nonl **  nl )) ** {{ end c-code 2! }}
   (( ` :  nl
      {{ start }} (( nonl ++  nl )) ++ {{ end forth-code 2! }}
   )) ??
   (( nl || eof ))
)) <- primitive ( -- )

(( (( primitive {{ printprim }} )) **  eof ))
parser primitives2something
warnings @ [IF]
.( parser generated ok ) cr
[THEN]

: primfilter ( file-id xt -- )
\ fileid is for the input file, xt ( -- ) is for the output word
 output !
 here input !
 here swap read-whole-file
 dup endinput !
 here - allot
 align
 primitives2something ;

\ types

struct
 2 cells field type-c-name
 cell const-field type-d-size
 cell const-field type-f-size
 cell const-field type-fetch-handler
 cell const-field type-store-handler
constant type-description

: data-stack-access ( n1 n2 n3 -- )
\ n1 is the offset of the accessed item, n2, n3 are effect-*-size
 drop swap - 1- dup
 if
   ." sp[" 0 .r ." ]"
 else
   drop ." TOS"
 endif ;

: fp-stack-access ( n1 n2 n3 -- )
\ n1 is the offset of the accessed item, n2, n3 are effect-*-size
 nip swap - 1- dup
 if
   ." fp[" 0 .r ." ]"
 else
   drop ." FTOS"
 endif ;

: fetch-single ( item -- )
 >r
 r@ item-name 2@ type
 ."  = (" 
 r@ item-type @ type-c-name 2@ type ." ) "
 r@ item-d-offset @ effect-in-size 2@ data-stack-access ." ;" cr
 rdrop ; 

: fetch-double ( item -- )
 >r
 r@ item-name 2@ type 
 ." = ({Double_Store _d; _d.cells.low = "
 r@ item-d-offset @ dup    effect-in-size 2@ data-stack-access
 ." ; _d.cells.high = " 1+ effect-in-size 2@ data-stack-access
 ." ; _d.dcell;});" cr
 rdrop ;

: fetch-float ( item -- )
 >r
 r@ item-name 2@ type
 ."  = "
 \ ." (" r@ item-type @ type-c-name 2@ type ." ) "
 r@ item-f-offset @ effect-in-size 2@ fp-stack-access ." ;" cr
 rdrop ;

: d-same-as-in? ( item -- f )
\ f is true iff the offset of item is the same as on input
 >r
 r@ item-name 2@ items @ search-wordlist 0=
 abort" bug"
 execute @
 dup r@ =
 if \ item first appeared in output
   drop false
 else
   item-d-offset @ r@ item-d-offset @ =
 endif
 rdrop ;

: is-in-tos? ( item -- f )
\ true if item has the same offset as the input TOS
 item-d-offset @ 1+ effect-in-size 2@ drop = ;

: really-store-single ( item -- )
 >r
 r@ item-d-offset @ effect-out-size 2@ data-stack-access ."  = (Cell)"
 r@ item-name 2@ type ." ;"
 rdrop ;

: store-single ( item -- )
 >r
 r@ d-same-as-in?
 if
   r@ is-in-tos?
   if
     ." IF_TOS(" r@ really-store-single ." );" cr
   endif
 else
   r@ really-store-single cr
 endif
 rdrop ;

: store-double ( item -- )
\ !! store optimization is not performed, because it is not yet needed
 >r
 ." {Double_Store _d; _d.dcell = " r@ item-name 2@ type ." ; "
 r@ item-d-offset @ dup    effect-out-size 2@ data-stack-access 
 ."  = _d.cells.low; " 1+ effect-out-size 2@ data-stack-access
 ." = _d.cells.high;}" cr
 rdrop ;

: f-same-as-in? ( item -- f )
\ f is true iff the offset of item is the same as on input
 >r
 r@ item-name 2@ items @ search-wordlist 0=
 abort" bug"
 execute @
 dup r@ =
 if \ item first appeared in output
   drop false
 else
   item-f-offset @ r@ item-f-offset @ =
 endif
 rdrop ;

: is-in-ftos? ( item -- f )
\ true if item has the same offset as the input TOS
 item-f-offset @ 1+ effect-in-size 2@ nip = ;

: really-store-float ( item -- )
 >r
 r@ item-f-offset @ effect-out-size 2@ fp-stack-access ."  = "
 r@ item-name 2@ type ." ;"
 rdrop ;

: store-float ( item -- )
 >r
 r@ f-same-as-in?
 if
   r@ is-in-ftos?
   if
     ." IF_FTOS(" r@ really-store-float ." );" cr
   endif
 else
   r@ really-store-float cr
 endif
 rdrop ;
 
: single-type ( -- xt1 xt2 n1 n2 )
 ['] fetch-single ['] store-single 1 0 ;

: double-type ( -- xt1 xt2 n1 n2 )
 ['] fetch-double ['] store-double 2 0 ;

: float-type ( -- xt1 xt2 n1 n2 )
 ['] fetch-float ['] store-float 0 1 ;

: s, ( addr u -- )
\ allocate a string
 here swap dup allot move ;

: starts-with ( addr u xt1 xt2 n1 n2 "prefix" -- )
\ describes a type
\ addr u specifies the C type name
\ n1 is the size of the type on the data stack
\ n2 is the size of the type on the FP stack
\ stack effect entries of the type start with prefix
 >r >r >r >r
 dup >r here >r s,
 create
 r> r> 2,
 r> r> r> , r> , swap , , ;

wordlist constant types
get-current
types set-current

s" Bool"	single-type starts-with f
s" Char"	single-type starts-with c
s" Cell"	single-type starts-with n
s" Cell"	single-type starts-with w
s" UCell"	single-type starts-with u
s" DCell"	double-type starts-with d
s" UDCell"	double-type starts-with ud
s" Float"	float-type  starts-with r
s" Cell *"	single-type starts-with a_
s" Char *"	single-type starts-with c_
s" Float *"	single-type starts-with f_
s" DFloat *"	single-type starts-with df_
s" SFloat *"	single-type starts-with sf_
s" Xt"		single-type starts-with xt
s" WID"		single-type starts-with wid
s" F83Name *"	single-type starts-with f83name

set-current

: get-type ( addr1 u1 -- type-descr )
\ get the type of the name in addr1 u1
\ type-descr is a pointer to a type-descriptor
 0 swap ?do
   dup i types search-wordlist
   if \ ok, we have the type ( addr1 xt )
     execute nip
     UNLOOP EXIT
   endif
 -1 s+loop
 \ we did not find a type, abort
 true abort" unknown type prefix" ;

: declare ( addr "name" -- )
\ remember that there is a stack item at addr called name
 create , ;

: declaration ( item -- )
 dup item-name 2@ items @ search-wordlist
 if \ already declared ( item xt )
   execute @ item-type @ swap item-type !
 else ( addr )
   dup item-name 2@ nextname dup declare ( addr )
   dup >r item-name 2@ 2dup get-type ( addr1 u type-descr )
   dup r> item-type ! ( addr1 u type-descr )
   type-c-name 2@ type space type ." ;" cr
 endif ;

: declaration-list ( addr1 addr2 -- )
 swap ?do
  i declaration
 item-descr +loop ;

: fetch ( addr -- )
 dup item-type @ type-fetch-handler execute ;

: declarations ( -- )
 wordlist dup items ! set-current
 effect-in effect-in-end @ declaration-list
 effect-out effect-out-end @ declaration-list ;

\ offset computation
\ the leftmost (i.e. deepest) item has offset 0
\ the rightmost item has the highest offset

: compute-offset ( n1 n2 item -- n3 n4 )
\ n1, n3 are data-stack-offsets
\ n2, n4 are the fp-stack-offsets
 >r
 swap dup r@ item-d-offset !
 r@ item-type @ type-d-size +
 swap dup r@ item-f-offset !
 r@ item-type @ type-f-size +
 rdrop ;

: compute-list ( addr1 addr2 -- n1 n2 )
\ n1, n2 are the final offsets
 0 0 2swap swap ?do
  i compute-offset
 item-descr +loop ;

: compute-offsets ( -- )
 effect-in effect-in-end @ compute-list effect-in-size 2!
 effect-out effect-out-end @ compute-list effect-out-size 2! ;

: flush-tos ( -- )
 effect-in-size 2@ effect-out-size 2@
 0<> rot 0= and
 if
   ." IF_FTOS(fp[0] = FTOS);" cr
 endif
 0<> swap 0= and
 if
   ." IF_TOS(sp[0] = TOS);" cr
 endif ;

: fill-tos ( -- )
 effect-in-size 2@ effect-out-size 2@
 0= rot 0<> and
 if
   ." IF_FTOS(FTOS = fp[0]);" cr
 endif
 0= swap 0<> and
 if
   ." IF_TOS(TOS = sp[0]);" cr
 endif ;

: fetches ( -- )
 effect-in-end @ effect-in ?do
   i fetch
 item-descr +loop ; 

: stack-pointer-updates ( -- )
\ we need not check if an update is a noop; gcc does this for us
 effect-in-size 2@
 effect-out-size 2@
 rot swap - ( d-in d-out f-diff )
 rot rot - ( f-diff d-diff )
 ?dup IF  ." sp += " 0 .r ." ;" cr  THEN
 ?dup IF  ." fp += " 0 .r ." ;" cr  THEN ;

: store ( item -- )
\ f is true if the item should be stored
\ f is false if the store is probably not necessary
 dup item-type @ type-store-handler execute ;

: stores ( -- )
 effect-out-end @ effect-out ?do
   i store
 item-descr +loop ; 

: .stack-list ( start end -- )
 swap ?do
   i item-name 2@ type space
 item-descr +loop ; 

: output-c ( -- )
 ." I_" c-name 2@ type ." :	/* " forth-name 2@ type ."  ( " stack-string 2@ type ."  ) */" cr
 ." /* " doc 2@ type ."  */" cr
 ." {" cr
 ." DEF_CA" cr
 declarations
 compute-offsets \ for everything else
 flush-tos
 fetches
 stack-pointer-updates cr
 ." NAME(" [char] " emit forth-name 2@ type [char] " emit ." )" cr \ debugging
 ." {" cr
 c-code 2@ type
 ." }" cr
 ." NEXT_P1;" cr
 stores
 fill-tos
 ." NEXT_P2;" cr
 ." }" cr
 cr
;

: output-label ( -- )
 ." &&I_" c-name 2@ type ." ," cr ;

: output-alias ( -- )
 primitive-number @ . ." alias " forth-name 2@ type cr
 -1 primitive-number +! ;

: output-forth ( -- )
 forth-code @ 0=
 IF    output-alias
 ELSE  ." : " forth-name 2@ type ."   ( "
       effect-in effect-in-end @ .stack-list ." -- "
       effect-out effect-out-end @ .stack-list ." )" cr
       forth-code 2@ type cr
       -1 primitive-number +!
 THEN ;

[IFDEF] documentation
: register-doc ( -- )
    get-current documentation set-current
    forth-name 2@ nextname create
    forth-name 2@ 2,
    stack-string 2@ 2,
    wordset 2@ 2,
    c-name 2@ 2,
    doc 2@ 2,
    set-current ;
[THEN]

: process-file ( addr u xt -- )
 >r r/o open-file abort" cannot open file"
 warnings @ if
 ." ------------ CUT HERE -------------" cr  endif
 r> primfilter ;

