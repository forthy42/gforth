\ Use Forth as server-side script language
: $> ( -- )
    BEGIN  source >in @ /string s" <$" search  0= WHILE
	type cr refill  0= UNTIL  EXIT  THEN
    nip source >in @ /string rot - dup 2 + >in +! type ;
: <HTML>  ." <HTML>" $> ;
