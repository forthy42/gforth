\ socket interface

\ Copyright (C) 1998 Free Software Foundation, Inc.

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

1 (int) libc gethostbyname gethostbyname ( name -- hostent )
3 (int) libc socket socket ( class type proto -- fd )
3 (int) libc connect connect ( fd sock size -- err )
2 (int) libc fdopen fdopen ( fd fileattr -- file )
1 (int) libc htonl htonl ( x -- x' )

struct
    cell% field h_name
    cell% field h_aliases
    cell% field h_addrtype
    cell% field h_length
    cell% field h_addr_list
end-struct hostent

struct
    cell% field family+port
    cell% field sin_addr
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
    or sockaddr-tmp family+port !
    host>addr sockaddr-tmp sin_addr !
    PF_INET SOCK_STREAM IPPROTO_TCP socket
    dup 0<= abort" no free socket" >r
    r@ sockaddr-tmp $10 connect 0< abort" can't connect"
    r> s" w+" c-string fdopen ;
