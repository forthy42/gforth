\ ekey etc.

\ Authors: Bernd Paysan, Anton Ertl, Neal Crook
\ Copyright (C) 1999,2002,2003,2004,2005,2006,2007,2008,2009,2013,2014,2015,2016,2017,2018,2019,2021,2022,2023,2024 Free Software Foundation, Inc.

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
$80000022 constant keycode-limit

create keycode-table keycode-limit keycode-start - cells allot

: keycode ( u1 "name" -- u2 ; name execution: -- u )
    dup keycode-limit keycode-start within -11 and throw
    dup constant
    dup latestnt keycode-table rot keycode-start - th !
    1+ ;

\ most of the keys are also in pfe, except:
\ k-insert, k-delete, k11, k12, s-k11, s-k12

28 constant mask-shift# ( n -- shift ) 

$1 mask-shift# lshift constant k-shift-mask ( -- u ) \ facility-ext
$2 mask-shift# lshift constant k-alt-mask ( -- u )   \ facility-ext
$4 mask-shift# lshift constant k-ctrl-mask ( -- u )  \ facility-ext

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
    ( ) k-alt-mask   and if ."  k-alt-mask or"   then ;

keycode-start
keycode k-left   ( -- u ) \ facility-ext
keycode k-right  ( -- u ) \ facility-ext
keycode k-up     ( -- u ) \ facility-ext
keycode k-down   ( -- u ) \ facility-ext
keycode k-home   ( -- u ) \ facility-ext
\G aka Pos1
keycode k-end    ( -- u ) \ facility-ext
keycode k-prior  ( -- u ) \ facility-ext
\G aka PgUp
keycode k-next   ( -- u ) \ facility-ext
\G aka PgDn    
keycode k-insert ( -- u ) \ facility-ext
keycode k-delete ( -- u ) \ facility-ext
\G the @key{DEL} key on my xterm, not backspace
keycode k-enter  ( -- u ) \ gforth
\ only useful in combinations, but it is keycode+#lf

\ function/keypad keys
keycode k-f1  ( -- u ) \ facility-ext k-f-1
keycode k-f2  ( -- u ) \ facility-ext k-f-2
keycode k-f3  ( -- u ) \ facility-ext k-f-3
keycode k-f4  ( -- u ) \ facility-ext k-f-4
keycode k-f5  ( -- u ) \ facility-ext k-f-5
keycode k-f6  ( -- u ) \ facility-ext k-f-6
keycode k-f7  ( -- u ) \ facility-ext k-f-7
keycode k-f8  ( -- u ) \ facility-ext k-f-8
keycode k-f9  ( -- u ) \ facility-ext k-f-9
keycode k-f10 ( -- u ) \ facility-ext k-f-10
keycode k-f11 ( -- u ) \ facility-ext k-f-11
keycode k-f12 ( -- u ) \ facility-ext k-f-12

keycode k-winch ( -- u ) \ gforth
\G A key code that may be generated when the user changes the window size.
keycode k-pause ( -- u ) \ gforth
keycode k-mute  ( -- u ) \ gforth
keycode k-volup ( -- u ) \ gforth
keycode k-voldown ( -- u ) \ gforth
keycode k-backspace ( -- u ) \ gforth
keycode k-tab ( -- u ) \ gforth
keycode k-sel ( -- u ) \ gforth
keycode k-fgcolor ( -- u ) \ gforth
keycode k-bgcolor ( -- u ) \ gforth
\G keycode for Android selections
keycode k-eof ( -- u ) \ gforth
\ always the last gforth-specific keycode
drop

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

: inskey@ ( -- c )
    key-buffer $@ drop c@
    key-buffer 0 1 $del ;
: buf-key ( -- c )
    \ buffered key
    key-buffer $@len if
	inskey@
    else
	defers key-ior
    then ;
' buf-key is key-ior

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

: esc-mask ( addr u -- addr' u' mask )
    ';' $split dup IF
	#0. 2swap >number  2swap drop 1- 0 max >r
	[: 2swap 2dup 1 safe/string s" 1" str= + type type ;] $tmp
	r> 7 and mask-shift# lshift
	EXIT
    ELSE  2drop over c@ 'O' = IF
	    1 /string
	    #0. 2swap >number  2swap drop 1- 0 max >r
	    [: 'O' emit type ;] $tmp
	    r> 7 and mask-shift# lshift
	EXIT  THEN
    THEN
    0 ;

: clear-ekey-buffer ( -- )
    ekey-buffer $free ;

