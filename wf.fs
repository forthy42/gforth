\ wiki forth

\ Copyright (C) 2003 Free Software Foundation, Inc.

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

require string.fs

: -scan ( addr u char -- addr' u' )
  >r  BEGIN  dup  WHILE  1- 2dup + c@ r@ =  UNTIL  THEN
  rdrop ;
: -$split ( addr u char -- addr1 u1 addr2 u2 )
  >r 2dup r@ -scan 2dup + c@ r> = negate over + >r
  2swap r> /string ;
: parse" ( -- addr u ) '" parse 2drop '" parse ;

\ tag handling

: .' '' parse postpone SLiteral postpone type ; immediate
: s' '' parse postpone SLiteral ; immediate

Variable indentlevel
Variable tag-option
s" " tag-option $!

: tag ( addr u -- ) '< emit type tag-option $@ type '> emit
    s" " tag-option $! ;
: tag/ ( addr u -- )  s"  /" tag-option $+! tag ;
: /tag ( addr u -- ) '< emit '/ emit type '> emit ;
: tagged ( addr1 u1 addr2 u2 -- )  2dup 2>r tag type 2r> /tag ;

: opt ( addr u opt u -- )  s"  " tag-option $+!
    tag-option $+! s' ="' tag-option $+! tag-option $+!
    s' "' tag-option $+! ;
: href= ( addr u -- )  s" href" opt ;
: id= ( addr u -- )  s" id" opt ;
: src=  ( addr u -- )  s" src" opt ;
: alt=  ( addr u -- )  s" alt" opt ;
: width=  ( addr u -- )  s" width" opt ;
: height=  ( addr u -- )  s" height" opt ;
: align= ( addr u -- ) s" align" opt ;
: class= ( addr u -- ) s" class" opt ;
: indent= ( -- )
    indentlevel @ 0 <# #S 'p hold #> class= ;
: mailto: ( addr u -- ) s'  href="mailto:' tag-option $+!
    tag-option $+! s' "' tag-option $+! ;

\ environment handling

Variable end-sec
Variable oldenv
Variable envs 30 0 [DO] 0 , [LOOP]

: env$ ( -- addr ) envs dup @ 1+ cells + ;
: env ( addr u -- ) env$ $! ;
: env? ( -- ) envs @ oldenv @
    2dup > IF  env$ $@ tag  THEN
    2dup < IF  env$ cell+ $@ /tag  env$ cell+ $off  THEN
    drop oldenv ! ;
: +env  1 envs +! ;
: -env end-sec @ envs @ 2 > or  IF  -1 envs +! env?  THEN ;
: -envs envs @ 0 ?DO  -env cr  LOOP ;
: >env ( addr u -- ) +env env env? ;

\ alignment

Variable table-format
Variable table#
Variable table-start

: >align ( c -- )
    CASE
	'l OF  s" left"      class=  ENDOF
	'r OF  s" right"     class=  ENDOF
	'c OF  s" center"    align=  ENDOF
	'< OF  s" left"      class=  ENDOF
	'> OF  s" right"     class=  ENDOF
	'= OF  s" center"    align=  ENDOF
	'~ OF  s" absmiddle" align=  ENDOF
    ENDCASE ;

: >talign ( c -- )
    CASE
	'l OF  s" left"   align=  ENDOF
	'r OF  s" right"  align=  ENDOF
	'c OF  s" center" align=  ENDOF
	'< OF  s" left"   align=  ENDOF
	'> OF  s" right"  align=  ENDOF
	'= OF  s" center" align=  ENDOF
	digit? IF  0 <# #S #> s" rowspan" opt
	    table# @ 1+ table-start ! THEN 0
    ENDCASE ;

: >border ( c -- )
    case
	'- of  s" border0" class= endof
	'+ of  s" border1" class= endof
    endcase ;

\ image handling

Create imgbuf $20 allot

Create pngsig $89 c, $50 c, $4E c, $47 c, $0D c, $0A c, $1A c, $0A c,
Create jfif   $FF c, $D8 c, $FF c, $E0 c, $00 c, $10 c, $4A c, $46 c,
              $49 c, $46 c,

