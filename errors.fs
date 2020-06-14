\ Load in error strings

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook, Jens Wilke
\ Copyright (C) 1995,1996,1997,1998,1999,2003,2007,2013,2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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
-59 s" ALLOCATE" rot errstring
-60 s" FREE" rot errstring
-61 s" RESIZE" rot errstring
-62 s" CLOSE-FILE" rot errstring
-63 s" CREATE-FILE" rot errstring
-64 s" DELETE-FILE" rot errstring
-65 s" FILE-POSITION" rot errstring
-66 s" FILE-SIZE" rot errstring
-67 s" FILE-STATUS" rot errstring
-68 s" FLUSH-FILE" rot errstring
-69 s" OPEN-FILE" rot errstring
-70 s" READ-FILE" rot errstring
-71 s" READ-LINE" rot errstring
-72 s" RENAME-FILE" rot errstring
-73 s" REPOSITION-FILE" rot errstring
-74 s" RESIZE-FILE" rot errstring
-75 s" WRITE-FILE" rot errstring
-76 s" WRITE-LINE" rot errstring
-77 s" Malformed xchar" rot errstring
-78 s" SUBSTITUTE" rot errstring
-79 s" REPLACES" rot errstring

\ Gforth's errors:

variable next-exception -2048 next-exception !

: exception ( addr u -- n ) \ exception- gforth
    \G @var{n} is a previously unused @code{throw} value in the range
    \G (-4095...-256). Consecutive calls to @code{exception} return
    \G consecutive decreasing numbers. Gforth uses the string
    \G @var{addr u} as an error message.
    next-exception @ errstring
    next-exception @
    -1 next-exception +! ;

: exceptions ( xt num -- n ) \ exceptions gforth
    \G Use @var{xt} to convert errors into strings. The error starting with
    \G @var{n} and lower are converted to [0..num[ when calling @var{xt}
    align ErrRanges @ here ErrRanges ! ,
    negate tuck next-exception @ 1+ dup , + , ,
    next-exception +!@ ;

s" Cannot tick compile-only word (try COMP' ... DROP)" exception drop
s" Write to broken pipe" exception constant broken-pipe-error ( -- n ) \ gforth
\G the error number for a broken pipe
s" Double free error" exception drop
s" Data before memory block was modified" exception drop
s" Data after memory block was modified" exception drop
s" Can't tick literal" exception drop
s" Warning treated as error" exception constant warning-error ( -- n )
s" Can't defer@ from this xt" exception drop
s" Can't ADDR on locals/uvalues" exception drop
s" wrong file type" exception drop
s" locals stack overflow" exception drop
s" locals stack underflow" exception drop
s" Bug in Gforth, please report" exception
>r : never-happens ( -- ) [ r> ] Literal
    \ you can use this when you have to provide an xt that is never reached
    throw ;
