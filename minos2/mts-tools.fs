\ mpeg transport stream tool
\ The MPEG transport stream consists of a 4 byte header with 188 bytes of data
\ optinally preceeded by a 4 bytes time stamp, which is then m2ts
\ all this is big endian

[ifundef] ts-fd  0 Value ts-fd  [then]

188 Value /packet
false value m2ts?

$FF400020 Constant header-mask
$47400020 Constant header-check \ adaption field exists

$0050 Constant rai \ random access indicator+pcr

/packet 4 + buffer: ts-packet
Variable packet#

: packet? ( -- flag )  1 packet# +!
    ts-packet /packet ts-fd read-file throw 188 u>=
    IF  ts-packet c@ $47 = dup ?EXIT
	ts-packet 4 + c@ $47 = IF
	    ts-packet 4 + ts-packet /packet 4 - move
	    ts-packet /packet 4 - + 8 ts-fd read-file throw 8 =
	    true to m2ts?  192 to /packet
	ELSE  false  THEN
    ELSE  false  THEN ;

: rai? ( -- flag )
    ts-packet be-ul@ header-mask and header-check =
    ts-packet 4 + be-uw@ rai and rai = and 0<> ;

: .rais ( -- )
    BEGIN   packet? WHILE
	    rai? IF
		." rai @ " ts-fd file-position throw ['] ud. $10 base-execute cr
	    THEN
    REPEAT ;

: >rai ( rpercent -- )
    ts-fd file-size throw d>f f* f>d
    2dup /packet ud/mod 2drop 0 d- ts-fd reposition-file throw
    BEGIN  packet?  WHILE  rai?  UNTIL  THEN
    ts-fd file-position throw /packet 0 d- ts-fd reposition-file throw ;

\ MTS file operations

User ts-writer ( addr u -- )
: ts-write-file ( addr u -- )  ts-fd write-file throw ;
' ts-write-file ts-writer !

: open-mts ( addr u -- )  packet# off
    ['] ts-write-file ts-writer !
    188 to /packet  false to m2ts?
    r/o open-file throw to ts-fd ;

: create-mts ( addr u -- )
    ['] ts-write-file ts-writer !
    188 to /packet  false to m2ts?
    r/w create-file throw to ts-fd ;

: close-mts ( -- )
    ts-fd close-file throw  0 to ts-fd ;

\ crc stuff, not performance critical

$04C11DB7 constant crc-32-poly

: +crc32 ( crc -- crc' )
    8 0 DO  dup $80000000 and 0<> crc-32-poly and swap 2* xor  LOOP
    $FFFFFFFF and ;

: crc32 ( addr u -- crc )  $FFFFFFFF -rot bounds
    ?DO  i c@ 24 lshift xor +crc32  LOOP ;

\ dump MTS stream

$1000 Value dump#
: .dump8 ( addr u -- )
    dup >r dump# min bounds ?DO  I c@ [: 0 <# # # #> type ;] $10 base-execute space LOOP
    r> dump# u> IF  ." ..."  THEN ;

: .pcr ( addr -- ) \ 27MHz 33 bit timestamp
    dup be-ul@ 600 um* rot 4 + be-uw@ >r r@ 15 rshift 300 * 0 d+
    r> $1ff and 0 d+ d>f 27M f/ f. ;

: .afield ( addr -- addr' )
    dup be-uw@ >r 2 + dup r@ 8 rshift 1 - + swap
    r@ $80 and IF  'd' emit  THEN
    r@ $40 and IF  'r' emit  THEN
    r@ $20 and IF  'p' emit  THEN
    r@ $E0 and IF  space  THEN
    r@ $10 and IF  ." pcr: " dup .pcr  6 +  THEN
    r@ $08 and IF  ." opcr: " dup .pcr  6 +  THEN
    r@ $04 and IF  ." slice: " count hex. THEN
    drop rdrop ;

: pts@ ( addr -- addr' n )
    count $E and
    >r dup be-uw@ 2/ r> 15 lshift or
    >r 2 + dup be-uw@ 2/ r> 15 lshift or
    >r 2 + r> ;

: .pts ( addr -- addr' ) pts@ s>f 90k f/ f. ;

: .pes ( addr -- addr' )
    dup be-uw@ >r 2 + dup 1+
    r@ $80 and IF  ." pts " .pts  THEN
    r> $40 and IF  ." dts " .pts  THEN
    drop count + ;

: .pusi ( addr -- addr' )
    dup be-ul@
    dup $1C0 $1E0 within IF  ." audio ch: " $1C0 - .  ELSE
	dup $1E0 $200 within IF  ." video ch: " $1E0 - .  ELSE
	    ." invalid channel:" hex.  THEN
    THEN  4 +
    dup be-uw@ ." len: " hex. 2 +  .pes ;

Variable pns s" " pns $!

: pns! ( pid pn -- ) dup 1+ 2* pns $@len dup >r umax pns $!len
    pns $@ r> /string erase
    2* pns $@ drop + w! ;

: pns? ( pid -- flag )
    pns $@ bounds ?DO  dup I w@ = IF  drop true  unloop  EXIT  THEN
    2 +LOOP  drop false ;

: be-w@+ ( addr -- addr' len )
    dup be-uw@ >r 2 + r> ;

: p>len ( addr -- addr' len )
    dup be-uw@ $3FF and >r
    dup 1- r@ 3 + crc32 0<> IF  ." inv-crc "  THEN  2 +  r> ;

: .ids ( addr -- addr' )
    be-w@+ hex.
    count ." vn " hex.
    count ." sn " hex.
    count ." lsn " hex. ;

: .pat ( addr -- addr' ) ." pat: " count + ( get to real PAT )
    count hex. \ pat table id
    p>len 2dup + { addr' } ." len " .
    ." tsid: " .ids
    addr' 4 - swap U+DO
	I 2 + be-uw@ $1FFF and  I be-uw@ 2dup pns!
	." pn " hex. ." pid " .
    4 +LOOP
    addr' ;

Create st-ids
$0 c, ," Reserved"
$1 c, ," MPEG-1 Video"
$2 c, ," MPEG-2 Video"
$3 c, ," MPEG-1 Audio"
$4 c, ," MPEG-2 Audio"
$5 c, ," ISO 13818-1 private sections"
$6 c, ," ISO 13818-1 PES private data"
$7 c, ," ISO 13522 MHEG"
$8 c, ," ISO 13818-1 DSM-CC"
$9 c, ," ISO 13818-1 auxiliary"
$a c, ," ISO 13818-6 multi-protocol encap"
$b c, ," ISO 13818-6 DSM-CC U-N msgs"
$c c, ," ISO 13818-6 stream descriptors"
$d c, ," ISO 13818-6 sections"
$e c, ," ISO 13818-1 auxiliary"
$f c, ," MPEG-2 AAC Audio"
$10 c, ," MPEG-4 Video"
$11 c, ," MPEG-4 LATM AAC Audio"
$12 c, ," MPEG-4 generic"
$13 c, ," ISO 14496-1 SL-packetized"
$14 c, ," ISO 13818-6 Synchronized Download Protocol"
$1b c, ," H.264 Video"
$80 c, ," DigiCipher II Video"
$81 c, ," A52/AC-3 Audio"
$82 c, ," HDMV DTS Audio"
$83 c, ," LPCM Audio"
$84 c, ," SDDS Audio"
$85 c, ," ATSC Program ID"
$86 c, ," DTS-HD Audio"
$87 c, ," E-AC-3 Audio"
$8a c, ," DTS Audio"
$91 c, ," A52b/AC-3 Audio"
$92 c, ," DVD_SPU vls Subtitle"
$94 c, ," SDDS Audio"
$a0 c, ," MSCODEC Video"
$ea c, ," Private ES (VC-1)"
$FF c,

: val>string { n id -- addr u }
    id  BEGIN  count dup $FF <  WHILE
	    n = IF  count  EXIT  THEN  count + aligned
    REPEAT  2drop s" Invalid tag " ;

: .pnt ( addr -- addr' ) ." pnt: " count + ( should be 0 )
    count hex. \ should be 2
    p>len 2dup + { addr' } ." len " .
    ." pn: " .ids
    dup be-uw@ ." pcr: " $1FFF and hex. 2 +
    be-w@+ $3FF and ." pd: " 2dup .dump8 +
    addr' 4 - swap U+DO
	i c@ ." { st: '" st-ids val>string type ." ' "
	i 1+ be-uw@ $1FFF and ." epid " .
	i 3 + be-w@+ $3FF and ." esd: " 2dup .dump8
	nip 5 + ." } "
    +LOOP
    addr' ;

Create desc-lst
$42 c, ," stuffing_descriptor"
$47 c, ," bouquet_name_descriptor"
$48 c, ," service_descriptor"
$49 c, ," country_availability_descriptor"
$4A c, ," linkage_descriptor"
$4B c, ," NVOD_reference_descriptor"
$4C c, ," time_shifted_service_descriptor"
$50 c, ," component_descriptor"
$51 c, ," mosaic_descriptor"
$53 c, ," CA_identifier_desriptor"
$57 c, ," telephone_descriptor"
$5D c, ," multilingual_service_name_descriptor"
$5F c, ," private_data_specifier_descriptor"
$64 c, ," data_broadcast_descriptor"
$6E c, ," announcement_support_descriptor"
$71 c, ," service_identifier_descriptor"
$72 c, ," service_availibility_descriptor"
$73 c, ," default_authority_descriptor"
$7D c, ," XAIT_location_descriptor"
$7F c, ," extension_descriptor"
$FF c,

: .descs ( addr u -- )
    bounds U+DO
	I c@ desc-lst val>string type ." : "
	I c@ $48 = IF
	    I 2 + c@ ." type: " hex.
	    I 3 + count 2dup ." '" type ." ', '" + count type ." ' "
	ELSE  I c@ 1+ count .dump8  THEN
    I 1+ c@ 2 + +LOOP ;

: .sdt ( addr -- addr' ) ." sdt: " count + ( should be 0 )
    count hex. \ should be $42
    p>len 2dup + { addr' } ." len " .
    ." tsid: " .ids
    be-w@+ ." onid: " hex.
    1+
    addr' 4 - swap U+DO
	I be-w@+ ." { sid: " hex.
	count ." eit: " 3 and hex.
	be-w@+ dup 13 rshift ." rstate: " hex.
	$3FF and 2dup .descs + ." } "
	I -
    +LOOP
    addr' ;

: .mts-header ( addr 4byte -- addr' )  packet# @ 1- /packet * hex. >r
    r@ $FF000000 and $47000000 <> IF  ." no TS header" drop rdrop  EXIT  THEN
    r@ $001FFF00 and 8 rshift { pid } ." pid: " pid .
    r@ $0000000F and hex.
    r@ $00000020 and IF  '<' emit .afield '>' emit space  THEN
    r@ $00400010 and $00400010 = IF
	pid 0=  IF  .pat  ELSE
	    pid 17 = IF  .sdt  ELSE
		pid pns? IF
		    .pnt
		ELSE
		    ." <pusi " .pusi '>' emit
		THEN
	    THEN
	THEN
    THEN  rdrop ;

: .dump8? ( addr u -- )
    2dup $FF skip 0= IF  drop 2drop  ELSE  drop .dump8  THEN ;

: dump-mts ( addr u -- )  open-mts  6 set-precision
    BEGIN  packet?  WHILE
	    ts-packet /packet 4 /string over + >r
	    ts-packet be-ul@ .mts-header
	    ts-packet be-ul@ $10 and IF  space r> over - .dump8?
	    ELSE  drop rdrop ." no payload"
	    THEN  cr
    REPEAT ;

\ generate MTS stream - e.g. from MKV

/packet 2* buffer: pat+pnt \ keep copies here, generate just once
/packet buffer: sdt

Variable pp-packets
400 Value pp-packet#
Variable sdt-packets
2000 Value sdt-packet#

: +cnt ( addr -- )
    dup >r be-ul@ dup 1+ $F and swap -$10 and or r> be-l! ;
: ppflush ( -- ) ( 'p' emit )
    pat+pnt /packet 2* ts-writer perform
    pat+pnt +cnt pat+pnt /packet + +cnt  0 pp-packets ! ;
: sdtflush ( -- ) ( 's' emit )
    sdt /packet ts-writer perform
    sdt +cnt  0 sdt-packets ! ;
: tsflush ( -- )
    ts-packet /packet ts-writer perform
    1 pp-packets +!  1 sdt-packets +!
    sdt-packets @ sdt-packet# 1- > IF sdtflush THEN
    pp-packets @ pp-packet# 1- > IF ppflush THEN ;

: >pat ( -- ) ts-packet pat+pnt /packet move ;
: >pnt ( -- ) ts-packet pat+pnt /packet + /packet move ;
: >sdt ( -- ) ts-packet sdt /packet move ;

Variable cnt-track s" " cnt-track $!
4 buffer: cnt-new

: cnt+ ( pid -- cnt )
    cnt-track $@ bounds ?DO
	dup I w@ = IF  drop I 2 + w@ dup 1+ $F and I 2 + w!  unloop  EXIT  THEN
    4 +LOOP
    cnt-new w! 1 cnt-new 2 + w! cnt-new 4 cnt-track $+! 0 ;

Variable tsdp

: tshere ( -- addr )  tsdp @ ;
: tsallot ( n -- ) tsdp +! ;
: ts-c, ( c -- )  tsdp @ c! 1 tsallot ;
: ts-w, ( w -- )  tsdp @ be-w! 2 tsallot ;
: ts-l, ( w -- )  tsdp @ be-l! 4 tsallot ;
: ts-data, ( addr u -- ) tshere swap dup tsallot move ;
: ts-string, ( addr u -- ) dup ts-c, ts-data, ;
: ts-full? ( -- flag ) tshere ts-packet - 188 u>= ;

: pid, ( pid -- )
    dup cnt+ swap 8 lshift $47000010 or or \ we always have payload
    ts-packet be-l!  ts-packet 4 + tsdp ! ;

: +flag ( flag -- )  ts-packet be-ul@ or ts-packet be-l! ;

: +psi ( -- )  $400000 +flag ;

: <af ( flags -- ) $20 +flag 0 ts-c, ts-c, ;
: af> ( -- )  tsdp @ ts-packet 5 + - ts-packet 4 + c! ;

: fsplit ( r -- r' n )  fdup floor fdup f>s f- ;
: pcr, ( r -- ) 45k f* fsplit ts-l,
    f2* fsplit 15 lshift 300e f* f>s or ts-w, ;
: pts, ( r flag -- ) >r 90k f* f>s \ only 32 bits on 32 bit systems, good enough for 13 hours
    dup 29 rshift $E and r> or ts-c,
    dup 15 rshift 2* 1 or ts-w,
    2* 1 or ts-w, ;
    
: <plen ( n -- here ) tshere swap ts-w, ;
: <string ( -- here ) tshere 0 ts-c, ;

: plen> ( here -- )
    tshere 4 + over 2 + - over be-uw@ or over be-w!
    1- tshere over - crc32 ts-l, ;
: descs> ( here -- )  tshere over 2 + - over be-uw@ or swap be-w! ;
: string> ( here -- ) tshere over 1+ - swap c! ;

: tsrest ( -- addr u ) tshere ts-packet /packet + over - ;
: fillFF ( -- ) tsrest $FF fill ;
: fill00 ( -- ) tsrest erase ;
: fillup ( addr len -- addr' len' )
    2dup tsrest nip umin dup >r ts-data, r> /string ;
: packup ( -- )
    tsrest nip dup >r 0> IF
	ts-packet 4 + ts-packet be-ul@ $20 and IF
	    dup c@ + 1+
	    tshere over - >r ts-packet 188 + r@ - r> move
	    ts-packet 4 + dup c@ + 1+ r@ $FF fill
	    r> ts-packet 4 + c@ + ts-packet 4 + c!
	ELSE
	    tshere over - >r ts-packet 188 + r@ - r> move
	    r@ 1- ts-packet 4 + c!
	    r@ 1 > IF  0 ts-packet 5 + c!  THEN
	    r@ 2 > IF  ts-packet 6 + r@ 2 - $FF fill  THEN
	    $20 +flag
	THEN
    THEN rdrop tsflush ;

: sdt, ( -- ) 17 pid, +psi 0 ts-c, $42 ts-c,
    $F000 <plen
    1 ( id ) ts-w, $C1 ( vn ) ts-c, 0 ts-c, 0 ts-c, ( sn+lsn) 1 ts-w, ( onid )
    $FF ( dummy ) ts-c, 1 ( sid ) ts-w, $FC ( eit ) ts-c,
    tshere $8000 ( rstate+len ) ts-w,
    $48 ts-c, <string  $01 ( type ) ts-c,
    s" Minos2" ts-string, s" MTS-Muxer" ts-string,
    string>
    descs>
    plen> fillFF >sdt ;

: pat, ( -- ) 0 pid, +psi 0 ts-c, $0 ts-c,
    $B000 <plen
    1 ( tsiid ) ts-w, $C1 ( vn ) ts-c, 0 ts-c, 0 ts-c, ( sn+lsn)
    1 ts-w, ( pn ) $F000 ts-w, ( pid )
    plen> fillFF >pat ;

: pnt, { video audio -- } $1000 pid, +psi  0 ts-c, $2 ts-c,
    $B000 <plen
    1 ( tsiid ) ts-w, $C1 ( vn ) ts-c, 0 ts-c, 0 ts-c, ( sn+lsn) $E100 ts-w, ( pcr )
    $F000 ts-w,
    video ts-c, $E100 ts-w, $F000 <plen descs>
    audio ts-c, $E101 ts-w, $F000 <plen descs>
    plen>
    fillFF >pnt ;

: len, ( len -- ) dup $FFFF > IF drop 0 THEN ts-w, ;
: audio, ( rpts len chan -- )  +psi $1F and $1C0 + ts-l, len,
    $8080 ts-w, <string $21 pts, string> ;
: video, ( rdts rpts len chan -- )  +psi $1F and $1E0 + ts-l, len,
    $80C0 ts-w, <string $31 pts, $11 pts, string> ;
: afield, ( [pcr..] flags -- ) >r
    r@ <af
    r@ $10 and IF pcr, THEN
    r> $08 and IF pcr, THEN af> ;

\ test header

: test-mts ( -- ) "test.ts" create-mts
    sdt, pat, $1B $03 pnt,
    $100 pid, 0.5e $50 afield, 0.2e 0.5e $2345 0 video, fillFF tsflush
    $101 pid,                       0.5e  $444 0 audio, fillFF tsflush
    close-mts ;