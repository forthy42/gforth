\ assertions

\ !! factor out line number printing, share with debugging.fs

variable assert-level \ all assertions above this level are turned off
1 assert-level !

: assertn ( n -- )
    \ this is internal (it is not immediate)
    assert-level @ >
    if
	POSTPONE (
    then ;

: assert0( ( -- )
    \ important assertions that should always be turned on
    0 assertn ; immediate
: assert1( ( -- )
    \ normal assertions; turned on by default
    1 assertn ; immediate
: assert2( ( -- )
    \ debugging assertions
    2 assertn ; immediate
: assert3( ( -- )
    \ slow assertions that you may not want to turn on in normal debugging;
    \ you would turn them on mainly for thorough checking
    3 assertn ; immediate
: assert( ( -- )
    \ equivalent to assert1(
    POSTPONE assert1( ; immediate

: (endassert) ( flag -- )
    \ three inline arguments
    if
	r> 3 cells + >r EXIT
    else
	r>
	dup 2@ type ." :" cell+ cell+
	@ 0 .r ." : failed assertion"
	true abort" assertion failed" \ !! or use a new throw code?
    then ;

: ) ( -- )
    POSTPONE (endassert) loadfilename 2@ 2, loadline @ , ; immediate
