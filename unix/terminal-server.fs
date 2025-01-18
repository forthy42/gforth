\ terminal server for Gforth

require unix/socket.fs

4444 Value gforth-port#

: term-cr "\x0d\x0a" type outfile-id flush-file throw ;

' (type) ' (emit) ' term-cr ' (form)  output: out>term
action-of at-xy action-of at-deltaxy action-of page action-of attr!
action-of control-sequence  action-of theme-color!
out>term
IS theme-color!  IS control-sequence
IS attr! IS page IS at-deltaxy IS at-xy
default-out op-vector !

: >term  out>term op-vector @ debug-vector ! ;

: read-iac ( -- )
    BEGIN  key?  WHILE
	    key #255 = IF key drop key drop THEN
    REPEAT ;

: get-connection ( -- )
    gforth-port# create-server { lsocket }
    lsocket 1 listen
    lsocket accept-socket
    dup to infile-id
    dup to outfile-id
    to debug-fid
    >term read-iac ;

