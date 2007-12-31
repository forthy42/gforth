\ ekey etc.

\ Copyright (C) 1999,2002,2003,2004,2005,2006,2007 Free Software Foundation, Inc.

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

: keycode ( u1 "name" -- u2 ; name execution: -- u )
    dup constant 1+ ;

\ most of the keys are also in pfe, except:
\ k-insert, k-delete, k11, k12, s-k11, s-k12

$40000000 constant k-shift-mask ( -- u ) \ X:ekeys
$20000000 constant k-ctrl-mask ( -- u )  \ X:ekeys
$10000000 constant k-alt-mask ( -- u )   \ X:ekeys

$80000000
keycode k-left   ( -- u ) \ X:ekeys  
keycode k-right  ( -- u ) \ X:ekeys
keycode k-up     ( -- u ) \ X:ekeys
keycode k-down   ( -- u ) \ X:ekeys
keycode k-home   ( -- u ) \ X:ekeys
\G aka Pos1
keycode k-end    ( -- u ) \ X:ekeys
keycode k-prior  ( -- u ) \ X:ekeys
\G aka PgUp
keycode k-next   ( -- u ) \ X:ekeys
\G aka PgDn    
keycode k-insert ( -- u ) \ X:ekeys
keycode k-delete ( -- u ) \ X:ekeys
\ the DEL key on my xterm, not backspace

\ function/keypad keys
keycode k-f1  ( -- u ) \ X:ekeys
keycode k-f2  ( -- u ) \ X:ekeys
keycode k-f3  ( -- u ) \ X:ekeys
keycode k-f4  ( -- u ) \ X:ekeys
keycode k-f5  ( -- u ) \ X:ekeys
keycode k-f6  ( -- u ) \ X:ekeys
keycode k-f7  ( -- u ) \ X:ekeys
keycode k-f8  ( -- u ) \ X:ekeys
keycode k-f9  ( -- u ) \ X:ekeys
keycode k-f10 ( -- u ) \ X:ekeys
keycode k-f11 ( -- u ) \ X:ekeys
keycode k-f12 ( -- u ) \ X:ekeys
drop
    
' k-f1  alias k1  ( -- u ) \ gforth-obsolete
' k-f2  alias k2  ( -- u ) \ gforth-obsolete
' k-f3  alias k3  ( -- u ) \ gforth-obsolete
' k-f4  alias k4  ( -- u ) \ gforth-obsolete
' k-f5  alias k5  ( -- u ) \ gforth-obsolete
' k-f6  alias k6  ( -- u ) \ gforth-obsolete
' k-f7  alias k7  ( -- u ) \ gforth-obsolete
' k-f8  alias k8  ( -- u ) \ gforth-obsolete
' k-f9  alias k9  ( -- u ) \ gforth-obsolete
' k-f10 alias k10 ( -- u ) \ gforth-obsolete
' k-f11 alias k11 ( -- u ) \ gforth-obsolete
' k-f12 alias k12 ( -- u ) \ gforth-obsolete
\ shifted fuinction keys (don't work in xterm (same as unshifted, but
\ s-k1..s-k8 work in the Linux console)
k-f1  k-shift-mask or constant s-k1  ( -- u ) \ gforth-obsolete 
k-f2  k-shift-mask or constant s-k2  ( -- u ) \ gforth-obsolete 
k-f3  k-shift-mask or constant s-k3  ( -- u ) \ gforth-obsolete 
k-f4  k-shift-mask or constant s-k4  ( -- u ) \ gforth-obsolete 
k-f5  k-shift-mask or constant s-k5  ( -- u ) \ gforth-obsolete 
k-f6  k-shift-mask or constant s-k6  ( -- u ) \ gforth-obsolete 
k-f7  k-shift-mask or constant s-k7  ( -- u ) \ gforth-obsolete 
k-f8  k-shift-mask or constant s-k8  ( -- u ) \ gforth-obsolete 
k-f9  k-shift-mask or constant s-k9  ( -- u ) \ gforth-obsolete 
k-f10 k-shift-mask or constant s-k10 ( -- u ) \ gforth-obsolete 
k-f11 k-shift-mask or constant s-k11 ( -- u ) \ gforth-obsolete
k-f12 k-shift-mask or constant s-k12 ( -- u ) \ gforth-obsolete

\ helper word
\ print a key sequence:
0 [IF]
: key-sequence  ( -- )
    key begin
        cr dup . emit
        key? while
        key
    repeat ;

: key-sequences ( -- )
    begin
        key-sequence cr
    again ;
[THEN]

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

