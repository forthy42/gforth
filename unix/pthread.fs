\ posix threads

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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
    \c #if defined(__sun) || defined(__FreeBSD__)
    \c #include <sys/filio.h>
    \c #else
    \c #include <sys/ioctl.h>
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
    \c   int __attribute__((unused)) perr=pipe(epipe);
    \c   addr[0]=fdopen(epipe[0], "r");
    \c   addr[1]=fdopen(epipe[1], "a");
    \c   setvbuf(addr[0], NULL, _IONBF, 0);
    \c   setvbuf(addr[1], NULL, _IONBF, 0);
    \c }
    \c void *gforth_thread(user_area * t)
    \c {
    \c   Cell x;
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
    \c   int result=EINVAL;
    \c #ifdef HAVE_PTHREAD_SETAFFINITY_NP
    \c #if defined(__NetBSD__)
    \c #define cpu_set_t cpuset_t
    \c #define CPUSETSIZE cpuset_size(cpusetp)
    \c #define CPU_ZERO(cset) cpuset_zero(cset)
    \c #define CPU_SET(ci, cset) cpuset_set(ci, cset)
    \c #define CPUSET_DESTROY(cset) cpuset_destroy(cset)
    \c   cpu_set_t * cpusetp=cpuset_create();
    \c #else
    \c #define CPUSETSIZE sizeof(cpu_set_t)
    \c #define CPUSET_DESTROY(cset)
    \c #define cpusetp &cpuset
    \c   cpu_set_t cpuset;
    \c #endif
    \c   int num_cores = sysconf(_SC_NPROCESSORS_ONLN);
    \c
    \c   if (core_id < 0 || core_id >= num_cores) {
    \c     goto err_exit;
    \c   }
    \c   
    \c   CPU_ZERO(cpusetp);
    \c   CPU_SET(core_id, cpusetp);
    \c   
    \c   result=pthread_setaffinity_np(pthread_self(), CPUSETSIZE, cpusetp);
    \c err_exit:
    \c   CPUSET_DESTROY(cpusetp);
    \c #endif
    \c   return result;
    \ if there's no such function, don't do anything
    \c }

    c-variable thread_start gforth_thread ( -- addr )
    c-function gforth_create_thread gforth_stacks n n n n -- a ( dsize fsize rsize lsize -- task )
    c-function pthread_create pthread_create a{(pthread_t*)} a a a -- n ( thread attr start arg )
    c-function pthread_exit pthread_exit a -- void ( retaddr -- )
    c-function pthread_join pthread_join a{*(pthread_t*)} a -- n ( thread retval -- errno )
    c-function pthread_kill pthread_kill a{*(pthread_t*)} n -- n ( id sig -- rvalue )
    e? os-type s" linux-android" string-prefix? 0= [IF]
	c-function pthread_cancel pthread_cancel a{*(pthread_t*)} -- n ( addr -- r )
    [THEN]
    c-function pthread_mutex_init pthread_mutex_init a a -- n ( mutex addr -- r )
    c-function pthread_mutex_destroy pthread_mutex_destroy a -- n ( mutex -- r )
    c-function pthread_mutex_lock pthread_mutex_lock a -- n ( mutex -- r )
    c-function pthread_mutex_unlock pthread_mutex_unlock a -- n ( mutex -- r )
    c-function sched_yield sched_yield -- void ( -- )
    c-function pthread_detach pthread_detach a{*(pthread_t*)} -- n ( addr -- r )
    c-function pthread_cond_init pthread_cond_init a a -- n ( cond attr -- r )
    c-function pthread_cond_destroy pthread_cond_destroy a -- n ( cond -- r )
    c-function pthread_cond_signal pthread_cond_signal a -- n ( cond -- r ) \ gforth-experimental
    c-function pthread_cond_broadcast pthread_cond_broadcast a -- n ( cond -- r ) \ gforth-experimental
    c-function pthread_cond_wait pthread_cond_wait a a -- n ( cond mutex -- r ) \ gforth-experimental
    c-function pthread_cond_timedwait pthread_cond_timedwait a a a -- n ( cond mutex abstime -- r ) \ gforth-experimental
    c-function create_pipe create_pipe a -- void ( pipefd[2] -- )
    c-function check_read check_read a -- n ( pipefd -- n )
    c-function wait_read wait_read a n n -- n ( pipefd timeoutns timeouts -- n )
    c-function stick-to-core stick_to_core n -- n ( core -- n )
    c-function pthread_self pthread_self -- t{*(pthread_t*)} ( pthread-id -- )
    c-function pthread_atfork pthread_atfork a a a -- n ( prepare parent child -- errorflag )
    callback# >r
    3 to callback#
    c-callback atfork: -- void ( -- )
    r> to callback#
end-c-library

require unix/pthread-types.fs
pthread_t cfield: pthread+ drop
pthread_mutex_t cfield: pthread-mutex+ drop
pthread_cond_t cfield: pthread-cond+ drop

