
start-macros

  $04 Rb: tosl    $05 Rb: tosh
  $08 Rb: temp1l  $09 Rb: temp1h

  $00 Rw: sp      $01 Rw: rp      $02 Rw: tos     $03 Rw: ip
  $04 Rw: temp1   $05 Rw: temp2

: next,
   \  cc_uc , (debug calla,       \ Debugger-Aufruf
    temp1 , ip ]+ mov,          \ fetch cfa
    temp2 , temp1 ]+ mov,        \ get code address
    cc_uc , temp2 ] jmpi, 
    ;

: next1,
    temp2 , temp1 ]+ mov,
    cc_uc , temp2 ] jmpi,
   ;

end-macros

Label into-forth
    ip , $ffff # mov,
    sp , $fd80 # mov,
    rp , $fde0 # mov,
    next,
End-Label

Label (dout)
    _s0tic . 7 , here jnb,   _s0tic . 7 bclr,
    _s0tbuf , rl6 movb,   ret,
End-Label

Start-Macros

\ : dout,	>r rl6 , r> # movb,
\	cc_uc , (dout) Calla, ;

: dout,  drop ;

end-macros

Code: :docol
    ': dout,
    rp -] , ip mov,             \ store IP on return stack
    ip , temp1 mov,             \ 
    ip , 2 # add,               \ zum PFA
    next,
End-Code

Code ;s
    '; dout,
    ip , rp ]+ mov,             \ fetch callback address
    next,
End-Code

\ Rest						25jul97jaw

Code: :dovar
    '2 dout,
    sp -] , tos mov,
    temp1 , 2 # add,
    tos , temp1 mov,
    next,
End-Code

Code: :docon
    '1 dout,
    sp -] , tos mov,
    temp1 , 2 # add,
    tos , temp1 ] mov,
    next,
End-Code

Code: :dodoes
    '6 dout,
    rp -] , ip mov,
    ip , temp1 ] mov,
    sp -] , tos mov,
    temp1 , 2 # add,
    tos , temp1 mov,
    next,
End-Code
    
Code: :dodefer
    '4 dout,
    temp1 , 2 # add,
    temp1 , temp1 ] mov,
    Next1,
End-Code

Code execute	( xt -- ) \ execute colon definition
    'E dout,
    temp1 , tos mov,
    tos , sp ]+ mov,
    Next1,
End-Code   

\ Zusatzroutinen zu bedingten Befehlen          ( 17.06.96/KK )
  Code branch   ( -- ) \ Inline-Sprung ausfÅhren
    ip , ip ] add,  next,
   End-Code

  Code ?branch  ( f -- ) \ Test und Sprung bei 0
    tos , tos mov,
    cc_z IF,  tos , sp ]+ mov,   ip , ip ] add,    next,  THEN,
              tos , sp ]+ mov,   ip , 2 s#  add,   next,
   End-Code

  Code lit     ( -- n ) \ Inline-Literal lesen
    sp -] , tos mov,   tos , ip ]+ mov,   next,
   End-Code

\ Stack Words                                     ( 17.06.96/KK )

  Code dup      ( n -- n n ) \ TOS verdoppeln
    sp -] , tos mov,   next,
   End-Code

  Code 2dup     ( d -- d d ) \ TOS/NOS verdoppeln
    temp1 , sp ] mov,   sp -] , tos mov,
    sp -] , temp1 mov,   next,
   End-Code

  Code drop     ( n -- ) \ TOS entfernen
    tos , sp ]+ mov,   next,
   End-Code

  Code 2drop    ( d -- ) \ TOS/NOS entfernen
    sp , 2 s# add,   tos , sp ]+ mov,   next,
   End-Code

  Code swap     ( n1 n2 -- n2 n1 ) \ TOS/NOS vertauschen
    temp1 , sp ] mov,   sp ] , tos mov,   tos , temp1 mov,
    next,
   End-Code

  Code over     ( n1 n2 -- n1 n2 n1 ) \ NOS verdoppeln
    sp -] , tos mov,   tos , sp 2 #] mov,   next,
   End-Code

  Code rot     ( n1 n2 n3 -- n2 n3 n1 ) \ Rotieren
    temp1 , sp ]+ mov,   temp2 , sp ]+ mov,
    sp -] , temp1 mov,   sp -] , tos mov,
    tos , temp2 mov,   next,                           End-Code

  Code -rot     ( n1 n2 n3 -- n3 n1 n2 ) \ Rotieren
    temp1 , sp ]+ mov,   temp2 , sp ]+ mov,
    sp -] , tos mov,   sp -] , temp2 mov,
    tos , temp1 mov,   next,                           End-Code

