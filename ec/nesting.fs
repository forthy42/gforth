\ nesting.fs displays nesting for primitive trace	12jun97jaw

Variable nestlevel

: main
  cr
  0 nestlevel !
  BEGIN
	key dup 9 u> WHILE
	dup
	CASE	': OF 	cr nestlevel @ spaces 1 nestlevel +! emit ENDOF
		'; OF	cr -1 nestlevel +! nestlevel @ spaces emit 
			cr nestlevel @ spaces ENDOF
		dup OF	dup 31 u> IF emit THEN ENDOF
	ENDCASE
  REPEAT drop bye ;	
