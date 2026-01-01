\ SEE.FS       highend SEE for ANSforth                16may93jaw

\ Authors: Bernd Paysan, Anton Ertl, David Kühling, Jens Wilke, Neal Crook
\ Copyright (C) 1995,2000,2003,2004,2006,2007,2008,2010,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024,2025 Free Software Foundation, Inc.

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
require wordinfo.fs

decimal

Vocabulary see-voc

get-current also see-voc definitions

\ Screen format words                                   16may93jaw

VARIABLE C-Output   1 C-Output  !
VARIABLE C-Formated 1 C-Formated !
VARIABLE C-Highlight 0 C-Highlight !
VARIABLE C-Clearline 0 C-Clearline !

VARIABLE XPos
VARIABLE YPos
VARIABLE Level

    
: Format ( u -- )
    C-Formated @ C-Output @ and if ( u )
	dup spaces dup XPos +! then
    drop ;


: level+ ( -- )
    7 Level +!
    Level @ XPos @ - dup 0> IF Format ELSE drop THEN ;

: level- ( -- )
    -7 Level +! ;

VARIABLE nlflag
VARIABLE uppercase	\ structure words are in uppercase

DEFER nlcount ( -- )
' noop IS nlcount

: nl ( -- )
    nlflag on ;
: (nl) ( -- )
    nlcount
    XPos @ Level @ = IF EXIT THEN \ ?Exit
    C-Formated @ IF
	C-Output @ IF
	    C-Clearline @ IF
		cols XPos @ - spaces
	    ELSE
		cr THEN
	    1 YPos +! 0 XPos !
	    Level @ spaces THEN
	Level @ XPos ! THEN ;

: warp?         ( len -- len )
                nlflag @ IF (nl) nlflag off THEN
                XPos @ over + cols u>= IF (nl) THEN ;

: ctype         ( adr len -- )
                warp? dup XPos +! C-Output @ 
		IF uppercase @ IF bounds ?DO i c@ toupper emit LOOP
				  uppercase off ELSE type THEN
		ELSE 2drop THEN ;

: cemit ( c -- )
    1 warp?
    over bl = Level @ XPos @ = and IF
	2drop
    ELSE
	XPos +! C-Output @ IF emit ELSE drop THEN
    THEN ;

	    
Defer xt-see-xt ( xt -- )
\ this one is just a forward declaration for indirect recursion

: .defname ( xt c-addr u -- )
    rot look
    if ( c-addr u nfa )
	-rot type space id.
    else
	drop ." noname " type
    then
    space ;

dup set-current

Defer discode ( addr u -- ) \ gforth
\G hook for the disassembler: disassemble u bytes of code at addr
' dump IS discode

definitions

: (next-head) ( addr1 addr2 -- addr )
    tuck >r u+do
	i xt? if
	    i dup >cfa swap name>string drop cell negate and dup 0= select
	    unloop rdrop exit
	then
    cell +loop
    r> ;

: next-head ( addr1 -- addr2 ) \ gforth-internal
    \G find the next header starting after addr1, up to here (unreliable).
    [ cell body> ] Literal +
    dup which-section? ?dup-IF
	[: section-dp @ (next-head) ;] swap section-execute
    ELSE
	here (next-head)
    THEN ;

: next-prim ( addr1 -- addr2 ) \ gforth-internal
    \G find the next primitive after addr1 (unreliable)
    1+ >r -1 primstart
    begin ( umin head R: boundary )
	>link @ dup
    while
	tuck ( name>interpret ) >code-address ( head1 umin ca R: boundary )
	r@ - umin
	swap
    repeat
    drop dup r@ negate u>=
    \ "umin+boundary within [0,boundary)" = "umin within [-boundary,0)"
    if ( umin R: boundary ) \ no primitive found behind -> use a default length
	drop 31
    then
    r> + ;

DEFER .string ( c-addr u xt -- )

: (.string)     ( c-addr u xt -- )
    [: execute ctype ;] execute-theme-color ;

' (.string) IS .string

: c-\emit ( c -- )
    \ show char in \-escaped form; note that newlines can have
    \ two chars, so they need to be handled at the string level.
    dup '" = over '\ = or if
	'\ cemit cemit
    else
	dup bl 127 within if
	    cemit
	else
	    base @ { oldbase } try
		$10 base ! 0 <<# # # 'x' hold '\' hold #> ctype #>> 0
	    restore
		oldbase base !
	    endtry
	    throw
	endif
    endif ;

