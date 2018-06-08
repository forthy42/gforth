\ find-name and find-name-in

\ this file is in the public domain

[defined] toupper 0= [IF]
    : toupper ( c1 -- c2 )
	dup 'a' 'z' 1+ within bl and - ; [THEN]
[defined] capscompare 0= [IF]
    : capscompare  ( c_addr1 u1 c_addr2 u2 -- -1|0|1 )
	rot 2dup u< negate >r 2dup u> r> or >r
	umin bounds ?DO
	    count toupper i c@ toupper - ?dup-IF
		nip 0< 2* 1+ unloop rdrop exit THEN
	LOOP
	drop r> ;
[THEN]

: find-name-in ( addr u wid -- nt / 0 )
    >r 0 -rot r>
    [: dup >r name>string 2over capscompare IF
	    rdrop true
	ELSE
	    rot drop r> -rot false
	THEN
    ;] swap traverse-wordlist 2drop ;
: find-name {: c-addr u -- nt | 0 :}
    get-order 0 swap 0 ?do ( widn...widi nt|0 )
	dup 0= if
	    drop c-addr u rot find-name-in
	else
	    nip
	then
    loop ;
