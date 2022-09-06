\ Benchmark for find-name-in

\ 10 million searches of all words in the forth-wordlist in the forth-wordlist
\ slightly depends on the version of Gforth

\ On a 4GHz Skylake:
\[~/gforth:132733] LC_NUMERIC=prog perf stat gforth-fast bench/hash-find2.fs
\
\ Performance counter stats for 'gforth-fast bench/hash-find2.fs':
\
\            445.63 msec task-clock                #    0.997 CPUs utilized
\                 4      context-switches          #    0.009 K/sec
\                 0      cpu-migrations            #    0.000 K/sec
\             1_919      page-faults               #    0.004 M/sec
\     1_777_969_995      cycles                    #    3.990 GHz
\     3_347_283_278      instructions              #    1.88  insn per cycle
\       380_018_231      branches                  #  852.771 M/sec
\        11_497_271      branch-misses             #    3.03% of all branches
\
\       0.447030027 seconds time elapsed
\
\       0.441976000 seconds user
\       0.004054000 seconds sys

: bench-nt ( n nt -- n1 f )
    name>string -1 /string forth-wordlist hash-find drop 1- dup ;

: bench ( n -- )
    begin
        ['] bench-nt forth-wordlist traverse-wordlist
    dup 0= until ;

10000000 bench bye
