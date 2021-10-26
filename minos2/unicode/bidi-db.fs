\ Bidirectional Unicode database (one byte per codepoint table)

\ Authors: Bernd Paysan
\ Copyright (C) 2021 Free Software Foundation, Inc.

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

get-current >r

$Variable bidi-db

$110000 bidi-db $!len
bidi-db $@ erase

: bidi@ ( start len -- addr' u' )
    bidi-db $@ 2swap >r safe/string r> umin ;

: bidis: ( n -- n+1 )
    Create dup , 1+
  DOES>  @ >r bidi@ r> fill ;
: flags: ( n -- n )
    Create dup ,
  DOES>  @ { flag }
    bidi@ bounds ?DO  flag I c+!  LOOP ;

Vocabulary bidis  also bidis definitions
0
\ strong left
synonym ..L 2drop 1+
bidis: ..LRE
bidis: ..LRO
bidis: ..LRI
\ strong right
bidis: ..R
bidis: ..RLE
bidis: ..RLO
bidis: ..RLI
bidis: ..AL

\ pop
bidis: ..PDF \ end {LR|RL}[EO]
bidis: ..PDI \ end {LR|RL}I

\ weak
bidis: ..AN
bidis: ..B
bidis: ..BN
bidis: ..CS
bidis: ..EN
bidis: ..ES
bidis: ..ET
bidis: ..FSI
bidis: ..NSM
bidis: ..S

\ neutral
bidis: ..WS
bidis: ..ON
drop
include minos2/unicode/bidis.fs

previous r@ set-current

Vocabulary verticals also verticals definitions
0
synonym ..R 2drop  $20 +
flags: ..U  $20 +
flags: ..Tr $20 +
flags: ..Tu $20 +
drop
include minos2/unicode/verticals.fs
previous r@ set-current

Vocabulary mirrors also mirrors definitions
0
synonym ..N 2drop  $80 +
flags: ..Y
drop

include minos2/unicode/mirrors.fs
previous r> set-current
