\ socket interface

\ Copyright (C) 1998,2000,2003,2005,2006,2007,2008,2009,2010,2011 Free Software Foundation, Inc.

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
c-function recvfrom recvfrom n a n n a a -- n ( socket buffer count flags srcaddr addrlen -- size )
c-function sendto sendto n a n n a n -- n ( socket buffer count flags srcaddr addrlen -- size )
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
\c #include <poll.h>
c-function poll poll a n n -- n ( fds nfds timeout -- r )
e? os-type s" linux-gnu" str= [IF]
    c-function ppoll ppoll a n a a -- n ( fds nfds timeout_ts sigmask -- r )
    \c #include <sys/epoll.h>
    c-function epoll_create epoll_create n -- n ( n -- epfd )
    c-function epoll_ctl epoll_ctl n n n a -- n ( epfd op fd event -- r )
    c-function epoll_wait epoll_wait n a n n -- n ( epfd events maxevs timeout -- r )
    s" uname -r" r/o open-pipe throw
    dup >r slurp-fid r> close-pipe throw drop
    2dup s" 3." string-prefix? >r drop free throw r> [IF]
	c-function recvmmsg recvmmsg n a n n a -- n ( sockfd hdr vlen flag timeout -- r )
	c-function sendmmsg sendmmsg n a n n -- n ( sockfd hdr vlen flag -- r )
    [THEN]
[THEN]
\c #include <netdb.h>
c-function getaddrinfo getaddrinfo a a a a -- n ( node service hints res -- r )
c-function freeaddrinfo freeaddrinfo a -- void ( res -- )
c-function gai_strerror gai_strerror n -- a ( errcode -- addr )
c-function setsockopt setsockopt n n n a n -- n ( sockfd level optname optval optlen -- r )
end-c-library

e? os-type s" darwin" string-prefix? [IF] : darwin ; [THEN]
e? os-type s" linux-android" str= [IF] : android ; [THEN]

4 4 2Constant int%
2 2 2Constant short%
int% 2Constant size_t%

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
    int% 2* field padding
end-struct sockaddr_in4

struct
    short% field sin6_family
    short% field sin6_port
    int% field sin6_flowinfo
    int% 4 * field sin6_addr
    int% field sin6_scope_id
end-struct sockaddr_in6

sockaddr_in4 %alignment sockaddr_in6 %alignment max
sockaddr_in4 %size sockaddr_in6 %size max 2Constant sockaddr_in

struct
    int% field fd
    short% field events
    short% field revents
end-struct pollfd

struct
    int% field ai_flags
    int% field ai_family
    int% field ai_socktype
    int% field ai_protocol
    size_t% field ai_addrlen
[defined] android [defined] darwin or [IF]
    cell% field ai_canonname
    cell% field ai_addr
[ELSE]
    cell% field ai_addr
    cell% field ai_canonname
[THEN]
    cell% field ai_next
end-struct addrinfo

e? os-type s" linux" string-prefix? [IF]
    struct
	cell% field iov_base
	cell% field iov_len
    end-struct iovec
    struct
	cell% field msg_name
	cell% field msg_namelen
	cell% field msg_iov \ iovec structures
	cell% field msg_iovlen
	cell% field msg_control
	cell% field msg_controllen
	cell% field msg_flags
	cell% field msg_len
    end-struct mmsghdr
[THEN]

' family alias family+port \ 0.6.2 32-bit field; used by itools

Create sockaddr-tmp
sockaddr-tmp sockaddr_in %size dup allot erase
Create hints
hints addrinfo %size dup allot erase
Variable addrres
Variable sockopt-on

