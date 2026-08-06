variable check# 0 check# !

: e.15-17 {: f: r -- :} \ gforth e-dot
    \G Print @i{r} with the least number of mantissa digits such that
    \G the result, when converted with @word{>float}, produces @i{r}
    \G (for finite @i{r}).  Caveat: @code{e.} only works as specified
    \G if @word{represent} (which calls the C library's
    \G @code{ecvt_r()}) produces the closest mantissa for the given
    \G buffer length; that is the case on not-too-old versions of
    \G glibc.
    20 15 do
        i 17 = if
            r i e.p unloop exit then
        r i ['] e.p >string-execute {: c-addr u :} \ c-addr u dump
        c-addr u >float 0= if \ inf or nan
            leave then
        r f= if
            leave then
        c-addr free throw
    loop
    c-addr u type
    c-addr free throw ;


: check {: f: r -- :}
    1 check# +!
    r `e. >string-execute {: d: e.-string :}
    r `e.15-17 >string-execute {: d: e.16-string :}
    e.-string e.16-string str= 0= if
        check# @ 12 .r space e.-string type space e.16-string type space r #15 e.p cr
    then ;

: s-as-f {: w^ n :} n f@ ;

1 52 lshift constant smallest-normal

1 seed!

: checkrnd ( u -- )
    0 ?do
        rnd abs dup smallest-normal >= if
            s-as-f check
        else
            drop
        then
    loop ;

: speedrnd {: u xt -- :}
    u 0 ?do
        rnd abs dup smallest-normal >= if
            s-as-f xt >string-execute drop free throw
        else
            drop
        then
    loop ;

: checkshort ( u -- )
    \ about 308 tests per u
    1 ?do
        308 0 do
            <<# i 0 #s 2drop s" e-" holds j 0 #s '.' hold #> >float assert( dup ) #>>
            drop check
        loop
    loop ;

\\\

calls and results:

[~/gforth:170275] time gforth edot-check.fs -e '1000000 `e. speedrnd .s bye'
<0> 
real    0m12.276s
user    0m12.274s
sys     0m0.001s
[~/gforth:170276] time gforth edot-check.fs -e '1000000 `e.15-17 speedrnd .s bye'
<0> 
real    0m2.052s
user    0m2.047s
sys     0m0.004s
[~/gforth:170271] time gforth edot-check.fs -e "10000000 checkrnd .s bye"
       19529 1.8071100890254e15 1807110089025400e 1807110089025400e
      376892 9.0733488798871e15 9073348879887100e 9073348879887100e
      520761 6.4662485482298e15 6466248548229800e 6466248548229800e
      623233 4.8387957875162e15 4838795787516200e 4838795787516200e
      666964 3.1374076362573e15 3137407636257300e 3137407636257300e
      676546 9.4174601502407e15 9417460150240700e 9417460150240700e
      864026 9.4891836635643e15 9489183663564300e 9489183663564300e
      963899 3.179839016423e14 317983901642300e 317983901642300e
     1035210 8.2736526331609e15 8273652633160900e 8273652633160900e
...
[~/gforth:170283] time gforth edot-check.fs -e "100000 checkshort check# ? .s bye"
30799692 <0> 
real    2m15.593s
user    2m14.433s
sys     0m1.156s
