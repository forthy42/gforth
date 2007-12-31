\ Load in error strings

\ Copyright (C) 1995,1996,1997,1998,1999,2003,2007 Free Software Foundation, Inc.

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

: linked ( addr -- )
    here over @ a, swap ! ;

: errstring ( addr u n -- )
    ErrLink linked
    ,
    string, align ;

decimal

-1 s" Aborted" rot errstring
-3 s" Stack overflow" rot errstring
-4 s" Stack underflow" rot errstring
-5 s" Return stack overflow" rot errstring
-6 s" Return stack underflow" rot errstring
-7 s" Do-loops nested too deeply" rot errstring
-8 s" Dictionary overflow" rot errstring
-9 s" Invalid memory address" rot errstring
-10 s" Division by zero" rot errstring
-11 s" Result out of range" rot errstring
-12 s" Argument type mismatch" rot errstring
-13 s" Undefined word" rot errstring
-14 s" Interpreting a compile-only word" rot errstring
-15 s" Invalid FORGET" rot errstring
-16 s" Attempt to use zero-length string as a name" rot errstring
-17 s" Pictured numeric ouput string overflow" rot errstring
-18 s" Parsed string overflow" rot errstring
-19 s" Word name too long" rot errstring
-20 s" Write to a read-only location" rot errstring
-21 s" Unsupported operation" rot errstring
-22 s" Control structure mismatch" rot errstring
-23 s" Address alignment exception" rot errstring
-24 s" Invalid numeric argument" rot errstring
-25 s" Return stack imbalance" rot errstring
-26 s" Loop parameters unavailable" rot errstring
-27 s" Invalid recursion" rot errstring
-28 s" User interrupt" rot errstring
-29 s" Compiler nesting" rot errstring
-30 s" Obsolescent feature" rot errstring
-31 s" >BODY used on non-CREATEd definition" rot errstring
-32 s" Invalid name argument" rot errstring
-33 s" Block read exception" rot errstring
-34 s" Block write exception" rot errstring
-35 s" Invalid block number" rot errstring
-36 s" Invalid file position" rot errstring
-37 s" File I/O exception" rot errstring
-38 s" Non-existent file" rot errstring
-39 s" Unexpected end of file" rot errstring
-40 s" Invalid BASE for floating point conversion" rot errstring
-41 s" Loss of precision" rot errstring
-42 s" Floating-point divide by zero" rot errstring
-43 s" Floating-point result out of range" rot errstring
-44 s" Floating-point stack overflow" rot errstring
-45 s" Floating-point stack underflow" rot errstring
-46 s" Floating-point invalid argument" rot errstring
-47 s" Compilation word list deleted" rot errstring
-48 s" invalid POSTPONE" rot errstring
-49 s" Search-order overflow" rot errstring
-50 s" Search-order underflow" rot errstring
-51 s" Compilation word list changed" rot errstring
-52 s" Control-flow stack overflow" rot errstring
-53 s" Exception stack overflow" rot errstring
-54 s" Floating-point underflow" rot errstring
-55 s" Floating-point unidentified fault" rot errstring
-56 s" QUIT" rot errstring
-57 s" Error in sending or receiving a character" rot errstring
-58 s" [IF], [ELSE], [THEN] error" rot errstring

\ Gforth's errors:

-2048 s" Cannot tick compile-only word (try COMP' ... DROP)" rot errstring
-2049 s" Write to broken pipe" rot errstring

variable next-exception -2050 next-exception !

: exception ( addr u -- n ) \ exception- gforth
    \G @var{n} is a previously unused @code{throw} value in the range
    \G (-4095...-256). Consecutive calls to @code{exception} return
    \G consecutive decreasing numbers. Gforth uses the string
    \G @var{addr u} as an error message.
    next-exception @ errstring
    next-exception @
    -1 next-exception +! ;

-2049 constant broken-pipe-error ( -- n ) \ gforth
\G the error number for a broken pipe

