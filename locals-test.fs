include glocals.fs

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
{ f } f
xif
  { a b }
  b a
[ ." before else" .s cr ]
xelse
[ ." after else" .s cr ]
  { c d }
  c d
xthen
[ ." locals-size after then:" locals-size @ . cr ]
f drop
;

2 3 1 xxxx . . cr
2 3 0 xxxx . . cr
cr cr cr

: xxx3
xbegin
  { a }
xuntil
a
;
." after xxx3" .s cr cr cr

: xxx2
[ ." start of xxx2" .s cr ]
xbegin
[ ." after begin" .s cr ]
  { a }
[ ." after { a }" .s cr ]
1 xwhile
[ ." after while" .s cr ]
  { b }
  a b
[ ." after a" .s cr ]
xrepeat
[ ." after repeat" .s cr
  also locals words previous cr
]
a
[ ." end of xxx2" .s cr ]
;

: xxx4
[ ." before if" localsinfo ]
xif
[ ." after if" localsinfo ]
{ a }
[ ." before begin" localsinfo ]
xbegin
[ ." after begin" localsinfo ]
[ 1 cs-roll ]
[ ." before then" localsinfo ]
xthen
{ b }
xuntil
[ ." after until" localsinfo ]
;

: xxx5
{ a }
xahead
xbegin
[ ." after begin" localsinfo ]
[ 1 cs-roll ]
xthen
[ ." after then" localsinfo ]
xuntil
[ ." after until" localsinfo ]
;

: xxx6
xif
{ x }
xelse
[ ." after else" localsinfo ]
xahead
xbegin
[ ." after begin" localsinfo ]
[ 2 CS-ROLL ] xthen
[ ." after then" localsinfo ]
xuntil
;

." xxx7 coming up" cr
: xxx7
{ b }
xdo
{ a }
[ ." before loop" localsinfo ]
xloop
[ ." after loop" localsinfo ]
;

." xxx8 coming up" cr

: xxx8
{ b }
x?do
{ a }
[ ." before loop" localsinfo ]
xloop
[ ." after loop" localsinfo ]
;

." xxx9 coming up" cr
: xxx9
{ b }
xdo
{ c }
[ ." before ?leave" leave-sp ? leave-stack . cr ]
x?leave
[ ." after ?leave" leave-sp ? cr ]
{ a }
[ ." before loop" localsinfo ]
xloop
[ ." after loop" localsinfo ]
;

." strcmp coming up" cr
: strcmp { addr1 u1 addr2 u2 -- n }
 addr1 addr2 u1 u2 min 0 x?do
   { s1 s2 }
   s1 c@ s2 c@ - ?dup xif
     unloop xexit
   xthen
   s1 char+ s2 char+
 xloop
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
 addr u 0 x?do
   { p }
   p c@ c = xif
     p xleave
   xthen
   p char+
 xloop
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
xif
[ ." after if" localsinfo ]
scope
[ ." after scope" localsinfo ]
{ a }
[ ." before endscope" localsinfo ]
endscope
[ ." before begin" localsinfo ]
xbegin
[ ." after begin" localsinfo ]
[ 1 cs-roll ]
[ ." before then" localsinfo ]
xthen
{ b }
xuntil
[ ." after until" localsinfo ]
;

