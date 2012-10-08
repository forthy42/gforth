\ fft based floating point benchmark

require fft.fs

: >values ( -- ) #points 0 ?DO  I $55 and s>f I $AA and s>f I values z!  LOOP ;

: setup ( -- ) 32 1024 * points >values ;

: main setup fft rfft normalize ;