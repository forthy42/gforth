\ Cilk-like sync+spawn interface

\ Copyright (C) 2016 Free Software Foundation, Inc.

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

s" grep siblings /proc/cpuinfo | head -1 | cut -f2 -d:" r/o open-pipe throw
include-file Value cores

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
: worker-thread ( -- )
    stacksize4 newtask4 activate [ up@ ]l invoker !
    BEGIN  invoker @ +worker stop  AGAIN ;

cores 1 max 0 [DO] worker-thread [LOOP] 1 ms sync# off

: sync ( -- )
    \G wait for all spawned tasks to complete
    BEGIN  sync# @  0> WHILE  stop  REPEAT ;
: spawn-rest ( xt -- )
    elit, up@ elit, ->spawn worker@ event> 1 sync# +! ;
: spawn ( xt -- )
    \G wait for a worker to become free, and spawn xt there
    <event spawn-rest  ;
: spawn1 ( n xt -- )
    \G wait for a worker to become free, and spawn xt there, with one argument
    <event swap elit, spawn-rest ;
: spawn2 ( n1 n2 xt -- )
    \G wait for a worker to become free, and spawn xt there, with two arguments
    <event >r swap elit, elit, r> spawn-rest ;

0 warnings !@
: bye ( -- )
    sync cores 0 ?DO worker@ kill LOOP 1 ms bye ;
warnings !