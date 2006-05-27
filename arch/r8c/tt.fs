\ Variables, constants

rom

bl bl 2constant empt
variable wiping
 2 constant col0
 0 constant row0
10 constant wide
20 constant deep
char J value left-key
char K value rot-key
char L value right-key
bl value drop-key
char P value pause-key
12 value refresh-key
char Q value quit-key

variable score
variable pieces
variable levels
variable delay

variable brow
variable bcol


\ stupid random number generator

variable seed $1234 seed !

: randomize  timer @ seed ! ;

: random \ max --- n ; return random number < max
  seed @ 13 * [ hex ] 07FFF [ decimal ] and
  dup seed !  swap mod ;

\ Access pairs of characters in memory:

: 2c@  dup 1+ c@ swap c@ ;
: 2c!  dup >r c! r> 1+ c! ;

: pn    ( n -- )  0 <# # # #> type ;
: ;pn   [char] ; emit pn ;
: ESC[  27 emit [char] [ emit ;
: at-xy 1+ swap 1+ swap ESC[ pn ;pn [char] H emit ;
: page  ESC[ ." 2J" 0 0 at-xy ;

: d<>  d- or 0<> ;
: d=  d- or 0= ;


\ Drawing primitives:

: 2emit  emit emit ;

: position \ row col --- ; cursor to the position in the pit
  2* col0 + swap row0 + at-xy ;

: stone  \ c1 c2 --- ; draw or undraw these two characters
    wiping @ if  2drop 2 spaces
    else  2emit  then ;


\ Define the pit where bricks fall into:

: def-pit create wide deep * 2* allot
  does> rot wide * rot + 2* + ;

ram
def-pit pit
rom

: empty-pit deep 0 do
	wide 0 do  empt j i pit 2c!
  loop loop ;


\ Displaying:

: draw-bottom \ --- ; redraw the bottom of the pit
  deep -1 position
  [char] + dup stone
  wide 0 do  [char] = dup stone  loop
  [char] + dup stone ;

: draw-frame \ --- ; draw the border of the pit
  deep 0 do
      i -1   position [char] | dup stone
      i wide position [char] | dup stone
  loop  draw-bottom ;

: bottom-msg \ addr cnt --- ;
  deep over 2/ wide swap - 2/ position type ;

: draw-line \ line ---
    dup 0 position  wide 0 do
	dup i pit 2c@ 2emit  loop  drop ;

: draw-pit \ --- ; draw the contents of the pit
  deep 0 do  i draw-line  loop ;

: show-key \ char --- ; visualization of that character
  dup bl <
  if  [char] @ or  [char] ^ emit  emit  space
  else  [char] ` emit  emit  [char] ' emit
  then ;

: show-help \ --- ; display some explanations
  30  1 at-xy ." ***** T E T R I S *****"
  30  2 at-xy ." ======Dirk Zoller======"
  30  4 at-xy ." Use keys:"
  32  5 at-xy left-key show-key ."  Move left"
  32  6 at-xy rot-key show-key ."  Rotate"
  32  7 at-xy right-key show-key ."  Move right"
  32  8 at-xy drop-key show-key ."  Drop"
  32  9 at-xy pause-key show-key ."  Pause"
  32 10 at-xy refresh-key show-key ."  Refresh"
  32 11 at-xy quit-key show-key ."  Quit"
  32 13 at-xy ." -> "
  30 16 at-xy ." Score:"
  30 17 at-xy ." Pieces:"
  30 18 at-xy ." Levels:"
\    0 22 at-xy ."  ==== This program was written 1994"
\    ."  in pure dpANS Forth by Dirk Uwe Zoller ===="
\    0 23 at-xy ."  =================== Copy it, port it,"
\    ."  play it, enjoy it! ====================="
;

: update-score \ --- ; display current score
  38 16 at-xy score @ 3 .r
  38 17 at-xy pieces @ 3 .r
  38 18 at-xy levels @ 3 .r ;

: refresh \ --- ; redraw everything on screen
  page draw-frame draw-pit show-help update-score ;


\ Define shapes of bricks:

: def-brick create
  does> rot 4 * rot + 2* + ;

: ,s" [char] " parse bounds
    DO  i c@ c,  LOOP ;

def-brick brick1 ,s"         "
   ,s" ######  "
   ,s"   ##    "
   ,s"         "

def-brick brick2 ,s"         "
   ,s" <><><><>"
   ,s"         "
   ,s"         "

def-brick brick3 ,s"         "
   ,s"   {}{}{}"
   ,s"   {}    "
   ,s"         "

def-brick brick4 ,s"         "
   ,s" ()()()  "
   ,s"     ()  "
   ,s"         "

def-brick brick5 ,s"         "
   ,s"   [][]  "
   ,s"   [][]  "
   ,s"         "

def-brick brick6 ,s"         "
   ,s" @@@@    "
   ,s"   @@@@  "
   ,s"         "

def-brick brick7 ,s"         "
   ,s"   %%%%  "
   ,s" %%%%    "
   ,s"         "

\ this brick is actually in use:

ram
def-brick brick  ,s"         "
   ,s"         "
   ,s"         "
   ,s"         "

def-brick scratch ,s"         "
   ,s"         "
   ,s"         "
   ,s"         "
rom

create bricks
  ' brick1 ,  ' brick2 ,  ' brick3 ,  ' brick4 ,
  ' brick5 ,  ' brick6 ,  ' brick7 ,

create brick-val
  1 c, 2 c, 3 c, 3 c, 4 c, 5 c, 5 c,


: is-brick
  >body ['] brick >body 32 cmove ;

: new-brick
  1 pieces +!  7 random
  bricks over cells + @ is-brick
  brick-val swap chars + c@ score +! ;

: rotleft 4 0 do 4 0 do
      j i brick 2c@  3 i - j scratch 2c!
  loop loop
  ['] scratch is-brick ;

: rotright 4 0 do 4 0 do
      j i brick 2c@  i 3 j - scratch 2c!
  loop loop
  ['] scratch is-brick ;

: draw-brick \ row col ---
  4 0 do 4 0 do
      j i brick 2c@  empt d<>
      if  over j + over i +  position
   j i brick 2c@  stone
      then
  loop loop  2drop ;

: show-brick wiping off draw-brick ;
: hide-brick wiping on  draw-brick ;

: put-brick
  4 0 do 4 0 do
      j i brick 2c@  empt d<>
      if  over j +  over i +  pit
   j i brick 2c@  rot 2c!
      then
  loop loop  2drop ;

: remove-brick
  4 0 do 4 0 do
      j i brick 2c@  empt d<>
      if  over j + over i + pit empt rot 2c!  then
  loop loop  2drop ;

: test-brick
  4 0 do 4 0 do
      j i brick 2c@ empt d<>
      if  over j +  over i +
   over dup 0< swap deep >= or
   over dup 0< swap wide >= or
   2swap pit 2c@  empt d<>
   or or if  unloop unloop 2drop false  exit  then
      then
  loop loop  2drop true ;

: move-brick
  brow @ bcol @ remove-brick
  swap brow @ + swap bcol @ + 2dup test-brick
  if  brow @ bcol @ hide-brick
      2dup bcol ! brow !
      2dup show-brick put-brick  true
  else  2drop brow @ bcol @ put-brick  false
  then ;

: rotate-brick \ flag --- flag ; left/right, success
  brow @ bcol @ remove-brick
  dup if  rotright  else  rotleft  then
  brow @ bcol @ test-brick
  over if  rotleft  else  rotright  then
  if  brow @ bcol @ hide-brick
      if  rotright  else  rotleft  then
      brow @ bcol @ put-brick
      brow @ bcol @ show-brick  true
  else  drop false  then ;

: insert-brick \ row col --- flag ; introduce a new brick
  2dup test-brick
  if  2dup bcol ! brow !
      2dup put-brick  draw-brick  true
  else  false  then ;

: drop-brick \ --- ; move brick down fast
  begin  1 0 move-brick 0=  until ;

: move-line \ from to ---
  over 0 pit  over 0 pit  wide 2*  cmove
  draw-line
  dup 0 pit  wide 2*  bl fill  draw-line ;

: line-full \ line-no --- flag
  true  wide 0
  do  over i pit 2c@ empt d=
      if  2drop false unloop exit  then
  loop nip ;

: remove-lines \ ---
  deep deep
  begin
      swap
      begin  1- dup 0< if
         2drop exit  then  dup line-full
      while  1 levels +!  10 score +!
      repeat
      swap 1-
      2dup <> if  2dup move-line  then
  again ;

: interaction \ --- flag
  case  key toupper
   left-key of  0 -1 move-brick drop  endof
   right-key of  0  1 move-brick drop  endof
   rot-key of  0 rotate-brick drop  endof
    drop-key of  drop-brick  endof
    pause-key of  S"  paused " bottom-msg
      key drop draw-bottom  endof
    refresh-key of  refresh  endof
    quit-key of  false exit  endof
  endcase  true ;

: initialize \ --- ; prepare for playing
  randomize empty-pit refresh
  0 score !  0 pieces !  0 levels !
  100 delay ! ;

: adjust-delay \ --- ; make it faster with increasing score
  levels @
  dup  50 < if  100 over -  else
  dup 100 < if   62 over 4 / -  else
  dup 500 < if   31 over 16 / -  else
      0  then then then
  delay !  drop ;

: play-game \ --- ; play one tetris game
  begin
      new-brick
      -1 3 insert-brick
  while
      begin  4 0
   do  35 13 at-xy
       delay @ ms key?
       if interaction 0=
    if  unloop exit  then
       then
   loop
   1 0 move-brick  0=
      until
      remove-lines
      update-score
      adjust-delay
  repeat ;

: tt  \ --- ; play the tetris game
  initialize
  s"  Press any key " bottom-msg
  key drop draw-bottom
  begin
      play-game
      s"  Again? " bottom-msg
      key toupper [char] Y =
  while  initialize  repeat
  0 23 at-xy cr ;
