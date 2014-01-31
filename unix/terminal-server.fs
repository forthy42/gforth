\ terminal server for Gforth

require unix/socket.fs

4444 Value gforth-port#

: term-cr "\x0d\x0a" type outfile-id flush-file throw ;

' (type) ' (emit) ' term-cr ' (form)  output: >term
what's at-xy what's at-deltaxy what's page what's attr!
>term
IS attr! IS page IS at-deltaxy IS at-xy
default-out op-vector !

: get-connection ( -- )
    gforth-port# create-server { lsocket }
    lsocket 1 listen
    lsocket accept-socket
    dup to infile-id
    dup to outfile-id
    to debug-fid
    >term
    key drop BEGIN  key? WHILE key drop REPEAT ;

