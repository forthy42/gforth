\ Tokenize Forth source code

\ Author: Bernd Paysan
\ Copyright (C) 2019 Free Software Foundation, Inc.

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

:noname ( ... -- ... )
    depth IF  ...  THEN
    fdepth IF  cr "F:" type f.s  THEN ; is printdebugdata

Vocabulary tokenizer

get-current also tokenizer definitions

Variable tokens[]

: +nt ( nt -- ) { w^ nt } nt cell tokens[] $+! ;
: ?token { nt -- index t / f }
    tokens[] $@ bounds ?DO
	nt I @ = IF  I tokens[] $@ drop - cell/ true  UNLOOP  EXIT  THEN
    cell +LOOP  nt +nt  false ;
: ?token-0 { nt -- }
    tokens[] $@ bounds ?DO
	nt I @ = IF  I off  LEAVE  THEN
    cell +LOOP  ;

\ remove tokens when local lists are adjusted

: drop-tokens ( wid -- )
    locals-list @
    BEGIN  2dup <> over 0<> and  WHILE
	    dup ?token-0 >link @  REPEAT  2drop ;

:noname ( wid -- )
    dup drop-tokens defers locals-list!
; is locals-list!

\ read tokenized input

Variable tokens$
0 Value token-pos#

s" unexpected token" exception constant !!token!!

: token@ ( -- token )
    token-pos# c@  1 +to token-pos# ;
: xc-token ( -- xchar )
    token-pos# xc@+ swap to token-pos# ;

: >parsed ( -- addr u )
    xc-token token-pos# swap dup +to token-pos# ;

\ backup recognizer

forth-recognizer value backup-recognizer

: backup-recognize ( addr u -- ... token )
    forth-recognizer >r backup-recognizer dup to forth-recognizer
    recognize  r> to forth-recognizer ;

: >nt ( -- nt )
    >parsed 2dup find-name dup IF  dup +nt nip nip
    ELSE  drop backup-recognize rectype-nt <> !!token!! and throw
	dup +nt  THEN ;
: nt@ ( -- nt )
    tokens[] $@ xc-token cells safe/string drop @ ;

\ token format:
\ 1 count string  -> nt rectype-nt ( need to convert string to nt first)
\ 2 xchar         -> nt rectype-nt
\ 3 cell          -> n rectype-num
\ 4 2*cell        -> d rectype-dnum
\ 5 float         -> f rectype-float
\ 6 string        -> addr u rectype-string
\ 7 count string  -> nt rectype-to ( need to convert string to nt first)
\ 8 xchar         -> nt rectype-to
\ 9 xcount string -> raw parse or parse-name input

Variable parsed-name$

: i, ( token -- )
    emit input-lexeme 2@ dup xemit type ;
: nt, ( nt token -- )
    emit name>string dup xemit type ;

Create blacklist \ things we don't want to tokenize, e.g. comments
' ( ,   ' noop , \ )
' \ ,   ' noop ,
' \G ,  ' noop ,
' \\\ , ' noop ,
also locals-types
' -- ,  ' } ,
' w: ,  ' w: ,
previous
here Constant blacklist-end
Variable blacklisted
Variable recursive?

