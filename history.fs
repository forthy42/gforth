\ History file support                                 16oct94py

0 Value history

2Variable forward^
2Variable backward^
2Variable end^

: get-history ( addr len -- wid )
  2dup r/w open-file 0<
  IF  drop r/w create-file throw  ELSE  nip nip  THEN
  to history
  history file-size throw
  2dup forward^ 2! 2dup backward^ 2! end^ 2! ;

s" gforth.history" get-history

\ moving in history file                               16oct94py

: clear-line ( max span addr pos1 -- max addr )
  backspaces over spaces swap backspaces ;

: clear-tib ( max span addr pos -- max 0 addr 0 false )
  clear-line 0 tuck dup ;

: get-line ( max addr -- max span addr pos dpos )
  history file-position throw  backward^ 2!
  2dup swap history read-line throw drop
  2dup type tuck
  history file-position throw  forward^ 2! ;

: next-line  ( max span addr pos1 -- max span addr pos2 false )
  clear-line
  forward^ 2@ history reposition-file throw
  get-line  0 ;

: prev-line  ( max span addr pos1 -- max span addr pos2 false )
  clear-line over 2 + negate s>d backward^ 2@ d+ 0. dmax
  history reposition-file throw  0.
  BEGIN   2over swap history read-line throw nip  WHILE
          history file-position throw
	  2dup backward^ 2@ d<  WHILE  2swap 2drop
  REPEAT  2drop  THEN
  history reposition-file throw get-line 0 ;

: ctrl  ( "<char>" -- ctrl-code )
  char [char] @ - postpone Literal ; immediate

Create lfpad #lf c,

: (enter)  ( max span addr pos1 -- max span addr pos2 true )
  >r end^ 2@ history reposition-file throw
  2dup swap history write-file throw
  lfpad 1 history write-file throw
  history file-position throw 2dup backward^ 2! end^ 2!
  r> (ret) ;

\ some other key commands                              16oct94py

: first-pos  ( max span addr pos1 -- max span addr 0 0 )
  backspaces 0 0 ;
: end-pos  ( max span addr pos1 -- max span addr span 0 )
  type-rest 2drop over 0 ;

: extract-word ( addr len -- addr' len' )  dup >r
  BEGIN  1- dup 0>=  WHILE  2dup + c@ bl =  UNTIL  THEN  1+
  tuck + r> rot - ;

Create prefix-found  0 , 0 ,

: word-lex ( nfa1 nfa2 -- -1/0/1 )
  dup 0=  IF  2drop 1  EXIT  THEN
  cell+ >r cell+ count $1F and
  dup r@ c@ $1F and =
  IF  r> char+ capscomp 0<=  EXIT  THEN
  nip r> c@ $1F and < ;

: search-prefix  ( addr len1 -- suffix len2 )
  context @ @  0 >r
  BEGIN  dup  WHILE
         >r dup r@ cell+ c@ $1F and <=
	 IF  2dup r@ cell+ char+ capscomp  0=
	     IF  r> dup r@ word-lex
		 IF  dup prefix-found @ word-lex
		     0>= IF  rdrop dup >r  THEN
		 THEN >r
	     THEN
	 THEN  r> @
  REPEAT drop r> dup prefix-found ! ?dup
  IF    cell+ count $1F and rot /string rot drop
  ELSE  2drop s" "  THEN  ;

: tab-expand ( max span addr pos1 -- max span addr pos2 0 )
  prefix-found cell+ @  0 ?DO  (del)  LOOP
  2dup extract-word search-prefix
  dup prefix-found @ 0<> - prefix-found cell+ !
  bounds ?DO  I c@ (ins)  LOOP
  prefix-found @ IF  bl (ins)  THEN  0 ;

: kill-prefix  ( key -- key )
  dup #tab <> IF  0 0 prefix-found 2!  THEN ;

' kill-prefix IS everychar

' next-line  ctrl N cells ctrlkeys + !
' prev-line  ctrl P cells ctrlkeys + !
' clear-tib  ctrl K cells ctrlkeys + !
' first-pos  ctrl A cells ctrlkeys + !
' end-pos    ctrl E cells ctrlkeys + !
' (enter)    #lf    cells ctrlkeys + !
' (enter)    #cr    cells ctrlkeys + !
' tab-expand #tab   cells ctrlkeys + !
