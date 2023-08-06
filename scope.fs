\ scope for structures and objects

\ Authors: Bernd Paysan
\ Copyright (C) 2022 Free Software Foundation, Inc.

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

$Variable scope<>
: >scope ( xt -- )
    get-current scope<> >stack also execute definitions ;
: scope{ ( "vocabulary" -- scope:addr )
    ' >scope ;
: }scope ( scope:addr -- )
    previous scope<> stack> set-current ;
: scope: ( "vocabulary" -- scope:addr )
    vocabulary latestxt >scope ;
: cs-scope: ( "vocabulary" -- scope:addr )
    cs-vocabulary latestxt >scope ;

: struct{ ( "scope" -- vars )
    cs-scope: s" sizeof" nextname begin-structure ;
: }struct ( vars -- )
    end-structure }scope ;

[IFDEF] class
    : class{ ( parent "scope" -- methods vars )
	class cs-scope: ;
    : }class ( methods vars -- )
	s" class" nextname end-class }scope ;
[THEN]
