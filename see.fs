\ SEE.FS       highend SEE for ANSforth                16may93jaw

\ May be cross-compiled

\ I'm sorry. This is really not "forthy" enough.

\ Ideas:        Level should be a stack

decimal

\ Screen format words                                   16may93jaw

VARIABLE C-Output   1 C-Output  !
VARIABLE C-Formated 1 C-Formated !
VARIABLE C-Highlight 0 C-Highlight !
VARIABLE C-Clearline 0 C-Clearline !

VARIABLE XPos
VARIABLE YPos
VARIABLE Level

: Format        C-Formated @ C-Output @ and
                IF dup spaces XPos +! ELSE drop THEN ;

: level+        7 Level +!
                Level @ XPos @ -
                dup 0> IF Format ELSE drop THEN ;

: level-        -7 Level +! ;

VARIABLE nlflag

DEFER nlcount ' noop IS nlcount

: nl            nlflag on ;
: (nl)          nlcount
                XPos @ Level @ = ?Exit
                C-Formated @ IF
                C-Output @
                IF C-Clearline @ IF 80 XPos @ - spaces
                                 ELSE cr THEN
                1 YPos +! 0 XPos !
                Level @ spaces
                THEN Level @ XPos ! THEN ;

: warp?         ( len -- len )
                nlflag @ IF (nl) nlflag off THEN
                XPos @ over + 79 u> IF (nl) THEN ;

: ctype         ( adr len -- )
                warp? dup XPos +! C-Output @ IF type ELSE 2drop THEN ;

: cemit         1 warp?
                over bl = Level @ XPos @ = and
                IF 2drop ELSE XPos +! C-Output @ IF emit ELSE drop THEN
                THEN ;

DEFER .string

[IFDEF] Green
VARIABLE Colors Colors on

: (.string)     ( c-addr u n -- )
                over warp? drop
                Colors @
                IF C-Highlight @ ?dup
                   IF   CT@ swap CT@ or
                   ELSE CT@
                   THEN
                attr! ELSE drop THEN
                ctype  ct @ attr! ;
[ELSE]
: (.string)     ( c-addr u n -- )
                drop ctype ;
[THEN]

' (.string) IS .string


: .struc        Str# .string ;

\ CODES                                                 15may93jaw

21 CONSTANT RepeatCode
22 CONSTANT AgainCode
23 CONSTANT UntilCode
\ 09 CONSTANT WhileCode
10 CONSTANT ElseCode
11 CONSTANT AheadCode
13 CONSTANT WhileCode2
14 CONSTANT Disable

\ FORMAT WORDS                                          13jun93jaw

VARIABLE C-Stop
VARIABLE Branches

VARIABLE BranchPointer
VARIABLE SearchPointer
CREATE BranchTable 500 allot
here 3 cells -
ACONSTANT MaxTable

: FirstBranch BranchTable cell+ SearchPointer ! ;

: (BranchAddr?) ( a-addr -- a-addr true | false )
        SearchPointer @
        BEGIN   dup BranchPointer @ u<
        WHILE
                dup @ 2 pick <>
        WHILE   3 cells +
        REPEAT
        nip dup  3 cells + SearchPointer ! true
        ELSE
        2drop false
        THEN ;

: BranchAddr?
        FirstBranch (BranchAddr?) ;

' (BranchAddr?) ALIAS MoreBranchAddr?

: CheckEnd ( a-addr -- true | false )
        BranchTable cell+
        BEGIN   dup BranchPointer @ u<
        WHILE
                dup @ 2 pick u<=
        WHILE   3 cells +
        REPEAT
        2drop false
        ELSE
        2drop true
        THEN ;

\
\                 addrw               addrt
\       BEGIN ... WHILE ... AGAIN ... THEN
\         ^         !        !          ^
\         ----------+--------+          !
\                   !                   !
\                   +-------------------+
\
\

: CheckWhile ( a-addrw a-addrt -- true | false )
        BranchTable
        BEGIN   dup BranchPointer @ u<
        WHILE   dup @ 3 pick u>
                over @ 3 pick u< and
                IF dup cell+ @ 3 pick u<
                        IF 2drop drop true EXIT THEN
                THEN
                3 cells +
        REPEAT
        2drop drop false ;

: ,Branch ( a-addr -- )
        BranchPointer @ dup MaxTable u> ABORT" SEE: Table overflow"
        !
        1 cells BranchPointer +! ;

: Type!   ( u -- )
        BranchPointer @ 1 cells - ! ;

: Branch! ( a-addr rel -- a-addr )
        over + over ,Branch ,Branch 0 ,Branch ;

\ DEFER CheckUntil
VARIABLE NoOutput
VARIABLE C-Pass

0 CONSTANT ScanMode
1 CONSTANT DisplayMode
2 CONSTANT DebugMode

: Scan? ( -- flag ) C-Pass @ 0= ;
: Display? ( -- flag ) C-Pass @ 1 = ;
: Debug? ( -- flag ) C-Pass @ 2 = ;

: back? ( n -- flag ) 0< ;
: ahead? ( n -- flag ) 0> ;

