\ yet another Forth objects extension

\ written by Anton Ertl 1996
\ public domain

\ This (in combination with compat/struct.fs) is in ANS Forth (with an
\ environmental dependence on case insensitivity; convert everything
\ to upper case for state sensitive systems).

\ If you don't use Gforth, you have to load compat/struct.fs first.
\ compat/struct.fs and this file together use the following words:

\ from CORE :
\ : 1- + swap invert and ; Create >r rot r@ dup , r> DOES> @ chars
\ cells 2* here - allot drop over execute 2dup move 2! 2@ ! ['] >body
\ ' Variable POSTPONE immediate ." . cr Constant +!
\ from CORE-EXT :
\ tuck :noname compile, true 
\ from BLOCK-EXT :
\ \ 
\ from DOUBLE :
\ 2Constant 2VARIABLE 
\ from EXCEPTION :
\ throw catch 
\ from EXCEPTION-EXT :
\ abort" 
\ from FILE :
\ ( 
\ from FLOAT :
\ floats 
\ from FLOAT-EXT :
\ dfloats sfloats 
\ from MEMORY :
\ allocate resize 
\ from TOOLS-EXT :
\ [IF] [THEN]

\ ---------------------------------------
\ MANUAL:

\ A class is defined like this:

\ <parent> class
\   ... field <name>
\   ...

\   ... inst-var <name>
\   ...

\ selector <name>

\ :noname ( ... object -- ... )
\   ... ;
\ method <name> \ new method
\ ...

\ :noname ( ... object -- ... )
\   ... ;
\ overrides <name> \ existing method
\ ...

\ end-class <name>

\ you can write fields, inst-vars, selectors, methods and overrides in
\ any order.

\ A call of a method looks like this:

\ ... <object> <method>

\ (<object> just needs to reside on the stack, there's no need to name it).

\ Instead of defining a method with ':noname ... ;', you can define it
\ also with 'm: ... ;m'. The difference is that with ':noname' the
\ "self" object is on the top of stack; with 'm:' you can get it with
\ 'this'. You should use 'this' only in an 'm:' method even though the
\ sample implementation does not enforce this.

\ The difference between a field and and inst-var is that the field
\ refers to an object at the top of data stack (i.e. a field has the
\ stack effect (object -- addr), whereas the inst-var refers to this
\ (i.e., it has the stack effect ( -- addr )); obviously, an inst-var
\ can only be used in an 'm:' method.

\ 'method' defines a new method selector and binds a method to it.

\ 'selector' defines a new method selector without binding a method to
\ it (you can use this to define abstract classes)

\ 'overrides' binds a different method (than the parent class) to an
\ existing method selector.

\ If you want to perform early binding, you can do it like this:

\ ... <object> [bind] <class> <method> \ compilation
\ ... <object> bind  <class> <method> \ interpretation

\ You can get at the method from the method selector and the class like
\ this:

\ bind' <class> <method>
\ \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

\ needs struct.fs

\ helper words

: -rot ( a b c -- c a b )
    rot rot ;

: perform ( ... addr -- ... )
    @ execute ;

: save-mem	( addr1 u -- addr2 u ) \ gforth
    \ copy a memory block into a newly allocated region in the heap
    swap >r
    dup allocate throw
    swap 2dup r> -rot move ;

: extend-mem	( addr1 u1 u -- addr addr2 u2 )
    \ extend memory block allocated from the heap by u aus
    \ the (possibly reallocated piece is addr2 u2, the extension is at addr )
    over >r + dup >r resize throw
    r> over r> + -rot ;

: 2,	( w1 w2 -- ) \ gforth
    here 2 cells allot 2! ;

: const-field ( size1 align1 -- size2 align2 )
    1 cells: field
does> ( addr -- w )
    @ + @ ;

struct
    1 cells: field object-map
end-struct object-struct

struct
    2 cells: field class-map
    const-field class-instance-size \ aus
    const-field class-instance-align \ aus
end-struct class-struct

2variable current-map \ address and size (in aus) of the current map

: class ( class -- size align )
    dup class-map 2@ save-mem current-map 2!
    dup class-instance-size
    swap class-instance-align ;

: end-class ( size align "name" -- )
    create
    current-map 2@ 2,
    swap , , ;

: no-method ( -- )
    abort" no method defined" ;

: method ( xt "name" -- )
    \ define method and selector
    current-map 2@ ( xt map-addr map-size )
    create dup ,
    1 cells extend-mem current-map 2!
    !
does> ( ... object -- ... )
    ( object addr )
    @ over ( object-map ) @ + ( object xtp ) perform ;

: selector ( "name" -- )
    \ define a method selector for later overriding in subclasses
    ['] no-method method ;

: override! ( xt method-xt -- )
    >body @ current-map 2@ drop + ! ;

: overrides ( xt "selector" -- )
    \ replace default method "method" with xt
    ' override! ;

: alloc-instance ( class -- object )
    \ make a new, (almost) uninitialized instance of a class
    dup class-instance-size allocate throw
    swap class-map 2@ drop over ( object-map ) ! ;

\ this/self, instance variables etc.

variable thisp
: this ( -- object )
    \ rename this into self if you are a Smalltalk fiend
    thisp @ ;

: m: ( -- xt colon-sys ) ( run-time: object -- )
    :noname 
    POSTPONE this
    POSTPONE >r
    POSTPONE thisp
    POSTPONE ! ;

: ;m ( colon-sys -- ) ( run-time: -- )
    POSTPONE r>
    POSTPONE thisp
    POSTPONE !
    POSTPONE ; ; immediate

: catch ( ... xt -- ... n )
    \ make it safe to call CATCH within a method.
    \ should also be done with all words containing CATCH.
    this >r catch r> thisp ! ;

: inst-var ( size1 align1 size align -- size2 align2 )
    field
does> ( -- addr )
    ( addr1 ) @ this + ;

\ early binding stuff

\ this is not generally used, only where you want to do something like
\ superclass method invocation (so that you don't have to name your methods)

: (bind) ( class method-xt -- xt )
    >body @ swap class-map 2@ drop + @ ;

: bind' ( "class" "method" -- xt )
    ' >body ' (bind) ;

: bind ( ... object "class" "method" -- ... )
    bind' execute ;

: [bind] ( compile-time: "class" "method" -- ; run-time: ... object -- ... )
    bind' compile, ; immediate

\ the object class

0 0 save-mem current-map 2!
object-struct \ no class to inherit from, so we have to do this manually
:noname ( object -- )
    ." object:" dup . ." class:" object-map @ . ;
method print
end-class object

\ examples
true [if]
cr object alloc-instance print

object class
:noname ( object -- )
    drop ." undefined" ;
overrides print
end-class nothing

nothing alloc-instance constant undefined

cr undefined print

\ instance variables and this
object class
1 cells: inst-var count-n
m: ( object -- )
    count-n @ . ;m
overrides print
m: ( object -- )
   0 count-n ! ;m
method init
m: ( object -- )
    1 count-n +! ;m
method inc
end-class counter

counter alloc-instance constant counter1

cr
counter1 init
counter1 print
counter1 inc
counter1 print
counter1 inc
counter1 inc
counter1 inc
counter1 print
counter1 print

\ examples of static binding

cr undefined bind object print
: object-print ( object -- )
    [bind] object print ;

cr undefined object-print
[then]
