\ generates random numbers                             12jan94py

Variable seed

$10450405 Constant generator

: rnd  ( -- n )  seed @ generator um* drop 1+ dup seed ! ;

: random ( n -- 0..n-1 )  rnd um* nip ;
