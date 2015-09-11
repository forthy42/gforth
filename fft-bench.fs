\ fft based floating point benchmark

require fft.fs

: >values ( -- ) #points 0 ?DO  I $55 and s>f I $AA and s>f I values z!  LOOP ;

: setup ( -- ) 32 1024 * points >values ;

: main setup fft rfft normalize ;

Variable pass
: test ( -- )
    main pass on
    #points 0 ?DO
	i values z@ fround f>s fround f>s
	I $aa and I $55 and d<> IF i . i values z@ z. cr pass off THEN
    LOOP  pass @ IF ." passed test" cr THEN ;
