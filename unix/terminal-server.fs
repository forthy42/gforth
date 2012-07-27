\ terminal server for Gforth

require unix/socket.fs

4444 Value gforth-port#

: term-cr "\x0d\x0a" type outfile-id flush-file throw ;

: get-connection ( -- )
    gforth-port# create-server { lsocket }
    lsocket 1 listen
    lsocket accept-socket
    dup to infile-id
    dup to outfile-id
    to errfile-id
    ['] term-cr is cr
    key drop BEGIN  key? WHILE key drop REPEAT ;
