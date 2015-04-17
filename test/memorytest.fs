\ To test the ANS Forth Memory-Allocation word set

\ This program was written by Gerry Jackson in 2006, with contributions from
\ others where indicated, and is in the public domain - it can be distributed
\ and/or modified in any way but please retain this notice.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

\ The tests are not claimed to be comprehensive or correct 

\ ------------------------------------------------------------------------------
\ Version 0.11 7 April 2015 Now checks memory region is unchanged following a
\              RESIZE. @ and ! in allocated memory.
\         0.8 10 January 2013, Added CHARS and CHAR+ where necessary to correct
\             the assumption that 1 CHARS = 1
\         0.7 1 April 2012  Tests placed in the public domain.
\         0.6 30 January 2011 CHECKMEM modified to work with ttester.fs
\         0.5 30 November 2009 <false> replaced with FALSE
\         0.4 9 March 2009 Aligned test improved and data space pointer tested
\         0.3 6 March 2009 { and } replaced with T{ and }T
\         0.2 20 April 2007  ANS Forth words changed to upper case
\         0.1 October 2006 First version released

\ ------------------------------------------------------------------------------
\ The tests are based on John Hayes test program for the core word set
\ and requires those files to have been loaded

\ Words tested in this file are:
\     ALLOCATE FREE RESIZE
\     
\ ------------------------------------------------------------------------------
\ Assumptions and dependencies:
\     - that 'addr -1 ALLOCATE' and 'addr -1 RESIZE' will return an error
\     - tester.fr or ttester.fs has been loaded prior to this file
\     - testing FREE failing is not done as it is likely to crash the
\       system
\ ------------------------------------------------------------------------------

TESTING Memory-Allocation word set

DECIMAL

\ ------------------------------------------------------------------------------
TESTING ALLOCATE FREE RESIZE

VARIABLE addr1
VARIABLE datsp

HERE datsp !
T{ 100 ALLOCATE SWAP addr1 ! -> 0 }T
T{ addr1 @ ALIGNED -> addr1 @ }T   \ Test address is aligned
T{ HERE -> datsp @ }T            \ Check data space pointer is unchanged
T{ addr1 @ FREE -> 0 }T

T{ 99 ALLOCATE SWAP addr1 ! -> 0 }T
T{ addr1 @ ALIGNED -> addr1 @ }T
T{ addr1 @ FREE -> 0 }T

T{ 50 CHARS ALLOCATE SWAP addr1 ! -> 0 }T

: writemem 0 DO I 1+ OVER C! CHAR+ LOOP DROP ;	( ad n -- )

\ checkmem is defined this way to maintain compatibility with both
\ tester.fr and ttester.fs which differ in their definitions of T{

: checkmem  ( ad n --- )
   0
   DO
      >R
      T{ R@ C@ -> R> I 1+ SWAP >R }T
      R> CHAR+
   LOOP
   DROP
;

addr1 @ 50 writemem addr1 @ 50 checkmem

T{ addr1 @ 28 CHARS RESIZE SWAP addr1 ! -> 0 }T
addr1 @ 28 checkmem

T{ addr1 @ 200 CHARS RESIZE SWAP addr1 ! -> 0 }T
addr1 @ 28 checkmem

\ ------------------------------------------------------------------------------
TESTING failure of RESIZE and ALLOCATE (unlikely to be enough memory)

\ This test relies on the previous test having passed

VARIABLE resize-ok
T{ addr1 @ -1 RESIZE 0= DUP resize-ok ! -> addr1 @ FALSE }T

\ Check unRESIZEd allocation is unchanged following RESIZE failure 
: mem?  resize-ok @ 0= if addr1 @ 28 checkmem then ;   \ Avoid using [IF]
mem?

T{ addr1 @ FREE -> 0 }T   \ Tidy up

T{ -1 ALLOCATE SWAP DROP 0= -> FALSE }T      \ Memory allocate failed

\ ------------------------------------------------------------------------------
TESTING @  and ! work in ALLOCATEd memory (provided by Peter Knaggs)

: write-cell-mem ( addr n -- )
  1+ 1 DO I OVER ! CELL+ LOOP DROP
;

: check-cell-mem ( addr n -- )
  1+ 1 DO
    I SWAP >R >R
    T{ R> ( I ) -> R@ ( addr ) @ }T
    R> CELL+
  LOOP DROP
;

\ Cell based access to the heap

T{ 50 CELLS ALLOCATE SWAP addr1 ! -> 0 }T 
addr1 @ 50 write-cell-mem
addr1 @ 50 check-cell-mem

\ ------------------------------------------------------------------------------

MEMORY-ERRORS SET-ERROR-COUNT

CR .( End of Memory-Allocation word tests) CR
