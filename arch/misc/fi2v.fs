\ kernel to verilog converter

: .## base @ >r hex 0 <# # # #> type r> base ! ;

create item 2 allot

: >v ( addr u -- ) r/o open-file throw >r
    ." @0"
    BEGIN  item 2 r@ read-file throw  WHILE
	cr item c@ .## item char+ c@ .##
    REPEAT
    cr r> close-file throw ;

script? [IF]
   : all2v argc @ 2 ?DO I arg >v LOOP ;
   all2v bye
[THEN]
