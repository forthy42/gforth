\ yet another Forth objects extension

\ written by Anton Ertl 1996-2000
\ public domain; NO WARRANTY

\ This (in combination with compat/struct.fs) is in ANS Forth (with an
\ environmental dependence on case insensitivity; convert everything
\ to upper case for state sensitive systems).

\ compat/struct.fs and this file together use the following words:

\ from CORE :
\ : 1- + swap invert and ; DOES> @ immediate drop Create rot dup , >r
\ r> IF ELSE THEN over chars aligned cells 2* here - allot execute
\ POSTPONE ?dup 2dup move Variable 2@ 2! ! ['] >body = 2drop ' r@ +!
\ Constant recurse 1+ BEGIN 0= UNTIL negate Literal ." .
\ from CORE-EXT :
\ tuck pick nip true <> 0> erase Value :noname compile, 
\ from BLOCK-EXT :
\ \ 
\ from DOUBLE :
\ 2Constant 
\ from EXCEPTION :
\ throw catch 
\ from EXCEPTION-EXT :
\ abort" 
\ from FILE :
\ ( 
\ from FLOAT :
\ faligned floats 
\ from FLOAT-EXT :
\ dfaligned dfloats sfaligned sfloats 
\ from LOCAL :
\ TO 
\ from MEMORY :
\ allocate resize free 
\ from SEARCH :
\ get-order set-order wordlist get-current set-current 

\ needs struct.fs

\ helper words

s" gforth" environment? [if]
    2drop
[else]

: -rot ( a b c -- c a b )
    rot rot ;

: perform ( ... addr -- ... )
    @ execute ;

: ?dup-if ( compilation: -- orig ; run-time: n -- n|  )
    POSTPONE ?dup POSTPONE if ; immediate

: save-mem	( addr1 u -- addr2 u ) \ gforth
    \ copy a memory block into a newly allocated region in the heap
    swap >r
    dup allocate throw
    swap 2dup r> -rot move ;

: resize ( a-addr1 u -- a-addr2 ior ) \ gforth
    over
    if
	resize
    else
	nip allocate
    then ;

: extend-mem	( addr1 u1 u -- addr addr2 u2 )
    \ extend memory block allocated from the heap by u aus
    \ the (possibly reallocated) piece is addr2 u2, the extension is at addr
    over >r + dup >r resize throw
    r> over r> + -rot ;

: \g ( -- )
    postpone \ ; immediate
[then]

\ data structures

struct
    cell% field object-map
end-struct object%

struct
    cell% 2* field interface-map
    cell%    field interface-map-offset \ aus
      \ difference between where interface-map points and where
      \ object-map points (0 for non-classes)
    cell%    field interface-offset \ aus
      \ offset of interface map-pointer in class-map (0 for classes)
end-struct interface%

interface%
    cell%    field class-parent
    cell%    field class-wordlist \ inst-vars and other protected words
    cell% 2* field class-inst-size ( class -- addr ) \ objects- objects
    \g Give the size specification for an instance (i.e. an object)
    \g of @var{class};
    \g used as @code{class-inst-size 2@ ( class -- align size )}.
end-struct class%

struct
    cell% field selector-offset \ the offset within the (interface) map
    cell% field selector-interface \ the interface offset
end-struct selector%

\ maps are not defined explicitly; they have the following structure:

\ pointers to interface maps (for classes) <- interface-map points here
\ interface%/class% pointer                <- (object-)map  points here
\ xts of methods 


\ code

\ selectors and methods

variable current-interface ( -- addr ) \ objects- objects
\g Variable: contains the class or interface currently being
\g defined.



: no-method ( -- )
    true abort" no method defined for this object/selector combination" ;

: do-class-method ( -- )
does> ( ... object -- ... )
    ( object selector-body )
    selector-offset @ over object-map @ + ( object xtp ) perform ;

: do-interface-method ( -- )
does> ( ... object -- ... )
    ( object selector-body )
    2dup selector-interface @ ( object selector-body object interface-offset )
    swap object-map @ + @ ( object selector-body map )
    swap selector-offset @ + perform ;

: method ( xt "name" -- ) \ objects- objects
    \g @code{name} execution: @code{... object -- ...}@*
    \g Create selector @var{name} and makes @var{xt} its method in
    \g the current class.
    create
    current-interface @ interface-map 2@ ( xt map-addr map-size )
    dup current-interface @ interface-map-offset @ - ,
    1 cells extend-mem current-interface @ interface-map 2! ! ( )
    current-interface @ interface-offset @ dup ,
    ( 0<> ) if
	do-interface-method
    else
	do-class-method
    then ;

: selector ( "name" -- ) \ objects- objects
    \g @var{name} execution: @code{... object -- ...}@*
    \g Create selector @var{name} for the current class and its
    \g descendents; you can set a method for the selector in the
    \g current class with @code{overrides}.
    ['] no-method method ;

: interface-override! ( xt sel-xt interface-map -- )
    \ xt is the new method for the selector sel-xt in interface-map
    swap >body ( xt map selector-body )
    selector-offset @ + ! ;

: class->map ( class -- map ) \ objects- objects
    \g @var{map} is the pointer to @var{class}'s method map; it
    \g points to the place in the map to which the selector offsets
    \g refer (i.e., where @var{object-map}s point to).
    dup interface-map 2@ drop swap interface-map-offset @ + ;

: unique-interface-map ( class-map offset -- )
    \ if the interface at offset in class map is the same as its parent,
    \ copy it to make it unique; used for implementing a copy-on-write policy
    over @ class-parent @ class->map ( class-map offset parent-map )
    over + @ >r  \ the map for the interface for the parent
    + dup @ ( interface-mapp interface-map )
    dup r> =
    if
	dup @ interface-map 2@ nip save-mem drop	
	swap !
    else
	2drop
    then ;

: class-override! ( xt sel-xt class-map -- ) \ objects- objects
    \g @var{xt} is the new method for the selector @var{sel-xt} in
    \g @var{class-map}.
    over >body ( xt sel-xt class-map selector-body )
    selector-interface @ ( xt sel-xt class-map offset )
    ?dup-if \ the selector is for an interface
	2dup unique-interface-map
	+ @
    then
    interface-override! ;

: overrides ( xt "selector" -- ) \ objects- objects
    \g replace default method for @var{selector} in the current class
    \g with @var{xt}. @code{overrides} must not be used during an
    \g interface definition.
    ' current-interface @ class->map class-override! ;

\ interfaces

\ every interface gets a different offset; the latest one is stored here
variable last-interface-offset 0 last-interface-offset !

: interface ( -- ) \ objects- objects
    \g Start an interface definition.
    interface% %allot >r
    r@ current-interface !
    current-interface 1 cells save-mem r@ interface-map 2!
    -1 cells last-interface-offset +!
    last-interface-offset @ r@ interface-offset !
    0 r> interface-map-offset ! ;

: end-interface-noname ( -- interface ) \ objects- objects
    \g End an interface definition. The resulting interface is
    \g @var{interface}.
    current-interface @ ;

: end-interface ( "name" -- ) \ objects- objects
    \g @code{name} execution: @code{-- interface}@*
    \g End an interface definition. The resulting interface is
    \g @var{interface}.
    end-interface-noname constant ;

\ visibility control

variable public-wordlist

: protected ( -- ) \ objects- objects
    \g Set the compilation wordlist to the current class's wordlist
    current-interface @ class-wordlist @
    dup get-current <>
    if \ we are not protected already
	get-current public-wordlist !
    then
    set-current ;

: public ( -- ) \ objects- objects
    \g Restore the compilation wordlist that was in effect before the
    \g last @code{protected} that actually changed the compilation
    \g wordlist.
    current-interface @ class-wordlist @ get-current =
    if \ we are protected
	public-wordlist @ set-current
    then ;

\ classes

: add-class-order ( n1 class -- wid1 ... widn n+n1 )
    dup >r class-parent @
    ?dup-if
	recurse \ first add the search order for the parent class
    then
    r> class-wordlist @ swap 1+ ;

: class>order ( class -- ) \ objects- objects
    \g Add @var{class}'s wordlists to the head of the search-order.
    >r get-order r> add-class-order set-order ;

: push-order class>order ; \ old name

: methods ( class -- ) \ objects- objects
    \g Makes @var{class} the current class. This is intended to be
    \g used for defining methods to override selectors; you cannot
    \g define new fields or selectors.
    dup current-interface ! class>order ;

: class ( parent-class -- align offset ) \ objects- objects
    \g Start a new class definition as a child of
    \g @var{parent-class}. @var{align offset} are for use by
    \g @var{field} etc.
    class% %allot >r
    dup interface-map 2@ save-mem r@ interface-map 2!
    dup interface-map-offset @ r@ interface-map-offset !
    r@ dup class->map !
    0 r@ interface-offset !
    dup r@ class-parent !
    wordlist r@ class-wordlist !
    r> methods
    class-inst-size 2@ ;

: remove-class-order ( wid1 ... widn n+n1 class -- n1 )
    \ note: no checks, whether the wordlists are correct
    begin
	>r nip 1-
	r> class-parent @ dup 0=
    until
    drop ;

: class-previous ( class -- ) \ objects- objects
    \g Drop @var{class}'s wordlists from the search order. No
    \g checking is made whether @var{class}'s wordlists are actually
    \g on the search order.
    >r get-order r> remove-class-order set-order ;

: drop-order class-previous ; \ old name

: end-methods ( -- ) \ objects- objects
    \g Switch back from defining methods of a class to normal mode
    \g (currently this just restores the old search order).
    current-interface @ class-previous ;

: end-class-noname ( align offset -- class ) \ objects- objects
    \g End a class definition. The resulting class is @var{class}.
    public end-methods
    current-interface @ class-inst-size 2!
    end-interface-noname ;

: end-class ( align offset "name" -- ) \ objects- objects
    \g @var{name} execution: @code{-- class}@*
    \g End a class definition. The resulting class is @var{class}.
    \ name execution: ( -- class )
    end-class-noname constant ;

\ classes that implement interfaces

: front-extend-mem ( addr1 u1 u -- addr addr2 u2 )
    \ Extend memory block allocated from the heap by u aus, with the
    \ old stuff coming at the end
    2dup + dup >r allocate throw ( addr1 u1 u addr2 ; R: u2 )
    dup >r + >r over r> rot move ( addr1 ; R: u2 addr2 )
    free throw
    r> dup r> ;
    
: implementation ( interface -- ) \ objects- objects
    \g The current class implements @var{interface}. I.e., you can
    \g use all selectors of the interface in the current class and its
    \g descendents.
    dup interface-offset @ ( interface offset )
    current-interface @ interface-map-offset @ negate over - dup 0>
    if \ the interface does not fit in the present class-map
	>r current-interface @ interface-map 2@
	r@ front-extend-mem
	current-interface @ interface-map 2!
	r@ erase
	dup negate current-interface @ interface-map-offset !
	r>
    then ( interface offset n )
    drop >r
    interface-map 2@ save-mem drop ( map )
    current-interface @ dup interface-map 2@ drop
    swap interface-map-offset @ + r> + ! ;

\ this/self, instance variables etc.

\ rename "this" into "self" if you are a Smalltalk fiend
0 value this ( -- object ) \ objects- objects
\g the receiving object of the current method (aka active object).
: to-this ( object -- ) \ objects- objects
    \g Set @code{this} (used internally, but useful when debugging).
    TO this ;

\ another implementation, if you don't have (fast) values
\ variable thisp
\ : this ( -- object )
\     thisp @ ;
\ : to-this ( object -- )
\     thisp ! ;

: enterm ( -- ; run-time: object -- )
    \g method prologue; @var{object} becomes new @code{this}.
    POSTPONE this
    POSTPONE >r
    POSTPONE to-this ;
    
: m: ( -- xt colon-sys; run-time: object -- ) \ objects- objects
    \g Start a method definition; @var{object} becomes new @code{this}.
    :noname enterm ;

: :m ( "name" -- xt; run-time: object -- ) \ objects- objects
    \g Start a named method definition; @var{object} becomes new
    \g @code{this}.  Has to be ended with @code{;m}.
    : enterm ;

: exitm ( -- ) \ objects- objects
    \g @code{exit} from a method; restore old @code{this}.
    POSTPONE r>
    POSTPONE to-this
    POSTPONE exit ; immediate

: ;m ( colon-sys --; run-time: -- ) \ objects- objects
    \g End a method definition; restore old @code{this}.
    POSTPONE r>
    POSTPONE to-this
    POSTPONE ; ; immediate

: catch ( ... xt -- ... n ) \ exception
    \ Make it safe to call CATCH within a method.
    \ should also be done with all words containing CATCH.
    this >r catch r> to-this ;

\ the following is a bit roundabout; this is caused by the standard
\ disallowing to change the compilation wordlist between CREATE and
\ DOES> (see RFI 3)

: inst-something ( align1 size1 align size xt "name" -- align2 size2 )
    \ xt ( -- ) typically is for a DOES>-word
    get-current >r
    current-interface @ class-wordlist @ set-current
    >r create-field r> execute
    r> set-current ;

: do-inst-var ( -- )
does> \ name execution: ( -- addr )
    ( addr1 ) @ this + ;

: inst-var ( align1 offset1 align size "name" -- align2 offset2 ) \ objects- objects
    \g @var{name} execution: @code{-- addr}@*
    \g @var{addr} is the address of the field @var{name} in
    \g @code{this} object.
    ['] do-inst-var inst-something ;

: do-inst-value ( -- )
does> \ name execution: ( -- w )
    ( addr1 ) @ this + @ ;

: inst-value ( align1 offset1 "name" -- align2 offset2 ) \ objects- objects
    \g @var{name} execution: @code{-- w}@*
    \g @var{w} is the value of the field @var{name} in @code{this}
    \g object.
    cell% ['] do-inst-value inst-something ;

: <to-inst> ( w xt -- ) \ objects- objects
    \g store @var{w} into the field @var{xt} in @code{this} object.
    >body @ this + ! ;

: [to-inst] ( compile-time: "name" -- ; run-time: w -- ) \ objects- objects
    \g store @var{w} into field @var{name} in @code{this} object.
    ' >body @ POSTPONE literal
    POSTPONE this
    POSTPONE +
    POSTPONE ! ; immediate

\ class binding stuff

: <bind> ( class selector-xt -- xt ) \ objects- objects
    \g @var{xt} is the method for the selector @var{selector-xt} in
    \g @var{class}.
    >body swap class->map over selector-interface @
    ?dup-if
	+ @
    then
    swap selector-offset @ + @ ;

: bind' ( "class" "selector" -- xt ) \ objects- objects
    \g @var{xt} is the method for @var{selector} in @var{class}.
    ' execute ' <bind> ;

: bind ( ... "class" "selector" -- ... ) \ objects- objects
    \g Execute the method for @var{selector} in @var{class}.
    bind' execute ;

: [bind] ( compile-time: "class" "selector" -- ; run-time: ... object -- ... ) \ objects- objects
    \g Compile the method for @var{selector} in @var{class}.
    bind' compile, ; immediate

: current' ( "selector" -- xt ) \ objects- objects
    \g @var{xt} is the method for @var{selector} in the current class.
    current-interface @ ' <bind> ;

: [current] ( compile-time: "selector" -- ; run-time: ... object -- ... ) \ objects- objects
    \g Compile the method for @var{selector} in the current class.
    current' compile, ; immediate

: [parent] ( compile-time: "selector" -- ; run-time: ... object -- ... ) \ objects- objects
    \g Compile the method for @var{selector} in the parent of the
    \g current class.
    current-interface @ class-parent @ ' <bind> compile, ; immediate

\ the object class

\ because OBJECT has no parent class, we have to build it by hand
\ (instead of with class)

class% %allot current-interface !
current-interface 1 cells save-mem current-interface @ interface-map 2!
0 current-interface @ interface-map-offset !
0 current-interface @ interface-offset !
0 current-interface @ class-parent !
wordlist current-interface @ class-wordlist !
object%
current-interface @ class>order

' drop ( object -- )
method construct ( ... object -- ) \ objects- objects
\g Initialize the data fields of @var{object}. The method for the
\g class @var{object} just does nothing: @code{( object -- )}.

:noname ( object -- )
    ." object:" dup . ." class:" object-map @ @ . ;
method print ( object -- ) \ objects- objects
\g Print the object. The method for the class @var{object} prints
\g the address of the object and the address of its class.

selector equal ( object1 object2 -- flag )

end-class object ( -- class ) \ objects- objects
\g the ancestor of all classes.

\ constructing objects

: init-object ( ... class object -- ) \ objects- objects
    \g Initialize a chunk of memory (@var{object}) to an object of
    \g class @var{class}; then performs @code{construct}.
    swap class->map over object-map ! ( ... object )
    construct ;

: xt-new ( ... class xt -- object ) \ objects- objects
    \g Make a new object, using @code{xt ( align size -- addr )} to
    \g get memory.
    over class-inst-size 2@ rot execute
    dup >r init-object r> ;

: dict-new ( ... class -- object ) \ objects- objects
    \g @code{allot} and initialize an object of class @var{class} in
    \g the dictionary.
    ['] %allot xt-new ;

: heap-new ( ... class -- object ) \ objects- objects
    \g @code{allocate} and initialize an object of class @var{class}.
    ['] %alloc xt-new ;
