\ Sections for the dictionary (like sections in assembly language)

\ Copyright (C) 2016 Free Software Foundation, Inc.

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

\ Deal with MARKERs and the native code thingies

0
field: section-start \ during run-time
field: section-end
field: section-dp
field: section-relocated \ section-start after relocation/savesystem
constant section-desc

0 value sections
variable #sections
variable current-section \ index

section-desc allocate throw to sections
0 current-section !
1 #sections !

forthstart            sections section-start !
usable-dictionary-end sections section-end !
here                  sections section-dp !
sections section-dp dpp !

256 1024 * value section-size

: hex.r ( u1 u2 -- )
    ['] .r #16 base-execute ;

: .sections ( -- )
    cr sections #16 hex.r ."  start              end               dp"
    
    #sections @ 0 u+do
        cr i current-section @ = if '>' else bl then emit
        sections i section-desc * +
        dup section-start @ #21 hex.r
        dup section-end   @ #17 hex.r
        section-dp        @ #17 hex.r
    loop ;

: current-section-addr ( -- addr )
    sections current-section @ section-desc * + ;

:noname ( -- addr )
    current-section-addr section-end @ ;
is usable-dictionary-end

: new-section ( -- )
    sections #sections @ section-desc * section-desc extend-mem drop to sections >r
    section-size allocate throw ( section-addr )
    dup r@ section-start !
    dup r@ section-dp !
    dup section-size + r> section-end !
    1 #sections +! ;

: set-section ( -- )
    \ any changes to other things after changing the section
    current-section-addr section-dp dpp ! ;

: next-section ( -- )
    \ switch to the next section, creating it if necessary
    1 current-section +!
    current-section @ #sections @ = if
	new-section
    then
    assert( current-section @ #sections @ < )
    set-section ;

: previous-section ( -- )
    \ switch to previous section
    assert( current-section @ 0> )
    -1 current-section +! set-section ;


.sections
cr next-section .sections
cr next-section .sections
cr previous-section .sections
cr previous-section .sections
cr
        
        