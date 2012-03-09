\ smart .s

: addr? ( addr -- flag )
    TRY  @  IFERROR  2drop  false  ELSE  drop  true  THEN   ENDTRY ;

: string? ( addr u -- flag )
    TRY  bounds ?DO  I c@ bl < IF  -1 throw  THEN  LOOP
	IFERROR  2drop drop false  ELSE  true  THEN  ENDTRY ;

: .string. ( addr u -- )
    '"' emit type '"' emit space ;
: .addr. ( addr -- )  hex. ;

: .s ( -- ) \ tools dot-s
\G Display the number of items on the data stack, followed by a list
\G of the items (but not more than specified by @code{maxdepth-.s};
\G TOS is the right-most item.
    ." <" depth 0 .r ." > "
    depth 0 max maxdepth-.s @ min
    dup 0
    ?do
	dup i - pick  over i - pick  2dup string? IF  .string. 2
	ELSE  drop dup addr? IF  .addr. 1
	    ELSE  .s. 1  THEN
	THEN
    +loop
    drop ;
