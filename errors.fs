\ Load in error strings

\ Copyright (C) 1995 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

: linked        here over @ a, swap ! ;

: ERR" ( n -- )
       ErrLink linked
       ,
       [char] " parse
       string, align ;

decimal

-1 ERR" Aborted"
-3 ERR" Stack overflow"
-4 ERR" Stack underflow"
-5 ERR" Return stack overflow"
-6 ERR" Return stack undeflow"
-7 ERR" Do-loops nested too deeply"
-8 ERR" Dictionary overflow"
-9 ERR" Invalid memory address"
-10 ERR" Division by zero"
-11 ERR" Result out of range"
-12 ERR" Argument type mismatch"
-13 ERR" Undefined word"
-14 ERR" Interpreting a compile-only word"
-15 ERR" Invalid FORGET"
-16 ERR" Attempt to use zero-length string as a name"
-17 ERR" Pictured numeric ouput string overflow"
-18 ERR" Parsed string overflow"
-19 ERR" Word name too long"
-20 ERR" Write to a read-only location"
-21 ERR" Unsupported operation"
-22 ERR" Control structure mismatch"
-23 ERR" Address alignment exception"
-24 ERR" Invalid numeric argument"
-25 ERR" Return stack imbalance"
-26 ERR" Loop parameters unavailable"
-27 ERR" Invalid recursion"
-28 ERR" User interrupt"
-29 ERR" Compiler nesting"
-30 ERR" Obsolescent feature"
-31 ERR" >BODY used on non-CREATEd definition"
-32 ERR" Invalid name argument"
-33 ERR" Block read exception"
-34 ERR" Block write exception"
-35 ERR" Invalid block number"
-36 ERR" Invalid file position"
-37 ERR" File I/O exception"
-38 ERR" Non-existent file"
-39 ERR" Unexpected end of file"
-40 ERR" Invalid BASE for floating point conversion"
-41 ERR" Loss of precision"
-42 ERR" Floating-point divide by zero"
-43 ERR" Floating-point result out of range"
-44 ERR" Floating-point stack overflow"
-45 ERR" Floating-point stack underflow"
-46 ERR" Floating-point invalid argument"
-47 ERR" Compilation word list deleted"
-48 ERR" invalid POSTPONE"
-49 ERR" Search-order overflow"
-50 ERR" Search-order underflow"
-51 ERR" Compilation word list changed"
-52 ERR" Control-flow stack overflow"
-53 ERR" Exception stack overflow"
-54 ERR" Floating-point underflow"
-55 ERR" Floating-point unidentified fault"
-56 ERR" QUIT"
-57 ERR" Error in sending or receiving a character"
-58 ERR" [IF], [ELSE], [THEN] error"
