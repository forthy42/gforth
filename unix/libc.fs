\ useful libc functions

\ Copyright (C) 2015 Free Software Foundation, Inc.

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

c-library libc
    \c #include <stdio.h>
    \c #include <errno.h>
    \c #include <unistd.h>
    \c #include <poll.h>
    \c #include <fcntl.h>
    \c #include <locale.h>
    \c #if HAVE_GETPAGESIZE
    \c #elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
    \c #define getpagesize() sysconf(_SC_PAGESIZE)
    \c #elif PAGESIZE
    \c #define getpagesize() PAGESIZE
    \c #endif
    c-value errno errno -- n ( -- value )
    c-function getpagesize getpagesize -- n ( -- size )
    c-function fileno fileno a{(FILE*)} -- n ( file* -- fd )
    c-function poll poll a n n -- n ( fds nfds timeout -- r )
    e? os-type s" linux-gnu" str= [IF]
	c-function ppoll ppoll a n a a -- n ( fds nfds timeout_ts sigmask -- r )
	\c #include <sys/epoll.h>
	c-function epoll_create epoll_create n -- n ( n -- epfd )
	c-function epoll_ctl epoll_ctl n n n a -- n ( epfd op fd event -- r )
	c-function epoll_wait epoll_wait n a n n -- n ( epfd events maxevs timeout -- r )
    [THEN]
    c-function fdopen fdopen n s -- a ( fd fileattr len -- file )
    c-function fcntl fcntl n n n -- n ( fd n1 n2 -- ior )
    c-function open open s n n -- n ( path len flags mode -- fd )
    c-function read read n a n -- n ( fd addr u -- u' )
    c-function write write n a n -- n ( fd addr u -- u' )
    c-function close close n -- n ( fd -- r )
    c-function setlocale setlocale n s -- a ( category locale -- locale )
    c-function fork fork -- n ( -- pid_t )
    c-function execve execve s a a -- n ( filename len argv envp -- ret )
    c-value environ environ -- a ( -- env )
end-c-library

getpagesize constant pagesize

begin-structure pollfd
    lfield: fd
    wfield: events
    wfield: revents
end-structure

$001 Constant POLLIN
$002 Constant POLLPRI
$004 Constant POLLOUT

0 Constant LC_CTYPE
1 Constant LC_NUMERIC
2 Constant LC_TIME
3 Constant LC_COLLATE
4 Constant LC_MONETARY
5 Constant LC_MESSAGES
6 Constant LC_ALL
7 Constant LC_PAPER
8 Constant LC_NAME
9 Constant LC_ADDRESS
10 Constant LC_TELEPHONE
11 Constant LC_MEASUREMENT
12 Constant LC_IDENTIFICATION

: fds!+ ( fileno flag addr -- addr' )
    >r r@ events w!  r@ fd l!  r> pollfd + ; 

: ?ior ( r -- )
    \G use errno to generate throw when failing
    0< IF  -512 errno - throw  THEN ;

: fd>file ( fd -- file )  s" w+" fdopen ;

: fork+exec ( filename len argv -- )
    fork 0= IF [: environ execve ;] catch drop (bye) ELSE drop 2drop THEN ;