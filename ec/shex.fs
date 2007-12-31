\ shex.fs Output Routines for Motorola S-Records		16jul97jaw

\ Copyright (C) 1998,2000,2003,2006,2007 Free Software Foundation, Inc.

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

\ this is a extention to cross.fs to save motorola s-records
\ the first part is generic to output s-records from and to any
\ destination
\ the second part is for the cross compiler

unlock >CROSS

decimal

\ only method to get information
defer @byte ' c@ IS @byte

\ only method to output
defer htype ' type IS htype

: hemit pad c! pad 1 htype ;
: hcr #lf hemit ;

: .## ( c -- ) 	     base @ swap hex s>d <# # # #> htype base ! ;
\ generic checksum support

variable csum
: csum+ ( c -- c )   dup csum +! ;
: .b  ( c -- ) 	     csum+ .## ;
: .w  ( w -- ) 	     dup 8 rshift .b 255 and .b ;
: .csum ( -- )	     csum @ 255 xor 255 and .b ;

2 constant adrlen
1 constant csumlen
32 constant maxline

: .smem ( destadr adr len type -- )
  'S hemit hemit 0 csum !
  dup adrlen + csumlen + .b
  rot .w
  bounds ?DO I @byte .b LOOP
  .csum hcr ;

: 3dup >r 2dup r@ -rot r> ;

: .sregion ( destadr adr len -- )
  BEGIN dup
  WHILE	3dup maxline min dup >r
	'1 .smem r@ /string rot r> + -rot
  REPEAT drop 2drop ;

: .startaddr ( adr -- )
  'S hemit '9 hemit 0 csum !
  adrlen csumlen + .b
  .w .csum hcr ;

\ specific for cross-compiler

0 value fd
: (htype) fd write-file throw ;
' (htype) IS htype

: tc@ X c@ ;
' tc@ IS @byte

variable start-addr

: save-region-shex ( adr len -- )
    bl parse w/o create-file throw to fd
\ PSC1000 trick:
\  'E hemit
\  2dup over swap 200 min .sregion
    0 0 0 '0 .smem
    over swap .sregion 
    start-addr @ .startaddr
    fd close-file throw ;

>MINIMAL

: cpu-start start-addr ! ;
: save-region-shex save-region-shex ;

>CROSS

lock
