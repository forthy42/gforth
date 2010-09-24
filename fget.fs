\ a http get command

\ Copyright (C) 2000,2002,2003,2006,2007 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

require unix/socket.fs
require string.fs

Create crlf #cr c, #lf c,

: writeln ( addr u fd -- )
    dup >r write-file throw crlf 2 r> write-file throw ;

: request ( host u request u proxy-host u port -- fid )
    open-socket >r
    s" GET " r@ write-file throw r@ write-file throw s"  HTTP/1.1" r@ writeln
    s" Host: " r@ write-file throw r@ writeln
    s" Connection: close" r@ writeln
    s" User-Agent: " r@ write-file throw
    s" Gforth Proxy 0.1" r@ writeln
    s" " r@ writeln r> ;

Variable proxy          \ s" proxy" proxy $! \ replace that with your proxy host
Variable proxy-port     \ 8080 proxy-port !  \ replace that with your proxy port

\ set proxy to your local proxy, and proxy-port to your local proxy port
\ if you need any.

: http-open ( host u request u -- fid )
    proxy @ 0= IF  2over 80  ELSE  proxy $@ proxy-port @  THEN request ;

wordlist Constant response
wordlist Constant response-values

Variable response-string
Variable maxnum

: get-rest ( addr -- )  source >in @ /string dup >in +! rot $! ;
: ?cr ( -- )
  #tib @ 1 >= IF  source 1- + c@ #cr = #tib +!  THEN ;
: refill-loop ( -- flag ) base @ >r base off
  BEGIN  refill ?cr  WHILE  ['] interpret catch drop  >in @ 0=  UNTIL
  true  ELSE  maxnum off false  THEN  r> base ! ;

: response:  ( -- )  name
    Forth definitions 2dup 1- nextname Variable
    response-values set-current nextname here cell - Create ,
DOES> @ get-rest ;
: >response  response-values 1 set-order ;

response set-current

: HTTP/1.1 response-string get-rest >response ;
: HTTP/1.0 response-string get-rest >response ;

\ response variables

Forth definitions

response: Allow:
response: Age:
response: Accept-Ranges:
response: Cache-Control:
response: Connection:
response: Proxy-Connection:
response: Content-Base:
response: Content-Encoding:
response: Content-Language:
response: Content-Length:
response: Content-Location:
response: Content-MD5:
response: Content-Range:
response: Content-Type:
response: Date:
response: ETag:
response: Expires:
response: Last-Modified:
response: Location:
response: Mime-Version:
response: Proxy-Authenticate:
response: Proxy-Connection:
response: Public:
response: Retry-After:
response: Server:
response: Transfer-Encoding:
response: Upgrade:
response: Via:
response: Warning:
response: WWW-Authenticate:
response: X-Cache:
response: X-Powered-By:

Forth definitions

\ response handling

: get-response ( fid -- ior )
    push-file loadfile !  loadline off  blk off
    response 1 set-order  ['] refill-loop catch
    only forth also  pop-file ;

\ data handling

Variable data-buffer

: clear-data ( -- )
    s" " data-buffer $! ;
: add-chunk ( u fid -- u' )
    swap data-buffer $@len dup >r + data-buffer $!len
    data-buffer $@ r@ /string rot read-file throw
    dup r> + data-buffer $!len ;
: read-sized ( u fid -- )
    add-chunk drop ;
: read-to-end ( fid -- )
    >r BEGIN  $1000 r@ add-chunk $1000 <> UNTIL  rdrop ;

: read-chunked ( fid -- ) base @ >r hex >r
    BEGIN  pad $100 r@ read-line throw  WHILE
	pad swap s>number drop dup WHILE  r@ add-chunk drop
	pad 1 r@ read-line throw  nip 0= UNTIL
    ELSE  drop  THEN  THEN  rdrop r> base ! ;

: read-data ( fid -- ) clear-data >r
    Content-Length @ IF
	Content-Length $@ s>number drop r> read-sized  EXIT  THEN
    Transfer-Encoding @ IF
	Transfer-Encoding $@ s" chunked" str= 0= IF
	    r> read-chunked  EXIT  THEN  THEN
    r> read-to-end ;

: fslurp ( addr u -- addr u )
    '/ $split -1 /string
    http-open dup >r get-response throw r> read-data  data-buffer $@ ;
