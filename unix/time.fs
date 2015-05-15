\ time interface

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

c-library time
    \c #include <time.h>
    c-function localtime_r localtime_r a a -- a ( time-addr tmaddr -- tmaddr )
    c-function tzset tzset -- void
    c-value tzname tzname -- a ( -- tzname[2] )
    c-value timezone timezone -- n ( -- n )
    c-value daylight daylight -- n ( -- n )
end-c-library

begin-structure tm
    lfield: tm_sec
    lfield: tm_min
    lfield: tm_hour
    lfield: tm_mday
    lfield: tm_mon
    lfield: tm_year
    lfield: tm_wday
    lfield: tm_yday
    lfield: tm_isdst
    field: tm_gmtoff
    field: tm_zone
end-structure

tm buffer: tm1

: >date&time ( sec -- sec min hour mday mon year )
    { w^ sec } sec tm1 localtime_r drop
    [ tm1 tm_sec  ]L l@
    [ tm1 tm_min  ]L l@
    [ tm1 tm_hour ]L l@
    [ tm1 tm_mday ]L l@
    [ tm1 tm_mon  ]L l@ 1+
    [ tm1 tm_year ]L l@ 1900 + ;

: tz? ( daylight -- addr u )
    0<> negate >r tzname r> cells + @ cstring>sstring ;

tzset \ needs to run once