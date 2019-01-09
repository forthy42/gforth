\ Tokenize Forth source code

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

Vocabulary tokenizer

tokenizer also definitions

Variable tokens[]

: ?token { nt -- index t / f }
    tokens[] $@ bounds ?DO
	nt I @ = IF  I tokens[] $@ drop - cell/ true  UNLOOP  EXIT  THEN
    cell +LOOP  addr nt cell tokens[] $+!  false ;

\ token format:
\ 1 count string  -> nt rectype-name ( need to convert string to nt first)
\ 2 xchar         -> nt rectype-name
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

Create blacklist \ things we don't want to tokenize, e.g. comments
' ( , ' \ ,
here blacklist - Constant blacklist#
Variable blacklisted

: ?blacklist ( nt -- flag )
    blacklist blacklist# bounds ?DO
	dup I @ = IF  drop true  UNLOOP  EXIT  THEN
    cell +LOOP  drop false ;

: tokenize-it ( rectype rec-xt -- rectype )
    drop case dup
	rectype-name of
	    over ?blacklist IF
		blacklisted on
	    ELSE
		over ?token IF  2 emit xemit
		ELSE  1 i,  THEN
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

forth definitions

: >tokenize ( addr u -- )
    r/w create-file throw to token-file
    ['] parse-name' is parse-name
    ['] parse'      is parse
    ['] tokenize    is trace-recognizer ;

: tokenize-file ( addr u -- )
    2dup '.' -scan [: type ." .ft" ;] $tmp >tokenize included
    reset-interpreter ;

tokenizer definitions

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

: >nt ( -- nt )
    >parsed find-name { nt } addr nt cell tokens[] $+! nt ;
: nt@ ( -- nt )
    tokens[] $@ xc-token cells safe/string drop @ ;

: token-parse ( -- addr u )
    token@ 9 <> !!token!! and throw
    >parsed ;

: token-nt-name ( -- nt rectype-name )
    >nt rectype-name ;
: token-nt ( -- nt rectype-name )
    nt@ rectype-name ;
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
    >parsed recognize ;

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

: token-recognizer ( n dummy -- ... rectype )
    drop cells token-actions + perform ;

1 stack: token-recognizers

' token-recognizer 1 token-recognizers set-stack

: token-int ( -- )  rp@ backtrace-rp0 !
    BEGIN  ?stack token-pos# tokens$ $@ + u< WHILE
	    token@ 0 parser1 int-execute
    REPEAT ;

forth definitions

: tokenize> ( addr u -- )
    open-fpath-file throw 2drop tokens$ $slurp
    tokens$ $@ drop to token-pos#
    forth-recognizer >r  token-recognizers to forth-recognizer
    ['] token-parse is parse
    ['] token-parse is parse-name
    ['] token-int catch  reset-interpreter
    r> to forth-recognizer
    tokens$ $free  0 to token-pos#  throw ;

previous

script? [IF]
    next-arg 2dup + 3 - 3 s" .ft" str= [IF]
	tokenize>
    [ELSE]
	tokenize-file bye
    [THEN]
[THEN]
