#! /usr/local/bin/gforth031

\ make our directory the search directory
sourcefilename extractpath fpath only-path

require wlibs/unixlib.fs
require wlibs/netlib.fs
require jflib/tools/fieldscan.fs

: usage
  ." httpclient.fs [ -p port ] [ -t timeout ] [ -s ] [ -r ] [ -b filename ] [ -e filename ]" cr
  ."               -h host resource-name" cr
  ." Options:" cr 
  ." -p N       Set portnumber to N (default is 80)" cr
  ." -t N       Set timeout to N (default is no timeout)" cr
  ." -b name    Save body (data) of response to file named name" cr
  ." -e name    Save header of response to file name" cr
  ." -h host    set host to host (dault is localhost)" cr
  ." -r         make an report" cr
  ." -s         silent operation, don't view requested data" cr 
  bye
  ;

Create hostname ," localhost" 300 chars allot 
Variable port 80 port !
Variable timeout 0 timeout !
Variable silent-flag silent-flag off
Variable result-flag result-flag off

Create crlf 13 c, 10 c, 13 c, 10 c,
Create wbuffer 300 chars allot
Create rbuffer 1000 chars allot

Variable Headerbytes 0 Headerbytes !
Variable Databytes 0 Databytes !
Variable StatusCode
Create Protocol 100 chars allot
Create ReasonPhrase 100 chars allot
0 Value header-fd
0 Value data-fd

: .args
  argc @ 0 DO
	." arg " i . ." : " i arg type cr 
  LOOP ;

0 Value optind

: end? ( -- flag)
    optind argc @ u>= ;

: arg? ( -- adr len )
\G get next argument
    end? ABORT" too few arguments!"
    optind arg
    1 optind + to optind ;

: scanarg
  2 to optind
  end? IF usage THEN
  BEGIN	end? 0=
  WHILE	optind arg 
	IF	c@ [char] - =
		IF
			optind arg
			1 optind + to optind
			forth-wordlist search-wordlist
			0= ABORT" wrong option!"
			execute -1
		ELSE	false
		THEN
	ELSE	true
	THEN
  WHILE
  REPEAT THEN
  ;	

: -? usage ;
: -h arg? hostname place ;
: -p 0.0 arg? >number 2drop d>s port ! ;
: -t 0.0 arg? >number 2drop d>s port ! ;
: -s silent-flag on ;
: -r result-flag on ;
: -b arg? r/w bin create-file throw to data-fd ;
: -e arg? r/w bin create-file throw to header-fd ;

: fd-readline ( adr len fd -- u ior )
    >r over + r> { startadr endadr fd } 
    startadr
    BEGIN
	dup 1 fd uread
	?dup IF nip startadr - EXIT THEN
	IF 	dup c@ 
		CASE	10 OF startadr - 0 EXIT ENDOF
			13 OF ENDOF
			dup OF char+ ENDOF
		ENDCASE
	THEN
	dup endadr =
    UNTIL
    startadr - 0 ;

: http-header ( sd -- ior ) { sd }

    \ read 1st line
    rbuffer 1000 sd fd-readline ?dup ?EXIT
    rbuffer swap
    bl fieldscan 100 min Protocol place
    bl fieldscan 0 -rot 0 -rot >number 2drop d>s StatusCode !
    bl fieldscan 100 min ReasonPhrase place 
    2drop 

    \ read until empty line
    BEGIN rbuffer 1000 sd fd-readline ?dup IF nip EXIT THEN
	  dup 
    WHILE dup 2 + HeaderBytes +!
	  rbuffer swap 
	  silent-flag @ 0= IF 2dup type cr THEN
	  header-fd IF header-fd write-line drop ELSE 2drop THEN
    REPEAT
    silent-flag @ 0= IF cr THEN
    ;

: http-body ( sd -- ior ) { sd }
    BEGIN rbuffer 200 sd uread -39 <>
    WHILE dup DataBytes +!
	  rbuffer swap 
	  silent-flag @ 0= IF 2dup type cr THEN
	  data-fd IF data-fd write-file drop ELSE 2drop THEN
    REPEAT 0 ;

: http-data ( sd -- ior ) { sd }
    sd http-header ?dup ?EXIT
    sd http-body ;

: main
    end? ABORT" no file specified!"
    timeout @ ?dup IF alarm THEN
    hostname count port @ connect-tcp-name { sd }
    s" GET " wbuffer place
    optind arg wbuffer +place
    s"  HTTP/1.0" wbuffer +place
    crlf 4 wbuffer +place
    wbuffer count sd uwrite throw drop
    sd http-data drop
    sd uclose throw 
    result-flag @ 
    IF	
	." returnstatus=okay" cr
	." statuscode=" StatusCode @ 0 u.r cr
	." reasonphrase=" [char] " emit ReasonPhrase count type [char] " emit cr
	." headerbytes=" HeaderBytes @ 0 u.r cr 
	." databytes=" DataBytes @ 0 u.r cr 
    THEN 
    header-fd ?dup IF close-file throw THEN 
    data-fd ?dup IF close-file throw THEN ;

: (DoError2) ( throw-code -- )
  Result-Flag @
  IF ." returnstatus=failed" cr THEN
  outfile-id dup flush-file drop >r
  stderr to outfile-id
  dup -2 =
  IF 
     "error @ ?dup
     IF
        cr count type 
     THEN
     drop
  ELSE
     .error
  THEN
  normal-dp dpp ! 
  r> to outfile-id
  ;

' (DoError2) IS DoError
scanarg
main

bye
