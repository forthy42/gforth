\ wiki forth

require string.fs

\ tag handling

: .' '' parse postpone SLiteral postpone type ; immediate
: s' '' parse postpone SLiteral ; immediate

Variable tag-option
s" " tag-option $!

: tag ( addr u -- ) '< emit type tag-option $@ type '> emit
    s" " tag-option $! ;
: /tag ( addr u -- ) '< emit '/ emit type '> emit ;
: tagged ( addr1 u1 addr2 u2 -- )  2dup 2>r tag type 2r> /tag ;

: opt ( addr u opt u -- )  s"  " tag-option $+!
    tag-option $+! s' ="' tag-option $+! tag-option $+!
    s' "' tag-option $+! ;
: href= ( addr u -- )  s" href" opt ;
: src=  ( addr u -- )  s" src" opt ;
: alt=  ( addr u -- )  s" alt" opt ;
: width=  ( addr u -- )  s" width" opt ;
: height=  ( addr u -- )  s" height" opt ;
: align= ( addr u -- ) s" align" opt ;
: mailto: ( addr u -- ) s'  href="mailto:' tag-option $+!
    tag-option $+! s' "' tag-option $+! ;

\ environment handling

Variable oldenv
Variable envs 10 0 [DO] 0 , [LOOP]

: env$ ( -- addr ) envs dup @ 1+ cells + ;
: env ( addr u -- ) env$ $! ;
: env? ( -- ) envs @ oldenv @
    2dup > IF  env$ $@ tag  THEN
    2dup < IF  env$ cell+ $@ /tag  env$ cell+ $off  THEN
    drop oldenv ! ;
: +env  1 envs +! ;
: -env -1 envs +! env? ;
: -envs envs @ 0 ?DO  -env cr  LOOP ;
: >env ( addr u -- ) +env env env? ;

\ alignment

: >align ( c -- )
    CASE
	'l OF  s" left"   align=  ENDOF
	'r OF  s" right"  align=  ENDOF
	'c OF  s" center" align=  ENDOF
	'< OF  s" left"   align=  ENDOF
	'> OF  s" right"  align=  ENDOF
	'| OF  s" center" align=  ENDOF
    ENDCASE ;

\ image handling

Create imgbuf $20 allot

Create pngsig $89 c, $50 c, $4E c, $47 c, $0D c, $0A c, $1A c, $0A c,
Create jfif   $FF c, $D8 c, $FF c, $E0 c, $00 c, $10 c, $4A c, $46 c,
              $49 c, $46 c,

: b@ ( addr -- x )   0 swap 4 bounds ?DO  8 lshift I c@ +  LOOP ;
: bw@ ( addr -- x )  0 swap 2 bounds ?DO  8 lshift I c@ +  LOOP ;

: gif? ( -- flag )
    s" GIF89a" imgbuf over compare 0=
    s" GIF87a" imgbuf over compare 0= or ;
: gif-size ( -- w h )
    imgbuf 6 + c@ imgbuf 7 + c@ 8 lshift +
    imgbuf 8 + c@ imgbuf 9 + c@ 8 lshift + ;

: png? ( -- flag )
    pngsig 8 imgbuf over compare 0= ;
: png-size ( -- w h )
    imgbuf $10 + b@ imgbuf $14 + b@ ;

: jpg? ( -- flag )
    jfif 10 imgbuf over compare 0= ;
: jpg-size ( fd -- w h )  >r
    2.  BEGIN
	2dup r@ reposition-file throw
	imgbuf $10 r@ read-file throw 0<>
	imgbuf bw@ $FFC0 $FFD0 within 0= and  WHILE
	imgbuf 2 + bw@ 2 + 0 d+  REPEAT
    2drop imgbuf 5 + bw@ imgbuf 7 + bw@  rdrop ;

: img-size ( fd -- w h )  >r
    gif? IF  gif-size  rdrop EXIT  THEN
    jpg? IF  r> jpg-size  EXIT  THEN
    png? IF  png-size  rdrop EXIT  THEN
    0 0 ;

: .img-size ( addr u -- )
    r/o open-file throw >r
    imgbuf $20 r@ read-file throw drop
    r@ img-size
    r> close-file throw
    ?dup IF  0 <# #S #> width=   THEN
    ?dup IF  0 <# #S #> height=  THEN ;

\ link creation

Variable link
Variable link-suffix
Variable iconpath

Variable do-size
Variable do-icon

Defer parse-line

: alt-suffix ( -- )
    link-suffix $@len 2 - link-suffix $!len
    s" [" link-suffix 0 $ins
    s" ]" link-suffix $+!
    link-suffix $@ alt= ;

