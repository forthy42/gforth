\ Regexp compiler

\ Copyright (C) 2005,2006,2007,2008,2010 Free Software Foundation, Inc.

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

\ The idea of the parser is the following:
\ As long as there's a match, continue
\ On a mismatch, LEAVE.
\ Insert appropriate control structures on alternative branches
\ Keep the old pointer (backtracking) on the stack
\ I try to keep the syntax as close to a real regexp system as possible
\ All regexp stuff is compiled into one function as forward branching
\ state machine

\ special control structure

: FORK ( compilation -- orig ; run-time f -- ) \ gforth
    \G AHEAD-like control structure: calls the code after JOIN.
    POSTPONE call >mark ; immediate restrict
: JOIN ( orig -- ) \ gforth
    \G THEN-like control structure for FORK
    postpone THEN ; immediate restrict

\ Charclasses

: +bit ( addr n -- )  + 1 swap c! ;
: -bit ( addr n -- )  + 0 swap c! ;
: @+ ( addr -- n addr' )  dup @ swap cell+ ;

0 Value cur-class
: charclass ( -- ) \ regexp-cg
    \G Create a charclass
    Create here dup to cur-class $100 dup allot erase ;
: +char ( char -- ) \ regexp-cg
    \G add a char to the current charclass
    cur-class swap +bit ;
: -char ( char -- ) \ regexp-cg
    \G remove a char from the current charclass
    cur-class swap -bit ;
: ..char ( start end -- ) \ regexp-cg
    \G add a range of chars to the current charclass
    1+ swap ?DO  I +char  LOOP ;
: or! ( n addr -- )  dup @ rot or swap ! ;
: and! ( n addr -- )  dup @ rot and swap ! ;
: +class ( class -- ) \ regexp-cg
    \G union of charclass @var{class} and the current charclass
    $100 0 ?DO  @+ swap
    cur-class I + or!  cell +LOOP  drop ;
: -class ( class -- ) \ regexp-cg
    \G subtract the charclass @var{class} from the current charclass
    $100 0 ?DO  @+ swap invert
    cur-class I + and!  cell +LOOP  drop ;

: char? ( addr class -- addr' flag )
    >r count r> + c@ ;

\ Charclass tests

: c? ( addr class -- ) \ regexp-pattern
    \G check @var{addr} for membership in charclass @var{class}
    ]] char? 0= ?LEAVE [[ ; immediate
: -c? ( addr class -- ) \ regexp-pattern
    \G check @var{addr} for not membership in charclass @var{class}
    ]] char?    ?LEAVE [[ ; immediate

charclass digit  '0 '9 ..char
charclass blanks 0 bl ..char
\ bl +char #tab +char #cr +char #lf +char ctrl L +char
charclass letter 'a 'z ..char 'A 'Z ..char
charclass any    0 $FF ..char #lf -char

: \d ( addr -- addr' ) \ regexp-pattern
    \G check for digit
    ]] digit c?        [[ ; immediate
: \s ( addr -- addr' ) \ regexp-pattern
    \G check for blanks
    ]] blanks c?       [[ ; immediate
: .? ( addr -- addr' ) \ regexp-pattern
    \G check for any single charachter
    ]] any c?          [[ ; immediate
: -\d ( addr -- addr' ) \ regexp-pattern
    \G check for not digit
    ]] digit -c?       [[ ; immediate
: -\s ( addr -- addr' ) \ regexp-pattern
    \G check for not blank
    ]] blanks -c?      [[ ; immediate
: ` ( "char" -- ) \ regexp-pattern
    \G check for particular char
    ]] count [[  char ]] Literal <> ?LEAVE [[ ;  immediate
: -` ( "char" -- ) \ regexp-pattern
    \G check for particular char
    ]] count [[  char ]] Literal = ?LEAVE [[ ;  immediate

\ loop stack

Variable loops  $40 3 * cells allot
: 3@ ( addr -- a b c )  dup >r 2 cells + @ r> 2@ ;
: 3! ( a b c addr -- )  dup >r 2! r> 2 cells + ! ;
: loops> ( -- addr ) -3 loops +!  loops @+ swap cells + 3@ ;
: >loops ( addr -- ) loops @+ swap cells + 3! 3 loops +! ;
: BEGIN, ( -- )  ]] BEGIN [[ >loops ;
: DONE, ( -- )  loops @ IF  loops> ]] DONE [[ ELSE ." no done left!" cr THEN ;

\ variables

Variable vars   &18 cells allot
Variable varstack 9 cells allot
Variable varsmax
: >var ( -- addr )
    vars @+ swap 2* cells +
    vars @ varstack @+ swap cells + !
    1 vars +! 1 varstack +! ;
: var> ( -- addr )
    -1 varstack +!
    varstack @+ swap cells + @
    1+ 2* cells vars + ;
Variable greed-counts  9 cells allot \ no more than 9 nested greedy loops
: greed' ( -- addr )  greed-counts dup @ + ;

\ start end

0 Value end$
0 Value last$
0 Value start$
: !end ( addr u -- addr )  over + to end$ dup to start$ ;
: end-rex? ( addr -- addr flag ) dup end$ u< ;
: start-rex? ( addr -- addr flag ) dup start$ u> ;
: ?end ( addr -- addr ) ]] dup end$ u> ?LEAVE [[ ; immediate
: rest$ ( addr -- addr addr u ) dup end$ over - ;
: >last ( addr -- flag )  dup to last$ end$ u<= ;

\ start and end

: \^ ( addr -- addr ) \ regexp-pattern
    \G check for string start
    ]] start-rex? ?LEAVE [[ ; immediate
: \$ ( addr -- addr ) \ regexp-pattern
    \G check for string end
    ]] end-rex? ?LEAVE [[ ; immediate

\ A word for string comparison

: (str=?) ( addr1 addr u -- addr2 )
    dup >r 2>r rest$ r@ umin 2r> compare IF rdrop true ELSE r> + false THEN ;
: str=? ( addr1 addr u -- addr2 ) ]] (str=?) ?LEAVE [[ ; immediate
: ,=" ( addr u -- ) tuck dup ]] rest$ Literal umin SLiteral compare ?LEAVE Literal + [[ ;
: =" ( <string>" -- ) \ regexp-pattern
    \G check for string
    '" parse ,=" ; immediate

\ regexp block

\ FORK/JOIN are like AHEAD THEN, but producing a call on AHEAD
\ instead of a jump.

: (( ( addr u -- ) \ regexp-pattern
    \G start regexp block
    vars off varsmax off loops off greed-counts off
    ]] FORK  AHEAD BUT JOIN !end [[ BEGIN, ; immediate
: )) ( -- flag ) \ regexp-pattern
    \G end regexp block
    ]] >last  ;S [[
    DONE, ]] drop false ;S THEN [[ ; immediate

\ greedy loops

\ Idea: scan as many characters as possible, try the rest of the pattern
\ and then back off one pattern at a time

: drops ( n -- ) 1+ cells sp@ + sp! ;

: {** ( addr -- addr addr ) \ regexp-pattern
    \G greedy zero-or-more pattern
    ]] false >r BEGIN  dup  FORK  BUT  WHILE  last$ r> 1+ >r  REPEAT [[
    ]] r>  AHEAD  BUT  JOIN [[
    BEGIN, ; immediate
' {** Alias {++ ( addr -- addr addr ) \ regexp-pattern
    \G greedy one-or-more pattern
    immediate
: **} ( sys -- ) \ regexp-pattern
    \G end of greedy zero-or-more pattern
    ]] >last  ;S [[ DONE, ]] drop false ;S  THEN [[
    ]] 1+ false  U+DO  FORK BUT [[
    ]] IF  I' I - 1- drops UNLOOP  true ;S  THEN  LOOP [[
    ]] false ;S JOIN [[ ; immediate
: ++} ( sys -- ) \ regexp-pattern
    \G end of greedy zero-or-more pattern
    ]] >last  ;S [[ DONE, ]] drop false ;S  THEN [[
    ]] false  U+DO  FORK BUT [[
    ]] IF  I' I - drops UNLOOP  true ;S  THEN  LOOP [[
    ]] drop false ;S JOIN [[ ; immediate

\ non-greedy loops

\ Idea: Try to match rest of the regexp, and if that fails, try match
\ first expr and then try again rest of regexp.

: {+ ( addr -- addr addr ) \ regexp-pattern
    \G non-greedy one-or-more pattern
    ]] BEGIN  [[ BEGIN, ; immediate
: {* ( addr -- addr addr ) \ regexp-pattern
    \G non-greedy zero-or-more pattern
    ]] {+ dup FORK BUT  IF  drop true  ;S THEN [[ ; immediate
: *} ( addr addr' -- addr' ) \ regexp-pattern
    \G end of non-greedy zero-or-more pattern
    ]] dup end$ u>  UNTIL [[
    DONE, ]] drop false  ;S  JOIN [[ ; immediate
: +} ( addr addr' -- addr' ) \ regexp-pattern
    \G end of non-greedy one-or-more pattern
    ]] dup FORK BUT  IF  drop true  ;S [[
    DONE, ]] drop false  ;S [[ BEGIN, ]] THEN *} [[ ; immediate

: // ( -- ) \ regexp-pattern
    \G search for string
    ]] {* 1+ *} [[ ; immediate

\ alternatives

\ idea: try to match one alternative and then the rest of regexp.
\ if that fails, jump back to second alternative

: THENs ( sys -- )  BEGIN  dup  WHILE  ]] THEN [[  REPEAT  drop ;

: {{ ( addr -- addr addr ) \ regexp-pattern
    \G Start of alternatives
    0 ]] dup FORK  IF  drop true ;S  BUT  JOIN [[ vars @ ; immediate
: || ( addr addr -- addr addr ) \ regexp-pattern
    \G separator between alternatives
    vars @ varsmax @ max varsmax !  vars !
    ]] AHEAD  BUT  THEN  [[
    ]] dup FORK  IF  drop true ;S  BUT  JOIN [[ vars @ ; immediate
: }} ( addr addr -- addr ) \ regexp-pattern
    \G end of alternatives
    vars @ varsmax @ max vars !  drop
    ]] AHEAD  BUT  THEN  drop false ;S [[  THENs ; immediate

\ match variables

: \( ( addr -- addr ) \ regexp-pattern
    \G start of matching variable; variables are referred as \\1--9
    ]] dup [[
    >var ]] ALiteral ! [[ ; immediate
: \) ( addr -- addr ) \ regexp-pattern
    \G end of matching variable
    ]] dup [[
    var> ]] ALiteral ! [[ ; immediate
: \0 ( -- addr u ) \ regexp-pattern
    \G the whole string
    start$ end$ over - ;
: \: ( i -- )
    Create 2* 1+ cells vars + ,
  DOES> ( -- addr u ) @ 2@ tuck - ;
: \:s ( n -- ) 0 ?DO  I \:  LOOP ;
9 \:s \1 \2 \3 \4 \5 \6 \7 \8 \9

\ replacements, needs string.fs

require string.fs

0 Value >>ptr
0 Value <<ptr
Variable >>string
: s>>  ( addr -- addr ) \ regexp-replace
    \G Start replace pattern region
    dup to >>ptr ;
: << ( run-addr addr u -- run-addr ) \ regexp-replace
    \G Replace string from start of replace pattern region with
    \G @var{addr} @var{u}
    <<ptr >>ptr over - >>string $+!
    >>string $+! dup to <<ptr ;
: <<" ( "string<">" -- ) \ regexp-replace
    \G Replace string from start of replace pattern region with
    \G @var{string}
    '" parse postpone SLiteral postpone << ; immediate
: >>string@ ( -- addr u )
    >>string $@ ;
: >>string0 ( addr u -- addr u )
    s" " >>string $!
    0 to >>ptr  over to <<ptr ;
: >>next ( -- addr u ) <<ptr end$ over - ;
: >>rest ( -- ) >>next >>string $+! ;
: s// ( addr u -- ptr )
    \G start search/replace loop
    ]] >>string0 (( // s>> [[ ; immediate
: >> ( addr -- addr )
    ]] <<ptr >>ptr u> ?LEAVE ?end [[ ; immediate
: //s ( ptr -- )
    \G search end
    ]] )) drop >>rest >>string@ [[ ; immediate
: //o ( ptr addr u -- addr' u' )
    \G end search/replace single loop
    ]] << //s [[ ; immediate
: //g ( ptr addr u -- addr' u' )
    \G end search/replace all loop
    ]] << LEAVE //s [[ ; immediate
