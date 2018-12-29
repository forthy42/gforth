\ forward definitions

\ Copyright (C) 2016,2017 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

\ This implementation relies on return address manipulation and
\ specific threaded-code properties

s" unresolved forward definition" exception constant unresolved-forward

: unfixed-forward ( ... -- ... )
    \ a forward child calls this word, which patches the call to the
    \ forward word (or reports an error if the forward word has not
    \ been resolved)
    r> dup {: retaddr :} cell+ @ dup 0= unresolved-forward and throw ( xt )
    dup r@ cell- {: target :}
    target @ retaddr 2 cells - = if \ primitive-centric call
        >body then
    target ! execute-exit ; 

: forward ( "name" -- )
    \ defines a very stylized colon definition, followed by a cell
    \ that contains 0 or (after resolution) the xt of the actual word
    \ (which must be a colon definition).  The call to UNFIXED-FORWARD
    \ is primitive-centric to make the implementation of
    \ UNFIXED-FORWARD easier.
    : ['] unfixed-forward opt-compile, ]] ; [[ 0 , ;

: is-forward? ( xt -- f )
    \ f is true if xt is a forward definition
    dup >code-address docol: = if
        >body cell+ @ ['] unfixed-forward >body = exit then
    drop false ;

: >forward-resolution ( xt -- addr )
    \ xt is of a forward word, addr is the cell containing the xt of
    \ the resolution (or 0).
    [ 3 cells >body ]L + ;

: auto-resolve ( addr u wid -- )
    \G auto-resolve the forward reference in check-shadow
    dup 2over rot find-name-in  dup IF
        dup is-forward? if
            latestxt swap >forward-resolution ! 2drop drop exit then
    then
    drop defers check-shadow ;

' auto-resolve is check-shadow

: .unresolved ( -- )
    \G print all unresolved forward references
    [: [: dup is-forward? IF
                dup >forward-resolution @ 0=
                [: dup .name ." is unresolved" cr ;] ?warning
            THEN  drop true ;] swap traverse-wordlist ;] map-vocs ;

\ testing
0 [if]
    forward forward1
    see forward1
    ' forward1 is-forward? cr .
    : forward2 forward1 ;
    ' forward2 5 cells dump
    ' forward2 catch cr .
    ' forward1 6 cells
    : forward1 285 ;
    dump
    forward2 .
    ' forward2 5 cells dump
    .s
[then]