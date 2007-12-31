\ Multitasker                                          19aug94py

\ Copyright (C) 1995,1996,1997,2001,2003,2007 Free Software Foundation, Inc.

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

Create sleepers  sleepers A, sleepers A, 0 ,

: link-task ( task1 task2 -- )
    \G LINK-TASK links task1 into the task chain of task2
    over 2@  2dup cell+ ! swap !  \ unlink task1
    2dup @ cell+ !  2dup dup @ rot 2!  ! ;

: sleep ( task -- )
    \G deactivates task
    sleepers  link-task ;
: wake  ( task -- )
    \G activates task
    next-task link-task ;

: pause ( -- )
    \G PAUSE is the task-switcher
    rp@ fp@ lp@ sp@ save-task !
    next-task @ up! save-task @ sp!
    lp! fp! rp! ;

: stop ( -- )
    \G STOP sleeps a task and switches to the next
    rp@ fp@ lp@ sp@ save-task !
    next-task @ up! save-task @ sp!
    lp! fp! rp! prev-task @ sleep ;

:noname    ' >body @ ;
:noname    ' >body @ postpone literal ; 
interpret/compile: user' ( 'user' -- n )
\G USER' computes the task offset of a user variable

: NewTask ( stacksize -- Task )  dup 2* 2* udp @ + dup
    \G NEWTASK creates a new, sleeping task
    allocate throw  + >r
    r@ over - udp @ - next-task over udp @ move
    r> over user' rp0 + ! dup >r
    dup r@ user' lp0   + ! over -
    dup r@ user' fp0   + ! over -
    dup r@ user' sp0   + ! over -
    dup r@ user' normal-dp + dup >r !
    r> r@ user' dpp  + ! 2drop
    0 r@ user' current-input + !
    r> dup 2dup 2! dup sleep ;

Create killer killer A, killer A,
: kill ( task -- )
    \G kills a task - deactivate and free task area
    dup killer link-task  killer dup dup 2!
    user' normal-dp + @ free throw ;

: kill-task ( -- )
    \G kills the current task, also on bottom of return stack of a new task
    next-task @ up! save-task @ sp!
    lp! fp! rp! prev-task @ kill ;

: (pass) ( x1 .. xn n task -- )  rdrop
  [ ' kill-task >body ] ALiteral r>
  rot >r r@ user' rp0 + @ 2 cells - dup >r 2!
  r>              swap 1+
  r@ user' fp0 + @ swap 1+
  r@ user' lp0 + @ swap 1+
  cells r@ user' sp0 + @ tuck swap - dup r@ user' save-task + !
  ?DO  I !  cell  +LOOP  r> wake ;

: activate ( task -- )
    \G activates the task.
    \G Continues execution with the caller of ACTIVATE.
    0 swap (pass) ;
: pass ( x1 .. xn n task -- )
    \G passes n parameters to the task and activates that task.
    \G Continues execution with the caller of PASS.
    (pass) ;

: single-tasking? ( -- flag )
    \G checks if only one task is running
    next-task dup @ = ;

: task-key   BEGIN  pause key? single-tasking? or  UNTIL  (key) ;
: task-emit  (emit) pause ;
: task-type  (type) pause ;

' task-key  IS key
' task-emit IS emit
' task-type IS type
