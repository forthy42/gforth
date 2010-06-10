$49056090 constant led#1                                                    
$49056094 constant led#2                                                    
: led-off $600000 led#1 ! ;                                                 
: led1 $200000 led#2 ! ;                                                    
: led2 $400000 led#2 ! ;
555 Constant /ms
: ms  0 ?DO /ms 0 DO LOOP LOOP ;
: blink BEGIN
  led-off led1 100 ms
  led-off led2 100 ms
  key? UNTIL key drop ;

