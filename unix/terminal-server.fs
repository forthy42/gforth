\ terminal server for Gforth

require unix/socket.fs

4444 Value gforth-port#
form 2value telnet-form

: term-cr "\x0d\x0a" type outfile-id flush-file throw ;

' (type) ' (emit) ' term-cr ' telnet-form  output: out>term
action-of at-xy action-of at-deltaxy action-of page action-of attr!
action-of control-sequence  action-of theme-color!
out>term
IS theme-color!  IS control-sequence
IS attr! IS page IS at-deltaxy IS at-xy
default-out op-vector !

: >term  out>term op-vector @ debug-vector ! ;

debug: telnet(

: .iac-will ( n -- )
    "\xFF\xFD" type emit ;
: .iac-do ( n -- )
    "\xFF\xFB" type emit ;
: .iac-will+do ( n -- )
    dup .iac-will .iac-do ;

:is ekey-extension
    dup $FF = \ telnet iac
    if  drop
	case key
	    $FA of \ detailed negotiation
		key $1F = IF
		    key 8 lshift key +
		    key 8 lshift key + swap
		    telnet( ." form: " 2dup dec. dec. cr )
		    to telnet-form
		THEN
	    endof
	    $FB of  telnet( ." will " key dec. cr )else( key drop )  endof
	    $FC of  telnet( ." wont " key dec. cr )else( key drop )  endof
	    $FD of  telnet( ." do "   key dec. cr )else( key drop )  endof
	    $FE of  telnet( ." dont " key dec. cr )else( key drop ) endof
	endcase
	k-winch  rdrop
    then ;

: get-connection ( -- )
    gforth-port# create-server { lsocket }
    lsocket 1 listen
    lsocket accept-socket
    dup to infile-id
    dup to outfile-id
    to debug-fid
    >term
    \ IAC commands
    3 .iac-will+do \ Suppress Go Ahead
    1 .iac-will+do \ Echo
    #31 .iac-will  \ Terminal size
;