: get-icon ( addr u -- )  iconpath @ IF  2drop  EXIT  THEN
    link-suffix $! s" .*" link-suffix $+!
    s" icons" open-dir throw >r
    BEGIN
	pad $100 r@ read-dir throw  WHILE
	pad swap 2dup link-suffix $@ filename-match
	IF  s" icons/" iconpath $! iconpath $+!
	    iconpath $@ 2dup .img-size src=
	    alt-suffix  s" img" tag true
	ELSE  2drop  false  THEN
    UNTIL  ELSE  drop  THEN
    r> close-dir throw ;

: link-icon? ( -- )  do-icon @ 0= ?EXIT
    iconpath @  IF  iconpath $off  THEN
    link $@ + 1- c@ '/ = IF  s" index.html"  ELSE  link $@  THEN
    BEGIN  '. $split 2swap 2drop dup  WHILE
	2dup get-icon  REPEAT  2drop ;

: link-size? ( -- )  do-size @ 0= ?EXIT
    link $@ r/o open-file IF  drop  EXIT  THEN >r
    r@ file-size throw $400 um/mod nip ."  (" 0 u.r ." k)"
    r> close-file throw ;

: link-options ( addr u -- addr' u' )
    do-size off  do-icon on
    over c@ '% = over 0> and IF  do-size on  1 /string  THEN
    over c@ '\ = over 0> and IF  do-icon off 1 /string  THEN ;

: parse-string ( addr u -- )
    evaluate-input cell new-tib #tib ! tib !
    ['] parse-line catch pop-file throw ;

: .link ( addr u -- ) '| $split 
    link-options link $!
    link $@len 0= IF  2dup link $! s" .html" link $+!  THEN
    link-icon? link $@ href= s" a" tag
    parse-string s" a" /tag link-size? ;
: >link ( -- )  '[ parse type '] parse .link ;

: .img ( addr u -- ) '| $split 
    dup IF  2swap alt=  ELSE  2drop  THEN
    tag-option $@len >r over c@ >align tag-option $@len r> = 1+ /string
    2dup .img-size src= s" img" tag ;
: >img ( -- )   '{ parse type '} parse .img ;

\ line handling

: char? ( -- c )  >in @ char swap >in ! ;
: parse-tag ( addr u char -- )
    >r r@ parse type
    r> parse 2swap tagged ;

: .text ( -- ) 	>in @ >r char drop
    source r@ /string >in @ r> - nip
    bounds ?DO  I c@
	case
	    '& of  ." &amp;"  endof
	    '< of  ." &lt;"   endof
	    dup emit
	endcase
    LOOP ;

Create do-words  $100 0 [DO] ' .text , [LOOP]

:noname '( emit 1 >in +! ; '( cells do-words + !

: bind-char ( xt -- )  char cells do-words + ! ;

: char>tag ( -- ) char >r
:noname bl sword postpone SLiteral r@ postpone Literal
    postpone parse-tag postpone ; r> cells do-words + ! ;

char>tag * b
char>tag _ em
char>tag # code

' >link bind-char [
' >img  bind-char {

: do-word ( char -- )  cells do-words + perform ;

: word? ( -- addr u )  >in @ >r bl sword r> >in ! ;

wordlist Constant autoreplacements

:noname ( -- )
    BEGIN char? do-word source nip >in @ = UNTIL ; is parse-line

: parse-line+ ( -- )
    BEGIN
	word? autoreplacements search-wordlist
	IF    execute  bl sword 2drop
	    source >in @ 1- /string drop c@ bl = >in +!
	ELSE  char? do-word  THEN
	source nip >in @ = UNTIL ;

: parse-to ( char -- ) >r
    BEGIN  char? dup r@ <> WHILE
	do-word source nip >in @ = UNTIL  ELSE  drop  THEN
    r> parse type ;

\ autoreplace

: autoreplace ( <[string|url]> -- )
    get-current autoreplacements set-current
    Create set-current
    here 0 , '[ parse 2drop '] parse rot $!
    DOES> $@ .link ;
    
\ paragraph handling

: parse-par ( -- )
    BEGIN  parse-line+ cr refill  WHILE
	source nip 0= UNTIL  THEN ;

: par ( addr u -- ) env? 2dup tag parse-par /tag cr cr ;
: line ( addr u -- ) env? 2dup tag parse-line+ /tag cr cr ;

\ handle global tags

Variable indentlevel
: indent ( n -- )  indentlevel @
    2dup < IF  2dup swap DO  -env -env  LOOP  THEN
    2dup > IF  2dup      DO  s" dl" >env  LOOP  THEN
    2dup = IF  -env  THEN
    drop indentlevel ! s" dt" >env ;
: +indent ( -- )  -env s" dd" >env ;

wordlist constant longtags

Variable end-sec

longtags set-current

