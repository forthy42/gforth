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

c-library filestat
    \c #include <sys/types.h>
    \c #include <sys/stat.h>
    \c #include <sys/time.h>
    \c #include <unistd.h>
    e? os-type s" linux-android" string-prefix? [IF]
	\ extern int futimens(int fd, const struct timespec times[2]);
	\c #include <sys/syscall.h>
	\c #include <sys/glibc-syscalls.h>
	\c int utimensat(int fd, const char *pathname,
	\c               const struct timespec ts[2], int flags) {
	\c   syscall(SYS_utimensat, fd, pathname, ts, flags);
	\c }
	\c int futimens(int fd, const struct timespec ts[2]) {
	\c   utimensat(fd, NULL, ts, 0);
	\c }
    [THEN]
    
    c-function stat stat a a -- n ( path buf -- r )
    c-function fstat fstat n a -- n ( fd buf -- r )
    c-function lstat lstat a a -- n ( path buf -- r )
    e? os-type 2dup s" darwin" string-prefix? -rot s" ios" str= or [IF]
	c-function utimes utimes a a -- n ( fd times -- r )
	c-function lutimes lutimes a a -- n ( fd times -- r )
	c-function futimes futimes n a -- n ( fd times -- r )
    [ELSE] \ linux stuff
	c-function utimensat utimensat n a a n -- n ( fd path times flags -- r )
	c-function futimens futimens n a -- n ( fd times -- r )
    [THEN]
    c-function chmod chmod a n -- n ( path mode -- r )
    c-function fchmod fchmod n n -- n ( fd mode -- r )
    c-function chown chown a n n -- n ( path uid git -- r )
    c-function fchown fchown n n n -- n ( fd uid git -- r )
    c-function lchown lchown a n n -- n ( path uid git -- r )
end-c-library

e? os-type s" darwin" string-prefix? [IF]
    : futimens ( a a -- r )  dup cell+ @ 1000 / over cell+ ! futimes ;
    : utimens ( a a -- r )  dup cell+ @ 1000 / over cell+ ! utimes ;
    : lutimens ( a a -- r )  dup cell+ @ 1000 / over cell+ ! lutimes ;
[ELSE]
    -100 Constant AT_FDCWD
    : utimens ( a a -- r )  AT_FDCWD -rot 0 utimensat ;
    : lutimens ( a a -- r ) AT_FDCWD -rot $100 utimensat ;
[THEN]

begin-structure file-stat
e? os-type s" darwin" string-prefix? [IF]
    cell 8 = [IF]
	drop 0 lfield: st_dev
	drop 8 8 +field st_ino
	drop 4 wfield: st_mode
	drop 16 lfield: st_uid
	drop 20 lfield: st_gid
	drop 24 lfield: st_rdev
	drop 96 8 +field st_size
	drop 112 lfield: st_blksize
	drop 104 8 +field st_blocks
	drop 32 16 +field st_atime
	drop 48 16 +field st_mtime
	drop 64 16 +field st_ctime
	drop 144
    [ELSE]
	drop 0 lfield: st_dev
	drop 8 8 +field st_ino
	drop 4 wfield: st_mode
	drop 16 lfield: st_uid
	drop 20 lfield: st_gid
	drop 24 lfield: st_rdev
	drop 60 8 +field st_size
	drop 76 lfield: st_blksize
	drop 68 8 +field st_blocks
	drop 28 8 +field st_atime
	drop 36 8 +field st_mtime
	drop 44 8 +field st_ctime
	drop 108
    [THEN]
[ELSE]
    cell 8 = [IF]
	drop 0 8 +field st_dev
	drop 8 field: st_ino
	drop 24 lfield: st_mode
	drop 28 lfield: st_uid
	drop 32 lfield: st_gid
	drop 40 8 +field st_rdev
	drop 48 field: st_size
	drop 56 field: st_blksize
	drop 64 field: st_blocks
	drop 72 16 +field st_atime
	drop 88 16 +field st_mtime
	drop 104 16 +field st_ctime
	drop 144
    [ELSE]
	drop 0 8 +field st_dev
	drop 12 field: st_ino
	drop 16 lfield: st_mode
	drop 24 lfield: st_uid
	drop 28 lfield: st_gid
	drop 32 8 +field st_rdev
	drop 44 field: st_size
	drop 48 field: st_blksize
	drop 52 field: st_blocks
	drop 56 8 +field st_atime
	drop 64 8 +field st_mtime
	drop 72 8 +field st_ctime
	drop 88
    [THEN]
[THEN]
end-structure

: ntime@ ( addr -- ud )  2@ 1000000000 um* rot 0 d+ ;
: utime@ ( addr -- ud )  2@ 1000000 um* rot 1000 / 0 d+ ;

: ntime! ( ud addr -- )  >r 1000000000 um/mod r> 2! ;
: utime! ( ud addr -- )  >r 1000000 um/mod >r 1000 * r> r> 2! ;
