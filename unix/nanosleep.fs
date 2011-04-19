\ nanosleep

\ Copyright (C) 2011 Free Software Foundation, Inc.

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

c-library nanosleep
    s" rt" add-lib
    \c #include <time.h>
    \c unsigned long gforth_nanosleep(unsigned long deltat) {
    \c struct timespec req;
    \c req.tv_sec = deltat/1000000000;
    \c req.tv_nsec = deltat%1000000000;
    \c nanosleep(&req, &req);
    \c return req.tv_sec*1000000000+req.tv_nsec;
    \c }
    \c unsigned long gforth_clock_nanosleep(unsigned long abst) {
    \c struct timespec req;
    \c req.tv_sec = abst/1000000000;
    \c req.tv_nsec = abst%1000000000;
    \c clock_nanosleep(CLOCK_MONOTONIC, TIMER_ABSTIME, &req, &req);
    \c return req.tv_sec*1000000000+req.tv_nsec;
    \c }
    \c unsigned long gforth_clock_gettime() {
    \c struct timespec req;
    \c clock_gettime(CLOCK_MONOTONIC, &req);
    \c return req.tv_sec*1000000000+req.tv_nsec;
    \c }
    \c unsigned long gforth_clock_getres() {
    \c struct timespec req;
    \c clock_getres(CLOCK_MONOTONIC, &req);
    \c return req.tv_sec*1000000000+req.tv_nsec;
    \c }
    
    c-function nanosleep gforth_nanosleep n -- n ( nsec -- left )
    c-function clock_nanosleep gforth_clock_nanosleep n -- n ( nsec -- left )
    c-function clock_gettime gforth_clock_gettime -- n ( -- nsec )
    c-function clock_getres gforth_clock_getres -- n ( -- nsec )
end-c-library