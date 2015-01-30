\ terminal server for Gforth

require unix/socket.fs

4444 Value gforth-port#

: term-cr "\x0d\x0a" type outfile-id flush-file throw ;

' (type) ' (emit) ' term-cr ' (form)  output: out>term
what's at-xy what's at-deltaxy what's page what's attr!
out>term
IS attr! IS page IS at-deltaxy IS at-xy
default-out op-vector !

: >term  out>term op-vector @ debug-vector ! ;

: get-connection ( -- )
    gforth-port# create-server { lsocket }
    lsocket 1 listen
    lsocket accept-socket
    dup to infile-id
    dup to outfile-id
    to debug-fid
    >term
    key drop BEGIN  key? WHILE key drop REPEAT ;

