\ test libcc.fs C interface

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2007,2017,2019 Free Software Foundation, Inc.

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

require libcc.fs

c-library libcc
\c #include <string.h>
\c #include <stdlib.h>

c-function strlen strlen a -- n
c-function labs labs n -- n

\c #define _FILE_OFFSET_BITS 64
\c #include <sys/types.h>
\c #include <unistd.h>
c-function dlseek lseek n d n -- d

\c #include <stdio.h>
c-function sprintf-nr sprintf a a n r -- n
c-function sprintf-rn sprintf a a r n -- n

\c #define sprintfull(a,s,ull) sprintf(a,s,(unsigned long long)ull)
c-function sprintfull sprintfull a a n -- n

\c #define sprintfll(a,s,ll) sprintf(a,s,(long long)ll)
c-function sprintfll sprintfll a a n -- n

\ test calling a C function pointer from Forth

\ first create a C function pointer:

\c typedef long (* func1)(long);
\c #define labsptr(dummy) ((func1)labs)
c-function labsptr labsptr -- a

\ now the call
\c #define call_func1(par1,fptr) ((func1)fptr)(par1)
c-function call_func1 call_func1 n func -- n

c-callback test-callback: u u -- u
c-funptr test-call {((Cell(*)(Cell,Cell))(ptr))} u u -- u
end-c-library

require ./tester.fs
decimal

t{ pad s\" n=%d r=%f\n\0" drop -5 -0.5e sprintf-nr pad swap s\" n=-5 r=-0.500000\n" str= -> true }t
t{ pad s\" r=%f n=%d\n\0" drop -0.5e -5 sprintf-rn pad swap s\" r=-0.500000 n=-5\n" str= -> true }t
t{ pad s\" ll=%lld\n\0" drop -1 sprintfll pad swap s\" ll=-1\n" str= -> true }t
cell 4 = [IF]
    t{ pad s\" ll=%d r=%f\n\0" drop -1 -0.5e sprintf-nr pad swap s\" ll=-1 r=-0.500000\n" str= -> true }t
    t{ pad s\" ull=%llu\n\0" drop -1 sprintfull pad swap s\" ull=18446744073709551615\n" str= -> true }t
    t{ pad s\" ull=%u r=%f\n\0" drop -1 -0.5e sprintf-nr pad swap s\" ull=4294967295 r=-0.500000\n" str= -> true }t
[ELSE]
    t{ pad s\" ll=%lld r=%f\n\0" drop -1 -0.5e sprintf-nr pad swap s\" ll=-1 r=-0.500000\n" str= -> true }t
    t{ pad s\" ull=%llu\n\0" drop -1 sprintfull pad swap s\" ull=18446744073709551615\n" str= -> true }t
    t{ pad s\" ull=%llu r=%f\n\0" drop -1 -0.5e sprintf-nr pad swap s\" ull=18446744073709551615 r=-0.500000\n" str= -> true }t
[THEN]

t{ s\" fooo\0" drop strlen -> 4 }t
t{ -5 labs -> 5 }t
t{ -5 labsptr call_func1 -> 5 }t

' + test-callback: constant plus

t{ 1 2 plus test-call -> 3 }t
