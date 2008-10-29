\ wiki forth

\ Copyright (C) 2003,2004,2005,2006,2007,2008 Free Software Foundation, Inc.

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

require string.fs

\ basic stuff

: -scan ( addr u char -- addr' u' )
  >r  BEGIN  dup  WHILE  1- 2dup + c@ r@ =  UNTIL  THEN
  rdrop ;
: -$split ( addr u char -- addr1 u1 addr2 u2 )
  >r 2dup r@ -scan 2dup + c@ r> = negate over + >r
  2swap r> /string ;
: parse" ( -- addr u ) '" parse 2drop '" parse ;
: .' '' parse postpone SLiteral postpone type ; immediate
: s' '' parse postpone SLiteral ; immediate
: .upcase ( addr u -- )  bounds ?DO  I c@ toupper emit  LOOP ;

\ character recoding

[IFDEF] maxascii $100 to maxascii 8-bit-io [THEN]
\ UTF-8 IO fails with .type:

: .type ( addr u -- )
    bounds ?DO  I c@
	case
	    '& of  ." &amp;"  endof
	    '< of  ." &lt;"   endof
\	    &164 of  ." &euro;" endof
	    dup emit
	endcase
    LOOP ;

\ tag handling

Variable indentlevel
Variable tag-option
Variable tag-class
Variable default-class
s" " tag-option $!
s" " tag-class $!
s" " default-class $!

: tag ( addr u -- ) '< emit type
    tag-class $@len IF  .\"  class=\"" tag-class $@ type '" emit  THEN
    tag-option $@ type
    '> emit
    s" " tag-option $! default-class $@ tag-class $! ;
: tag/ ( addr u -- )  s"  /" tag-option $+! tag ;
: /tag ( addr u -- ) '< emit '/ emit type '> emit ;
: tagged ( addr1 u1 addr2 u2 -- )  2dup 2>r tag .type 2r> /tag ;

: opt ( addr u opt u -- )  s"  " tag-option $+!
    tag-option $+! s' ="' tag-option $+!
\    BEGIN  dup  WHILE  '& $split >r >r tag-option $+! r> r>
\	    dup IF  s" %26" tag-option $+!  THEN
\    REPEAT  2drop
    tag-option $+!
    s' "' tag-option $+! ;
: n>string ( n -- addr u )  0 <# #S #> ;
: xy>string ( x y -- )  swap 0 <# #S 'x hold 2drop 0 #S 's hold #> ;
: opt# ( n opt u -- )  rot n>string 2swap opt ;
: href= ( addr u -- )  s" href" opt ;
: id= ( addr u -- )  s" id" opt ;
: src=  ( addr u -- )  s" src" opt ;
: alt=  ( addr u -- )  s" alt" opt ;
: width=  ( n -- )  s" width" opt# ;
: height=  ( n -- )  s" height" opt# ;
: align= ( addr u -- ) s" align" opt ;
: class= ( addr u -- )
    tag-class $@len IF  s"  " tag-class $+!  THEN
    tag-class $+! ;
: dclass= ( addr u -- )  2dup class=
    default-class $! ;
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
: env? ( -- ) envs @ oldenv @ over oldenv !
    2dup > IF  env$ $@ tag  THEN
    2dup < IF  env$ cell+ $@ /tag  env$ cell+ $off  THEN
    2drop ;
: +env  1 envs +! ;
: -env end-sec @ envs @ 1 > or  IF  -1 envs +! env?  THEN ;
: -envs envs @ 0 ?DO  -env cr  LOOP ;
: -tenvs envs @ 1 ?DO  -env cr  LOOP ;
: >env ( addr u -- ) +env env env? ;

\ alignment

Variable table-format
Variable table#
Create table-starts &10 0 [DO] 0 c, 0 c, [LOOP]
Variable taligned

