

doer? :docon [IF]
: docon: ( -- addr )	\ gforth
    \G the code address of a @code{CONSTANT}
    ['] :docon ;
[THEN]

: docol: ( -- addr )	\ gforth
    \G the code address of a colon definition
    0 ;

doer? :dovar [IF]
: dovar: ( -- addr )	\ gforth
    \G the code address of a @code{CREATE}d word
    \ in rom-applications variable might be implemented with constant
    \ use really a created word!
    ['] :dovar ;
[THEN]

doer? :douser [IF]
: douser: ( -- addr )	\ gforth
    \G the code address of a @code{USER} variable
    ['] :douser ;
[THEN]

doer? :dodefer [IF]
: dodefer: ( -- addr )	\ gforth
    \G the code address of a @code{defer}ed word
    ['] :dodefer ;
[THEN]

doer? :dofield [IF]
: dofield: ( -- addr )	\ gforth
    \G the code address of a @code{field}
    ['] :dofield ;
[THEN]

has? prims 0= [IF]
: dodoes: ( -- addr )	\ gforth
    \G the code address of a @code{field}
    ['] :dodoes ;
[THEN]

: check-inliners	( -- code-address true | xt false )
  dup @
  CASE	dovar: SkipInlineMark @ OF	drop dovar: true EXIT ENDOF
	docon: SkipInlineMark @ OF	drop docon: true EXIT ENDOF
	douser: SkipInlineMark @ OF	drop douser: true EXIT ENDOF
  ENDCASE
  false ;

: call-destination
  \ isolate value
  dup @ $07FFFFFF and
  \ do sign extention if we need to
  dup $04000000 and
  IF	$F8000000 or THEN
  \ and resolve offset
  cells + ( dest ) ;

: check-calls ( dest -- code-address true | dest false )
\ if it is a call at the beginning of a definition
\ we have to check whether it is a call to a doer
  dup
  CASE  dodoes: 	OF true EXIT ENDOF
	dodefer: 	OF true EXIT ENDOF
  ENDCASE
  false ;

: >code-address ( cfa -- code-address )
  dup c@ $F8 and $08 =
  IF \ call detected, calculate destination
	call-destination
	check-calls
  ELSE	check-inliners
  THEN
  \ we found nothing, must be a normal colon definition
  0= IF drop docol: THEN
  ;

: doer!	( code-address cfa -- )
  here >r dp !
  docol, ]comp
  colon,
  fini, comp[
  r> dp ! ;

: code-address! ( code-address cfa -- )
  over
  IF	doer!
  ELSE	-1 ABORT" Arghh!" 
  THEN  ;  

: does-code! 	( code-address cfa -- )
  dodoes: over doer!
  cell+ ! ;

: /does-handler 
  0 ;

: does-handler! ( does-handler-addr -- )
  drop ;

