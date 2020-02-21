\ wrapper to load Swig-generated libraries

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2016,2017,2018,2019 Free Software Foundation, Inc.

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

cs-vocabulary gps \ needs to be case sensitive
get-current also gps definitions

include unix/gps.fs

set-current

gps_data_t buffer: gps-data
GPSD_API_MAJOR_VERSION 7 >= [IF]
    #33 cells  buffer: gps-message
[THEN]

: gps-local-open ( -- flag )
    s" shared memory" s" 2947" gps-data gps_open ;

: gps-fix ( -- addr )
    gps-data
    [ GPSD_API_MAJOR_VERSION 7 >= ] [IF] gps-message #33 cells [THEN]
    gps_read drop gps-data gps_data_t-fix ;

previous
