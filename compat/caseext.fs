\ Eaker's CASE with extensions to make it a general control structure

\ This file is in the public domain. NO WARRANTY.

\ Dependencies: Assumes control-flow stack items are at least
\ partially on the data stack

\ compatibility stuff

[undefined] compile-only [if] : compile-only ; [then]
[undefined] cs-drop [if]
    : cs-drop postpone ahead 1 cs-roll postpone again postpone then ;
[then]
[undefined] \g [if] : \g source >in ! drop ; immediate [then]

\ a case-sys is: old-case-depth orig1 ... orign dest

variable case-depth \ contains the stack depth after old-case-depth was pushed

: case  ( compilation  -- case-sys ; run-time  -- ) \ core-ext
    \g Start a @code{case} structure.
    case-depth @ depth case-depth !
    postpone begin ; immediate compile-only

: ?of ( compilation  -- of-sys ; run-time  f -- ) \ gforth question-of
    \g If f is true, continue; otherwise, jump behind @code{endof} or
    \g @code{contof}.
    POSTPONE IF ; immediate compile-only

: of ( compilation  -- of-sys ; run-time x1 x2 -- |x1 ) \ core-ext
    \g If x1=x2, continue (dropping both); otherwise, leave x1 on the
    \g stack and jump behind @code{endof} or @code{contof}.
    postpone over postpone = postpone ?of postpone drop ; immediate compile-only

: endof ( compilation case-sys1 of-sys -- case-sys2 ; run-time  -- ) \ core-ext end-of
    \g Exit the enclosing @code{case} structure by jumping behind
    \g @code{endcase}/@code{next-case}.
    postpone else 1 cs-roll ; immediate compile-only

: contof ( compilation case-sys1 of-sys -- case-sys2 ; run-time  -- ) \ gforth cont-of
    \g Restart the @code{case} loop by jumping to the enclosing
    \g @code{case}.
    1 cs-pick postpone again postpone then ; immediate compile-only

: closecase ( old-case-depth orig1 ... orign -- )
    begin
	depth case-depth @ > while
	    postpone then
    repeat
    case-depth ! ;

: endcase ( compilation case-sys -- ; run-time x -- ) \ core-ext end-case
    \g Finish the @code{case} structure; drop x, and continue behind
    \g the @code{endcase}.  Dropping x is useful in the original
    \g @code{case} construct (with only @code{of}s), but you may have
    \g to supply an x in other cases (especially when using
    \g @code{?of}).
    postpone drop cs-drop closecase ; immediate compile-only

: next-case ( compilation case-sys -- ; run-time -- ) \ gforth
    \g Restart the @code{case} loop by jumping to the matching
    \g @code{case}.  Note that @code{next-case} does not drop a cell,
    \g unlike @code{endcase}.
    postpone again closecase ; immediate compile-only
