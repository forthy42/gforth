\ Hashed dictionaries                                  15jul94py

\ Copyright (C) 1995 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

11 value hashbits
1 hashbits lshift Value Hashlen

Variable insRule        insRule on
Variable revealed

\ Memory handling                                      10oct94py

Variable HashPointer
Variable HashIndex
0 Value HashTable

\ DelFix and NewFix are from bigFORTH                  15jul94py

: DelFix ( addr root -- ) dup @ 2 pick ! ! ;
: NewFix  ( root len # -- addr )
  BEGIN  2 pick @ ?dup  0= WHILE  2dup * allocate throw
         over 0 ?DO  dup 4 pick DelFix 2 pick +  LOOP  drop
  REPEAT  >r drop r@ @ rot ! r@ swap erase r> ;

\ compute hash key                                     15jul94py

: hash ( addr len -- key )
    hashbits (hashkey1) ;
\   (hashkey)
\   Hashlen 1- and ;

: bucket ( addr len wordlist -- bucket-addr )
    \ @var{bucket-addr} is the address of a cell that points to the first
    \ element in the list of the bucket for the string @var{addr len}
    wordlist-extend @ -rot hash xor ( bucket# )
    cells HashTable + ;

: hash-find ( addr len wordlist -- nfa / false )
    >r 2dup r> bucket @ (hashfind) ;

\ hash vocabularies                                    16jul94py

: lastlink! ( addr link -- )
  BEGIN  dup @ dup  WHILE  nip  REPEAT  drop ! ;

: (reveal ( nfa wid -- )
    dup wordlist-extend @ 0<
    IF
	2drop EXIT
    THEN
    over name>string rot bucket >r
    HashPointer 2 Cells $400 NewFix
    tuck cell+ ! r> insRule @
    IF
	dup @ 2 pick ! !
    ELSE
	lastlink!
    THEN
    revealed on ;

: hash-reveal ( nfa wid -- )
    2dup (reveal) (reveal ;

: addall  ( -- )
    voclink
    BEGIN  @ dup @  WHILE  dup 'initvoc  REPEAT  drop ;

: clearhash  ( -- )
    HashTable Hashlen cells bounds
    DO  I @
	BEGIN  dup  WHILE
	       dup @ swap HashPointer DelFix
        REPEAT  I !
    cell +LOOP  HashIndex off ;

: re-hash  clearhash addall ;
: (rehash) ( addr -- )
  drop revealed @ IF  re-hash revealed off  THEN ;

Create hashsearch-map ( -- wordlist-map )
    ' hash-find A, ' hash-reveal A, ' (rehash) A,

\ hash allocate and vocabulary initialization          10oct94py

: hash-alloc ( addr -- addr )  HashTable 0= IF
  Hashlen cells allocate throw TO HashTable
  HashTable Hashlen cells erase THEN
  HashIndex @ over !  1 HashIndex +!
  HashIndex @ Hashlen >=
  IF  HashTable >r clearhash
      1 hashbits 1+ dup  to hashbits  lshift  to hashlen
      r> free >r  0 to HashTable
      addall r> throw
  THEN ;

: (initvoc) ( addr -- )
    cell+ dup @  0< IF  drop EXIT  THEN
    dup 2 cells - @ hashsearch-map <> IF  drop EXIT  THEN
    insRule @ >r  insRule off  hash-alloc 3 cells - dup
    BEGIN  @ dup  WHILE  2dup swap (reveal  REPEAT
    2drop  r> insRule ! ;

' (initvoc) IS 'initvoc

\ Hash-Find                                            01jan93py

: make-hash
  Root   hashsearch-map context @ cell+ !
  Forth  hashsearch-map context @ cell+ !
  addall          \ Baum aufbauen
;

make-hash  \ Baumsuche ist installiert.

: hash-cold  ( -- ) Defers 'cold
  HashPointer off  0 TO HashTable  HashIndex off
  voclink
  BEGIN  @ dup @  WHILE
         dup cell - @ >r
         dup 'initvoc
         r> over cell - !
  REPEAT  drop ;
' hash-cold ' 'cold >body !

: .words  ( -- )
  base @ >r hex HashTable  Hashlen 0
  DO  cr  i 2 .r ." : " dup i cells +
      BEGIN  @ dup  WHILE
             dup cell+ @ .name  REPEAT  drop
  LOOP  drop r> base ! ;

\ \ this stuff is for evaluating the hash function
\ : square dup * ;

\ : countwl  ( -- sum sumsq )
\     \ gives the number of words in the current wordlist
\     \ and the sum of squares for the sublist lengths
\     0 0
\     hashtable Hashlen cells bounds DO
\        0 i BEGIN
\            @ dup WHILE
\            swap 1+ swap
\        REPEAT
\        drop
\        swap over square +
\        >r + r>
\        1 cells
\    +LOOP ;

\ : chisq ( -- n )
\     \ n should have about the same size as Hashlen
\     countwl Hashlen 2 pick */ swap - ;
