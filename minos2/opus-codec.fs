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
require unix/pthread.fs

also opus

debug: opus( \ )
\ +db opus( \ )

\ Opus en/decoder

2 Value channels
[IFUNDEF] sample-rate  #48000 Value sample-rate    [THEN]
[IFUNDEF] samples/frame  #960 Value samples/frame  [THEN]

: opus-encoder ( channels -- encoder ) { | w^ err }
    sample-rate swap OPUS_APPLICATION_AUDIO err opus_encoder_create ;
: opus-decoder ( -- decoder ) { | w^ err }
    sample-rate swap err opus_decoder_create ;

1 opus-encoder Value opus-mono-enc
1 opus-decoder Value opus-mono-dec
2 opus-encoder Value opus-stereo-enc
2 opus-decoder Value opus-stereo-dec

0 Value rec-file
0 Value rec-idx

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
\ Magic | 8b channels | 8b frames | 16b samples/frame | 64b index |
\ { 6b amplitude | 10b len }16b*

begin-structure idx-head
    4 +field idx-magic
    cfield: idx-channels
    cfield: idx-frames
    wfield: idx-samples
    8 +field idx-pos
end-structure

sample-rate samples/frame / 2* $10 + Constant /idx-block

: write-idx ( -- )
    idx$ $@ /idx-block umin rec-idx write-file throw
    idx$ $free ;
: w$+! ( value addr -- )  2 swap $+!len le-w! ;
: xd$+! ( dvalue addr -- ) 8 swap $+!len le-xd! ;
: >idx-frame ( dpos -- )
    "Opus"   idx$ $!
    channels idx$ c$+!
    sample-rate samples/frame / idx$ c$+!
    samples/frame idx$ w$+!
    idx$ xd$+! ;
: >idx-block ( len amp -- )
    idx$ $@len 0= IF
	rec-file flush-file throw
	rec-file file-position throw >idx-frame
    THEN
    $7FFF min 2* $FC00 and or idx$ w$+! ;

[IFUNDEF] write-record
    Defer write-record
[THEN]

:noname ( addr u -- )
    write$ $+!  samples/frame 2* channels * { bytes }
    BEGIN
	write$ $@len bytes u>= WHILE
	    bytes write-opus $!len
	    opus-mono-enc opus-stereo-enc channels 1 = select
	    write$ $@ bytes umin
	    2dup >amplitude >r
	    2/ channels / write-opus $@ opus_encode write-opus $!len
	    write-opus $@len r> >idx-block
	    write-opus $@ rec-file write-file throw
	    idx$ $@len /idx-block u>= IF  write-idx  THEN
	    write$ 0 bytes $del
    REPEAT ; is write-record

0 Value play-file
Variable idx-block  \ in memory index
0 Value idx-pos#
Variable play-block \ in memory file
0 Value play-pos#
Variable opus-out

: $alloc ( u string -- addr u )
    over >r $+!len r> ;

[IFDEF] exceptions
    :noname negate opus_strerror ; #10 exceptions \ really only 7, but allow more
    >r : ?opus-ior ( n -- )
	dup 0< IF  [ r> ]L + throw  THEN drop ;
[ELSE]
    $100 buffer: opus-error$
    : ?opus-ior ( n -- n )
	dup 0< IF  [: opus_strerror opus-error$ place
		opus-error$ "error ( " ( meh ) ! -2  throw ;] do-debug
	THEN ;
[THEN]

: in-idx-block ( pos -- addr len )
    idx-block $@ rot safe/string ;
: >idx-pos ( block -- )
    in-idx-block $10 u>= IF
	idx-pos le-uxd@ 
	play-file IF
	    play-file reposition-file throw
	ELSE
	    drop to play-pos#
	THEN
    ELSE  drop  THEN ;
: read-idx-block ( -- frame-size )
    idx-block $@len 0= IF  0 EXIT  THEN
    idx-pos# idx-block $@ drop idx-frames c@ dup >r /mod
    r> 2* idx-head + *
    over 0= IF  dup >idx-pos  THEN
    swap 2* + idx-head +
    in-idx-block 2 u>= IF  le-uw@  ELSE  drop 0  THEN
    1 +to idx-pos# ;
: read-opus-block ( frame-size -- )
    $3FF and  play-file IF
	read-opus $!len
	read-opus $@ play-file read-file throw drop
    ELSE
	>r play-block $@ play-pos# safe/string
	r> umin dup +to play-pos# read-opus $!
    THEN ;
: /frame ( -- u )
    idx-block $@ $10 u>= IF
	dup idx-channels c@ swap idx-samples le-uw@ * 2*
    ELSE  drop 0  THEN ;
: /sample ( -- u ) idx-block $@ $10 u>= IF
	idx-channels c@ 2*  ELSE  drop 0  THEN ;

Semaphore opus-block-sem

Variable opus-mono-blocks
[IFDEF] opensles
    opus-block-sem ,
    0 ,
    0 ,
[THEN]
Variable opus-stereo-blocks
[IFDEF] opensles
    opus-block-sem ,
    0 ,
    0 ,
[THEN]

: opus-blocks ( -- addr )
    opus-mono-blocks opus-stereo-blocks /sample 2 = select ;

