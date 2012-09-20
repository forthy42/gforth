#! /usr/bin/gforth

\ This file is in the public domain. NO WARRANTY.

\ Example CGI script

.( Content-Type: text/plain; charset=us-ascii) cr
.( Content-Transfer-Encoding: 7bit) cr
cr
: zeroes ( u -- )
    0 +do '0 emit loop ;

: u.rz ( u1 u2 -- )
    >r s>d  <<# #s #> r> over - zeroes type #>> ;

.( It's ) time&date 4 u.rz .( -) 2 u.rz .( -) 2 u.rz space
                    2 u.rz .( :) 2 u.rz .( :) 2 u.rz cr
\ : printargs ( -- )
\     argc @ 0 +do
\         ." arg" i . ." = '" i arg type ." '" cr
\     loop ;
\ printargs
\ s" QUERY_STRING" getenv type cr
\ s" PATH_INFO" getenv type cr
\ s" PATH_TRANSLATED" getenv type
\ s" CONTENT_LENGTH" getenv type
bye
