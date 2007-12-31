\ test gforth locals

\ Copyright (C) 1995,1996,1997,2000,2003,2007 Free Software Foundation, Inc.

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


require glocals.fs
require debugs.fs

: localsinfo \ !! only debugging
 ." stack: " .s ." locals-size: " locals-size ? ." locals-list"
 also locals words previous cr ;

." before foo" cr
: foo
{ c: a  b  c: c  d: d }
a .
b .
d type
c . cr
;

." before" .s cr
lp@ . cr
1 2 3 s" xxx" foo
lp@ . cr
." after" .s cr


." xxx" cr
.s cr
depth . cr


." testing part 2" cr

: xxxx
   [ ." starting xxxx" .s cr ]
{ f } f
if
 { a b }
 b a
[ ." before else" .s cr ]
else
[ ." after else" .s cr ]
 { c d }
 c d
then
[ ." locals-size after then:" locals-size @ . cr ]
~~ f ~~ drop
[ ." ending xxxx" .s cr ]
;

2 3 1 xxxx . . cr
2 3 0 xxxx . . cr
cr cr cr

: xxx3
begin
  { a }
until
a
;
." after xxx3" .s cr cr cr

: xxx2
[ ." start of xxx2" .s cr ]
begin
[ ." after begin" .s cr ]
  { a }
[ ." after { a }" .s cr ]
1 while
[ ." after while" .s cr ]
 { b }
 a b
[ ." after a" .s cr ]
repeat
[ ." after repeat" .s cr
  also locals words previous cr
]
a
[ ." end of xxx2" .s cr ]
;

: xxx4
[ ." before if" localsinfo ]
if
[ ." after if" localsinfo ]
{ a }
[ ." before begin" localsinfo ]
begin
[ ." after begin" localsinfo ]
[ 1 cs-roll ]
[ ." before then" localsinfo ]
then
{ b }
until
[ ." after until" localsinfo ]
;

: xxx5
{ a }
a drop    
ahead
assume-live
begin
[ ." after begin" localsinfo ]
a drop    
[ 1 cs-roll ]
then
[ ." after then" localsinfo ]
until
[ ." after until" localsinfo ]
;

." xxx6 coming up" cr
: xxx6
    [ ." starting xxx6" localsinfo ]
if
{ x }
else
[ ." after else" localsinfo ]
ahead
begin
[ ." after begin" localsinfo ]
[ 2 CS-ROLL ] then
[ ." after then" localsinfo ]
until
then
    [ ." ending xxx6" localsinfo ]
;

." xxx7 coming up" cr
: xxx7
{ b }
do
{ a }
[ ." before loop" localsinfo ]
loop
[ ." after loop" localsinfo ]
;

." xxx8 coming up" cr

: xxx8
{ b }
?do
{ a }
[ ." before loop" localsinfo ]
loop
[ ." after loop" localsinfo ]
;

." xxx9 coming up" cr
: xxx9
{ b }
do
{ c }
[ ." before ?leave" leave-sp ? leave-stack . cr ]
?leave
[ ." after ?leave" leave-sp ? cr ]
{ a }
[ ." before loop" localsinfo ]
loop
[ ." after loop" localsinfo ]
;

." strcmp coming up" cr
: strcmp { addr1 u1 addr2 u2 -- n }
 addr1 addr2 u1 u2 min 0 ?do
   { s1 s2 }
   s1 c@ s2 c@ - ?dup if
     unloop exit
   then
   s1 char+ s2 char+
 loop
 2drop
 u1 u2 - ;

: teststrcmp
." lp@:" lp@ . cr
s" xxx" s" yyy" strcmp . cr
." lp@:" lp@ . cr
s" xxx" s" xxx" strcmp . cr
." lp@:" lp@ . cr
s" xxx" s" xxxx" strcmp . cr
." lp@:" lp@ . cr
s" xxx3" s" xxx2" strcmp . cr
." lp@:" lp@ . cr
s" " s" " strcmp . cr
." lp@:" lp@ . cr
." lp@:" lp@ . cr
." stack:" .s cr
;

: findchar { c addr u -- i }
 addr u 0 ?do
   { p }
   p c@ c = if
     p leave
   then
   p char+
 loop
 addr - ;


: testfindchar
." findcahr " cr
." lp@:" lp@ . cr
[char] a s" xxx" findchar . cr
." lp@:" lp@ . cr
[char] a s" " findchar . cr
." lp@:" lp@ . cr
[char] a s" wam" findchar . cr
." lp@:" lp@ . cr
[char] a s" wma" findchar . cr
." lp@:" lp@ . cr
[char] a s" awam" findchar . cr
." lp@:" lp@ . cr
." stack:" .s cr
;



." stack:" .s cr
teststrcmp
testfindchar
." hey you" cr

: xxx10
[ ." before if" localsinfo ]
if
[ ." after if" localsinfo ]
scope
[ ." after scope" localsinfo ]
{ a }
[ ." before endscope" localsinfo ]
endscope
[ ." before begin" localsinfo ]
begin
[ ." after begin" localsinfo ]
[ 1 cs-roll ]
[ ." before then" localsinfo ]
then
{ b }
until
[ ." after until" localsinfo ]
;

: xxx11
    if
    { a }
    exit
    [ ." after xexit" localsinfo ]
    else
    { b }
    [ ." before xthen" localsinfo ]
    then
    [ ." after xthen" localsinfo ]
;

." strcmp1 coming up" cr
: strcmp1 { addr1 u1 addr2 u2 -- n }
 u1 u2 min 0 ?do
   addr1 c@ addr2 c@ - ?dup if
     unloop exit
   then
   addr1 char+ TO addr1
   addr2 char+ TO addr2
 loop
 u1 u2 - ;

: teststrcmp1
." lp@:" lp@ . cr
s" xxx" s" yyy" strcmp1 . cr
." lp@:" lp@ . cr
s" xxx" s" xxx" strcmp1 . cr
." lp@:" lp@ . cr
s" xxx" s" xxxx" strcmp1 . cr
." lp@:" lp@ . cr
s" xxx3" s" xxx2" strcmp1 . cr
." lp@:" lp@ . cr
s" " s" " strcmp1 . cr
." lp@:" lp@ . cr
." lp@:" lp@ . cr
." stack:" .s cr
;
teststrcmp1

." testing the abominable locals-ext wordset" cr
: puke locals| this read you can |
    you read this can ;

1 2 3 4 puke . . . . cr

\ just some other stuff

: life1 { b0 b1 b23 old -- new }
    b23 invert old b1 b0 xor and old invert b1 and b0 and or and ;

: life2 { b0 b1 b23 old -- new }
    b0 b1 or old b0 xor b1 xor b23 or invert and ;

$5555 $3333 $0f0f $00ff life1 .
$5555 $3333 $0f0f $00ff life2 .
.s
cr

: test
    1 { a }  ." after }" cr
    2 { b -- }  ." after --" cr
;
test
.s cr

bye