: >align ( c -- )
    CASE
	'l OF  s" left"      class=  ENDOF
	'r OF  s" right"     class=  ENDOF
	'c OF  s" center"    class=  ENDOF
	'< OF  s" left"      class=  ENDOF
	'> OF  s" right"     class=  ENDOF
	'= OF  s" center"    class=  ENDOF
	'~ OF  s" middle"    class=  ENDOF
    ENDCASE ;

: >talign ( c -- )
    CASE
	'l OF  s" left"   align=  ENDOF
	'r OF  s" right"  align=  ENDOF
	'c OF  s" center" align=  ENDOF
	'< OF  s" left"   align=  ENDOF
	'> OF  s" right"  align=  ENDOF
	'= OF  s" center" align=  ENDOF
    ENDCASE  taligned on ;

: >border ( c -- )
    case
	'- of  s" border0" class= endof
	'+ of  s" border1" class= endof
    endcase ;

\ image handling

wordlist Constant img-sizes

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
    0 0 rdrop ;

3 set-precision

: f.size  ( r -- )
  f$ dup >r 0<=
  IF    '0 emit
  ELSE  scratch r@ min type  r@ precision - zeros  THEN
  r@ negate zeros
  scratch r> 0 max /string 0 max -zeros
  dup IF  '. emit  THEN  type ;

12.9e FConstant pixels
FVariable factor  1e factor f!

: size-does> ( -- )  DOES> ( -- )
    ." img." dup body> >name .name
    2@ ." { width: "
    s>d d>f pixels f/ f.size ." em; height: "
    s>d d>f pixels f/ f.size ." em; }" cr ;

: size-css ( file< > -- )
    outfile-id >r
    bl sword r/w create-file throw to outfile-id
    img-sizes wordlist-id
    BEGIN  @ dup  WHILE
	    dup name>int execute
    REPEAT  drop
    outfile-id close-file throw
    r> to outfile-id
    dup 0< IF  throw  ELSE  drop  THEN ;

: size-class ( x y addr u -- x y )
    2dup class=
    2dup img-sizes search-wordlist  IF  drop 2drop
    ELSE
	get-current >r img-sizes set-current
	nextname Create 2dup
	s>d d>f factor f@ f* f>d d>s ,
	s>d d>f factor f@ f* f>d d>s ,
	size-does>
	r> set-current
    THEN ;

: .img-size ( addr u -- )
    r/o open-file IF  drop  EXIT  THEN  >r
    imgbuf $20 r@ read-file throw drop
    r@ img-size
    r> close-file throw
    2dup or IF  2dup xy>string size-class  THEN  
    ?dup IF  width=   THEN
    ?dup IF  height=  THEN
;

\ link creation

Variable link
Variable link-sig
Variable link-suffix
Variable iconpath
Variable icon-prefix
Variable icon-tmp

Variable do-size
Variable do-icon
Variable do-expand

Defer parse-line

: .img ( addr u -- )
    dup >r '@ -$split  dup r> = IF  2swap 2drop
    ELSE  2swap icon-tmp $! icon-prefix $@ icon-tmp $+! icon-tmp $+!
	icon-tmp $@  THEN
    dup >r '| -$split  dup r> = IF  2swap  THEN 
    dup IF  2swap alt=  ELSE  2drop s" " alt=  THEN
    tag-class $@len >r over c@ >align  tag-class $@len r> = 1+ /string
    tag-class $@len >r over c@ >border tag-class $@len r> = 1+ /string
    2dup .img-size src= s" img" tag/ ;
: >img ( -- )   '{ parse type '} parse .img ;

: alt-suffix ( -- )
    link-suffix $@len 2 - link-suffix $!len
    s" [" link-suffix 0 $ins
    s" ]" link-suffix $+!
    link-suffix $@ alt= ;

: replace.- ( addr u -- )
    bounds ?DO  I c@ '. = IF  '- I c!  THEN  LOOP ;

