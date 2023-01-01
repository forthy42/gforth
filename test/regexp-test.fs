\ regexp test

\ Authors: Bernd Paysan, Anton Ertl, David Kühling
\ Copyright (C) 2005,2007,2009,2010,2018,2019,2022 Free Software Foundation, Inc.

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

coverage? false to coverage?
require test/ttester.fs
to coverage?

charclass [bl-]   blanks +class '-' +char
charclass [0-9(]  '(' +char '0' '9' ..char

: ?str= ( addr1 u1 addr2 u2 -- flag )
    2over 2over str= IF  2drop 2drop true  ELSE
	2swap .\" mismatch, \"" type .\" \"!=\"" type .\" \"" cr false
    THEN ;

: telnum ( addr u -- flag )
    (( {{ ` (  \( \d \d \d \) ` ) || \( \d \d \d \) }}  blanks c?
    \( \d \d \d \) [bl-] c?
    \( \d \d \d \d \) {{ \$ || -\d }} )) ;

: ?tel ( addr u -- ) telnum
    IF  '(' emit \1 type ." ) " \2 type '-' emit \3 type ."  succeeded"
    ELSE \0 type ."  failed" THEN ;

\ --- Telephone number match ---
t{ s" (123) 456-7890" ' ?tel $tmp s" (123) 456-7890 succeeded" ?str= -> true }t
t{ s" (123) 456-7890 " ' ?tel $tmp s" (123) 456-7890 succeeded" ?str= -> true }t
t{ s" (123)-456 7890" ' ?tel $tmp s" (123)-456 7890 failed" ?str= -> true }t
t{ s" (123) 456 789" ' ?tel $tmp s" (123) 456 789 failed" ?str= -> true }t
t{ s" 123 456-7890" ' ?tel $tmp s" (123) 456-7890 succeeded" ?str= -> true }t
t{ s" 123 456-78909" ' ?tel $tmp s" 123 456-78909 failed" ?str= -> true }t

: telnum2 ( addr u -- flag )
    (( // {{ [0-9(] -c? || \^ }}
    {{ ` (  \( \d \d \d \) ` ) || \( \d \d \d \) }}  blanks c?
    \( \d \d \d \) [bl-] c?
    \( \d \d \d \d \) {{ \$ || -\d }} )) ;

: ?tel2 ( addr u -- ) telnum2
    IF   '(' emit \1 type ." ) " \2 type '-' emit \3 type ."  succeeded"
    ELSE \0 type ."  failed" THEN ;

\  --- Telephone number search ---

t{ s" blabla (123) 456-7890" ' ?tel2 $tmp s" (123) 456-7890 succeeded" ?str= -> true }t
t{ s" blabla (123) 456-7890 " ' ?tel2 $tmp s" (123) 456-7890 succeeded" ?str= -> true }t
t{ s" blabla (123)-456 7890" ' ?tel2 $tmp s" blabla (123)-456 7890 failed" ?str= -> true }t
t{ s" blabla (123) 456 789" ' ?tel2 $tmp s" blabla (123) 456 789 failed" ?str= -> true }t
t{ s" blabla 123 456-7890" ' ?tel2 $tmp s" (123) 456-7890 succeeded" ?str= -> true }t
t{ s" blabla 123 456-78909" ' ?tel2 $tmp s" blabla 123 456-78909 failed" ?str= -> true }t
t{ s" (123) 456-7890" ' ?tel2 $tmp s" (123) 456-7890 succeeded" ?str= -> true }t
t{ s"  (123) 456-7890 " ' ?tel2 $tmp s" (123) 456-7890 succeeded" ?str= -> true }t
t{ s" a (123)-456 7890" ' ?tel2 $tmp s" a (123)-456 7890 failed" ?str= -> true }t
t{ s" la (123) 456 789" ' ?tel2 $tmp s" la (123) 456 789 failed" ?str= -> true }t
t{ s" bla 123 456-7890" ' ?tel2 $tmp s" (123) 456-7890 succeeded" ?str= -> true }t
t{ s" abla 123 456-78909" ' ?tel2 $tmp s" abla 123 456-78909 failed" ?str= -> true }t

\ --- Number extraction test ---

charclass [0-9,./:]  '0' '9' ..char ',' +char '.' +char '/' +char ':' +char

: ?num
    (( // \( {++ [0-9,./:] c? ++} \) ))
    IF  \1 type  ELSE  \0 type ."  failed"  THEN ;

t{ s" 1234" ' ?num $tmp s" 1234" ?str= -> true }t
t{ s" 12,345abc" ' ?num $tmp s" 12,345" ?str= -> true }t
t{ s" foobar12/345:678.9abc" ' ?num $tmp s" 12/345:678.9" ?str= -> true }t
t{ s" blafasel" ' ?num $tmp s" blafasel failed" ?str= -> true }t

\ --- String test ---

: ?string
    (( // \( {{ =" foo" || =" bar" || =" test" }} \) ))
    IF  \1 type  ELSE  \0 type ."  failed" THEN ;

t{ s" dies ist ein test" ' ?string $tmp s" test" ?str= -> true }t
t{ s" foobar" ' ?string $tmp s" foo" ?str= -> true }t
t{ s" baz bar foo" ' ?string $tmp s" bar" ?str= -> true }t
t{ s" Hier kommt nichts vor" ' ?string $tmp s" Hier kommt nichts vor failed" ?str= -> true }t

\ --- longer matches test ---

: ?foos
    (( \( {** =" foo" **} \) ))
    IF  \1 type  ELSE  \0 type ."  failed"  THEN ;

: ?foobars
    (( // \( {** =" foo" **} \) \( {++ =" bar" ++} \) ))
    IF  \1 type ',' emit \2 type  ELSE  \0 type ."  failed"  THEN ;

: ?foos1
    (( // \( {+ =" foo" +} \) \( {++ =" bar" ++} \) ))
    IF  \1 type ',' emit \2 type  ELSE  \0 type ."  failed"  THEN ;

t{ s" foobar" ' ?foos $tmp s" foo" ?str= -> true }t
t{ s" foofoofoobar" ' ?foos $tmp s" foofoofoo" ?str= -> true }t
t{ s" fofoofoofofooofoobarbar" ' ?foos $tmp s" " ?str= -> true }t
t{ s" bla baz bar" ' ?foos $tmp s" " ?str= -> true }t
t{ s" foofoofoo" ' ?foos $tmp s" foofoofoo" ?str= -> true }t

t{ s" foobar" ' ?foobars $tmp s" foo,bar" ?str= -> true }t
t{ s" foofoofoobar" ' ?foobars $tmp s" foofoofoo,bar" ?str= -> true }t
t{ s" fofoofoofofooofoobarbar" ' ?foobars $tmp s" foo,barbar" ?str= -> true }t
t{ s" bla baz bar" ' ?foobars $tmp s" ,bar" ?str= -> true }t
t{ s" foofoofoo" ' ?foobars $tmp s" foofoofoo failed" ?str= -> true }t

t{ s" foobar" ' ?foos1 $tmp s" foo,bar" ?str= -> true }t
t{ s" foofoofoobar" ' ?foos1 $tmp s" foofoofoo,bar" ?str= -> true }t
t{ s" fofoofoofofooofoobarbar" ' ?foos1 $tmp s" foo,barbar" ?str= -> true }t
t{ s" bla baz bar" ' ?foos1 $tmp s" bla baz bar failed" ?str= -> true }t
t{ s" foofoofoo" ' ?foos1 $tmp s" foofoofoo failed" ?str= -> true }t

\ backtracking on decissions

: ?aab ( addr u -- flag )
   (( {{ =" aa" || =" a" }} {{ =" ab" || =" a" }} )) ;
s" aab" ?aab 0= [IF] .( aab failed!) cr [THEN]

\ buffer overrun test (bug in =")

\ --- buffer overrun test ---

 : ?long-string
    (( // \( =" abcdefghi" \) ))
    IF  \1 type THEN ;

4096 allocate throw 4096 + 8 - constant test-string
 s" abcdefgh" test-string swap cmove>
\ provoking overflow [i.e. see valgrind output]
t{ test-string 8 ' ?long-string $tmp s" " ?str= -> true }t

\ --- simple replacement test ---

: delnum  ( addr u -- addr' u' )   s// \d >> s" " //g ;
: test-delnum  ( addr u addr' u' -- )
   2swap delnum 2over 2over str= 0= IF
      ." test-delnum: got '" type ." ', expected '" type ." '" false
   ELSE  2drop 2drop true THEN ;
t{ s" 0"  s" " test-delnum -> true }t
t{ s" 00"  s" " test-delnum -> true }t
t{ s" 0a"  s" a" test-delnum -> true }t
t{ s" a0"  s" a" test-delnum -> true }t
t{ s" aa"  s" aa" test-delnum -> true }t

: delcomment  ( addr u -- addr' u' )  s// ` # {** .? **} >> s" " //g ;
t{ s" hello # test " delcomment s" hello " ?str= -> true }t
: delparents  ( addr u -- addr' u' )  s// ` ( {* .? *} ` ) >> s" ()" //g ;
t{ s" delete (test) and (another test) " delparents s" delete () and () " ?str= -> true }t

\ --- replacement tests ---

: hms>s ( addr u -- addr' u' )
  s// \( \d \d \) ` : \( \d \d \) ` : \( \d \d \) >>
  \1 s>number drop 60 *
  \2 s>number drop + 60 *
  \3 s>number drop + 0 <# 's' hold #s #> //g ;

t{ s" bla 12:34:56 fasel 00:01:57 blubber" hms>s s" bla 45296s fasel 117s blubber" ?str= -> true }t

: hms>s,del() ( addr u -- addr' u' )
  s// {{ ` ( // ` ) >> <<" ()"
      || \( \d \d \) ` : \( \d \d \) ` : \( \d \d \)
         >> \1 s>number drop 60 *
            \2 s>number drop + 60 *
            \3 s>number drop + 0 <# 's' hold #s #> <<
      }} LEAVE //s ;

t{ s" (bla) 12:34:56 (fasel) 00:01:57 (blubber)" hms>s,del() s" () 45296s () 117s ()" ?str= -> true }t

\ more tests from David Kühling

: underflow1  ( c-addr u -- flag )
   (( {{
         {{ ` - || }} \d
         || \d
      }} )) ;
T{ s" -1dummy" underflow1 -> true }T

: underflow2  ( -- )
   (( \( {{ \s {** \s **} 
	 || =" /*" // =" */"
	 || =" //" {** \d **} }} \) )) ;
T{ s" /*10203030203030404*/   " underflow2 -> true }T
T{ pad 0 underflow2 -> false }T

charclass [*] '* +char
charclass [*/] '* +char '/ +char

: underflow3  ( -- )
   ((
      =" /*"
      \( {** {{ [*] -c? || ` * [*/] -c? }} **} \)
      {++ ` * ++} ` /
   )) ;

\ this still seems to be too complicated
T{ s" /*10203030203030404*/   " underflow3 -> true }T
t{ \1 s" 10203030203030404" ?str= -> true }t

: underflow4  ( -- )
   (( \( {{ {** \d **} || {** \d **} }} \d \) )) ;

T{ s" 0  " underflow4 -> true }T

coverage? [IF] .coverage cov% cr [THEN]
script? [IF]
    #ERRORS @ 0= [IF] ." passed" [ELSE] #ERRORS ? ." errors" [THEN]
    cr bye
[THEN]
