\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke, Neal Crook
\ Copyright (C) 1995,1998,2000,2003,2005,2007,2009,2010,2012,2013,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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
Variable endif?  -1 cells allot -1 1 cells c,s

: dummy ;  immediate
: scan-rec  @ (listlfind)  dup 0=  IF  drop ['] dummy  THEN  translate-nt ;

Create [struct]-search    0 , ' drop A,  ' voc-to A,  ' scan-rec A,
' noop A, ' default-name>comp A, ' noname>string A, ' noname>link A,
' :dodoes A, [struct]-search A,  here  NIL A,  NIL A,
AConstant [struct]-voc

: scanif-r ( addr u -- xt )
    [struct]-voc find-name-in name?int ;

: ?if ( -- )  countif @ 0< IF  endif? on  THEN ;

: scanning? ( -- flag ) endif? @ 0= ;

UNLOCK  Tlast @ TNIL Tlast !  LOCK
\ last @  0 last !

: [IF]
  1 countif +! ?if ;        immediate
: [THEN]
  -1 countif +! ?if ;       immediate
: [ELSE]
  postpone [THEN] postpone [IF] ; immediate

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
[struct]-voc wordlist-id !

\ Interpretative Structuren                            30apr92py

: [defined] ( "<spaces>name" -- flag ) \ tools-ext bracket-defined
    \G returns true if name is found in current search order.  Immediate word.
    sp@ fp@ 2>r
    parse-name forth-recognize translate-nt?
    2r> rot >r fp! sp! r> ; immediate
' [defined] alias defined immediate
: [undefined] ( "<spaces>name" -- flag ) \ tools-ext bracket-undefined
    \G returns false if name is found in current search order.  Immediate word.
     postpone [defined] 0= ; immediate

: scanif ( -- )
    countif off endif? off  current-sourcepos3 >r >r >r
    BEGIN
	BEGIN
	    parse-name dup  WHILE  scanif-r execute
	    endif? @  UNTIL  rdrop rdrop rdrop  EXIT  THEN  2drop
	refill  WHILE
	endif? @  UNTIL  rdrop rdrop rdrop  EXIT  THEN
    r> r> r> source drop + 1 input-lexeme 2! loadline ! loadfilename# !
    s" unfinished [IF] at end of file" true ['] type ?warning
    endif? on ;

: [IF] ( flag -- ) \ tools-ext bracket-if
  \G If flag is @code{TRUE} do nothing (and therefore
  \G execute subsequent words as normal). If flag is @code{FALSE},
  \G parse and discard words from the parse
  \G area (refilling it if necessary using
  \G @code{REFILL}) including nested instances of @code{[IF]}..
  \G @code{[ELSE]}.. @code{[THEN]} and @code{[IF]}.. @code{[THEN]}
  \G until the balancing @code{[ELSE]} or @code{[THEN]} has been
  \G parsed and discarded. Immediate word.
    0= IF  scanif  THEN ;                               immediate

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
