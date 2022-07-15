\ HTTP client implementation for VFX
\ This is a very hacky implementation, but it works.
\ Common words for the compat layer
\ (c)copyright 2017 by Gerald Wodni <gerald.wodni@gmail.com>

\ some helpers
: str>num ( c-addr n -- n )
    2>r 0 0 2r> >number 2drop drop ;

: str-to-lower ( c-addr n -- c-addr n )
    2dup bounds ?do
        i c@ dup [CHAR] A >= over [CHAR] Z <= and if
            $20 + i c!
        else
            drop
        then
    loop ;

: skip-bl ( c-addr n -- c-addr 2 n2 )
    2>r \ save start address
    2r@ bounds do
        i c@ bl <> if
            i leave
        then
    loop 2r>
    >r over swap - r> \ number of skipped blanks
    swap - ; \ change length

\ data in receiving socket
: skey? ( -- f )
    buffer-len @ 0<> ;

\ make sure we refill and get at least 1 char
: srefill ( socket -- )
    begin
        skey? 0=
    while
        dup (srefill)
    repeat drop ;

\ read char from socket
: skey ( socket -- c )
    srefill rbuffer c@
    0 buffer-len ! ;

\ strip \r
: skey-no\r ( socket -- c )
    begin
        dup skey dup 13 =
    while
        drop
    repeat nip ;

: sline ( c-addr n socket -- c-addr n )
    -rot    \ save socket
    over >r \ save buffer
    bounds do
        dup skey-no\r dup 10 = if \ leave on newline
            2drop i leave \ push current buffer-offset
        then
        i c!
    loop
    r@ - r> swap ; \ return buffer with read size

\ I might fall for locals some day, this is far easier than plain sline
: sline-until { c-addr-buf n-buf socket c-until -- c-addr n }
    c-addr-buf n-buf bounds do
        socket skey-no\r dup c-until = over 10 = or if
            drop i leave
        then
        i c!
    loop
    c-addr-buf - c-addr-buf swap ;

80 constant header-max
header-max buffer: header-buffer

: header-buf ( s -- c-addr n s )
    >r header-buffer header-max r> ;

: header-line ( s -- c-addr n )
    header-buf sline ;

: header-name ( s -- c-addr n )
    header-buf [CHAR] : sline-until str-to-lower ;

: http-status ( s -- n )
    header-line s"  " search if
        3 >= if
            1+ 3 str>num
        else \ return one on invalid string length
            1
        then
    else \ return zero if no space was found
        0
    then ;

80 constant slines-max
slines-max buffer: slines-buffer
: slines ( socket -- )
    101 0 do
        dup >r slines-buffer slines-max r> sline ." LINE:" i . type cr
    loop drop ;

\ parse all headers and return content length
: http-length ( s -- n-content-length )
    0
    {: length :}
    begin
        dup header-name
        dup 0<>
    while
        s" content-length" compare 0= if
            dup header-line
            skip-bl str>num to length
        else
            dup header-line 2drop
        then
    repeat 2drop drop length ;

: http-body ( socket c-addr n -- )
    bounds ?do
        dup skey i c!
    loop drop ;

\ directories
-529 constant error-exists
