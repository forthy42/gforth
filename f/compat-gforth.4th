\ HTTP client implementation for GForth
\ (c)copyright 2015-2017 by Gerald Wodni <gerald.wodni@gmail.com>,
\   Rick Carlino <rick.carlino@gmail.com>

[undefined] BUFFER: [if]
  : BUFFER: ( u "<name>" -- ; -- addr )
    CREATE ALLOT
  ;
[then]

[undefined] {: [if]
  12345 CONSTANT undefined-value

  : match-or-end? ( c-addr1 u1 c-addr2 u2 -- f )
    2 PICK 0= >R COMPARE 0= R> OR ;

  : scan-args
    \ 0 c-addr1 u1 -- c-addr1 u1 ... c-addrn un n c-addrn+1 un+1
    BEGIN
        2DUP S" |" match-or-end? 0= WHILE
        2DUP S" --" match-or-end? 0= WHILE
        2DUP S" :}" match-or-end? 0= WHILE
        ROT 1+ PARSE-NAME
    AGAIN THEN THEN THEN ;

  : scan-locals
    \ n c-addr1 u1 -- c-addr1 u1 ... c-addrn un n c-addrn+1 un+1
    2DUP S" |" COMPARE 0= 0= IF
        EXIT
    THEN
    2DROP PARSE-NAME
    BEGIN
        2DUP S" --" match-or-end? 0= WHILE
        2DUP S" :}" match-or-end? 0= WHILE
        ROT 1+ PARSE-NAME
        POSTPONE undefined-value
    AGAIN THEN THEN ;

  : scan-end ( c-addr1 u1 -- c-addr2 u2 )
    BEGIN
        2DUP S" :}" match-or-end? 0= WHILE
        2DROP PARSE-NAME
    REPEAT ;

  : define-locals ( c-addr1 u1 ... c-addrn un n -- )
    0 ?DO
        (LOCAL)
    LOOP
    0 0 (LOCAL) ;

  : {: ( -- )
    0 PARSE-NAME
    scan-args scan-locals scan-end
    2DROP define-locals
  ; IMMEDIATE
[then]

include unix/socket.fs

80 constant http-port
1 constant buffer-max       \ receiving buffer length ( yes we only care about single chars )
buffer-max buffer: rbuffer  \ receiving buffer
variable buffer-len         \ chars in receiving buffer

\ attempt to refill
: (srefill) ( socket -- )
    rbuffer buffer-max read-socket nip buffer-len ! ;

: http-open ( c-addr-path n-path c-addr-host n-host -- socket )
    2dup \ save host
    http-port open-socket >r
        s" GET " r@ write-socket    \ start get request
        2swap r@ write-socket       \ send path
        s\"  HTTP/1.1\r\nHost: " r@ write-socket
        r@ write-socket             \ send host
        s\" \r\nConnection: Close\r\n\r\n" r@ write-socket
        r>
    ;

include compat-common.4th

: http-slurp ( c-addr-path n-path c-addr-host n-host -- c-addr-response n-response n-status )
    http-open
    dup http-status >r

    >r \ socket

    r@ http-length
    dup allocate throw swap \ c-addr n-len

    2dup r@ -rot http-body  \ read body into buffer

    r> close-socket
    r> \ status
    ;


\ directories
: create-directories ( c-addr n -- ior )
    $1FF mkdir-parents      \ add mask
    dup error-exists = if   \ ignore error-exists
        drop 0
    then ;