: dec-opus-block ( -- ) { | w^ opus-buffer }
    /frame 3 * opus-buffer $!len
    opus-mono-dec opus-stereo-dec /sample 2 = select
    read-opus $@ opus-buffer $@ 2/ 0 opus_decode dup 0>= IF
	/sample * opus-buffer $!len
    ELSE
	drop /frame opus-buffer $!len
	opus-buffer $@ erase
    THEN
    opus-buffer $@len 0= IF  opus-buffer $free
    ELSE
	opus( ." push opus buffer" cr )
	opus-buffer @ [: opus-blocks >stack ;] opus-block-sem c-section
    THEN ;

0 Value opus-task

: 1-opus-block ( -- )
    read-idx-block read-opus-block
    read-opus $@len  IF  dec-opus-block
    ELSE  "" $make [: opus-blocks >stack ;] opus-block-sem c-section  THEN ;

8 Value max-queue#

: opus-block-task ( -- )
    stacksize4 NewTask4 to opus-task
    opus-task activate   debug-out debug-vector !  nothrow
    [: BEGIN
	    1-opus-block
	    opus-blocks $[]# max-queue# >= IF  stop  THEN
	read-opus $@len 0= UNTIL ;] catch ?dup-IF  DoError  THEN
    0 to opus-task ;

: read-opus-buf ( -- buf )
    [: opus-blocks back> ;] opus-block-sem c-section
    opus-task ?dup-IF  wake  THEN ;

[IFDEF] pulse-exec##
    : stream@ ( -- stream )
	case /sample
	    2 of  mono-play    endof
	    4 of  stereo-play  endof
	    0 swap endcase ;
[THEN]

: opus-go ( -- )
    opus-task ?dup-IF  wake  ELSE  opus-block-task  THEN ;

: start-play ( -- )
    0 to idx-pos#  0 to play-pos#  opus-go
    [IFDEF] pulse-exec##
	stream@ ?dup-IF
	    resume-stream  rdrop
	ELSE
	    sample-rate ['] read-opus-buf dup
	    ['] play-mono ['] play-stereo
	    /sample 2 = select
	    pulse-exec##
	THEN
    [ELSE] \ opensles based?
	[IFDEF] opensles
	    sles-play epipew opus-task 's @ opus-blocks 2 cells + 2!
	    sample-rate opus-blocks ['] read-opus-buf
	    /sample 2 = IF  play-mono  ELSE  play-stereo  THEN
	[THEN]
    [THEN] ;

[IFUNDEF] pause-play
    : pause-play ( -- )
	[IFDEF] pulse-exec##
	    stream@ ?dup-IF  ['] pause-stream pulse-exec#  THEN
	    opus-task ?dup-IF  halt  THEN
	[THEN] ;
[THEN]
[IFUNDEF] resume-play
    : resume-play ( -- )
	[IFDEF] pulse-exec##
	    stream@ ?dup-IF  opus-go  ['] resume-stream pulse-exec#  THEN
	[THEN] ;
[THEN]

: discard-opus-blocks ( -- )
    [:  opus-blocks get-stack 0 ?DO  { | w^ buf } buf $free  LOOP
	opus-blocks $free ;]
    opus-block-sem c-section ;
    
: open-play ( addr-play u addr-idx u -- )
    opus-task ?dup-IF  halt discard-opus-blocks  THEN
    [IFDEF] pulse-exec#
	stream@ ?dup-IF  ['] flush-stream pulse-exec#  THEN
    [THEN]
    idx-block $slurp-file
    play-block $slurp-file \ r/o open-file throw to play-file
    start-play ;
: open-play+ ( addr u -- ) { | w^ play$ w^ idx$ }
    2dup play$ $! ".opus" play$ $+!
    idx$ $! ".aidx" idx$ $+!
    play$ $@ idx$ $@ open-play
    play$ $free idx$ $free ;
: open-rec ( addr-rec u addr-idx u -- )
    w/o create-file throw to rec-idx
    w/o create-file throw to rec-file ;
: open-rec+ ( addr u -- ) { | w^ rec$ w^ idx$ }
    2dup rec$ $! ".opus" rec$ $+!
    idx$ $! ".aidx" idx$ $+!
    rec$ $@ idx$ $@ open-rec
    rec$ $free idx$ $free ;
: close-rec ( -- )
    write-idx rec-idx close-file rec-file close-file
    0 to rec-idx  0 to rec-file
    throw throw ;
[IFDEF] mono-rec
    : close-rec-mono ( -- )
	[:  mono-rec flush-stream
	    mono-rec pa_stream_disconnect ?pa-ior
	    mono-rec pa_stream_unref  0 to mono-rec
	;] pulse-exec 10 ms close-rec ;
[THEN]
[IFDEF] stereo-rec
    : close-rec-stereo ( -- )
	[:  stereo-rec flush-stream
	    stereo-rec pa_stream_disconnect ?pa-ior
	    stereo-rec pa_stream_unref  0 to stereo-rec
	;] pulse-exec 10 ms close-rec ;
[THEN]
[IFDEF] drain-record
    : close-rec-mono ( -- )  drain-record close-rec ;
    synonym close-rec-stereo close-rec-mono
[THEN]

: raw>opus ( addr u -- ) { | w^ raw$ }
    2dup open-rec+ raw$ $! ".raw" raw$ $+!
    raw$ $@ write$ $slurp-file
    "" write-record close-rec raw$ $free ;

previous

0 warnings !@
: bye ( -- )
    opus-task ?dup-IF
	opus-task kill  0 to opus-task  5 ms
    THEN  bye ;
warnings !
