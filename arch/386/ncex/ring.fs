\ Ring buffer handling

\ The following words provide tools to handle a finite ring buffer acting as
\ both a fifo and an array. The user of these words has to create the actual
\ data space for the buffer. 
\
\ Data is assumed to be added at the head pointer and retrieved at the tail
\ pointer. The pointer contain the index where next fetch/store has to be
\ done.
\
\ To use this file you need to define the following constants to contain the
\ exception numbers. So you can use e.g. the proposed exception word (in FLK
\ and gforth included).

\ Constant 		Meaning
\ RB-E-OVERFLOW 	Tried to advance head pointer in a full buffer.
\ RB-E-UNDERFLOW 	Tried to remove data from an empty buffer.
\ RB-E-RANGE 		Tried to access an index outside the given limit.
S" Ring buffer overflow!" exception constant RB-E-OVERFLOW
S" Ring buffer underflow!" exception constant RB-E-UNDERFLOW
S" Ring buffer index out of range!" exception constant RB-E-RANGE

\ Structure of the ring buffer record
\ Offset 	Meaning
\ 0 		Length
\ 1 Cell 	Head index
\ 2 cells 	tail index
\ 3 cells 	state ( RB-S-...)

\ States of a ring buffer. It is full, empty or none of both.
0 CONSTANT RB-S-NORMAL
1 CONSTANT RB-S-FULL
2 CONSTANT RB-S-EMPTY

\ \ Create a structure to store the working data of the ring buffer.
: RING-BUFFER 				( length -<name>- )
( OK )
 CREATE , 0 , 0 , RB-S-EMPTY , ;

\ Return the length of the buffer.
: RB-LENGTH 				( rb -- length )
( OK )
  @ ;

\ Return the state of the buffer.
: RB-STATE@ 				( rb -- state )
( OK )
  3 CELLS + @ ;

\ Set the state of the buffer.
: RB-STATE! 				( state rb -- )
( OK )
  3 CELLS + ! ;

\ Factor to fetch, modify and store a pointer. 
: (rb-advance) 				( rb p-addr -- rb p)
( OK )
  DUP @ 1+ 				\ rb p-addr p
  ROT DUP @ 				\ p-addr p rb len
  ROT TUCK 				\ p-a rb p len p
  > INVERT IF 				\ p-a rb p
    DROP 0 				
  THEN 					\ p-addr rb p
  ROT OVER SWAP ! ; 			\ rb p
  
\ Advance the head pointer (Data has been already added.).
: RB-INSERTED 				( rb -- )
( OK )
  DUP RB-STATE@ RB-S-FULL = 		\ rb full?
  IF RB-E-OVERFLOW THROW THEN 		\ rb
  DUP CELL+ 				\ rb hp-addr
  (rb-advance) 				\ rb hp
  OVER 2 CELLS + @ 			\ rb hp tp
  = IF RB-S-FULL ELSE RB-S-NORMAL THEN 	\ rb state
  SWAP RB-STATE! ;

\ Advance the tail pointer (Data has been already removed.).
: RB-DELETED 				( rb -- )
( OK )
  DUP RB-STATE@ RB-S-EMPTY = 		\ rb empty?
  IF RB-E-UNDERFLOW THROW THEN 		\ rb
  DUP 2 CELLS + (rb-advance) 		\ rb tp
  OVER CELL+ @ 				\ rb tp hp
  = IF RB-S-EMPTY ELSE RB-S-NORMAL THEN \ rb state
  SWAP RB-STATE! ;

\ Return the index relative to the tail pointer. Therefore index 0 is the
\ element that was added first, index 1 the one added next etc.
: RB-INDEX 				( ind rb -- absind )
( OK )
  2DUP @ < INVERT 
  IF RB-E-RANGE THROW THEN 		\ ind rb
  DUP @ SWAP 2 CELLS + @ 		\ ind len tp
  ROT + 				\ len absind
  2DUP > IF 				\ len absind
    ( correct index )
    NIP 				\ absind
  ELSE 					\ len absind
    ( outside )
    SWAP - 				\ absind
  THEN ; 

\ Set the ring buffer to its initial state.
: RB-RESET 				( rb -- )
( OK )
  CELL+ 0 OVER ! 			\ hp-a
  CELL+ 0 OVER ! 			\ tp-a
  CELL+ RB-S-EMPTY SWAP ! ;

\ Return the number of entries.
: RB-ENTRIES 				( rb -- entries )
( OK )
  DUP RB-STATE@ RB-S-EMPTY = IF
    DROP 0 EXIT
  THEN
  DUP CELL+ DUP @ SWAP CELL+ @ 		\ rb hp tp
  2DUP > IF ( no wrap around ) 		\ rb hp tp
    ROT DROP 
  ELSE ( wrap around ) 			\ rb hp tp
    ROT @ 				\ hp tp length
    ROT + SWAP 				\ hp+l tp
  THEN 
  - ;

\ Decrease the head-pointer.
: RB-SHORTEND 				( rb -- )
( OK )
  DUP RB-STATE@ RB-S-EMPTY =
  IF RB-E-UNDERFLOW THROW THEN 		\ rb
  DUP CELL+ @ 				\ rb hp
  DUP 0= IF 				\ rb hp
    DROP DUP @  			\ rb hp
  THEN 1- 				\ rb hp-1
  2DUP SWAP CELL+ ! 			\ rb nhp
  OVER 2 CELLS + @ 			\ rb nhp tp
  = IF RB-S-EMPTY ELSE RB-S-NORMAL THEN \ rb state
  SWAP RB-STATE!  ;

\ Return the index of the head pointer.
: RB-HEAD-INDEX 			( rb -- ind )
  CELL+ @ ;

\ Return the index of the tail pointer.
: RB-TAIL-INDEX 			( rb -- ind )
  2 CELLS + @ ;

