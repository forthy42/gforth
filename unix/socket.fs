\ socket interface

\ Copyright (C) 1998,2000,2003,2005,2006,2007,2008 Free Software Foundation, Inc.

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

c-library socket
\c #include <netdb.h>
c-function gethostbyname gethostbyname a -- a ( name -- hostent )
\c #include <unistd.h>
c-function gethostname gethostname a n -- n ( c-addr u -- ior )
\c #include <errno.h>
\c #define get_errno() errno
c-function errno get_errno -- n ( -- value )
\c #include <sys/types.h>
\c #include <sys/socket.h>
c-function socket socket n n n -- n ( class type proto -- fd )
c-function closesocket close n -- n ( fd -- ior )
c-function connect connect n a n -- n ( fd sock size -- err )
c-function send send n a n n -- n ( socket buffer count flags -- size )
c-function recv recv n a n n -- n ( socket buffer count flags -- size )
c-function listen() listen n n -- n ( socket backlog -- err )
c-function bind bind n a n -- n ( socket sockaddr socklen --- err )
c-function accept() accept n a a -- n ( socket sockaddr addrlen -- fd )
\c #include <stdio.h>
c-function fdopen fdopen n a -- a ( fd fileattr -- file )
\c #include <fcntl.h>
c-function fcntl fcntl n n n -- n ( fd n1 n2 -- ior )
\c #include <arpa/inet.h>
c-function htonl htonl n -- n ( x -- x' )
c-function htons htons n -- n ( x -- x' )
c-function ntohl ntohl n -- n ( x -- x' )
\c #define fileno1(file) fileno((FILE*)(file))
c-function fileno fileno1 a -- n ( file* -- fd )
end-c-library

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
[ s" os-type" environment? drop s" cygwin" str= ] [IF]
    &12 +
[ELSE]
    h_addr_list
[THEN]
    @ @ @ ntohl ;

   2 Constant PF_INET
   1 Constant SOCK_STREAM
   6 Constant IPPROTO_TCP
   4 Constant F_SETFL
  11 Constant EWOULDBLOCK
$100 Constant MSG_WAITALL
$802 Constant O_NONBLOCK|O_RDWR
2000 Value    SOCKET-TIMEOUT

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

: create-server  ( port# -- lsocket )
    sockaddr-tmp 4 CELLS ERASE
    htonl PF_INET OR sockaddr-tmp !
    PF_INET SOCK_STREAM 0 socket
    dup 0< abort" no free socket" >r
    r@ sockaddr-tmp 16 bind 0= IF  r> exit  ENDIF
    r> drop true abort" bind :: failed" ;

\ from itools.frt

' open-socket Alias open-service

: ms@  utime 1000 um/mod nip ; ( -- u ) 

: $put ( c-addr1 u1 c-addr2 -- ) swap cmove ;

: $+ 	( c-addr1 u1 c-addr2 u2 -- c-addr3 u3 )
    { c-addr1 u1 c-addr2 u2 }
    u1 u2 + allocate throw 
    c-addr1 u1  2 pick       $put 
    c-addr2 u2  2 pick u1 +  $put  
    u1 u2 + ;

Create hostname$ 0 c, 255 chars allot
Create alen   16 ,
Create crlf 2 c, 13 c, 10 c,

: listen ( lsocket /queue -- )
    listen() 0< abort" listen :: failed" ;

\ This call blocks the server until a client appears. The client uses socket to
\ converse with the server.
: accept-socket ( lsocket -- socket )
    16 alen !
    sockaddr-tmp alen accept() 
    dup 0< IF  errno cr ." accept() :: error #" .  
	abort" accept :: failed"  
    ENDIF   s" w+" c-string fdopen ;

: +cr  ( c-addr1 u1 -- c-addr2 u2 ) crlf count $+ ;

: blocking-mode ( socket flag -- ) >r fileno
    f_setfl r> IF  0  
    ELSE  o_nonblock|o_rdwr  
    THEN  
    fcntl 0< abort" blocking-mode failed" ;

: hostname ( -- c-addr u )
    hostname$ c@ 0= IF
	hostname$ 1+ 255 gethostname drop
	hostname$ 1+ 255 0 scan nip 255 swap - hostname$ c!
    THEN
    hostname$ count ;
: set-socket-timeout ( u -- ) 200 + to socket-timeout ;
: get-socket-timeout ( -- u ) socket-timeout 200 - ;
: write-socket ( c-addr size socket -- ) fileno -rot 0 send 0< throw ;
: close-socket ( socket -- ) fileno closesocket drop ;

: (rs)  ( socket c-addr maxlen -- c-addr size ) 
    2 pick >r r@ false blocking-mode  rot fileno -rot
    over >r msg_waitall recv
    dup 0<  IF  0 max
	errno dup 0<> swap ewouldblock <> and abort" (rs) :: socket read error"
    THEN
    r> swap
    r> true blocking-mode ;

: read-socket ( socket c-addr maxlen -- c-addr u )
    ms@ socket-timeout + { socket c-addr maxlen tmax -- c-addr size }
    BEGIN 
	socket c-addr maxlen (rs) dup 0=
	ms@ tmax u< and 
    WHILE 
	    2drop
    REPEAT ;
