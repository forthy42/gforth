\ socket interface

\ Copyright (C) 1998,2000,2003 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

require lib.fs
[IFUNDEF] libc  library libc libc.so.6  [THEN]

libc gethostbyname ptr (ptr) gethostbyname ( name -- hostent )
libc socket int int int (int) socket ( class type proto -- fd )
libc connect int ptr int (int) connect ( fd sock size -- err )
libc fdopen int ptr (ptr) fdopen ( fd fileattr -- file )
libc htonl int (int) htonl ( x -- x' )

4 4 2Constant int%

struct
    cell% field h_name
    cell% field h_aliases
    int% field h_addrtype
    int% field h_length
    cell% field h_addr_list
end-struct hostent

struct
    int% field family+port
    int% field sin_addr
    cell% 2* field padding
end-struct sockaddr_in

Create sockaddr-tmp
sockaddr-tmp sockaddr_in %size dup allot erase

: c-string ( addr u -- addr' )
    tuck pad swap move pad + 0 swap c! pad ;

: host>addr ( addr u -- x )
    \G converts a internet name into a IPv4 address
    \G the resulting address is in network byte order
    c-string gethostbyname dup 0= abort" address not found"
    h_addr_list @ @ @ ;

2 Constant PF_INET
1 Constant SOCK_STREAM
6 Constant IPPROTO_TCP

: open-socket ( addr u port -- fid )
    htonl PF_INET [ base c@ 0= ] [IF] $10 lshift [THEN]
    or sockaddr-tmp family+port ffi-i!
    host>addr sockaddr-tmp sin_addr ffi-i!
    PF_INET SOCK_STREAM IPPROTO_TCP socket
    dup 0<= abort" no free socket" >r
    r@ sockaddr-tmp $10 connect 0< abort" can't connect"
    r> s" w+" c-string fdopen ;
