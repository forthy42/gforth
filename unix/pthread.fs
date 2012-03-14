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
    \c #define wholepage(n) (((n)+pagesize-1)&~(pagesize-1))
    \c typedef struct {
    \c   Cell data_stack_size;
    \c   Cell fp_stack_size;
    \c   Cell return_stack_size;
    \c   Cell locals_stack_size;
    \c   Cell sp0, fp0, rp0, lp0, up0;
    \c   Cell boot_entry;
    \c   Cell saved_ip, saved_rp;
    \c } threadId;
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
    \c int gforth_create_thread(threadId * t)
    \c {
    \c #if HAVE_GETPAGESIZE
    \c   pagesize=getpagesize(); /* Linux/GNU libc offers this */
    \c #elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
    \c   pagesize=sysconf(_SC_PAGESIZE); /* POSIX.4 */
    \c #elif PAGESIZE
    \c   pagesize=PAGESIZE; /* in limits.h according to Gallmeister's POSIX.4 book */
    \c #endif
    \c   Cell dsize = wholepage(t->data_stack_size);
    \c   Cell rsize = wholepage(t->return_stack_size);
    \c   Cell fsize = wholepage(t->fp_stack_size);
    \c   Cell lsize = wholepage(t->locals_stack_size);
    \c   size_t totalsize = dsize+fsize+rsize+lsize+5*pagesize;
    \c   Cell a = (Cell)alloc_mmap(totalsize);
    \c   if (a != (Cell)MAP_FAILED) {
    \c     page_noaccess((void*)a); a+=pagesize; t->up0=a; a+=dsize; t->sp0=a;
    \c     page_noaccess((void*)a); a+=pagesize; a+=fsize; t->fp0=a;
    \c     page_noaccess((void*)a); a+=pagesize; a+=rsize; t->rp0=a;
    \c     page_noaccess((void*)a); a+=pagesize; a+=lsize; t->lp0=a;
    \c     page_noaccess((void*)a);
    \c     return 1;
    \c   }
    \c   return 0;
    \c }
    \c
    \c void *gforth_thread(threadId * t)
    \c {
    \c   gforth_UP = (char*)(t->up0);
    \c   return gforth_engine((void*)(t->boot_entry), (Cell*)(t->sp0), (Cell*)(t->rp0), (Float*)(t->fp0), (void*)(t->lp0), (char*)&(t->saved_ip));
    \c }
    \c void *gforth_thread_p()
    \c {
    \c   return (void*)&gforth_thread;
    \c }
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
    c-function pthread+ pthread_plus a -- a ( addr -- addr' )
    c-function pthreads pthreads n -- n ( n -- n' )
    c-function thread_start gforth_thread_p -- a ( -- addr )
    c-function gforth_create_thread gforth_create_thread a -- n ( addr -- n )
    c-function pthread_create pthread_create a a a a -- n ( thread attr start arg )
    c-function pthread_exit pthread_exit a -- void ( retaddr -- )
    c-function pthread_mutex_init pthread_mutex_init a a -- n ( mutex addr -- r )
    c-function pthread_mutex_lock pthread_mutex_lock a -- n ( mutex -- r )
    c-function pthread_mutex_unlock pthread_mutex_unlock a -- n ( mutex -- r )
    c-function pthread-mutex+ pthread_mutex_plus a -- a ( mutex -- mutex' )
    c-function pthread-mutexes pthread_mutexes n -- n ( n -- n' )
    c-function pause pthread_yield -- void ( -- )
end-c-library

begin-structure threadId
field: data_stack_size
field: fp_stack_size
field: return_stack_size
field: locals_stack_size
field: t_sp0
field: t_fp0
field: t_rp0
field: t_lp0
field: t_up0
field: boot_entry
field: saved_ip
field: saved_rp
1 pthreads +field t_pthread
end-structure

: NewTask ( stacksize -- task )
    threadId allocate throw >r
    dup 2dup r@ 2! r@ cell+ cell+ 2!
    r@ gforth_create_thread drop
    next-task r@ t_up0 @ udp @ move
    r> ;

: activate ( task -- )
    r> swap >r
    r@ boot_entry !
    r@ t_pthread 0 thread_start r> pthread_create drop ;

: semaphore ( "name" -- )
    Create here 1 pthread-mutexes allot 0 pthread_mutex_init drop ;

: lock ( addr -- )  pthread_mutex_lock drop ;
: unlock ( addr -- )  pthread_mutex_unlock drop ;

: stacksize ( -- n ) forthstart 4 cells + @ ;

true [IF] \ test
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