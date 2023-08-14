\ posix threads

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022 Free Software Foundation, Inc.

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

:noname defers 'image
    preserve key-ior  preserve deadline
; is 'image

c-library pthread
    \c #include <pthread.h>
    \c #include <limits.h>
    \c #include <sys/mman.h>
    \c #include <unistd.h>
    \c #include <setjmp.h>
    \c #include <stdio.h>
    \c #include <signal.h>
    \c #ifndef FIONREAD
    \c #include <sys/socket.h>
    \c #endif
    \c #ifdef __x86_64
    \c #ifdef FORCE_SYMVER
    \c #define TOSTRING(x) #x
    \c #define STRINGIFY(x) TOSTRING(x) /* Two stages necessary */
    \c __asm__(".symver pthread_sigmask,pthread_sigmask@GLIBC_" STRINGIFY(FORCE_SYMVER));
    \c #endif
    \c #endif
    \c 
    \c void create_pipe(FILE ** addr)
    \c {
    \c   int epipe[2];
    \c   pipe(epipe);
    \c   addr[0]=fdopen(epipe[0], "r");
    \c   addr[1]=fdopen(epipe[1], "a");
    \c   setvbuf(addr[1], NULL, _IONBF, 0);
    \c }
    \c void *gforth_thread(user_area * t)
    \c {
    \c   Cell x;
    \c   int throw_code;
    \c   void *ip0=(void*)(t->save_task);
    \c   sigset_t set;
    \c   gforth_UP=t;
    \c   gforth_setstacks(t);
    \c
    \c   *--gforth_SP=(Cell)t;
    \c
    \c   pthread_cleanup_push((void (*)(void*))gforth_free_stacks, (void*)t);
    \c   gforth_sigset(&set, SIGINT, SIGQUIT, SIGTERM, SIGWINCH, 0);
    \c   pthread_sigmask(SIG_BLOCK, &set, NULL);
    \c   x=gforth_go(ip0);
    \c   pthread_cleanup_pop(1);
    \c   pthread_exit((void*)x);
    \c }
    \c static inline void *gforth_thread_p()
    \c {
    \c   return (void*)&gforth_thread;
    \c }
    \c static inline void *pthread_plus(void * thread)
    \c {
    \c   return thread+sizeof(pthread_t);
    \c }
    \c static inline Cell pthreads(Cell thread)
    \c {
    \c   return thread*(int)sizeof(pthread_t);
    \c }
    \c static inline void *pthread_mutex_plus(void * thread)
    \c {
    \c   return thread+sizeof(pthread_mutex_t);
    \c }
    \c static inline Cell pthread_mutexes(Cell thread)
    \c {
    \c   return thread*(int)sizeof(pthread_mutex_t);
    \c }
    \c static inline void *pthread_cond_plus(void * thread)
    \c {
    \c   return thread+sizeof(pthread_cond_t);
    \c }
    \c static inline Cell pthread_conds(Cell thread)
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
    \c int wait_read(FILE * fid, Cell timeoutns, Cell timeouts)
    \c {
    \c   struct pollfd fds = { fileno(fid), POLLIN, 0 };
    \c #if defined(linux) && !defined(__ANDROID__)
    \c   struct timespec tout = { timeouts, timeoutns };
    \c   ppoll(&fds, 1, &tout, 0);
    \c #else
    \c   poll(&fds, 1, timeoutns/1000000+timeouts*1000);
    \c #endif
    \c   return check_read(fid);
    \c }
    \c /* optional: CPU affinity */
    \c #include <sched.h>
    \c int stick_to_core(int core_id) {
    \c #ifdef HAVE_PTHREAD_SETAFFINITY_NP
    \c   cpu_set_t cpuset;
    \c   int num_cores = sysconf(_SC_NPROCESSORS_ONLN);
    \c 
    \c   if (core_id < 0 || core_id >= num_cores)
    \c     return EINVAL;
    \c   
    \c   CPU_ZERO(&cpuset);
    \c   CPU_SET(core_id, &cpuset);
    \c   
    \c   return pthread_setaffinity_np(pthread_self(), sizeof(cpu_set_t), &cpuset);
    \c #else
    \c   return 0;
    \c #endif
    \ if there's no such function, don't do anything
    \c }

    c-function pthread+ pthread_plus a -- a ( addr -- addr' )
    c-function pthreads pthreads n -- n ( n -- n' )
    c-function thread_start gforth_thread_p -- a ( -- addr )
    c-function gforth_create_thread gforth_stacks n n n n -- a ( dsize fsize rsize lsize -- task )
    c-function pthread_create pthread_create a{(pthread_t*)} a a a -- n ( thread attr start arg )
    c-function pthread_exit pthread_exit a -- void ( retaddr -- )
    c-function pthread_kill pthread_kill a{*(pthread_t*)} n -- n ( id sig -- rvalue )
    e? os-type s" linux-android" string-prefix? 0= [IF]
	c-function pthread_cancel pthread_cancel a{*(pthread_t*)} -- n ( addr -- r )
    [THEN]
    c-function pthread_mutex_init pthread_mutex_init a a -- n ( mutex addr -- r )
    c-function pthread_mutex_destroy pthread_mutex_destroy a -- n ( mutex -- r )
    c-function pthread_mutex_lock pthread_mutex_lock a -- n ( mutex -- r )
    c-function pthread_mutex_unlock pthread_mutex_unlock a -- n ( mutex -- r )
    c-function pthread-mutex+ pthread_mutex_plus a -- a ( mutex -- mutex' )
    c-function pthread-mutexes pthread_mutexes n -- n ( n -- n' )
    c-function pthread-cond+ pthread_cond_plus a -- a ( cond -- cond' )
    c-function pthread-conds pthread_conds n -- n ( n -- n' )
    c-function sched_yield sched_yield -- void ( -- )
    c-function pthread_detach_attr pthread_detach_attr -- a ( -- addr )
    c-function pthread_cond_signal pthread_cond_signal a -- n ( cond -- r ) \ gforth-experimental
    c-function pthread_cond_broadcast pthread_cond_broadcast a -- n ( cond -- r ) \ gforth-experimental
    c-function pthread_cond_wait pthread_cond_wait a a -- n ( cond mutex -- r ) \ gforth-experimental
    c-function pthread_cond_timedwait pthread_cond_timedwait a a a -- n ( cond mutex abstime -- r ) \ gforth-experimental
    c-function create_pipe create_pipe a -- void ( pipefd[2] -- )
    c-function check_read check_read a -- n ( pipefd -- n )
    c-function wait_read wait_read a n n -- n ( pipefd timeoutns timeouts -- n )
    c-function stick-to-core stick_to_core n -- n ( core -- n )
    c-function pthread_self pthread_self -- t{*(pthread_t*)} ( pthread-id -- )
end-c-library

require ./libc.fs
require set-compsem.fs

User pthread-id
host? [IF]
    -1 cells pthread+ uallot drop

    pthread-id pthread_self
[THEN]

User epiper
User epipew
User wake#

: user' ( "name" -- u ) \ gforth-experimental
    \G @i{U} is the offset of the user variable @i{name} in the user
    \G area of each task.
    ' >body @ ;
compsem: ' >body @ postpone Literal ;

' next-task alias up@ ( -- addr ) \ gforth-experimental
\G @i{Addr} is the start of the user area of the current task
\G (@i{addr} also serves as the @i{task} identifier of the current
\G task).

0 warnings !@
: 's ( addr1 task -- addr2 ) \ gforth-experimental
\G With @i{addr1} being an address in the user data of the current
\G task, @i{addr2} is the corresponding address in @i{task}'s user
\G data.
    + up@ - ;
warnings !

s" GFORTH_IGNLIB" getenv s" true" str= 0= [IF]
    epiper create_pipe \ create pipe for main task
[THEN]

:noname ( -- )
    epiper @ ?dup-if epiper off close-file drop  THEN
    epipew @ ?dup-if epipew off close-file drop  THEN
    tmp$[] $[]free 0 (bye) ;
IS kill-task

Defer thread-init
:noname ( -- )
    rp@ cell+ backtrace-rp0 !  tmp$[] off  ofile off  tfile off
    [IFDEF] sh$ #0. sh$ 2! [THEN]
    current-input off create-input ; IS thread-init

: newtask4 ( u-data u-return u-fp u-locals -- task ) \ gforth-experimental
    \G creates @i{task} with data stack size @i{u-data}, return stack
    \G size @i{u-return}, FP stack size @i{u-fp} and locals stack size
    \G @i{u-locals}.
    gforth_create_thread >r
    throw-entry r@ udp @ throw-entry up@ - /string move
    word-pno-size chars r@ pagesize + over - dup holdbufptr r@ 's !
    + dup holdptr r@ 's !  holdend r@ 's !
    epiper r@ 's create_pipe
    action-of kill-task >body rp0 r@ 's @ 1 cells - dup rp0 r@ 's ! !
    r> ;

: newtask ( stacksize -- task ) \ gforth-experimental
    \G creates @i{task}; each stack (data, return, FP, locals) has size
    \G @i{stacksize}.
    dup 2dup newtask4 ;

: task ( ustacksize "name" -- ) \ gforth-experimental
    \G creates a task @i{name}; each stack (data, return, FP, locals)
    \G has size @i{ustacksize}.@*
    \G @i{name} execution: ( -- @i{task} )
    newtask constant ;

: (activate) ( task -- ) \ gforth-experimental
    \G activates task, the current procedure will be continued there
    r> swap >r  save-task r@ 's !
    pthread-id r@ 's pthread_detach_attr thread_start r> pthread_create drop ; compile-only

: activate ( run-time nest-sys1 task -- ) \ gforth-experimental
    \G Let @i{task} perform the code behind @code{activate}, and
    \G return to the caller of the word containing @code{activate}.
    \G When the task returns from the code behind @code{activate}, it
    \G terminates itself.
    ]] (activate) up! thread-init [[ ; immediate compile-only

: (pass) ( x1 .. xn n task -- ) \ gforth-experimental
    r> swap >r  save-task r@ 's !
    1+ dup cells negate  sp0 r@ 's @ -rot  sp0 r@ 's +!
    sp0 r@ 's @ swap 0 ?DO  tuck ! cell+  LOOP  drop
    pthread-id r@ 's pthread_detach_attr thread_start r> pthread_create drop ; compile-only

: pass ( x1 .. xn n task -- ) \ gforth-experimental
    \G Pull @i{x1 .. xn n} from the current task's data stack and push
    \G @i{x1 .. xn} on @i{task}'s data stack.  Let @i{task} perform
    \G the code behind @code{pass}, and return to the caller of the
    \G word containing @code{pass}.  When the task returns from the
    \G code behind @code{pass}, it terminates itself.
    ]] (pass) up! sp0 ! thread-init [[ ; immediate compile-only

: initiate ( xt task -- ) \ gforth-experimental
    \G Let @i{task} execute @i{xt}.  Upon return from the @i{xt}, the task
    \G terminates itself (VFX compatible).  Use one-time executable closures
    \G to pass arbitrary paramenters to a task.
    1 swap pass execute ;

: semaphore ( "name" -- ) \ gforth-experimental
    \G create a named semaphore @i{name}@*
    \G @i{name} execution: ( -- @i{semaphore} )
    Create host? IF
	1 pthread-mutexes allocate throw dup ,
	0 pthread_mutex_init drop
    ELSE  0 ,  THEN
    ['] @ set-does> ;
synonym sema semaphore

: cond ( "name" -- ) \ gforth-experimental
    \G create a named condition
    Create host? IF
	1 pthread-conds dup allocate throw dup ,
	swap erase
    ELSE  0 ,  THEN
    ['] @ set-does> ;

: lock ( semaphore -- ) \ gforth-experimental
\G lock the semaphore
    pthread_mutex_lock drop ;
: unlock ( semaphore -- ) \ gforth-experimental
\G unlock the semaphore
    pthread_mutex_unlock drop ;

: critical-section ( xt semaphore -- )  \ gforth-experimental
    \G Execute @i{xt} while locking @i{semaphore}.  After leaving
    \G @i{xt}, @i{semaphore} is unlocked even if an exception is
    \G thrown.
    { sema } try sema lock execute 0 restore sema unlock endtry throw ;
synonym c-section critical-section

: >pagealign-stack ( n addr -- n' ) \ gforth-experimental
    -1 under+ 1- pagesize negate mux 1+ ;
: stacksize ( -- u ) \ gforth-experimental
    \G @i{u} is the data stack size of the main task.
    forthstart 5 cells + @ ;
: stacksize4 ( -- u-data u-return u-fp u-locals ) \ gforth-experimental
    \G Pushes the data, return, FP, and locals stack sizes of the main task.
    forthstart 5 cells + 4 cells bounds DO  I @  cell +LOOP
    2>r >r  sp0 @ >pagealign-stack r> fp0 @ >pagealign-stack 2r> ;

: execute-task ( xt -- task ) \ gforth-experimental
    \G Create a new task @var{task} with the same stack sizes as the
    \G main task. Let @i{task} execute @i{xt}.  Upon return from the
    \G @i{xt}, the task terminates itself.
    stacksize4 newtask4 tuck initiate ;

\ event handling

s" Undefined event"   exception Constant !!event!!
s" Event buffer full" exception Constant !!ebuffull!!

Variable event#  1 event# !

User eventbuf# $100 uallot drop \ 256 bytes buffer for atomic event squences
User event-start

: 'event ( -- addr )  eventbuf# dup @ + cell+ ;
: event+ ( n -- addr )
    dup eventbuf# @ + $100 u>= !!ebuffull!! and throw
    'event swap eventbuf# +! ;
: <event ( -- ) \ gforth-experimental open-angle-event
    \G Starts an event sequence (a message).
    eventbuf# @ IF  event-start @ 1 event+ c! eventbuf# @ event-start !  THEN ;
: event> ( task -- ) \ gforth-experimental event-close-angle
    \G Finishes the event sequence (message) and sends it to @i{task}.
    \G If @i{task}'s message queue is full, this operation blocks
    \G until there is enough room.  Currently message queues are
    \G implemented as pipes and can typically buffer 4096 bytes.
    eventbuf# @ event-start @ u> IF
	>r eventbuf# cell+ eventbuf# @ event-start @
	?dup-if  /string  event-start @ 1- eventbuf# !  'event c@ event-start !
	else  eventbuf# off  then
	epipew r> 's @ write-file throw
    ELSE  drop  THEN ;

: event-crash  !!event!! throw ;

Create event-table $100 0 [DO] ' event-crash , [LOOP]

: event-does ( -- )  DOES>  @ 1 event+ c! ;
: event: ( "name" -- colon-sys ) \ gforth-experimental "event-colon"
    \G Defines an event @i{name} and starts a colon definition.@*
    \G @i{name} execution: ( -- )@* Add the colon definition to the
    \G current event sequence.  Eventually the task receiving the
    \G event sequence will run the colon definition.
    Create event# @ ,  event-does
    here 0 , >r  noname : latestxt dup event# @ cells event-table + !
    r> ! 1 event# +! ;
: (stop) ( -- )  epiper @ key-file
    dup 0>= IF  cells event-table + perform  ELSE  drop  THEN ;
: event? ( -- flag )  epiper @ check_read 0> ;

: ?events ( -- ) \ gforth-experimental question-events
    \G Perform all event sequences in the current task's message
    \G queue, one event sequence at a time.
    BEGIN  event?  WHILE  (stop)  REPEAT ;

: stop ( -- ) \ gforth-experimental
\G stops the current task, and waits for events (which may restart it)
    (stop) ?events ;
: stop-ns ( timeout -- ) \ gforth-experimental
\G Stop with timeout (in nanoseconds), better replacement for ms
    epiper @ swap 0 1000000000 um/mod wait_read 0> IF  stop  THEN ;
: stop-dns ( dtimeout -- ) \ gforth-experimental
    epiper @ -rot 1000000000 um/mod wait_read 0> IF  stop  THEN ;
\G Stop with dtimeout (in nanoseconds), better replacement for ms

: event-loop ( -- ) \ gforth-experimental
    \G Wait for event sequences, and execute any event sequences when
    \G they arrive.  Return to waiting if no event sequences are in
    \G the queue.  This word never returns.
    BEGIN  stop  AGAIN ;

: pause ( -- ) \ gforth-experimental
    \G voluntarily switch to the next waiting task (@code{pause} is
    \G the traditional cooperative task switcher; in the pthread
    \G multitasker, you don't need @code{pause} for cooperation, but
    \G you still can use it e.g. when you have to resort to polling
    \G for some reason).  This also checks for events in the queue.
    sched_yield ?events ;
: thread-deadline ( d -- ) \ gforth-experimental
    \G wait until absolute time @var{d} in nanoseconds, base is 1970-1-1 0:00
    \G UTC
    BEGIN  2dup ntime d- 2dup d0> WHILE  stop-dns  REPEAT
    2drop 2drop ;
' thread-deadline is deadline

event: :>lit  0 { w^ n } n cell epiper @ read-file throw drop n @ ;
event: :>flit 0e { f^ r } r float epiper @ read-file throw drop r f@ ;

: elit,  ( x -- ) \ gforth-experimental e-lit-comma
    \G Put @i{x} in the current event sequence.  The receiving task
    \G will push @i{x}.
    :>lit cell event+ [ cell 8 = ] [IF] x! [ELSE] l! [THEN] ;

: estring, ( c-addr u -- ) \ gforth-experimental e-string-comma
    \G Put the string @i{c-addr u} in the current event sequence.  The
    \G receiving task will push a string @i{c-addr2 u}, with the same
    \G contents, but possibly a different address.  Do not change the
    \G contents of the string after sending the event sequence,
    \G because the receiving task may or may not see the change (or
    \G worse, some in-between state).
    swap elit, elit, ;
: eflit, ( r -- ) \ gforth-experimental e-f-lit-comma
    \G Put @i{r} in the current event sequence.  The receiving task
    \G will push @i{r} on the floating-point stack.
    :>flit { f^ r } r float event+ float move ;

event: :>wake ( wake# -- )  wake# ! ;
event: :>sleep  stop ;

: restart ( task -- ) \ gforth-experimental
    \G Wake a task
    <event 0 elit, :>wake event> ;
synonym wake restart ( task -- ) \ gforth-experimental

event: :>restart ( wake# task -- ) <event swap elit, :>wake event> ;

: halt ( task -- ) \ gforth-experimental
    \G Stop a task
    <event :>sleep event> ;
synonym sleep halt ( task -- )

: kill ( task -- ) \ gforth-experimental
    \G Terminate @i{task}.
    user' pthread-id +
    [IFDEF] pthread_cancel
	pthread_cancel drop
    [ELSE]
	15 pthread_kill drop
    [THEN] ;

: event| ( task -- ) \ gforth-experimental
    \G send an event and block
    dup up@ = IF \ don't block, just eval what we sent to ourselves
	event> ?events
    ELSE
	wake# @ 1+ dup >r elit, up@ elit, :>restart event>
	BEGIN  stop  wake# @ r@ =  UNTIL  rdrop
    THEN ;

\ User deferred words, user values

[IFUNDEF] >uvalue
    : >uvalue ( xt -- addr )
	>body @ next-task + ;
    to-opt: >body @ postpone useraddr , ;
[THEN]

' >uvalue defer-table to-method: udefer-to

: UDefer ( "name" -- ) \ gforth
    \G @i{Name} is a task-local deferred word.@*
    \G @i{Name} execution: ( ... -- ... )
    Create cell uallot ,
    [: @ up@ + perform ;] set-does>
    ['] udefer-to set-to
    [: >body @ postpone useraddr , postpone perform ;] set-optimizer ;

false [IF] \ event test - send to myself
    <event 1234 elit, up@ event> ?event 1234 = [IF] ." event ok" cr [THEN]
[THEN]

\ key for pthreads

User keypollfds pollfd 2* cell- uallot drop

:noname defers 'image
    keypollfds pollfd 2* erase
    event-start off
    eventbuf# $100 erase
    pthread-id [ host? [IF] 0 pthread+ [ELSE] cell [THEN] ]L erase
    epiper off
    epipew off
    wake# off
; is 'image

: prep-key ( -- )
    keypollfds >r
    stdin fileno    POLLIN r> fds!+ >r
    epiper @ fileno POLLIN r> fds!+ drop ;

: thread-key ( -- key )
    prep-key
    BEGIN  key? winch? @ or 0= WHILE  keypollfds 2 -1 poll drop
	    keypollfds pollfd + revents w@ POLLIN and IF  ?events  THEN
    REPEAT  winch? @ IF  EINTR  ELSE  defers key-ior  THEN ;

' thread-key is key-ior

:noname defers 'cold
    host? IF
	pthread-id pthread_self epiper create_pipe
	preserve key-ior  preserve deadline
    THEN ; is 'cold

\ a simple test (not commented in)

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
