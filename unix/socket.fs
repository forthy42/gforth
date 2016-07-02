\ socket interface

\ Copyright (C) 1998,2000,2003,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014,2015 Free Software Foundation, Inc.

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
    \c #include <sys/types.h>
    \c #include <sys/socket.h>
    c-function socket socket n n n -- n ( class type proto -- fd )
    c-function connect connect n a n -- n ( fd sock size -- err )
    c-function send send n a n n -- n ( socket buffer count flags -- size )
    c-function recv recv n a n n -- n ( socket buffer count flags -- size )
    c-function recvfrom recvfrom n a n n a a -- n ( socket buffer count flags srcaddr addrlen -- size )
    c-function sendto sendto n a n n a n -- n ( socket buffer count flags srcaddr addrlen -- size )
    c-function listen() listen n n -- n ( socket backlog -- err )
    c-function bind bind n a n -- n ( socket sockaddr socklen --- err )
    c-function accept() accept n a a -- n ( socket sockaddr addrlen -- fd )
    \c #include <arpa/inet.h>
    c-function htonl htonl n -- n ( x -- x' )
    c-function htons htons n -- n ( x -- x' )
    c-function ntohl ntohl n -- n ( x -- x' )
    \c #include <netdb.h>
    c-function getaddrinfo getaddrinfo a a a a -- n ( node service hints res -- r )
    c-function freeaddrinfo freeaddrinfo a -- void ( res -- )
    c-function gai_strerror gai_strerror n -- a ( errcode -- addr )
    c-function setsockopt setsockopt n n n a n -- n ( sockfd level optname optval optlen -- r )
    c-function getsockname getsockname  n a a -- n ( sockfd addr *len -- r )
    e? os-type s" linux-android" string-prefix? 0= [IF]
	\c #include <ifaddrs.h>
	c-function getifaddrs getifaddrs a -- n ( ifap -- r )
	c-function freeifaddrs freeifaddrs a -- void ( ifa -- )
    [THEN]
end-c-library

require libc.fs

' close alias closesocket

e? os-type s" darwin" string-prefix? [IF] [IFUNDEF] darwin : darwin ; [THEN] [THEN]
e? os-type s" bsd" search nip nip [IF] [IFUNDEF] darwin : darwin ; [THEN]
    [IFUNDEF] bsd : bsd ; [THEN] [THEN]
e? os-type s" linux-android" string-prefix? [IF] [IFUNDEF] android : android ; [THEN] [THEN]
e? os-type s" cygwin" string-prefix? [IF] [IFUNDEF] cygwin : cygwin ; [THEN] [THEN]
e? os-type s" linux-gnu" string-prefix? [IF] [IFUNDEF] linux : linux ; [THEN] [THEN]

begin-structure hostent
    field: h_name
    field: h_aliases
    lfield: h_addrtype
    lfield: h_length
    field: h_addr_list
end-structure

begin-structure sockaddr_in4
    wfield: family
    wfield: port
    lfield: sin_addr
    8 + \ padding
end-structure

begin-structure sockaddr_in6
    wfield: sin6_family
    wfield: sin6_port
    lfield: sin6_flowinfo
    $10 +field sin6_addr
    lfield: sin6_scope_id
end-structure

sockaddr_in4 sockaddr_in6 max Constant sockaddr_in

begin-structure addrinfo
    lfield: ai_flags
    lfield: ai_family
    lfield: ai_socktype
    lfield: ai_protocol
    field: ai_addrlen
[defined] linux [IF] \ linux has it the wrong way round
    field: ai_addr
    field: ai_canonname
[ELSE]
    field: ai_canonname
    field: ai_addr
[THEN]
    field: ai_next
end-structure

begin-structure ifaddrs
    field: ifa_next
    field: ifa_name
    lfield: ifa_flags
    field: ifa_addr
    field: ifa_netmask
    field: ifa_ifu
    field: ifa_data
end-structure

e? os-type s" linux" string-prefix? [IF]
    begin-structure iovec
	field: iov_base
	field: iov_len
    end-structure
    begin-structure mmsghdr
	field: msg_name
	field: msg_namelen
	field: msg_iov \ iovec structures
	field: msg_iovlen
	field: msg_control
	field: msg_controllen
	field: msg_flags
	field: msg_len
    end-structure
[THEN]

' family alias family+port \ 0.6.2 32-bit field; used by itools

Create sockaddr-tmp
sockaddr-tmp sockaddr_in dup allot erase
Create hints
hints addrinfo dup allot erase

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
    35 Constant EAGAIN
    46 Constant EPFNOSUPPORT
    EPFNOSUPPORT 1 + Constant EAFNOSUPPORT
    EPFNOSUPPORT 3 + Constant EADDRNOTAVAIL
    EPFNOSUPPORT 5 + Constant ENETUNREACH
[ELSE]
    [IFDEF] Cygwin
    23 Constant PF_INET6
     2 Constant AF_INET
    23 Constant AF_INET6
    27 Constant IPV6_V6ONLY
    11 Constant EWOULDBLOCK
    $1 Constant MSG_OOB
    $2 Constant MSG_PEEK
    $4 Constant MSG_DONTROUTE
    $8 Constant MSG_WAITALL
   $10 Constant MSG_DONTWAIT
 $4002 Constant O_NONBLOCK|O_RDWR
 $1006 Constant SO_RCVTIMEO
 $0004 Constant SO_REUSEADDR
 $FFFF Constant SOL_SOCKET
    11 Constant EAGAIN
    96 Constant EPFNOSUPPORT
   106 Constant EAFNOSUPPORT
   114 Constant ENETUNREACH
   125 Constant EADDRNOTAVAIL
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
	machine "mips" str= [IF]
	    \ MIPS uses different numbers for some constants
 $1006 Constant SO_RCVTIMEO
 $0004 Constant SO_REUSEADDR
 $FFFF Constant SOL_SOCKET
   122 Constant EPFNOSUPPORT
	[ELSE]
    20 Constant SO_RCVTIMEO
     2 Constant SO_REUSEADDR
     1 Constant SOL_SOCKET
	    machine "hppa" str= [IF]
		224 Constant EPFNOSUPPORT
	    [ELSE]
		machine "sparc" str= [IF]
		    46 Constant EPFNOSUPPORT
		[ELSE]
		    96 Constant EPFNOSUPPORT
		[THEN]
	    [THEN]
	[THEN]
	11 Constant EAGAIN
	EPFNOSUPPORT 1 + Constant EAFNOSUPPORT
	EPFNOSUPPORT 3 + Constant EADDRNOTAVAIL
	EPFNOSUPPORT 5 + Constant ENETUNREACH
    [THEN]
[THEN]
[IFDEF] linux
    \ netlink socket stuff
    16 Constant PF_NETLINK
    PF_NETLINK Constant AF_NETLINK
    0 Constant NETLINK_ROUTE

    begin-structure sockaddr_nl
	wfield: nl_family	\ AF_NETLINK
	wfield: nl_pad		\ zero
	lfield: nl_pid		\ port ID
	lfield: nl_groups	\ multicast groups mask
    end-structure

    begin-structure nlmsghdr
	lfield: nlmsg_len	\ Length of message including header
	wfield: nlmsg_type	\ Message content
	wfield: nlmsg_flags	\ Additional flags
	lfield: nlmsg_seq	\ Sequence number
	lfield: nlmsg_pid	\ Sending process port ID
    end-structure

    \ message types:
    16 Constant RTM_NEWLINK
    17 Constant RTM_DELLINK
    20 Constant RTM_NEWADDR
    21 Constant RTM_DELADDR

    \ message address
    begin-structure ifaddrmsg
	cfield: ifam_family    \ Address type
	cfield: ifam_prefixlen \ Prefixlength of address
	cfield: ifam_flags     \ Address flags
	cfield: ifam_scope     \ Address scope
	lfield: ifam_index     \ Interface index
    end-structure

    $01 Constant IFA_F_SECONDARY
    $02 Constant IFA_F_NODAD
    $04 Constant IFA_F_OPTIMISTIC
    $08 Constant IFA_F_DADFAILED
    $10 Constant IFA_F_HOMEADDRESS
    $20 Constant IFA_F_DEPRECATED
    $40 Constant IFA_F_TENTATIVE
    $80 Constant IFA_F_PERMANENT
    : ifa-f$ ( -- addr u ) s" snofhdtp" ;

    \ message attribute
    begin-structure rtattr
	wfield: rta_len
	wfield: rta_type
    end-structure

[THEN]
machine "mips" str= [IF]
    \ Linux on mips is weird again...
   2 Constant SOCK_STREAM
   1 Constant SOCK_DGRAM
[ELSE]
   1 Constant SOCK_STREAM
   2 Constant SOCK_DGRAM
[THEN]
   0 Constant IPPROTO_IP
  41 Constant IPPROTO_IPV6
  10 Constant IP_MTU_DISCOVER
  23 Constant IPV6_MTU_DISCOVER
  67 Constant IP_DONTFRAG
   2 Constant IP_PMTUDISC_DO
   4 Constant F_SETFL

2variable socket-timeout-d #2000. socket-timeout-d 2!

s" no free socket"     exception Constant !!nosock!!
s" bind failed"        exception Constant !!nobind!!
s" getaddrinfo failed" exception Constant !!noaddr!!
s" can't connect"      exception Constant !!noconn!!
s" listen failed"      exception Constant !!listen!!
s" accept failed"      exception Constant !!accept!!
s" blocking-mode failed" exception Constant !!blocking!!
s" sock read error"    exception Constant !!sockread!!

: close-server ( server -- )
    \G close raw server socket
    close ?ior ;

: new-socket ( -- server )
    PF_INET SOCK_STREAM 0 socket dup 0<= ?ior ;

: new-socket6 ( -- server )  true { w^ sockopt }
    PF_INET6 SOCK_STREAM 0 socket dup 0<= ?ior
    dup IPPROTO_IPV6 IPV6_V6ONLY sockopt 4 setsockopt drop ;

: new-udp-socket ( -- server )
    PF_INET SOCK_DGRAM 0 socket dup 0<= ?ior
    [defined] darwin [defined] cygwin [ or ] [IF]
	\    dup IPPROTO_IP IP_DONTFRAG sockopt-on 1 over l! 4
	\    setsockopt ?ior
    [ELSE]
	IP_PMTUDISC_DO 0 { w^ sockopt } sockopt l!
	dup IPPROTO_IP IP_MTU_DISCOVER sockopt 4
	setsockopt ?ior
    [THEN] ;

: new-udp-socket6 ( -- server ) 0 { w^ sockopt }
    PF_INET6 SOCK_DGRAM 0 socket dup 0<= ?ior
    [defined] darwin [defined] cygwin [ or ] [IF]
	\    dup IPPROTO_IP IP_DONTFRAG sockopt-on 1 over l! 4
	\    setsockopt drop
    [ELSE]
	IP_PMTUDISC_DO sockopt l!
	dup IPPROTO_IPV6 IPV6_MTU_DISCOVER sockopt 4
	setsockopt ?ior
    [THEN]
    dup IPPROTO_IPV6 IPV6_V6ONLY sockopt dup on 4 setsockopt ?ior ;

: new-udp-socket46 ( -- server )
    PF_INET6 SOCK_DGRAM 0 socket dup 0<= ?ior
    [defined] darwin [defined] cygwin [ or ] [IF]
	\    dup IPPROTO_IP IP_DONTFRAG sockopt-on 1 over l! 4
	\    setsockopt ?ior
    [ELSE]
	IP_PMTUDISC_DO 0 { w^ sockopt } sockopt l!
	dup IPPROTO_IPV6 IPV6_MTU_DISCOVER sockopt 4
	setsockopt ?ior
    [THEN]
;

\ getaddrinfo based open-socket

: >hints ( socktype -- )
    hints addrinfo erase
    PF_UNSPEC hints ai_family l!
    hints ai_socktype l! ;

: get-info ( addr u port -- info ) 0 { w^ addrres }
    base @ >r  decimal  0 <<# 0 hold #s #>  r> base ! drop
    >r c-string r> hints addrres getaddrinfo #>>
    ?dup IF
	gai_strerror cstring>sstring type
	!!noaddr!! throw  THEN
    addrres @ ;

: get-socket ( info -- socket )  dup >r >r
    BEGIN  r@  WHILE
	    r@ ai_family l@ r@ ai_socktype l@ r@ ai_protocol l@ socket
	    dup 0>= IF
		dup r@ ai_addr @ r@ ai_addrlen l@ connect
		IF
		    close-server
		ELSE
		    fd>file rdrop r> freeaddrinfo  EXIT
		THEN
	    ELSE  drop  THEN
	    r> ai_next @ >r  REPEAT
    rdrop r> freeaddrinfo !!noconn!! throw ;

: open-socket ( addr u port -- fid )
    SOCK_STREAM >hints  get-info  get-socket ;

: open-udp-socket ( addr u port -- fid )
    SOCK_DGRAM >hints  get-info  get-socket ;

: reuse-addr ( socket -- ) 0 { w^ sockopt } 1 sockopt l!
    SOL_SOCKET SO_REUSEADDR sockopt 4 setsockopt drop ;
\ : reuse-port ( socket -- ) \ only on BSD for now...
\     SOL_SOCKET SO_REUSEPORT sockopt-on 1 over l! 4 setsockopt drop ;

: port+family ( port# family -- )
    sockaddr-tmp sockaddr_in erase
    sockaddr-tmp family w!
    sockaddr-tmp port be-w! ;

: create-server  ( port# -- server )
    AF_INET port+family
    new-socket dup ?ior dup reuse-addr >r
    r@ sockaddr-tmp sockaddr_in4 bind ?ior r> ;

: create-server6  ( port# -- server )
    AF_INET6 port+family
    new-socket6 dup ?ior dup reuse-addr >r
    r@ sockaddr-tmp sockaddr_in6 bind ?ior r> ;

: create-udp-server  ( port# -- server )
    AF_INET port+family
    new-udp-socket dup ?ior dup reuse-addr >r
    r@ sockaddr-tmp sockaddr_in4 bind ?ior r> ;

: create-udp-server6  ( port# -- server )
    AF_INET6 port+family
    new-udp-socket6 dup ?ior dup reuse-addr >r
    r@ sockaddr-tmp sockaddr_in6 bind ?ior r> ;

: create-udp-server46  ( port# -- server )
    AF_INET6 port+family
    new-udp-socket46 dup ?ior dup reuse-addr >r
    r@ sockaddr-tmp sockaddr_in6 bind ?ior r> ;

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

: listen ( server /queue -- )
    listen() ?ior ;

\ This call blocks the server until a client appears. The client uses socket to
\ converse with the server.
: accept-socket ( server -- socket )
    sockaddr_in alen !
    sockaddr-tmp alen accept() 
    dup ?ior fd>file ;

: +cr  ( c-addr1 u1 -- c-addr2 u2 ) crlf count $+ ;

: blocking-mode ( socket flag -- ) >r fileno
    f_setfl r> IF  0  
    ELSE  o_nonblock|o_rdwr  
    THEN  
    fcntl ?ior ;

: hostname ( -- c-addr u )
    hostname$ c@ 0= IF
	hostname$ 1+ 255 gethostname drop
	hostname$ 1+ 255 0 scan nip 255 swap - hostname$ c!
    THEN
    hostname$ count ;
: set-socket-timeout ( u -- ) 200 + s>d socket-timeout-d 2! ;
: get-socket-timeout ( -- u ) socket-timeout-d 2@ drop 200 - ;
: write-socket ( c-addr size socket -- ) fileno -rot 0 send 0< throw ;
: close-socket ( socket -- ) close-file throw ;

: (rs)  ( socket c-addr maxlen -- c-addr size ) 
    2 pick >r r@ false blocking-mode  rot fileno -rot
    over >r msg_waitall recv
    dup 0<  IF  0 max
	errno dup 0<> swap ewouldblock <> and ?ior
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
	errno dup 0<> swap ewouldblock <> and ?ior
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
