\ socket interface

\ Copyright (C) 1998,2000,2003,2005,2006,2007 Free Software Foundation, Inc.

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

\c #include <netdb.h>
c-function gethostbyname gethostbyname a -- a ( name -- hostent )
\c #include <sys/types.h>
\c #include <sys/socket.h>
c-function socket socket n n n -- n ( class type proto -- fd )
c-function connect connect n a n -- n ( fd sock size -- err )
\c #include <stdio.h>
c-function fdopen fdopen n a -- a ( fd fileattr -- file )
\c #include <arpa/inet.h>
c-function htonl htonl n -- n ( x -- x' )
c-function htons htons n -- n ( x -- x' )
c-function ntohl ntohl n -- n ( x -- x' )

4 4 2Constant int%
2 2 2Constant short%

struct
    cell% field h_name
    cell% field h_aliases
    int% field h_addrtype
    int% field h_length
    cell% field h_addr_list
end-struct hostent

struct
    short% field family
    short% field port
    int% field sin_addr
    cell% 2* field padding
end-struct sockaddr_in

' family alias family+port \ 0.6.2 32-bit field; used by itools

Create sockaddr-tmp
sockaddr-tmp sockaddr_in %size dup allot erase

: c-string ( addr u -- addr' )
    tuck pad swap move pad + 0 swap c! pad ;

: host>addr ( addr u -- x )
    \G converts a internet name into a IPv4 address
    \G the resulting address is in network byte order
    c-string gethostbyname dup 0= abort" address not found"
    h_addr_list @ @ @ ntohl ;

2 Constant PF_INET
1 Constant SOCK_STREAM
6 Constant IPPROTO_TCP

: new-socket ( -- socket )
    PF_INET SOCK_STREAM IPPROTO_TCP socket
    dup 0<= abort" no free socket" ;

: >inetaddr ( ip port sockaddr -- ) >r
    r@ sockaddr_in %size erase
    PF_INET r@ family w!
    htons r@ port w!
    htonl r> sin_addr l! ;

: open-socket ( addr u port -- fid )
    -rot host>addr
    swap sockaddr-tmp >inetaddr
    new-socket >r
    r@ sockaddr-tmp sockaddr_in %size connect 0< abort" can't connect"
    r> s" w+" c-string fdopen ;