Code sp@
    sp -] , tos mov,
    tos , sp mov,
    next,
End-Code

Code sp!
    sp , tos mov,
    tos , sp ]+ mov,
    next,
End-Code

Code rp@
    sp -] , tos mov,
    tos , rp mov,
    next,
End-Code

Code rp!
    rp , tos mov,
    tos , sp ]+ mov,
    next,
End-Code

Code r>
    sp -] , tos mov,
    tos , rp ]+ mov,
    next,
End-Code

Code >r
    rp -] , tos mov,
    tos , sp ]+ mov,
    next,
End-Code
    
Code r@
    sp -] , tos mov,
    tos , rp ] mov,
    next,
End-Code

\ Arithmetik					 ( 17.06.96/KK )

  Code +        ( n1 n2 -- n3 ) \ Addition
    tos , sp ]+ add,   next,
   End-Code

  Code -        ( n1 n2 -- n3 ) \ Subtraktion
    temp1 , tos mov,   tos , sp ]+ mov,
    tos , temp1 sub,   next,
   End-Code

  Code and      ( n1 n2 -- n3 ) \ Logische AND-VerknÅpfung
    tos , sp ]+ and,   next,
   End-Code

  Code xor      ( n1 n2 -- n3 ) \ Logische AND-VerknÅpfung
    tos , sp ]+ xor,   next,
   End-Code

  Code or       ( n1 n2 -- n3 ) \ Logische OR-VerknÅpfung
    tos , sp ]+ or,   next,
   End-Code

  Code 0=       ( n -- f ) \ Test auf 0
    tos , tos mov,
    cc_z IF,  tos , -1 # mov,  ELSE,  tos , 0 s# mov,  THEN,
    next,
   End-Code

  Code =        ( n1 n2 -- f ) \ Test auf Gleichheit
    tos , sp ]+ sub,
    cc_z IF,  tos , -1 # mov,  ELSE,  tos , 0 s# mov,  THEN,
    next,
   End-Code

\ Memory                                     ( 01.01.97/KK )

  Code c@       ( addr -- c ) \ Byte auslesen
    tosl , tos ] movb,   tosh , 0 s# movb,   next,
   End-Code

  Code @        ( addr -- n ) \ Wort auslesen
    tos , tos ] mov,   next,
   End-Code

  Code c!       ( c addr -- ) \ Byte schreiben
    temp1 , sp ]+ mov,   tos ] , temp1l movb,
    tos , sp ]+ mov,   next,
   End-Code

  Code !        ( n addr -- ) \ Wort schreiben
    temp1 , sp ]+ mov,   tos ] , temp1 mov,
    tos , sp ]+ mov,   next,
   End-Code


\ SIO-Grundroutinen                             ( 09.06.96/KK )
  Code key?     ( -- f ) \ Flag, ob Zeichen anliegt
    sp -] , tos mov,   tos , 0 s# mov,
    _s0ric . 7 , here 6 + jnb,
    tos , 1 s# sub,   next,                            End-Code
  Code (key)      ( -- char ) \ Zeichen holen
    _s0ric . 7 , here jnb,   _s0ric . 7 bclr,   sp -] , tos mov,
    tosh , 0 s# movb,   tosl , _s0rbuf movb,   next,   End-Code
  Code emit?    ( -- f ) \ Flag, ob Zeichen ausgebbar
    sp -] , tos mov,   tos , 0 s# mov,
    _s0tic . 7 , here 6 + jnb,
    tos , 1 s# sub,   next,                            End-Code
  Code (emit)     ( char -- ) \ Zeichen ausgeben
    _s0tic . 7 , here jnb,   _s0tic . 7 bclr,
    _s0tbuf , tosl movb,   tos , sp ]+ mov,   next,    End-Code