: get-icon ( addr u -- )  iconpath @ IF  2drop  EXIT  THEN
    link-suffix $! link-suffix $@ replace.-
    s" .*" link-suffix $+!
    icon-prefix $@ open-dir IF  drop  EXIT  THEN >r
    BEGIN
	pad $100 r@ read-dir throw  WHILE
	pad swap 2dup link-suffix $@ filename-match
	IF  icon-prefix $@ iconpath $! s" /" iconpath $+! iconpath $+!
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
    r@ file-size throw $400 um/mod nip
    dup $800 < IF  ."  (" 0 u.r ." k)"
	ELSE  $400 / ."  (" 0 u.r ." M)" THEN
    r> close-file throw ;

: link-sig? ( -- )
    link $@ link-sig $! s" .sig" link-sig $+!
    link-sig $@ r/o open-file IF  drop  EXIT  THEN
    close-file throw
    ."  (" link-sig $@ href= s" a" tag
    s" |-@/sig.gif" .img ." sig" s" /a" tag ." )" ;

: link-warn? ( -- ) \ local links only
    link $@ ': scan nip ?EXIT
    link $@ '# $split 2drop dup IF
	r/o open-file nip IF
	    s" Dead Link '" stderr write-file throw
	    link $@ stderr write-file throw
	    s\" ' !!!\n" stderr write-file throw
	THEN
    ELSE  2drop  THEN ;

: link-options ( addr u -- addr' u' )
    do-size off  do-icon on  do-expand off
    over c@ '% = over 0> and IF  do-size on   1 /string  THEN
    over c@ '\ = over 0> and IF  do-icon off  1 /string  THEN
    over c@ '* = over 0> and IF  do-expand on 1 /string  THEN ;

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

Variable expand-link
Variable expand-prefix
Variable expand-postfix

: ?expand ( addr u -- addr u' )  expand-link $!
    do-expand @ IF
	expand-prefix $@ expand-link 0 $ins
	expand-postfix $@ expand-link $+!  THEN
    0 >r
    BEGIN  expand-link $@ r@ /string  WHILE
	    r> 1+ >r
	    c@ '& = IF  s" amp;" expand-link r@ $ins  THEN
    REPEAT  drop rdrop
    expand-link $@ ;

: .link ( addr u -- ) dup >r '| -$split  dup r> = IF  2swap  THEN 
    link-options link $!
    link $@len 0= IF  2dup link $! ( s" .html" link $+! ) THEN
    link $@ ?expand
    href= s" a" tag link-icon?
    parse-string s" a" /tag link-size? link-sig? link-warn? ;
: >link ( -- )  '[ parse type '] parse .link ;

\ line handling

: char? ( -- c )  >in @ char swap >in ! $FF umin ;

: parse-tag ( addr u char -- )
    >r r@ parse .type
    r> parse 2swap tagged ;

: .text ( -- ) 	>in @ >r char drop
    source r@ /string >in @ r> - nip .type ;

Create do-words  $100 0 [DO] ' .text , [LOOP]

