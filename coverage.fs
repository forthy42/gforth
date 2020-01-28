\ Code coverage tool

\ Author: Bernd Paysan
\ Copyright (C) 2018,2019 Free Software Foundation, Inc.

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

require sections.fs

unused extra-section coverage

: cover-end ( -- addr ) ['] here coverage ;
cover-end Constant cover-start
: cover, ( n -- ) ['] , coverage ;
: cover-end! ( addr -- )  [: dp ! ;] coverage ;

[IFUNDEF] coverage?
    0 Value coverage? ( -- flag ) \ gforth-exp
    \G Coverage check on/off.
[THEN]
0 value dead-cov?

: cov+, ( -- )
    coverage?  dead-code @ 0= and  loadfilename# @ 0>= and  IF
	current-sourceview input-lexeme @ + cover,
	postpone inc# cover-end , 0 cover,
    THEN
    false to dead-cov? ;

: cov+ ( -- ) \ gforth-exp
    \G Add a coverage tag here.
    dead-cov? 0= state @ and  IF  cov+,  THEN
    false to dead-cov? ; immediate compile-only
: ?cov+ ( flag -- flag ) \ gforth-exp
    ]] dup IF ELSE THEN [[ ; immediate compile-only

:noname defers :-hook                     cov+, ; is :-hook
:noname defers if-like            postpone cov+ ; is if-like
:noname defers until-like         postpone cov+ ; is until-like
:noname defers basic-block-end    postpone cov+ ; is basic-block-end
:noname defers exit-like      true to dead-cov? ; is exit-like
:noname defers before-line        postpone cov+ ; is before-line
:noname true to dead-cov?
    defers then-like  postpone cov+ ; is then-like

: cov% ( -- ) \ gforth-exp
    \G Print the percentage of basic blocks loaded after
    \G @file{coverage.fs} that are executed at least once.
    0 cover-end cover-start U+DO
	I cell+ @ 0<> -
    2 cells +LOOP  #2000 cells cover-end cover-start - */
    0 <# '%' hold # '.' hold #s #> type ."  coverage" ;

: .cover-raw ( -- ) \ gforth-exp
    \G Print raw execution counts.
    cover-end cover-start U+DO
	I @ .sourceview ." : " I cell+ ? cr
    2 cells +LOOP ;

Defer .cov#

: .ansi-cov# ( n -- )
    >r info-color error-color r@ select
    dup Invers or attr! space r> 0 .r  attr! ;
: .paren-cov# ( n -- ) ."  ( " 0 .r ." ) " ;

: color-cover ( -- ) ['] .ansi-cov#  is .cov# ;
\G Print execution counts in colours (default).
: bw-cover    ( -- ) ['] .paren-cov# is .cov# ;
\G Print execution counts in parentheses (source-code compatible).
color-cover

: ?del-cover ( addr u -- n )
    \G Remove coverage comment.
    2dup s"  ( " string-prefix?  IF
	3 dup >r /string
	BEGIN  over c@ digit?  WHILE  drop 1 /string r> 1+ >r  REPEAT
	s" ) " string-prefix? IF  r> 2 +  ELSE  rdrop  0  THEN
    ELSE  2drop  0  THEN ;

: .cover-file { fn -- } \ gforth-exp
    \G Print coverage in included file with index @var{fn}.
    fn included-buffer 0 locate-line 0 { d: buf lpos d: line cpos }
    cover-end cover-start U+DO
	I @ view>filename# fn = IF
	    buf lpos
	    BEGIN  dup I @ view>line u<  WHILE
		    line cpos safe/string type cr default-color attr!
		    locate-line  to line  0 to cpos
	    REPEAT  to lpos  to buf
	    line cpos safe/string
	    over I @ view>char cpos - tuck type +to cpos  2drop
	    I cell+ @ .cov#
	    line cpos safe/string ?del-cover +to cpos
	THEN
    2 cells +LOOP
    line cpos safe/string type cr  default-color attr!  buf type ;

: covered? ( fn -- flag ) \ gforth-exp
    \G Check if included file with index @var{fn} has coverage information.
    false cover-end cover-start U+DO 
	over I @ view>filename# = or
    2 cells +LOOP  nip ;

: .coverage ( -- ) \ gforth-exp
    \G Show code with execution frequencies.
    cr included-files $[]# 0 ?DO
	I covered? IF
	    I [: included-files $[]@ type ':' emit cr ;]
	    warning-color color-execute
	    I .cover-file
	THEN
    LOOP ;

: annotate-cov ( -- ) \ gforth-exp
    \G For every file with coverage information, produce a @code{.cov}
    \G file that has the execution frequencies inserted.  We recommend
    \G to use @code{bw-cover} first (with the default
    \G @code{color-cover} you get escape sequences in the files).
    included-files $[]# 0 ?DO
	I covered? IF
	    I [: included-files $[]@ type ." .cov" ;] $tmp
	    r/w create-file dup 0= IF
		drop { fd }
		I ['] .cover-file fd outfile-execute  fd close-file throw
	    ELSE
		I [: included-files $[]@ type space
		    .error-string cr ;] warning-color color-execute
		drop  THEN \ ignore write errors
	THEN
    LOOP ;

\ load and save coverage

$10 buffer: cover-hash

: hash-cover ( -- addr u ) \ gforth-exp
    cover-hash $10 erase
    cover-end cover-start U+DO
	I cell false cover-hash hashkey2
    2 cells +LOOP
    cover-hash $10 ;

: cover-filename ( -- addr u ) \ gforth-exp
    "~/.cache/gforth/" 2dup $1ff mkdir-parents drop
    [: type
	hash-cover bounds ?DO  I c@ 0 <# # # #> type LOOP ." .covbin" ;]
    ['] $tmp $10 base-execute ;

: save-cov ( -- ) \ gforth-exp
    \G Save coverage counters.
    cover-filename r/w create-file throw >r
    cover-start cover-end over - r@ write-file throw
    r> close-file throw ;

: load-cov ( -- ) \ gforth-exp
    \G Load coverage counters.
    cover-filename r/o open-file dup #-514 = IF
	2drop true [: ." no saved coverage found" cr ;] ?warning
	EXIT  THEN  throw  >r
    cover-start r@ file-size throw drop r@ read-file throw
    cover-start + cover-end!
    r> close-file throw ;

true to coverage?

\ coverage tests

[defined] test-it [IF]
    : test1 ( n -- )  0 ?DO  I 3 > ?LEAVE I . LOOP ;
    : yes ." yes" ;
    : no  ." no" ;
    : test2 ( flag -- ) IF  yes  ELSE  no  THEN ;
[THEN]
