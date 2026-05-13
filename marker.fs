\ marker                                               18dec94py

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook, Jens Wilke
\ Copyright (C) 1995,1998,2000,2003,2005,2007,2009,2010,2011,2013,2014,2016,2017,2018,2019,2020,2021,2023,2024 Free Software Foundation, Inc.

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


\ Marker creates a mark that is removed (including everything 
\ defined afterwards) when executing the mark.

Defer marker, \ compile markers, chained
Defer marker! \ restore markers, chained
Defer marker>here \ set here

: @+ ( addr1 -- addr2 x )
    dup cell+ swap @ ;

: included-files-mark ( -- u )
    included-files $@len ;

\ hmm, most of the saving appears to be pretty unnecessary: we could
\ derive the wordlists and the words that have to be kept from the
\ saved value of dp value. - anton

:is marker, ( -- mark )
    here align
    action-of marker! ,
    action-of marker, ,
    action-of marker>here ,
    included-files-mark ,
    dup A, \ here
    voclink @ A, \ vocabulary list start
    \ for all wordlists, remember wordlist-id (the linked list)
    voclink
    BEGIN
	@ dup
    WHILE
	dup 0 wordlist-link - wordlist-id @ A,
    REPEAT
    drop
    \ remember udp
    udp @ ,
    \ remember hm-list
    hm-list @ ,
    \ remember dyncode-ptr
    here ['] noop , cell "\x80" drop "\x00" drop compile-prims
;

:is marker! ( mark-addr -- here mark-addr' )
    \ reset included files count; resize will happen on next add-included-file
    @+ is marker!
    @+ is marker,
    @+ is marker>here
    dup @ dup >r included-files $@ r> /string cell MEM+DO  I $free  LOOP
    included-files $!len cell+
    \ rest of marker!
    @+ swap ( here rest-of-marker )
    @+ voclink !
    \ restore wordlists to former words
    voclink
    BEGIN
	@ dup 
    WHILE
	over @ over 0 wordlist-link - wordlist-id !
	cell under+
    REPEAT
    drop
    \ rehash wordlists to remove forgotten words
    \ why don't we do this in a single step? - anton
    voclink
    BEGIN
	@ dup
    WHILE
	dup 0 wordlist-link - initwl
    REPEAT
    drop
    \ restore udp and dp
    dup @ udp !
    cell+ dup @ hm-list !
    cell+ [IFDEF] forget-dyncode dup forget-dyncode3 drop [then]
    cell+ ;

: marker ( "<spaces> name" -- ) \ core-ext
    \G Create a definition, @i{name} (called a @i{mark}) whose
    \G execution semantics are to remove itself and everything 
    \G defined after it.
    marker, Create A,
DOES> ( -- )
    @ aligned dup perform drop marker>here ;

\ sections-marker:
\ #extra-sections
\ size of sections
\ section-dps (as many as given by size)

:is marker, ( -- )
    defers marker,
    current-section @ ,
    #extra-sections @ ,
    sections $@ dup , cell mem+do
	['] section-dp i @ section-execute @ ,
	['] lastnt     i @ section-execute @ ,
    loop ;

:is marker! ( addr1 -- addr2 )
    defers marker!
    @+ current-section ! set-section
    #extra-sections @ over @ ?do
	sections back> free throw loop
    @+ #extra-sections !
    sections $@len over @ ?do
	sections stack> free throw
    cell +loop
    assert( sections $@len over @ = )
    cell+ sections $@ cell mem+do
	@+ ['] section-dp i @ section-execute !
	@+ ['] lastnt     i @ section-execute !
    loop ;

\ finally set here and check for things depending on here

:is marker>here ( here -- )
    ->here
    \ clean up vocabulary stack
    0 ['] search-order >body $@ cell MEM+DO
	I @ dup here u>
	IF  drop  ELSE  swap 1+  THEN
    LOOP
    dup 0= or set-order \ -1 set-order if order is empty
    get-current here > IF
	forth-wordlist set-current
    THEN ;