: c-\type ( c-addr u -- )
    \ type string in \-escaped form
    begin
	dup while
	    2dup newline string-prefix? if
		'\ cemit 'n cemit
		newline nip /string
	    else
		over c@ c-\emit 1 /string
	    endif
    repeat
    2drop ;

\ DEFER CheckUntil
VARIABLE NoOutput
VARIABLE C-Pass

0 CONSTANT ScanMode
1 CONSTANT DisplayMode
2 CONSTANT DebugMode

: Scan? ( -- flag ) C-Pass @ 0= ;
: Display? ( -- flag ) C-Pass @ 1 = ;
: Debug? ( -- flag ) C-Pass @ 2 = ;
: ?.string  ( c-addr u xt -- )   Display? if .string else 2drop drop then ;

Defer see-threaded

\ The branchtable consists of three entrys:
\ address of branch , branch destination , branch type

CREATE BranchTable 128 cells allot
here 3 cells -
ACONSTANT MaxTable
VARIABLE BranchPointer	\ point to the end of branch table
VARIABLE SearchPointer
VARIABLE C-Stop

\ try see quotations, but so far fails, because can't reenter see-threaded

: smart.quotation. ( n depth -- t / n f )
    drop dup xt? IF
	dup name>string d0= IF
	    dup >code-address docol: = IF
		s" [: " ['] Com-color .string
		BranchPointer @ BranchTable
		{ bp SaveTable[ 128 cells ] }
		2 Level +! >body see-threaded  -2 Level +!
		SaveTable[ BranchTable 128 cells move
		bp BranchPointer !  C-Stop off
		s" ] " ['] Com-color .string
		true EXIT  THEN  THEN  THEN
    false ;

: c-lits ( -- )
    display? IF
	sp@ >r  smart.s-skip off
	['] smart.quotation. smart<> >back
	litstack get-stack  litstack $free
	dup 0 ?DO  dup I - pick ['] smart.s. $tmp ctype  LOOP  drop
	smart<> back> drop  r> sp!
    ELSE
	litstack $free
    THEN ;

Variable struct-pre
: .struc ( c-addr u -- )       
    c-lits uppercase on ['] Str-color
    struct-pre $@ ['] Str-color .string
    .string struct-pre $free ;

\ CODES (Branchtypes)                                    15may93jaw

21 Constant RepeatCode
22 Constant AgainCode
23 Constant UntilCode
24 Constant LoopCode
\ 09 CONSTANT WhileCode
10 Constant ElseCode
11 Constant AheadCode
13 Constant WhileCode2
14 Constant Disable
15 Constant LeaveCode


\ FORMAT WORDS                                          13jun93jaw

VARIABLE Branches

: FirstBranch ( -- )
    BranchTable cell+ SearchPointer ! ;

: (BranchAddr?) ( a-addr1 -- a-addr2 true | false )
    \ searches a branch with destination a-addr1
    \ a-addr1: branch destination
    \ a-addr2: pointer in branch table
    SearchPointer @ BEGIN
	dup BranchPointer @ u< WHILE
	    dup @ third <> WHILE
		3 th
	REPEAT
	nip dup  3 th SearchPointer ! true
    ELSE
	2drop false THEN ;

: BranchAddr? ( a-addr1 -- a-addr2 true | false )
        FirstBranch (BranchAddr?) ;

' (BranchAddr?) ALIAS MoreBranchAddr? ( a-addr1 -- a-addr2 true | false )

: CheckEnd ( a-addr -- true | false )
    BranchTable cell+ BEGIN
	dup BranchPointer @ u< WHILE
	    dup @ third u<= WHILE
		3 th REPEAT
	2drop false
    ELSE
	2drop true THEN ;

?: cell- ( addr1 -- addr2 ) cell - ;
    
: MyBranch      ( a-addr -- a-addr a-addr2 )
\ finds branch table entry for branch at a-addr
    dup @ BranchAddr? BEGIN
    WHILE
	    cell- @ over <> WHILE
		dup @ MoreBranchAddr? REPEAT
	SearchPointer @ 3 cells -
    ELSE
	true ABORT" SEE: Table failure" THEN ;

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
    BranchTable >r BEGIN
	r@ BranchPointer @ u< WHILE
	    2dup r@ @ within IF
		over r@ cell+ @ u> IF
		    2drop rdrop true EXIT THEN
	    THEN
	    r> 3 th >r REPEAT
    2drop rdrop false ;

: ,Branch ( a-addr -- )
        BranchPointer @ dup MaxTable u> ABORT" SEE: Table overflow"
        !
        1 cells BranchPointer +! ;

: Type!   ( u -- )
        BranchPointer @ cell- ! ;

: Branch! ( a-addr rel -- a-addr )
    over ,Branch ,Branch 0 ,Branch ;
\        over + over ,Branch ,Branch 0 ,Branch ;

: back? ( addr target -- addr flag )
    over u< ;

: .word1 ( addr x -- addr )
    \ print x as a word if possible; for primitives, x must be fetched
    \ with @decompile-prim
    dup look 0= IF
        drop dup threaded>name dup 0= if
	    drop over cell- @ dup body> look IF
		nip nip dup ." <" name>string rot wordinfo .string ." > "
	    ELSE
		2drop smart.
	    THEN
	    EXIT
	then
    THEN
    nip dup immediate? IF
	bl cemit  ." [COMPILE] "
    THEN
    dup name>string rot wordinfo .string
    ;

: c-call ( addr1 -- addr2 )
    Display? IF
	dup @ body> .word1 bl cemit
    THEN
    cell+ ;

: c-callxt ( addr1 -- addr2 )
    Display? IF
	dup @ .word1 bl cemit
    THEN
    cell+ ;

\ here docon: , docol: , dovar: , douser: , dodefer: , dofield: ,
\ here over - 2constant doers

[IFDEF] !does
    : c-does> ( -- )
	\ end of create part
	Display? IF S" DOES> " ['] Com-color .string THEN ;
\	maxaligned /does-handler + ; \ !! no longer needed for non-cross stuff
[THEN]

: c># ( n -- addr u ) `smart. $tmp ;
: c-. ( n -- ) c># ['] default-color .string ;

: c>lit ( addr1 -- addr2 )
    dup @ >lits cell+ ;
: c-lit ( addr1 -- addr2 )
    dup @ dup body> dup cfaligned over = swap in-dictionary? and if
	( addr1 addr1@ )
	dup body> >code-address dovar: = if
	    drop c-call EXIT
	endif
    endif
    over 4 th over = if
	over 1 th @decompile-prim ['] call xt= >r
	over 3 th @decompile-prim ['] ;S xt=
	r> and if
	    over 2 th@ ['] set-does> >body = if  drop
		S" DOES> " ['] Com-color ?.string 4 th EXIT endif
	endif
	[IFDEF] !;abi-code
	    over 2 th@ ['] !;abi-code >body = if  drop
		S" ;abi-code " ['] Com-color ?.string   4 th
		c-stop on
		Display? if
		    dup   dup  next-head   over - discode 
		    S" end-code" ['] Com-color ?.string 
		then   EXIT
	    endif
	[THEN]
    endif
    Display? if ( addr1 addr1@ )
	dup c-. then
    drop cell+ ;

: c-lit+ ( addr1 -- addr2 )
    Display? if
	dup @ c-.
	s" + " ['] default-color .string
    endif
    cell+ ;

: c-lit@ ( addr1 -- addr2 )
    Display? if
	dup @
	dup body> xt? IF
	    dup body> >code-address dovalue: = IF
		dup body> name>string dup IF
		    ['] val-color .string space  drop cell+  EXIT
		ELSE  2drop  THEN  THEN  THEN
	c-. s" @ " ['] default-color .string
    then
    cell+ ;

: id.-without ( addr -- addr )
    \ !! the stack effect cannot be correct
    \ prints a name without () and without -LP+!#, e.g. a (+LOOP) or (s")
    dup cell- @threaded>name dup IF
	dup ``(/loop) = over ``(/loop)-lp+!# = or if drop ``+loop then
	name>string over c@ '( = IF
	    1 /string
	THEN
	2dup "-lp+!#" string-suffix? if 6 - then
	2dup + 1- c@ ') = IF 1- THEN
	.struc
    ELSE
	drop THEN ;

: c-c"
	Display? IF nl id.-without THEN
        count 2dup + aligned -rot
        Display?
        IF      bl cemit ['] default-color .string
                '"' cemit bl cemit
        ELSE    2drop
        THEN ;

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
    dup @ dup
    >r @decompile-prim ['] lit xt= 0= if rdrop false exit endif
    r@ cell+ @ over cell+ <> if rdrop false exit endif
    \ we have at least C"
    r@ 2 th @decompile-prim dup ['] lit xt= if
	drop r@ 3 th@ over cell+ + aligned r@ = if
	    \ we have at least s"
	    r@ 4 th @decompile-prim ['] lit-perform xt=
	    r@ 5 th@ ['] type >body = and if
		6 s\" .\\\" "
	    else
		4 s\" s\\\" "
	    endif
	    \ !! make newline if string too long?
	    display? if
		['] default-color .string r@ cell+ @ r@ 3 th@ c-\type '" cemit bl cemit
	    else
		2drop
	    endif
	    nip cells r> + true exit
	endif
    endif
    ['] f@ xt= if
	display? if
	    r@ cell+ @ f@ 10 8 16 f>str-rdp ['] default-color .string bl cemit
	endif
	drop r> 3 th true exit
    endif
    \ !! check if count matches space?
    display? if
	s\" c\" " ['] default-color .string r@ cell+ @ count ['] default-color .string '" cemit bl cemit
    endif
    drop r> 2 th true ;

: Forward? ( a-addr true | false -- a-addr true | false )
    \ a-addr is pointer into branch table
    \ returns true when jump is a forward jump
    IF
	dup dup @ swap cell- @ u> IF
	    true
	ELSE
	    drop false
	THEN
	\ only if forward jump
    ELSE
	false
    THEN ;

: RepeatCheck ( a-addr1 a-addr2 true | false -- false )
    IF
	BEGIN
	    2dup cell- @ swap @ u<= WHILE
		drop dup cell+ MoreBranchAddr? 0=
	    UNTIL
	    false
	ELSE
	    true THEN
    ELSE
	false THEN ;

: c-branch ( addr1 -- addr2 )
    c-string? ?exit Scan? IF
	dup @ Branch! dup @ back? IF
	    \ might be: AGAIN, REPEAT
	    dup cell+ BranchAddr? Forward? RepeatCheck IF
		RepeatCode Type! cell+ Disable swap !
	    ELSE
		AgainCode Type! THEN
	ELSE
	    dup cell+ BranchAddr? Forward? IF
		ElseCode Type! drop
	    ELSE
		AheadCode Type! THEN
	THEN
    THEN
    Display? IF
	dup @ back? IF
	    \ might be: AGAIN, REPEAT
	    level- nl dup cell+ BranchAddr? Forward? RepeatCheck IF
		drop S" REPEAT " .struc nl
	    ELSE
		S" AGAIN " .struc nl THEN
	ELSE
	    MyBranch cell+ @ LeaveCode = IF
		S" LEAVE " .struc
	    ELSE
		dup cell+ BranchAddr? Forward? IF
		    dup cell+ @ WhileCode2 = IF
			nl S" ELSE " .struc level+
		    ELSE
			level- nl S" ELSE" .struc level+ THEN
		    cell+ Disable swap !
		ELSE
		    S" AHEAD " .struc level+ THEN
	    THEN
	THEN
    THEN
    Debug? IF
	@ \ !!! cross-interacts with debugger !!!
    ELSE
	cell+ THEN ;

: DebugBranch ( addr -- x addr | addr )
    \ !! reconstructed stack effect, code looks broken
    \ should probably be ( addr -- addr )
    Debug? IF
	dup @ swap THEN ; \ return 2 different addresses

: c-?branch ( addr -- addr2 )
    Scan? IF
	dup @ Branch!  dup @ Back? IF ( addr )
	    UntilCode Type! THEN
    THEN ( addr )
    Display? IF
	dup @ Back? IF ( addr )
	    level- nl S" UNTIL " .struc nl
	ELSE
	    dup dup @ CheckWhile IF ( addr )
		MyBranch cell+ dup @ 0= IF ( addr addr2 )
		    WhileCode2 swap !
		ELSE
		    drop THEN ( addr )
		level- nl  S" WHILE " .struc  level+
	    ELSE ( addr )
		MyBranch cell+ @ LeaveCode = IF ( addr )
		    s" 0= ?LEAVE " .struc
		ELSE
		    nl S" IF " .struc level+ THEN
	    THEN
	THEN ( addr )
    THEN ( addr )
    DebugBranch cell+ ;

: c-?dup-?branch ( addr -- addr2 )
    Scan? 0= IF
	s" ?dup-" struct-pre $!  THEN
    c-?branch ;
    
: c-for ( -- )
    Display? IF
	nl S" FOR" .struc level+ THEN ;

: c-loop ( addr -- addr1 )
    scan? IF
	dup @ Branch!  LoopCode Type! THEN
    Display? IF
	level- nl id.-without nl bl cemit THEN
    DebugBranch cell+  Scan? IF
	dup BranchAddr? BEGIN ( addr1 addr2 f )
	WHILE ( addr1 addr2 )
		cell+ LeaveCode swap ! dup MoreBranchAddr? REPEAT 
    THEN ( addr1 ) \ perverse stack effect of MoreBranchAddr?
    cell+ ;

: c-do ( addr -- addr )
    Display? IF
	dup BranchAddr? IF ( addr addr1 )
	    cell+ dup @ LoopCode = IF ( addr addr2 )
		Disable swap !	nl id.-without level+
	    ELSE
		drop ." 2>r "  THEN ( addr )
	ELSE ( addr ) \ perverse stack effect of ?BranchAddr
	    ." 2>r "  THEN
    THEN ;

: c-?do ( addr1 -- addr2 )
    Display? IF
	nl id.-without level+
	dup cell+ BranchAddr?  IF  Disable swap !  THEN
    THEN
    DebugBranch cell+ ;

: c-exit ( addr1 -- addr2 )
    dup cell- CheckEnd IF ( addr1 )
	Display? IF
	    nlflag off S" ;" ['] Com-color .string THEN
	C-Stop on
    ELSE
	Display? IF S" EXIT " .struc THEN
    THEN
    Debug? IF drop THEN ( !! unbalanced! ) ; \ !!! cross-interacts with debugger !!!

: c-abort" ( c-addr -- c-addr-end )
    count 2dup + aligned -rot Display? IF (  c-addr-end c-addr1 u )
	S" ABORT" .struc
	'"' cemit bl cemit ['] default-color .string
	'"' cemit bl cemit
    ELSE
	2drop THEN ;

[IFDEF] (compile)
: c-(compile) ( addr -- )
    Display? IF
	s" POSTPONE " ['] Com-color .string
	dup @ look 0= ABORT" SEE: No valid XT"
	name>string ['] default-color .string bl cemit
    THEN
    cell+ ;
[THEN]

[IFDEF] u#exec
    Variable u#what \ global variable to specify what to search for
    : search-u#gen ( 0 offset1 offset2 nt -- xt/0 offset1 offset2 flag )
	dup >code-address docol: = IF
	    dup >body @decompile-prim u#what @ xt=
	    over >body 3 th @decompile-prim ['] ;S xt= and
	    IF  >r 2dup r@ >body cell+ 2@ d=
		IF  r> -rot 2>r nip 2r> false  EXIT  THEN
		r>
	    THEN
	THEN  drop true ;
    : c-u#gen ( addr -- addr' )
	display? IF
	    0 over 2@
	    [: ['] search-u#gen swap traverse-wordlist ;] map-vocs
	    2drop
	    ?dup-IF
		>name name>string ['] Com-color .string bl cemit
		2 th EXIT  THEN
	    u#what @ name>string ['] Com-color .string bl cemit
	    dup @ c-. cell+ dup @ c-. cell+
	ELSE  2 th  THEN ;

    : c-u#exec ( addr -- addr' )  ['] u#exec u#what ! c-u#gen ;
    : c-u#+    ( addr -- addr' )  ['] u#+    u#what ! c-u#gen ;
[THEN]

[IFDEF] call-c#
    : c-call-c# ( addr -- addr' )
	display? IF
	    dup @ body> name>string ['] Com-color .string bl cemit
	THEN  cell+ ;
[THEN]

[DEFINED] useraddr [DEFINED] up@ or [IF]
    : ?type-found ( offset nt flag -- offset flag' )
	IF  2dup >body @ = IF  -rot nip false  EXIT
	    THEN  THEN  drop true ;
    : search-uservar ( offset nt -- offset flag )
	( name>interpret ) dup >code-address douser: = ?type-found ;
    : c-searcharg ( addr xt addr u -- addr' ) 2>r >r
	display? IF
	    0 over @
	    r@ map-vocs drop
	    display? IF
		?dup-IF  name>string ['] Com-color .string bl cemit
		ELSE  r> 2r@ ['] Com-color .string >r
		    dup @ c-. bl cemit
		THEN
	    THEN
	THEN  cell+ rdrop rdrop rdrop ;
    : c-useraddr ( addr -- addr' )
	[: ['] search-uservar swap traverse-wordlist ;]
	s" useraddr " c-searcharg ;
[THEN]
[IFDEF] up@
    : c-up@ ( addr -- addr' )
	dup @decompile-prim ['] lit+ xt= IF
	    cell+ c-useraddr
	ELSE
	    display? IF
		s" up@ " ['] default-color .string
	    THEN
	THEN  ;
[THEN]
[IFDEF] user@
    : search-userval ( offset nt -- offset flag )
	( name>interpret ) dup >does-code ['] infile-id >does-code = ?type-found ;
    : c-user@ ( addr -- addr' )
	[: ['] search-userval swap traverse-wordlist ;]
	s" user@ " c-searcharg ;
[THEN]

CREATE C-Table \ primitives map to code only
	        ' lit A,            ' c-lit A,
[IFDEF] does-exec ' does-exec A,	    ' c-callxt A, [THEN]
[IFDEF] does-xt ' does-xt A,        ' c-callxt A, [THEN]
[IFDEF] extra-exec ' extra-exec A,	    ' c-callxt A, [THEN]
[IFDEF] extra-xt ' extra-xt A,	    ' c-callxt A, [THEN]
[IFDEF] lit@	' lit@ A,	    ' c-lit@ A, [THEN]
[IFDEF] call	' call A,           ' c-call A, [THEN]
[IFDEF] call-loc ' call-loc A,      ' c-call A, [THEN]
\		' useraddr A,	    ....
		' lit-perform A,    ' c-call A,
		' lit+ A,	    ' c-lit+ A,
\ [IFDEF] (s")	' (s") A,	    ' c-c" A, [THEN]
\ [IFDEF] (.")	' (.") A,	    ' c-c" A, [THEN]
\ [IFDEF] "lit    ' "lit A,           ' c-c" A, [THEN]
\ [IFDEF] (c")	' (c") A,	    ' c-c" A, [THEN]
        	' (do) A,           ' c-do A,
[IFDEF] (+do)	' (+do) A,	    ' c-?do A, [THEN]
[IFDEF] (u+do)	' (u+do) A,	    ' c-?do A, [THEN]
[IFDEF] (-do)	' (-do) A,	    ' c-?do A, [THEN]
[IFDEF] (u-do)	' (u-do) A,	    ' c-?do A, [THEN]
[IFDEF] (-[do)  ' (-[do) A,         ' c-?do A, [THEN]
[IFDEF] (u-[do) ' (u-[do) A,        ' c-?do A, [THEN]
        	' (?do) A,          ' c-?do A,
        	' (for) A,          ' c-for A,
        	' ?branch A,        ' c-?branch A,
        	' ?dup-?branch A,   ' c-?dup-?branch A,
        	' branch A,         ' c-branch A,
        	' (loop) A,         ' c-loop A,
        	' (+loop) A,        ' c-loop A,
[IFDEF] (s+loop) ' (s+loop) A,      ' c-loop A, [THEN]
[IFDEF] (-loop) ' (-loop) A,        ' c-loop A, [THEN]
[IFDEF] (/loop) ' (/loop) A,        ' c-loop A, [THEN]
        	' (next) A,         ' c-loop A,
        	' ;s A,             ' c-exit A,
\ [IFDEF] (abort") ' (abort") A,      ' c-abort" A, [THEN]
\ only defined if compiler is loaded
\ [IFDEF] (compile) ' (compile) A,      ' c-(compile) A, [THEN]
[IFDEF] u#exec  ' u#exec A,         ' c-u#exec A, [THEN]
[IFDEF] u#+     ' u#+ A,            ' c-u#+ A, [THEN]
[IFDEF] call-c# ' call-c# A,        ' c-call-c# A, [THEN]
[IFDEF] useraddr ' useraddr A,      ' c-useraddr A, [THEN]
[IFDEF] user@    ' user@ A,         ' c-user@ A, [THEN]
[IFDEF] up@     ' up@ A,            ' c-up@ A, [THEN]
        	0 ,		here 0 ,

avariable c-extender
c-extender !

\ DOTABLE                                               15may93jaw

: DoTable ( ca/cfa -- flag )
    C-Output @ IF
	dup ['] lit xt= IF  drop c>lit true  EXIT  THEN
    THEN
    c-lits
    C-Table BEGIN ( cfa table-entry )
	dup @ dup 0=  IF
	    drop cell+ @ dup IF ( next table!)
		dup @
	    ELSE ( end!)
		2drop false EXIT
	    THEN 
	THEN
	\ jump over to extender, if any 26jan97jaw
	third swap xt= 0=
    WHILE
	    2 th
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
		    dup Disable <>
		    over LeaveCode <> and
		    over LoopCode <> and
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
    dup cell+ swap @decompile-prim
    dup >r DoTable IF rdrop EXIT THEN
    r> Display?
    IF
	.word1 bl cemit
    ELSE
	drop
    THEN ;

: c-init ( -- )
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

\ user words

: seecode ( xt -- )
    dup s" Code" .defname
    >code-address
    dup in-dictionary? \ user-defined code word?
    if
	dup next-head
    else
	dup next-prim
    then
    threading-method 2 = IF  @ >r @ r>  THEN
    over - discode
    ." end-code" cr ;
: seeabicode ( xt -- )
    dup s" ABI-Code" .defname
    >body dup dup next-head 
    swap - discode
    ." end-code" cr ;
: see;abicode ( xt -- )
    dup s" ;ABI-Code" .defname
    >body dup dup next-head 
    swap - discode
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
	." IS " id. cr
    else
	." latestxt >body !"
    then ;
:is see-threaded ( addr -- )
    C-Pass @ DebugMode = IF
	ScanMode c-pass !
	EXIT
    THEN
    ScanMode c-pass ! dup makepass
    DisplayMode c-pass ! makepass ;
: seedoes ( xt -- )
    \ !! make it work for general xt set-does> words
    dup s" create" .defname cr
    s" DOES> " ['] Com-color .string XPos @ Level !
    >does-code see-threaded ;
: seecol ( xt -- )
    dup s" :" .defname nl
    2 Level !
    >body see-threaded ;
: seefield ( xt -- )
    dup >body ." 0 " ? ." 0 0 "
    s" Field" .defname cr ;
: seeumethod ( xt -- )
    dup s" umethod" .defname cr
    dup defer@ xt-see-xt cr
    >name ?dup-if
	." IS " id. cr
    else
	." latestxt >body !"
    then ;
: umethod? ( xt -- flag )
    >body dup @decompile-prim ['] u#exec xt= swap
    3 th @decompile-prim ['] ;S xt= and ;

\ user visible words

set-current

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
        docol: of dup umethod? IF  seeumethod  ELSE  seecol  THEN  endof
[IFDEF] docolloc:
	docolloc: of  seecol  endof
[THEN]
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
[IFDEF] doabicode:
        doabicode: of seeabicode endof
[THEN]
[IFDEF] do;abicode:
        do;abicode: of see;abicode endof
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
: (.compile-only) ( nt -- )
    compile-only? IF  ."  compile-only"  THEN ;

: name-see ( nfa -- )
    dup synonym? IF
	." Synonym " dup id. dup >body @ id.
    ELSE
	dup alias? IF
	    dup >body @ name>string nip 0= IF
		dup >body @ h.
	    ELSE
		." ' " dup >body @ id.
	    THEN ." Alias " dup id.
	THEN
    THEN
    dup >r
    dup name>compile 
    over r@ name>interpret =
    if \ normal or immediate word
	swap xt-see (.immediate)
	r@ (.compile-only)
    else
	\ interpret/compile word
	r@ name>interpret xt-see-xt cr
	swap xt-see-xt cr
	." interpret/compile: " r@ id. drop
    then
    rdrop drop ;

: see ( "<spaces>name" -- ) \ tools
    \G Locate @var{name} using the current search order. Display the
    \G definition of @var{name}. Since this is achieved by decompiling
    \G the definition, the formatting is mechanised and some source
    \G information (comments, interpreted sequences within definitions
    \G etc.) is lost.
    view' name-see ;

previous
