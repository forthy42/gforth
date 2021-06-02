\ i2c.fs	I²C access to Microchip EEPROMs
\
\ Authors: Bernd Paysan
\ Copyright (C) 2021 Free Software Foundation, Inc.

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

require arch/arm64/gpios.fs
require unix/i2c.fs

s" /dev/i2c-0" r/w open-file throw Value i2c-0-fd
s" /dev/i2c-1" r/w open-file throw Value i2c-1-fd
i2c-1-fd Value i2c-fd

[DEFINED] odroid-n2+ [DEFINED] odroid-n2 or [IF]
    : mux-i2c-0 ( -- )
	#1 #3 mux! #1 #5 mux! ;
    : mux-i2c-1 ( -- )
	#2 #27 mux! #2 #28 mux! ;
    : i2ctl ( msgs n -- )
	{ | msgbuf[ i2c_rdwr_ioctl_data ] }
	msgbuf[ i2c_rdwr_ioctl_data-nmsgs l!
	msgbuf[ i2c_rdwr_ioctl_data-msgs !
	i2c-fd fileno I2C_RDWR msgbuf[ ioctl ?ior ;
[THEN]

[IFUNDEF] alloz
    : alloz ( n -- )
	here swap dup allot erase ;
[THEN]

$12 buffer: i2c-writebuf \ 1 or 2 bytes command, rest write buffer
$10 buffer: i2c-readbuf
i2c_msg buffer: i2c-writemsg
i2c-writebuf i2c-writemsg i2c_msg-buf !

i2c_msg 2* buffer: i2c-readmsgs
i2c-readmsgs i2c_msg + Constant i2c-readmsg2

i2c-writebuf i2c-readmsgs i2c_msg-buf !
i2c-readbuf i2c-readmsg2 i2c_msg-buf !

: i2c-addr ( addr -- )
    \G specify device address
    dup i2c-writemsg i2c_msg-addr w!
    dup i2c-readmsgs i2c_msg-addr w!
    i2c-readmsg2 i2c_msg-addr w! ;

: i2c-writeb ( cmd len -- )
    swap i2c-writebuf c!
    1+ i2c_writemsg i2c_msg-len w!
    i2c-writemsg 1 i2ctl ;
: i2c-write2 ( cmd len -- )
    swap i2c-writebuf be-w!
    2 + i2c_writemsg i2c_msg-len w!
    i2c-writemsg 1 i2ctl ;
: i2c-readb ( cmd len -- )
    swap i2c-writebuf c!
    1 i2c_readmsgs i2c_msg-len w!
    i2c-rreadmsg2 i2c_msg-len w!
    i2c-readmsgs 2 i2ctl ;
: i2c-readw ( cmd len -- )
    swap i2c-writebuf be-w!
    2 i2c_readmsgs i2c_msg-len w!
    i2c-rreadmsg2 i2c_msg-len w!
    i2c-readmsgs 2 i2ctl ;

: i2c-c@ ( cmd -- byte )    1 i2c-readb  i2c-readbuf c@ ;
: i2c-w@ ( cmd -- word )    2 i2c-readb  i2c-readbuf w@ ;
: i2c-l@ ( cmd -- long )    4 i2c-readb  i2c-readbuf l@ ;
: i2c-x@ ( cmd -- extra )   8 i2c-readb  i2c-readbuf x@ ;
: i2cw-c@ ( cmd -- byte )   1 i2c-readw  i2c-readbuf c@ ;
: i2cw-w@ ( cmd -- word )   2 i2c-readw  i2c-readbuf w@ ;
: i2cw-l@ ( cmd -- long )   4 i2c-readw  i2c-readbuf l@ ;
: i2cw-x@ ( cmd -- extra )  8 i2c-readw  i2c-readbuf x@ ;

: i2c-c! ( byte cmd -- )   swap i2c-writebuf c!  1 i2c-writeb ;
: i2c-w! ( word cmd -- )   swap i2c-writebuf w!  2 i2c-writeb ;
: i2c-l! ( long cmd -- )   swap i2c-writebuf l!  4 i2c-writeb ;
: i2c-x! ( extra cmd -- )  swap i2c-writebuf x!  8 i2c-writeb ;
: i2cw-c! ( byte cmd -- )  swap i2c-writebuf c!  1 i2c-writew ;
: i2cw-w! ( word cmd -- )  swap i2c-writebuf w!  2 i2c-writew ;
: i2cw-l! ( long cmd -- )  swap i2c-writebuf l!  4 i2c-writew ;
: i2cw-x! ( extra cmd -- ) swap i2c-writebuf x!  8 i2c-writew ;
