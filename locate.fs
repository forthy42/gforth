\ locate, using the TAGS file

require string.fs

Variable last-file
Variable line-buf

\ example: invoke vi

Defer do-location

Variable sys-buf
: invoke-vi ( filename u line -- ) base @ >r decimal
    s" vi " sys-buf $!
    0 <# bl hold #S '+ hold #> sys-buf $+!
    sys-buf $+!
    r> base ! sys-buf $@ system ;
' invoke-vi IS do-location

\ scan file

: tag-line ( fid -- flag ) >r
    s" " line-buf $!
    $100 line-buf $!len
    line-buf $@ r> read-line throw
    swap line-buf $!len ;
: check-word ( addr u -- addr u flag )
    line-buf $@ #del $split 2nip ctrl A $split 2drop
    2swap search nip nip ;
: get-file ( fid -- )
    tag-line drop line-buf $@ ', $split 2drop last-file $! ;
: print-location ( -- ) base @ >r decimal
    last-file $@ line-buf $@ ctrl A $split 2nip ', $split 2drop
    0. 2swap >number 2drop drop r> base !
    do-location ;
: locate ( "name" -- )  s" " last-file $!  bl sword
    s" TAGS" r/o open-file throw >r
    BEGIN  r@ tag-line  WHILE
        s" " line-buf $@ str=
        IF    r@ get-file
        ELSE  2dup check-word
            IF  print-location 2drop  r> close-file throw  EXIT  THEN
        THEN
    REPEAT r> close-file throw 2drop true abort" tag not found" ;
