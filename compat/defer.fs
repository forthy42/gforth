\ deferred words and perform

\ This file is in the public domain. NO WARRANTY.

: noop ;

: perform ( ? addr -- ? )
    @ execute ;

: defer ( "name" -- )
    create ['] noop , \ you should not rely on initialization with noop
does> ( ? -- ? )
    perform ;

: <is> ( xt "name" -- )
    ' >body ! ;

: [is] ( compilation: "name" -- ; run-time: xt -- )
    ' >body postpone Literal postpone ! ; immediate
    

