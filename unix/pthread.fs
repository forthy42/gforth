\ posix threads

\ Copyright (C) 2012 Free Software Foundation, Inc.

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

c-library pthread
    \c #include <pthread.h>
    \c #include <limits.h>
    \c #include <sys/mman.h>
    \c #include <unistd.h>
    \c #include <setjmp.h>
    \c #include <stdio.h>
    \c #include <signal.h>
    \c #define wholepage(n) (((n)+pagesize-1)&~(pagesize-1))
    \c
    \c void gforth_cleanup_thread(void * t)
    \c {
    \c #if HAVE_GETPAGESIZE
    \c   Cell pagesize=getpagesize(); /* Linux/GNU libc offers this */
    \c #elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
    \c   Cell pagesize=sysconf(_SC_PAGESIZE); /* POSIX.4 */
    \c #elif PAGESIZE
    \c   Cell pagesize=PAGESIZE; /* in limits.h according to Gallmeister's POSIX.4 book */
    \c #endif
    \c   Cell size = wholepage((Cell)(((user_area*)t)->lp0)+pagesize-(Cell)t);
    \c #ifdef SIGSTKSZ
    \c   size += 2*SIGSTKSZ;
    \c #endif
    \c   munmap(t, size);
    \c }
    \c
    \c #ifndef HAS_BACKLINK
    \c static void *(*saved_gforth_pointers)(Cell);
    \c #endif
    \c 
    \c void *gforth_thread(user_area * t)
    \c {
    \c   void *x;
    \c   int throw_code;
    \c   jmp_buf throw_jmp_buf;
    \c #ifndef HAS_BACKLINK
    \c   void *(*gforth_pointers)(Cell) = saved_gforth_pointers;
    \c #endif
    \c   Cell signal_data_stack[24];
    \c   Cell signal_return_stack[16];
    \c   Float signal_fp_stack[1];
    \c   void *ip0=(void*)(t->save_task);
    \c   Cell *sp0=(Cell*)(t->sp0)-1;
    \c   Cell *rp0=(Cell*)(t->rp0);
    \c   Float *fp0=(Float*)(t->fp0);
    \c   void *lp0=(void*)(t->lp0);
    \c
    \c   pthread_cleanup_push(&gforth_cleanup_thread, (void*)t);
    \c 
    \c   throw_jmp_handler = &throw_jmp_buf;
    \c   ((Cell*)(t->sp0))[-1]=(Cell)t;
    \c 
    \c   while((throw_code=setjmp(*throw_jmp_handler))) {
    \c     signal_data_stack[15]=throw_code;
    \c     ip0=(void*)(t->throw_entry);
    \c     sp0=signal_data_stack+15;
    \c     rp0=signal_return_stack+16;
    \c     fp0=signal_fp_stack;
    \c   }
    \c   x=gforth_engine(ip0, sp0, rp0, fp0, lp0);
    \c   pthread_cleanup_pop(1);
    \c   pthread_exit(x);
    \c }
    \c #ifdef HAS_BACKLINK
    \c void *gforth_thread_p()
    \c {
    \c   return (void*)&gforth_thread;
    \c }
    \c #else
    \c #define gforth_thread_p() gforth_thread_ptr(gforth_pointers)
    \c void *gforth_thread_ptr(GFORTH_ARGS)
    \c {
    \c   saved_gforth_pointers=gforth_pointers;
    \c   return (void*)&gforth_thread;
    \c }
    \c #endif
    \c void *pthread_plus(void * thread)
    \c {
    \c   return thread+sizeof(pthread_t);
    \c }
    \c Cell pthreads(Cell thread)
    \c {
    \c   return thread*(int)sizeof(pthread_t);
    \c }
    \c void *pthread_mutex_plus(void * thread)
    \c {
    \c   return thread+sizeof(pthread_mutex_t);
    \c }
    \c Cell pthread_mutexes(Cell thread)
    \c {
    \c   return thread*(int)sizeof(pthread_mutex_t);
    \c }
    \c void *pthread_cond_plus(void * thread)
    \c {
    \c   return thread+sizeof(pthread_cond_t);
    \c }
    \c Cell pthread_conds(Cell thread)
    \c {
    \c   return thread*(int)sizeof(pthread_cond_t);
    \c }
    \c pthread_attr_t * pthread_detach_attr(void)
    \c {
    \c   static pthread_attr_t attr;
    \c   pthread_attr_init(&attr);
    \c   pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED);
    \c   return &attr;
    \c }
    \c void create_pipe(FILE ** addr)
    \c {
    \c   int epipe[2];
    \c   pipe(epipe);
    \c   addr[0]=fdopen(epipe[0], "r");
    \c   addr[1]=fdopen(epipe[1], "a");
    \c   setvbuf(addr[1], NULL, _IONBF, 0);
    \c }
    \c #include <sys/ioctl.h>
    \c #include <errno.h>
    \c int check_read(FILE * fid)
    \c {
    \c   int pipe = fileno(fid);
    \c   int chars_avail;
    \c   int result = ioctl(pipe, FIONREAD, &chars_avail);
    \c   return (result==-1) ? -errno : chars_avail;
    \c }
    \c #include <poll.h>
    \c int wait_read(FILE * fid, Cell timeout)
    \c {
    \c   struct pollfd fds = { fileno(fid), POLLIN, 0 };
    \c #ifdef linux
    \c   struct timespec tout = { timeout/1000000000, timeout%1000000000 };
    \c   ppoll(&fds, 1, &tout, 0);
    \c #else
    \c   poll(&fds, 1, timeout/1000000);
    \c #endif
    \c   return check_read(fid);
    \c }
    c-function pthread+ pthread_plus a -- a ( addr -- addr' )
    c-function pthreads pthreads n -- n ( n -- n' )
    c-function thread_start gforth_thread_p -- a ( -- addr )
    c-function gforth_create_thread gforth_stacks n n n n -- a ( dsize rsize fsize lsize -- task )
    c-function pthread_create pthread_create a a a a -- n ( thread attr start arg )
    c-function pthread_exit pthread_exit a -- void ( retaddr -- )
    c-function pthread_mutex_init pthread_mutex_init a a -- n ( mutex addr -- r )
    c-function pthread_mutex_lock pthread_mutex_lock a -- n ( mutex -- r )
    c-function pthread_mutex_unlock pthread_mutex_unlock a -- n ( mutex -- r )
    c-function pthread-mutex+ pthread_mutex_plus a -- a ( mutex -- mutex' )
    c-function pthread-mutexes pthread_mutexes n -- n ( n -- n' )
    c-function pthread-cond+ pthread_cond_plus a -- a ( cond -- cond' )
    c-function pthread-conds pthread_conds n -- n ( n -- n' )
    c-function pause sched_yield -- void ( -- )
    c-function pthread_detatch_attr pthread_detach_attr -- a ( -- addr )
    c-function pthread_cond_signal pthread_cond_signal a -- n ( cond -- r )
    c-function pthread_cond_broadcast pthread_cond_broadcast a -- n ( cond -- r )
    c-function pthread_cond_wait pthread_cond_wait a a -- n ( cond mutex -- r )
    c-function pthread_cond_timedwait pthread_cond_timedwait a a a -- n ( cond mutex abstime -- r )
    c-function create_pipe create_pipe a -- void ( pipefd[2] -- )
    c-function check_read check_read a -- n ( pipefd -- n )
    c-function wait_read wait_read a n -- n ( pipefd timeout -- n )
end-c-library

User pthread-id  -1 cells pthread+ uallot drop
User epiper
User epipew

epiper create_pipe \ create pipe for main task

:noname    ' >body @ ;
:noname    ' >body @ postpone literal ; 
interpret/compile: user' ( 'user' -- n )
\G USER' computes the task offset of a user variable

: >task ( user task -- user' )  + next-task - ;

: kill-task ( -- )
    epiper @ close-file drop   epipew @ close-file drop  0 (bye) ;

:noname ( -- )
    [ here throw-entry ! ]
    handler @ ?dup-0=-IF
	>stderr cr ." uncaught thread exception: " .error cr
	kill-task
    THEN
    (throw1) ; drop

: NewTask4 ( dsize rsize fsize lsize -- task )
    gforth_create_thread >r
    throw-entry r@ udp @ throw-entry next-task - /string move
    word-pno-size chars dup allocate throw dup holdbufptr r@ >task !
    + dup holdptr r@ >task !  holdend r@ >task !
    epiper r@ >task create_pipe
    ['] kill-task >body  rp0 r@ >task @ 1 cells - dup rp0 r@ >task ! !
    handler r@ >task off
    r> ;

: NewTask ( stacksize -- task )  dup 2dup NewTask4 ;

: (activate) ( task -- )
    r> swap >r  save-task r@ >task !
    pthread-id r@ >task pthread_detatch_attr thread_start r> pthread_create drop ; compile-only

: activate ( task -- )
    ]] (activate) up! [[ ; immediate compile-only

: (pass) ( x1 .. xn n task -- )
    r> swap >r  save-task r@ >task !
    1+ dup cells negate  sp0 r@ >task @ -rot  sp0 r@ >task +!
    sp0 r@ >task @ swap 0 ?DO  tuck ! cell+  LOOP  drop
    pthread-id r@ >task 0 thread_start r> pthread_create drop ; compile-only

: pass ( x1 .. xn n task -- )
    ]] (pass) up! sp0 ! [[ ; immediate compile-only

: sema ( "name" -- ) \ gforth
    \G create a named semaphore
    Create here 1 pthread-mutexes allot 0 pthread_mutex_init drop ;

: cond ( "name" -- ) \ gforth
    \G create a named condition
    Create here 1 pthread-conds dup allot erase ;

: lock ( addr -- )  pthread_mutex_lock drop ;
: unlock ( addr -- )  pthread_mutex_unlock drop ;

: stacksize ( -- n ) forthstart 4 cells + @
    sp0 @ $FFF and -$1000 or + ;
: stacksize4 ( -- dsize rsize fsize lsize )
    forthstart 4 cells + 4 cells bounds DO  I @  cell +LOOP
    2swap swap sp0 @ $FFF and -$1000 or + swap 2swap
    swap       fp0 @ $FFF and -$1000 or + swap ;

\ event handling

s" Undefined event"   exception Constant !!event!!
s" Event buffer full" exception Constant !!ebuffull!!

Variable event#  1 event# !

User eventbuf# $100 uallot drop \ 256 bytes buffer for atomic event squences
-1 eventbuf# !

: <event  eventbuf# off ;
: 'event ( -- addr )  eventbuf# dup @ + cell+ ;
: event+ ( n -- addr )
    dup eventbuf# @ + $100 u>= !!ebuffull!! and throw
    'event swap eventbuf# +! ;
: event> ( task -- )  >r eventbuf# cell+ eventbuf# @ -1 eventbuf# !
    epipew r> >task @ write-file throw ;

: event-crash  !!event!! throw ;

Create event-table $100 0 [DO] ' event-crash , [LOOP]

: event-does ( task/ -- )  DOES>  @ eventbuf# @ 0< dup >r IF  <event  THEN
    'event c! 1 eventbuf# +!  r> IF  event>  THEN ;
: event:  Create event# @ ,  event-does
    here 0 , >r  noname : lastxt dup event# @ cells event-table + !
    r> ! 1 event# +! ;
: (stop) ( -- )  epiper @ key-file cells event-table + perform ;
: event? ( -- flag )  epiper @ check_read 0> ;
: ?events ( -- )  BEGIN  event?  WHILE  (stop)  REPEAT ;
: stop ( -- )  (stop) ?events ;
: stop-ns ( timeout -- ) epiper @ swap wait_read 0> IF  stop  THEN ;
: event-loop ( -- )  BEGIN  stop  AGAIN ;

event: ->lit  0  sp@ cell  epiper @ read-file throw drop ;
event: ->flit 0e fp@ float epiper @ read-file throw drop ;
event: ->wake ;
event: ->sleep  stop ;

: wake ( task -- )  <event ->wake event> ;
: sleep ( task -- ) <event ->sleep event> ;

: elit,  ( x -- ) ->lit cell event+ [ cell 8 = ] [IF] x! [ELSE] l! [THEN] ;
: e$, ( addr u -- )  swap elit, elit, ;
: eflit, ( x -- ) ->flit fp@ float event+ float move fdrop ;

false [IF] \ event test
    <event 1234 elit, next-task event> ?event 1234 = [IF] ." event ok" cr [THEN]
[THEN]

false [IF] \ test
    sema testsem
    
    : test-thread1
	stacksize4 NewTask4 activate  0 hex
	BEGIN
	    testsem lock
	    ." Thread-Test1 " dup . cr 1000 ms
	    testsem unlock  1+
	    100 0 DO  pause  LOOP
	AGAIN ;

    : test-thread2
	stacksize4 NewTask4 activate  0 decimal
	BEGIN
	    testsem lock
	    ." Thread-Test2 " dup . cr 1000 ms
	    testsem unlock  1+
	    100 0 DO  pause  LOOP
	AGAIN ;

    test-thread1
    test-thread2
[THEN]
