\ TOOLS.FS     Toolkit extentions                      2may93jaw

\ May be cross-compiled

hex

\ .S            CORE / CORE EXT                         9may93jaw

variable maxdepth-.s
9 maxdepth-.s !

: .s ( -- )
    ." <" depth 0 .r ." > "
    depth 0 max maxdepth-.s @ min
    dup 0
    ?do
	dup i - pick .
    loop
    drop ;

\ DUMP                       2may93jaw - 9may93jaw    06jul93py
\ looks very nice, I know

Variable /dump

: .4 ( addr -- addr' )
    3 FOR  -1 /dump +!  /dump @ 0<
        IF  ."    "  ELSE  dup c@ 0 <# # # #> type space  THEN
    char+ NEXT ;
: .chars ( addr -- )
    /dump @ bounds
    ?DO I c@ dup 7f bl within
	IF  drop [char] .  THEN  emit
    LOOP ;

: .line ( addr -- )
  dup .4 space .4 ." - " .4 space .4 drop  10 /dump +!  space .chars ;

: dump  ( addr u -- )
    cr base @ >r hex        \ save base on return stack
    0 ?DO  I' I - 10 min /dump !
	dup 8 u.r ." : " dup .line cr  10 +
	10 +LOOP
    drop r> base ! ;

\ ?                                                     17may93jaw

: ? @ . ;

\ INCLUDE see.fs

