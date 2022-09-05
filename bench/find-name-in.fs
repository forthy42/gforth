\ Benchmark for find-name-in

\ 10 million searches of all words in the forth-wordlist in the forth-wordlist
\ slightly depends on the version of Gforth


: bench-nt ( n nt -- n1 f )
    name>string forth-wordlist find-name-in drop 1- dup ;

: bench ( n -- )
    begin
        ['] bench-nt forth-wordlist traverse-wordlist
    dup 0= until ;

10000000 bench bye
