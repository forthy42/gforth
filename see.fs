\ SEE.FS       highend SEE for ANSforth                16may93jaw

\ Copyright (C) 1995,2000,2003,2004,2006,2007,2008 Free Software Foundation, Inc.

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


\ May be cross-compiled

\ I'm sorry. This is really not "forthy" enough.

\ Ideas:        Level should be a stack

require look.fs
require termsize.fs
require wordinfo.fs

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
VARIABLE uppercase	\ structure words are in uppercase

DEFER nlcount ' noop IS nlcount

: nl            nlflag on ;
: (nl)          nlcount
                XPos @ Level @ = IF EXIT THEN \ ?Exit
                C-Formated @ IF
                C-Output @
                IF C-Clearline @ IF cols XPos @ - spaces
                                 ELSE cr THEN
                1 YPos +! 0 XPos !
                Level @ spaces
                THEN Level @ XPos ! THEN ;

: warp?         ( len -- len )
                nlflag @ IF (nl) nlflag off THEN
                XPos @ over + cols u>= IF (nl) THEN ;

: ctype         ( adr len -- )
                warp? dup XPos +! C-Output @ 
		IF uppercase @ IF bounds ?DO i c@ toupper emit LOOP
				  uppercase off ELSE type THEN
		ELSE 2drop THEN ;

: cemit         1 warp?
                over bl = Level @ XPos @ = and
                IF 2drop ELSE XPos +! C-Output @ IF emit ELSE drop THEN
                THEN ;

DEFER .string ( c-addr u n -- )

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

: c-\type ( c-addr u -- )
    \ type string in \-escaped form
    begin
	dup while
	    2dup newline string-prefix? if
		'\ cemit 'n cemit
		newline nip /string
	    else
		over c@
		dup '" = over '\ = or if
		    '\ cemit cemit
		else
		    dup bl 127 within if
			cemit
		    else
			base @ >r try
			    8 base ! 0 <<# # # # '\ hold #> ctype #>> 0
			restore
			    r@ base !
			endtry
			rdrop throw
		    endif
		endif
		1 /string
	    endif
    repeat
    2drop ;

: .struc        
	uppercase on Str# .string ;

\ CODES (Branchtypes)                                    15may93jaw

21 CONSTANT RepeatCode
22 CONSTANT AgainCode
23 CONSTANT UntilCode
\ 09 CONSTANT WhileCode
10 CONSTANT ElseCode
11 CONSTANT AheadCode
13 CONSTANT WhileCode2
14 CONSTANT Disable
15 CONSTANT LeaveCode


\ FORMAT WORDS                                          13jun93jaw

VARIABLE C-Stop
VARIABLE Branches

VARIABLE BranchPointer	\ point to the end of branch table
VARIABLE SearchPointer

\ The branchtable consists of three entrys:
\ address of branch , branch destination , branch type

CREATE BranchTable 128 cells allot
here 3 cells -
ACONSTANT MaxTable

: FirstBranch BranchTable cell+ SearchPointer ! ;

: (BranchAddr?) ( a-addr1 -- a-addr2 true | false )
\ searches a branch with destination a-addr1
\ a-addr1: branch destination
\ a-addr2: pointer in branch table
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

: MyBranch      ( a-addr -- a-addr a-addr2 )
\ finds branch table entry for branch at a-addr
                dup @
                BranchAddr?
                BEGIN
                WHILE 1 cells - @
                      over <>
                WHILE dup @
                      MoreBranchAddr?
                REPEAT
                SearchPointer @ 3 cells -
                ELSE    true ABORT" SEE: Table failure"
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
    over ,Branch ,Branch 0 ,Branch ;
\        over + over ,Branch ,Branch 0 ,Branch ;

\ DEFER CheckUntil
VARIABLE NoOutput
VARIABLE C-Pass

0 CONSTANT ScanMode
1 CONSTANT DisplayMode
2 CONSTANT DebugMode

: Scan? ( -- flag ) C-Pass @ 0= ;
: Display? ( -- flag ) C-Pass @ 1 = ;
: Debug? ( -- flag ) C-Pass @ 2 = ;

: back? ( addr target -- addr flag )
    over u< ;

: .word ( addr x -- addr )
    \ print x as a word if possible
    dup look 0= IF
	drop dup threaded>name dup 0= if
	    drop over 1 cells - @ dup body> look
	    IF
		nip nip dup ." <" name>string rot wordinfo .string ." > "
	    ELSE
		2drop ." <" 0 .r ." > "
	    THEN
	    EXIT
	then
    THEN
    nip dup cell+ @ immediate-mask and
    IF
	bl cemit  ." POSTPONE "
    THEN
    dup name>string rot wordinfo .string
    ;

: c-call ( addr1 -- addr2 )
    Display? IF
	dup @ body> .word bl cemit
    THEN
    cell+ ;

