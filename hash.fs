\ Hashed dictionaries                                  15jul94py

\ Copyright (C) 1995,1998 Free Software Foundation, Inc.

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

[IFUNDEF] allocate
: reserve-mem here swap allot ;
\ move to a kernel/memory.fs
[ELSE]
: reserve-mem allocate throw ;
[THEN]

[IFUNDEF] hashbits
11 Value hashbits
[THEN]
1 hashbits lshift Value Hashlen

\ compute hash key                                     15jul94py

[IFUNDEF] hash
: hash ( addr len -- key )
    hashbits (hashkey1) ;
[THEN]

Variable insRule        insRule on
Variable revealed

\ Memory handling                                      10oct94py

Variable HashPointer
Variable HashIndex
0 Value HashTable

\ forward declarations
0 Value hashsearch-map
Defer hash-alloc ( addr -- addr )

\ DelFix and NewFix are from bigFORTH                  15jul94py

: DelFix ( addr root -- ) dup @ 2 pick ! ! ;
: NewFix  ( root len # -- addr )
  BEGIN  2 pick @ ?dup  0= WHILE  2dup * reserve-mem
         over 0 ?DO  dup 4 pick DelFix 2 pick +  LOOP  drop
  REPEAT  >r drop r@ @ rot ! r@ swap erase r> ;

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

: inithash ( wid -- )
    wordlist-extend
    insRule @ >r  insRule off  hash-alloc 3 cells -
    dup wordlist-id
    BEGIN  @ dup  WHILE  2dup swap (reveal  REPEAT
    2drop  r> insRule ! ;

: addall  ( -- )
    voclink
    BEGIN  @ dup WHILE
	   dup 0 wordlist-link -
	   dup wordlist-map @ hashsearch-map = 
	   IF  inithash ELSE drop THEN
    REPEAT  drop ;

: clearhash  ( -- )
    HashTable Hashlen cells bounds
    DO  I @
	BEGIN  dup  WHILE
	    dup @ swap HashPointer DelFix
	REPEAT
	I !
	cell +LOOP
    HashIndex off 
    voclink
    BEGIN ( wordlist-link-addr )
	@ dup
    WHILE ( wordlist-link )
	dup 0 wordlist-link - ( wordlist-link wid ) 
	dup wordlist-map @ hashsearch-map = 
	IF ( wordlist-link wid )
	    0 swap wordlist-extend !
	ELSE
	    drop
	THEN
    REPEAT
    drop ;

: rehashall  ( wid -- ) 
  drop revealed @ 
  IF 	clearhash addall revealed off 
  THEN ;

: (rehash)   ( wid -- )
  dup wordlist-extend @ 0=
  IF   inithash
  ELSE rehashall THEN ;

\ >rom ?!
align here    ' hash-find A, ' hash-reveal A, ' (rehash) A, ' (rehash) A,
to hashsearch-map

\ hash allocate and vocabulary initialization          10oct94py

:noname ( addr -- addr )
  HashTable 0= 
  IF  Hashlen cells reserve-mem TO HashTable
      HashTable Hashlen cells erase THEN
  HashIndex @ over !  1 HashIndex +!
  HashIndex @ Hashlen >=
  [ [IFUNDEF] allocate ]
  ABORT" no more space in hashtable"
  [ [ELSE] ]
  IF  HashTable >r clearhash
      1 hashbits 1+ dup  to hashbits  lshift  to hashlen
      r> free >r  0 to HashTable
      addall r> throw
  THEN 
  [ [THEN] ] ; is hash-alloc

\ Hash-Find                                            01jan93py
has? cross 0= 
[IF]
: make-hash
  hashsearch-map forth-wordlist wordlist-map !
  addall ;
  make-hash \ Baumsuche ist installiert.
[ELSE]
  hashsearch-map forth-wordlist wordlist-map !
[THEN]

\ for ec version display that vocabulary goes hashed

: hash-cold  ( -- )
[ has? ec [IF] ] ." Hashing..." [ [THEN] ]
  HashPointer off  0 TO HashTable  HashIndex off
  addall
\  voclink
\  BEGIN  @ dup WHILE
\         dup 0 wordlist-link - initvoc
\  REPEAT  drop 
[ has? ec [IF] ] ." Done" cr [ [THEN] ] ;

' hash-cold INIT8 chained

: .words  ( -- )
  base @ >r hex HashTable  Hashlen 0
  DO  cr  i 2 .r ." : " dup i cells +
      BEGIN  @ dup  WHILE
             dup cell+ @ name>string type space  REPEAT  drop
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
