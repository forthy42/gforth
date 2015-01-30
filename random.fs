\ generates random numbers                             12jan94py

\ Copyright (C) 1995,2000,2003,2007,2013 Free Software Foundation, Inc.

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

0 [IF] \ pretty pad linear congruent generator
    Variable seed
    
    $10450405 Constant generator
    
    : rnd  ( -- n )  seed @ generator um* drop 1+ dup seed ! ;
    : seed-init ( -- )  ntime drop seed +!  rnd drop ;
[ELSE]
    \ hash based generator, deterministic
    \ seed-init feeds entropy into the generator
    \ 2.2 times slower than lcg on 64 bit platform
    \ passed SmallCrush, Rabbit, Alphabit, FIPS-140-2
    $10 buffer: rng-state
    8 buffer: seed

    : rnd ( -- n ) seed 7 0 rng-state hashkey2 rng-state 2@ xor ;
    cell 8 = [IF]
	: seed-init ( -- ) ntime drop seed ! rnd drop ;
    [ELSE]
	: seed-init ( -- ) ntime seed 2! rnd drop ;
    [THEN]
[THEN]
seed-init

: random ( n -- 0..n-1 )  rnd um* nip ;
