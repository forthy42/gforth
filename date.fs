\ convert day since 0-3-1 to ymd
\ public domain
 
: /mod3 ( n1 n2 -- r q )
    dup >r /mod dup 4 = IF  drop r@ + 3  THEN  rdrop ;
 
: day2dow ( day -- dow )  1+ 7 mod ;
 
\ julian calendar

: j-day2ymd ( day -- y m d )
    1461 /mod 4 * swap
    365 /mod3 under+
    31 + 5 153 */mod swap 5 / >r
    2 + dup 12 > IF  12 - 1 under+  THEN
    r> 1+ ;
 
: (ymd2day) ( y m d -- day year/4 )
    1- -rot
    2 - dup 0<= IF  12 + -1 under+  THEN
    153 5 */mod swap 0= >r 31 - swap
    4 /mod swap 365 * swap >r + + r> swap r> + 1+ swap ;
 
: j-ymd2day ( y m d -- day )  (ymd2day) 1461 * + ;
 
\ gregorian calendar
 
1582 10 15 (ymd2day) 1 0 d+ 2Constant gregorian.
1582 10 5 j-ymd2day Constant gregorian
 
: day2ymd ( day -- y m d )
    dup gregorian >= IF
	2 - 146097 /mod 400 * swap
	36524 /mod3 100 * under+
	j-day2ymd 2>r + 2r>
    ELSE
	j-day2ymd
    THEN ;
 
: ymd2day ( y m d -- day )
    (ymd2day)
    over 1+ over gregorian. d< 0= IF
	25 /mod swap 1461 * swap
	4 /mod swap 36524 * swap
	146097 * + + + 2 +
    ELSE
	1461 * +
    THEN ;

[defined] t{ [defined] cov% and [IF]
    t{ 0 3 1 ymd2day dup day2dow -> 0 1 }t cov% cr
    t{ 1582 10 15 ymd2day 1- day2ymd -> 1582 10 4 }t cov% cr
    t{ 1400 3 1 ymd2day 1- day2ymd -> 1400 2 29 }t cov% cr
    t{ 2018 1 1 ymd2day 1- day2ymd -> 2017 12 31 }t cov% cr
    \ the tests up to here are sufficient for a full code coverage.
    \ they are not sufficient to ensure functionality.
    t{ -61 day2ymd -> -1 12 31 }t cov% cr \ check if we can go negative
    t{ 1900 3 1 ymd2day 1- day2ymd -> 1900 2 28 }t cov% cr
    t{ 1582 10 4 ymd2day 1+ day2ymd -> 1582 10 15 }t cov% cr
    13 1 [DO] t{ 2018 [I] 13 ymd2day day2ymd -> 2018 [I] 13 }t [LOOP] cov% cr
    32 1 [DO] t{ 2018 12 [I] ymd2day day2ymd -> 2018 12 [I] }t [LOOP] cov% cr
    t{ 2018 2 1 ymd2day 1- day2ymd -> 2018 1 31 }t cov% cr
    t{ 2018 3 1 ymd2day 1- day2ymd -> 2018 2 28 }t cov% cr
    t{ 2018 4 1 ymd2day 1- day2ymd -> 2018 3 31 }t cov% cr
    t{ 2018 5 1 ymd2day 1- day2ymd -> 2018 4 30 }t cov% cr
    t{ 2018 6 1 ymd2day 1- day2ymd -> 2018 5 31 }t cov% cr
    t{ 2018 7 1 ymd2day 1- day2ymd -> 2018 6 30 }t cov% cr
    t{ 2018 8 1 ymd2day 1- day2ymd -> 2018 7 31 }t cov% cr
    t{ 2018 9 1 ymd2day 1- day2ymd -> 2018 8 31 }t cov% cr
    t{ 2018 10 1 ymd2day 1- day2ymd -> 2018 9 30 }t cov% cr
    t{ 2018 11 1 ymd2day 1- day2ymd -> 2018 10 31 }t cov% cr
    t{ 2018 12 1 ymd2day 1- day2ymd -> 2018 11 30 }t cov% cr
    2100 1904 [DO] t{ [I] 3 1 ymd2day 1- day2ymd -> [I] 2 29 }t 4 [+LOOP]
    2000 1700 [DO] t{ [I] 3 1 ymd2day 1- day2ymd -> [I] 2 28 }t 100 [+LOOP] cov% cr
    1620 1560 [DO] t{ [I] 1 3 ymd2day day2ymd -> [I] 1 3 }t [LOOP] cov% cr
    7  0 [DO] t{ 1896 [I] + 12 13 ymd2day day2dow -> [I] }t [lOOP]    cov% cr
    2000 1 1 ymd2day 1461 bounds [DO] t{ [I] day2ymd ymd2day -> [I] }t [LOOP] cov% cr
    1580 1 1 ymd2day 1461 bounds [DO] t{ [I] day2ymd ymd2day -> [I] }t [LOOP] cov% cr
    .coverage
    #ERRORS @ [IF]  error-color attr!  ." had " #ERRORS ? ." errors"
    [ELSE]  info-color attr!  ." passed successful"  [THEN]
    default-color attr! cr cov% cr
[THEN]
