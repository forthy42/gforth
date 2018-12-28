\ test forward

\ Copyright (C) 2003,2004,2005,2006,2007,2009,2011,2015,2016,2017 Free Software Foundation, Inc.

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
t{ : forward1 285 ; -> }t
t{ : forward6 forward4 ; -> }t
\ simple-see forward6
t{ forward2 -> 285 }t
t{ forward3 -> 285 }t
t{ forward5 -> 285 }t
t{ forward6 -> 285 }t
