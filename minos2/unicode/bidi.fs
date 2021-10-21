\ bidi file

\ Authors: Bernd Paysan
\ Copyright (C) 2021 Free Software Foundation, Inc.

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

Variable firstindex
2Variable lastindex
: bidi: ( -- )
    Create
  DOES> ( index <tag> -- )
    dup lastindex @ tuck <> and IF
	lastindex cell+ @ 1+ firstindex @ tuck - swap hex. hex.
	." .." lastindex @ name>string type cr
	over firstindex !
    THEN  lastindex 2! ;

: included-pipe ( addr u -- )
    r/o open-pipe throw ['] read-loop execute-parsing-file ;

Vocabulary bidis also bidis definitions

bidi: L
bidi: AL
bidi: AN
bidi: B
bidi: BN
bidi: CS
bidi: EN
bidi: ES
bidi: ET
bidi: FSI
bidi: LRE
bidi: LRI
bidi: LRO
bidi: NSM
bidi: ON
bidi: PDF
bidi: PDI
bidi: R
bidi: RLE
bidi: RLI
bidi: RLO
bidi: S
bidi: WS

next-arg s" input" replaces
hex
s" cut -f1,5 -d';' %input% | tr ';' ' '" $substitute drop included-pipe
