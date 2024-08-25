\ measure the two words given in the manual

\ invoke with gforth-fast -e "1 7" bench/stagediv.fs
\ where 1 (or 2) is the number of stages, and 7 is the u for array/

\ for j in 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 100 2000; do echo $j; for i in 1 2; do perf stat -e cycles:u gforth-fast -e "$i $j" bench/stagediv.fs |& grep cycles:u; done; done


constant u
constant stages


stages 1 = [if]
: array/ ( addr u n -- )
  -rot cells bounds u+do
    i @ over / i !
  1 cells +loop
  drop ;
[then]
stages 2 = [if]
: array/ ( addr u n -- )
  {: | reci[ staged/-size ] :}
  reci[ /f-stage1m
  cells bounds u+do
    i @ reci[ /f-stage2m i !
  1 cells +loop ;
[then]

#2000 constant #elems
create orig #elems cells allot
create working #elems cells allot

: init ( -- )
    1000000 #elems 0 do
        dup orig i th ! 10 +
    loop ;

init

: bench ( -- )
    10000000 #elems / 0 ?do
        orig working #elems cells move
        working #elems u / 0 ?do
            dup u 3 array/
            u cells +
        loop
        drop
    loop ;
bench
bye
        
    