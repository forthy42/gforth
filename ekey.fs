\ ekey etc.

\ Copyright (C) 1999,2002,2003,2004 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.


\ this implementation of EKEY just translates VT100/ANSI escape
\ sequences to ekeys.

\ Caveats: It also translates the sequences if they were not generated
\ by pressing the key; moreover, it waits until a complete sequence or
\ an invalid prefix to a sequence has arrived before reporting true in
\ EKEY? and before completing EKEY.  One way to fix this would be to
\ use timeouts when waiting for the next key; however, this may lead
\ to situations in slow networks where single events result in several
\ EKEYs, which appears less desirable to me.

\ The keycode names are compatible with pfe-0.9.14

: keycode ( "name" -- ; name execution: -- u )
    create ;

keycode k-left
keycode k-right
keycode k-up
keycode k-down
keycode k-home
keycode k-end
keycode k-prior \ note: captured by xterm
keycode k-next \ note: captured by xterm
keycode k-insert \ not in pfe
127 constant k-delete \ not an escape sequence on my xterm, so use ASCII code
\ function/keypad keys
keycode k1
keycode k2
keycode k3
keycode k4
keycode k5
keycode k6
keycode k7
keycode k8
keycode k9
keycode k10
keycode k11 \ not in pfe
keycode k12 \ not in pfe
\ shifted fuinction keys (don't work in xterm (same as unshifted, but
\ s-k1..s-k8 work in the Linux console)
keycode s-k1
keycode s-k2
keycode s-k3
keycode s-k4
keycode s-k5
keycode s-k6
keycode s-k7
keycode s-k8
keycode s-k9
keycode s-k10
keycode s-k11 \ not in pfe
keycode s-k12 \ not in pfe

\ helper word
\ print a key sequence:
\ : key-sequence  ( -- )
\     key begin
\         cr dup . emit
\         key? while
\         key
\     repeat ;

create key-buffer 8 chars allot
2variable key-buffered  key-buffer 0 key-buffered 2!

: char-append-buffer ( c addr -- )
    tuck 2@ chars + c!
    dup 2@ 1+ rot 2! ;

:noname ( -- c )
    \ buffered key
    key-buffered 2@ dup if
	1- 2dup key-buffered 2!
	chars + c@
    else
	2drop defers key
    then ;
is key

: unkey ( c -- )
    key-buffered char-append-buffer ;
    
: unkeys ( addr u -- )
    -1 swap 1- -do
	dup i chars + c@ unkey
	1 -loop
    drop ;

:noname ( -- flag )
    key-buffered 2@ nip 0<> defers key? or ;
is key?

table constant esc-sequences \ and prefixes

create ekey-buffer 8 chars allot
2variable ekey-buffered

[IFUNDEF] #esc  27 Constant #esc  [THEN]

: esc-prefix ( -- u )
    key? if
	key ekey-buffered char-append-buffer
	ekey-buffered 2@ esc-sequences search-wordlist
	if
	    execute exit
	endif
    endif
    ekey-buffered 2@ unkeys #esc ;

: esc-sequence ( xt addr u -- ; name execution: -- u ) recursive
    \ define key "name" and all prefixes
    2dup 1- dup
    if
	2dup esc-sequences search-wordlist
	if
	    drop 2drop
	else
	    ['] esc-prefix -rot esc-sequence
	then
    else
	2drop
    then ( xt addr u )
    nextname alias ;

\ nac02dec1999 exclude the escape sequences if we are using crossdoc.fs to generate
\ a documentation file. Do this because key sequences [ and OR here clash with
\ standard names and so prevent them appearing in the documentation. 
[IFUNDEF] put-doc-entry
get-current esc-sequences set-current

\ esc sequences (derived by using key-sequence in an xterm)

' k-left	s" [D"	esc-sequence
' k-right	s" [C"	esc-sequence
' k-up		s" [A"	esc-sequence
' k-down	s" [B"	esc-sequence
' k-home	s" [H"	esc-sequence
' k-end		s" [F"	esc-sequence
' k-prior	s" [5~"	esc-sequence
' k-next	s" [6~"	esc-sequence
' k-insert	s" [2~"	esc-sequence

' k1	s" OP"	esc-sequence
' k2	s" OQ"	esc-sequence
' k3	s" OR"	esc-sequence
' k4	s" OS"	esc-sequence
' k5	s" [15~" esc-sequence
' k6	s" [17~" esc-sequence
' k7	s" [18~" esc-sequence
' k8	s" [19~" esc-sequence
' k9	s" [20~" esc-sequence
' k10	s" [21~" esc-sequence
' k11	s" [23~" esc-sequence
' k12	s" [24~" esc-sequence

\ esc sequences from Linux console:

' k1       s" [[A" esc-sequence
' k2       s" [[B" esc-sequence
' k3       s" [[C" esc-sequence
' k4       s" [[D" esc-sequence
' k5       s" [[E" esc-sequence
' k-delete s" [3~" esc-sequence
' k-home   s" [1~" esc-sequence
' k-end    s" [4~" esc-sequence

' s-k1 s" [25~" esc-sequence
' s-k2 s" [26~" esc-sequence
' s-k3 s" [28~" esc-sequence
' s-k4 s" [29~" esc-sequence
' s-k5 s" [31~" esc-sequence
' s-k6 s" [32~" esc-sequence
' s-k7 s" [33~" esc-sequence
' s-k8 s" [34~" esc-sequence

set-current
[ENDIF]

: clear-ekey-buffer ( -- )
      ekey-buffer 0 ekey-buffered 2! ;

: ekey ( -- u ) \ facility-ext e-key
    key dup #esc =
    if
	drop clear-ekey-buffer
	esc-prefix
    then ;

: ekey>char ( u -- u false | c true ) \ facility-ext e-key-to-char
    dup 256 u< ;

' key? alias ekey? ( -- flag )

\  : esc? ( -- flag ) recursive
\      key? 0=
\      if
\  	false exit
\      then
\      key ekey-buffered char-append-buffer
\      ekey-buffered 2@ esc-sequences search-wordlist
\      if
\  	['] esc-prefix =
\  	if
\  	    esc? exit
\  	then
\      then
\      true ;

\  : ekey? ( -- flag ) \ facility-ext e-key-question
\      \G Return @code{true} if a keyboard event is available (use
\      \G @code{ekey} to receive the event), @code{false} otherwise.
\      key?
\      if
\  	key dup #esc =
\  	if
\  	    clear-ekey-buffer esc?
\  	    ekey-buffered 2@ unkeys
\  	else
\  	    true
\  	then
\  	swap unkey
\      else
\  	false
\      then ;

\ : test-ekey?
\     begin
\ 	begin
\ 	    begin
\ 		key? until
\ 	    ekey? until
\ 	.s ekey .s drop
\     again ;
\ test-ekey?
