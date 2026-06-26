\ generates random numbers                             12jan94py

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 1995,2000,2003,2007,2013,2017,2019,2020 Free Software Foundation, Inc.

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

0 [IF] \ pretty bad linear congruent generator
    Variable seed
    
    $10450405 Constant generator
    
    : rnd  ( -- x )  seed @ generator um* drop 1+ dup seed ! ;
    : seed-init ( -- )  ntime drop seed +!  rnd drop ;
    : seed! ( x -- )  seed ! ;
[ELSE]
    \ hash based generator, deterministic
    \ seed-init feeds entropy into the generator
    \ 2.2 times slower than lcg on 64 bit platform
    \ passed SmallCrush, Rabbit, Alphabit, FIPS-140-2
    user rng-state $10 cell- uallot drop
    user seed $8 cell- uallot drop

    : rnd ( -- x ) \ gforth
	\G @i{x} is a random number, with all bits randomly set.
	seed 8 0 rng-state hashkey2 rng-state 2@ xor ;
    cell 8 = [IF]
	: seed-init ( -- ) ntime drop seed ! rnd drop ;
    [ELSE]
	: seed-init ( -- ) ntime seed 2! rnd drop ;
    [THEN]
    : seed! ( x -- ) \ gforth
	\G Set seed for @word{rnd} and @word{random} to a specific
	\G value, resulting in a deterministic sequence.
	seed $8 erase rng-state $10 erase seed ! rnd drop ;
[THEN]
seed-init
:noname defers 'cold seed-init ; is 'cold

: random ( u1 -- u2 ) \ gforth
    \g @i{U2} is a random number 0<=@i{u2}<@i{u1}.
    rnd um* nip ;
