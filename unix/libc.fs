\ useful libc functions

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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
    \c #include <sys/stat.h>
    \c #if HAVE_GETPAGESIZE
    \c #elif HAVE_SYSCONF && defined(_SC_PAGESIZE)
    \c #define getpagesize() sysconf(_SC_PAGESIZE)
    \c #elif PAGESIZE
    \c #define getpagesize() PAGESIZE
    \c #endif
    \c #define set_errno(n) (errno=n)
    \c extern char ** environ;
    c-value errno errno -- n ( -- value )
    c-function ->errno set_errno n -- void ( n -- )
    c-function getpagesize getpagesize -- n ( -- size )
    c-function fileno fileno a{(FILE*)} -- n ( file* -- fd )
    c-function poll poll a n n -- n ( fds nfds timeout -- r )
    e? os-type s" linux-gnu" string-prefix? [IF]
	c-function ppoll ppoll a n a a -- n ( fds nfds timeout_ts sigmask -- r )
	\c #if HAVE_SYS_EPOLL_H
	\c # include <sys/epoll.h>
	\c #endif
	c-function epoll_create epoll_create n -- n ( n -- epfd )
	c-function epoll_ctl epoll_ctl n n n a -- n ( epfd op fd event -- r )
	c-function epoll_wait epoll_wait n a n n -- n ( epfd events maxevs timeout -- r )
	\c #if HAVE_SPAWN_H
	\c # include <spawn.h>
	\c #endif
	c-function posix_spawnp posix_spawnp a s a a a a -- n ( *pid path addr actions attrp argv envp -- ret )
    [THEN]
    c-function fdopen fdopen n s -- a ( fd fileattr len -- file )
    c-function fcntl fcntl n n n -- n ( fd n1 n2 -- ior )
    c-function open open s n n -- n ( path len flags mode -- fd )
    c-function read read n a n -- n ( fd addr u -- u' )
    c-function write write n a n -- n ( fd addr u -- u' )
    c-function close close n -- n ( fd -- r )
    c-function setlocale setlocale n s -- a ( category locale len -- locale )
    c-function (getpid) getpid -- n ( -- n ) \ for completion
    c-function (fork) fork -- n ( -- pid_t )
    c-function execvp execvp s a -- n ( filename len argv -- ret )
    c-function exit() exit n -- void ( ret -- )
    c-function symlink symlink s s -- n ( target len1 path len2 -- ret )
    c-function link link s s -- n ( target len1 path len2 -- ret )
    c-function readlink readlink s a n -- n ( path len buf len2 -- ret )
    c-function rmdir rmdir s -- n ( path len -- ret )
    c-function mknod mknod s n n -- n ( path mode dev -- ret )
    c-function mkstemp mkstemp a -- n ( c-addr -- n )
    c-function getcwd getcwd a u -- a ( c-addr u -- c-addr )
    c-function strlen strlen a -- n
    getentropy? [IF]
	\c #include <sys/random.h>
	c-function getentropy getentropy a n -- n ( buffer len -- n )
    [THEN]
    getrandom? [IF]
	\c #include <sys/random.h>
	c-function getrandom getrandom a n u -- n ( buffer len flag -- n )
    [THEN]
    c-function setenv setenv s s n -- n ( name un value uv overwrie -- n )
    c-function unsetenv unsetenv s -- n ( name u -- n )
    c-value environ environ -- a ( -- env )
end-c-library

host? [IF] getpagesize [ELSE] $400 [THEN] Value pagesize

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
    >r r@ events w!  r@ fd l!  0 r@ revents w!  r> pollfd + ;
: fds[]! ( fileno flag addr index -- )
    over $@len over 1+ pollfd * umax 2 pick $!len
    pollfd * swap $@ drop + tuck events w! fd l! ;

: ?errno-throw ( f -- )
    \ throw code computed from errno if f!=0
    IF  -512 errno - throw THEN ;

: ?ior ( x -- )
    \G use errno to generate throw when failing
    -1 = ?errno-throw ;

[defined] int-execute [if]
    variable saved-errno 0 saved-errno !
    : int-errno-exec r> { r } saved-errno @ ->errno defers int-execute
	errno saved-errno ! r >r ;
    host? [IF] ' int-errno-exec is int-execute [THEN]
[then]

: fd>file ( fd -- fid )
    s" w+" fdopen dup 0= ?errno-throw ;

host? [IF] (getpid) [ELSE] 0 [THEN] Value getpid

: fork() ( -- pid )
    (fork) (getpid) to getpid ;

Variable fpid

e? os-type s" linux-gnu" string-prefix? [IF]
: fork+exec ( filename len argv -- )
    >r fpid [ cell 8 = 1 pad ! pad c@ 0= and ] [IF] 4 + [THEN]
    -rot 0 0 r> environ posix_spawnp ?ior ;
[ELSE]
: fork+exec ( filename len argv -- )
    fork() dup 0= IF  drop ['] exit() is throw  execvp exit()
    ELSE  fpid ! drop 2drop  THEN ;
[THEN]

:noname defers 'cold
    ['] int-errno-exec is int-execute
    getpagesize to pagesize
    (getpid) to getpid ; is 'cold
