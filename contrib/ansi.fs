\ ansi.fs
\
\ ANSI Terminal words for gforth
\
\ Copyright (c) 1999--2004 Krishna Myneni
\ Creative Consulting for Research and Education
\
\ This software is provided under the terms of the GNU
\ General Public License.
\
\ ====> Requires that the file strings.fs be included first
\
\ Revisions: 
\    06-10-1999
\    10-11-1999 force cursor to 0 0 on page; define at-xy  KM
\    01-23-2000 replaced char with [char] for ANS Forth compatibility KM
\    08-29-2002 use 0,0 as top left for AT-XY in accord with ANS Forth  KM
\    09-08-2004 added console query words provided by Charley Shattuck: 
\                 AT-XY?  ROWS  COLS   
\               Note that ROWS and COLS are also provided in gforth and PFE
\    09-10-2004 added scrolling words -- CS
\    09-17-2004 ported from kForth  KM

\ Colors

0 constant BLACK
1 constant RED
2 constant GREEN
3 constant YELLOW
4 constant BLUE
5 constant MAGENTA
6 constant CYAN
7 constant WHITE


variable orig_base

: save_base ( -- | store current base and set to decimal )
	base @ orig_base ! 
	decimal ;

: restore_base ( -- | restore original base )
	orig_base @ base ! ;

save_base

: ansi_escape ( -- | output escape code )
	27 emit [char] [ emit ;


: clrtoeol ( -- | clear to end of line )
	ansi_escape [char] K emit ;

: gotoxy ( x y -- | position cursor at col x row y, origin is 1,1 )
	save_base
	ansi_escape s>string count type [char] ; emit
	s>string count type [char] H emit
	restore_base ;

\ : at-xy ( x y -- |  ANS compatible version of gotoxy, origin is 0,0 )
\	save_base
\	ansi_escape 1+ s>string count type [char] ; emit
\	1+ s>string count type [char] H emit
\	restore_base ;

\ : page ( -- | clear the screen and put cursor at top left )
\	ansi_escape [char] 2 emit [char] J emit 0 0 at-xy ;

: cur_up ( n -- | move cursor up by n lines )
	save_base  
	ansi_escape s>string count type [char] A emit
	restore_base ;

: cur_down ( n -- | move cursor down by n lines )
	save_base 
	ansi_escape s>string count type [char] B emit 
	restore_base ;

: cur_left ( n -- | move cursor left by n columns )
	save_base
	ansi_escape s>string count type [char] D emit 
	restore_base ;

: cur_right ( n -- | move cursor right by n columns )
	save_base
	ansi_escape s>string count type [char] C emit 
	restore_base ;

: save_cursor ( -- | save current cursor position )
	ansi_escape [char] s emit ;

: restore_cursor ( -- | restore cursor to previously saved position )
	ansi_escape [char] u emit ;

: foreground ( n -- | set foreground color to n )
	save_base
	ansi_escape 30 + s>string count type [char] m emit 
	restore_base ;

: background ( n -- | set background color to n )
	save_base
	ansi_escape 40 + s>string count type [char] m emit 
	restore_base ;

: text_normal ( -- | set normal text display )
	ansi_escape [char] 0 emit [char] m emit ;

: text_bold ( -- | set bold text )
	ansi_escape [char] 1 emit [char] m emit ;

: text_underline ( -- | set underlined text )
	save_base
	ansi_escape [char] 4 emit [char] m emit
	restore_base ;

: text_blink ( -- | set blinking text )
	save_base
	ansi_escape [char] 5 emit [char] m emit
	restore_base ;

: text_reverse ( -- | set reverse video text )
	save_base
	ansi_escape [char] 7 emit [char] m emit
	restore_base ;  

: read-cdnumber  ( c - n | read a numeric entry delimited by character c)
	>r 0 begin
		key dup r@ - while
		swap 10 * swap [char] 0 - +
	repeat
	r> 2drop ;

: at-xy?  ( -- x y | return the current cursor coordinates)
	ansi_escape ." 6n"
	key drop key drop  \ <esc> [
	[char] ; read-cdnumber [char] R read-cdnumber
	1- swap 1- ;

\ : rows  ( -- n | return row size of console) 
\    save_cursor  0 100 at-xy  at-xy? nip  restore_cursor ;

\ : cols  ( -- n | return column size of console)
\    save_cursor  200 0 at-xy  at-xy? drop restore_cursor ;  

: reset-scrolling  (  - )
	ansi_escape [char] r emit ;

: scroll-window  ( start end - )
	ansi_escape swap u>string count type
	[char] ; emit u>string count type
	[char] r emit ;

: scroll-up  (  - ) ansi_escape [char] M emit ;

: scroll-down  (  - ) ansi_escape [char] D emit ;


restore_base
