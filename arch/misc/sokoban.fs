\ sokoban - a maze game in FORTH

\ Copyright (C) 1995,1997,1998,2000,2003,2004,2007 Free Software Foundation, Inc.

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

\ Contest from Rick VanNorman in comp.lang.forth

\ SOKOBAN

\ Sokoban is a visual game of pushing.  You (Soko) are represented by the
\ at-sign "@"  You may move freely through the maze on unoccupied spaces.
\ The dollar-signs "$" are the rocks you have to push. You can only push
\ one rock at a time, and cannot push a rock through a wall "#" or over
\ another rock. The object is to push the rocks to their goals which are
\ indicated by the periods ".".  There are 50 levels, the first of which
\ is shown below.

\ program is ANS FORTH with environmental dependency of case-insensitiv
\ source. Tested with gforth, bigFORTH and pfe

\ bell (7) is replaced with "Wuff!" ;-)
\ (this is a german joke)
\ I don't like the keyboard interpreting CASE-statement either, but
\ was to lazy to use a table.
\ I could have used blocks as level tables, but as I don't have a good
\ block editor for gforth now, I let it be.

Create pn-tab ," 000102030405060708091011121314151617181920212223242526272829303132333435363738394041424344454647484950515253545556575859606162636465666768697071727374757677787980"

