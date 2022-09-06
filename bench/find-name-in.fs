\ Benchmark for find-name-in

\ 10 million searches of all words in the forth-wordlist in the forth-wordlist
\ slightly depends on the version of Gforth

\ On a 4GHz Skylake:

\ [~/gforth:132716] LC_NUMERIC=prog perf stat gforth-fast bench/find-name-in.fs
\ Performance counter stats for 'gforth-fast bench/find-name-in.fs':
\
\          1_450.07 msec task-clock                #    0.999 CPUs utilized
\                 6      context-switches          #    0.004 K/sec
\                 0      cpu-migrations            #    0.000 K/sec
\             5_779      page-faults               #    0.004 M/sec
\     5_792_850_484      cycles                    #    3.995 GHz
\    14_221_025_238      instructions              #    2.45  insn per cycle
\     1_619_785_077      branches                  # 1117.040 M/sec
\        20_963_841      branch-misses             #    1.29% of all branches
\
\       1.451415567 seconds time elapsed
\
\       1.430380000 seconds user
\       0.020033000 seconds sys



: bench-nt ( n nt -- n1 f )
    name>string forth-wordlist find-name-in drop 1- dup ;

: bench ( n -- )
    begin
        ['] bench-nt forth-wordlist traverse-wordlist
    dup 0= until ;

10000000 bench bye
