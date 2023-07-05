\ user friendly interface to generate to-actions

\ Authors: Bernd Paysan
\ Copyright (C) 2023 Free Software Foundation, Inc.

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

: to+addr: ( xt table "name" -- ) \ gforth
    \G create a to-method with ADDR enabled, where
    \G @var{xt} creates the address to access the field,
    \G and @var{table} contains the operators to store to it.
    Create , ,
    [: >r r@ cell+ perform r> @ to-!exec ;] set-does>
    [: >r lits# 0= IF  r> does,  EXIT  THEN
    r@ cell+ @ compile, r> @ to-!, ;] set-optimizer ;

: to: ( xt table "name" -- ) \ gforth
    \G create a to-method with ADDR disabled, where
    \G @var{xt} creates the address to access the field,
    \G and @var{table} contains the operators to store to it.
    Create , ,
    [: !!?addr!! >r r@ cell+ perform r> @ to-!exec ;] set-does>
    [: !!?addr!! >r lits# 0= IF  r> does,  EXIT  THEN
    r@ cell+ @ compile, r> @ to-!, ;] set-optimizer ;
