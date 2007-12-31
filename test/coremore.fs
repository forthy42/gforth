\ test some core stuff not tested by John Hayes' suite

\ Copyright (C) 2006,2007 Free Software Foundation, Inc.

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

\ uses some non-core and non-standard words

require ./tester.fs
decimal

\ division
environment-wordlist >order
{ 1 1 dnegate 2 ' sm/rem catch 0= -> -1 max-n invert true }

{ 1 1 -2 ' sm/rem catch 0= -> 1 max-n invert true }

{ max-u max-n 2/ max-n invert ' fm/mod catch -> -1 max-n invert 0 }
{ max-u max-n 2/ max-n invert ' sm/rem catch -> max-n max-n negate 0 }

{ 0 max-n 2/ 1+ max-n invert ' fm/mod catch -> 0 max-n invert 0 }
{ 0 max-n 2/ 1+ max-n invert ' sm/rem catch -> 0 max-n invert 0 }

{ 1 max-n 2/ 1+ max-n invert ' sm/rem catch 0= -> 1 max-n invert true }

{ 0 max-u -1. d+ max-u ' um/mod catch 0= -> max-u 1- max-u true }
