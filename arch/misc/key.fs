\ misc-key.fs basic-io for misc processor		01feb97jaw


c: key? $ffff x@ 0<> ;

c: (key)  BEGIN key? UNTIL $fffe x@ ;

c: (emit) $fffc x! ;

c: (type)  BEGIN  dup  WHILE
    >r dup c@ (emit) 1+ r> 1-  REPEAT  2drop ;
\ bounds ?DO i c@ emit LOOP ;


