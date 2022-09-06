\ Benchmark for find-name-in, for misses

\ 10 million searches of all words in the forth-wordlist with their
\ preceeding character (typically resulting in a miss), in the
\ forth-wordlist slightly depends on the version of Gforth

\ On a 4GHz Skylake:
\ [~/gforth:132718] LC_NUMERIC=prog perf stat gforth-fast bench/find-name-in2.fs
\ Performance counter stats for 'gforth-fast bench/find-name-in2.fs':
\
\            523.76 msec task-clock                #    0.997 CPUs utilized
\                 2      context-switches          #    0.004 K/sec
\                 0      cpu-migrations            #    0.000 K/sec
\             1_917      page-faults               #    0.004 M/sec
\     2_090_343_363      cycles                    #    3.991 GHz
\     4_287_377_850      instructions              #    2.05  insn per cycle
\       490_034_458      branches                  #  935.610 M/sec
\        11_571_986      branch-misses             #    2.36% of all branches
\
\       0.525155591 seconds time elapsed
\
\       0.524156000 seconds user
\       0.000000000 seconds sys

: bench-nt ( n nt -- n1 f )
    name>string -1 /string forth-wordlist find-name-in drop 1- dup ;

: bench ( n -- )
    begin
        ['] bench-nt forth-wordlist traverse-wordlist
    dup 0= until ;

10000000 bench bye
