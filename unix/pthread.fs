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
    \c typedef struct {
    \c   Cell next_task;
    \c   Cell prev_task;
    \c   Cell save_task;
    \c   Cell sp0, rp0, fp0, lp0;
    \c   Cell throw_entry;
    \c } user_area;
    \c int pagesize = 1;
    \c void page_noaccess(void *a)
    \c {
    \c   /* try mprotect first; with munmap the page might be allocated later */
    \c   if (mprotect(a, pagesize, PROT_NONE)==0) {
    \c     return;
    \c   }
    \c   if (munmap(a,pagesize)==0) {
    \c     return;
    \c   }
    \c }  
    \c void * alloc_mmap(Cell size)
    \c {
    \c   void *r;
    \c 
    \c #if defined(MAP_ANON)
    \c   r = mmap(NULL, size, PROT_EXEC|PROT_READ|PROT_WRITE, MAP_ANON|MAP_PRIVATE, -1, 0);
    \c #else /* !defined(MAP_ANON) */
    \c   /* Ultrix (at least) does not define MAP_FILE and MAP_PRIVATE (both are
    \c      apparently defaults) */
    \c   static int dev_zero=-1;
    \c 
    \c   if (dev_zero == -1)
    \c     dev_zero = open("/dev/zero", O_RDONLY);
    \c   if (dev_zero == -1) {
    \c     r = MAP_FAILED;
    \c   } else {
    \c     r=mmap(NULL, size, PROT_EXEC|PROT_READ|PROT_WRITE, MAP_FILE|MAP_PRIVATE, dev_zero, 0);
    \c   }
    \c #endif /* !defined(MAP_ANON) */
    \c   return r;  
    \c }
    \c
    \c Cell gforth_create_thread(Cell dsize, Cell rsize, Cell fsize, Cell lsize)
    \c {
    \c #if HAVE_GETPAGESIZE
    \c   pagesize=getpagesize(); /* Linux/GNU libc offers this */
    \c #elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
    \c   pagesize=sysconf(_SC_PAGESIZE); /* POSIX.4 */
    \c #elif PAGESIZE
    \c   pagesize=PAGESIZE; /* in limits.h according to Gallmeister's POSIX.4 book */
    \c #endif
    \c #ifdef SIGSTKSZ
    \c   stack_t sigstack;
    \c   int sas_retval=-1;
    \c #endif
    \c   size_t totalsize;
    \c   Cell a;
    \c   user_area * up0;
    \c   Cell dsizep = wholepage(dsize);
    \c   Cell rsizep = wholepage(rsize);
    \c   Cell fsizep = wholepage(fsize);
    \c   Cell lsizep = wholepage(lsize);
    \c   totalsize = dsizep+fsizep+rsizep+lsizep+6*pagesize;
    \c #ifdef SIGSTKSZ
    \c   totalsize += 2*SIGSTKSZ;
    \c #endif
    \c   a = (Cell)alloc_mmap(totalsize);
    \c   if (a != (Cell)MAP_FAILED) {
    \c     up0=(user_area*)a; a+=pagesize;
    \c     page_noaccess((void*)a); a+=pagesize; up0->sp0=a+dsize; a+=dsizep;
    \c     page_noaccess((void*)a); a+=pagesize; up0->fp0=a+fsize; a+=fsizep;
    \c     page_noaccess((void*)a); a+=pagesize; up0->rp0=a+rsize; a+=rsizep;
    \c     page_noaccess((void*)a); a+=pagesize; up0->lp0=a+lsize; a+=lsizep;
    \c     page_noaccess((void*)a); a+=pagesize;
    \c #ifdef SIGSTKSZ
    \c     sigstack.ss_sp=(void*)a+SIGSTKSZ;
    \c     sigstack.ss_size=SIGSTKSZ;
    \c     sas_retval=sigaltstack(&sigstack,(stack_t *)0);
    \c #endif
    \c     return (Cell)up0;
    \c   }
    \c   return 0;
    \c }
    \c
    \c void gforth_cleanup_thread(void * t)
    \c {
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
    c-function pthread+ pthread_plus a -- a ( addr -- addr' )
    c-function pthreads pthreads n -- n ( n -- n' )
    c-function thread_start gforth_thread_p -- a ( -- addr )
    c-function gforth_create_thread gforth_create_thread n n n n -- a ( dsize rsize fsize lsize -- task )
    c-function pthread_create pthread_create a a a a -- n ( thread attr start arg )
    c-function pthread_exit pthread_exit a -- void ( retaddr -- )
    c-function pthread_mutex_init pthread_mutex_init a a -- n ( mutex addr -- r )
    c-function pthread_mutex_lock pthread_mutex_lock a -- n ( mutex -- r )
    c-function pthread_mutex_unlock pthread_mutex_unlock a -- n ( mutex -- r )
    c-function pthread-mutex+ pthread_mutex_plus a -- a ( mutex -- mutex' )
    c-function pthread-mutexes pthread_mutexes n -- n ( n -- n' )
    c-function pause sched_yield -- void ( -- )
    c-function pthread_detatch_attr pthread_detach_attr -- a ( -- addr )
    c-function create_pipe create_pipe a -- void ( pipefd[2] -- )
end-c-library

User pthread-id  -1 cells pthread+ uallot drop
User epiper
User epipew

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

: lock ( addr -- )  pthread_mutex_lock drop ;
: unlock ( addr -- )  pthread_mutex_unlock drop ;

: stacksize ( -- n ) forthstart 4 cells + @
    sp0 @ $FFF and -$1000 or + ;
: stacksize4 ( -- dsize rsize fsize lsize )
    forthstart 4 cells + 4 cells bounds DO  I @  cell +LOOP
    2swap swap sp0 @ $FFF and -$1000 or + swap 2swap
    swap       fp0 @ $FFF and -$1000 or + swap ;

false [IF] \ test
    semaphore testsem
    
    : test-thread1
	stacksize NewTask activate  0 hex
	BEGIN
	    testsem lock
	    ." Thread-Test1 " dup . cr 1000 ms
	    testsem unlock  1+
	    100 0 DO  pause  LOOP
	AGAIN ;

    : test-thread2
	stacksize NewTask activate  0 decimal
	BEGIN
	    testsem lock
	    ." Thread-Test2 " dup . cr 1000 ms
	    testsem unlock  1+
	    100 0 DO  pause  LOOP
	AGAIN ;

    test-thread1
    test-thread2
[THEN]
