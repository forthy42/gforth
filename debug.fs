\ DEBUG.FS     Debugger                                12jun93jaw

\ Copyright (C) 1995,1996,1997,2000,2003,2004,2007 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

require see.fs

decimal

VARIABLE dbg-ip     \ instruction pointer for debugger

\ !! move to see?

: save-see-flags ( -- n* cnt )
  C-Output @
  C-Formated @ 1 ;

: restore-see-flags ( n* cnt -- )
  drop C-Formated !
  C-Output ! ;

: scanword ( body -- )
        >r save-see-flags r>
        c-init C-Output off
        ScanMode c-pass !
        dup MakePass
        0 Level !
        0 XPos !
        DisplayMode c-pass !
        MakePass
        restore-see-flags ;

: .n ( n -- )    0 <# # # # # #S #> ctype bl cemit ;

: d.s   ( .. -- .. )  ." [ " depth . ." ] "
    depth 4 min dup 0 ?DO dup i - pick .n LOOP drop ;

: NoFine ( -- )
    XPos off YPos off
    NLFlag off Level off
    C-Formated off ;
		
: Leave-D ( -- ) ;

: disp-step ( -- )
\ display step at current dbg-ip
        DisplayMode c-pass !            \ change to displaymode
        cr
        c-stop off
        Base @ hex dbg-ip @ 8 u.r space dbg-ip @ @ 8 u.r space
        Base !
        save-see-flags
        NoFine 10 XPos !
        dbg-ip @ DisplayMode c-pass ! Analyse drop
        25 XPos @ - 0 max spaces ." -> " 
        restore-see-flags ;

: get-next ( -- n | n n )
        DebugMode c-pass !
        dbg-ip @ Analyse ;

: jump          ( addr -- )
    r> drop \ discard last ip
    >r ;

AVARIABLE DebugLoop

1 cells Constant breaker-size \ !!! dependency: ITC

: breaker ( R:body -- )
    r> breaker-size - dbg-ip ! DebugLoop @ jump ;

CREATE BP 0 , 0 ,
CREATE DT 0 , 0 ,

: set-bp        ( 0 n | 0 n n -- ) \ !!! dependency: ITC
                0. BP 2!
                ?dup IF dup BP ! dup @ DT !
                        ['] Breaker swap !
                        ?dup IF dup BP cell+ ! dup @ DT cell+ !
                                ['] Breaker swap ! drop THEN
                     THEN ;

: restore-bp    ( -- ) \ !!! dependency: ITC
    BP @ ?dup IF DT @ swap ! THEN
    BP cell+ @ ?dup IF DT cell+ @ swap ! THEN ;

VARIABLE Body

: nestXT-checkSpecial ( xt -- xt2 | cfa xt2 )
    dup ['] call = IF
	drop dbg-ip @ cell+ @ body>  EXIT
    THEN
    dup >does-code IF
	\ if nest into a does> we must leave
	\ the body address on stack as does> does...
	dup >body swap EXIT
    THEN
    dup ['] EXECUTE = IF   
	\ xt to EXECUTE is next stack item...
	drop EXIT 
    THEN
    dup ['] PERFORM = IF
	\ xt to EXECUTE is addressed by next stack item
	drop @ EXIT 
    THEN
    BEGIN
	dup >code-address dodefer: =
    WHILE
	    \ load xt of DEFERed word
	    cr ." nesting defered..." 
	    >body @    
    REPEAT ;

: nestXT ( xt -- true | body false )
\G return true if we are not able to debug this, 
\G body and false otherwise
  nestXT-checkSpecial 
  \ scan code with xt-see
  DebugMode c-pass ! C-Output off
  xt-see C-Output on
  c-pass @ DebugMode = dup
  IF      cr ." Cannot debug!!"
  THEN ;

VARIABLE Nesting

VARIABLE Unnest

: D-KEY         ( -- flag )
        BEGIN
                Unnest @ IF 0 ELSE key THEN
                CASE    [char] n OF     dbg-ip @ @ nestXT EXIT ENDOF
                        [char] s OF     Leave-D
                                        -128 THROW ENDOF
                        [char] a OF     Leave-D
                                        -128 THROW ENDOF
                        [char] d OF     Leave-D
                                        cr ." Done..." cr
                                        Nesting off
                                        r> drop dbg-ip @ >r
                                        EXIT ENDOF
                        [char] ? OF     cr ." Nest Stop Done Unnest" cr
                                        ENDOF
                        [char] u OF     Unnest on true EXIT ENDOF
                        drop true EXIT
                ENDCASE
        AGAIN ;

: (_debug) ( body ip -- )
        0 Nesting !
        BEGIN   Unnest off
                cr ." Scanning code..." cr C-Formated on
                swap scanword dbg-ip !
                cr ." Nesting debugger ready!" cr
                BEGIN   d.s disp-step D-Key
                WHILE   C-Stop @ 0=
                WHILE   0 get-next set-bp
                        dbg-ip @ jump
                        [ here DebugLoop ! ]
                        restore-bp
                REPEAT
                Nesting @ 0= IF EXIT THEN
                -1 Nesting +! r>
                ELSE
                get-next >r 1 Nesting +!
                THEN
                dup
        AGAIN ;

: (debug) dup (_debug) ;

: dbg ( "name" -- ) \ gforth 
    ' NestXT IF EXIT THEN (debug) Leave-D ;

: break:, ( -- )
  latestxt postpone literal ;

: (break:)
    r> ['] (_debug) >body >r ;
  
: break: ( -- ) \ gforth
    break:, postpone (break:) ; immediate

: (break")
    cr
    ." BREAK AT: " type cr
    r> ['] (_debug) >body >r ;

: break" ( 'ccc"' -- ) \ gforth
    break:,
    postpone s"
    postpone (break") ; immediate