: --- 1 indent cr s" hr" tag cr +indent ;
: *   1 indent s" h1" line +indent ;
: **  1 indent s" h2" line +indent ;
: *** 2 indent s" h3" line +indent ;
: - s" ul" env s" li" par ;
: + s" ol" env s" li" par ;
: << +env ;
: <* s" center" >env ;
: >> -env ;
: *> -env ;
: :: interpret ;
: . end-sec on indentlevel off ;
: :code s" pre" >env
    BEGIN  source >in @ /string type cr refill  WHILE
	source s" :endcode" compare 0= UNTIL  THEN
  -env ;
: \ postpone \ ;

definitions
    
\ Table

Variable table-format
Variable table#

: |tag  table-format $@ table# @ /string drop c@ >align
    >env  1 table# +! ;
: |d  table# @ IF  -env  THEN  s" td" |tag ;
: |h  table# @ IF  -env  THEN  s" th" |tag ;
: |line  s" tr" >env  table# off ;
: line|  -env -env cr ;

: next-char ( -- char )  source drop >in @ + c@ ;

longtags set-current

: <| s" table" >env bl sword table-format $! ;
: |> -env ;
: +| |line
    BEGIN
	|h '| parse-to next-char '+ =  UNTIL line| ;
: -| |line
    BEGIN
	|d '| parse-to next-char '- =  UNTIL line| ;

definitions

\ parse a section

: section-line ( -- )  >in off
    bl sword longtags search-wordlist
    IF    execute
    ELSE  source nip IF  >in off s" p" par  THEN  THEN ;
: refill-loop ( -- )  end-sec off
    BEGIN  refill  WHILE
	section-line end-sec @ UNTIL  THEN ;
: parse-section ( -- )
    refill-loop ;

\ HTML head

: .title ( addr u -- )
    .' <!doctype html public "-//w3c//dtd html 4.0 transitional//en">' cr
    s" html" >env s" head" >env
    .'   <meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">' cr
    s" title" tagged cr
    -env ;

\ HTML trailer

Variable mail
Variable mail-name

: .trailer
    s" address" >env s" center" >env
    ." Last modified: " time&date rot 0 u.r swap 1-
    s" janfebmaraprmayjunjulaugsepoctnovdec" rot 3 * /string 3 min type
    0 u.r ."  by "
    mail $@ mailto: mail-name $@ s" a" tagged
    -envs ;

\ top word

: parse" ( -- addr u ) '" parse 2drop '" parse ;

: maintainer
    bl sword mail $! parse" mail-name $! ;

Variable style$
: style> style$ @ 0= IF  s" " style$ $!  THEN  style$ $@ tag-option $! ;
: >style tag-option $@ style$ $! s" " tag-option $! ;

: style  style> opt >style ;
: background ( -- )  parse" s" background" style ;
: text ( -- )  parse" s" text" style ;
    warnings @ warnings off
: link ( -- )  parse" s" link" style ;
    warnings !
: vlink ( -- ) parse" s" vlink" style ;
: marginheight ( -- ) parse" s" marginheight" style ;

: wf ( -- )
    outfile-id >r
    bl sword r/w create-file throw to outfile-id
    parse" .title
    +env style> s" body" env env?
    ['] parse-section catch .trailer
    outfile-id close-file throw
    r> to outfile-id
    dup 0< IF  throw  ELSE  drop  THEN ;

: eval-par ( addr u -- )
  s" wf-temp.wf" r/w create-file throw >r
  r@ write-file r> close-file throw
  push-file s" wf-temp.wf" r/o open-file throw loadfile !
  parse-par parse-section
  loadfile @ close-file swap 2dup or
  pop-file  drop throw throw
  s" wf-temp.wf" delete-file throw ;

\ simple text data base

: get-rest ( addr -- ) 0 parse -trailing rot $! ;
Create $lf 1 c, #lf c,
: get-par ( addr -- )  >r  s" " r@ $+!
    BEGIN  0 parse 2dup s" ." compare  WHILE
	r@ $@len IF  $lf count r@ $+!  THEN  r@ $+!
	refill 0= UNTIL  ELSE  2drop  THEN
    rdrop ;

Variable last-entry
Variable field#

: table: ( xt n -- )  Create , ,  1 field# !
    DOES> 2@ >in @ >r longtags set-current
    Create definitions swap , r> >in !
    here last-entry !
    dup 0 DO  0 ,  LOOP
    1 DO  s" " last-entry @ I cells + $!  LOOP
    last-entry @ get-rest
    DOES> dup cell+ swap perform ;

: field:  Create field# @ , 1 field# +!
DOES> @ cells last-entry @ + get-rest ;
: par:  Create field# @ , 1 field# +!
DOES> @ cells last-entry @ + get-par ;

: >field  ' >body @ cells postpone Literal postpone + ; immediate
