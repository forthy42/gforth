\ substitute stuff

require string.fs

wordlist AConstant macros-wordlist

: macro: ( addr u -- ) Create here 0 , $! DOES> $@ ;

: replaces ( addr1 len1 addr2 len2 -- )
    \G create a macro with name @var{addr2 len2} and content @var{addr1 len1}.
    \G if the macro already exists, just change the content.
    2dup macros-wordlist search-wordlist
    IF  nip nip >body $!
    ELSE
	get-current >r macros-wordlist set-current
	['] macro: execute-parsing
	r> set-current
    THEN ;

User macro$

: $substitute ( addr1 len1 -- addr2 len2 n )
    \G substitute all macros in text @var{addr1 len1}.
    \G @var{n} is the number of substitutions, @var{addr2 len2} the result.
    macro$ $off 0 >r
    BEGIN  dup  WHILE  '%' $split
	    2swap macro$ $+! dup IF
		over c@ '%' = IF
		    '%' macro$ c$+! 1 /string
		ELSE
		    '%' $split 2swap dup 0= IF
			2drop s" %" macro$ $+! r> 1+ >r
		    ELSE
			macros-wordlist search-wordlist  IF
			    -rot 2>r execute macro$ $+! 2r> r> 1+ >r
			THEN
		    THEN
		THEN
	    THEN
    REPEAT  2drop macro$ $@ r> ;

: substitute ( addr1 len1 addr2 len2 -- addr2 len3 n )
    \G substitute all macros in text @var{addr1 len1}, and copy the
    \G result to @var{addr2 len2}.  @var{n} is the number of
    \G substitutions, @var{addr2 len3} the result.  If
    \G @var{len2}=@var{len3}, it is likely that the string did not fit.
    2>r $substitute -rot
    2r> rot umin 2dup 2>r move 2r> rot ;

: unescape ( addr1 u1 dest -- dest u2 )
    \G double all delimiters in @var{addr1 u1}, so that substitute
    \G will result in the original text.  Note that the buffer
    \G @var{dest} does not have a size, as in worst case, it will need
    \G just twice as many characters as @var{u1}. @{dest u2} is the
    \G resulting string.
    dp @ >r dup >r dp !
    bounds ?DO
	I c@ dup '%' = IF  dup c,  THEN  c,
    LOOP  r> here over -  r> dp ! ;