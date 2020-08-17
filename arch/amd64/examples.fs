\ A few examples using gForth assembler for AMD64
\ provided by Anthony Vogelaar, Perth UK
\ rev 0.10  2020-08-17  initial release

\ Do nothing
ABI-CODE aNOP  ( -- )
   DI          AX      MOV          \ SP out := SP in 
                       RET
END-CODE


\ Drop TOS
ABI-CODE aDROP  ( n -- )
   DI          AX      MOV          \ SPout := SPin
   8   #       AX      ADD          \ SP := SP + 8 byte = 64 bit
                       RET
END-CODE


\ Push 5 on the data stack
ABI-CODE aFIVE   ( -- 5 )
   DI          AX      MOV          \ SPout := SPin
   8   #       AX      SUB          \ Increase stack by 1 cell
   5   #       AX  )   MOV          \ Add 5 to TOS
                       RET
END-CODE 


\ Push 10 and 20 to data stack
ABI-CODE aTOS2  ( -- n n )
   DI          AX      MOV          \ SPout := SPin
   8   #       AX      SUB          \ Increase stack by 1 cell
   10  #       AX  )   MOV          \ Add 10 to TOS
   8   #       AX      SUB          \ Increase stack by 1 cell
   20  #       AX  )   MOV          \ Add 20 to TOS
                       RET
END-CODE


\ Push 5 and 1 to data stack
ABI-CODE aFIVE.   ( -- 5 1 )
   DI          AX      MOV          \ SPout := SPin
   16  #       AX      SUB          \ Increase stack by 2 cells
   5   #   8   AX  D)  MOV          \ Add 5 to TOS-1
   1   #       AX  )   MOV          \ Add 1 to TOS
                       RET
END-CODE


\ Get Time Stamp Counter as two 32 bit integers
\ The TSC is incremented every CPU clock pulse
ABI-CODE aRDTSC   ( -- TSCl TSCh )
                       RDTSC        \ DX:AX := TSC
   $FFFFFFFF # AX      AND          \ Clear upper 32 bit AX
   0xFFFFFFFF # DX     AND          \ Clear upper 32 bit DX
   AX          R8      MOV          \ Tempory save AX
   DI          AX      MOV          \ SPout := SPin
   16  #       AX      SUB          \ Create two cells on data stack
   R8  8       AX  D)  MOV          \ TOS-1 := saved AX = TSC low
   DX          AX  )   MOV          \ TOS := Dx = TSC high
                       RET
END-CODE


\ Get Time Stamp Counter as 64 bit integer
ABI-CODE RDTSC   ( -- TSC )
                       RDTSC        \ DX:AX := TSC
   $FFFFFFFF # AX      AND          \ Clear upper 32 bit AX
   32  #       DX      SHL          \ Move lower 32 bit DX to upper 32 
bit
   AX          DX      OR           \ Combine AX wit DX in DX
   DI          AX      MOV          \ SPout := SPin
   8   #       AX      SUB          \ Add 1 cell to stack
   DX          AX  )   MOV          \ TOS := DX
                       RET
END-CODE


VARIABLE V

\ Assign 4 to variable V
ABI-CODE V=4 ( -- )
   BX                  PUSH         \ Save BX, used by gforth
   V   #       BX      MOV          \ BX := address of V
   4   #       BX )    MOV          \ Write 4 to V
   BX                  POP          \ Restore BX
   DI          AX      MOV          \ SPout := SPin
                       RET
END-CODE


\ Assign 5 to variable V
ABI-CODE V=5 ( -- )
   V   #       CX      MOV          \ BX := address of V
   5   #       CX )    MOV          \ Write 5 to V
   DI          AX      MOV          \ SPout := SPin
                       RET
END-CODE


\ Do two IF tests
ABI-CODE TEST2  ( -- n n )
   DI      AX          MOV              \ SPout := SPin
   5   #   CX          MOV              \ CX := 5
   5   #   CX          CMP
   0= IF
       1   #   DX          MOV          \ If CX = 5 then DX := 1  <--
   ELSE
       2   #   DX          MOV          \ else DX := 2
   THEN
   8   #   AX          SUB              \ Add DX to stack
   DX      AX  )       MOV
   6   #   CX          CMP
   0= IF
       3   #   DX          MOV          \ If CX = 6 then DX := 3
   ELSE
       4   #   DX          MOV          \ else DX := 4  <--
   THEN
   8   #   AX          SUB              \ Add DX to stack
   DX      AX  )       MOV
                       RET
END-CODE


\ Do four loops
ABI-CODE LOOP4  ( -- n n n n )
   DI      AX          MOV             \ SPout := SPin
   -1  #   CX          MOV             \ CX := -1
   4   #   DX          MOV             \ DX := 4 loop counter
   BEGIN
       8   #   AX          SUB         \ Add CX to stack
       CX      AX  )       MOV
       1   #   DX          SUB         \ DX := DX - 1
   0= UNTIL
                    RET
END-CODE