:noname '( emit 1 >in +! ; '( cells do-words + !

: bind-char ( xt -- )  char cells do-words + ! ;

: char>tag ( -- ) char >r
:noname bl sword postpone SLiteral r@ postpone Literal
    postpone parse-tag postpone ; r> cells do-words + ! ;

: >tag '\ parse type '\ parse tag ;

char>tag * b
char>tag / i
char>tag _ em
char>tag # code
:noname  '~ parse .type '~ parse .type ; '~ cells do-words + !

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
    BEGIN
	word? autoreplacements search-wordlist
	IF    execute  bl sword 2drop
	    source >in @ 1- /string drop c@ bl = >in +! bl true
	ELSE  char? dup r@ <>  THEN  WHILE
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
    BEGIN
	parse-line+ cr refill  WHILE
	source nip 0= UNTIL  THEN ;

: par ( addr u -- ) env?
    2dup tag parse-par /tag cr cr ;

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

: filenamize ( addr u -- )
    bounds ?DO
	I c@  dup 'A 'Z 1+ within IF  bl + nav+
	ELSE  dup 'a 'z 1+ within IF  nav+
	    ELSE  dup '0 '9 1+ within IF  nav+
		ELSE  dup  bl = over '- = or IF  '- nav+
		    ELSE  drop
		    THEN  THEN  THEN  THEN
    LOOP ;
: >nav ( addr u -- addr' u' )
    nav-name $!  create-navs @ 0=
    IF  s" navigate/nav.scm" r/w create-file throw create-navs !  THEN
    s' (script-fu-nav-file "' nav$ $! nav-name $@ nav$ $+!
    s' " "./navigate/' nav$ $+!  s" " nav-file $!
    nav-name $@ filenamize
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
true Value toc-image

: .toc-entry ( toc flag -- )
    swap cell+ dup @ swap cell+ dup cell+ $@ 2dup ?expand href=
    '# scan 1 /string toc-name $@ compare >r
    $@ toc-image IF  s" a" tag .img swap
	IF
	    case
		2 of  s" ^]|-@/arrow_up.jpg" .img  endof
		3 of
		    r@ 0= IF s" *]|-@/circle.jpg"
		    ELSE s" v]|-@/arrow_down.jpg"  THEN  .img  endof
	    endcase
	ELSE
	    case
		0 of  s" ^]|-@/arrow_up.jpg" .img  endof
		1 of  s" >]|-@/arrow_right.jpg" .img  endof
		2 of  s" *]|-@/circle.jpg" .img  endof
		3 of  s" v]|-@/arrow_down.jpg" .img  endof
	    endcase
	THEN
	s" a" /tag ." <!--" cr ." -->"
    ELSE
	'[ skip  2dup '| scan nip - 2swap swap
	IF
	    CASE
		2 OF  s" up" class=  ENDOF
		3 OF  r@ 0= IF  s" circle" ELSE  s" down"  THEN class=  ENDOF
	    ENDCASE
	ELSE
	    CASE
		0  OF  s" up" class=  ENDOF
		1  OF  s" right" class=  ENDOF
		2  OF  s" circle" class=  ENDOF
		3  OF  s" down" class=  ENDOF
	    ENDCASE
	THEN
	s" a" tag parse-string s" a" /tag ." <!--" cr ." -->"
    THEN
    rdrop
    1 toc-index +! toc-index @ /toc-line mod 0=
    IF  -env cr s" p" >env  THEN ;

: print-toc ( -- ) toc-index off cr
    toc-image IF  s" img-menu"  ELSE  s" menu"  THEN class=
    s" div" >env cr s" p" >env
    0 parse
    dup 0= IF  toc-name $! 0  ELSE
	toc-name $! toc-name $@ id= s" " s" a" tagged  2
    THEN  >r
    toc-link  BEGIN  @ dup  WHILE
	dup cell+ @ 3 = r@ 0= and IF  rdrop 1 >r ( s" br" tag/ cr )  THEN
	dup cell+ @ r@ >= IF  dup r@ 2 = .toc-entry  THEN
	dup cell+ @ 2 = r@ 2 = and IF  s" br" tag/ toc-index off THEN
    REPEAT  drop rdrop -env -env cr ;

\ handle global tags

: indent ( n -- )
    indentlevel @ over
    indentlevel !
    2dup < IF swap DO  -env   LOOP  EXIT THEN
    2dup > IF      DO   s" div" >env  LOOP EXIT THEN
    2dup = IF drop IF  -env  s" div" >env  THEN THEN
;
: +indent ( -- )
    indentlevel @ IF  -env indent= s" div" >env  THEN
;

wordlist constant longtags

Variable divs

longtags set-current

: --- 0 indent cr s" hr" tag/ cr ;
: *   1 indent +indent s" h1" dclass= s" h1" par s" " dclass= ;
: **  1 indent +indent s" h2" dclass= s" h2" par s" " dclass= ;
: *** 2 indent +indent s" h3" dclass= s" h3" par s" " dclass= ;
: --  0 indent cr print-toc ;
: &&  0 parse id= ;
: -   s" ul" env s" li" par ;
: +   s" ol" env s" li" par ;
: ?   s" dl" env s" dt" par ;
: :   s" dl" env s" dd" par ;
: -<< s" ul" env env? s" li" >env ;
: +<< s" ol" env env? s" li" >env ;
\ : ?<< s" dl" env env? s" dt" >env ; \ not allowed
: :<< s" dl" env env? s" dd" >env ;
: p<< s" p" >env ;
: <<  +env ;
: <*  s" center" class= ;
: <red  s" red" class= s" p" >env parse-par ;
: red> -env ;
: >>  -env ;
: *> ;
: ::  interpret ;
: .   end-sec on 0 indent ;
: :code s" pre" >env
    BEGIN  source >in @ /string .type cr refill  WHILE
	source s" :endcode" str= UNTIL  THEN
    -env ;
: :code-file s" pre" >env
    parse" slurp-file type -env ;
: \   postpone \ ;

definitions

: LT  get-order longtags swap 1+ set-order
    bl sword parser previous ; immediate

\ Table

: next-char ( -- char )  source drop >in @ + c@ ;
: next-table ( -- )
    BEGIN
	table-starts table# @ 2* + dup c@ dup
	IF    1- over c! 1+ c@ 1+  ELSE  swap 1+ c! 0  THEN
	dup WHILE  table# +!  REPEAT  drop
    table-format $@ table# @ /string drop c@ taligned ! ;
: next>align ( -- )
    next-char dup bl <> over '\ <> and
    IF  taligned ! 1 >in +!  ELSE  drop  THEN ;

: |tag ( addr u -- )
    next-table
    next-char '/ = IF  1 >in +!
	next-char digit?  IF
	    dup 1- table-starts table# @ 2* + c!
	    s" rowspan" opt# 1 >in +!  THEN
	next>align
    THEN
    next-char '\ = IF  1 >in +!
	next-char digit?  IF
	    dup 1- table-starts table# @ 2* + 1+ c!
	    dup 1- table# +!
	    s" colspan" opt# 1 >in +!  THEN
	next>align
    THEN
    taligned @ >talign >env
    1 table# +! ;
: |d  table# @ 0> IF  -env  THEN  s" td" |tag ;
: |h  table# @ 0> IF  -env  THEN  s" th" |tag ;
: |line  s" tr" >env table# off ;
: line|  1 >in +! -env -env cr ;

longtags set-current

: <| ( -- )  table-starts &20 erase
    s" table" class= s" div" >env
    bl sword table-format $! bl sword
    dup IF  s" border" opt  ELSE  2drop  THEN
    s" table" >env ;
: |> -env -env cr cr ;
: +| ( -- )
    |line  BEGIN  |h '| parse-to next-char '+ =  UNTIL line| ;
: -| ( -- )
    |line  BEGIN  |d '| parse-to next-char '- =  UNTIL line| ;
: =| ( -- )
    |line  |h '| parse-to
           BEGIN  |d '| parse-to next-char '= =  UNTIL line| ;

definitions

\ parse a section

: section-par ( -- )  >in off
    bl sword longtags search-wordlist
    IF    execute
    ELSE  source nip IF  >in off s" p" par  THEN  THEN ;
: parse-section ( -- )  end-sec off
    BEGIN  refill  WHILE
	section-par end-sec @  UNTIL  THEN  end-sec off ;

\ HTML head

Variable css-file
Variable print-file
Variable ie-css-file
Variable content
Variable _charset
Variable _lang
Variable _favicon

: lang@  ( -- addr u )
    _lang @ IF  _lang $@  ELSE  s" en"  THEN ;
: .css ( -- )
    css-file @ IF  css-file $@len IF
	    s" StyleSheet" s" rel" opt
	    css-file $@ href= s" screen" s" media" opt
	    s" text/css" s" type" opt s" link" tag/ cr
	THEN  THEN
    ie-css-file @ IF
	." <!--[if lt IE 7.0]>" cr
	.'    <style type="text/css">@import url(' ie-css-file $@ type ." );</style>" cr
	." <![endif]-->" cr
    THEN ;
: .print ( -- )
    print-file @ IF  print-file $@len IF
           s" StyleSheet" s" rel" opt
           print-file $@ href= s" print" s" media" opt
           s" text/css" s" type" opt s" link" tag/ cr
       THEN  THEN ;
: .title ( addr u -- )  1 envs ! oldenv off
    _charset $@ s" utf-8" str= 0=
    IF  .' <?xml version="1.0" encoding="' _charset $@ .upcase .' "?>' cr  THEN
    .' <!DOCTYPE html' cr
    .'   PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"' cr
    .'   "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">' cr
    s" http://www.w3.org/1999/xhtml" s" xmlns" opt
    lang@ s" xml:lang" opt lang@ s" lang" opt
    s" html" >env cr s" head" >env cr
    s" Content-Type" s" http-equiv" opt
    content $@ s" content" opt
    s" meta" tag/ cr .css .print
    _favicon @ IF
	s" shortcut icon" s" rel" opt
	_favicon $@ href=
	s" image/x-icon" s" type" opt
	s" link" tag/ cr  THEN
    s" title" tagged cr
    -env ;

\ HTML trailer

Variable public-key
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
    s" Mail|@/mail.gif" .img mail $@ mailto: mail-name $@ s" a" tagged
    public-key @ IF
	public-key $@ href=  s" a" tag
	s" PGP key|-@/gpg-asc.gif" .img s" a" /tag
    THEN
    -envs ;

\ top word

: maintainer ( -- )
    '< sword -trailing mail-name $! '> sword mail $! ;
: pgp-key ( -- )
    bl sword -trailing public-key $! ;
: charset ( -- )  s" application/xhtml+xml; charset=" content $!
    bl sword -trailing 2dup content $+! _charset $! ;

charset iso-8859-1

: created ( -- )
    bl sword orig-date $! ;
: icons
    bl sword icon-prefix $! ;
: lang
    bl sword _lang $! ;
: favicon
    bl sword _favicon $! ;
: expands '# sword expand-prefix $! bl sword expand-postfix $! ;

icons icons

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
: print-css ( -- ) parse" print-file $! ;
: ie-css ( -- ) parse" ie-css-file $! ;

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
  parse-par -env parse-section
  loadfile @ close-file swap 2dup or
  pop-file  drop throw throw
  s" wf-temp.wf" delete-file throw ;

\ simple text data base

Variable last-entry
Variable field#

: table: ( xt n -- )  Create 0 , ['] type , , ,  1 field# !
    DOES> 2 cells + 2@ >in @ >r longtags set-current
    Create definitions swap , r> >in !
    here last-entry !
    dup 0 DO  0 ,  LOOP
    1 DO  s" " last-entry @ I cells + $!  LOOP
    last-entry @ get-rest
    DOES> dup cell+ swap perform ;

: field:  Create field# @ , ['] type , 1 field# +!
DOES> @ cells last-entry @ + get-rest ;
: par:  Create field# @ , ['] eval-par , 1 field# +!
DOES> @ cells last-entry @ + get-par ;

: >field-rest >body @ cells postpone Literal postpone + ;
: >field ' >field-rest ; immediate

: db-line ( -- )
    BEGIN
	source >in @ /string nip  WHILE
	    '% parse  postpone SLiteral postpone type
	    '% parse dup IF
		'| $split 2swap
		sfind 0= abort" Field not found"
		dup postpone r@ >field-rest  postpone $@
		over IF  drop evaluate  ELSE
		    nip nip >body cell+ @ compile,
		THEN
	    ELSE  2drop  postpone cr  THEN
    REPEAT ;

: db-par ( -- )  LT postpone p<< postpone >r
    BEGIN  db-line refill  WHILE  next-char '. = UNTIL  1 >in +!  THEN
    postpone rdrop ( LT postpone >> ) ; immediate
