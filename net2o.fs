\ Internet 2.0 experiments

require unix/socket.fs
require string.fs

\ Create udp socket

4242 Constant net2o-udp

0 Value net2o-sock
0 Value net2o-srv

: new-server ( -- )
    net2o-udp create-udp-server s" w+" c-string fdopen to net2o-srv ;

: new-client ( hostaddr u -- )
    net2o-udp open-udp-socket to net2o-sock ;

$81A Constant maxpacket

Create inbuf maxpacket allot

2 8 2Constant address%

struct
    short% field flags
    address% field dest
    address% field addr
    address% field junk
end-struct net2o-header

: read-a-packet ( -- addr u )
    net2o-srv inbuf maxpacket read-socket-from ;

: send-a-packet ( addr u -- n )
    net2o-sock fileno -rot 0 sockaddr-tmp 16 sendto ;

\ clients routing table

8 Value route-bits
8 Constant /address
' dfloats Alias addresses
0 Value routes

: init-route ( -- )
    routes IF  routes free  0 to routes  throw  THEN
    /address route-bits lshift dup allocate throw to routes
    routes swap erase ;

: route-hash ( addr -- hash )
    /address route-bits (hashkey1) ;

: insert-address ( -- )
    sockaddr-tmp route-hash addresses routes + /address move ;
\ FIXME: doesn't check for collissons

: address>route ( -- n/-1 )
    sockaddr-tmp route-hash dup addresses routes + /address tuck
    str= 0= IF  drop -1  THEN ;
: route>address ( n -- )
    addresses routes + sockaddr-tmp /address move ;

\ bit reversing

: bitreverse8 ( u1 -- u2 )
    0 8 0 DO  2* over 1 and + swap 2/ swap  LOOP  nip ;

Create reverse-table $100 0 [DO] [I] bitreverse8 c, [LOOP]

: reverse8 ( c1 -- c2 ) reverse-table + c@ ;
: reversex ( x1 -- x2 )
    0 8 0 DO  8 lshift over $FF and reverse8 or
	swap 8 rshift swap  LOOP ;

\ route a packet

: packet-route ( -- flag )
    inbuf dest c@ 0= IF  true  EXIT  THEN \ local packet
    address>route reverse8  inbuf dest c@ route>address
    inbuf dest dup 1+ swap /address 1- move
    inbuf dest /address 1- + c!  false ;

\ packet&header size

$80 Constant destsize#
$40 Constant addrsize#
$20 Constant junksize#
$06 Constant datasize#

: header-size ( x -- u ) >r 2
    r@ destsize# and IF  8  ELSE  2  THEN +
    r@ addrsize# and IF  8  ELSE  2  THEN +
    r@ junksize# and IF  8  ELSE  0  THEN +
    rdrop ;

Create header-sizes  $100 0 [DO] [I] header-size c, $20 [+LOOP]

: packet-size ( -- n )
    inbuf c@ 5 rshift header-sizes + c@
    $20 inbuf c@ datasize# and lshift + ;
: packet-body ( -- addr )
    inbuf dup c@ 5 rshift header-sizes + c@ + ;

\ packet delivery table

\ each source has multiple destination spaces

0 Value delivery-table
Variable return-addr
Variable dest-addr
8 Value delivery-bits

: init-delivery-table ( -- )
    delivery-table IF  delivery-table free  0 to delivery-table  throw  THEN
    1 cells delivery-bits lshift dup allocate throw to delivery-table
    delivery-table swap erase ;

: >ret-addr ( -- )
    inbuf dest @ reversex return-addr ! ;
: >dest-addr ( -- )
    0 inbuf addr 8 bounds ?DO  8 lshift I c@ or  LOOP ;

: ret-hash ( -- n )  return-addr 1 cells delivery-bits (hashkey1) ;

: check-dest ( -- addr t / f )
    ret-hash cells delivery-table +
    dup @ 0= IF  drop false  EXIT  THEN
    $@ bounds ?DO
	I 2@ 1- bounds dest-addr @ within
	0= IF  I cell+ 2@ dest-addr @ swap - + true UNLOOP  EXIT  THEN
    3 cells +LOOP
    false ;

Create dest-mapping  0 , 0 , 0 ,

: map-dest ( addr u addr' -- )
    ret-hash cells delivery-table + >r
    r@ @ 0= IF  s" " r@ $!  THEN
    dest-mapping 2 cells + ! dest-mapping 2!
    dest-mapping 3 cells r> $+! ;
