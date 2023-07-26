#! /usr/bin/gforth

DECIMAL
CREATE FLAGS 8190 ALLOT
FLAGS 8190 + CONSTANT EFLAG

: PRIMES  ( -- n )  FLAGS 8190 1 FILL  0 3  EFLAG FLAGS
  DO   I C@
       IF  DUP I + DUP EFLAG <
           IF    EFLAG SWAP
                 DO  0 I C! DUP  +LOOP
           ELSE  DROP  THEN  SWAP 1+ SWAP
           THEN  2 +
       LOOP  DROP ;

: BENCHMARK  0 100 0 DO  PRIMES NIP  LOOP ;
UTIME BENCHMARK . UTIME 2SWAP D- CR D. .( usecs)
script? [IF] bye [THEN]

\ HPPA/720, 50 MHz: user 3.90s
