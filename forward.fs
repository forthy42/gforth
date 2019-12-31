\ forward definitions

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2016,2017,2018,2019 Free Software Foundation, Inc.

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

\ note that there are related words with the same name here and in
\ forward1.fs, but they behave differently.

s" unresolved forward definition" exception constant unresolved-forward
s" forward must be resolved with :" exception constant forward-needs-:

: unresolved-forward-error ( -- )
    unresolved-forward throw ;

: unfixed-forward ( ... -- ... )
    \ the compiled code for a forward child branches to this word,
    \ which patches the call to the forward word; if the word has not
    \ been resolved, this produces an error after patching the call to
    \ report an error.
    r@ cell- {: target :}
    target @ cell- @ dup >body target !
    execute-exit ;

: forward ( "name" -- )
    \g Defines a forward reference to a colon definition.  Defining a
    \g colon definition with the same name in the same wordlist
    \g resolves the forward references.  Use @code{.unresolved} to
    \g check whether any forwards are unresolved.
    defer ['] unresolved-forward-error lastxt defer!
    ['] branch peephole-compile, ['] unfixed-forward >body ,
    [: ['] call peephole-compile, >body cell+ , ;] set-optimizer ;

: is-forward? ( xt -- f )
    \ f is true if xt is an unresolved forward definition
    dup >code-address dodefer: = if
        >body @ ['] unresolved-forward-error = exit then
    drop false ;

: auto-resolve ( addr u wid -- )
    \G auto-resolve the forward reference in check-shadow
    dup 2over rot find-name-in dup if
	dup is-forward? if
	    latestxt >code-address docol: <> forward-needs-: and throw
            latestxt swap defer! 2drop drop exit then then
    drop defers check-shadow ;

' auto-resolve is check-shadow

: .unresolved ( -- )
    \G print all unresolved forward references
    [: [:   replace-sourceview >r dup name>view to replace-sourceview
	    dup is-forward? [: dup .name ." is unresolved" cr ;] ?warning
	    r> to replace-sourceview
            drop true ;] swap traverse-wordlist ;] map-vocs ;

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
