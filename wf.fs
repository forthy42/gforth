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

\ link creation

Variable link
Variable link-suffix
Variable iconpath

Variable do-size

: link-icon? ( -- )
    link $@ '. $split link-suffix $! 2drop s" .*" link-suffix $+!
    s" icons" open-dir throw >r
    BEGIN
	pad $100 r@ read-dir throw  WHILE
	pad swap 2dup link-suffix $@ filename-match
	IF  s" icons/" iconpath $! iconpath $+!
	    iconpath $@ src= s" img" tag true
	ELSE  2drop  false  THEN
    UNTIL  ELSE  drop  THEN \ ELSE  '( emit link-suffix $@ 2 - type ') emit  THEN
    r> close-dir throw ;

: link-size? ( -- )  do-size @ 0= ?EXIT
    link $@ r/o open-file IF  drop  EXIT  THEN >r
    r@ file-size throw $400 um/mod nip ."  (" 0 u.r ." k)"
    r> close-file throw ;

: link-options ( addr u -- addr' u' )
    do-size off
    over c@ '% = over 0> and IF  do-size on  1 /string  THEN ;

: .link ( -- )  '[ parse type '] parse '| $split
    link-options link $!
    link $@len 0= IF  2dup link $! s" .html" link $+!  THEN
    link-icon? link $@ href= s" a" tagged
    link-size? ;

: .img ( -- ) '{ parse type '} parse '| $split
    dup IF  2swap alt=  ELSE  2drop  THEN  src= s" img" tag ;

\ line handling

: char? ( -- c )  >in @ char swap >in ! ;
: parse-tag ( addr u char -- )
    >r r@ parse type
    r> parse 2swap tagged ;

: .bold ( -- )  s" b" '* parse-tag ;
: .em   ( -- )  s" em" '_ parse-tag ;

: do-word ( char -- )
    CASE
	'* OF .bold ENDOF
	'_ OF .em   ENDOF
	'[ OF .link ENDOF
	'{ OF .img  ENDOF
	>in @ >r char drop
	source r@ /string >in @ r> - nip type
    ENDCASE ;

: parse-line ( -- )
    BEGIN  char? do-word source nip >in @ = UNTIL ;

: parse-to ( char -- ) >r
    BEGIN  char? dup r@ <> WHILE
	do-word source nip >in @ = UNTIL  ELSE  drop  THEN
    r> parse type ;

\ paragraph handling

: parse-par ( -- )
    BEGIN  parse-line cr refill  WHILE
	source nip 0= UNTIL  THEN ;

: par ( addr u -- ) env? 2dup tag parse-par /tag cr cr ;
: line ( addr u -- ) env? 2dup tag parse-line /tag cr cr ;

\ handle global tags

wordlist constant longtags

Variable end-sec

longtags set-current

: --- cr s" hr" tag cr ;
: * s" h1" line ;
: ** s" h2" line ;
: *** s" h3" line ;
: - s" ul" env s" li" par ;
: + s" ol" env s" li" par ;
: << +env ;
: <* s" center" >env ;
: >> -env ;
: *> -env ;
: . end-sec on ;
: \ postpone \ ;

definitions
    
\ Table

Variable table-format
Variable table#

: |tag  table-format $@ table# @ /string drop c@
    CASE
	'l OF  s" left"   align=  ENDOF
	'r OF  s" right"  align=  ENDOF
	'c OF  s" center" align=  ENDOF
    ENDCASE  >env  1 table# +! ;
: |d  table# @ IF  -env  THEN  s" td" |tag ;
: |h  table# @ IF  -env  THEN  s" th" |tag ;
: |line  s" tr" >env  table# off ;
: line|  -env -env cr ;

longtags set-current

: <| s" table" >env bl sword table-format $! ;
: |> -env ;
: +| |line
    BEGIN
	|h '| parse-to char? '+ =  UNTIL line| ;
: -| |line
    BEGIN
	|d '| parse-to char? '- =  UNTIL line| ;

definitions

\ parse a section

: refill-loop ( -- )  end-sec off
    BEGIN  refill  WHILE  >in off
	bl sword find-name
	?dup IF  name>int execute
	ELSE  source nip IF  >in off s" p" par  THEN  THEN
	end-sec @ UNTIL  THEN ;
: parse-section ( -- )
    get-order  longtags 1 set-order  refill-loop set-order ;

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

: maintainer
    bl sword mail $! '" parse 2drop '" parse mail-name $! ;

: wf ( -- )
    outfile-id >r
    bl sword r/w create-file throw to outfile-id
    '" parse 2drop '" parse .title
    +env s" body" env
    ['] parse-section catch .trailer
    outfile-id close-file throw
    r> to outfile-id
    dup 0< IF  throw  ELSE  drop  THEN ;

\ simple text data base

: get-rest ( addr -- ) 0 parse -trailing rot $! ;

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

: >field  ' >body @ cells postpone Literal postpone + ; immediate
