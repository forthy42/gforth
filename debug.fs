\ DEBUG.FS     Debugger                                12jun93jaw

\ Copyright (C) 1995 Free Software Foundation, Inc.

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

VARIABLE IP     \ istruction pointer for debugger

\ Formated debugger words                               12jun93jaw

false [IF]

Color: Men#
<A red >b yellow >f bold A> Men# CT!

CREATE D-LineIP 80 cells allot
CREATE D-XPos   300 chars allot align
CREATE D-LineA  80 cells allot
VARIABLE ^LineA

VARIABLE D-Lines
VARIABLE D-Line
VARIABLE D-MaxLines 10 D-MaxLines !
VARIABLE D-Bugline

: WatcherInit
        D-MaxLines @ 3 + YPos ! 0 D-Line ! ;

: (lines)
        1 cells ^LineA +!
        O-PNT@ ^LineA @ ! ;

VARIABLE Body

: ScanWord ( body -- )
        dup body !
        c-init
        ScanMode c-pass !
        C-Formated on   0 Level !
        C-ClearLine on
        Colors on
        0 XPos ! 0 YPos !
        O-INIT
        dup MakePass
        DisplayMode c-pass !
        c-stop off
        D-LineIP 80 cells erase
        0 D-Lines ! dup D-LineIP !
        O-PNT@ D-LineA ! D-LineA ^LineA !
        ['] (lines) IS nlcount
        XPos @ D-XPos c!
        BEGIN   analyse
                D-Lines @ YPos @ <>
                IF      YPos @ D-Lines !
                        dup YPos @ cells D-LineIP + !
                THEN
                XPos @ over Body @ - 0 1 cells um/mod nip chars
                D-XPos + c!
                C-Stop @
        UNTIL drop
        O-PNT@ YPos @ 1+ cells D-LineA + !
        -1 YPos @ 1+ cells D-LineIP + !
        O-DEINIT
        C-Formated off
        0 D-Line !
        ['] noop IS nlcount ;

: SearchLine ( addr -- n )
        D-LineIP D-Lines @ 0
        ?DO     dup @ 2 pick U> IF 2drop I 1- UNLOOP EXIT THEN
                cell+
        LOOP    2drop 0 ;

: Display ( n -- )
        dup cells D-LineA + @ O-Buffer +
        swap D-MaxLines @ + D-Lines @ min 1+
             cells D-LineA + @ O-Buffer +
        over - type ;

\ [IFDEF] Green Colors on [THEN]
\        dup D-TableL + C@ dup Level ! dup XPos ! spaces 0 YPos !
\        D-LineIP + @ C-Stop off
\        BEGIN
\        [IFDEF] Green IP @ over =
\                IF hig# C-Highlight ! ELSE C-Highlight off THEN
\        [THEN]
\                Analyse
\                C-Stop @ YPos @ D-MaxLines @ u>= or
\        UNTIL   drop ;

: TopLine
        0 0 at-xy
        Men# CT@ attr!
        ." OSB-DEBUG (C) 1993 by Jens A. Wilke" cr cr
        \ one step beyond
        0 CT@ attr! ;

: BottomLine
        0 D-MaxLines @ 3 + at-xy
        Men# CT@ attr!
        ." U-nnest D-one N-est A-bort" cr
        0 CT@ attr! ;

VARIABLE LastIP

: (supress)
        YPos @ D-MaxLines @ U>=
        IF c-output off THEN ;

: DispIP
        ['] (supress) IS nlcount
        dup SearchLine D-Line @ - dup YPos ! 2 +
        over Body @ - 0 1 cells um/mod nip chars D-XPos + c@
        swap AT-XY
        Analyse drop
        ['] noop IS nlcount
        c-output on ;

: Watcher ( -- )
        TopLine
        IP @ SearchLine dup D-Line @ dup D-MaxLines @ +
        within
        IF      drop D-Line @ Display
        ELSE    D-MaxLines @ 2/ - 0 max dup D-Line !
                Display
        THEN
        C-Formated off Colors on
\        LastIP @ ?DUP IF DispIP THEN
        Hig# C-Highlight !
        IP @ DispIP IP @ LastIP !
        C-Formated on C-Highlight off
        BottomLine ;


' noop ALIAS \w immediate

\ end formated debugger words

[ELSE]
' \ alias \w immediate

: scanword ( body -- )
        c-init C-Output off
        ScanMode c-pass !
        dup MakePass
        0 Level !
        0 XPos !
        DisplayMode c-pass !
        MakePass
        C-Output on ;
[THEN]

: .n    0 <# # # # # #S #> ctype bl cemit ;

: d.s   ." [ " depth . ." ] "
        depth 4 min dup 0 ?DO dup i - pick .n LOOP drop ;

: NoFine        XPos off YPos off
                NLFlag off Level off
                C-Formated off
[IFDEF] Colors  Colors off [THEN]
                ;

: disp-step
        DisplayMode c-pass !            \ change to displaymode
\       Branches Off                    \ don't display
\                                       \ BEGIN and THEN
        cr
\w      YPos @ 1+ D-BugLine !
\w      Watcher
        c-stop off
\w      0 D-BugLine @ at-xy
        Base @ hex IP @ 8 u.r space IP @ @ 8 u.r space
        Base !
        NoFine 10 XPos !
\w      D-Bugline @ YPos !
        ip @ DisplayMode c-pass ! Analyse drop
        25 XPos @ - 0 max spaces ." -> " ;

: get-next ( -- n | n n )
        DebugMode c-pass !
        ip @ Analyse ;

: jump          ( addr -- )
                r> drop \ discard last ip
                >r ;

AVARIABLE DebugLoop

: breaker      r> 1 cells - IP ! DebugLoop @ jump ;

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
                DebugMode c-pass ! C-Output off
                xt-see C-Output on
                c-pass @ DebugMode = dup
                IF      ." Cannot debug" cr
                THEN ;         

VARIABLE Nesting

: Leave-D
[IFDEF] Colors  Colors on [THEN]
                C-Formated on
                C-Output on ;

VARIABLE Unnest

: D-KEY         ( -- flag )
        BEGIN
                Unnest @ IF 0 ELSE key THEN
                CASE    [char] n OF     IP @ @ NestXT EXIT ENDOF
                        [char] s OF     Leave-D
                                        -128 THROW ENDOF
                        [char] a OF     Leave-D
                                        -128 THROW ENDOF
                        [char] d OF     Leave-D
                                        cr ." Done..." cr
                                        Nesting off
                                        r> drop IP @ >r
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
                dup scanword IP !
                cr ." Nesting debugger ready!" cr
                \w WatcherInit 0 CT@ attr! page
                BEGIN   disp-step D-Key
                WHILE   C-Stop @ 0=
                WHILE   0 get-next set-bp
                        IP @ jump
                        [ here DebugLoop ! ]
                        restore-bp
                        d.s
                REPEAT
                Nesting @ 0= ?EXIT
                -1 Nesting +! r>
                ELSE
                IP @ 1 cells + >r 1 Nesting +!
                THEN
        AGAIN ;

: dbg   ' NestXT ?EXIT (debug) Leave-D ;

: break:
        r> ['] (debug) >body >r ;

: (break")
        cr
        ." BREAK AT: " type cr
        r> ['] (debug) >body >r ;

: break"
        postpone s"
        postpone (break") ; immediate