Create pthreads 0 pthread+ ,
DOES> @ * ;
opt: @ ]] literal * [[ ;
' pthreads create-from pthread-mutexes reveal 0 pthread-mutex+ ,
' pthreads create-from pthread-conds   reveal 0 pthread-cond+ ,

require ./libc.fs
require set-compsem.fs

User pthread-joinwait
User pthread-id
-1 cells pthread+ aligned uallot drop

host? [IF]
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

[IFUNDEF] up@
' next-task alias up@ ( -- addr ) \ gforth-experimental
    \G @i{Addr} is the start of the user area of the current task
    \G (@i{addr} also serves as the @i{task} identifier of the current
    \G task).
[THEN]

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
    pthread-joinwait @ 0= IF  pthread-id pthread_detach drop  THEN
    epiper @ ?dup-if epiper off close-file drop  THEN
    epipew @ ?dup-if epipew off close-file drop  THEN
    tmp$[] $[]free
    0 (bye) ;
IS kill-task

Defer prepare-fork  ' noop is prepare-fork
Defer parent-fork   ' noop is parent-fork
Defer child-fork    ' noop is child-fork

0 Value prepare-cb#
0 Value parent-cb#
0 Value child-cb#

: atfork-cbs ( -- )
    [ ' atfork: >body @ ]L ['] atfork: >body !
    [: prepare-fork ;] atfork: to prepare-cb#
    [: parent-fork ;]  atfork: to parent-cb#
    [: child-fork ;]   atfork: to child-cb# ;
: atfork-init ( -- )
    prepare-cb# parent-cb# child-cb# pthread_atfork errno-throw ;

Defer thread-init
:noname ( -- )
    rp@ cell+ backtrace-rp0 !  tmp$[] off  ofile off  tfile off
    [IFDEF] sh$ #0. sh$ 2! [THEN]
    current-input off create-input
    host? IF  atfork-init  THEN ; IS thread-init

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

: (activate) ( task -- ) \ gforth-internal
    \G activates task, the current procedure will be continued there
    r> swap >r  save-task r@ 's !
    pthread-id r@ 's 0 thread_start r> pthread_create drop ; compile-only

: activate ( run-time nest-sys1 task -- ) \ gforth-experimental
    \G Let @i{task} perform the code behind @code{activate}, and
    \G return to the caller of the word containing @code{activate}.
    \G When the task returns from the code behind @code{activate}, it
    \G terminates itself.
    ]] (activate) up! thread-init [[ ; immediate compile-only

: (pass) ( x1 .. xn n task -- ) \ gforth-internal
    r> swap >r  save-task r@ 's !
    1+ dup cells negate  sp0 r@ 's @ -rot  sp0 r@ 's +!
    sp0 r@ 's @ swap 0 ?DO  tuck ! cell+  LOOP  drop
    pthread-id r@ 's 0 thread_start r> pthread_create drop ; compile-only

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
    Create  here 1 pthread-mutexes allot
    host? IF
	0 pthread_mutex_init drop
    ELSE  drop  THEN ;
synonym sema semaphore

: cond ( "name" -- ) \ gforth-experimental
    \G create a named condition
    Create  here 1 pthread-conds allot
    host? IF
	0 pthread_cond_init drop
    ELSE  drop  THEN ;

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

: >pagealign-stack ( n addr -- n' ) \ gforth-internal
    -1 under+ 1- pagesize negate mux 1+ ;
: stacksize ( -- u ) \ gforth-experimental
    \G @i{u} is the data stack size of the main task.
    forthstart section-desc + @ ;
: stacksize4 ( -- u-data u-return u-fp u-locals ) \ gforth-experimental
    \G Pushes the data, return, FP, and locals stack sizes of the main task.
    forthstart section-desc + 4 cells cell MEM+DO  I @  LOOP
    2>r >r  sp0 @ >pagealign-stack r> fp0 @ >pagealign-stack 2r> ;

: execute-task ( xt -- task ) \ gforth-experimental
    \G Create a new task @var{task} with the same stack sizes as the
    \G main task. Let @i{task} execute @i{xt}.  Upon return from the
    \G @i{xt}, the task terminates itself.
    stacksize4 newtask4 tuck initiate ;

: (stop) ( -- )
    {: | w^ xt :} xt cell epiper @ read-file throw cell = IF
	xt perform
    THEN ;
: send-event ( xt task -- ) \ gforth-experimental
    \G Inter-task communication: send @var{xt} @code{( -- )} to
    \G @var{task}.  @var{task} executes the xt at some later point in
    \G time.  To pass parameters, construct a one-shot closure that
    \G contains the parameters (@pxref{Closures}) and pass the xt of
    \G that closure.
    >r {: w^ xt :} xt cell epipew r> 's @ write-file throw ;
: event? ( -- flag ) epiper @ check_read 0> ;

: ?events ( -- ) \ gforth-experimental question-events
    \G Execute all event xts in the current task's message
    \G queue, one xt at a time.
    BEGIN  event?  WHILE  (stop)  REPEAT ;

: stop ( -- ) \ gforth-experimental
\G stops the current task, and waits for events (which may restart it)
    (stop) ?events ;
: stop-ns ( timeout -- ) \ gforth-experimental
\G Stop with timeout (in nanoseconds), better replacement for ms
    epiper @ swap 0 1000000000 um/mod wait_read 0> IF  stop  THEN ;
: stop-dns ( dtimeout -- ) \ gforth-experimental
\G Stop with timeout (in nanoseconds), better replacement for ms
    epiper @ -rot 1000000000 um/mod wait_read 0> IF  stop  THEN ;
\G Stop with dtimeout (in nanoseconds), better replacement for ms

: event-loop ( -- ) \ gforth-experimental
    \G Wait for event xts and execute these xts when they arrive, one
    \G at a time.  Return to waiting if no event xts are in the queue.
    \G This word never returns.
    BEGIN stop AGAIN ;

: pause ( -- ) \ gforth-experimental
    \G voluntarily switch to the next waiting task (@code{pause} is
    \G the traditional cooperative task switcher; in the pthread
    \G multitasker, you don't need @code{pause} for cooperation, but
    \G you still can use it e.g. when you have to resort to polling
    \G for some reason).  This also checks for events in the queue.
    sched_yield ?events ;
: thread-deadline ( d -- ) \ gforth-experimental
    \G stop until absolute time @var{d} in nanoseconds, base is
    \G 1970-1-1 0:00 UTC, but you usually will want to base your
    \G deadlines on a time you get with @code{ntime}.
    BEGIN  2dup ntime d- 2dup d0> WHILE  stop-dns  REPEAT
    2drop 2drop ;
' thread-deadline is deadline

: (restart) ( task wake# -- )
    [{: n :}h1 n wake# ! ;] swap send-event ;
: restart ( task -- ) \ gforth-experimental
    \G Wake @i{task} (no difference from @code{wake})
    0 (restart) ;
synonym wake restart ( task -- ) \ gforth-experimental
    \G Wake @i{task}

: halt ( task -- ) \ gforth-experimental
    \G Stop @i{task} (no difference from @code{sleep})
    ['] stop swap send-event ;
synonym sleep halt ( task -- ) \ gforth-experimental
    \G Stop @i{task} (no difference from @code{halt})

: event-block ( task -- ) \ gforth-internal
    \G send an event and wait for the answer
    dup up@ = IF \ don't block, just eval what we sent to ourselves
	drop ?events
    ELSE
	wake# @ 1+ dup >r up@ [{: wake task :}h1
	    task wake (restart) ;] swap send-event
	BEGIN  stop  wake# @ r@ =  UNTIL  rdrop
    THEN ;

: join ( task -- ) \ gforth-experimental
    \G wait for the task to terminate
    up@ over user' pthread-joinwait + !
    user' pthread-id + { thread-id[ 0 pthread+ ] }
    thread-id[ 0 pthread_join drop ;

: (kill) ( task xt -- ) \ gforth-experimental
    \G Terminate @i{task} by executing @i{xt}, which has the stack effect @code{( task -- )}.
    up@ over user' pthread-joinwait + !
    over user' pthread-id + { thread-id[ 0 pthread+ ] }
    execute  thread-id[ 0 pthread_join drop ;

: kill ( task -- ) \ gforth-experimental
    \G Terminate @i{task}.
    [: user' pthread-id +
	[IFDEF] pthread_cancel
	    pthread_cancel drop
	[ELSE]
	    15 pthread_kill drop
	[THEN] ;] (kill) ;

\ User deferred words, user values

[IFUNDEF] >uvalue
    : >uvalue ( xt -- addr )
	>body @ up@ + ;
    fold1: >body @ postpone up@ lit, postpone + ;
[THEN]

' >uvalue defer-table to-class: udefer-to

: UDefer ( "name" -- ) \ gforth
    \G @i{Name} is a task-local deferred word.@*
    \G @i{Name} execution: ( ... -- ... )
    Create cell uallot ,
    [: @ up@ + perform ;] set-does>
    ['] udefer-to set-to
    [: >body @ postpone up@ lit, postpone + postpone perform ;] set-optimizer ;

\ key for pthreads

User keypollfds pollfd 2* cell- aligned uallot drop

:noname defers 'image
    keypollfds pollfd 2* erase
    pthread-id pthread_t erase
    epiper off
    epipew off
    wake# off
    0 to prepare-cb#
    0 to parent-cb#
    0 to child-cb#
; is 'image

: prep-key ( -- )
    keypollfds >r
    infile-id fileno POLLIN r> fds!+ >r
    epiper @  fileno POLLIN r> fds!+ drop ;

: thread-key ( -- key )
    prep-key
    BEGIN  key? 0= WHILE  keypollfds 2 -1 poll drop
	    keypollfds pollfd + revents w@ POLLIN and IF  ?events  THEN
	keypollfds revents w@ POLLIN POLLHUP or and UNTIL  THEN
    defers key-ior ;

' thread-key is key-ior

:is 'cold defers 'cold
    host? IF
	atfork-cbs atfork-init
	pthread-id pthread_self epiper create_pipe
	preserve key-ior  preserve deadline
    THEN ;

host? [IF] atfork-cbs atfork-init [THEN]

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
