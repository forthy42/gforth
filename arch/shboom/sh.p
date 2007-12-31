\ Copyright (C) 1997,2003,2004,2007 Free Software Foundation, Inc.

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
\ along with this program; if not, see http://www.gnu.org/licenses/.

fpath= ./|../gec/
s" arch/shboom/mach.fs"
Create mach-file here over 1+ allot place
include ec/builttag.fs
include cross.fs
include ec/shex.fs

\ load compiler extentions
unlock >CROSS
include arch/shboom/compiler.fs

$180 $8000 over - ( start len ) region dictionary
setup-target

>ENVIRON

false SetValue compiler
true SetValue interpreter

lock

prims-include

: cell M 24 ;

: branch  ( -- ) 
 r> dup @ + >r ;   

: ?branch  ( f -- )
 0= dup     \ !f !f
 r> dup @   \ !f !f IP branchoffset
 rot and +  \ !f IP|IP+branchoffset
 swap 0= cell and + \ IP''
 >r ;
 
: skip1
  r> cell+ noop noop noop >r ; 

: (emit)
  $A0300014
  .quad4 dup @ M 90 M 20
  and M 17 M FF M FF
  drop $A0300000 ! ;

\ : (emit) $A0300014 BEGIN dup @ $20 and UNTIL drop $A0300000 ! ;

: (type)
  BEGIN dup  WHILE
        >r dup c@ (emit) 1+ r> 1-
  REPEAT 2drop ;

: (key)
  $A0300014
  .quad4 dup @ M 90 M 01
  and M 17 M FF M FF
  drop $A0300000 @ $7f and ;

: key? $A0300014 @ 01 and 0= 0= ;

\ noninline versions:
\ : :dovar   ( '2 (emit ) r> cell+ ; isdoer
\ : :docon ( '( (emit ) noop noop r> cell+ @ ; isdoer
\ : :douser  ( '5 (emit ) noop r> cell+ @ up@ + ; isdoer

: :dovar _inline 7 add_pc, ; isdoer
: :docon _inline 7 add_pc, @ ; isdoer
: :douser _inline 7 add_pc, @ up@ + ; isdoer

: :dodoes  ( '4 (emit ) r> dup cell+ swap @ execute ; isdoer
\ .quad3 push.l up@ +
: :dodefer ( '6 (emit ) noop r> cell+ @ execute ; isdoer


'1 constant #1
include ec/dotx.fs

1 [IF]      
undef-words
include kernel/prim.fs
all-words
include kernel/vars.fs
include kernel/basics.fs
include kernel/io.fs
include kernel/nio.fs
[THEN]

variable test2
create ctest 'A c, 'B c, 'C c, 'D c,
create test$ ," Hallo dies ist ein Test!"

\ : c@ @ $ff and ;

\ : test  '* $A0000000 ! ;
: looptest '9 .quad4 BEGIN dup (emit) 1- dup '0 = UNTIL (emit) '; (emit) ;
: looptest3 BEGIN looptest AGAIN ;
: looptest2 	'A (emit) 'B (emit)
		'0 skip1 1+ noop noop noop 1+ noop noop noop 1+ noop noop noop
  		1+ noop noop noop (emit) 
  		'1 1 xor (emit) '3 $fe and (emit) '4 $01 or (emit)
  		'5 0 0= + (emit) '6 1 0= + (emit)
  		'0 '0 = dup 'Y and swap 0= 'N and or (emit) 
  		'0 1237 = dup 'Y and swap 0= 'N and or (emit) 
  		'; dup (emit) (emit) 
  		;

has? interpreter 0= [IF]
: boot  \ '. (emit) (emit) (emit)
        '. (emit)
        test$ count (type)
\        ." Hallo dies ist ein Test!!!" cr cr 
        ." Hallo" cr
	$5123 .x $9831 .x
	looptest test$ count (type)
	'. dup (emit) (emit)
	BEGIN test$ count (type) AGAIN
\  	'1 (emit) '2 (emit) 
\	ctest dup c@ (emit) 1+ dup c@ (emit) 1+ dup c@ (emit) 1+ c@ (emit)
\ 	test2 test$ test$ 1+ dup c@ (emit) 1+ dup c@ (emit) 
\	test$ count (type) ;
	;
[THEN]

has? interpreter [IF]
include kernel/saccept.fs
include kernel/errore.fs
\ include kernel/interp.fs
include kernel/int.fs
has? compiler [IF]
include kernel/comp.fs
include kernel/cond-old.fs    \ load IF and co w/o locals
include kernel/toolsext.fs
[THEN]
include kernel/doers.fs
include kernel/version.fs
include kernel/tools.fs               \ load tools ( .s dump )

[THEN]

\ include /devel/src/forth/bench/8queens.fs
include arch/misc/tt.fs
include fib.fs
include ../jeans/ec/bench/benchrd.fs

: test1 10 0 DO I . I 5 = IF LEAVE THEN LOOP ;

create tibbuf 100 allot

include arch/shboom/dis2.fs

include kernel/special.fs

: boot
\D 1 'B (emit)
  main-task up!
  rp@ rp0 !
  sp@ sp0 !
  tibbuf dup >tib ! tibstack ! #tib off >in off
  BEGIN
        ['] cold catch DoError
  AGAIN ;
[THEN]

\ Initialization

>ram
here normal-dp !
unlock tudp @
lock
udp !

unlock tlast @
lock
1 cells - dup forth-wordlist ! last !

' boot cpu-start

$180 here $180 - save-region-shex sh.s3

unlock >MINIMAL
: l 	s" cat sh.s3 >/dev/cua6" system ;
: g	s" echo G >/dev/cua6" system ;
lock

.unresolved
unlock
.regions