: c-(compile)
        Display? IF s" POSTPONE " Com# .string
                    dup @ look 0= ABORT" SEE: No valid XT"
                    cell+ count $1F and 0 .string bl cemit
                 THEN
        cell+ ;

: c-lit
        Display? IF dup @ dup abs 0 <# #S rot sign #> 0 .string bl cemit THEN
        cell+ ;

: c-s"
        count 2dup + aligned -rot
        Display?
        IF      [char] S cemit [char] " cemit bl cemit 0 .string
                [char] " cemit bl cemit
        ELSE    2drop
        THEN ;

: c-."
        count 2dup + aligned -rot
        Display?
        IF      [char] . cemit
                [char] " cemit bl cemit 0 .string
                [char] " cemit bl cemit
        ELSE    2drop
        THEN ;

: c-c"
        count 2dup + aligned -rot
        Display?
        IF      [char] C cemit [char] " cemit bl cemit 0 .string
                [char] " cemit bl cemit
        ELSE    2drop
        THEN ;


: Forward? ( a-addr true | false -- )
        IF      dup dup @ swap 1 cells - @ -
                Ahead? IF true ELSE drop false THEN
                \ only if forward jump
        ELSE    false THEN ;

: RepeatCheck
        IF  BEGIN  2dup
                   1 cells - @ swap dup @ +
                   u<=
            WHILE  drop dup cell+
                   MoreBranchAddr? 0=
            UNTIL  false
            ELSE   true
            THEN
        ELSE false
        THEN ;

: c-branch
        Scan?
        IF      dup @ Branch!
                dup @ back?
                IF                      \ might be: AGAIN, REPEAT
                        dup cell+ BranchAddr? Forward?
                        RepeatCheck
                        IF      RepeatCode Type!
                                cell+ Disable swap !
                        ELSE    AgainCode Type!
                        THEN
                ELSE    dup cell+ BranchAddr? Forward?
                        IF      ElseCode Type! drop
                        ELSE    AheadCode Type!
                        THEN
                THEN
        THEN
        Display?
        IF
                dup @ back?
                IF                      \ might be: AGAIN, REPEAT
                        level- nl
                        dup cell+ BranchAddr? Forward?
                        RepeatCheck
                        IF      drop S" REPEAT " .struc nl
                        ELSE    S" AGAIN " .struc nl
                        THEN
                ELSE    dup cell+ BranchAddr? Forward?
                        IF      dup cell+ @ WhileCode2 =
                                IF nl S" ELSE" .struc level+
                                ELSE level- nl S" ELSE" .struc level+ THEN
                                cell+ Disable swap !
                        ELSE    S" AHEAD" .struc level+
                        THEN
                THEN
        THEN
        Debug?
        IF      dup @ +
        ELSE    cell+
        THEN ;

: MyBranch      ( a-addr -- a-addr a-addr2 )
                dup @ over +
                BranchAddr?
                BEGIN
                WHILE 1 cells - @
                      over <>
                WHILE dup @ over +
                      MoreBranchAddr?
                REPEAT
                SearchPointer @ 3 cells -
                ELSE    true ABORT" SEE: Table failure"
                THEN ;

: DebugBranch
        Debug?
        IF      dup @ over + swap THEN ; \ return 2 different addresses

: c-?branch
        Scan?
        IF      dup @ Branch!
                dup @ Back?
                IF      UntilCode Type! THEN
        THEN
        Display?
        IF      dup @ Back?
                IF      level- nl S" UNTIL " .struc nl
                ELSE    dup    dup @ over +
                        CheckWhile
                        IF      MyBranch
                                cell+ dup @ 0=
                                         IF WhileCode2 swap !
                                         ELSE drop THEN
                                level- nl
                                S" WHILE" .struc
                                level+
                        ELSE    nl S" IF" .struc level+
                        THEN
                THEN
        THEN
        DebugBranch
        cell+ ;

: c-do
        Display? IF nl S" DO" .struc level+ THEN ;

: c-?do
        Display? IF nl S" ?DO" .struc level+ THEN
        DebugBranch cell+ ;

: c-for
        Display? IF nl S" FOR" .struc level+ THEN ;

: c-next
        Display? IF level- nl S" NEXT " .struc nl THEN
        DebugBranch cell+ cell+ ;

: c-loop
        Display? IF level- nl S" LOOP " .struc nl THEN
        DebugBranch cell+ cell+ ;


: c-+loop
        Display? IF level- nl S" +LOOP " .struc nl THEN
        DebugBranch cell+ cell+ ;

: c-leave
        Display? IF S" LEAVE " .struc THEN
        Debug? IF dup @ + THEN cell+ ;

: c-?leave
        Display? IF S" ?LEAVE " .struc THEN
        cell+ DebugBranch swap cell+ swap cell+ ;

: c-exit  dup 1 cells -
        CheckEnd
        IF      Display? IF nlflag off S" ;" Com# .string THEN
                C-Stop on
        ELSE    Display? IF S" EXIT " .struc THEN
        THEN
        Debug? IF drop THEN ;

