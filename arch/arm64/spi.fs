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

Create readbuf  $3 c, 0 c, $10 allot
Create writebuf $2 c, 0 c, $10 allot

Create spi-rd-msgs
spi_ioc_transfer 2* alloz

spi-rd-msgs spi_ioc_transfer + Constant spi-rd-msg2

spi-hz spi-rd-msgs spi_ioc_transfer-speed_hz l!
readbuf spi-rd-msgs spi_ioc_transfer-tx_buf !
2 spi-rd-msgs spi_ioc_transfer-len l!
spi-hz spi-rd-msg2 spi_ioc_transfer-speed_hz l!
readbuf 2 + spi-rd-msg2 spi_ioc_transfer-rx_buf !
8 spi-rd-msgs spi_ioc_transfer-bits_per_word c!
8 spi-rd-msg2 spi_ioc_transfer-bits_per_word c!

: spi-c@ ( addr -- byte )
    readbuf 1+ c!
    1 spi-rd-msg2 spi_ioc_transfer-len l!
    2 spi-rd-msgs spioctl
    readbuf 2 + c@ ;

Create spi-wr-msgs
spi_ioc_transfer alloz

spi-hz spi-wr-msgs spi_ioc_transfer-speed_hz l!
8 spi-wr-msgs spi_ioc_transfer-bits_per_word c!

Create stbuf $5 c, 0 c,

Create spi-st-msgs
spi_ioc_transfer 2* alloz

spi-st-msgs spi_ioc_transfer + Constant spi-st-msg2

spi-hz spi-st-msgs spi_ioc_transfer-speed_hz l!
stbuf spi-st-msgs spi_ioc_transfer-tx_buf !
1 spi-st-msgs spi_ioc_transfer-len l!
spi-hz spi-st-msg2 spi_ioc_transfer-speed_hz l!
stbuf spi-st-msg2 spi_ioc_transfer-rx_buf !
1 spi-st-msg2 spi_ioc_transfer-len l!
8 spi-st-msgs spi_ioc_transfer-bits_per_word c!
8 spi-st-msg2 spi_ioc_transfer-bits_per_word c!

: spi-status@ ( -- status )
    $5 stbuf c!
    2 spi-st-msgs spioctl
    stbuf 1+ c@ ;

: spi-wren ( -- )
    $6 stbuf c!
    1 spi-st-msgs spioctl ;
: spi-wrdi ( -- )
    $4 stbuf c!
    1 spi-st-msgs spioctl ;
