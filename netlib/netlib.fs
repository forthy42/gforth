\ netlib.fs include netlib.so and forth utilities		08mar98jaw

require ./../wordlib.fs

WordLibrary netlib.fs ./netlib.so

\ ------        Address structures

decimal

struct
		char%		field sa_len
		char%		field sa_family
		char% 14 *	field sa_data
end-struct sockaddr

struct
		char% 2*	field sin_family
		char% 2*	field sin_port
		char% 4 *	field sin_addr
		char% 8 *	field sin_fill
end-struct sockaddr_in

\ ------        Socket Types and address families

1   constant SOCK_STREAM        \ stream (connection) socket
2   constant SOCK_DGRAM         \ datagram (conn.less) socket
3   constant SOCK_RAW           \ raw socket
4   constant SOCK_RDM           \ reliably-delivered message
5   constant SOCK_SEQPACKET     \ sequential packet socket
10  constant SOCK_PACKET        \ linux specific way of
                                \ getting packets at the dev
                                \ level.  For writing rarp and
                                \ other similar things on the
                                \ user level.

2   constant AF_INET            \ just define the most important
                                \ one

1   constant SOL_SOCKET

\ ------        place +place                                    01jan95jaw

[IFUNDEF] place
: place ( c-addr1 u c-addr2 )
        2dup c! char+ swap move ;
[THEN]

[IFUNDEF] +place
: +place ( adr len adr )
        2dup >r >r
        dup c@ char+ + swap move
        r> r> dup c@ rot + swap c! ;
[THEN]

\ ------        IP number conversion                            31dec95jaw

variable ip-class

: (ip>)
        2dup [char] . scan
        dup >r swap >r -
        s>number drop or
        r> r>
        dup 0= IF EXIT THEN
        1 ip-class +!
        1- swap 1+ swap ;

: dotted>ip     ( adr len -- u )
        0 ip-class !
        0 -rot 4 0 DO rot 8 lshift -rot (ip>) LOOP
        2drop ;

CREATE IP-Num 0 , 30 chars allot align

: ip>dotted   ( u -- adr len )
        dup 24 rshift
        255 and 0 <# [char] . hold #S #>  IP-Num place
        dup 16 rshift
        255 and 0 <# [char] . hold #S #>  IP-Num +place
        dup 8 rshift
        255 and 0 <# [char] . hold #S #>  IP-Num +place
        255 and 0 <# #S #>  IP-Num +place
        IP-Num count ;

\ ------        Host and Networkbyteorder                       30dec95jaw
\               Shift routines

1 here ! here c@        \ check byte order
[IF]                    \ little endian
: htonl         >r
                r@ 255 and                      24 lshift
                r@ [ 255 8 lshift ] literal and 8 lshift
                r@ [ 255 16 lshift ] literal and 8 rshift
                r> [ 255 24 lshift ] literal and 24 rshift
                or or or ;

: htons         >r
                r@ 255 and                      8 lshift
                r> [ 255 8 lshift ] literal and 8 rshift
                or ;

' htonl ALIAS ntohl
' htons ALIAS ntohs
[ELSE]
' NOOP ALIAS htonl
' NOOP ALIAS htons
' NOOP ALIAS ntohl
' NOOP ALIAS ntohs
[THEN]

\ ------        Short memory handling                           30dec95jaw

1 here ! here c@        \ check byte order
[IF]                    \ little endian
[IFUNDEF] s@ : s@ ( adr -- s )	@ 65535 and ; [THEN] 
[IFUNDEF] s! : s! ( s adr -- )	over 255 and over c!
	                        swap 8 rshift 255 and swap char+ c! ; [THEN]
[ELSE]
[IFUNDEF] s@ : s@ ( adr -- s )	@ 16 rshift ; [THEN]
[IFUNDEF] s! : s! ( s adr -- )	over 8 rshift 255 and over c!
				swap 255 and swap char+ c! ; [THEN]
[THEN]
[IFUNDEF] s+! : s+! ( s adr -- ) swap over s@ + swap s! ; [THEN]

\ ------	Utils						08mar98jaw

: uerr
    -1 = ;

: hostip ( adr len -- ip )
\G returns the first valid ip address of host with name (adr len)
\G as 32 Bit value in host byte order
    net-gethostbyname dup 0= ABORT" can't resolve domain name!"
    4 cells + @ ( list address ) @ ( address of address ) @ ( address ) 
    ntohl ;

: connect-tcp	( sockaddr_in* -- sock_fd )
    AF_INET SOCK_STREAM 0 net-socket
    dup uerr ABORT" couldn't make socket"
    >r sockaddr_in %size r@ net-connect uerr ABORT" couldn't connect"
    r> ;

: ipport!sockaddr	{ ip port sockaddr* -- }
    port htons sockaddr* sin_port s!
    ip   htonl sockaddr* sin_addr !
    AF_INET sockaddr* sin_family s! ;

: connect-tcp-ip 	( ip port -- sock_fd )
    sockaddr_in %alloc dup >r ipport!sockaddr
    r@ connect-tcp 
    r> free throw ;

: connect-tcp-name 	( adr len port -- sock_fd )
    >r hostip r> connect-tcp-ip ;

