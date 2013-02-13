\ wrap TYPE and EMIT into strings

0 Value $execstr
: $type ( addr u -- )  $execstr $+! ;
: $emit ( char -- )    $execstr c$+! ;
: $exec ( xt addr -- )
    \G execute xt while the standard output (TYPE, EMIT, and everything
    \G that uses them) is redirected to the string variable addr.
    $execstr action-of type action-of emit { oldstr oldtype oldemit }
    try
	to $execstr \ $execstr @ 0= IF s" " $execstr $! THEN
	['] $type is type
	['] $emit is emit
	execute
	0 \ throw ball
    restore
	oldstr to $execstr
	oldtype is type
	oldemit is emit
    endtry
    throw ;
