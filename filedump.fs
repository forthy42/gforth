#! /usr/stud/paysan/bin/forth
\ file hex dump

Create buffer $10 allot

: dumpline ( addr handle -- flag )
  buffer $10 2dup 0 fill rot read-file throw
  $10 <> swap 6 u.r ." : "  buffer .line cr ;

: init  cr $10 base ! ;

: filedump  ( addr count -- )  init r/o bin open-file throw >r
  0  BEGIN  $10 bounds  r@ dumpline  UNTIL  drop
  r> close-file throw ;

script? [IF]
   : alldump argc @ 2 ?DO I arg 2dup type ." :" filedump LOOP ;
   alldump bye
[THEN]
