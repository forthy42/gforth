#! /usr/stud/paysan/bin/forth

$1FFE Constant 8190
Create flags 8190 allot
flags 8190 + AConstant eflag

: PRIMES  ( -- n )  FLAGS 8190 1 FILL  0 3  EFLAG FLAGS
  DO   I C@
       IF  DUP I + DUP EFLAG <
           IF    EFLAG SWAP
                 DO  0 I C! DUP  +LOOP
           ELSE  DROP  THEN  >R 1+ R>
           THEN  2 +
       LOOP  DROP ;

: BENCHMARK  0 &100 0 DO  PRIMES NIP  LOOP ;
&10 BASE !
BENCHMARK .
BYE
\ HPPA/720, 50 MHz: user 3.90s