: esc-sequence ( u1 addr u -- ; name execution: -- u2 ) recursive
    \ define escape sequence addr u (=name) to have value u1; if u1=0,
    \ addr u is a prefix of some other sequence (with key code u2);
    \ also, define all prefixes of addr u if necessary.
    2dup 1- dup
    if
        2dup esc-sequences search-wordlist
        if
            drop 2drop
        else
            0 -rot esc-sequence \ define the prefixes
        then
    else
        2drop
    then ( u1 addr u )
    nextname dup if ( u1 )
        constant \ full sequence for a key
    else
        drop ['] esc-prefix alias
    endif ;

\ nac02dec1999 exclude the escape sequences if we are using crossdoc.fs to generate
\ a documentation file. Do this because key sequences [ and OR here clash with
\ standard names and so prevent them appearing in the documentation. 
[IFUNDEF] put-doc-entry
get-current esc-sequences set-current

\ esc sequences (derived by using key-sequence in an xterm)
k-left   s" [D" esc-sequence
k-right  s" [C" esc-sequence
k-up     s" [A" esc-sequence
k-down   s" [B" esc-sequence
k-home   s" [H" esc-sequence
k-end    s" [F" esc-sequence
k-prior  s" [5~" esc-sequence
k-next   s" [6~" esc-sequence
k-insert s" [2~" esc-sequence
k-delete s" [3~" esc-sequence

k-left   k-shift-mask or s" [1;2D" esc-sequence
k-right  k-shift-mask or s" [1;2C" esc-sequence
k-up     k-shift-mask or s" [1;2A" esc-sequence
k-down   k-shift-mask or s" [1;2B" esc-sequence
k-home   k-shift-mask or s" [1;2H" esc-sequence
k-end    k-shift-mask or s" [1;2F" esc-sequence
k-prior  k-shift-mask or s" [5;2~" esc-sequence
k-next   k-shift-mask or s" [6;2~" esc-sequence
k-insert k-shift-mask or s" [2;2~" esc-sequence
k-delete k-shift-mask or s" [3;2~" esc-sequence

k-left   k-ctrl-mask  or s" [1;5D" esc-sequence
k-right  k-ctrl-mask  or s" [1;5C" esc-sequence
k-up     k-ctrl-mask  or s" [1;5A" esc-sequence
k-down   k-ctrl-mask  or s" [1;5B" esc-sequence
k-home   k-ctrl-mask  or s" [1;5H" esc-sequence
k-end    k-ctrl-mask  or s" [1;5F" esc-sequence
k-prior  k-ctrl-mask  or s" [5;5~" esc-sequence
k-next   k-ctrl-mask  or s" [6;5~" esc-sequence
k-insert k-ctrl-mask  or s" [2;5~" esc-sequence
k-delete k-ctrl-mask  or s" [3;5~" esc-sequence

k-left   k-alt-mask   or s" [1;3D" esc-sequence
k-right  k-alt-mask   or s" [1;3C" esc-sequence
k-up     k-alt-mask   or s" [1;3A" esc-sequence
k-down   k-alt-mask   or s" [1;3B" esc-sequence
k-home   k-alt-mask   or s" [1;3H" esc-sequence
k-end    k-alt-mask   or s" [1;3F" esc-sequence
k-prior  k-alt-mask   or s" [5;3~" esc-sequence
k-next   k-alt-mask   or s" [6;3~" esc-sequence
k-insert k-alt-mask   or s" [2;3~" esc-sequence
k-delete k-alt-mask   or s" [3;3~" esc-sequence

k1      s" OP"  esc-sequence
k2      s" OQ"  esc-sequence
k3      s" OR"  esc-sequence
k4      s" OS"  esc-sequence
k5      s" [15~" esc-sequence
k6      s" [17~" esc-sequence
k7      s" [18~" esc-sequence
k8      s" [19~" esc-sequence
k9      s" [20~" esc-sequence
k10     s" [21~" esc-sequence
k11     s" [23~" esc-sequence
k12     s" [24~" esc-sequence

s-k1    s" [1;2P" esc-sequence
s-k2    s" [1;2Q" esc-sequence
s-k3    s" [1;2R" esc-sequence
s-k4    s" [1;2S" esc-sequence
s-k5    s" [15;2~" esc-sequence
s-k6    s" [17;2~" esc-sequence
s-k7    s" [18;2~" esc-sequence
s-k8    s" [19;2~" esc-sequence
s-k9    s" [20;2~" esc-sequence
s-k10   s" [21;2~" esc-sequence
s-k11   s" [23;2~" esc-sequence
s-k12   s" [24;2~" esc-sequence

k-f1  k-ctrl-mask or  s" [1;5P" esc-sequence
k-f2  k-ctrl-mask or  s" [1;5Q" esc-sequence
k-f3  k-ctrl-mask or  s" [1;5R" esc-sequence
k-f4  k-ctrl-mask or  s" [1;5S" esc-sequence
k-f5  k-ctrl-mask or  s" [15;5~" esc-sequence
k-f6  k-ctrl-mask or  s" [17;5~" esc-sequence
k-f7  k-ctrl-mask or  s" [18;5~" esc-sequence
k-f8  k-ctrl-mask or  s" [19;5~" esc-sequence
k-f9  k-ctrl-mask or  s" [20;5~" esc-sequence
k-f10 k-ctrl-mask or  s" [21;5~" esc-sequence
k-f11 k-ctrl-mask or  s" [23;5~" esc-sequence
k-f12 k-ctrl-mask or  s" [24;5~" esc-sequence

k-f1  k-alt-mask  or  s" [1;3P" esc-sequence
k-f2  k-alt-mask  or  s" [1;3Q" esc-sequence
k-f3  k-alt-mask  or  s" [1;3R" esc-sequence
k-f4  k-alt-mask  or  s" [1;3S" esc-sequence
k-f5  k-alt-mask  or  s" [15;3~" esc-sequence
k-f6  k-alt-mask  or  s" [17;3~" esc-sequence
k-f7  k-alt-mask  or  s" [18;3~" esc-sequence
k-f8  k-alt-mask  or  s" [19;3~" esc-sequence
k-f9  k-alt-mask  or  s" [20;3~" esc-sequence
k-f10 k-alt-mask  or  s" [21;3~" esc-sequence
k-f11 k-alt-mask  or  s" [23;3~" esc-sequence
k-f12 k-alt-mask  or  s" [24;3~" esc-sequence

\ esc sequences from Linux console:

k1       s" [[A" esc-sequence
k2       s" [[B" esc-sequence
k3       s" [[C" esc-sequence
k4       s" [[D" esc-sequence
k5       s" [[E" esc-sequence
\ k-delete s" [3~" esc-sequence \ duplicate from above
k-home   s" [1~" esc-sequence
k-end    s" [4~" esc-sequence

s-k1 s" [25~" esc-sequence
s-k2 s" [26~" esc-sequence
s-k3 s" [28~" esc-sequence
s-k4 s" [29~" esc-sequence
s-k5 s" [31~" esc-sequence
s-k6 s" [32~" esc-sequence
s-k7 s" [33~" esc-sequence
s-k8 s" [34~" esc-sequence

set-current
[ENDIF]

: clear-ekey-buffer ( -- )
    ekey-buffer 0 ekey-buffered 2! ;

: ekey ( -- u ) \ facility-ext e-key
    \G Receive a keyboard event @var{u} (encoding implementation-defined).
    key dup #esc =
    if
        drop clear-ekey-buffer
        esc-prefix
    then ;

: ekey>char ( u -- u false | c true ) \ facility-ext e-key-to-char
    \G Convert keyboard event @var{u} into character @code{c} if possible.
    dup k-left u< ; \ k-left must be first!

: ekey>fkey ( u1 -- u2 f ) \ X:ekeys
\G If u1 is a keyboard event in the special key set, convert
\G keyboard event @var{u1} into key id @var{u2} and return true;
\G otherwise return @var{u1} and false.
    ekey>char 0= ;

' key? alias ekey? ( -- flag ) \ facility-ext e-key-question
\G True if a keyboard event is available.

\  : esc? ( -- flag ) recursive
\      key? 0=
\      if
\       false exit
\      then
\      key ekey-buffered char-append-buffer
\      ekey-buffered 2@ esc-sequences search-wordlist
\      if
\       ['] esc-prefix =
\       if
\           esc? exit
\       then
\      then
\      true ;

\  : ekey? ( -- flag ) \ facility-ext e-key-question
\      \G Return @code{true} if a keyboard event is available (use
\      \G @code{ekey} to receive the event), @code{false} otherwise.
\      key?
\      if
\       key dup #esc =
\       if
\           clear-ekey-buffer esc?
\           ekey-buffered 2@ unkeys
\       else
\           true
\       then
\       swap unkey
\      else
\       false
\      then ;

0 [if]
: test-ekey?
    begin
      begin
          begin
              key? until
          ekey? until
      .s ekey .s drop
    again ;
\ test-ekey?
[then]