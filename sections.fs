\ Sections for the dictionary (like sections in assembly language)

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2016,2018 Free Software Foundation, Inc.

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

\ !! ToDo: better integration with the rest of the system, in
\ particular the definitions or usages of FORTHSTART,
\ USABLE-DICTIONARY-END, DPP; and the usage for locals

\ anpassen: in-dictionary? one-head? head? dictionary-end allot
\ Deal with MARKERs and the native code thingies

$Variable sections   \ section stack (grows in both directions)
user #extra-sections \ hidden extra sections not part of the next/prev
		     \ section stack

256 1024 * value section-defaultsize

s" at first section" exception constant first-section-error
s" extra sections have no previous or next section" exception
constant extra-section-error

: set-section ( -- )
    \ any changes to other things after changing the section
    section-dp dpp !
    [IFDEF] check-dp  section-dp to check-dp [THEN] ;

: section-execute ( xt section -- )
    \ execute xt with the current section being in the extra section
    current-section @ {: old-section :} try
         ( xt section ) current-section ! set-section execute 0
    restore
        old-section current-section ! set-section endtry
    throw ;

: sections-execute {: xt -- :}
    sections $@ bounds u+do
	xt i @ section-execute
    cell +loop ;

:noname ( ip-addr -- view / 0 )
    0 swap
    [n:l defers addr>view dup 0= select ;] sections-execute ;
is addr>view

:noname ( x -- f )
    0 swap [n:l in-dictionary1? or ;] sections-execute ;
is in-dictionary?

: .sections ( -- )
    cr ."             start      size               dp name"
    current-section @
    [:  cr dup current-section @ = if '>' else bl then emit
	section-start @ #16 hex.r
	section-size  @ #10 hex.r
	section-dp    @ #17 hex.r space
	section-name @ id. ;] sections-execute  drop ;

: create-section ( size -- section )
    current-section @ >r
    dup allocate throw dup current-section !
    dup section-start !
    section-desc + section-dp !
    section-size !
    ``noname section-name !
    locs[] $saved
    current-section @ r> current-section ! ;

: new-section ( -- )
    section-defaultsize create-section sections >stack ;

:noname ( -- )
    forthstart current-section ! set-section ;
is reset-dpp

: section# ( -- n )
    0 sections $@ bounds u+do
	I @ current-section @ = IF
	    unloop  EXIT  THEN
	1+
    cell +loop  drop -1 ;

: #>section ( n -- )
    cells >r sections $@ r> safe/string IF
	@ current-section ! set-section
    ELSE
	drop
    THEN ;

: next-section ( -- )
    \ switch to the next section, creating it if necessary
    section# dup #extra-sections @ < extra-section-error and throw
    1+ dup sections stack# = IF  new-section  THEN
    #>section ;

: previous-section ( -- )
    \ switch to previous section
    section#
    dup #extra-sections @ < extra-section-error and throw
    dup #extra-sections @ = first-section-error and throw
    1- #>section ;

\ extra sections

: >extra-sections ( section -- )
    sections >back 1 #extra-sections +! ;

: extra-section ( size "name" -- )
    create-section dup >extra-sections
    create dup ,
    latest [: section-name ! ;] rot section-execute
  does> ( xt -- ) @ section-execute ;
    
\ initialization

``forth section-name !
forthstart sections >stack

\ savesystem

:noname ( fid -- )
    [: section-name @ ``forth <> IF
	    section-start 2@ maxaligned 2 pick write-file throw
	THEN ;] sections-execute  drop
; is dump-sections

[defined] test-it [if] 
section-defaultsize extra-section bla
cr .sections
:noname 50 allot ; bla
.sections
cr next-section .sections
cr next-section .sections
cr previous-section .sections
cr previous-section .sections

next-section
: foo ." foo" ;
previous-section
: bar ." bar" ;
cr .sections
cr
\ s" xxxsections" dump-fi
[then]
