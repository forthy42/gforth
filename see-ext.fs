\ see-ext.fs extentions for see locals, floats

\ made extra 26jan97jaw

: c-loop-lp+!#  c-loop cell+ ;
: c-?branch-lp+!#  c-?branch cell+ ;
: c-branch-lp+!#   c-branch  cell+ ;

: c-@local#
    Display? IF
	S" @local" 0 .string
	dup @ dup 1 cells / abs 0 <# #S rot sign #> 0 .string bl cemit
    THEN
    cell+ ;

: c-flit
    Display? IF
	dup f@ scratch represent 0=
	IF    2drop  scratch 3 min 0 .string
	ELSE
	    IF  '- cemit  THEN  1-
	    scratch over c@ cemit '. cemit 1 /string 0 .string
	    'E cemit
	    dup abs 0 <# #S rot sign #> 0 .string bl cemit
	THEN THEN
    float+ ;

: c-f@local#
    Display? IF
	S" f@local" 0 .string
	dup @ dup 1 floats / abs 0 <# #S rot sign #> 0 .string bl cemit
    THEN
    cell+ ;

: c-laddr#
    Display? IF
	S" laddr# " 0 .string
	dup @ dup abs 0 <# #S rot sign #> 0 .string bl cemit
    THEN
    cell+ ;

: c-lp+!#
    Display? IF
	S" lp+!# " 0 .string
	dup @ dup abs 0 <# #S rot sign #> 0 .string bl cemit
    THEN
    cell+ ;

create c-extend1
	' @local# A,        ' c-@local# A,
        ' flit A,           ' c-flit A,
	' f@local# A,       ' c-f@local# A,
	' laddr# A,         ' c-laddr# A,
	' lp+!# A,          ' c-lp+!# A,
        ' ?branch-lp+!# A,  ' c-?branch-lp+!# A,
        ' branch-lp+!# A,   ' c-branch-lp+!# A,
        ' (loop)-lp+!# A,   ' c-loop-lp+!# A,
        ' (+loop)-lp+!# A,  ' c-loop-lp+!# A,
        ' (s+loop)-lp+!# A, ' c-loop-lp+!# A,
        ' (-loop)-lp+!# A,  ' c-loop-lp+!# A,
        ' (next)-lp+!# A,   ' c-loop-lp+!# A,
	0 ,		here 0 ,

\ extend see-table
c-extender @
c-extend1 over a!
c-extender !
