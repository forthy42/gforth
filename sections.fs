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

0
field: section-start \ during run-time
field: section-end
field: section-dp
field: section-relocated \ section-start after relocation/savesystem
constant section-header

0 value sections
variable #sections
variable current-section \ index

section-header allocate throw to sections
0 current-section !
1 #sections !

forthstart            sections section-start !
usable-dictionary-end sections section-end !
here                  sections section-dp !
sections section-dp dpp !

256 1024 * value section-size

: new-section ( -- )
    sections #sections @ section-header extend-mem drop to sections >r
    section-size allocate throw ( section-addr )
    dup r@ section-start !
    dup r@ section-dp !
    dup section-size + r> section-end !
    1 #sections +! ;

: set-dp ( -- )
    sections current-section @ th section-dp dpp ! ;

: next-section ( -- )
    \ switch to the next section, creating it if necessary
    1 current-section +!
    current-section @ #sections @ = if
	new-section
    then
    assert( current-section @ #sections @ < )
    set-dp ;

: previous-section ( -- )
    \ switch to previous section
    assert( current-section @ 0> )
    -1 current-section +! set-dp ;
