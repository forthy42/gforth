
\ oof.fs	Object Oriented FORTH
\ 		This file is (c) 1996,2000 by Bernd Paysan
\			e-mail: bernd.paysan@gmx.de
\
\		Please copy and share this program, modify it for your system
\		and improve it as you like. But don't remove this notice.
\
\		Thank you.
\

\  The program uses the following words
\  from CORE :
\  decimal : bl word 0= ; = cells Constant Variable ! Create , allot @
\  IF POSTPONE >r ELSE +! dup + THEN immediate r> * >body cell+
\  Literal drop align here aligned DOES> execute ['] 2@ recurse swap
\  1+ over LOOP and EXIT ?dup 0< ] [ rot r@ - i negate +LOOP 2drop
\  BEGIN WHILE 2dup REPEAT 1- rshift > / ' move UNTIL or count
\  from CORE-EXT :
\  nip false Value tuck true ?DO compile, erase pick :noname 0<> 
\  from BLOCK-EXT :
\  \ 
\  from EXCEPTION :
\  throw 
\  from EXCEPTION-EXT :
\  abort" 
\  from FILE :
\  ( S" 
\  from FLOAT :
\  faligned 
\  from LOCAL :
\  TO 
\  from MEMORY :
\  allocate free 
\  from SEARCH :
\  find definitions get-order set-order get-current wordlist
\  set-current search-wordlist
\  from SEARCH-EXT :
\  also Forth previous 
\  from STRING :
\  /string compare 
\  from TOOLS-EXT :
\  [IF] [THEN] [ELSE] state 
\  from non-ANS :
\  cell dummy [THEN] ?EXIT Vocabulary [ELSE] ( \G 

\ Loadscreen                                           27dec95py

decimal

: define?  ( -- flag )
  bl word find  nip 0= ;

define? cell  [IF]
1 cells Constant cell
[THEN]

define? \G [IF]
: \G postpone \ ; immediate
[THEN]

define? ?EXIT [IF]
: ?EXIT  postpone IF  postpone EXIT postpone THEN ; immediate
[THEN]

define? Vocabulary [IF]
: Vocabulary wordlist create ,
DOES> @ >r get-order nip r> swap set-order ;
[THEN]

define? faligned [IF] false [ELSE] 1 faligned 8 = [THEN]
[IF]
: 8aligned ( n1 -- n2 )  faligned ;
[ELSE]
: 8aligned ( n1 -- n2 )  7 + -8 and ;
[THEN]

Vocabulary Objects  also Objects also definitions

Vocabulary types  types also

0 cells Constant :wordlist
1 cells Constant :parent
2 cells Constant :child
3 cells Constant :next
4 cells Constant :method#
5 cells Constant :var#
6 cells Constant :newlink
7 cells Constant :iface
8 cells Constant :init

0 cells Constant :inext
1 cells Constant :ilist
2 cells Constant :ilen
3 cells Constant :inum

Variable op
: op! ( o -- )  op ! ;

Forth definitions

Create ostack 0 , 16 cells allot

: ^ ( -- o )  op @ ;
: o@ ( -- o )  op @ @ ;
: >o ( o -- )
    state @
    IF    postpone ^ postpone >r postpone op!
    ELSE  1 ostack +! ^ ostack dup @ cells + ! op!
    THEN  ; immediate
: o> ( -- )
    state @
    IF    postpone r> postpone op!
    ELSE  ostack dup @ cells + @ op! -1 ostack +!
    THEN  ; immediate
: o[] ( n -- ) o@ :var# + @ * ^ + op! ;

Objects definitions

\ Coding                                               27dec95py

0 Constant #static
1 Constant #method
2 Constant #early
3 Constant #var
4 Constant #defer

: exec?    ( addr -- flag )
  >body cell+ @ #method = ;
: static?  ( addr -- flag )
  >body cell+ @ #static = ;
: early?   ( addr -- flag )
  >body cell+ @ #early  = ;
: defer?   ( addr -- flag )
  >body cell+ @ #defer  = ;

false Value oset?

: o+,   ( addr offset -- )
  postpone Literal postpone ^ postpone +
  oset? IF  postpone op!  ELSE  postpone >o  THEN  drop ;
: o*,   ( addr offset -- )
  postpone Literal postpone * postpone Literal postpone +
  oset? IF  postpone op!  ELSE  postpone >o  THEN ;
: ^+@  ( offset -- addr )  ^ + @ ;
: o+@,  ( addr offset -- )
    postpone Literal postpone ^+@  oset? IF  postpone op!  ELSE  postpone >o  THEN drop ;
: ^*@  ( offset -- addr )  ^ + @ tuck @ :var# + @ 8aligned * + ;
: o+@*, ( addr offset -- )
  postpone Literal postpone ^*@  oset? IF  postpone op!  ELSE  postpone >o  THEN drop ;

\ variables / memory allocation                        30oct94py

Variable lastob
Variable lastparent   0 lastparent !
Variable vars
Variable methods
Variable decl  0 decl !
Variable 'link

: crash  true abort" unbound method" ;

: link, ( addr -- ) align here 'link !  , 0 , 0 , ;

0 link,

\ type declaration                                     30oct94py

: vallot ( size -- offset )  vars @ >r  dup vars +!
    'link @ 0=
    IF  lastparent @ dup IF  :newlink + @  THEN  link,
    THEN
    'link @ 2 cells + +! r> ;

: valign  ( -- )  vars @ aligned vars ! ;
define? faligned 0= [IF]
: vfalign ( -- )  vars @ faligned vars ! ;
[THEN]

: mallot ( -- offset )    methods @ cell methods +! ;

types definitions

: static   ( -- ) \ oof- oof
    \G Create a class-wide cell-sized variable.
    mallot Create , #static ,
DOES> @ o@ + ;
: method   ( -- ) \ oof- oof
    \G Create a method selector.
    mallot Create , #method ,
DOES> @ o@ + @ execute ;
: early    ( -- ) \ oof- oof
    \G Create a method selector for early binding.
    Create ['] crash , #early ,
DOES> @ execute ;
: var ( size -- ) \ oof- oof
    \G Create an instance variable
    vallot Create , #var ,
DOES> @ ^ + ;
: defer    ( -- ) \ oof- oof
    \G Create an instance defer
    valign cell vallot Create , #defer ,
DOES> @ ^ + @ execute ;

\ dealing with threads                                 29oct94py

Objects definitions

: object-order ( wid0 .. widm m addr -- wid0 .. widn n )
    dup  IF  2@ >r recurse r> swap 1+  ELSE  drop  THEN ;

: interface-order ( wid0 .. widm m addr -- wid0 .. widn n )
    dup  IF    2@ >r recurse r> :ilist + @ swap 1+
         ELSE  drop  THEN ;

: add-order ( addr -- n )  dup 0= ?EXIT  >r
    get-order r> swap >r 0 swap
    dup >r object-order r> :iface + @ interface-order
    r> over >r + set-order r> ;

: drop-order ( n -- )  0 ?DO  previous  LOOP ;

\ object compiling/executing                           20feb95py

: o, ( xt early? -- )
  over exec?   over and  IF 
      drop >body @ o@ + @ compile,  EXIT  THEN
  over static? over and  IF 
      drop >body @ o@ + @ postpone Literal  EXIT THEN
  drop dup early?  IF >body @  THEN  compile, ;

: findo    ( string -- cfa n )
    o@ add-order >r
    find
    ?dup 0= IF drop set-order true abort" method not found!" THEN
    r> drop-order ;

false Value method?

: method,  ( object early? -- )  true to method?
    swap >o >r bl word  findo  0< state @ and
    IF  r> o,  ELSE  r> drop execute  THEN  o> false to method?  ;

: cmethod,  ( object early? -- )
    state @ dup >r
    0= IF  postpone ]  THEN
    method,
    r> 0= IF  postpone [  THEN ;

: early, ( object -- )  true to oset?  true  method,
  state @ oset? and IF  postpone o>  THEN  false to oset? ;
: late,  ( object -- )  true to oset?  false method,
  state @ oset? and IF  postpone o>  THEN  false to oset? ;

\ new,                                                 29oct94py

previous Objects definitions

Variable alloc
0 Value ohere

: oallot ( n -- )  ohere + to ohere ;

: ((new, ( link -- )
  dup @ ?dup IF  recurse  THEN   cell+ 2@ swap ohere + >r
  ?dup IF  ohere >r dup >r :newlink + @ recurse r> r> !  THEN
  r> to ohere ;

: (new  ( object -- )
  ohere >r dup >r :newlink + @ ((new, r> r> ! ;

: init-instance ( pos link -- pos )
    dup >r @ ?dup IF  recurse  THEN  r> cell+ 2@
    IF  drop dup >r ^ +
        >o o@ :init + @ execute  0 o@ :newlink + @ recurse o>
        r> THEN + ;

: init-object ( object -- size )
    >o o@ :init + @ execute  0 o@ :newlink + @ init-instance o> ;

: (new, ( object -- ) ohere dup >r over :var# + @ erase (new
    r> init-object drop ;

: size@  ( objc -- size )  :var# + @ 8aligned ;
: (new[],   ( n o -- addr ) ohere >r
    dup size@ rot over * oallot r@ ohere dup >r 2 pick -
    ?DO  I to ohere >r dup >r (new, r> r> dup negate +LOOP
    2drop r> to ohere r> ;

\ new,                                                 29oct94py

Create chunks here 16 cells dup allot erase

: DelFix ( addr root -- ) dup @ 2 pick ! ! ;

: NewFix  ( root size # -- addr )
  BEGIN  2 pick @ ?dup 0=
  WHILE  2dup * allocate throw over 0
         ?DO    dup 4 pick DelFix 2 pick +
         LOOP
         drop
  REPEAT
  >r drop r@ @ rot ! r@ swap erase r> ;

: >chunk ( n -- root n' )
  1- -8 and dup 3 rshift cells chunks + swap 8 + ;

: Dalloc ( size -- addr )
  dup 128 > IF  allocate throw EXIT  THEN
  >chunk 2048 over / NewFix ;

: Salloc ( size -- addr ) align here swap allot ;

: dispose, ( addr size -- )
    dup 128 > IF drop free throw EXIT THEN
    >chunk drop DelFix ;

: new, ( o -- addr )  dup :var# + @
  alloc @ execute dup >r to ohere (new, r> ;

: new[], ( n o -- addr )  dup :var# + @ 8aligned
  2 pick * alloc @ execute to ohere (new[], ;

Forth definitions

: dynamic ['] Dalloc alloc ! ;  dynamic
: static  ['] Salloc alloc ! ;

Objects definitions

\ instance creation                                    29mar94py

: instance, ( o -- )  alloc @ >r static new, r> alloc ! drop
  DOES> state @ IF  dup postpone Literal oset? IF  postpone op!  ELSE  postpone >o  THEN  THEN early,
;
: ptr,      ( o -- )  0 , ,
  DOES>  state @
    IF    dup postpone Literal postpone @ oset? IF  postpone op!  ELSE  postpone >o  THEN cell+
    ELSE  @  THEN late, ;

: array,  ( n o -- )  alloc @ >r static new[], r> alloc ! drop
    DOES> ( n -- ) dup dup @ size@
          state @ IF  o*,  ELSE  nip rot * +  THEN  early, ;

\ class creation                                       29mar94py

Variable voc#
Variable classlist
Variable old-current
Variable ob-interface

: voc! ( addr -- )  get-current old-current !
  add-order  2 + voc# !
  get-order wordlist tuck classlist ! 1+ set-order
  also types classlist @ set-current ;

: (class-does>  DOES> false method, ;

: (class ( parent -- )  (class-does>
    here lastob !  true decl !  0 ob-interface !
    0 ,  dup voc!  dup lastparent !
  dup 0= IF  0  ELSE  :method# + 2@  THEN  methods ! vars ! ;

: (is ( addr -- )  bl word findo drop
    dup defer? abort" not deferred!"
    >body @ state @
    IF    postpone ^ postpone Literal postpone + postpone !
    ELSE  ^ + !  THEN ;

: inherit   ( -- )  bl word findo drop
    dup exec?  IF  >body @ dup o@ + @ swap lastob @ + !  EXIT  THEN
    abort" Not a polymorph method!" ;

\ instance variables inside objects                    27dec93py

: instvar,    ( addr -- ) dup , here 0 , 0 vallot swap !
    'link @ 2 cells + @  IF  'link @ link,  THEN
    'link @ >r dup r@ cell+ ! :var# + @ dup vars +! r> 2 cells + !
    DOES>  dup 2@ swap state @ IF  o+,  ELSE  ^ + nip nip  THEN
           early, ;

: instptr>  ( -- )  DOES>  dup 2@ swap
    state @ IF  o+@,  ELSE  ^ + @ nip nip  THEN  late, ;

: instptr,    ( addr -- )  , here 0 , cell vallot swap !
    instptr> ;

: (o* ( i addr -- addr' ) dup @ :var# + @ 8aligned rot * + ;

: instarray,  ( addr -- )  , here 0 , cell vallot swap !
    DOES>  dup 2@ swap
           state @  IF  o+@*,  ELSE  ^ + @ nip nip (o*  THEN
           late, ;

\ bind instance pointers                               27mar94py

: ((link ( addr -- o addr' ) 2@ swap ^ + ;

: (link  ( -- o addr )  bl word findo drop >body state @
    IF postpone Literal postpone ((link EXIT THEN ((link ;

: parent? ( class o -- class class' ) @
  BEGIN  2dup = ?EXIT dup  WHILE  :parent + @  REPEAT ;

: (bound ( obj1 obj2 adr2 -- ) >r over parent?
    nip 0= abort" not the same class !" r> ! ;

: (bind ( addr -- ) \ <name>
    (link state @ IF postpone (bound EXIT THEN (bound ;

: (sbound ( o addr -- ) dup cell+ @ swap (bound ;

Forth definitions

: bind ( o -- )  '  state @
  IF   postpone Literal postpone >body postpone (sbound EXIT  THEN
  >body (sbound ;  immediate

Objects definitions

\ method implementation                                29oct94py

Variable m-name
Variable last-interface  0 last-interface !

: interface, ( -- )  last-interface @
    BEGIN  dup  WHILE  dup , @  REPEAT drop ;

: inter, ( iface -- )
    align here over :inum + @ lastob @ + !
    here over :ilen + @ dup allot move ;

: interfaces, ( -- ) ob-interface @ lastob @ :iface + !
    ob-interface @
    BEGIN  dup  WHILE  2@ inter,  REPEAT  drop ;

: lastob!  ( -- )  lastob @ dup
    BEGIN  nip dup @ here cell+ 2 pick ! dup 0= UNTIL  drop
    dup , op! o@ lastob ! ;

: thread,  ( -- )  classlist @ , ;
: var,     ( -- )  methods @ , vars @ , ;
: parent,  ( -- o parent )
    o@ lastparent @ 2dup dup , 0 ,
    dup IF  :child + dup @ , !   ELSE  , drop  THEN ;
: 'link,  ( -- )
    'link @ ?dup 0=
    IF  lastparent @ dup  IF  :newlink + @  THEN  THEN , ;
: cells,  ( -- )
  methods @ :init ?DO  ['] crash , cell +LOOP ;

\ method implementation                                20feb95py

types definitions

: how:  ( -- ) \ oof- oof how-to
\G End declaration, start implementation
    decl @ 0= abort" not twice!" 0 decl !
    align  interface,
    lastob! thread, parent, var, 'link, 0 , cells, interfaces,
    dup
    IF    dup :method# + @ >r :init + swap r> :init /string move
    ELSE  2drop  THEN ;

: class; ( -- ) \ oof- oof end-class
\G End class declaration or implementation
    decl @ IF  how:  THEN  0 'link !
    voc# @ drop-order old-current @ set-current ;

: ptr ( -- ) \ oof- oof
    \G Create an instance pointer
    Create immediate lastob @ here lastob ! instptr, ;
: asptr ( class -- ) \ oof- oof
    \G Create an alias to an instance pointer, cast to another class.
    cell+ @ Create immediate
    lastob @ here lastob ! , ,  instptr> ;

: Fpostpone  postpone postpone ; immediate

: : ( <methodname> -- ) \ oof- oof colon
    decl @ abort" HOW: missing! "
    bl word findo 0= abort" not found"
    dup exec? over early? or over >body cell+ @ 0< or
    0= abort" not a method"
    m-name ! :noname ;

Forth

: ; ( xt colon-sys -- ) \ oof- oof
    postpone ;
    m-name @ dup >body swap exec?
    IF    @ o@ +
    ELSE  dup cell+ @ 0< IF  2@ swap o@ + @ +  THEN
    THEN ! ; immediate

Forth definitions

\ object                                               23mar95py

Create object  immediate  0 (class \ do not create as subclass
         cell var  oblink       \ create offset for backlink
         static    thread       \ method/variable wordlist
         static    parento      \ pointer to parent
         static    childo       \ ptr to first child
         static    nexto        \ ptr to next child of parent
         static    method#      \ number of methods (bytes)
         static    size         \ number of variables (bytes)
	 static    newlink      \ ptr to allocated space
	 static    ilist        \ interface list
	 method    init ( ... -- ) \ object- oof
         method    dispose ( -- ) \ object- oof

         early     class ( "name" -- ) \ object- oof
	 early     new ( -- o ) \ object- oof
	 			immediate
	 early     new[] ( n -- o ) \ object- oof new-array
				immediate
         early     : ( "name" -- ) \ object- oof define
         early     ptr ( "name" -- ) \ object- oof
         early     asptr ( o "name" -- ) \ object- oof
         early     [] ( n "name" -- ) \ object- oof array
	 early     ::  ( "name" -- ) \ object- oof scope
	 			immediate
         early     class? ( o -- flag ) \ object- oof class-query
	 early     super  ( "name" -- ) \ object- oof
				immediate
         early     self ( -- o ) \ object- oof
	 early     bind ( o "name" -- ) \ object- oof
				immediate
         early     bound ( class addr "name" -- ) \ object- oof
	 early     link ( "name" -- class addr ) \ object- oof
				immediate
	 early     is  ( xt "name" -- ) \ object- oof
				immediate
	 early     send ( xt -- ) \ object- oof
				immediate
	 early     with ( o -- ) \ object- oof
				immediate
	 early     endwith ( -- ) \ object- oof
				immediate
	 early     ' ( "name" -- xt ) \ object- oof tick
				immediate
	 early     postpone ( "name" -- ) \ object- oof
				immediate
	 early     definitions ( -- ) \ object- oof
	 
\ base object class implementation part                23mar95py

how:
0 parento !
0 childo !
0 nexto !
    : class   ( -- )       Create immediate o@ (class ;
    : :       ( -- )       Create immediate o@
	decl @ IF  instvar,    ELSE  instance,  THEN ;
    : ptr     ( -- )       Create immediate o@
	decl @ IF  instptr,    ELSE  ptr,       THEN ;
    : asptr   ( addr -- )
	decl @ 0= abort" only in declaration!"
	Create immediate o@ , cell+ @ , instptr> ;
    : []      ( n -- )     Create immediate o@
	decl @ IF  instarray,  ELSE  array,     THEN ;
    : new     ( -- o )     o@ state @
	IF  Fpostpone Literal Fpostpone new,  ELSE  new,  THEN ;
    : new[]   ( n -- o )   o@ state @
	IF Fpostpone Literal Fpostpone new[], ELSE new[], THEN ;
    : dispose ( -- )       ^ size @ dispose, ;
    : bind    ( addr -- )  (bind ;
    : bound   ( o1 o2 addr2  -- ) (bound ;
    : link    ( -- o addr ) (link ;
    : class?  ( class -- flag )  ^ parent? nip 0<> ;
    : ::      ( -- )
	state @ IF  ^ true method,  ELSE  inherit  THEN ;
    : super   ( -- )       parento true method, ;
    : is      ( cfa -- )   (is ;
    : self    ( -- obj )   ^ ;
    : init    ( -- )       ;
    
    : '       ( -- xt )  bl word findo 0= abort" not found!"
	state @ IF  Fpostpone Literal  THEN ;
    : send    ( xt -- )  execute ;
    : postpone ( -- )  o@ add-order Fpostpone Fpostpone drop-order ;
    
    : with ( -- )
	state @ oset? 0= and IF  Fpostpone >o  THEN
	o@ add-order voc# ! false to oset? ;
    : endwith  Fpostpone o> voc# @ drop-order ;

    : definitions
	o@ add-order 1+ voc# ! also types o@ lastob !
	false to oset?   get-current old-current !
	thread @ set-current ;
class; \ object

\ interface                                            01sep96py

Objects definitions

: implement ( interface -- ) \ oof-interface- oof
    align here over , ob-interface @ , ob-interface !
    :ilist + @ >r get-order r> swap 1+ set-order  1 voc# +! ;

: inter-method, ( interface -- ) \ oof-interface- oof
    :ilist + @ bl word count 2dup s" '" str=
    dup >r IF  2drop bl word count  THEN
    rot search-wordlist
    dup 0= abort" Not an interface method!"
    r> IF  drop state @ IF  postpone Literal  THEN  EXIT  THEN
    0< state @ and  IF  compile,  ELSE  execute  THEN ;

Variable inter-list
Variable lastif
Variable inter#

Vocabulary interfaces  interfaces definitions

: method  ( -- ) \ oof-interface- oof
    mallot Create , inter# @ ,
DOES> 2@ swap o@ + @ + @ execute ;

: how: ( -- ) \ oof-interface- oof
    align
    here lastif @ !  0 decl !
    here  last-interface @ ,  last-interface !
    inter-list @ ,  methods @ ,  inter# @ ,
    methods @ :inum cell+ ?DO  ['] crash ,  LOOP ;

: interface; ( -- ) \ oof-interface- oof
    old-current @ set-current
    previous previous ;

: : ( <methodname> -- ) \ oof-interface- oof colon
    decl @ abort" HOW: missing! "
    bl word count lastif @ @ :ilist + @
    search-wordlist 0= abort" not found"
    dup >body cell+ @ 0< 0= abort" not a method"
    m-name ! :noname ;

Forth

: ; ( xt colon-sys -- ) \ oof-interface- oof
  postpone ;
  m-name @ >body @ lastif @ @ + ! ; immediate

Forth definitions

: interface-does>
    DOES>  @ decl @  IF  implement  ELSE  inter-method,  THEN ;
: interface ( -- ) \ oof-interface- oof
    Create  interface-does>
    here lastif !  0 ,  get-current old-current !
    last-interface @ dup  IF  :inum @  THEN  1 cells - inter# !
    get-order wordlist
    dup inter-list ! dup set-current swap 1+ set-order
    true decl !
    0 vars ! :inum cell+ methods !  also interfaces ;

previous previous


