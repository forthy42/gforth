\ header-methods.fs does the intelligent compile, vtable handling

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2012,2013,2014,2015,2016,2018,2019,2021,2023,2024 Free Software Foundation, Inc.

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

defer call-check ( xt -- xt ) ' noop is call-check
defer does-check ( xt -- xt ) ' noop is does-check

: value, >body ['] lit peephole-compile, , ['] @ peephole-compile, ;
: constant, >body @ lit, ;
: :, ( xt -- ) call-check >body ['] call peephole-compile, , ;
: variable, >body lit, ;
: user, >body @ ['] up@ peephole-compile, ['] lit+ peephole-compile, , ;
: defer, >body lit, postpone perform  ;
: field+, >body @ lit, postpone + ;
: abi-code, >body ['] abi-call peephole-compile, , ;
: ;abi-code, ['] ;abi-code-exec peephole-compile, , ;
: does, ( xt -- ) does-check dup >body lit, >extra @ compile, ;
: umethod, >body cell+ 2@ ['] u#exec peephole-compile, , , ;
: uvar, >body cell+ 2@ ['] u#+ peephole-compile, , , ;
\ : :loc, >body ['] call-loc peephole-compile, , ;

: (uv) ( ip -- xt-addr ) 2@ next-task + @ cell- @ swap cells + ;
:noname cell+ (uv) ;
fold1: cell+ lit, postpone (uv) ;
defer-table to-class: is-umethod ( method-xt -- ) \ gforth-internal

AVariable hm-list
