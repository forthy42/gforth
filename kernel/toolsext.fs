\ Copyright (C) 1995,1998,2000,2003,2005,2007 Free Software Foundation, Inc.

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

Warnings off

Variable countif

: dummy ;  immediate
: >exec  >r ; restrict ( :-)
: scanIF   f83find  dup 0=  IF  drop ['] dummy >head-noprim  THEN  ;

Create [struct]-search    ' scanIF A,  ' (reveal) A,  ' drop A, ' drop A,
Create [struct]-voc       [struct]-search A,
                          NIL A,       NIL A,       NIL A,

: ?if  countif @ 0<
  IF  [ [struct]-voc 3 cells + ] ALiteral @ lookup !  THEN ;

UNLOCK  Tlast @ TNIL Tlast !  LOCK
\ last @  0 last !

: [IF]
  1 countif +! ?if ;       immediate
: [THEN]
  -1 countif +! ?if ;       immediate
: [ELSE]
  postpone [THEN] postpone [IF] ;
                                     immediate
' [IF]   Alias [IFDEF]               immediate
' [IF]   Alias [IFUNDEF]             immediate
' [THEN] Alias [ENDIF]               immediate
' [IF]   Alias [BEGIN]               immediate
' [IF]   Alias [WHILE]               immediate
' [THEN] Alias [UNTIL]               immediate
' [THEN] Alias [AGAIN]               immediate
' [IF]   Alias [DO]                  immediate
' [IF]   Alias [?DO]                 immediate
' [THEN] Alias [LOOP]                immediate
' [THEN] Alias [+LOOP]               immediate
: [REPEAT]  postpone [AGAIN] postpone [THEN] ;
                                     immediate
\ The following was too smart for its own good; consider "postpone (".
\ Moreover, ANS Forth specifies that the next [THEN] ends an [IF]
\ (even if its in a '( ... )').

\ ' ( Alias (                          immediate ( keep fontify happy)
\ ' \ Alias \                          immediate

UNLOCK Tlast @ swap Tlast ! LOCK
\ last @ swap last !
1 cells - [struct]-voc cell+ !

\ Interpretative Structuren                            30apr92py

: [defined] ( "<spaces>name" -- flag )   bl word find nip 0<> ; immediate
  \G returns true if name is found in current search order
' [defined] alias defined immediate
: [undefined] ( "<spaces>name" -- flag ) postpone [defined] 0= ; immediate
  \G returns false if name is found in current search order

: [IF] ( flag -- ) \ tools-ext bracket-if
  \G If flag is @code{TRUE} do nothing (and therefore
  \G execute subsequent words as normal). If flag is @code{FALSE},
  \G parse and discard words from the parse
  \G area (refilling it if necessary using
  \G @code{REFILL}) including nested instances of @code{[IF]}..
  \G @code{[ELSE]}.. @code{[THEN]} and @code{[IF]}.. @code{[THEN]}
  \G until the balancing @code{[ELSE]} or @code{[THEN]} has been
  \G parsed and discarded. Immediate word.
       0= IF  countif off
              lookup @ [ [struct]-voc 3 cells + ] ALiteral !
	      [struct]-voc lookup !
          THEN ;                                      immediate

: [IFDEF] ( "<spaces>name" -- ) \ gforth bracket-if-def
  \G If name is found in the current search-order, behave like
  \G @code{[IF]} with a @code{TRUE} flag, otherwise behave like
  \G @code{[IF]} with a @code{FALSE} flag. Immediate word.
  postpone [defined]    postpone [IF] ;                 immediate

: [IFUNDEF] ( "<spaces>name" -- ) \ gforth bracket-if-un-def
  \G If name is not found in the current search-order, behave like
  \G @code{[IF]} with a @code{TRUE} flag, otherwise behave like
  \G @code{[IF]} with a @code{FALSE} flag. Immediate word.
  postpone [defined] 0= postpone [IF] ;                 immediate

: [ELSE]  ( -- ) \ tools-ext bracket-else
  \G Parse and discard words from the parse
  \G area (refilling it if necessary using
  \G @code{REFILL}) including nested instances of @code{[IF]}..
  \G @code{[ELSE]}.. @code{[THEN]} and @code{[IF]}.. @code{[THEN]}
  \G until the balancing @code{[THEN]} has been parsed and discarded.
  \G @code{[ELSE]} only gets executed if the balancing @code{[IF]}
  \G was @code{TRUE}; if it was @code{FALSE}, @code{[IF]} would
  \G have parsed and discarded the @code{[ELSE]}, leaving the
  \G subsequent words to be executed as normal.
  \G Immediate word.
  0 postpone [IF] ;                                   immediate

: [THEN] ( -- ) \ tools-ext bracket-then
  \G Do nothing; used as a marker for other words to parse
  \G and discard up to. Immediate word.
  ;                                                   immediate

: [ENDIF] ( -- ) \ gforth bracket-end-if
  \G Do nothing; synonym for @code{[THEN]}
  ;                                                   immediate

\ Structs for interpreter                              28nov92py

User (i)

: [DO]  ( n-limit n-index -- ) \ gforth bracket-do
  >in @ -rot
  DO   I (i) ! dup >r >in ! interpret r> swap +LOOP  drop ;
                                                      immediate

: [?DO] ( n-limit n-index -- ) \ gforth bracket-question-do
  2dup = IF 2drop postpone [ELSE] ELSE postpone [DO] THEN ;
                                                      immediate

: [+LOOP] ( n -- ) \ gforth bracket-question-plus-loop
  rdrop ;                                             immediate

: [LOOP] ( -- ) \ gforth bracket-loop
  1 rdrop ;                                           immediate

: [FOR] ( n -- ) \ gforth bracket-for
  0 swap postpone [DO] ;                              immediate

: [NEXT] ( n -- ) \ gforth bracket-next
  -1 rdrop ;                                          immediate

:noname (i) @ ;
:noname (i) @ postpone Literal ;
interpret/compile: [I] ( -- n ) \ gforth bracket-i

: [BEGIN] ( -- ) \ gforth bracket-begin
  >in @ >r BEGIN r@ >in ! interpret UNTIL rdrop ;     immediate

' [+LOOP]  Alias [UNTIL] ( flag -- ) \ gforth bracket-until
                                                      immediate

: [REPEAT]  ( -- ) \ gforth bracket-repeat
  false rdrop ;                                       immediate

' [REPEAT] Alias [AGAIN] ( -- ) \ gforth bracket-again
                                                      immediate

: [WHILE]   ( flag -- ) \ gforth bracket-while
  0= IF   postpone [ELSE] true rdrop 1 countif +!  THEN ;
                                                      immediate

\ Warnings on