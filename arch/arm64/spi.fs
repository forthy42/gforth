\ spi.fs	SPI access
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
require unix/spi.fs

: _IOC ( dir type nr size -- constant )
    >r swap 8 lshift or r> $10 lshift or swap $1E lshift or ;
: SPI_IOC_MESSAGE ( n -- constant )
    >r 1 SPI_IOC_MAGIC 0 r> spi_ioc_transfer * _IOC ;
: SPI_IOC_WR_MAX_SPEED_HZ ( -- constant )
    1 SPI_IOC_MAGIC 4 4 _IOC ;
: SPI_IOC_RD_MAX_SPEED_HZ ( -- constant )
    2 SPI_IOC_MAGIC 4 4 _IOC ;

s" /dev/spidev0.0" r/w open-file throw Value spi-fd

[DEFINED] odroid-n2+ [DEFINED] odroid-n2 or [IF]
    : mux-spi ( -- )
	#4 #19 mux!  #4 #21 mux!  #4 #23 mux!
	#22 output-pin  #24 output-pin  #26 output-pin
	#22 pinset  #24 pinset  #26 pinset ;
    : spioctl ( n buf -- )
	#24 pinclr
	>r >r spi-fd fileno r> SPI_IOC_MESSAGE r> ioctl #24 pinset ?ior ;
[THEN]

: alloz ( n -- )
    here swap dup allot erase ;

#3000000 constant spi-hz

1 Constant MC-WRSR
2 Constant MC-WRITE
3 Constant MC-READ
4 Constant MC-WRDI
5 Constant MC-RDSR
6 Constant MC-WREN

$20 buffer: pagebuf
Create readbuf  MC-READ  c, 0 w,
Create writebuf MC-WRITE c, 0 w,

Create spi-rd-msgs
spi_ioc_transfer 2* alloz

spi-rd-msgs spi_ioc_transfer + Constant spi-rd-msg2

spi-hz spi-rd-msgs spi_ioc_transfer-speed_hz l!
readbuf spi-rd-msgs spi_ioc_transfer-tx_buf !
2 spi-rd-msgs spi_ioc_transfer-len l!
spi-hz spi-rd-msg2 spi_ioc_transfer-speed_hz l!
pagebuf spi-rd-msg2 spi_ioc_transfer-rx_buf !
8 spi-rd-msgs spi_ioc_transfer-bits_per_word c!
8 spi-rd-msg2 spi_ioc_transfer-bits_per_word c!

: spi-readb ( addr len -- )
    spi-rd-msg2 spi_ioc_transfer-len l!
    2 spi-rd-msgs spi_ioc_transfer-len l!
    readbuf 1+ c!
    2 spi-rd-msgs spioctl ;
: spi-readw ( addr len -- )
    spi-rd-msg2 spi_ioc_transfer-len l!
    3 spi-rd-msgs spi_ioc_transfer-len l!
    readbuf 1+ be-w!
    2 spi-rd-msgs spioctl ;
: spi-c@ ( addr -- byte )    1 spi-readb  pagebuf c@ ;
: spi-w@ ( addr -- word )    2 spi-readb  pagebuf w@ ;
: spi-l@ ( addr -- long )    4 spi-readb  pagebuf l@ ;
: spi-x@ ( addr -- extra )   8 spi-readb  pagebuf x@ ;
: spiw-c@ ( addr -- byte )   1 spi-readw  pagebuf c@ ;
: spiw-w@ ( addr -- word )   2 spi-readw  pagebuf w@ ;
: spiw-l@ ( addr -- long )   4 spi-readw  pagebuf l@ ;
: spiw-x@ ( addr -- extra )  8 spi-readw  pagebuf x@ ;

Create stbuf MC-RDSR c, 0 c,

Create spi-st-msgs
spi_ioc_transfer 2* alloz

spi-st-msgs spi_ioc_transfer + Constant spi-st-msg2

spi-hz spi-st-msgs spi_ioc_transfer-speed_hz l!
stbuf spi-st-msgs spi_ioc_transfer-tx_buf !
1 spi-st-msgs spi_ioc_transfer-len l!
spi-hz spi-st-msg2 spi_ioc_transfer-speed_hz l!
stbuf 1+ spi-st-msg2 spi_ioc_transfer-rx_buf !
1 spi-st-msg2 spi_ioc_transfer-len l!
8 spi-st-msgs spi_ioc_transfer-bits_per_word c!
8 spi-st-msg2 spi_ioc_transfer-bits_per_word c!

: spi-status@ ( -- status )
    MC-RDSR stbuf c!
    2 spi-st-msgs spioctl
    stbuf 1+ c@ ;
: spi-status! ( status -- )
    MC-WRSR stbuf c!  stbuf 1+ c!
    2 spi-st-msgs spi_ioc_transfer-len l!
    1 spi-st-msgs spioctl
    1 spi-st-msgs spi_ioc_transfer-len l! ;

: spi-wren ( -- )
    MC-WREN stbuf c!
    1 spi-st-msgs spioctl ;
: spi-wrdi ( -- )
    MC-WRDI stbuf c!
    1 spi-st-msgs spioctl ;

: spi-wip| ( -- )
    BEGIN  spi-status@ 1 and 0=  UNTIL ;

Create spi-wr-msgs
spi_ioc_transfer 2* alloz

spi-wr-msgs spi_ioc_transfer + Constant spi-wr-msg2

spi-hz spi-wr-msgs spi_ioc_transfer-speed_hz l!
8 spi-wr-msgs spi_ioc_transfer-bits_per_word c!
writebuf spi-wr-msgs spi_ioc_transfer-tx_buf !
spi-hz spi-wr-msg2 spi_ioc_transfer-speed_hz l!
8 spi-wr-msg2 spi_ioc_transfer-bits_per_word c!
pagebuf spi-wr-msg2 spi_ioc_transfer-tx_buf !

: spi-writeb ( addr len -- )
    2 spi-wr-msgs spi_ioc_transfer-len l!
    spi-wr-msg2 spi_ioc_transfer-len l!
    writebuf 1+ c!
    2 spi-wr-msgs spioctl spi-wip| ;
: spi-writew ( addr len -- )
    3 spi-wr-msgs spi_ioc_transfer-len l!
    spi-wr-msg2 spi_ioc_transfer-len l!
    writebuf 1+ be-w!
    2 spi-wr-msgs spioctl spi-wip| ;
: spi-c! ( byte addr -- )   swap pagebuf c!  1 spi-writeb ;
: spi-w! ( word addr -- )   swap pagebuf w!  2 spi-writeb ;
: spi-l! ( long addr -- )   swap pagebuf l!  4 spi-writeb ;
: spi-x! ( extra addr -- )  swap pagebuf x!  8 spi-writeb ;
: spiw-c! ( byte addr -- )  swap pagebuf c!  1 spi-writew ;
: spiw-w! ( word addr -- )  swap pagebuf w!  2 spi-writew ;
: spiw-l! ( long addr -- )  swap pagebuf l!  4 spi-writew ;
: spiw-x! ( extra addr -- ) swap pagebuf x!  8 spi-writew ;