: b@ ( addr -- x )   0 swap 4 bounds ?DO  8 lshift I c@ +  LOOP ;
: bw@ ( addr -- x )  0 swap 2 bounds ?DO  8 lshift I c@ +  LOOP ;

: gif? ( -- flag )
    s" GIF89a" imgbuf over str=
    s" GIF87a" imgbuf over str= or ;
: gif-size ( -- w h )
    imgbuf 8 + c@ imgbuf 9 + c@ 8 lshift +
    imgbuf 6 + c@ imgbuf 7 + c@ 8 lshift + ;

: png? ( -- flag )
    pngsig 8 imgbuf over str= ;
: png-size ( -- w h )
    imgbuf $14 + b@ imgbuf $10 + b@ ;

: jpg? ( -- flag )
    jfif 10 imgbuf over str= ;
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
    r/o open-file IF  drop  EXIT  THEN  >r
    imgbuf $20 r@ read-file throw drop
    r@ img-size
    r> close-file throw
    ?dup IF  0 <# #S #> width=   THEN
    ?dup IF  0 <# #S #> height=  THEN ;

\ link creation

Variable link
Variable link-sig
Variable link-suffix
Variable iconpath

Variable do-size
Variable do-icon

Defer parse-line

: .img ( addr u -- ) dup >r '| -$split  dup r> = IF  2swap  THEN 
    dup IF  2swap alt=  ELSE  2drop  THEN
    tag-option $@len >r over c@ >align  tag-option $@len r> = 1+ /string
    tag-option $@len >r over c@ >border tag-option $@len r> = 1+ /string
    2dup .img-size src= s" img" tag/ ;
: >img ( -- )   '{ parse type '} parse .img ;

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
	    iconpath $@ 2dup .img-size src= '- >border
	    alt-suffix  s" img" tag/ true
	ELSE  2drop  false  THEN
    UNTIL  ELSE  drop  THEN
    r> close-dir throw ;

: link-icon? ( -- )  do-icon @ 0= ?EXIT
    iconpath @  IF  iconpath $off  THEN
    link $@ + 1- c@ '/ = IF  s" index.html"  ELSE  link $@  THEN
    '# $split 2drop
    BEGIN  '. $split 2swap 2drop dup  WHILE
	2dup get-icon  REPEAT  2drop ;

: link-size? ( -- )  do-size @ 0= ?EXIT
    link $@ r/o open-file IF  drop  EXIT  THEN >r
    r@ file-size throw $400 um/mod nip ."  (" 0 u.r ." k)"
    r> close-file throw ;

: link-sig? ( -- )
    link $@ link-sig $! s" .sig" link-sig $+!
    link-sig $@ r/o open-file IF  drop  EXIT  THEN
    close-file throw
    ."  (" link-sig $@ href= s" a" tag
    s" |-icons/sig.gif" .img ." sig" s" /a" tag ." )" ;

: link-options ( addr u -- addr' u' )
    do-size off  do-icon on
    over c@ '% = over 0> and IF  do-size on  1 /string  THEN
    over c@ '\ = over 0> and IF  do-icon off 1 /string  THEN ;