: c-string ( addr u -- addr' )
    tuck pad swap move pad + 0 swap c! pad ;

     0 Constant PF_UNSPEC
     2 Constant PF_INET
[IFDEF] darwin
    30 Constant PF_INET6
 $0210 Constant AF_INET
 $1E1C Constant AF_INET6
    27 Constant IPV6_V6ONLY
    35 Constant EWOULDBLOCK
   $40 Constant MSG_WAITALL
   $80 Constant MSG_DONTWAIT
  $006 Constant O_NONBLOCK|O_RDWR
 $1006 Constant SO_RCVTIMEO
     4 Constant SO_REUSEADDR
 $FFFF Constant SOL_SOCKET
[ELSE]
    10 Constant PF_INET6
     2 Constant AF_INET
    10 Constant AF_INET6
    26 Constant IPV6_V6ONLY
    11 Constant EWOULDBLOCK
   $40 Constant MSG_DONTWAIT
  $100 Constant MSG_WAITALL
$10000 Constant MSG_WAITFORONE
  $802 Constant O_NONBLOCK|O_RDWR
    20 Constant SO_RCVTIMEO
     2 Constant SO_REUSEADDR
     1 Constant SOL_SOCKET
[THEN]
   1 Constant SOCK_STREAM
   2 Constant SOCK_DGRAM
   0 Constant IPPROTO_IP
  41 Constant IPPROTO_IPV6
  10 Constant IP_MTU_DISCOVER
  23 Constant IPV6_MTU_DISCOVER
  67 Constant IP_DONTFRAG
   2 Constant IP_PMTUDISC_DO
   4 Constant F_SETFL
$001 Constant POLLIN
$002 Constant POLLPRI
$004 Constant POLLOUT

2variable socket-timeout-d 2000. socket-timeout-d 2!

: new-socket ( -- socket )
    PF_INET SOCK_STREAM 0 socket
    dup 0<= abort" no free socket" ;

: new-socket6 ( -- socket )
    PF_INET6 SOCK_STREAM 0 socket
    dup 0<= abort" no free socket"
    dup IPPROTO_IPV6 IPV6_V6ONLY sockopt-on dup on 4 setsockopt drop ;

: new-udp-socket ( -- socket )
    PF_INET SOCK_DGRAM 0 socket
    dup 0<= abort" no free socket"
[IFDEF] darwin
\    dup IPPROTO_IP IP_DONTFRAG sockopt-on 1 over l! 4
\    setsockopt drop
[ELSE]
    dup IPPROTO_IP IP_MTU_DISCOVER sockopt-on IP_PMTUDISC_DO over l! 4
    setsockopt drop
[THEN] ;

: new-udp-socket6 ( -- socket )
    PF_INET6 SOCK_DGRAM 0 socket
    dup 0<= abort" no free socket"
[IFDEF] darwin
\    dup IPPROTO_IP IP_DONTFRAG sockopt-on 1 over l! 4
\    setsockopt drop
[ELSE]
    dup IPPROTO_IPV6 IPV6_MTU_DISCOVER sockopt-on IP_PMTUDISC_DO over l! 4
    setsockopt drop
[THEN]
    dup IPPROTO_IPV6 IPV6_V6ONLY sockopt-on dup on 4 setsockopt drop ;

: new-udp-socket46 ( -- socket )
    PF_INET6 SOCK_DGRAM 0 socket
    dup 0<= abort" no free socket"
[IFDEF] darwin
\    dup IPPROTO_IP IP_DONTFRAG sockopt-on 1 over l! 4
\    setsockopt drop
[ELSE]
    dup IPPROTO_IPV6 IPV6_MTU_DISCOVER sockopt-on IP_PMTUDISC_DO over l! 4
    setsockopt drop
[THEN]
;

\ getaddrinfo based open-socket

: >hints ( socktype -- )
    hints addrinfo %size erase
    PF_UNSPEC hints ai_family l!
    hints ai_socktype l! ;

: get-info ( addr u port -- info )
    base @ >r  decimal  0 <<# 0 hold #s #>  r> base ! drop
    >r c-string r> hints addrres getaddrinfo #>>
    ?dup IF
	gai_strerror cstring>sstring type
	true abort" getaddrinfo failed"  THEN
    addrres @ ;

: get-socket ( info -- socket )  dup >r >r
    BEGIN  r@  WHILE
	    r@ ai_family l@ r@ ai_socktype l@ r@ ai_protocol l@ socket
	    dup 0>= IF
		dup r@ ai_addr @ r@ ai_addrlen l@ connect
		IF
		    closesocket drop
		ELSE
		    s" w+" c-string fdopen
		    rdrop r> freeaddrinfo  EXIT
		THEN
	    ELSE  drop  THEN
	    r> ai_next @ >r  REPEAT
    rdrop r> freeaddrinfo true abort" can't connect" ;

: open-socket ( addr u port -- fid )
    SOCK_STREAM >hints  get-info  get-socket ;

: open-udp-socket ( addr u port -- fid )
    SOCK_DGRAM >hints  get-info  get-socket ;

: create-server  ( port# -- lsocket )
    sockaddr-tmp sockaddr_in %size erase
    AF_INET sockaddr-tmp family w!
    htons   sockaddr-tmp port w!
    new-socket
    dup 0< abort" no free socket" >r
    r@ SOL_SOCKET SO_REUSEADDR sockopt-on 1 over ! 4 setsockopt drop
    r@ sockaddr-tmp sockaddr_in4 %size bind 0= IF  r> exit  ENDIF
    r> drop true abort" bind :: failed" ;

: create-server6  ( port# -- lsocket )
    sockaddr-tmp sockaddr_in %size erase
    AF_INET6 sockaddr-tmp family w!
    htons   sockaddr-tmp port w!
    new-socket6
    dup 0< abort" no free socket" >r
    r@ sockaddr-tmp sockaddr_in6 %size bind 0= IF  r> exit  ENDIF
    r> drop true abort" bind :: failed" ;

: create-udp-server  ( port# -- lsocket )
    sockaddr-tmp sockaddr_in %size erase
    AF_INET sockaddr-tmp family w!
    htons   sockaddr-tmp port w!
    new-udp-socket
    dup 0< abort" no free socket" >r
    r@ sockaddr-tmp sockaddr_in4 %size bind 0= IF  r> exit  ENDIF
    r> drop true abort" bind :: failed" ;

: create-udp-server6  ( port# -- lsocket )
    sockaddr-tmp sockaddr_in6 %size erase
    AF_INET6 sockaddr-tmp family w!
    htons   sockaddr-tmp port w!
    new-udp-socket6
    dup 0< abort" no free socket" >r
    r@ sockaddr-tmp sockaddr_in6 %size bind 0= IF  r> exit  ENDIF
    r> drop true abort" bind :: failed" ;

: create-udp-server46  ( port# -- lsocket )
    sockaddr-tmp sockaddr_in6 %size erase
    AF_INET6 sockaddr-tmp family w!
    htons   sockaddr-tmp port w!
    new-udp-socket46
    dup 0< abort" no free socket" >r
    r@ sockaddr-tmp sockaddr_in6 %size bind 0= IF  r> exit  ENDIF
    r> drop true abort" bind :: failed" ;

\ from itools.frt

' open-socket Alias open-service

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
: set-socket-timeout ( u -- ) 200 + s>d socket-timeout-d 2! ;
: get-socket-timeout ( -- u ) socket-timeout-d 2@ drop 200 - ;
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
    utime socket-timeout-d 2@ d+ { socket c-addr maxlen d: tmax -- c-addr size }
    BEGIN 
	socket c-addr maxlen (rs) dup 0=
	utime tmax d< and 
    WHILE 
	    2drop
    REPEAT ;

: (rs-from)  ( socket c-addr maxlen -- c-addr size ) 
    2 pick >r  r@ false blocking-mode  rot fileno -rot
    over >r msg_waitall sockaddr-tmp alen  recvfrom
    dup 0<  IF  0 max
	errno dup 0<> swap ewouldblock <> and abort" (rs) :: socket read error"
    THEN
    r> swap
    r> true blocking-mode ;

: read-socket-from ( socket c-addr maxlen -- c-addr u )
    utime socket-timeout-d 2@ d+ { socket c-addr maxlen d: tmax -- c-addr size }
    BEGIN 
	socket c-addr maxlen (rs-from) dup 0=
	utime tmax d< and 
    WHILE 
	    2drop
    REPEAT ;