: c-callxt ( addr1 -- addr2 )
    Display? IF
	dup @ .word bl cemit
    THEN
    cell+ ;

\ here docon: , docol: , dovar: , douser: , dodefer: , dofield: ,
\ here over - 2constant doers

: c-lit ( addr1 -- addr2 )
    Display? IF
	dup @ dup body> dup cfaligned over = swap in-dictionary? and if
	    ( addr1 addr1@ )
	    dup body> @ dovar: = if
		drop c-call EXIT
	    endif
	endif
	\ !! test for cfa here, and print "['] ..."
	dup abs 0 <# #S rot sign #> 0 .string bl cemit
    endif
    cell+ ;

: c-lit+ ( addr1 -- addr2 )
    Display? if
	dup @ dup abs 0 <# #S rot sign #> 0 .string bl cemit
	s" + " 0 .string
    endif
    cell+ ;

: .name-without ( addr -- addr )
    \ !! the stack effect cannot be correct
    \ prints a name without a() e.g. a(+LOOP) or (s")
    dup 1 cells - @ threaded>name dup IF
	name>string over c@ 'a = IF
	    1 /string
	THEN
	 over c@ '( = IF
	    1 /string
	THEN
	2dup + 1- c@ ') = IF 1- THEN .struc ELSE drop 
    THEN ;

[ifdef] (s")
: c-c"
	Display? IF nl .name-without THEN
        count 2dup + aligned -rot
        Display?
        IF      bl cemit 0 .string
                [char] " cemit bl cemit
        ELSE    2drop
        THEN ;
[endif]

: c-string? ( addr1 -- addr2 f )
    \ f is true if a string was found and decompiled.
    \ if f is false, addr2=addr1
    \ recognizes the following patterns:
    \ c":     ahead X: len string then lit X
    \ flit:   ahead X: float      then lit X f@
    \ s\":    ahead X: string     then lit X lit len
    \ .\":    ahead X: string     then lit X lit len type
    \ !! not recognized anywhere:
    \ abort": if ahead X: len string then lit X c(abort") then
    dup @ back? if false exit endif
    dup @ >r
    r@ @ decompile-prim ['] lit xt>threaded <> if rdrop false exit endif
    r@ cell+ @ over cell+ <> if rdrop false exit endif
    \ we have at least C"
    r@ 2 cells + @ decompile-prim dup ['] lit xt>threaded = if
	drop r@ 3 cells + @ over cell+ + aligned r@ = if
	    \ we have at least s"
	    r@ 4 cells + @ decompile-prim ['] lit-perform xt>threaded =
	    r@ 5 cells + @ ['] type >body = and if
		6 s\" .\\\" "
	    else
		4 s\" s\\\" "
	    endif
	    \ !! make newline if string too long?
	    display? if
		0 .string r@ cell+ @ r@ 3 cells + @ c-\type '" cemit bl cemit
	    else
		2drop
	    endif
	    nip cells r> + true exit
	endif
    endif
    ['] f@ xt>threaded = if
	display? if
	    r@ cell+ @ f@ 10 8 16 f>str-rdp 0 .string bl cemit
	endif
	drop r> 3 cells + true exit
    endif
    \ !! check if count matches space?
    display? if
	s\" c\" " 0 .string r@ cell+ @ count 0 .string '" cemit bl cemit
    endif
    drop r> 2 cells + true ;

: Forward? ( a-addr true | false -- a-addr true | false )
    \ a-addr is pointer into branch table
    \ returns true when jump is a forward jump
    IF
	dup dup @ swap 1 cells - @ u> IF
	    true
	ELSE
	    drop false
	THEN
	\ only if forward jump
    ELSE
	false
    THEN ;

: RepeatCheck ( a-addr1 a-addr2 true | false -- false )
        IF  BEGIN  2dup
                   1 cells - @ swap @
                   u<=
            WHILE  drop dup cell+
                   MoreBranchAddr? 0=
            UNTIL  false
            ELSE   true
            THEN
        ELSE false
        THEN ;

: c-branch ( addr1 -- addr2 )
    c-string? ?exit
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
                ELSE    MyBranch cell+ @ LeaveCode =
			IF 	S" LEAVE " .struc
			ELSE
				dup cell+ BranchAddr? Forward?
       	                 	IF      dup cell+ @ WhileCode2 =
       	                         	IF nl S" ELSE" .struc level+
                                	ELSE level- nl S" ELSE" .struc level+ THEN
                                	cell+ Disable swap !
                        	ELSE    S" AHEAD" .struc level+
                        	THEN
			THEN
                THEN
        THEN
        Debug?
        IF      @ \ !!! cross-interacts with debugger !!!
        ELSE    cell+
        THEN ;

: DebugBranch
        Debug?
        IF      dup @ swap THEN ; \ return 2 different addresses

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
                                S" WHILE " .struc
                                level+
                        ELSE    MyBranch cell+ @ LeaveCode =
				IF   s" 0= ?LEAVE " .struc
				ELSE nl S" IF " .struc level+
				THEN
                        THEN
                THEN
        THEN
        DebugBranch
        cell+ ;

: c-for
        Display? IF nl S" FOR" .struc level+ THEN ;

: c-loop
        Display? IF level- nl .name-without nl bl cemit THEN
        DebugBranch cell+ 
	Scan? 
	IF 	dup BranchAddr? 
		BEGIN   WHILE cell+ LeaveCode swap !
			dup MoreBranchAddr?
		REPEAT
	THEN
	cell+ ;

: c-do
        Display? IF nl .name-without level+ THEN ;

: c-?do ( addr1 -- addr2 )
    Display? IF
	nl .name-without level+
    THEN
    DebugBranch cell+ ;

: c-exit ( addr1 -- addr2 )
    dup 1 cells -
    CheckEnd
    IF
	Display? IF nlflag off S" ;" Com# .string THEN
	C-Stop on
    ELSE
	Display? IF S" EXIT " .struc THEN
    THEN
    Debug? IF drop THEN ; \ !!! cross-interacts with debugger !!!

: c-abort"
        count 2dup + aligned -rot
        Display?
        IF      S" ABORT" .struc
                [char] " cemit bl cemit 0 .string
                [char] " cemit bl cemit
        ELSE    2drop
        THEN ;

[IFDEF] (does>)
: c-does>               \ end of create part
        Display? IF S" DOES> " Com# .string THEN
	maxaligned /does-handler + ;
[THEN]

[IFDEF] (compile)
: c-(compile)
    Display?
    IF
	s" POSTPONE " Com# .string
	dup @ look 0= ABORT" SEE: No valid XT"
	name>string 0 .string bl cemit
    THEN
    cell+ ;
[THEN]

CREATE C-Table
	        ' lit A,            ' c-lit A,
		' does-exec A,	    ' c-callxt A,
		' lit@ A,	    ' c-call A,
[IFDEF] call	' call A,           ' c-call A, [THEN]
\		' useraddr A,	    ....
		' lit-perform A,    ' c-call A,
		' lit+ A,	    ' c-lit+ A,
[IFDEF] (s")	' (s") A,	    ' c-c" A, [THEN]
[IFDEF] (.")	' (.") A,	    ' c-c" A, [THEN]
[IFDEF] "lit    ' "lit A,           ' c-c" A, [THEN]
[IFDEF] (c")	' (c") A,	    ' c-c" A, [THEN]
        	' (do) A,           ' c-do A,
[IFDEF] (+do)	' (+do) A,	    ' c-?do A, [THEN]
[IFDEF] (u+do)	' (u+do) A,	    ' c-?do A, [THEN]
[IFDEF] (-do)	' (-do) A,	    ' c-?do A, [THEN]
[IFDEF] (u-do)	' (u-do) A,	    ' c-?do A, [THEN]
        	' (?do) A,          ' c-?do A,
        	' (for) A,          ' c-for A,
        	' ?branch A,        ' c-?branch A,
        	' branch A,         ' c-branch A,
        	' (loop) A,         ' c-loop A,
        	' (+loop) A,        ' c-loop A,
[IFDEF] (s+loop) ' (s+loop) A,      ' c-loop A, [THEN]
[IFDEF] (-loop) ' (-loop) A,        ' c-loop A, [THEN]
        	' (next) A,         ' c-loop A,
        	' ;s A,             ' c-exit A,
[IFDEF] (abort") ' (abort") A,      ' c-abort" A, [THEN]
\ only defined if compiler is loaded
[IFDEF] (compile) ' (compile) A,      ' c-(compile) A, [THEN]
[IFDEF] (does>) ' (does>) A,        ' c-does> A, [THEN]
        	0 ,		here 0 ,

avariable c-extender
c-extender !

\ DOTABLE                                               15may93jaw

: DoTable ( ca/cfa -- flag )
    decompile-prim C-Table BEGIN ( cfa table-entry )
	dup @ dup 0=  IF
	    drop cell+ @ dup IF ( next table!)
		dup @
	    ELSE ( end!)
		2drop false EXIT
	    THEN 
	THEN
	\ jump over to extender, if any 26jan97jaw
	xt>threaded 2 pick <>
    WHILE
	    2 cells +
    REPEAT
    nip cell+ perform
    true
;

: BranchTo? ( a-addr -- a-addr )
        Display?  IF    dup BranchAddr?
                        IF
				BEGIN cell+ @ dup 20 u>
                                IF drop nl S" BEGIN " .struc level+
                                ELSE
                                  dup Disable <> over LeaveCode <> and
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
    IF
	.word bl cemit
    ELSE
	drop
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

Defer xt-see-xt ( xt -- )
\ this one is just a forward declaration for indirect recursion

: .defname ( xt c-addr u -- )
    rot look
    if ( c-addr u nfa )
	-rot type space .name
    else
	drop ." noname " type
    then
    space ;

Defer discode ( addr u -- ) \ gforth
\G hook for the disassembler: disassemble code at addr of length u
' dump IS discode

: next-head ( addr1 -- addr2 ) \ gforth
    \G find the next header starting after addr1, up to here (unreliable).
    here swap u+do
	i head? -2 and if
	    i unloop exit
	then
    cell +loop
    here ;

[ifundef] umin \ !! bootstrapping help
: umin ( u1 u2 -- u )
    2dup u>
    if
	swap
    then
    drop ;
[then]

: next-prim ( addr1 -- addr2 ) \ gforth
    \G find the next primitive after addr1 (unreliable)
    1+ >r -1 primstart
    begin ( umin head R: boundary )
	@ dup
    while
	tuck name>int >code-address ( head1 umin ca R: boundary )
	r@ - umin
	swap
    repeat
    drop dup r@ negate u>=
    \ "umin+boundary within [0,boundary)" = "umin within [-boundary,0)"
    if ( umin R: boundary ) \ no primitive found behind -> use a default length
	drop 31
    then
    r> + ;

: seecode ( xt -- )
    dup s" Code" .defname
    >code-address
    dup in-dictionary? \ user-defined code word?
    if
	dup next-head
    else
	dup next-prim
    then
    over - discode
    ." end-code" cr ;
: seevar ( xt -- )
    s" Variable" .defname cr ;
: seeuser ( xt -- )
    s" User" .defname cr ;
: seecon ( xt -- )
    dup >body ?
    s" Constant" .defname cr ;
: seevalue ( xt -- )
    dup >body ?
    s" Value" .defname cr ;
: seedefer ( xt -- )
    dup >body @ xt-see-xt cr
    dup s" Defer" .defname cr
    >name ?dup-if
	." IS " .name cr
    else
	." latestxt >body !"
    then ;
: see-threaded ( addr -- )
    C-Pass @ DebugMode = IF
	ScanMode c-pass !
	EXIT
    THEN
    ScanMode c-pass ! dup makepass
    DisplayMode c-pass ! makepass ;
: seedoes ( xt -- )
    dup s" create" .defname cr
    S" DOES> " Com# .string XPos @ Level !
    >does-code see-threaded ;
: seecol ( xt -- )
    dup s" :" .defname nl
    2 Level !
    >body see-threaded ;
: seefield ( xt -- )
    dup >body ." 0 " ? ." 0 0 "
    s" Field" .defname cr ;

: xt-see ( xt -- ) \ gforth
    \G Decompile the definition represented by @i{xt}.
    cr c-init
    dup >does-code
    if
	seedoes EXIT
    then
    dup xtprim?
    if
	seecode EXIT
    then
    dup >code-address
    CASE
	docon: of seecon endof
[IFDEF] dovalue:
        dovalue: of seevalue endof
[THEN]
	docol: of seecol endof
	dovar: of seevar endof
[IFDEF] douser:
	douser: of seeuser endof
[THEN]
[IFDEF] dodefer:
	dodefer: of seedefer endof
[THEN]
[IFDEF] dofield:
	dofield: of seefield endof
[THEN]
	over       of seecode endof \ direct threaded code words
	over >body of seecode endof \ indirect threaded code words
	2drop abort" unknown word type"
    ENDCASE ;

: (xt-see-xt) ( xt -- )
    xt-see cr ." latestxt" ;
' (xt-see-xt) is xt-see-xt

: (.immediate) ( xt -- )
    ['] execute = if
	."  immediate"
    then ;

: name-see ( nfa -- )
    dup name>int >r
    dup name>comp 
    over r@ =
    if \ normal or immediate word
	swap xt-see (.immediate)
    else
	r@ ['] ticking-compile-only-error =
	if \ compile-only word
	    swap xt-see (.immediate) ."  compile-only"
	else \ interpret/compile word
	    r@ xt-see-xt cr
	    swap xt-see-xt cr
	    ." interpret/compile: " over .name drop
	then
    then
    rdrop drop ;

: see ( "<spaces>name" -- ) \ tools
    \G Locate @var{name} using the current search order. Display the
    \G definition of @var{name}. Since this is achieved by decompiling
    \G the definition, the formatting is mechanised and some source
    \G information (comments, interpreted sequences within definitions
    \G etc.) is lost.
    name find-name dup 0=
    IF
	drop -&13 throw
    THEN
    name-see ;


