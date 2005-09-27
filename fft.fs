\           *** Fast Fourier Transformation ***        15may93py

require complex.fs

: Carray ( -- )  Create 0 ,  DOES> @ swap complex' + ;
Carray values
Carray expix

: r+ BEGIN 2dup xor -rot and dup WHILE 1 rshift REPEAT  drop ;
: reverse  ( n -- )  2/ dup dup 2* 1
  DO  dup I < IF  dup values I values 2dup z@ z@ z! z! THEN
      over r+  LOOP  2drop ;

\ reverse carry add                                    23sep05py
8 Value #points
: realloc ( n addr -- )
    dup @  IF  dup @ free throw  THEN  swap allocate throw swap ! ;
: points  ( n --- )  dup to #points dup complex' dup
  ['] values >body realloc  2/
  ['] expix  >body realloc
  dup 0 DO  0e 0e I values z!  LOOP
  1e 0e 0 expix z! 2/ dup 2/ dup 2/ dup 1+ 1
  ?DO  pi I I' 1- 2* 2* fm*/ fsincos fswap   I expix z!  LOOP
  ?DO  I' I - 1- expix z@ fswap    I 1+ expix z!  LOOP  dup 2/
  ?DO  I' I -    expix z@ fswap fnegate fswap
                                    I    expix z!  LOOP ;
: .values  ( -- )  precision  4 set-precision
  #points 0 DO  I values z@ z. cr  LOOP  set-precision ;
: .expix  ( -- )   precision  4 set-precision
  #points 2/ 0 DO  I expix z@ z. cr  LOOP  set-precision ;
' .values ALIAS .rvalues

\ FFT                                                  23sep05py

: z2dup+ zover zover z+ ;

: butterfly ( cexpix addr1 addr2 -- cexpix )
  zdup over z@ z* dup z@ z2dup+ z! zr- z! ;
: butterflies ( cexpix step off end start -- )
  ?DO  dup I + values I values butterfly
  over +LOOP  zdrop drop ;
: fft-step ( n flag step steps -- n flag step )
  0 DO  I 2 pick I' */ 2/ expix z@
        2 pick IF  fnegate  THEN  I' 2 pick I butterflies
  LOOP ;

\ FFT                                                  23sep05py

: (fft ( n flag -- )  swap dup reverse 1
  BEGIN  2dup >  WHILE  dup 2* swap fft-step
  REPEAT  2drop drop ;

: normalize ( -- )  #points dup s>f 1/f
  0 DO  I values dup z@ 2 fpick zscale z!  LOOP  fdrop ;

: fft  ( -- )  #points  true (fft ;
: rfft ( -- )  #points false (fft ;

