\  NCEX xt cache
\
\  Copyright (C) 1998 Lars Krueger 
\
\  This file is part of FLK.
\
\  FLK is free software; you can redistribute it and/or
\  modify it under the terms of the GNU General Public License
\  as published by the Free Software Foundation, either version 3
\  of the License, or (at your option) any later version.
\
\  This program is distributed in the hope that it will be useful,
\  but WITHOUT ANY WARRANTY; without even the implied warranty of
\  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\  GNU General Public License for more details.
\
\  You should have received a copy of the GNU General Public License
\  along with this program; if not, write to the Free Software
\  Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ The length of the optimizer cache.
10 CONSTANT OPTCACHELEN

\ xt's of unoptimized words
CREATE opt2xt OPTCACHELEN CELLS ALLOT
\ Values of cached literals
CREATE opt2val OPTCACHELEN CELLS ALLOT

\ Ring buffer structure
OPTCACHELEN RING-BUFFER opt2rb

\ Return the offset of the nth element in the ring buffer.
: opt-offs ( ind -- offs )
  opt2rb RB-INDEX CELLS ;

\ Return the stored value.
: opt-getlit 				( ind -- x )
  opt-offs opt2val + @ ;

\ Change the stored value.
: opt-setlit 				( x ind -- )
  opt-offs opt2val + ! ; 

\ Move a cell from foffs to toffs relative to arr.
: (opt-move) 				( toffs foffs arr -- )
  TUCK + @ -ROT + ! ; 

\ Remove one item at from.
: (opt-remove) 				( from -- )
  DUP opt2rb RB-ENTRIES 1- 		\ from from entries
  SWAP - 				\ from cnt
  0 ?DO 				\ from
    DUP opt-offs 			\ from t-offs
    OVER 1+ opt-offs 			\ from t-o f-offs
    2DUP opt2xt (opt-move)
    opt2val (opt-move) 			\ from
    1+
  LOOP DROP 
  opt2rb RB-SHORTEND
  ;

\ Remove the given number of items starting at from.
: opt-remove  				( from cnt -- )
  0 ?DO 
    DUP (opt-remove)
  LOOP DROP ;

\ Handle the real compilation.
: ((opt-flush-1)) 			( oxt -- )
  ?DUP IF ( optimizer ) 		\ oxt 
    EXECUTE
  ELSE ( normal compilation ) 		\ 
    0 opt-offs				\ offset
    opt2xt OVER + @ 			\ offset xt
    ?DUP IF ( word ) 			\ offset xt
      NIP call-gateway,
    ELSE ( constant ) 			\ offs
      opt2val + @ 			\ x
      nc-literal 
    THEN
    opt2rb RB-DELETED
  THEN 
  ;

\ Read the given cache index and find the xt in the optimizer tree.
: (find-cached-xt) ( node ind -- nnode flg )
  opt-offs opt2xt + @ 		\ node xt
  SWAP (opt-flush-find)  	\ nnode flg
  ;

\ Replace the current optimizer xt by the one stored in node.
: (update-oxt) ( oxt node -- noxt node )
  DUP (get-oxt) \ oxt node noxt
  ?DUP IF \ oxt node noxt
    ROT DROP \ node noxt
    SWAP 
  THEN \ oxt node
  ;

\ Flush one item from the cache. This word is the heart of the optimizer.
\ Algo: Traverse the brothers for the current xt. When found, remember the
\ optimizer xt, read the next xt and go to the son. Continue until no further
\ brothers can be found or no more xt's are in the cache. If there is a valid
\ optimizer xt execute it. If no optimizer could be found, compile the first
\ xt and remove it.
\ Due the requirement of using this file in both host and target the execution
\ of the optimizer is hidden in (opt,) 
: (opt-flush-1) 			( -- )
  0 opttree @ 				\ oxt node
  opt2rb RB-ENTRIES 0 ?DO 		\ oxt node
    DUP I (find-cached-xt) 		\ oxt node nnode flg
    IF ( not found ) 			\ oxt node 
      DROP ((opt-flush-1)) UNLOOP EXIT
    THEN  				\ oxt node nnode
    NIP 				\ oxt node
    (update-oxt) 			\ oxt node
    (next-node-down)
  LOOP DROP 				\ oxt
  ((opt-flush-1)) ;

\ Flush the whole cache.
: (opt-flush) 				( -- )
  BEGIN
    opt2rb RB-ENTRIES
  WHILE
    (opt-flush-1)
  REPEAT ;

\ Add an item to the cache.
: (opt-add-item) 			( xt x -- )
  opt2rb RB-STATE@ RB-S-FULL = IF 	\ xt x 
     2>R (opt-flush-1) 2R>
  THEN
  opt2rb RB-HEAD-INDEX CELLS 		\ xt x offs
  TUCK opt2val + ! 			\ xt offs
  opt2xt + ! 
  opt2rb RB-INSERTED ;
 
\ Add a constant to the cache.
: (opt-add-const) 			( x -- )
  0 SWAP (opt-add-item) ;

\ Add an xt to the cache
: (opt-add-xt) 				( xt -- )
  0 (opt-add-item) ;

