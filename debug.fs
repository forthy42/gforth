\ DEBUG.FS     Debugger                                12jun93jaw

\ Copyright (C) 1995,1996,1997 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

decimal

VARIABLE dbg-ip     \ istruction pointer for debugger

: scanword ( body -- )
        c-init C-Output off
        ScanMode c-pass !
        dup MakePass
        0 Level !
        0 XPos !
        DisplayMode c-pass !
        MakePass
        C-Output on ;

: .n    0 <# # # # # #S #> ctype bl cemit ;

: d.s   ." [ " depth . ." ] "
        depth 4 min dup 0 ?DO dup i - pick .n LOOP drop ;

: NoFine        XPos off YPos off
                NLFlag off Level off
                C-Formated off
                ;

: disp-step
        DisplayMode c-pass !            \ change to displaymode
        cr
        c-stop off
        Base @ hex dbg-ip @ 8 u.r space dbg-ip @ @ 8 u.r space
        Base !
        NoFine 10 XPos !
        dbg-ip @ DisplayMode c-pass ! Analyse drop
        25 XPos @ - 0 max spaces ." -> " ;

: get-next ( -- n | n n )
        DebugMode c-pass !
        dbg-ip @ Analyse ;

: jump          ( addr -- )
                r> drop \ discard last ip
                >r ;

AVARIABLE DebugLoop

: breaker      r> 1 cells - dbg-ip ! DebugLoop @ jump ;

CREATE BP 0 , 0 ,
CREATE DT 0 , 0 ,

: set-bp        ( 0 n | 0 n n -- )
                0. BP 2!
                ?dup IF dup BP ! dup @ DT !
                        ['] Breaker swap !
                        ?dup IF dup BP cell+ ! dup @ DT cell+ !
                                ['] Breaker swap ! drop THEN
                     THEN ;

: restore-bp    ( -- )
                BP @ ?dup IF DT @ swap ! THEN
                BP cell+ @ ?dup IF DT cell+ @ swap ! THEN ;

VARIABLE Body

: NestXT        ( xt -- true | body false )
		\ special deal for create does> words
		\ leaves body address on the stack
		dup >does-code IF dup >body swap THEN

                DebugMode c-pass ! C-Output off
                xt-see C-Output on
                c-pass @ DebugMode = dup
                IF      ." Cannot debug" cr
                THEN ;         

VARIABLE Nesting

: Leave-D
                C-Formated on
                C-Output on ;

VARIABLE Unnest

: D-KEY         ( -- flag )
        BEGIN
                Unnest @ IF 0 ELSE key THEN
                CASE    [char] n OF     dbg-ip @ @ NestXT EXIT ENDOF
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

: (debug) ( body -- )
        0 Nesting !
        BEGIN   Unnest off
                cr ." Scanning code..." cr C-Formated on
                dup scanword dbg-ip !
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
                dbg-ip @ 1 cells + >r 1 Nesting +!
                THEN
        AGAIN ;

: dbg \ gforth 
    ' NestXT IF EXIT THEN (debug) Leave-D ;

has? compiler [IF]
: break: \ gforth
    r> ['] (debug) >body >r ;

: (break")
    cr
    ." BREAK AT: " type cr
    r> ['] (debug) >body >r ;

: break" \ gforth
    postpone s"
    postpone (break") ; immediate
[THEN]
