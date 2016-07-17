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

\ anpassen: in-dictionary? one-head? head? dictionary-end allot
\ Deal with MARKERs and the native code thingies

0
field: section-start \ during run-time
field: section-end
field: section-dp
field: section-name \ nt, for named sections
constant section-desc

uvalue sections \ address base of descriptor table (grows in both dirs)
user #sections
user current-section \ index
user #extra-sections \ counts up, but the extra sections are below SECTIONS

256 1024 * value section-size

s" at first section" exception constant first-section-error
s" extra sections have no previous or next section" exception
constant extra-section-error

: section-addr ( i -- addr )
    section-desc * sections + ;

: current-section-addr ( -- addr )
    current-section @ section-addr ;

: hex.r ( u1 u2 -- )
    ['] .r #16 base-execute ;

: .sections ( -- )
    cr ."             start              end               dp "
    sections hex. 
    #sections @ #extra-sections @ negate +do
        cr i current-section @ = if '>' else bl then emit
        i section-desc * sections +
        dup section-start @ #16 hex.r
        dup section-end   @ #17 hex.r
        dup section-dp    @ #17 hex.r
        space section-name @ id.
    loop ;

: init-section ( section size -- )
    \ initialize section descriptor 
    swap >r
    dup allocate throw
    dup r@ section-start !
    dup r@ section-dp !
    + r@ section-end !
    [ ' noname >name ]L r> section-name ! ;

: new-section ( -- )
    sections #extra-sections @ section-desc * -
    #sections @ #extra-sections @ + section-desc * section-desc extend-mem drop
    #extra-sections @ section-desc * + to sections
    section-size init-section
    1 #sections +! ;

: set-section ( -- )
    \ any changes to other things after changing the section
    current-section-addr section-dp dpp ! ;

:noname ( -- )
    0 current-section ! set-section ;
is reset-dpp

: next-section ( -- )
    \ switch to the next section, creating it if necessary
    current-section @ 0< extra-section-error and throw 
    1 current-section +!
    current-section @ #sections @ = if
	new-section
    then
    assert( current-section @ #sections @ < )
    set-section ;

: previous-section ( -- )
    \ switch to previous section
    current-section @ 0< extra-section-error and throw
    current-section @ 0= first-section-error and throw
    -1 current-section +! set-section ;

\ extra sections

: add-extra-section ( -- section )
    sections #extra-sections @ section-desc * -
    #sections @ #extra-sections @ + {: #total-sections :}
    #total-sections 1+ section-desc * resize throw {: sections-base :}
    sections-base dup section-desc + #total-sections section-desc * move
    1 #extra-sections +!
    sections-base #extra-sections @ section-desc * + to sections
    set-section sections-base ;

: extra-section ( size "name" -- )
    add-extra-section dup rot init-section
    create #extra-sections @ negate ,
    latest swap section-name !
  does> ( xt -- )
    \ execute xt with the current section being in the extra section
    current-section @ {: old-section :} try
         ( xt addr ) @ current-section ! set-section execute 0
    restore
        old-section current-section ! set-section endtry
    throw ;
    
\ initialization

: sections-ude ( -- addr )
    current-section-addr section-end @ ;

: init-sections ( -- )
    section-desc allocate throw to sections
    0 current-section !
    1 #sections !
    0 #extra-sections !
    forthstart            sections section-start !
    usable-dictionary-end sections section-end !
    here                  sections section-dp !
    [ ' noname >name ]L   sections section-name !
    sections section-dp dpp ! \ !! dpp is reset to normal-dp on throw
    ['] sections-ude is usable-dictionary-end ;

:noname ( -- )
    init-sections
    defers 'cold ;
is 'cold

init-sections

\ savesystem

: dump-fi ( c-addr u -- )
    prepare-for-dump
    0 current-section ! set-section
    maxalign here { sect0-here }
    #sections @ 1 u+do
	i section-addr >r
	r@ section-start @ assert( dup dup maxaligned = )
	r@ section-dp @ maxaligned dup r> section-dp !
	over - save-mem-dict 2drop loop
    here forthstart - forthstart 2 cells + !
    here normal-dp ! 
    w/o bin create-file throw >r
    preamble-start here over - r@ write-file throw
    sect0-here sections section-dp !
    sections #sections @ section-desc * r@ write-file throw
    .sections cr
    #sections 1 cells r@ write-file throw
    r> close-file throw ;

[defined] testing [if]
section-size extra-section bla
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