: c-;code               \ end of create part
        Display? IF S" DOES> " Com# .string THEN
        Cell+ cell+ ;

: c-abort"
        count 2dup + aligned -rot
        Display?
        IF      S" ABORT" .struc
                [char] " cemit bl cemit 0 .string
                [char] " cemit bl cemit
        ELSE    2drop
        THEN ;


CREATE C-Table
        ' lit A,         ' c-lit A,
        ' (s") A,        ' c-s" A,
        ' (.") A,        ' c-." A,
        ' "lit A,        ' c-c" A,
        ' ?branch A,     ' c-?branch A,
        ' branch A,      ' c-branch A,
        ' leave A,       ' c-leave A,
        ' ?leave A,      ' c-?leave A,
        ' (do) A,        ' c-do A,
        ' (?do) A,       ' c-?do A,
        ' (for) A,       ' c-for A,
        ' (loop) A,      ' c-loop A,
        ' (+loop) A,     ' c-+loop A,
        ' (next) A,      ' c-next A,
        ' ;s A,          ' c-exit A,
        ' (;code) A,     ' c-;code A,
        ' (abort") A,    ' c-abort" A,
        ' (compile) A,   ' c-(compile) A,
        0 ,

\ DOTABLE                                               15may93jaw

: DoTable ( cfa -- flag )
        C-Table
        BEGIN   dup @ dup
        WHILE   2 pick <>
        WHILE   2 cells +
        REPEAT
        nip cell+ @ EXECUTE
        true
        ELSE
        2drop drop false
        THEN ;

: BranchTo? ( a-addr -- a-addr )
        Display?  IF     dup BranchAddr?
                        IF BEGIN cell+ @ dup 20 u>
                                IF drop nl S" BEGIN " .struc level+
                                ELSE
                                  dup Disable <>
                                  IF   WhileCode2 =
                                       IF nl S" THEN " .struc nl ELSE
                                       level- nl S" THEN " .struc nl THEN
                                  ELSE drop THEN
                                THEN
                                  dup MoreBranchAddr? 0=
                           UNTIL
                        THEN
                  THEN ;

: analyse ( a-addr1 -- a-addr2 )
        Branches @ IF BranchTo? THEN
        dup cell+ swap @
        dup >r DoTable r> swap IF drop EXIT THEN
        Display?
        IF look 0= IF  drop dup 1 cells - @ .  \ ABORT" SEE: Bua!"
           ELSE  dup cell+ count 31 and rot wordinfo .string  THEN  bl cemit
        ELSE drop
        THEN ;

: c-init
        0 YPos ! 0 XPos !
        0 Level ! nlflag off
        BranchTable BranchPointer !
        c-stop off
        Branches on ;

: makepass ( a-addr -- )
        c-stop off
        BEGIN
                analyse
                c-stop @
        UNTIL drop ;

DEFER dosee

: dopri .name ." is primitive" cr ;
: dovar .name ." is variable" cr ;
: docon  dup .name ." is constant, value: "
         cell+ (name>) >body @ . cr ;
: doval .name ." is value" cr ;
: dodef .name ." is defered word, is: "
         here @ look 0= ABORT" SEE: No valid xt in defered word"
        .name cr here @ look drop dosee ;
: dodoe .name ." is created word" cr
        S" DOES> " Com# .string XPos @ Level !
        here @ dup C-Pass @ DebugMode = IF ScanMode c-pass ! EXIT THEN
        ScanMode c-pass ! dup makepass
        DisplayMode c-pass ! makepass ;
: doali .name ." is alias of "
        here @ .name cr
        here @ dosee ;
: docol S" : " Com# .string
        dup cell+ count $1F and 2 pick wordinfo .string bl cemit bl cemit
        ( XPos @ ) 2 Level !
        name> >body
        C-Pass @ DebugMode = IF ScanMode c-pass ! EXIT THEN
        ScanMode c-pass ! dup makepass
        DisplayMode c-pass ! makepass ;

create wordtypes
        Pri# ,   ' dopri A,
        Var# ,   ' dovar A,
        Con# ,   ' docon A,
        Val# ,   ' doval A,
        Def# ,   ' dodef A,
        Doe# ,   ' dodoe A,
        Ali# ,   ' doali A,
        Col# ,   ' docol A,
        0 ,

: (dosee) ( lfa -- )
        dup dup cell+ c@ 32 and IF over .name ." is an immediate word" cr THEN
        wordinfo
        wordtypes
        BEGIN dup @ dup
        WHILE 2 pick = IF cell+ @ nip EXECUTE EXIT THEN
              2 cells +
        REPEAT
        2drop
        .name ." Don't know how to handle" cr ;

' (dosee) IS dosee

: xtc ( xt -- )       \ do see at xt
        Look 0= ABORT" SEE: No valid XT"
        cr c-init
        dosee ;

: see   name find 0= IF ." Word unknown" cr drop exit THEN
        xtc ;

: lfc   cr c-init cell+ dosee ;
: nfc   cr c-init dosee ;