: ?blacklist ( nt -- nt' )
    blacklist-end blacklist ?DO
	dup I @ = IF  drop I cell+ @  UNLOOP  EXIT  THEN
    2 cells +LOOP  drop false ;

: tokenize-it ( rectype rec-xt -- rectype )
    drop recursive? @ ?EXIT
    case dup
	rectype-nt of
	    over ?blacklist dup IF
		[ also locals-types ]
		dup ['] w: <> blacklisted !
		[ previous ]
		dup ['] noop <> IF
		    dup ?token  IF  2 emit xemit drop
		    ELSE  1 nt,  THEN
		ELSE  drop  THEN
	    ELSE
		drop
		over ?token IF  2 emit xemit
		ELSE  1 i,  THEN
	    THEN
	    nextname$ $@ d0<> IF
		9 emit nextname$ $@ dup xemit type
	    THEN
	endof
	rectype-num of
	    3 emit >r dup { w^ x } x cell type r>
	endof
	rectype-dnum of
	    4 emit >r 2dup { d^ x } x 2 cells type r>
	endof
	rectype-float of
	    5 emit >r fdup { f^ x } x 1 floats type r>
	endof
	rectype-string of
	    6 emit >r 2dup dup xemit type r>
	endof
	rectype-to of
	    over ?token IF  8 emit xemit
	    ELSE  7 i,  THEN
	endof
	9 i,
    endcase  parsed-name$ $free ;

0 Value token-file

: t, ( ... xt -- )
    token-file outfile-execute ;

: tokenize ( rectype rec-xt -- rectype )
    ['] tokenize-it t, ;

: parse-name' ( -- addr u )
    parsed-name$ $@len blacklisted @ 0= and IF
	parsed-name$ $@ [: 9 emit dup xemit type ;] t,
    THEN
    blacklisted off
    defers parse-name 2dup parsed-name$ $! ;

: parse' ( char -- addr u )
    defers parse
    blacklisted @ 0= IF
	[: 9 emit dup xemit 2dup type ;] t,
    THEN  blacklisted off ;

: reset-interpreter ( -- )
    [ action-of parse-name       ]L is parse-name
    [ action-of parse            ]L is parse
    [ action-of trace-recognizer ]L is trace-recognizer ;

dup set-current

: >tokenize ( addr u -- )
    r/w create-file throw to token-file
    ['] parse-name' is parse-name
    ['] parse'      is parse
    ['] tokenize    is trace-recognizer ;

: tokenize-file ( addr u -- )
    2dup '.' -scan [: type ." .ft" ;] $tmp >tokenize included
    reset-interpreter ;

tokenizer definitions

: token-parse ( -- addr u )
    case  token@
	1 of  >parsed 2dup find-name +nt  endof
	2 of  nt@ name>string             endof
	7 of  >parsed                     endof
	9 of  >parsed                     endof
	!!token!! throw
    endcase ;

: token-nt-name ( -- nt rectype-nt )
    >nt rectype-nt ;
: token-nt ( -- nt rectype-nt )
    nt@ rectype-nt ;
: token-num ( -- n rectype-num )
    token-pos# @ 1 cells +to token-pos# rectype-num ;
: token-dnum ( -- n rectype-num )
    token-pos# 2@ 2 cells +to token-pos# rectype-dnum ;
: token-float ( -- n rectype-num )
    token-pos# f@ 1 floats +to token-pos# rectype-float ;
: token-string ( -- addr u rectype-string )
    >parsed rectype-string ;
: token-to-name ( -- nt rectype-to )
    >nt rectype-to ;
: token-to ( -- nt rectype-to )
    nt@ rectype-to ;
: token-generic ( -- ... rectype-??? )
    >parsed backup-recognize ;

Create token-actions
0                ,
' token-nt-name  ,
' token-nt       ,
' token-num      ,
' token-dnum     ,
' token-float    ,
' token-string   ,
' token-to-name  ,
' token-to       ,
' token-generic  ,

: token-recognizer ( n 0 / addr u -- ... rectype )
    ?dup-IF  backup-recognize  ELSE
	cells token-actions + perform
    THEN ;

1 stack: token-recognizers

' token-recognizer 1 token-recognizers set-stack

: token-int ( -- )
    BEGIN  ?stack token-pos# tokens$ $@ + u< WHILE
	    token@ 0 parser1 int-execute
    REPEAT ;

set-current

: tokenize> ( addr u -- )
    open-fpath-file throw 2drop tokens$ $slurp
    tokens$ $@ drop to token-pos#
    forth-recognizer to backup-recognizer
    token-recognizers to forth-recognizer
    ['] token-parse is parse-name
    [: drop token-parse ;] is parse
    ['] token-int catch  reset-interpreter
    backup-recognizer to forth-recognizer
    dup IF
	." Error at byte " token-pos# tokens$ $@ drop - hex. cr
    THEN
    tokens$ $free  0 to token-pos#  throw ;

previous

script? [IF]
    next-arg 2dup + 3 - 3 s" .ft" str= [IF]
	tokenize>
    [ELSE]
	tokenize-file bye
    [THEN]
[THEN]
