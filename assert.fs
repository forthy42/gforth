\ assertions

\ !! factor out line number printing, share with debugging.fs

variable assert-level ( -- a-addr ) \ gforth
\G all assertions above this level are turned off
1 assert-level !

: assertn ( n -- ) \ gforth assert-n
    \ this is internal (it is not immediate)
    assert-level @ >
    if
	POSTPONE (
    then ;

: assert0( ( -- ) \ gforth assert-zero
    \G important assertions that should always be turned on
    0 assertn ; immediate
: assert1( ( -- ) \ gforth assert-one
    \G normal assertions; turned on by default
    1 assertn ; immediate
: assert2( ( -- ) \ gforth assert-two
    \G debugging assertions
    2 assertn ; immediate
: assert3( ( -- ) \ gforth assert-three
    \G slow assertions that you may not want to turn on in normal debugging;
    \G you would turn them on mainly for thorough checking
    3 assertn ; immediate
: assert( ( -- ) \ gforth
    \G equivalent to assert1(
    POSTPONE assert1( ; immediate

: (endassert) ( flag -- ) \ gforth-internal
    \ three inline arguments
    if
	r> 3 cells + >r EXIT
    else
	r>
	dup 2@ type ." :" cell+ cell+
	@ 0 .r ." : failed assertion"
	true abort" assertion failed" \ !! or use a new throw code?
    then ;

: ) ( -- ) \ gforth	close-paren
    \G end an assertion
    POSTPONE (endassert) loadfilename 2@ 2, loadline @ , ; immediate
