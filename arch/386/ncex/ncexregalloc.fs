\ ==============================================================================
\ =============================== meta assembler ===============================
\ ==============================================================================

\ The register allocator uses the registers eax, ebx, ecx, edx, esi and esi to
\ cache the upper 6 items on the data stack. It is very time expensive to
\ restore the correct possitions of all 6 registers especially if we assume
\ that an average of 3 registers is cached. But caching only one register by
\ default is never a bad idea.

\ At the begin of each word, all registers are available.
\ Using 32-bit-flat-memory-mode, only near calls are used. EBP is used as the
\ data stack pointer. Therefore an offset is always nessesary. This offset
\ is accumulated when moving data to and from the stack. It is assumed to be
\ zero at the begin and end of each word, therefore EBP points to TOS. 
\ In conjunction with the (future) use of inlineable words, this saves one
\ ADD/SUB EBP, 4 in every word using the stack, but introduces on ADD EBP,x in
\ the return. This can be paired with the last store of the register, that
\ have to be flushed to stack or the move to make EAX TOS or the RET itself.

\ Using the registers as virtual TOS+x "stackrobatics" with less than seven
\ registers do not cost any cycle, if no register has to be loaded before or
\ flushed after. Only in case the op. request a special register in a special
\ place, some mov, or xchg, is produced.
 
