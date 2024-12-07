\ Tokenize Forth source code

\ Author: Bernd Paysan
\ Copyright (C) 2019,2020,2021,2023 Free Software Foundation, Inc.

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

require recognizer-ext.fs

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

Defer backup-recognize

: >nt ( -- nt )
    >parsed 2dup find-name dup IF  dup +nt nip nip
    ELSE  drop backup-recognize ['] translate-nt <> !!token!! and throw
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
: n, ( addr u -- ) 9 emit dup xemit type ;

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

: ?blacklist ( nt -- nt' )
    blacklist-end blacklist ?DO
	dup I @ = IF  drop I cell+ @  UNLOOP  EXIT  THEN
    2 cells +LOOP  drop false ;

translate-method: tokenizing

' translate-nt :method tokenizing ( xt -- xt )
    dup ?blacklist dup IF
	[ also locals-types ]
	dup ['] w: <> blacklisted !
	[ previous ]
	dup ['] noop <> IF
	    dup ?token  IF  2 emit xemit drop
	    ELSE  1 nt,  THEN
	ELSE  drop  THEN
    ELSE
	drop dup ?token IF  2 emit xemit
	ELSE  1 i,  THEN
    THEN
    nextname$ $@ d0<> IF
	nextname$ $@ n,
    THEN ;
' translate-num :method tokenizing ( n -- n )
    3 emit dup { w^ x } x cell type ;
' translate-dnum :method tokenizing ( d -- d )
    4 emit 2dup { d^ x } x 2 cells type ;
' translate-float :method tokenizing ( r -- r )
    5 emit fdup { f^ x } x 1 floats type ;
' translate-string :method tokenizing ( addr u -- addr u )
    6 emit 2dup dup xemit type ;
' translate-to :method tokenizing ( xt -- )
    over ?token IF  8 emit xemit
    ELSE  7 i,  THEN ;

: tokenize-it ( rectype rec-xt -- rectype )
    drop rec-level @ 0> ?EXIT
    \ dup [: .addr. space rec-level ? cr ;] stdout outfile-execute
    ?scan-string dup >r ['] tokenizing catch
    dup #-13 = IF  2drop  9 i,  ELSE  throw  THEN  r>
    parsed-name$ $free ;

0 Value token-file

: t, ( ... xt -- )
    token-file outfile-execute ;

: tokenize ( rectype rec-xt -- rectype )
    ['] tokenize-it t, ;

: parse-name' ( -- addr u )
    parsed-name$ $@len blacklisted @ 0= and IF
	parsed-name$ $@ ['] n, t,
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

: (tokenize-file) ( addr u -- )
    r/w create-file throw to token-file
    ['] parse-name' is parse-name
    ['] parse'      is parse
    ['] tokenize    is trace-recognizer ;

: tokenize-file ( addr u -- )
    2dup '.' -scan [: type ." .ft" ;] $tmp (tokenize-file) included
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

: token-nt-name ( -- nt translate-nt )
    >nt ['] translate-nt ;
: token-nt ( -- nt translate-nt )
    nt@ ['] translate-nt ;
: token-num ( -- n translate-num )
    token-pos# @ 1 cells +to token-pos# ['] translate-num ;
: token-dnum ( -- n translate-dnum )
    token-pos# 2@ 2 cells +to token-pos# ['] translate-dnum ;
: token-float ( -- n translate-float )
    token-pos# f@ 1 floats +to token-pos# ['] translate-float ;
: token-string ( -- addr u translate-string )
    >parsed ['] translate-string ;
: token-to-name ( -- nt translate-to )
    >nt ['] translate-to ;
: token-to ( -- nt translate-to )
    nt@ ['] translate-to ;
: token-generic ( -- ... translate-??? )
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

: rec-token
    dup 0= IF drop cells token-actions + perform
    ELSE  2drop 0  THEN ;

Create !token-table ' warn! A, ' n/a A, ' n/a A, [: 0 swap $[] @ defer@ ;] A,   [: 0 swap $[] @ defer! ;] A,

' >body !token-table to-class: token-to-class

' backup-recognize ' rec-token 2 recognizer-sequence: token-recognize
\ transfer defer@ and defer! from token-recognize to backup-recognize
' token-to-class set-to

: token-int ( -- )
    BEGIN  ?stack token-pos# tokens$ $@ + u< WHILE
	    token@ 0 forth-recognize execute
    REPEAT ;

set-current

: tokenize> ( addr u -- )
    open-fpath-file throw 2drop tokens$ $slurp
    tokens$ $@ drop to token-pos#
    action-of forth-recognize is backup-recognize
    ['] token-recognize is forth-recognize
    ['] token-parse is parse-name
    [: drop token-parse ;] is parse
    ['] token-int catch  reset-interpreter
    action-of backup-recognize is forth-recognize
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
