\ Mini-OOF extension: current object in user variable  08jan13py

\ Authors: Bernd Paysan
\ Copyright (C) 2014,2015,2019 Free Software Foundation, Inc.

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

Variable class-o

: user-o ( "name" -- )
    0 uallot class-o !  User ;
: uval-o ( "name" -- )
    0 uallot class-o !  UValue ;

: umethod ( m v -- m' v )
    postpone nocov[
    over >r : postpone u#exec class-o @ , r> cell/ , postpone ;
    cell under+
    ['] umethod, set-optimizer
    ['] is-umethod set-to
    postpone ]nocov ;

: uvar ( m v size -- m v' )
    postpone nocov[
    over >r : postpone u#+ class-o @ , r> , postpone ; +
    ['] uvar, set-optimizer
    postpone ]nocov ;

: uclass ( c "name" -- c m v )
    ' >body @ class-o ! dup cell- cell- 2@ ;