: pn    ( n -- )  2* pn-tab 1+ + 2 type ;
: ;pn   [char] ; emit pn ;
: ESC[  &27 emit [char] [ emit ;
: at-xy 1+ swap 1+ swap ESC[ pn ;pn [char] H emit ;
: page  ESC[ ." 2J" 0 0 at-xy ;

40 Constant /maze  \ maximal maze line

Create maze  1 cells allot /maze 25 * allot  \ current maze
Variable mazes   0 mazes !  \ root pointer
Variable soko    0 soko !   \ player position
Variable >maze   0 >maze !  \ current compiled maze

\ score information

Variable rocks     0 rocks !  \ number of rocks left
Variable level#    0 level# ! \ Current level
Variable moves     0 moves !  \ number of moves
Variable score     0 score !  \ total number of scores

UNLOCK
>MINIMAL

: new-maze ( n -- addr ) \ add a new level
    T here mazes rot 1 H ?DO T @ H LOOP T !
    0 , 0 , here >maze ! 0 , H ;
: count-$ ( addr u -- n )  0 rot rot
    over + swap ?DO  I c@ [char] $ = -  LOOP ;
: m: ( "string" -- )  \ add a level line (top first!)
    -1 parse tuck 2dup count-$
    T >maze @ 1 cells - dup @ rot + swap ! H
    bounds ?DO  I c@ T c, H  LOOP
    /maze swap - 0 ?DO  bl T c, H  LOOP
    T >maze @ here over cell+ - swap ! H ;

LOCK

: maze-field ( -- addr n )
    maze dup cell+ swap @ chars ;

: .score ( -- )
    ." Level: " level# @ 2 .r ."  Score: " score @ 4 .r
    ."  Moves: " moves @ 6 .r ."  Rocks: " rocks @ 2 .r ;

: .maze ( -- )  \ display maze
    0 0 at-xy  .score
    cr  maze-field over + swap
    DO  I /maze type cr  /maze chars  +LOOP ;

: find-soko ( -- n )
    maze-field 0
    DO  dup I chars + c@ [char] @ =
	IF  drop I  UNLOOP  EXIT  THEN
    LOOP  true abort" No player in field!" ;

: level ( n -- flag )  \ finds level n
    dup level# !
    mazes  swap 0
    ?DO  @  dup 0= IF  drop false  UNLOOP  EXIT  THEN  LOOP
    cell+ dup @ rocks !
    cell+ dup @ cell+ maze swap chars move
    find-soko soko ! true ;

\ now the playing rules as replacement strings

: 'soko ( -- addr ) \ gives player's address
    maze cell+ soko @ chars + ;

: apply-rule? ( addr u offset -- flag )
    'soko 2swap
    \ offset soko-addr addr u
    0 DO
	over c@ over c@ <>
	IF  drop 2drop false  UNLOOP  EXIT  THEN
	>r over chars + r> char+
    LOOP  2drop drop  true ;

: apply-rule! ( addr u offset -- )
    'soko
    2swap
    \ offset soko-addr addr u
    0 DO
	count rot tuck c! rot tuck chars + rot
    LOOP  2drop drop ;

: play-rule ( addr1 u1 addr2 u2 offset -- flag )
    >r 2swap r@  apply-rule?
    IF  r> apply-rule! true  ELSE  r> drop 2drop false  THEN ;

\ player may move up, down, left and right

: (move)  ( offset -- )
    >r  1 moves +!
    S" @ "  S"  @"  r@ play-rule  IF  r> soko +!  EXIT  THEN
    S" @."  S"  &"  r@ play-rule  IF  r> soko +!  EXIT  THEN
    S" @$ " S"  @$" r@ play-rule  IF  r> soko +!  EXIT  THEN
    S" @*." S"  &*" r@ play-rule  IF  r> soko +!  EXIT  THEN
    S" @* " S"  &$" r@ play-rule
          IF  r> soko +!  1 rocks +! -1 score +!  EXIT  THEN
    S" @$." S"  @*" r@ play-rule
          IF  r> soko +! -1 rocks +!  1 score +!  EXIT  THEN
    S" &*." S" .&*" r@ play-rule  IF  r> soko +!  EXIT  THEN
    S" &$ " S" .@$" r@ play-rule  IF  r> soko +!  EXIT  THEN
    S" & "  S" .@"  r@ play-rule  IF  r> soko +!  EXIT  THEN
    S" &."  S" .&"  r@ play-rule  IF  r> soko +!  EXIT  THEN
    S" &* " S" .&$" r@ play-rule
    IF  r> soko +!  1 rocks +! -1 score +!  EXIT  THEN
    -1 moves +!  r> drop  ;

: soko-right  1            (move) ;
: soko-left   -1           (move) ;
: soko-down   /maze        (move) ;
: soko-up     /maze negate (move) ;

: print-help
    ." Move soko '@' with h, j, k or l key (like vi)" cr
    ." or with vt100 cursor keys." cr ;

Variable redraw

: play-loop ( -- )  redraw on
    BEGIN
	rocks @ 0=
	IF
	    level# @ 1+ level  0= IF  EXIT  THEN
	    redraw on
	THEN
	key? 0= redraw @ and  IF  .maze redraw off  THEN
	key
	CASE
	    [char] ? OF  print-help false  ENDOF
	    
	    [char] h OF  soko-left  redraw on false  ENDOF
	    [char] j OF  soko-down  redraw on false  ENDOF
	    [char] k OF  soko-up    redraw on false  ENDOF
	    [char] l OF  soko-right redraw on false  ENDOF

	    \ vt100 cursor keys should work too
	    27       OF  key [char] [ <>   ENDOF
	    [char] D OF  soko-left  redraw on false  ENDOF
	    [char] B OF  soko-down  redraw on false  ENDOF
	    [char] A OF  soko-up    redraw on false  ENDOF
	    [char] C OF  soko-right redraw on false  ENDOF

	    [char] q OF  true              ENDOF
	false swap  ENDCASE
    UNTIL ;

\ start game with "sokoban"

: sokoban ( -- )
    page 1 level IF  play-loop ." Game finished!"  THEN ;
    
001 new-maze
m:     #####
m:     #   #
m:     #$  #
m:   ###  $##
m:   #  $ $ #
m: ### # ## #   ######
m: #   # ## #####  ..#
m: # $  $          ..#
m: ##### ### #@##  ..#
m:     #     #########
m:     #######
002 new-maze
m: ############
m: #..  #     ###
m: #..  # $  $  #
m: #..  #$####  #
m: #..    @ ##  #
m: #..  # #  $ ##
m: ###### ##$ $ #
m:   # $  $ $ $ #
m:   #    #     #
m:   ############
003 new-maze
m:         ########
m:         #     @#
m:         # $#$ ##
m:         # $  $#
m:         ##$ $ #
m: ######### $ # ###
m: #....  ## $  $  #
m: ##...    $  $   #
m: #....  ##########
m: ########
004 new-maze
m:            ########
m:            #  ....#
m: ############  ....#
m: #    #  $ $   ....#
m: # $$$#$  $ #  ....#
m: #  $     $ #  ....#
m: # $$ #$ $ $########
m: #  $ #     #
m: ## #########
m: #    #    ##
m: #     $   ##
m: #  $$#$$  @#
m: #    #    ##
m: ###########
005 new-maze
m:         #####
m:         #   #####
m:         # #$##  #
m:         #     $ #
m: ######### ###   #
m: #....  ## $  $###
m: #....    $ $$ ##
m: #....  ##$  $ @#
m: #########  $  ##
m:         # $ $  #
m:         ### ## #
m:           #    #
m:           ######
006 new-maze
m: ######  ###
m: #..  # ##@##
m: #..  ###   #
m: #..     $$ #
m: #..  # # $ #
m: #..### # $ #
m: #### $ #$  #
m:    #  $# $ #
m:    # $  $  #
m:    #  ##   #
m:    #########
