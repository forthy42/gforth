\ Multitasker                                          19aug94py

\ Copyright (C) 1995-1997 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

Create sleepers  sleepers A, sleepers A, 0 ,

\ LINK-TASK links task1 into the task chain of task2
: link-task ( task1 task2 -- )
  over 2@  2dup cell+ ! swap !  \ unlink task1
  2dup @ cell+ !  2dup dup @ rot 2!  ! ;

: sleep ( task -- )  sleepers  link-task ;
: wake  ( task -- )  next-task link-task ;

\ PAUSE is the task-switcher
: pause ( -- )
  rp@ fp@ lp@ sp@ save-task !
  next-task @ up! save-task @ sp!
  lp! fp! rp! ;

\ STOP sleeps a task and switches to the next
: stop ( -- )
  rp@ fp@ lp@ sp@ save-task !
  next-task @ up! save-task @ sp!
  lp! fp! rp! prev-task @ sleep ;

\ USER' computes the task offset
: user' ( 'user' -- n )
    ' >body @ postpone literal ; immediate
interpretation:
    ' >body @ ;

\ NEWTASK creates a new, sleeping task
: NewTask ( n -- Task )  dup 2* 2* udp @ + dup
  allocate throw  + >r
  r@ over - udp @ - next-task over udp @ move
  r> over user' r0 + ! dup >r
  dup r@ user' l0   + ! over -
  dup r@ user' f0   + ! over -
  dup r@ user' s0   + ! over -
  dup r@ user' normal-dp + dup >r !
   r> r@ user' dpp  + ! + $10 +
      r@ user' >tib + !
  r> dup 2dup 2! dup sleep ;

: kill-task
  next-task @ up! save-task @ sp!
  lp! fp! rp! prev-task @ dup dup link-task user' normal-dp + @ free throw ;

: (pass) ( x1 .. xn n task -- )  rdrop
  [ ' kill-task >body ] ALiteral r>
  rot >r r@ user' r0 + @ 2 cells - dup >r 2!
  r>              swap 1+
  r@ user' f0 + @ swap 1+
  r@ user' l0 + @ swap 1+
  cells r@ user' s0 + @ tuck swap - dup r@ user' save-task + !
  ?DO  I !  cell  +LOOP  r> wake ;

: activate ( task -- )  0 swap (pass) ;
: pass ( x1 .. xn n task -- )  (pass) ;

: single-tasking? ( -- flag )
    next-task dup @ = ;

: task-key   BEGIN  pause key? single-tasking? or  UNTIL  (key) ;
: task-emit  (emit) pause ;
: task-type  (type) pause ;

' task-key  IS key
' task-emit IS emit
' task-type IS type
