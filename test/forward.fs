\ test forward

\ Author: Anton Ertl
\ Copyright (C) 2003,2004,2005,2006,2007,2009,2011,2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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

\ require ../forward.fs
require ./ttester.fs
decimal

t{ forward forward1 -> }t
t{ : forward2 forward1 ; -> }t
t{ : forward3 forward1 ; -> }t
t{ : forward4 postpone forward1 ; immediate -> }t
t{ : forward5 forward4 ; -> }t
t{ ' forward1 constant forward7 -> }T
t{ defer forward8 -> }t
t{ ' forward1 is forward8 -> }t
t{ : forward9 ['] forward1 compile, ; immediate -> }t
t{ : forwarda forward9 ; -> }t
t{ : forward1 285 ; -> }t
t{ : forward6 forward4 ; -> }t
\ simple-see forward6
t{ forward2 -> 285 }t
t{ forward3 -> 285 }t
t{ forward5 -> 285 }t
t{ forward6 -> 285 }t
t{ : x execute ; forward7 x forward7 x -> 285 285 }t
t{ forward8 forward8 -> 285 285 }t
t{ : forwardb forward9 ; -> }t
t{ forwarda forwarda -> 285 285 }t
t{ forwardb forwardb -> 285 285 }t

