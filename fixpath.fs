\ fix path in gforth*.exe

." Fixing " 3 arg type ."  with " 2 arg type cr

$7410 Constant gforth.exe
$68D0 Constant gforth-fast.exe
$81E0 Constant gforth-ditc.exe

include string.fs

Variable path$  2 arg path$ $!
Variable pathes$  2 arg pathes$ $!
Variable exe$

pathes$ 1 1 $del
s" //" pathes$ 0 $ins
: fixpathes ( addr u -- )
  bounds ?DO  I c@ '\ = IF  '/ I c!  THEN  LOOP ;
pathes$ $@ fixpathes
s" .:" pathes$ 0 $ins

: fix-exe ( offset addr u -- )
  path$ $@ exe$ $! s" \" exe$ $+! exe$ $+!
  exe$ $@ r/w bin open-file throw >r
  0 r@ reposition-file throw
  pathes$ $@ 2dup + 0 swap c! 1+ r@ write-file throw
  r> close-file throw ;

3 arg evaluate 3 arg fix-exe

bye
