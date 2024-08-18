\ Cilk-like sync+spawn interface

\ Author: Bernd Paysan
\ Copyright (C) 2016,2019,2020,2022,2023 Free Software Foundation, Inc.

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

require unix/pthread.fs

e? os-type 2dup s" darwin" string-prefix? -rot s" openbsd" string-prefix? or [IF]
    s" sysctl -n hw.ncpu" r/o open-pipe throw slurp-fid over >r
    s>number drop
    r> free throw
[ELSE] e? os-type s" linux" search nip nip [IF]
	s" /sys/devices/system/cpu/present" slurp-file over >r
	#lf -scan '-' $split 2nip
	s>number drop 1+
	r> free throw
    [ELSE]
	1 \ we don't know
    [THEN]
[THEN]
1 max Value cores ( -- u ) \ cilk
\G A value containing the number of worker tasks to use.  By default
\G this is the number of hardware threads (with SMT/HT), if we can
\G determine that, otherwise 1.  If you want to use a different
\G number, change @code{cores} before calling @code{cilk-init}.

User sync#
Variable workers
User invoker
semaphore workers-sema

: worker@ ( -- worker )
    BEGIN
	[: workers $@ IF @ workers 0 cell $del ELSE drop 0 THEN ;]
	workers-sema c-section
    dup 0= WHILE drop stop REPEAT ;

: +worker ( task -- )
    up@ [{: w^ task :}h1
	task cell [: workers $+! ;] workers-sema c-section
	-1 sync# +! ;] swap send-event ;

: worker-thread ( invoker -- )
    1 stacksize4 newtask4 pass invoker !
    BEGIN  invoker @ +worker stop  AGAIN ;

: cilk-sync ( -- ) \ cilk
    \G Wait for all subproblems to complete.
    BEGIN  sync# @  0> WHILE  stop  REPEAT ;
: start-workers cores 1 max 0 ?DO up@ worker-thread 1 sync# +! LOOP cilk-sync ;
: cilk-init ( -- ) \ cilk
    \G Start the worker tasks if not already done.
    workers @ 0= IF  start-workers  THEN ;

: spawn ( xt -- ) \ cilk
    \G Execute @i{xt} ( -- ) in a worker task.
    \G Use one-time executable closures to pass heap-allocated closures,
    \G allowing to pass arbitrary data from the spawner to the code
    \G running in the worker.@*
    \G E.g.: @code{( n r ) [@{: n f: r :@}h1 code ;] spawn}
    up@ [{: xt: xt task :}h1 task invoker ! xt ;]
    worker@ send-event 1 sync# +! ;
: spawn1 ( x xt -- ) \ cilk
    \G Execute @i{xt} ( x -- ) in a worker task.
    [{: x xt: xt :}h1 x xt ;] spawn ;
: spawn2 ( x1 x2 xt -- ) \ cilk
    \G Execute @i{xt} ( x1 x2 -- ) in a worker task.
    [{: d: x xt: xt :}h1 x xt ;] spawn ;

: cilk-bye ( -- ) \ cilk
    \G Terminate all workers.
    cilk-sync workers $@len cell/ 0 ?DO [: 0 (bye) ;] spawn LOOP
    #10000. ns workers $free  sync# off ;

s" GFORTH_IGNLIB" getenv s" true" str= 0= [IF]
    :noname ( -- )
	cilk-bye defers bye ; is bye
[THEN]
