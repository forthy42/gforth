#! /usr/local/bin/gforth

warnings off

include string.fs

Variable url
Variable protocol

: get ( addr -- )  name rot $! ;
: get-rest ( addr -- )  source >in @ /string dup >in +! rot $! ;

Table constant http/1.0

: rest:  ( -- )  name
  Forth definitions 2dup 1- nextname Variable
  http/1.0 set-current nextname here cell - Create ,
  DOES> @ get-rest ;

\ HTTP protocol                                        26mar00py

http/1.0 set-current

: GET               url get protocol get-rest ;
rest: User-Agent:
rest: Pragma:
rest: Host:
rest: Accept:
rest: Accept-Encoding:
rest: Accept-Language:
rest: Accept-Charset:
rest: Via:
rest: X-Forwarded-For:
rest: Cache-Control:
rest: Connection:
rest: Referer:

definitions

s" close" connection $!
s" /nosuchfile" url $!
s" HTTP/1.0" protocol $!

Variable maxnum

: ?cr ( -- )
  #tib @ 1 >= IF  source 1- + c@ #cr = #tib +!  THEN ;
: refill-loop ( -- flag )
  BEGIN  refill ?cr  WHILE  interpret  >in @ 0=  UNTIL
  true  ELSE  maxnum off false  THEN ;
: get-input ( -- flag ior )
  infile-id push-file loadfile !  0 loadline ! blk off
  http/1.0 1 set-order  ['] refill-loop catch
  only forth also  pop-file ;

\ Keep-Alive handling                                  26mar00py

: .connection ( -- )
  ." Connection: "
  connection $@ s" Keep-Alive" compare 0= maxnum @ 0> and
  IF  connection $@ type cr
      ." Keep-Alive: timeout=15, max=" maxnum @ 0 .r cr
      -1 maxnum +!  ELSE  ." close" cr maxnum off  THEN ;

\ Use Forth as server-side script language             26mar00py

: $> ( -- )
    BEGIN  source >in @ /string s" <$" search  0= WHILE
        type cr refill  0= UNTIL  EXIT  THEN
    nip source >in @ /string rot - dup 2 + >in +! type ;
: <HTML> ( -- )  ." <HTML>" $> ;

\ Rework HTML directory                                26mar00py

Variable htmldir

: rework-htmldir ( addr u -- addr' u' / ior )
  htmldir $!
  htmldir $@ 1 min s" ~" compare 0=
  IF    s" /.html-data" htmldir dup $@ 2dup '/ scan
        nip - nip $ins
  ELSE  s" /usr/local/httpd/htdocs/" htmldir 0 $ins  THEN
  htmldir $@ 1- 0 max + c@ '/ = htmldir $@len 0= or
  IF  s" index.html" htmldir dup $@len $ins  THEN
  htmldir $@ file-status nip ?dup ?EXIT
  htmldir $@ ;

\ MIME type handling                                   26mar00py

: >mime ( addr u -- mime u' )  2dup tuck over + 1- ?DO
  I c@ '. = ?LEAVE  1-  -1 +LOOP  /string ;

: >file ( addr u -- size fd )
  r/o bin open-file throw >r
  r@ file-size throw drop
  ." Accept-Ranges: bytes" cr
  ." Content-Length: " dup 0 .r cr r> ;
: transparent ( size fd -- ) >r
  dup allocate throw swap
  over swap r@ read-file throw over swap type
  free r> close-file throw throw ;

: transparent:  Create ,"  DOES>  >r  >file
  .connection
  ." Content-Type: "  r> count type cr cr
  transparent ;

\ mime types                                           26mar00py

: lastrequest
  ." Connection: close" cr maxnum off
  ." Content-Type: text/html" cr cr ;

wordlist constant mime
mime set-current

: shtml ( addr u -- )  lastrequest  included ;

transparent: html text/html"
transparent: gif image/gif"
transparent: jpg image/jpeg"
transparent: png image/png"
transparent: gz application/x-gzip"
transparent: bz2 application/x-bzip2"
transparent: exe application/octet-stream"
transparent: class application/octet-stream"
transparent: sig application/pgp-signature"
transparent: txt text/plain"

definitions

lastxt @ Alias txt

\ http errors                                          26mar00py

: .ok   ." HTTP/1.1 200 OK" cr ;
: html-error ( n addr u -- )
    ." HTTP/1.1 " 2 pick . 2dup type cr lastrequest
    ." <HTML><HEAD><TITLE>" 2 pick . 2dup type ." </TITLE></HEAD>" cr
    ." <BODY><H1>" type drop ." </H1>" cr ;
: .trailer ( -- )
    ." <HR><ADDRESS>Gforth httpd 0.1</ADDRESS>" cr
    ." </BODY></HTML>" cr ;
: .nok  &400 s" Bad Request" html-error
    ." <P>Your browser sent a request that this server could not understand.</P>" cr
    ." <P>Invalid request in: <CODE>" error-stack cell+ 2@ swap type
    ." </CODE></P>" cr .trailer ;
: .nofile  &404 s" Not Found" html-error
    ." <P>The requested URL <CODE>" url $@ type
    ." </CODE> was not found on this server</P>" cr .trailer ;

\ http server                                          26mar00py

: http  get-input  IF  .nok  ELSE
    IF  url $@ 1 /string rework-htmldir
	dup 0< IF  drop .nofile
	ELSE  .ok  2dup >mime mime search-wordlist
	    IF  catch IF  maxnum off THEN  ELSE  txt  THEN
	THEN  THEN  THEN  outfile-id flush-file throw ;

: httpd  ( n -- )  maxnum !
  BEGIN  http  maxnum @ 0=  UNTIL ;

( script? [IF] ) &100 httpd bye ( [THEN] )
