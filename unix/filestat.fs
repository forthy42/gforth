\ file status and similar stuff                      04oct2013py

\ Copyright (C) 2012,2013,2014,2015 Free Software Foundation, Inc.

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

c-library stat
    \c #include <sys/types.h>
    \c #include <sys/stat.h>
    \c #include <sys/time.h>
    \c #include <unistd.h>
    e? os-type s" linux-android" string-prefix? [IF]
	\ extern int futimens(int fd, const struct timespec times[2]);
	\c #include <sys/syscall.h>
	\c int utimensat(int fd, const char *pathname,
	\c               const struct timespec ts[2], int flags) {
	\c   syscall(__NR_utimensat, fd, pathname, ts, flags);
	\c }
	\c int futimens(int fd, const struct timespec ts[2]) {
	\c   utimensat(fd, NULL, ts, 0);
	\c }
    [THEN]
    
    c-function stat stat s a -- n ( path len buf -- r )
    c-function fstat fstat n a -- n ( fd buf -- r )
    c-function lstat lstat s a -- n ( path len buf -- r )
    e? os-type 2dup s" darwin" string-prefix? -rot s" ios" str= or [IF]
	c-function utimes utimes s a -- n ( path len times -- r )
	c-function lutimes lutimes s a -- n ( path len times -- r )
	c-function futimes futimes n a -- n ( fd times -- r )
    [ELSE] \ linux stuff
	c-function utimensat utimensat n s a n -- n ( fd path len times flags -- r )
	c-function futimens futimens n a -- n ( fd times -- r )
    [THEN]
    c-function chmod chmod s n -- n ( path len mode -- r )
    c-function fchmod fchmod n n -- n ( fd mode -- r )
    c-function chown chown s n n -- n ( path len uid git -- r )
    c-function fchown fchown n n n -- n ( fd uid git -- r )
    c-function lchown lchown s n n -- n ( path len uid git -- r )
end-c-library

e? os-type s" darwin" string-prefix? [IF]
    : futimens ( a a -- r )  dup cell+ @ 1000 / over cell+ ! futimes ;
    : utimens ( a a -- r )  dup cell+ @ 1000 / over cell+ ! utimes ;
    : lutimens ( a a -- r )  dup cell+ @ 1000 / over cell+ ! lutimes ;
[ELSE]
    -100 Constant AT_FDCWD
    : utimens ( addr u a -- r )  >r AT_FDCWD -rot r> 0 utimensat ;
    : lutimens ( addr u a -- r ) >r AT_FDCWD -rot r> $100 utimensat ;
[THEN]

include ./stat.fs

base @ 8 base !

0170000 Constant S_IFMT \ bit mask for the file type bit field
0140000 Constant S_IFSOCK \ socket
0120000 Constant S_IFLNK \ symbolic link
0100000 Constant S_IFREG \ regular file
0060000 Constant S_IFBLK \ block device
0040000 Constant S_IFDIR \ directory
0020000 Constant S_IFCHR \ character device
0010000 Constant S_IFIFO \ FIFO

base !

: ntime@ ( addr -- ud )  2@ 1000000000 um* rot 0 d+ ;
: utime@ ( addr -- ud )  2@ 1000000 um* rot 1000 / 0 d+ ;

: ntime! ( ud addr -- )  >r 1000000000 um/mod r> 2! ;
: utime! ( ud addr -- )  >r 1000000 um/mod >r 1000 * r> r> 2! ;
