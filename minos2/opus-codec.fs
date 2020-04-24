\ Opus codec for PulseAudio driver

\ Authors: Bernd Paysan
\ Copyright (C) 2020 Free Software Foundation, Inc.

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

require unix/opus.fs

also opus

\ Opus en/decoder

: opus-encoder ( -- encoder ) { | w^ err }
    #48000 1 OPUS_APPLICATION_AUDIO err opus_encoder_create ;
: opus-decoder ( -- decoder ) { | w^ err }
    #48000 1 err opus_decoder_create ;

opus-encoder Value opus-enc
opus-decoder Value opus-dec

"test.opus" r/w create-file throw Value rec-file
"test.idx" r/w create-file throw Value index-file

Variable write$
Variable idx$
Variable write-opus
Variable read-opus

: >amplitude ( addr u -- avgamp )
    0 -rot bounds ?DO
	I sw@ abs max
    2 +LOOP ;

\ index file for fast seeking:
\ One block per second. Block format:
\ Magic | 8b channels | 8b subblocks# | 16b samples/subblock | 64b index |
\ { 16b amplitude | 16b len }*

begin-structure idx-head
    4 +field idx-magic
    cfield: idx-channels
    cfield: idx-frames
    wfield: idx-samples
    8 +field idx-pos
end-structure

begin-structure idx-tuple
    wfield: idx-amp
    wfield: idx-len
end-structure

frames/s 4 * $10 + Constant /idx-block

: write-idx ( -- )
    idx$ $@ /idx-block umin index-file write-file throw
    idx$ $free ;
: w$+! ( value addr -- )  2 swap $+!len le-w! ;
: >idx-frame ( dpos -- )
    "Opus"   idx$ $!
    1        idx$ c$+!
    frames/s idx$ c$+!
    #480     idx$ w$+!
    8 idx$ $+!len le-xd! ;
: >idx-block ( len amp -- )
    idx$ w$+!
    idx$ w$+! ;

:noname ( addr u -- )
    write$ $+!
    BEGIN
	write$ $@len #480 2* u>= WHILE
	    #480 2* write-opus $!len
	    opus-enc write$ $@ drop #480 2*
	    2dup >amplitude >r write-opus $@ opus_encode write-opus $!len
	    idx$ $@len 0= IF
		rec-file file-position throw >idx-frame
	    THEN
	    write-opus $@len r> >idx-block
	    write-opus $@ rec-file write-file throw
	    idx$ $@len /idx-block u>= IF  write-idx  THEN
	    write$ 0 #480 2* $del
    REPEAT ; is write-record

0 Value play-file
0 Value play-idx
Variable idx-block
0 Value idx-pos#
Variable opus-out

: $alloc ( u string -- addr u )
    over >r $+!len r> ;

: ?opus-ior ( n -- n )
    dup 0< IF  [: opus_strerror pa-error$ place
	    pa-error$ "error \ "
	    ! -2  throw ;] do-debug
    THEN ;
: read-idx-block ( -- )
    $10 idx-block $!len
    idx-block $@ play-idx read-file throw drop
    idx-block $@ drop idx-frames c@ 4 * idx-block $alloc
    play-idx read-file throw drop
    idx-block $@ drop idx-pos le-uxd@
    play-file reposition-file throw ;
: read-opus-block ( n -- )
    4 * idx-block $@ drop idx-head + + idx-len w@ read-opus $!len
    read-opus $@ play-file read-file throw drop ;

Variable opus-blocks
Semaphore opus-block-sem

: dec-opus-block ( -- ) { | w^ opus-buffer }
    #480 12 * 2 * opus-buffer $!len
    opus-dec read-opus $@ opus-buffer $@ 2/ 0 opus_decode ?opus-ior
    opus-buffer $!len
    opus-buffer $@len 0= IF  opus-buffer $free
    ELSE
	opus-buffer @ [: opus-blocks >stack ;] opus-block-sem c-section
    THEN ;

0 Value opus-task

: opus-block-task ( -- )
    stacksize4 NewTask4 to opus-task
    opus-task activate   debug-out debug-vector !
    BEGIN
	opus-blocks $[]# 2 > IF  stop  THEN
	idx-pos# frames/s mod dup 0= IF
	    read-idx-block
	THEN
	read-opus-block read-opus $@len  WHILE
	dec-opus-block
	1 +to idx-pos#
    REPEAT  0 to opus-task ;
	    
:noname ( -- buf )
    [: opus-blocks back> ;] opus-block-sem c-section
    opus-task wake
; is read-record
: open-play ( addr-play u addr-idx u -- )
    r/o open-file throw to play-idx
    r/o open-file throw to play-file
    opus-block-task ;

previous