: esc-prefix ( -- u )
    BEGIN
	key? \ ?dup-0=-if  1 ms key?  endif \ workaround for Windows 1607 Linux
    WHILE
	    key ekey-buffer c$+!
	    ekey-buffer $@ ['] esc-mask #10 base-execute >r
	    esc-sequences search-wordlist
	    if
		execute r> or clear-ekey-buffer exit
	    endif
	    rdrop
    REPEAT
    ekey-buffer $@ unkeys #esc clear-ekey-buffer ;

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
    k-tab    k-shift-mask or s" [Z" esc-sequence
    k-tab    k-alt-mask   or s" x" over #tab swap c! esc-sequence

    k-enter  k-shift-mask or s" OM" esc-sequence
    k-enter  k-alt-mask or   s" x" over #cr swap c! esc-sequence
    k-enter  k-alt-mask or k-shift-mask or s" eOM" over #esc swap c! esc-sequence
    k-backspace k-alt-mask or   s" D" over #del swap c! esc-sequence

    k-f1      s" OP"  esc-sequence
    k-f2      s" OQ"  esc-sequence
    k-f3      s" OR"  esc-sequence
    k-f4      s" OS"  esc-sequence
    k-f5      s" [15~" esc-sequence
    k-f6      s" [17~" esc-sequence
    k-f7      s" [18~" esc-sequence
    k-f8      s" [19~" esc-sequence
    k-f9      s" [20~" esc-sequence
    k-f10     s" [21~" esc-sequence
    k-f11     s" [23~" esc-sequence
    k-f12     s" [24~" esc-sequence

    \ esc sequences from Linux console:

    k-f1       s" [[A" esc-sequence
    k-f2       s" [[B" esc-sequence
    k-f3       s" [[C" esc-sequence
    k-f4       s" [[D" esc-sequence
    k-f5       s" [[E" esc-sequence
    \ k-delete s" [3~" esc-sequence \ duplicate from above
    k-home   s" [1~" esc-sequence
    k-end    s" [4~" esc-sequence

    k-f1 k-shift-mask or s" [25~" esc-sequence
    k-f2 k-shift-mask or s" [26~" esc-sequence
    k-f3 k-shift-mask or s" [28~" esc-sequence
    k-f4 k-shift-mask or s" [29~" esc-sequence
    k-f5 k-shift-mask or s" [31~" esc-sequence
    k-f6 k-shift-mask or s" [32~" esc-sequence
    k-f7 k-shift-mask or s" [33~" esc-sequence
    k-f8 k-shift-mask or s" [34~" esc-sequence

    \ esc sequences for MacOS X iterm <e7a7c785-3bea-408b-94e9-4b59b008546f@x16g2000prn.googlegroups.com>
    k-left   s" OD" esc-sequence
    k-right  s" OC" esc-sequence
    k-up     s" OA" esc-sequence
    k-down   s" OB" esc-sequence

    k-pause   s" [P" esc-sequence
    k-mute    s" VM" esc-sequence
    k-volup   s" VU" esc-sequence
    k-voldown s" VD" esc-sequence

    k-sel     s" [S"  esc-sequence
    k-fgcolor s" ]10rgb:" esc-sequence
    k-bgcolor s" ]11rgb:" esc-sequence
set-current
[ENDIF]

[IFDEF] max-single-byte
    : read-xkey ( key -- flag )
	ekey-buffer c$+!
	[ pad 3 $80 fill pad 3 ] SLiteral
	ekey-buffer $+!
	ekey-buffer $@ x-size  1 ekey-buffer $!len 1 +do
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
    : xkey? ( -- flag ) \ xchar x-key-query
	key? dup if
	    drop key read-xkey ekey-buffer $@ unkeys
	    clear-ekey-buffer  then ;
[THEN]

0 Value ekey-rgb
: read-rgb ( -- )
    term-rgb$ $free
    BEGIN  key?  WHILE  key dup $07 <> WHILE  term-rgb$ c$+!
	REPEAT  drop  THEN
    term-rgb$ $@ string>rgb to ekey-rgb ;

Defer ekey-extension ' noop is ekey-extension

: ekey ( -- u ) \ facility-ext e-key
    \G Receive a keyboard event @var{u} (encoding implementation-defined).
    BEGIN  0 winch? atomic!@ 0= WHILE  key-ior dup EINTR = WHILE  drop  REPEAT
    ELSE
	k-winch  EXIT
    THEN
    dup EOK =
    [IFDEF] EBADF over EBADF = or [THEN]
    IF  drop k-eof  EXIT  THEN
    dup #esc =
    if
	drop esc-prefix
	dup k-fgcolor k-bgcolor 1+ within IF  read-rgb  THEN
	exit
    then
    ekey-extension
    [IFDEF] max-single-byte
	get-xkey
    [THEN]
    dup $89 = IF  drop k-tab k-alt-mask or  THEN
;

[IFDEF] max-single-byte
: ekey>char ( u -- u false | c true ) \ facility-ext e-key-to-char
    \G Convert keyboard event @var{u} into character @code{c} if
    \G possible.  Note that non-ASCII characters produce @code{false}
    \G from both @code{ekey>char} and @code{ekey>fkey}.  Instead of
    \G @code{ekey>char}, use @code{ekey>xchar} if available.
    dup max-single-byte u< ; \ k-left must be first!
: ekey>xchar ( u -- u false | xc true ) \ xchar-ext e-key-to-x-char
    \G Convert keyboard event @var{u} into xchar @code{xc} if
    \G possible.
    dup k-left u< ; \ k-left must be first!
: ekey>fkey ( u1 -- u2 f ) \ facility-ext e-key-to-f-key
\G If u1 is a keyboard event in the special key set, convert
\G keyboard event @var{u1} into key id @var{u2} and return true;
\G otherwise return @var{u1} and false.
    ekey>xchar 0= ;

' xkey? alias ekey? ( -- flag ) \ facility-ext e-key-question
[ELSE]
: ekey>char ( u -- u false | c true ) \ facility-ext e-key-to-char
    \G Convert keyboard event @var{u} into character @code{c} if possible.
    dup k-left u< ; \ k-left must be first!
: ekey>fkey ( u1 -- u2 f ) \ facility-ext
\G If u1 is a keyboard event in the special key set, convert
\G keyboard event @var{u1} into key id @var{u2} and return true;
\G otherwise return @var{u1} and false.
    ekey>char 0= ;

' key? alias ekey? ( -- flag ) \ facility-ext e-key-question
[THEN]

\ integrate ekey into line editor

' ekey is edit-key

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
