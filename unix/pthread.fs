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
    \c extern void * alloc_mmap(Cell size);
    \c extern void page_noaccess(void *a);
    \c #define wholepage(n) (((n)+p-1)&~(p-1))
    \c typedef struct {
    \c   Cell data_stack_size;
    \c   Cell fp_stack_size;
    \c   Cell return_stack_size;
    \c   Cell locals_stack_size;
    \c   void *boot_entry;
    \c } ImageHeader;
    \c void *gforth_thread(ImageHeader * header)
    \c {
    \c   Cell *sp0;
    \c   Cell *rp0;
    \c   Float *fp0;
    \c   char *lp0;
    \c   Cell
    \c #if HAVE_GETPAGESIZE
    \c   p=getpagesize(); /* Linux/GNU libc offers this */
    \c #elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
    \c   p=sysconf(_SC_PAGESIZE); /* POSIX.4 */
    \c #elif PAGESIZE
    \c   p=PAGESIZE; /* in limits.h according to Gallmeister's POSIX.4 book */
    \c #endif
    \c   Cell dsize = wholepage(header->data_stack_size);
    \c   Cell rsize = wholepage(header->return_stack_size);
    \c   Cell fsize = wholepage(header->fp_stack_size);
    \c   Cell lsize = wholepage(header->locals_stack_size);
    \c   size_t totalsize = dsize+fsize+rsize+lsize+5*p;
    \c   void *a = alloc_mmap(totalsize);
    \c   if (a != (void *)MAP_FAILED) {
    \c     page_noaccess(a); a+=p; a+=dsize; sp0=a;
    \c     page_noaccess(a); a+=p; a+=fsize; fp0=a;
    \c     page_noaccess(a); a+=p; a+=rsize; rp0=a;
    \c     page_noaccess(a); a+=p; a+=lsize; lp0=a;
    \c     page_noaccess(a);
    \c     return gforth_engine(header->boot_entry, sp0, rp0, fp0, lp0, 0);
    \c   }
    \c   return NULL;
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
    c-function pthread+ pthread_plus a -- a ( addr -- addr' )
    c-function pthreads pthreads n -- n ( n -- n' )
    c-function thread_start gforth_thread_p -- a ( -- addr )
    c-function pthread_create pthread_create a a a a -- n ( thread attr start arg )
end-c-library
