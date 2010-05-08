\ Rewritten and improved bitmask code

\ Author: David Kühling <dvdkhlng AT gmx DOT de>
\ Created: May 2010

\ Copyright (C) 2010 Free Software Foundation, Inc.

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

: bitset?  ( x #bit -- flag )  \ get value of single bit in cell x
   rshift 1 and ;
: setbit  ( x1 0|1 #bit -- x2 )  \ set value of single bit in cell x
   1 over lshift invert -rot    \ mask for deleting bit
   lshift                       \ mask for optionally setting bit
   -rot and or ;                \ first delete then optionally set bit
: (bits/cell)  ( -- +n )  \ measure number of bits per cell
   1 1 begin  1 lshift dup while
	 swap 1+ swap
   repeat  drop ;
(bits/cell) CONSTANT bits/cell

: dispense  ( x1-val x2-mask -- x3-masked )
   \ encode val into the bits given by mask.  bits in mask can be spread out
   \ as much as you like.  for signed values 'val', first apply 'narrow' below
   bits/cell 0 do		\ iterate over bits in mask
      dup i bitset? if		\ if mask bit set:
	 over 1 and  i setbit		\ replace bit in mask by val's bit
	 swap 1 rshift swap		\ and remove bit from val
      then
   loop
   swap 0<> ABORT" dispense: value does not fit into masked bits" ;
: embed  ( x1-accu x2-val x3-mask -- x4-result )
   \ encode 'val' into bits set given by mask, replacing corresponding bits in
   \ 'accu'
   dup >r dispense         \ dispense value over masked bits
   swap r> invert and      \ delete corresponding bits in accu
   or ;                    \ and add dispensed bits

: mask ( +n -- mask )  \ get bitmask for lowest #n bits
   0 invert  swap lshift invert ;
: narrow  ( n1 n2 -- x )  \ limit signed value to n2 bits
   \ note: assumes 2-complement number n1 and 2-complement host
   2dup mask and -rot     \ compute masked value,
   1-                     \ but before returning, check whether no bits lost
   -1 over lshift              ( lower bund)
   1 rot lshift                ( upper bound)
   within 0= ABORT" narrow: signed value out of range" ;

: maskinto ( "x-mask" --  runtime:  x1-val x1-accu -- x2-masked )
   \ for backwards compatability with old bitmask code
    ]] swap [[ parse-word s>number drop ]]L embed [[ ; IMMEDIATE
