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
    c-function fdopen fdopen n a -- a ( fd fileattr -- file )
    c-function fcntl fcntl n n n -- n ( fd n1 n2 -- ior )
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

: fds!+ ( fileno flag addr -- addr' )
    >r r@ events w!  r@ fd l!  r> pollfd + ; 
