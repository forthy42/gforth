\ Internet 2.0 experiments

require unix/socket.fs

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

: read-a-packet ( -- addr u )
    net2o-srv inbuf maxpacket read-socket-from ;

