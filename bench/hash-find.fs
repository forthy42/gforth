\ Benchmark for find-name-in

\ 10 million searches of all words in the forth-wordlist in the forth-wordlist
\ slightly depends on the version of Gforth

\ On a 4GHz Skylake:
\ [~/gforth:132730] LC_NUMERIC=prog perf stat gforth-fast bench/hash-find.fs
\
\ Performance counter stats for 'gforth-fast bench/hash-find.fs':
\
\            620.49 msec task-clock                #    0.998 CPUs utilized
\                 5      context-switches          #    0.008 K/sec
\                 0      cpu-migrations            #    0.000 K/sec
\             1_918      page-faults               #    0.003 M/sec
\     2_481_937_292      cycles                    #    4.000 GHz
\     4_575_206_301      instructions              #    1.84  insn per cycle
\       555_544_637      branches                  #  895.328 M/sec
\        20_669_577      branch-misses             #    3.72% of all branches
\
\       0.621859756 seconds time elapsed
\
\       0.620845000 seconds user
\       0.000000000 seconds sys

: bench-nt ( n nt -- n1 f )
    name>string forth-wordlist hash-find drop 1- dup ;

: bench ( n -- )
    begin
        ['] bench-nt forth-wordlist traverse-wordlist
    dup 0= until ;

10000000 bench bye
