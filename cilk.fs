\ Cilk-like sync+spawn interface

\ Author: Bernd Paysan
\ Copyright (C) 2016,2019,2020,2022 Free Software Foundation, Inc.

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
    s" sysctl -n hw.ncpu" r/o open-pipe throw slurp-fid s>number drop
[ELSE] e? os-type s" linux" search nip nip [IF]
	s" /sys/devices/system/cpu/present" slurp-file over >r
	#lf -scan '-' $split 2nip s>number drop 1+ r> free throw
    [ELSE]
	1 \ we don't know
    [THEN]
[THEN]
1 max Value cores

Variable sync#
Variable workers
User invoker

: worker@ ( -- worker )
    BEGIN  workers $@ IF @ workers 0 cell $del ELSE drop 0 THEN
    dup 0= WHILE drop stop REPEAT ;

event: ->sync ( task -- )
    { w^ task } task cell workers $+! -1 sync# +! ;
: +worker ( task -- )
    <event up@ elit, ->sync event> ;

event: ->spawn ( xt task -- )
    invoker ! execute clearstack ;
: worker-thread ( -- ) \ cilk
    stacksize4 newtask4 activate [ up@ ]l invoker !
    BEGIN  invoker @ +worker stop  AGAIN ;

: cilk-sync ( -- ) \ cilk
    \G wait for all spawned tasks to complete
    BEGIN  sync# @  0> WHILE  stop  REPEAT ;
: start-workers cores 1 max 0 ?DO worker-thread 1 sync# +! LOOP cilk-sync ;
: cilk-init ( -- ) \ cilk
    \G Start the workers if not already done
    workers @ 0= IF  start-workers  THEN ;

: spawn-rest ( xt -- )
    elit, up@ elit, ->spawn worker@ event> 1 sync# +! ;
: spawn ( xt -- ) \ cilk
    \G wait for a worker to become free, and spawn xt there
    <event spawn-rest  ;
: spawn1 ( n xt -- ) \ cilk
    \G wait for a worker to become free, and spawn xt there, with one argument
    <event swap elit, spawn-rest ;
: spawn2 ( n1 n2 xt -- ) \ cilk
    \G wait for a worker to become free, and spawn xt there, with two arguments
    <event >r swap elit, elit, r> spawn-rest ;
: spawn-closure ( xt -- ) \ cilk
    \G for passing a heap closure: execute and free it.  This allows to pass
    \G all sorts of arguments to code.
    \G Usage @code{[@{: vars :@}h code ;] spawn-closure ;}
    [: dup >r execute r> >addr free throw ;] spawn1 ;

: cilk-bye ( -- ) \ cilk
    \G kill all workers
    cilk-sync workers $@len cell/ 0 ?DO [: 0 (bye) ;] spawn LOOP
    #10000. ns workers $free  sync# off ;

s" GFORTH_IGNLIB" getenv s" true" str= 0= [IF]
    :noname ( -- )
	 cilk-bye defers bye ; is bye
[THEN]
