\ assertions

\ !! factor out line number printing, share with debugging.fs

variable assert-level \ all assertions above this level are turned off
1 assert-level !

: assertn ( n -- )
    assert-level @ >
    if
	POSTPONE (
    then ;

: assert0( ( -- )
    0 assertn ; immediate
: assert1( ( -- )
    1 assertn ; immediate
: assert2( ( -- )
    2 assertn ; immediate
: assert3( ( -- )
    3 assertn ; immediate
: assert( ( -- )
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
