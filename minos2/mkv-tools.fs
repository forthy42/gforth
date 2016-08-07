\ Matroska tools

\ Copyright (C) 2014 Free Software Foundation, Inc.

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

require mini-oof2.fs
require unix/mmap.fs
require mts-tools.fs

\ Matroska is a "binary XML" markup language, called EBML

s" Invalid Id"        exception constant !!ebml-id!!
s" Invalid data size" exception constant !!ebml-ds!!
s" Early terminate scanning" exception constant !!ebml-early!!
s" Early termination of cue reading" exception Constant !!cueterm!!

: id-8x  24 rshift >r 1+  r> ;
: id-4x  16 rshift >r 2 + r> ;
: id-2x   8 rshift >r 3 + r> ;
: id-1x            >r 4 + r> ;
: id-00   !!ebml-ds!! throw ;
Create id@-table
' id-00 , ' id-1x , ' id-2x , ' id-2x ,
4 0 [DO] ' id-4x , [LOOP]
8 0 [DO] ' id-8x , [LOOP]

: id@+ ( addr -- addr' id )
    dup be-ul@ dup 28 rshift cells id@-table + perform ;

: track@+ ( addr -- addr' id )
    dup be-ul@
    dup $80000000 and IF  24 rshift     $80 xor >r 1+  r>  EXIT  THEN
    dup $40000000 and IF  16 rshift   $4000 xor >r 2 + r>  EXIT  THEN
    dup $20000000 and IF   8 rshift $200000 xor >r 3 + r>  EXIT  THEN
    dup $10000000 and IF          $10000000 xor >r 4 + r>  EXIT  THEN
    !!ebml-id!! throw ;

cell 8 = [IF]
    : ds-8x  56 rshift             $7F and >r 1+  r> ;
    : ds-4x  48 rshift           $3FFF and >r 2 + r> ;
    : ds-2x  40 rshift         $1FFFFF and >r 3 + r> ;
    : ds-1x  32 rshift        $FFFFFFF and >r 4 + r> ;
    : ds-08  24 rshift      $7FFFFFFFF and >r 5 + r> ;
    : ds-04  16 rshift    $3FFFFFFFFFF and >r 6 + r> ;
    : ds-02   8 rshift  $1FFFFFFFFFFFF and >r 7 + r> ;
    : ds-01            $FFFFFFFFFFFFFF and >r 8 + r> ;
    Create ds@-table
    ' id-00 , ' ds-01 , ' ds-02 , ' ds-02 ,
    4 0 [DO] ' ds-04 , [LOOP]
    8 0 [DO] ' ds-08 , [LOOP]
    $10 0 [DO] ' ds-1x , [LOOP]
    $20 0 [DO] ' ds-2x , [LOOP]
    $40 0 [DO] ' ds-4x , [LOOP]
    $80 0 [DO] ' ds-8x , [LOOP]
    : ds@+ ( addr -- addr' ddatasize )
	dup be-ux@ over c@ cells ds@-table + perform 0 ;
[ELSE]
    : ds@+ ( addr -- addr' ddatasize )
	dup be-ul@
	dup $80000000 and IF  24 rshift     $7F and >r 1+  r> 0 EXIT  THEN
	dup $40000000 and IF  16 rshift   $3FFF and >r 2 + r> 0 EXIT  THEN
	dup $20000000 and IF   8 rshift $1FFFFF and >r 3 + r> 0 EXIT  THEN
	dup $10000000 and IF          $0FFFFFFF and >r 4 + r> 0 EXIT  THEN
	dup $08000000 and IF          $07FFFFFF and >r 5 + r>
	    over 1 - be-ul@ swap 24 drshift EXIT  THEN
	dup $04000000 and IF          $03FFFFFF and >r 6 + r>
	    over 2 - be-ul@ swap 16 drshift EXIT  THEN
	dup $02000000 and IF          $01FFFFFF and >r 7 + r>
	    over 3 - be-ul@ swap  8 drshift EXIT  THEN
	dup $01000000 and IF          $00FFFFFF and >r 8 + r>
	    over 4 - be-ul@ swap            EXIT  THEN
	!!ebml-id!! throw ;
[THEN]

6 Constant NAL-SEI \ additional element on h.264

: nal@+ ( addr -- addr' value )
    0 >r  BEGIN  count dup $FF =  WHILE  r> + >r  REPEAT
    r> + ;

\ buffer in mkvs
\ to quickly read in mkvs, we let things overlap a bit, so that we can access tags
\ without having to reread.

$10000 Constant mkvslice# \ 64k ought to be enough for everybody,
$1000 Constant mkvoverlap# \ + 4k for sliding

object class
    field: mkvfd
    field: mkvsize \ used size of the buffer
    2field: mkvoff  \ offset into buffer
    mkvslice# mkvoverlap# + +field mkvbuf
end-class mkvbuf-c

: open-mkv ( addr u class -- mkv-o ) >r
    r/o open-file throw
    r> new >o mkvfd ! o o> ;

: close-mkv ( -- )
    mkvfd @ close-file dispose throw ;

: read-mkv ( -- )
    mkvsize @ 0= IF
	mkvbuf mkvslice# mkvoverlap# + mkvfd @ read-file throw
	mkvsize !  EXIT  THEN
    mkvbuf dup mkvsize @ mkvoverlap# - 0 max + swap mkvoverlap# move
    mkvbuf mkvsize @ mkvoverlap# min dup >r + mkvslice# mkvfd @ read-file throw
    r> + mkvsize ! ;

: large-skip ( addr n -- addr' )
    \ ." Large skip: " dup . cr
    0 over mkvoverlap# + over mkvoff 2+!
    mkvfd @ file-position throw d+ mkvfd @ reposition-file throw
    drop mkvbuf mkvslice# + mkvsize off ;

: reread-mkv ( addr -- addr' )
    BEGIN  dup mkvbuf mkvslice# + u>= WHILE
	    dup mkvbuf mkvslice# + mkvoverlap# + - dup 0>
	    IF  large-skip  ELSE  drop  THEN
	    read-mkv mkvslice# dup 0 mkvoff 2+! -
    REPEAT ;

: seek-mkv ( d -- )
    2dup mkvoff 2! mkvfd @ reposition-file throw mkvsize off  read-mkv ;

table constant mkv-table

4 buffer: id-name

: tab #tab emit ;

mkvbuf-c class 
    method do-master
    method do-binary
    method do-uint
    method do-int
    method do-float
    method do-date
    method do-utf8
    method do-string
end-class mkv-doers

' drop mkv-doers is do-master
' drop mkv-doers is do-binary
' drop mkv-doers is do-uint
' drop mkv-doers is do-int
' drop mkv-doers is do-float
' drop mkv-doers is do-date
' drop mkv-doers is do-utf8
' drop mkv-doers is do-string

: .mkvname ( body -- ) cell- @ .name ;

: :+ ( n -- n' )  -1 xor ;

Variable tag#  tag# off

: id>string ( id -- addr u )
    id-name be-l! id-name 4 ;

Vocabulary mkv-tags

: tag: ( level id default-xt "name" -- ) -rot
    id>string nextname
    get-current mkv-table set-current Create 
    [: dup cell+ swap perform ;] set-does> set-current
    here >r 0 ,
    tag# @ ,  cell tag# +!
    dup 0< IF  -1 xor 2* 1+  ELSE  2*  THEN ,
    >r method r> , lastxt r> ! ;

: master:  ['] do-master tag: ;
: uint:    ['] do-uint   tag: ;
: int:     ['] do-int    tag: ;
: binary:  ['] do-binary tag: ;
: string:  ['] do-string tag: ;
: utf8:    ['] do-utf8   tag: ;
: float:   ['] do-float  tag: ;
: date:    ['] do-date   tag: ;

mkv-doers class
    also mkv-tags definitions
    require mkv-tags.fs
    previous definitions
end-class mkv-tag-c

: default-methods ( xt -- )
    >body 2@ mkv-tag-c + ! ;
' mkv-tags >body ' default-methods map-wordlist

mkv-tag-c class end-class mkv-dump

Variable mkvlevel

\ dump stuff

: @uint ( addr u -- u ) 0 -rot
    bounds ?DO  8 lshift I c@ or  LOOP ;
: @int ( addr u -- n )  over c@ dup $80 and negate or -rot
    1 /string bounds ?DO  8 lshift I c@ or  LOOP ;
Variable (float<>)
: @float ( addr u -- r ) 8 = IF  be-uxd@ (float<>) xd! (float<>) df@
    ELSE  be-ul@ (float<>) l! (float<>) sf@  THEN ;

: .binary ( addr u body -- addr u ) .mkvname tab 2dup .dump8 cr ;
: .uint ( addr u body -- addr u ) .mkvname tab 2dup @uint . cr ;
: .int ( addr u body -- addr u ) .mkvname tab 2dup @int . cr ;
: .date ( addr u body -- addr u ) .mkvname tab 2dup @uint
    0 <# 9 0 DO # LOOP '.' hold #s #> type cr ;
: .float ( addr u body -- addr u )  .mkvname tab
    2dup @float  f. cr ;
: .utf8 ( addr u body -- addr u )  .mkvname tab '"' emit 2dup type '"' emit cr ;
: .simpleblock ( addr u body -- addr u ) .mkvname tab
    over track@+ . be-w@+ dup $8000 and negate or . count hex.
    >r 2dup over r> swap - /string .dump8 cr ;

' .binary mkv-dump to do-binary
' .uint   mkv-dump to do-uint
' .int    mkv-dump to do-int
' .float  mkv-dump to do-float
' .date   mkv-dump to do-date
' .utf8   mkv-dump to do-utf8
' .utf8   mkv-dump to do-string
also mkv-tags
' .simpleblock mkv-dump to SimpleBlock
previous

: mkv-exec ( addr u id -- addr u )
    id>string mkv-table search-wordlist
    0= IF  !!ebml-id!! throw  THEN
    execute ;

: dhex. ( d -- ) [: '$' emit d. ;] $10 base-execute ;

: mkv-section ( addr u xt -- ) { xt } 1 mkvlevel +! over + >r
    BEGIN
	dup r@ u< mkvsize @ 0> and  WHILE
	    xt execute
	    id@+ >r  ds@+ drop  r>  mkvoff 2@ 2>r
	    mkv-exec
	    + mkvoff 2@ 2r> d- drop r> over - >r -
	    dup reread-mkv tuck - ?dup-if r> swap - >r then
    REPEAT  drop rdrop  -1 mkvlevel +! ;

: addr>pos ( addr -- pos ) mkvbuf - 0 mkvoff 2@ d+ ;

: .mheader ( addr -- addr )
    mkvlevel @ spaces dup addr>pos dhex. ." : "
    dup id@+ hex. ds@+ dhex. drop ;
: .master ( addr u body -- addr u ) .mkvname cr
    2dup ['] .mheader mkv-section ;
' .master mkv-dump to do-master

: start-mkv ( addr u class -- buf size o ) mkvlevel on open-mkv >o
    read-mkv mkvbuf mkvfd @ file-size throw drop o o> ;

: dump-mkv ( addr u -- ) mkv-dump start-mkv >o
    ['] .mheader mkv-section 2drop close-mkv o> ;

\ read in structure for easy accessibility

\ MTS coverter stuff

mkv-tag-c class
    field: seeks#
    field: seeks
    field: cue-tags
    field: tc-scale
    field: mkv-h
    field: mkv-w
    field: track#
    field: track-type
    field: sample-frequency
    field: audio-channels
    field: default-duration
    ffield: total-duration
    field: codec
    field: codec-private
    field: lang
    field: lang-name
    field: time-code
    ffield: dts
end-class mkv-file

2Variable tag-pos
2Variable seek-pos
: !offset ( addr -- ) dup addr>pos tag-pos 2! ;

\ just simply recurse into stuff
: mkv-recurse ( addr u body -- addr u )  drop 2dup ['] !offset mkv-section ;
' mkv-recurse  mkv-file to do-master

also mkv-tags

3 cells buffer: seektag \ tag, dposition
4 cells buffer: cuetag \ time, track, dposition

: >seekpos ( addr u tag -- dseek ) { tag } bounds ?DO
	I @ tag = IF  I cell+ 2@  unloop  EXIT  THEN
    3 cells +LOOP  -1. ;

\ seekhead will terminate after doing the head
:noname  tag-pos 2@ seek-pos 2!
    mkv-recurse nothrow !!ebml-early!! throw ; mkv-file to SeekHead
\ segment will catch early termination
:noname  mkvlevel @ >r ['] mkv-recurse catch r> mkvlevel !
    dup !!ebml-early!! = IF  2drop noThrow
	\ we want to run all the seeks
	\ those are the meta informations
	seeks# @ seeks $[]@ bounds ?DO
	    I cell+ 2@ seek-mkv
	    mkvbuf id@+ >r ds@+ drop r>
	    dup $114D9B74 <> IF  mkv-exec  ELSE  drop  THEN  2drop
	3 cells +LOOP
	1 seeks# +!
    ELSE  throw  THEN ;
mkv-file to Segment
:noname  drop 2dup @uint seektag ! ;
mkv-file to SeekID
:noname  drop 2dup @uint 0 seek-pos 2@ d+ seektag cell+ 2!
    seektag 3 cells seeks# @ seeks $[]
    dup @ IF  $+!  ELSE  $!  THEN ; mkv-file to SeekPosition

cell 8 = [IF] ' df! [ELSE] ' sf! [THEN] alias cf!
cell 8 = [IF] ' df@ [ELSE] ' sf@ [THEN] alias cf@

\ info storing
' mkv-recurse mkv-file to Info
:noname drop 2dup @uint tc-scale ! ; mkv-file to TimecodeScale
\ track storing
' mkv-recurse mkv-file to Tracks
' mkv-recurse mkv-file to TrackEntry
' mkv-recurse mkv-file to Video
' mkv-recurse mkv-file to Audio
' mkv-recurse mkv-file to Tags
' mkv-recurse mkv-file to Tag
' mkv-recurse mkv-file to SimpleTag
:noname drop 2dup @uint track# @ default-duration $[] ! ; mkv-file to DefaultDuration
:noname drop 2dup @float tc-scale @ fm* 1e-9 f* total-duration f! ; mkv-file to Duration
:noname drop 2dup @uint mkv-w ! ; mkv-file to PixelWidth
:noname drop 2dup @uint mkv-h ! ; mkv-file to PixelHeight
:noname drop 2dup @uint track# ! ; mkv-file to TrackNumber
:noname drop 2dup track# @ codec $[]! ; mkv-file to CodecID
:noname drop 2dup track# @ codec-private $[]! ; mkv-file to CodecPrivate
:noname drop 2dup track# @ lang $[]! ; mkv-file to Language
:noname drop 2dup track# @ lang-name $[]! ; mkv-file to Name
:noname drop 2dup @float track# @ sample-frequency $[] cf! ; mkv-file to SamplingFrequency
:noname drop 2dup @uint track# @ track-type $[] ! ; mkv-file to TrackType
:noname drop 2dup @uint track# @ audio-channels $[] ! ; mkv-file to Channels

\ cue storing
' mkv-recurse mkv-file to Cues
' mkv-recurse mkv-file to CuePoint
' mkv-recurse mkv-file to CueTrackPositions
:noname drop 2dup @uint cuetag ! ; mkv-file to CueTime
:noname drop 2dup @uint cuetag cell+ ! ; mkv-file to CueTrack
:noname drop 2dup @uint 0 seek-pos 2@ d+ cuetag 2 cells + 2!
    cuetag 4 cells cue-tags $+! ; mkv-file to CueClusterPosition

\ cluster handling

' mkv-recurse mkv-file to Cluster
:noname drop 2dup @uint dup time-code !
    1e-9 fm* tc-scale @ fm* dts f! ; mkv-file to TimeCode

: >pts ( ticks -- ftime ) time-code @ + 1e-9 fm* tc-scale @ fm* ;
Variable random-access

: fill-mts ( addr end pid -- addr' ) { pid }
    >r  BEGIN  r@ over - fillup  WHILE
	    ts-full? IF  tsflush pid pid,  THEN
	    dup reread-mkv tuck - ?dup-if r> swap - >r then
    REPEAT  drop r> ;
: string>mts ( addr u pid -- ) { pid }
    over + >r  BEGIN  r@ over - fillup  WHILE
	    ts-full? IF  tsflush pid pid,  THEN
    REPEAT  drop rdrop ;
: nal-fill-mts ( addr end pid -- addr' ) { pid }
    >r BEGIN  dup r@ u<  WHILE
	"\0\0\0\1" pid string>mts
	    dup be-ul@ >r 4 + r> over + tuck pid fill-mts
	    tuck - ?dup-if r> swap - >r then
    REPEAT  rdrop ;
: sei-len ( -- n )  0
    1 codec-private $[]@ 6 safe/string  bounds ?DO
    4 +  I be-uw@ tuck + swap 3 + +LOOP ;
: sei? ( addr -- flag ) 4 + c@ $1F and $06 = ;
: ?sei ( addr pid -- addr' ) { pid }
    dup sei? IF  \ ."  SEI found "
	1 codec-private $[]@ 6 safe/string  bounds ?DO
	    "\0\0\0\1" pid string>mts
	    I be-uw@ I 2 + over pid string>mts
	3 + +LOOP
    THEN ;

0.2e FConstant pts+
0.0e FConstant dts+
0.0e FConstant pcr+

\ AAC header stuff

Create aac-template \ template
$FF c, $F1 c, \ protection absent
$4C c, $80 c, \ profiles+sample rate
$00 c, $1F c, $FC c, \ length+vbr
0 value aac-flag
Create aac-rates
96000 , 88200 , 64000 , 48000 , 44100 , 32000 , 24000 , 22050 ,
16000 , 12000 , 11025 , 8000 , 7350 , 0 , 

: >aac-rate ( rate -- n )
    aac-rates >r  BEGIN  dup r@ @ u<  WHILE  r> cell+ >r  REPEAT
    drop r> aac-rates - cell/ ;

: aac-init ( -- )
    2 codec $[]@ "A_AAC" str= to aac-flag aac-flag IF
	2 sample-frequency $[] cf@ f>s >aac-rate 10 lshift $4000 or
	2 audio-channels $[] @ dup 8 = + 6 lshift or aac-template 2 + be-w!
    THEN ;

: aac-header ( len -- addr u )
    7 + dup 5 lshift $1F or aac-template 5 + c!
    3 rshift aac-template 4 + c!
    aac-template 7 ;

\ Variable vidcnt

: .video ( addr end time-off -- addr' ) >pts ( 'v' emit )
    $100 pid, \ vidcnt @ 1 and 0= IF
    fdup pcr+ f+
    random-access @ IF  $50  ELSE  $10  THEN  afield, \ THEN
    dts f@ dts+ f+ fswap pts+ f+ 2dup swap -  $D +
    2 pick sei? IF  sei-len +  THEN
    2 pick be-ul@ 2 <> IF   6 +  THEN  0 video,
    over be-ul@ 2 <> IF  $00000001 ts-l, $09F0 ts-w,  THEN
    >r $100 ?sei r>
    $100 nal-fill-mts packup
    1e-9 1 default-duration $[] @ fm* dts f@ f+ dts f!
    random-access off ( 1 vidcnt +! ) ;
: .audio ( addr end time-off -- addr' ) >pts ( 'a' emit )
    $101 pid, pcr+ f+ $40 afield, pts+ f+ 2dup swap - 8 +
    aac-flag IF  2 pick be-uw@ $FFF1 <> dup >r 7 and + 0 audio,
	r> IF  2dup - negate aac-header ts-data,  THEN
    ELSE  0 audio,  THEN
    $101 fill-mts packup ;

:noname drop 2dup + { end }
    over track@+ { track }
    be-w@+  dup $8000 and negate or { time-off }
    count drop \ discardable does not interest us
    track 1 = IF  end time-off .video  THEN
    track 2 = IF  end time-off .audio  THEN
    drop ; mkv-file to SimpleBlock

previous

: .seekseg ( addr u -- ) bounds ?DO
	." ID: " I @ hex.  ." Seek: " I cell+ 2@ dhex. cr
    3 cells +LOOP ;

: .seeks ( -- )
    seeks $[]# 0 ?DO  I seeks $[]@ .seekseg cr LOOP ;
: .cues ( -- )
    cue-tags $@ bounds ?DO
	." Time: " I @ . ." Track: " I cell+ @ . ." Pos: " I 2 cells + 2@ dhex. cr
    4 cells +LOOP ;

: .mkv-info ( -- )
    ." W*H: " mkv-w ? mkv-h ? ." tick: " tc-scale ?
    cr track# @ 1+ 1 ?DO
	." Codec: '" I codec $[]@ type
	." ' Lang: '" I lang $[]@ type
	." ' TrackType: " I track-type $[] ?
	."  SampleF: " I sample-frequency $[] cf@ f.
	." fps: " 1e9 I default-duration $[] @ fm/ f.
	."  Name: " I lang-name $[]@ type
	."  CodecPrivate: " I codec-private $[]@ .dump8 cr
    LOOP ;

: new-mkv-file ( addr u -- ) mkv-file start-mkv >o
    s" " cue-tags $!
    ['] !offset mkv-section o o> ;

: seeks-mkv ( addr u -- )  new-mkv-file >o
    .seeks .cues .mkv-info close-mkv o> ;

FVariable mkv-time-off

: >mkv-time ( r1 -- r2 )
     1e9 tc-scale @ fm/ f* ;

: >cue ( r% -- i ) \ find closest cue
    total-duration f@ >mkv-time f* f>s  0e mkv-time-off f!
    cue-tags $@ drop cue-tags $@ bounds ?DO
	over I @ - abs >r 2dup @ - abs r>
	> IF  drop I  I @ s>f 1e >mkv-time f/ mkv-time-off f!  THEN
    4 cells +LOOP  nip cue-tags $@ drop - 4 cells / ;

$1F43B675 Constant cluster-id

: read-cluster ( addr -- addr' )
    id@+ >r ds@+ drop r>
    dup cluster-id = IF
	mkvoff 2@ 2>r  mkv-exec
	+ mkvoff 2@ 2r> d- drop -
    ELSE  drop + reread-mkv  THEN ;

: read-cue { cue end -- }
    sdtflush ppflush
    cue 2 cells + 2@ seek-mkv  mkvbuf  random-access on
    BEGIN
	read-cluster
	cue 4 cells + end u<  IF  dup addr>pos  cue 6 cells + 2@ du>=
	ELSE  dup id@+ nip cluster-id <>  THEN
    UNTIL  drop ;

0 value cues>mts-run?

: cues>mts-loop ( index -- )  true to cues>mts-run?
    cue-tags $@ rot 2* 2* cells safe/string bounds ?DO
	( '.' emit ) I I' read-cue
    4 cells +LOOP cr false to cues>mts-run? ;

Variable first-cue

: cues>mts ( index -- )  first-cue !  cues>mts-run?
    IF  nothrow !!cueterm!! throw  THEN
    BEGIN  first-cue @ ['] cues>mts-loop catch dup !!cueterm!! <> and throw
    nothrow cues>mts-run? 0= UNTIL ;

: codec-init ( -- vcodec acodec )
    1 codec $[]@ "V_MPEG4/ISO/AVC" str= IF $1B ELSE 0 THEN
    2 codec $[]@ "A_AAC" str= IF  $0F  aac-init  ELSE
	2 codec $[]@ "A_MPEG/L3" str= IF $03 ELSE
	    ." unknown codec: " 2 codec $[]@ type cr
	    0
	THEN
    THEN ;

: mkv2mts ( addr-mkv u-mkv addr-mts u-mts -- )  create-mts
    new-mkv-file >o
    .mkv-info
    sdt, pat,
    codec-init pnt,
    0 cues>mts
    close-mkv o> ;

\ buffer transport between task and callback

require unix/pthread.fs
require unix/mmap.fs

2Variable queue-io   \ pointers, strictly advancing
2Variable queue-buf  \ addr+size for queue written to
Variable queue-used  \ queue is used
0 Value cue-task

: new-queue ( -- )  0. queue-buf 2!  0. queue-io 2! ;

event: ->read-mkv ( addr u -- )  queue-buf 2! ;

: wait-for-write ( -- )
    BEGIN   queue-buf @ 0= WHILE  stop  REPEAT ;
: packet>queue ( addr u -- addr' u' )
    2dup queue-buf 2@ rot umin dup >r move
    r@ safe/string queue-buf 2@ r@ /string queue-buf 2!
    r> queue-io +! ;
: push-queue ( addr u -- )
    BEGIN  packet>queue dup WHILE
	    wait-for-write  REPEAT  2drop ;

: wait-for-read ntime 60000000. d+ { u d: deadline -- u }
    BEGIN  queue-io 2@ -  WHILE  pause  deadline ntime d- d0<  UNTIL  THEN
    u queue-buf @ - ;
: pull-queue ( addr u -- u )
    dup queue-io cell+ +!
    dup >r <event e$, ->read-mkv cue-task event>
    r> wait-for-read ;

: fill-mts-buf ( -- )
    BEGIN  queue-buf @  WHILE  sdtflush  REPEAT ;

: cue-converter ( -- ) cue-task ?EXIT  new-queue
    stacksize4 newtask4 dup to cue-task activate
    ['] push-queue ts-writer ! 0 >o
    BEGIN  stop  fill-mts-buf  AGAIN o> ;

0 Value cue-cont?
0 Value mkv-file-o
event: ->open-mkv ( addr u -- ) new-mkv-file >o rdrop
    o to mkv-file-o sdt, pat, codec-init pnt, ;
event: ->close-mkv ( -- )  close-mkv 0 >o rdrop ;
event: ->cues ( index -- )  cues>mts ;
event: ->cue-abort ( -- )  false to cues>mts-run?
    nothrow !!cueterm!! throw ;
event: ->cue-pause ( -- )  false to cue-cont? BEGIN  stop cue-cont?  UNTIL ;
event: ->cue-cont ( -- ) true to cue-cont? ;

0 Value mts-fd

: convert-mkv ( addr u addrmts umts -- )  r/w create-file throw to mts-fd
    cue-converter
    <event e$, ->open-mkv 0 elit, ->cues cue-task event>
    BEGIN  pad /packet 128 * pull-queue pad over mts-fd write-file throw
    0= UNTIL ;
