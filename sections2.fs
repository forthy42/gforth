\ Sections (part 2) for the dictionary (like sections in assembly language)

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2016,2018,2019,2020,2021,2024 Free Software Foundation, Inc.

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

: reverse-sections-execute ( xt -- )
    >r sections $@ cell mem-do
        j i @ section-execute
    loop
    rdrop ;

: .sections ( -- ) \ gforth dot-sections
    \G Show all the sections and their status.
    cr ."             start      size      used name"
    current-section @
    [:  cr dup current-section @ = if '>' else bl then emit
	section-start @ #16 hex.r
	section-size  @ #10 dec.r
	section-dp    @ section-start @ - #10 dec.r space
	section-name @ id. ;] reverse-sections-execute  drop ;
