\ convert day since 0-3-1 to ymd
 
: /mod3 ( n1 n2 -- r q )
    dup >r /mod dup 4 = IF  drop r@ + 3  THEN  rdrop ;
 
: day2dow ( day -- dow )
    2 + 7 mod ;
 
\ julian calendar
 
: j-day2ymd ( day -- y m d )
    1461 /mod 4 * swap
    365 /mod3 rot + swap
    31 + 5 153 */mod swap 5 /
    >r 2 + dup 12 > IF  12 - swap 1+ swap  THEN
    r> 1+ ;
 
: (ymd2day) ( y m d -- day year/4 )  1- -rot
    2 - dup 0<= IF  12 + swap 1- swap  THEN
    153 5 */ 31 - swap
    4 /mod swap 365 * swap >r + + r> ;
 
: j-ymd2day ( y m d -- day )  (ymd2day)
    1461 * + ;
 
\ gregorian calendar
 
1582 10 15 (ymd2day) 2Constant gregorian.
1582 10 5 j-ymd2day Constant gregorian
 
: day2ymd ( day -- y m d ) dup gregorian >= IF
	1 - 146097 /mod 400 * swap
	36524 /mod3 100 * rot + swap
	j-day2ymd 2>r + 2r>
    ELSE
	1 + j-day2ymd
    THEN ;
 
: ymd2day ( y m d -- day )  (ymd2day)
    2dup gregorian. d< 0= IF
	25 /mod swap 1461 * swap
	4 /mod swap 36524 * swap
	146097 * + + + 2 +
    ELSE
	1461 * +
    THEN ;
