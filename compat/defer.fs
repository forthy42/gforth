\ deferred words and perform

\ This file is in the public domain. NO WARRANTY.

: perform ( ? addr -- ? )
    @ execute ;

: defer ( "name" -- )
    create ['] abort , \ you should not rely on initialization with noop
does> ( ? -- ? )
    perform ;

: defer@ ( xt1 -- xt2 )
  >body @ ;

: defer! ( xt2 xt1 -- )
  >body ! ;

: <is> ( xt "name" -- )
    ' defer! ;

: [is] ( compilation: "name" -- ; run-time: xt -- )
    postpone ['] postpone defer! ; immediate

: is
  state @ if
    postpone [is]
  else
    <is>
  then ; immediate

: action-of
 state @ if
   POSTPONE ['] POSTPONE defer@
 else
   ' defer@
then ; immediate
    