s" Gforth" environment? [IF] s" 0.5.0" str= [IF] 
: parse-string ( c-addr u -- ) \ core,block
    s" *evaluated string*" loadfilename>r
    push-file #tib ! >tib !
    >in off blk off loadfile off -1 loadline !
    ['] parse-line catch
    pop-file r>loadfilename throw ;
[ELSE]
: parse-string ( addr u -- )
    evaluate-input cell new-tib #tib ! tib !
    ['] parse-line catch pop-file throw ;
[THEN] [THEN]

: .link ( addr u -- ) dup >r '| -$split  dup r> = IF  2swap  THEN 
    link-options link $!
    link $@len 0= IF  2dup link $! ( s" .html" link $+! ) THEN
    link $@ href= s" a" tag link-icon?
    parse-string s" a" /tag link-size? link-sig? ;
: >link ( -- )  '[ parse type '] parse .link ;

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

: >tag '\ parse type '\ parse tag ;

char>tag * b
char>tag _ em
char>tag # code

' >link bind-char [
' >img  bind-char {
' >tag  bind-char \

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

: par ( addr u -- ) env? indent=
    2dup tag parse-par /tag cr cr ;
: line ( addr u -- ) env? 2dup tag parse-line+ /tag cr cr ;

\ scan strings

: get-rest ( addr -- ) 0 parse -trailing rot $! ;
Create $lf 1 c, #lf c,
: get-par ( addr -- )  >r  s" " r@ $+!
    BEGIN  0 parse 2dup s" ." str= 0=  WHILE
	r@ $@len IF  $lf count r@ $+!  THEN  r@ $+!
	refill 0= UNTIL  ELSE  2drop  THEN
    rdrop ;

\ toc handling

Variable toc-link

: >last ( addr link -- link' )
    BEGIN  dup @  WHILE  @  REPEAT  ! 0 ;

Variable create-navs
Variable nav$
Variable nav-name
Variable nav-file
Create nav-buf 0 c,
: nav+ ( char -- )  nav-buf c! nav-buf 1 nav-file $+! ;

: >nav ( addr u -- addr' u' )
    nav-name $!  create-navs @ 0=
    IF  s" navigate/nav.scm" r/w create-file throw create-navs !  THEN
    s' (script-fu-nav-file "' nav$ $! nav-name $@ nav$ $+!
    s' " "./navigate/' nav$ $+!  s" " nav-file $!
    nav-name $@ bounds ?DO
	I c@  dup 'A 'Z 1+ within IF  bl + nav+
	ELSE  dup 'a 'z 1+ within IF  nav+
	ELSE  dup '0 '9 1+ within IF  nav+
	ELSE  dup  bl = swap '- = or IF  '- nav+
	THEN  THEN  THEN  THEN
	LOOP
    nav-file $@ nav$ $+! s' .jpg")' nav$ $+!
    nav$ $@ create-navs @ write-line throw
    s" [" nav$ $! nav-name $@ nav$ $+!
    s" |-navigate/" nav$ $+! nav-file $@ nav$ $+! s" .jpg" nav$ $+!
    nav$ $@ ;

: toc, ( n -- ) , '| parse >nav here 0 , $! 0 parse here 0 , $! ;
: up-toc   align here toc-link >last , 0 toc, ;
: top-toc  align here toc-link >last , 1 toc, ;
: this-toc align here toc-link >last , 2 toc, ;
: sub-toc  align here toc-link >last , 3 toc, ;
: new-toc  toc-link off ;

Variable toc-name
Variable toc-index
6 Value /toc-line

: .toc-entry ( toc flag -- )
    swap cell+ dup @ swap cell+ dup cell+ $@ 2dup href= s" a" tag
    '# scan 1 /string toc-name $@ compare >r
    $@ .img swap
    IF
	case
	    2 of  s" ^]|-icons/arrow_up.jpg" .img  endof
	    3 of
		r@ 0= IF s" *]|-icons/circle.jpg"
		    ELSE s" v]|-icons/arrow_down.jpg"  THEN  .img  endof
	endcase
    ELSE
	case
	    0 of  s" ^]|-icons/arrow_up.jpg" .img  endof
	    1 of  s" >]|-icons/arrow_right.jpg" .img  endof
	    2 of  s" *]|-icons/circle.jpg" .img  endof
	    3 of  s" v]|-icons/arrow_down.jpg" .img  endof
	endcase
    THEN
    s" a" /tag rdrop ." <!--" cr ." -->"
    1 toc-index +! toc-index @ /toc-line mod 0=
    IF  s" br" tag/ THEN ;

: print-toc ( -- ) toc-index off cr s" menu" id= s" div" >env cr
    0 parse
    dup 0= IF  toc-name $! 0  ELSE
	toc-name $! toc-name $@ id= s" " s" a" tagged  2
    THEN  >r
    toc-link  BEGIN  @ dup  WHILE
	dup cell+ @ 3 = r@ 0= and IF  rdrop 1 >r ( s" br" tag/ cr )  THEN
	dup cell+ @ r@ >= IF  dup r@ 2 = .toc-entry  THEN
	dup cell+ @ 2 = r@ 2 = and IF  s" br" tag/ toc-index off THEN
    REPEAT  drop rdrop -env cr ;

\ handle global tags

: indent ( n -- )
    indentlevel @ over
    indentlevel !
    2dup < IF swap DO  -env -env  LOOP  EXIT THEN
    over 1 = IF  = IF  -env -env  THEN  EXIT  THEN
    2dup > IF      DO  s" dl" >env s" dt" >env  LOOP EXIT THEN
    2dup = IF drop IF  -env  s" dt" >env  THEN THEN
;
: +indent ( -- )
    indentlevel @ IF  -env -env s" dl" >env s" dd" >env  THEN
;

wordlist constant longtags

Variable divs

longtags set-current

: --- 0 indent cr s" hr" tag/ cr +indent ;
: *   1 indent s" h1" line +indent ;
: **  1 indent s" h2" line +indent ;
: *** 2 indent s" h3" line +indent ;
: -- 0 indent cr print-toc ;
: && ( -- ) divs @ IF  -env  THEN  +env
    0 parse id= s" div" env env? divs on ;
: - s" ul" env s" li" par ;
: + s" ol" env s" li" par ;
: << +env ;
: <* s" center" class= ;
: <red  s" #ff0000" s" color" opt s" font" >env ;
: red> -env ;
: >> -env ;
: *> ;
: :: interpret ;
: . end-sec on 0 indent ;
: :code indent= s" pre" >env
    BEGIN  source >in @ /string type cr refill  WHILE
	source s" :endcode" str= UNTIL  THEN
    -env ;
: :code-file indent= s" pre" >env
    parse" r/o open-file throw >r
    r@ file-size throw drop dup allocate throw
    2dup swap r@ read-file throw 2dup type drop
    -env free throw drop
    r> close-file throw ;
: \ postpone \ ;

definitions
    
\ Table

: |tag  table-format $@ table# @ /string drop c@ >talign
    >env  1 table# +! ;
: |d  table# @ table-start @ > IF  -env  THEN  s" td" |tag ;
: |h  table# @ table-start @ > IF  -env  THEN  s" th" |tag ;
: |line  s" tr" >env  table-start @ table# ! ;
: line|  -env -env cr ;

: next-char ( -- char )  source drop >in @ + c@ ;

longtags set-current

: <| bl sword table-format $! table-start off bl sword
    dup IF  s" border" opt  ELSE  2drop  THEN s" table" >env ;
: |> -env -env cr cr ;
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

Variable css-file

: .title ( addr u -- )
    .' <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//en" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">' cr
    s" html" >env s" head" >env cr
    s" Content-Type" s" http-equiv" opt
    s" text/xhtml; charset=iso-8859-1" s" content" opt
    s" meta" tag/
    css-file @ IF css-file $@len IF
	s" StyleSheet" s" rel" opt
	css-file $@ href=
	s" text/css" s" type" opt s" link" tag/
    THEN THEN
    s" title" tagged cr
    -env ;

\ HTML trailer

Variable mail
Variable mail-name
Variable orig-date

: .lastmod
    ." Last modified: " time&date rot 0 u.r swap 1-
    s" janfebmaraprmayjunjulaugsepoctnovdec" rot 3 * /string 3 min type
    0 u.r ;

: .trailer
    s" center" class= s" address" >env
    orig-date @ IF  ." Created " orig-date $@ type ." . "  THEN
    .lastmod
 ."  by "
    s" Mail|icons/mail.gif" .img mail $@ mailto: mail-name $@ s" a" tagged
    -envs ;

\ top word

: maintainer ( -- )
    bl sword mail $! parse" mail-name $! ;
: created ( -- )
    bl sword orig-date $! ;

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
: css ( -- ) parse" css-file $! ;

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
