\ NCEX Optimizer tree construction.
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

S" Word is not a primitive." exception constant exnoprim

VARIABLE opttree
0 opttree !

\ Run down the chain stored in node-var and return the node containing xt and
\ true or false if not found.
: (opt-find-brother) 		 	( xt node-var -- node true / false )
  BEGIN 
    @ DUP 				\ xt node do?
  WHILE 				\ xt node
    2DUP CELL+ @ = 			\ xt node found?
    IF 					\ xt node
      NIP TRUE EXIT
    THEN 				\ xt node
  REPEAT 2DROP FALSE ; 

\ Find in node and its brothers the one that contains xt and return it and/or
\ a failure flag.
: (opt-flush-find) 			( xt node -- nnode false / true )
  BEGIN 				\ xt node
    DUP
  WHILE 				\ xt node
    DUP CELL+ @ 			\ xt node node-xt
    PLUCK = IF 				\ xt node 
      NIP FALSE EXIT
    THEN
    @ 					\ xt next-node
  REPEAT 2DROP TRUE ;

\ Make a node.
: (opt-make-node) 			( lastnode xt -- node )
  HERE 					\ ln xt' node
  ROT DUP 				\ xt' node ln ln 
  @ ,  					\ xt' node ln
  OVER SWAP ! 				\ xt' node 
  SWAP , 0 , 0 , 			\ node
; 

\ A pretty 0.
: opt( 				( -- 0 )
  0 ;

\ Placeholder for literal. Use only within opt( )opt:.
: ''# 				( ... n -- .. 0 n+1 )
  0 SWAP 1+ ;

\ Find the word in the dictionary and append the target xt to the list.
: '' 				( ... n -<name>- ... xt n+1 )
  ' dup xtprim? 0= exnoprim ?throw \ xt
  SWAP 1+
  ;

\ A pretty :NONAME.
: )opt: 			( -- xt colon-sys/ )
  :NONAME 
  ;

\ Structure of a node:
\ Offset  	Meaning
\ 0 		next brother 
\ 1 cell 	xt to optimizer away
\ 2 cells 	xt of optimizer
\ 3 cells 	next node (downward)

\ Get the next node downwards.
: (next-node-down) 3 CELLS + @ ; 

\ Get the optimizer xt.
: (get-oxt) 2 CELLS + @ ;

: (.opttree) 				( ind node-var -- )
  BEGIN 				\ ind v
    @ DUP 				\ ind node cont?
  WHILE 				\ ind node
    OVER SPACES
    DUP CELL+ @ 			\ ind node xt
    DUP 0= IF  				\ ind node xt
      ." -- number -- " 
      DROP
    ELSE 				\ ind node xt
      DUP 
      >name name>string TYPE
      SPACE . 
    THEN  				\ ind node
    DUP 2 CELLS + @ 			\ ind node opt?
    IF ."  ***" THEN CR
    2DUP 3 CELLS + 			\ ind node ind son-var
    SWAP 2 + SWAP RECURSE 		\ ind node
  REPEAT 2DROP
;

: .opttree  				( -- )
  CR 
  ." ##################### tree of optimizers ########################" CR
  0 opttree (.opttree)
  ." #################################################################" CR
;

\ Do the actual work of adding the optimizer to the tree. 
: ;opt 					( ... n xt colon-sys/ -- )
  POSTPONE ; SWAP 			\ ... xt n
  DUP 0= ABORT" no words to optimize."
  opttree 				\ ... xt n lastnode
  BEGIN 				\ ... xt n lastnode
    OVER 2 +  				\ ... xt n lastnode n+2 
    ROLL 				\ ... xt n lastnode xt'
    2DUP SWAP 				\ ... xt n ln xt' xt' ln
    (opt-find-brother) 			\ ... xt n ln xt' ((node true) /false)
    IF 					\ ... xt n lastnode xt' node
      NIP NIP 				\ ... xt n node
    ELSE 				\ ... xt n ln xt'
      (opt-make-node) 			\ ... xt n node
    THEN 				\ ... xt n node
    3 CELLS + 				\ ... xt n lastnode
    SWAP 1- SWAP OVER 0= 		\ ... xt n-1 lastnode fini?
  UNTIL 				\ ... xt 0 lastnode
  NIP 1 CELLS - 			\ xt opt-addr
  DUP @ 				\ xt opt-addr old-xt
  ABORT" Trying to define two optimizers for the same sequence."
  !
; IMMEDIATE

