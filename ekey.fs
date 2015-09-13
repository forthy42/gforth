\ ekey etc.

\ Copyright (C) 1999,2002,2003,2004,2005,2006,2007,2008,2009,2013,2014 Free Software Foundation, Inc.

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

$80000000 constant keycode-start
$80000017 constant keycode-limit

create keycode-table keycode-limit keycode-start - cells allot

: keycode ( u1 "name" -- u2 ; name execution: -- u )
    dup keycode-limit keycode-start within -11 and throw
    dup constant
    dup latest keycode-table rot keycode-start - th !
    1+ ;

\ most of the keys are also in pfe, except:
\ k-insert, k-delete, k11, k12, s-k11, s-k12

$40000000 constant k-shift-mask ( -- u ) \ X:ekeys
$20000000 constant k-ctrl-mask ( -- u )  \ X:ekeys
$10000000 constant k-alt-mask ( -- u )   \ X:ekeys

: simple-fkey-string ( u1 -- c-addr u ) \ gforth
    \G @i{c-addr u} is the string name of the function key @i{u1}.
    \G Only works for simple function keys without modifier masks.
    \G Any @i{u1} that does not correspond to a simple function key
    \G currently produces an exception.
    dup keycode-limit keycode-start within -24 and throw
    keycode-table swap keycode-start - th @ name>string ;

: fkey. ( u -- ) \ gforth fkey-dot
    \G Print a string representation for the function key @i{u}.
    \G @i{U} must be a function key (possibly with modifier masks),
    \G otherwise there may be an exception.
    dup [ k-shift-mask k-ctrl-mask k-alt-mask or or invert ] literal and
    simple-fkey-string type
    dup k-shift-mask and if ."  k-shift-mask or" then
    dup k-ctrl-mask  and if ."  k-ctrl-mask or"  then
        k-alt-mask   and if ."  k-alt-mask or"   then ;

keycode-start
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
keycode k-enter  ( -- u ) \ gforth
\ only useful in combinations, but it is keycode+#lf

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

Variable key-buffer

: char-append-buffer ( c addr -- )
    >r { c^ ins-char }  ins-char 1 r> 0 $ins ;

: buf-key ( -- c )
    \ buffered key
    key-buffer $@len if
	key-buffer $@ drop c@
	key-buffer 0 1 $del
    else
	defers key
    then ;
' buf-key is key

: unkey ( c -- )  key-buffer char-append-buffer ;
    
: unkeys ( addr u -- )  key-buffer 0 $ins ;

: inskey ( key -- )  key-buffer c$+! ;
: inskeys ( addr u -- )  key-buffer $+! ;

: buf-key? ( -- flag )
    key-buffer $@len 0<> defers key? or ;
' buf-key? is key?

table constant esc-sequences \ and prefixes

Variable ekey-buffer
[IFUNDEF] #esc  27 Constant #esc  [THEN]

Create esc-masks#
0 ,
k-shift-mask ,
k-alt-mask ,
k-shift-mask k-alt-mask or ,
k-ctrl-mask ,
k-shift-mask k-ctrl-mask or ,
k-alt-mask k-ctrl-mask or ,
k-shift-mask k-alt-mask or k-ctrl-mask or ,

: esc-mask ( addr u -- addr' u' mask )
    ';' $split dup IF
	0. 2swap >number  2swap drop 1- 0 max >r
	[: 2swap 2dup 1 safe/string s" 1" str= + type type ;] $tmp
	r> 7 and cells esc-masks# + @
	EXIT
    ELSE  2drop over c@ 'O' = IF
	    1 /string
	    0. 2swap >number  2swap drop 1- 0 max >r
	    [: 'O' emit type ;] $tmp
	    r> 7 and cells esc-masks# + @
	EXIT  THEN
    THEN
    0 ;

: esc-prefix ( -- u )
    key? if
	key ekey-buffer c$+!
	ekey-buffer $@ esc-mask >r
        esc-sequences search-wordlist
        if
            execute r> or exit
	endif
	rdrop
    endif
    ekey-buffer $@ unkeys #esc ;

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

k-enter  k-shift-mask or s" OM" esc-sequence
k-enter  k-alt-mask or   s" x" over #cr swap c! esc-sequence
k-enter  k-alt-mask or k-shift-mask or s" eOM" over #esc swap c! esc-sequence

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

\ esc sequences for MacOS X iterm <e7a7c785-3bea-408b-94e9-4b59b008546f@x16g2000prn.googlegroups.com>
k-left   s" OD" esc-sequence
k-right  s" OC" esc-sequence
k-up     s" OA" esc-sequence
k-down   s" OB" esc-sequence

set-current
[ENDIF]

: clear-ekey-buffer ( -- )
    ekey-buffer $off ;

[IFDEF] max-single-byte
    : read-xkey ( key -- flag )
	clear-ekey-buffer
	ekey-buffer c$+!
	ekey-buffer $@ x-size 1 +do
	    key? 0= ?leave
	    key ekey-buffer c$+!
	loop
	ekey-buffer $@ x-size ekey-buffer $@len u>= ;
    : get-xkey ( u -- xc )
	dup max-single-byte u>= if
	    read-xkey if
		ekey-buffer $@ drop xc@+ nip  else
		ekey-buffer $@ unkeys key     then
	    clear-ekey-buffer
	then ;
    : xkey? ( -- flag )
	key? dup if
	    drop key read-xkey ekey-buffer $@ unkeys
	    clear-ekey-buffer  then ;
[THEN]

: ekey ( -- u ) \ facility-ext e-key
    \G Receive a keyboard event @var{u} (encoding implementation-defined).
    key dup #esc =
    if
        drop clear-ekey-buffer
        esc-prefix  exit
    then
    [IFDEF] max-single-byte
	get-xkey
    [THEN]
;

[IFDEF] max-single-byte
: ekey>char ( u -- u false | c true ) \ facility-ext e-key-to-char
    \G Convert keyboard event @var{u} into character @code{c} if possible.
    dup max-single-byte u< ; \ k-left must be first!
: ekey>xchar ( u -- u false | xc true ) \ xchar-ext e-key-to-xchar
    \G Convert keyboard event @var{u} into xchar @code{xc} if possible.
    dup k-left u< ; \ k-left must be first!
: ekey>fkey ( u1 -- u2 f ) \ X:ekeys
\G If u1 is a keyboard event in the special key set, convert
\G keyboard event @var{u1} into key id @var{u2} and return true;
\G otherwise return @var{u1} and false.
    ekey>xchar 0= ;

' xkey? alias ekey? ( -- flag ) \ facility-ext e-key-question
[ELSE]
: ekey>char ( u -- u false | c true ) \ facility-ext e-key-to-char
    \G Convert keyboard event @var{u} into character @code{c} if possible.
    dup k-left u< ; \ k-left must be first!
: ekey>fkey ( u1 -- u2 f ) \ X:ekeys
\G If u1 is a keyboard event in the special key set, convert
\G keyboard event @var{u1} into key id @var{u2} and return true;
\G otherwise return @var{u1} and false.
    ekey>char 0= ;

' key? alias ekey? ( -- flag ) \ facility-ext e-key-question
[THEN]

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