\ An example:
\ : test 		( a b c -- e )
\ (1) ROT 		\ b c a 		2 cycles for loading ebx, ecx
\ (2) ROT 		\ c a b 		no cycle, just changing the
\ 						the order while compiling
\ (3) + 		\ c a+b 		one cycle for the add
\ (4) SWAP 		\ a+b c 		no cycle
\ (5) 2* 2* 		\ a+b 4*c 		2 cycles for the shifts
\ (6) + 		\ a+b+4*c 		1 cycle for the add
\ (7) ; 					2 cycles for flush + bp ofs
\
\ In line (1) EBX and ECX are loaded, because ROT asks for 3 used registers.
\ In line (2) only the tables in the compiler are changed, but no code is
\ produced.
\ Lines (3) and (5) perform their calculations by producing one ADD and 
\ two SHL's.
\ Line (4) produces no code, just the tables are changed.
\ In Line (7) the consistent state must be accomplished, therefore
\ one register has to be flushed (and EAX may be loaded from an other register, I
\ haven't tracked this) and EBP has to be increased by 8.

\ The registers need numbers to identify them.
    0 		CONSTANT VREG-EAX  		\ virtual register numbers for 
    1 		CONSTANT VREG-ECX   		\   register allocator
    2 		CONSTANT VREG-EDX
    3 		CONSTANT VREG-EBX
    4 		CONSTANT VREG-ESI
    5 		CONSTANT VREG-EDI
    6 	 	CONSTANT #USEREGS
-1 	CONSTANT REG-NONE

\ Translate vreg to reg using a look-up table.
CREATE ((vreg>reg)) 0 , 1 , 2 , 3 , 6 , 7 ,
: (vreg>reg) 			( vreg -- reg )
  DUP REG-NONE = 
  IF ." Called (vreg>reg) with invalid register." ABORT THEN
  CELLS ((vreg>reg)) + @ ;

\ This array contains the state of the register allocator. The cell i contains
\ the number of the register caching TOS+i.
CREATE tos-cache #USEREGS CHARS ALLOT
: cache! 				( reg ind -- )
  CHARS tos-cache + C! ;
: cache@ 				( ind -- reg )
  CHARS tos-cache + C@ ;

\ This array contains the numbers of registers that were marked free.
CREATE free-cache #USEREGS CHARS ALLOT
: free! 					( reg ind -- )
  CHARS free-cache + C! ;
: free@ 					( ind -- reg )
  CHARS free-cache + C@ ;

\ Number of items in tos-cache
0 VALUE #tos-cache

\ Increase #tos-cache
: (#tc++) #tos-cache 1+ TO #tos-cache ;

\ Number of items in free-cache
0 VALUE #free-req

\ Who are you?
: vreg>name 				( vreg -- addr len )
  CASE 
    VREG-EAX OF S" eax" ENDOF
    VREG-EBX OF S" ebx" ENDOF
    VREG-ECX OF S" ecx" ENDOF
    VREG-EDX OF S" edx" ENDOF
    VREG-ESI OF S" esi" ENDOF
    VREG-EDI OF S" edi" ENDOF
    >R S" unknown" R>
  ENDCASE ;

\ Mark a register as requested with free.
: (mark-free) 				( vreg -- )
  #free-req free! #free-req 1+ TO #free-req ;

\ Check if vreg is marked free.
: (#marked) 				( vreg -- flag )
  #free-req 0 ?DO 			\ vreg
    I free@ OVER = IF
      DROP TRUE UNLOOP EXIT
    THEN
  LOOP 
  DROP FALSE ;

\ Number of consequtively requested registers in compiler
0 VALUE #reg-req

\ Increase #reg-req .
: (#rr++) #reg-req 1+ TO #reg-req ;

\ Accumulated offset to ebp
0 VALUE offs-ebp

\ Maintain a offs-ebp to be within +/- 124 to fit into a byte.
: (add-ebp) 			( n -- )
  offs-ebp + 			\ no
  DUP ABS 124 >= IF 		\ no
    [ebp] ebp lea,
    0
  THEN
  TO offs-ebp ;

\ Save the state of the allocator on the stack. No code is produced.
: save-allocator 			( -- allocator )
  #USEREGS 0 DO I cache@ LOOP
  #USEREGS 0 DO I free@ LOOP
  #tos-cache #free-req offs-ebp ;

\ Restore the state of the allocator from the stack. No code is produced.
: restore-allocator 			( allocator -- )
  TO offs-ebp
  TO #free-req TO #tos-cache
  #USEREGS 0 DO #USEREGS 1- I - free! LOOP
  #USEREGS 0 DO #USEREGS 1- I - cache! LOOP ;

\ Print the state of the allocator.
: .regalloc 				( -- )
  ." Used: "
  #tos-cache 0= IF
    ." none"
  ELSE
    #tos-cache 0 DO
      #tos-cache 1- I - cache@ vreg>name TYPE ."  "
    LOOP
  THEN
  ."   Free: "
  #free-req 0= IF
    ." none" 
  ELSE
    #free-req 0 DO
      #free-req 1- I - free@ vreg>name TYPE ."  "
    LOOP
  THEN ."  offs: " offs-ebp . CR ;

\ Check whether vreg is used
: (#used) 				( vreg -- flag )
  #tos-cache 0 ?DO
     I cache@ OVER = IF
        DROP UNLOOP TRUE EXIT
     THEN
  LOOP
  DROP  FALSE ;

\ find the vreg in current requested cache slot
: tc(#rr) 				( -- vreg )
  #reg-req cache@ ;
  
\ check whether enough registers are in use
: (#enough) 				( -- flag )
  #reg-req #tos-cache < ; 

\ load the register vreg into current requested slot
: (#load) 				( vreg -- )
  BOFFS offs-ebp #tos-cache CELLS + [ebp] 
  DUP (vreg>reg) SZ-32 (#reg) mov,
  #tos-cache cache! 
  #tos-cache 1+ TO #tos-cache 
  ;

\ find the cache-slot vreg is in. Return -1 for non-cached register
: (#find) 				( vreg -- nr )
  #tos-cache 0 ?DO
    I cache@ OVER = IF
      DROP I UNLOOP EXIT
    THEN
  LOOP
  DROP -1 ;

\ exchange the meanings and the contents of the regs
\ free(vreg1) used(vreg2)  -> mov vreg1, vreg2
\ used(vreg1) free(vreg2)  -> mov vreg2, vreg1
\ else                     -> xchg vreg1, vreg2
: tc-xchg 				( vreg1 vreg2 -- )
  2DUP = IF 2DROP EXIT THEN
  DUP (#used) INVERT IF 		( vreg2 free ) \ vreg1 vreg2
    2DUP SWAP 
    (vreg>reg) SZ-32 (#reg) 
    (vreg>reg) SZ-32 (#reg) mov, 	\ vreg1 vreg2
    SWAP (#find) 			\ vreg2 tos1
    cache!
  ELSE
    OVER (#used) INVERT IF 		( vreg1 free ) \ vreg1 vreg2
      2DUP 
      (vreg>reg) SZ-32 (#reg) 
      (vreg>reg) SZ-32 (#reg) mov, 	\ vreg1 vreg2
      (#find) 				\ vreg1 tos2
      cache!
    ELSE
      2DUP
      (vreg>reg) SZ-32 (#reg) 
      (vreg>reg) SZ-32 (#reg) xchg, 	\ vreg1 vreg2
      2DUP (#find) SWAP (#find) 	\ vreg1 vreg2 tos2 tos1
      ROT SWAP 				\ vreg1 tos2 vreg2 tos1
      cache! cache!
    THEN
  THEN ;

\ Set the bit vreg in mask, if vreg is cached.
\ This word has an environmental dependency. It assumes, that one cell
\ has more than #USEREGS bits.
: (cached-mask) 				( -- mask )  
  0 #tos-cache 0 ?DO
    1 I cache@ LSHIFT
    OR
  LOOP ;
  
\ Find the first unrequested register in cache or REG-NONE if all are used.
: (unrequested) 				( -- vreg )
  (cached-mask)
  #free-req 0 ?DO
    1 I free@ LSHIFT OR
  LOOP
  #USEREGS 0 DO
    DUP 1 I LSHIFT AND 0= IF
      DROP I UNLOOP EXIT
    THEN
  LOOP
  DROP REG-NONE 
 ;

\ flush the lowest cache slot to memory and return the free register
: (flushreg) 				( -- vreg )
  #tos-cache 1- cache@ DUP 		\ vreg vreg
  ( mov [ebp + offs + {#tc-1}*4], vreg )
  (vreg>reg) SZ-32 (#reg) 	
  BOFFS #tos-cache 1- CELLS offs-ebp + [ebp] mov,
  #tos-cache 1- TO #tos-cache ;
  
\ request a virtual register by number
\ n is tos + #reg-req
\ condition 				action
\ --------------------------------------------------------
\ free(vreg) & #rr<#tc 			xchg(vreg,tc(#rr))
\ free(vreg) & #rr=#tc 			load(vreg)
\ used(vreg) & #rr<#tc 			xchg(vreg,tc(#rr))
\ used(vreg) & #rr=#tc & free(vreg2) 	load(vreg2), xchg(vreg2,vreg)
\ used(vreg) & #rr=#tc & used(vreg2) 	flush(#ur-1), xchg( #ur-1, vreg)
: (#req) 				( vreg -- )
( OK )
  DUP (#used) IF 			\ vreg
    (#enough) IF
      tc(#rr) tc-xchg
    ELSE ( req. reg in use ) 		\ vreg
      (unrequested) 			\ vreg vreg2
      DUP REG-NONE = IF ( none free ) 		
        DROP (flushreg) 		\ vreg vreg2
      ELSE ( 1 free found )
        DUP (#load)
      THEN
      tc-xchg
    THEN
  ELSE 					\ vreg
    (#enough) IF
      tc(#rr) tc-xchg
    ELSE
      (unrequested) 			\ vreg vreg2
      DUP REG-NONE <> IF
        DUP (#load)
	tc-xchg
      ELSE
        DROP
	(flushreg) tc-xchg
      THEN
    THEN
  THEN (#rr++) ;

: req-eax VREG-EAX (#req) ;
: req-ebx VREG-EBX (#req) ;
: req-ecx VREG-ECX (#req) ;
: req-edx VREG-EDX (#req) ;
: req-edi VREG-EDI (#req) ;
: req-esi VREG-ESI (#req) ;

\ Is register eax, ebx, ecx or edx?
: (is-a-d) 				( vreg -- flag )
    VREG-EAX VREG-ESI within ;
  
\ Request the register if unused.
: (#req-unused) 			( vreg -- ok? )
  DUP (#used) INVERT DUP 		\ vreg ok? ok?
  IF SWAP (#req) ELSE NIP THEN ;

\ Request any one of eax-edx. Check if one of them is not used. If so request
\ it else check all other requested below if they are eax-edx. If so request
\ it. Else error.
: req-a-d 					( -- )
  (#enough) IF
    #reg-req cache@ (is-a-d) IF
     (#rr++) EXIT
    THEN
  THEN
  VREG-EAX (#req-unused) IF EXIT THEN
  VREG-ECX (#req-unused) IF EXIT THEN
  VREG-EDX (#req-unused) IF EXIT THEN
  VREG-EBX (#req-unused) IF EXIT THEN
  #tos-cache #reg-req ?DO
    I cache@ DUP (is-a-d) 			\ vreg general?
    IF (#req) UNLOOP EXIT ELSE DROP THEN
  LOOP
  ." Can't request that many general registers." ABORT ;

\ request any virtual register
: req-any 				( -- )
  (#enough) INVERT IF
    (unrequested) 				\ vreg
    (#load)
  THEN (#rr++) 
  ;

\ request any BUT the register vreg
\ n is tos + #reg-req
\ condition 				action
\ --------------------------------------------------------
\ enough, n<>vreg 			---
\ enough, n=vreg, free(vreg2) 		xchg(vreg,vreg2)
\ enough, n=vreg, none free, #rr<#ur-1  xchg(vreg,#ur-1)
\ enough, n=vreg, none free, #rr=#ur-1 	xchg(vreg,0)
\ not enough, used(vreg) 		load(vreg2)
\ not enough, free(vreg), free(vreg2) 	load(vreg2)
\ not enough, free(vreg), used(vreg2) 	xchg(vreg,vreg2), load(vreg2)
: (xchg-not) 				( vreg -- )
  (unrequested) 				\ vreg vreg2
  DUP REG-NONE <> IF
    tc-xchg
  ELSE
    #reg-req #USEREGS 1- = IF
      0 
    ELSE
      #USEREGS 1- 
    THEN
    cache@ tc-xchg
  THEN ;
  
: (#req-not) 				( vreg -- )
  (#enough) IF
    tc(#rr) 				\ vreg n
    OVER = IF 				\ vreg
      (xchg-not)
    ELSE
      DROP
    THEN
  ELSE 					\ vreg
    DUP (#used) IF 			\ vreg
      DUP
      (unrequested) DUP REG-NONE = 
      TooManyRegs 			\ vreg2
      (#load)
    ELSE 				\ vreg
      (unrequested) DUP REG-NONE <>
      IF 				\ vreg vreg2
        (#load) DROP
      ELSE
        TUCK 				\ vreg2 vreg vreg2
	tc-xchg (#load)
      THEN
    THEN
  THEN (#rr++) ;

: req-not-eax VREG-EAX (#req-not) ;

CREATE a-d-table VREG-EAX , VREG-EBX , VREG-ECX , VREG-EDX ,
: forall-a-d 4 0 ;

\ find the first unmarked register eax, ebx, ecx or edx
\ return nr or -1 if all marked
: (unmarked-a-d) 			( -- vreg )
  a-d-table
  forall-a-d DO 			\ addr
    DUP @ DUP (#marked) INVERT 		\ addr vreg mark
    IF
      NIP UNLOOP EXIT
    THEN
    DROP CELL+
  LOOP DROP REG-NONE ;

\ all eax-edx marked
: (a-d-marked) 				( -- flag )
  (unmarked-a-d) REG-NONE = ;

\ request a free register, but only eax, ebx, ecx or edx
\ cond 					action
\ ----------------------------------------------
\ a-d marked 				error
\ a-d req, s-d req 			flush
\ a-d unreq. 				mark
\ a-d req, s-d unreq 			swap, mark 			
: a-d-free 				( -- )
( OK )
  (a-d-marked) 
  IF ." Can't request this many general registers." ABORT THEN
  (unrequested) 
  DUP REG-NONE = IF 			\ vreg
    DROP (flushreg) 			\ vreg
  THEN
  DUP (is-a-d) IF 			\ vreg
    (mark-free)
  ELSE 					\ vreg-s-d 
    (unmarked-a-d) 			\ vreg-s-d vreg-a-d
    TUCK tc-xchg (mark-free)
  THEN ;

\ request a free register
\ cond 					action
\ ----------------------------------------------
\ all marked 				error
\ all requested 				error
\ all cached 				flush, mark
\ uncached(vreg) 			mark
: req-free 				( -- ) ( OK )
  #free-req #reg-req +
  #USEREGS = IF ." All registers requested." ABORT THEN
  (unrequested) 				\ vreg
  DUP REG-NONE = IF
     DROP (flushreg) 			\ vreg
  THEN
  (mark-free) 
  ;

\ request a free register by number vreg 		TODO more efficient
\ cond 						action
\ ----------------------------------------------------
\ marked(vreg) 					error
\ unused(vreg) 					mark
\ unmarked(vreg), cached(vreg), unused(v2) 	swap, mark
\ unmarked(vreg), all cached, vreg=#ur-1 	flush, mark
\ unmarked(vreg), all cached, vreg<>#ur-1 	flush, swap, mark
: (#req-free) 				( vreg -- )
  DUP (#marked) IF ." Register is already marked." ABORT THEN
  DUP (#used) INVERT IF
    (mark-free)
  ELSE 					\ vreg
    (unrequested) 			\ vreg vreg2
    2DUP = IF (internal-error) THEN 	\ vreg vreg2
    DUP REG-NONE <> IF 			\ vreg vreg2
      SWAP TUCK tc-xchg 		\ vreg
      (mark-free)
    ELSE 				\ vreg vreg2
      DROP
      DUP (#find) 			\ vreg nr
      DUP -1 = IF (internal-error) THEN
      #USEREGS 1- OVER = IF
        (flushreg) 			\ vreg vreg
	2DUP <> IF (internal-error) THEN
	DROP (mark-free)
      ELSE
	(flushreg) 			\ vreg vreg2
	SWAP TUCK tc-xchg (mark-free)
      THEN
    THEN
  THEN ;

: free-eax VREG-EAX (#req-free) ;
: free-ecx VREG-ECX (#req-free) ;
: free-edx VREG-EDX (#req-free) ;
: free-edi VREG-EDI (#req-free) ;
: free-esi VREG-ESI (#req-free) ;

\ swap the vregs in tos+n1 and tos+n2
: tos-swap 				( n1 n2 -- )
  2DUP #reg-req < SWAP #reg-req < AND
  INVERT IF ." Too few registers requested." ABORT THEN
  2DUP 					\ n1 n2 n1 n2
  cache@ SWAP cache@ 			\ n1 n2 r2 r1
  ROT 					\ n1 r2 r1 n2
  cache! SWAP cache! ;

\ drop the tos+0
: (reg-free) 				( -- )
  #tos-cache DUP 0= IF ." No register in cache." ABORT THEN
  1- DUP TO #tos-cache
  0 
  BEGIN 					\ end curr
    DUP 1+ cache@ 			\ end curr vreg
    OVER cache! 				\ end curr
    1+ 2DUP <=
  UNTIL 2DROP 
  4 (add-ebp) ;

\ free n times the tos+0
: reg-free 				( n -- )
  0 ?DO
    (reg-free)
  LOOP ;

\ put the register in free-cache+n on top of stack
: free>tos 				( n  -- )
  DUP #free-req >= IF ." Too few free registers requested." ABORT THEN
  free@ 					\ vreg
  ( make space for register )
  #tos-cache BEGIN 			\ vreg i
    1- DUP 0< INVERT
  WHILE 					\ vreg i-1
    DUP cache@ OVER 1+ cache! 		
  REPEAT DROP
  0 cache!
  -4 (add-ebp)
  (#tc++) ;

\ reset register allocator ( at start of compiler word )
: regalloc-reset 			( -- )
  0 TO #reg-req 0 TO #free-req reset-labels ;

\ initialize register allocator ( at start of word compilation)
: regalloc-init 				( -- )
  0 TO #tos-cache
  0 TO offs-ebp ;

\ flush all registers to stack and correct ebp
: regalloc-flush 			( -- )
  BEGIN
    #tos-cache 0<> 
  WHILE
    (flushreg) DROP
  REPEAT
  offs-ebp IF
    BOFFS offs-ebp [ebp] ebp lea,
  THEN 0 TO offs-ebp ;

\ flush all except 2 registers to stack and correct ebp
: regalloc-flush-do 			( -- )
  BEGIN
    #tos-cache 2 > 
  WHILE
    (flushreg) DROP
  REPEAT
  offs-ebp -8 <> IF
    BOFFS offs-ebp 8 + [ebp] ebp lea,
  THEN
  -8 TO offs-ebp 
  ;

\ access to meta-register
: (tosn) 				( n -- )
  DUP #tos-cache >= IF ." Request more registers." ABORT THEN
  cache@ (vreg>reg) SZ-32 (#reg) ;

: tos0 0 (tosn) ;
: tos1 1 (tosn) ;
: tos2 2 (tosn) ;
: tos3 3 (tosn) ;
: tos4 4 (tosn) ;
: tos5 5 (tosn) ;

: ([tosn]) 				( offs n -- )
  DUP #tos-cache >= IF ." Request more registers." ABORT THEN
  cache@ (vreg>reg) (#[reg]) ;

: [tos0] 0 ([tosn]) ;
: [tos1] 1 ([tosn]) ;
: [tos2] 2 ([tosn]) ;
: [tos3] 3 ([tosn]) ;
: [tos4] 4 ([tosn]) ;
: [tos5] 5 ([tosn]) ;
 
: (freen) 				( n -- )
  DUP #free-req >= IF ." Request more free registers." ABORT THEN
  free@ (vreg>reg) SZ-32 (#reg) ;

: free0 0 (freen) ;
: free1 1 (freen) ;
: free2 2 (freen) ;
: free3 3 (freen) ;
: free4 4 (freen) ;
: free5 5 (freen) ;

: ([freen]) 				( offs n -- )
  DUP #free-req >= IF ." Request more free registers." ABORT THEN
  free@ (vreg>reg) (#[reg]) ;
  
: [free0] 0 ([freen]) ;
: [free1] 1 ([freen]) ;
: [free2] 2 ([freen]) ;
: [free3] 3 ([freen]) ;
: [free4] 4 ([freen]) ;
: [free5] 5 ([freen]) ;

: (vreg>reg_l) 				( vreg -- )
  DUP (is-a-d) INVERT IF ." Can't get lower part of edi or esi." ABORT THEN
  CASE
    VREG-EAX OF reg-al ENDOF
    VREG-EDX OF reg-dl ENDOF
    VREG-ECX OF reg-cl ENDOF
    VREG-EBX OF reg-bl ENDOF
  ENDCASE ;

: (vreg>reg_h) 				( vreg -- )
  DUP (is-a-d) INVERT IF ." Can't get higher part of edi or esi." ABORT THEN
  CASE
    VREG-EAX OF reg-ah ENDOF
    VREG-EDX OF reg-dh ENDOF
    VREG-ECX OF reg-ch ENDOF
    VREG-EBX OF reg-bh ENDOF
  ENDCASE ;

\ n is the nr of the 32 bit virtual register
: (free_l) 				( n -- )
  DUP #free-req >= IF ." Request more free registers." ABORT THEN
  free@ (vreg>reg_l) ;

: (free_h) 				( n -- )
  DUP #free-req >= IF ." Request more free registers." ABORT THEN
  free@ (vreg>reg_h) ;
  
: free0l 0 (free_l) ;
: free1l 1 (free_l) ;
: free2l 2 (free_l) ;
: free3l 3 (free_l) ;
: free4l 4 (free_l) ;
: free5l 5 (free_l) ;
: free0h 0 (free_h) ;
: free1h 1 (free_h) ;
: free2h 2 (free_h) ;
: free3h 3 (free_h) ;
: free4h 4 (free_h) ;
: free5h 5 (free_h) ;

: (tos_l) 				( n -- )
  DUP #tos-cache >= IF ." Request more registers." ABORT THEN
  cache@ (vreg>reg_l) ;

: (tos_h) 				( n -- )
  DUP #tos-cache >= IF ." Request more registers." ABORT THEN
  cache@ (vreg>reg_h) ;

: tos0l 0 (tos_l) ;
: tos1l 1 (tos_l) ;
: tos2l 2 (tos_l) ;
: tos3l 3 (tos_l) ;
: tos4l 4 (tos_l) ;
: tos5l 5 (tos_l) ;
: tos0h 0 (tos_h) ;
: tos1h 1 (tos_h) ;
: tos2h 2 (tos_h) ;
: tos3h 3 (tos_h) ;
: tos4h 4 (tos_h) ;
: tos5h 5 (tos_h) ;

\ Scale index base addressing. All words are ( offs -- )
: [tos0+tos1]   SC-1 0 cache@ (vreg>reg) 1 cache@ (vreg>reg) (#[sib]) ;
: [4*tos0+tos1] SC-4 0 cache@ (vreg>reg) 1 cache@ (vreg>reg) (#[sib]) ;

\ ==============================================================================
\ =============================== compiler support =============================
\ ==============================================================================

\ Produce a near jump and put the address of the cell with the distance on the
\ stack. Due to the fact that these jumps are relative no relocation is
\ nessesary.
: fwd-jmp 				( xt -- addr )
  0 ## EXECUTE asm-here 4 - ;

\ Resolve the jump to this address.
: resolve-jmp 				( fwd-addr -- )
  asm-here 					\ addr here
  over 4 + - 				\ addr rel
  SWAP asm-! ;

\ Save the allocator state in the returned dyn-array.
: allocator-state 			( addr --  )
  #tos-cache OVER C! CHAR+ 		\ addr
  #tos-cache 0 ?DO 			\ addr
    I cache@  				\ addr reg
    OVER C! CHAR+
  LOOP 
  offs-ebp SWAP C!
  ;

\ Set the bit with the number vreg for each register vreg in the state.
: (state-mask) 				( state #regs -- mask )
  0 -ROT 0 ?DO 				\ mask state 
    DUP I CHARS + C@ 			\ mask state vreg
    1 SWAP LSHIFT 			\ mask state vrmask
    ROT OR SWAP 			\ mask state
  LOOP DROP ;

\ Find the lowest bit set in x and return it's number.
: lowest-bit 				( x -- nr )
  #USEREGS 0 DO 			\ x
    1 I LSHIFT OVER AND 		\ x reg?
    IF 					\ x
      DROP I UNLOOP EXIT
    THEN
  LOOP DROP REG-NONE ;

\ Try to load a register that is in state but not in cache. There must be at
\ least one of them since (alloc-load) is called only with fewer regs in cache
\ than in state.
: ((alloc-load)) 			( state #regs -- )
  ( find regs in cache )
  (cached-mask) 			\ state #regs cache-mask
  ( find regs in state )
  -ROT (state-mask) 			\ cache-mask state-mask
  ( leave all flags that are in state-mask AND NOT in cache-mask )
  SWAP INVERT AND 			\ load-mask
  DUP 0= IF (internal-error) THEN 	\ load-mask
  lowest-bit 				\ vreg
  (#load) ;

\ Load as many registers as nessesary. Try to use those that needed.
: (alloc-load) 				( state #regs -- state #regs )
  BEGIN
    #tos-cache 				\ state #regs #tc
    OVER 				\ state #regs #tc #regs
    <
  WHILE
    2DUP ((alloc-load))
  REPEAT 
  ;

\ Flush some register till the same number as in state in reached.
: (alloc-flush) 			( #regs -- #regs )
  BEGIN
    #tos-cache OVER 			\ #regs #tc #regs
    >
  WHILE
    (flushreg) DROP
  REPEAT ;

\ Exchange vreg and cache(ind) if nessesary.
: (alloc-adjust) 			( vreg ind -- )
  cache@ 				\ vreg creg
  2DUP = IF
    2DROP
  ELSE
    tc-xchg
  THEN ;

\ Perform sign extension from a byte to a cell.
: (sign-extend) 			( c -- n )
  DUP 128 AND 				\ c sign-bit
  0<> 255 INVERT AND OR
;

\ Retrieve the save offset and correct ebp if nessesary. If save-flags? is
\ true, the CPU flags are saved before and restored afterwards ( add could
\ change them).
: (fix-offs) 				( save-flags? state ind -- )
  CHARS + C@ (sign-extend) 		\ save? dest-offs
  offs-ebp OVER = IF
    2DROP
  ELSE 					\ save? dest-offs
    OVER IF pushf, THEN 		\ save? dest-offs
    offs-ebp OVER - 
    BOFFS [ebp] ebp lea,
    TO offs-ebp 			\ save?
    IF popf, THEN
  THEN ;

\ Rebuild the allocator to the given state.  The index of the user data in
\ state is returned.
\ Algo:
\ 1. too few regs cached? -> load registers
\ 2. too many regs cached? -> flush registers
\ 3. exchange regs
: allocator-rebuild 			( save-flags? state -- )
  DUP C@ SWAP CHAR+ SWAP 		\ save? state #regs
  (alloc-load) 
  (alloc-flush) 			\ save? state #regs
  0 SWAP 				\ save? state ind #regs
  0 ?DO 				\ save? state ind
    2DUP CHARS + C@ 			\ save? state ind reg(ind)
    OVER (alloc-adjust) 		\ save? state ind
    1+
  LOOP 					\ save? state ind
  (fix-offs) ;

\ Store the state in the allocator without generating code.
: allocator-store 			( state -- )
  DUP C@ 				\ state #regs
  DUP TO #tos-cache
  1 CHARS SWAP 				\ state ind #regs
  0 ?DO 				\ state ind
    2DUP CHARS + C@ 			\ state ind vreg
    I cache! 				\ state ind
    CHAR+
  LOOP 					\ state ind
  + C@ (sign-extend) TO offs-ebp ;


