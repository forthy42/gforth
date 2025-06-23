\ documentation source to texi format converter

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 1995,1996,1997,1998,1999,2003,2005,2007,2008,2013,2018,2019,2021,2022,2023 Free Software Foundation, Inc.

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

\ documentation source can contain lines in the form `doc-word' and
\ `short-word'. These are converted to appropriate full or short
\ (without the description) glossary entries for word.

\ The glossary entries are generated from data present in the wordlist
\ `documentation'. Each word resides there under its own name.

\ gforth versions for words
\ words are processed from older to younger versions

\ you can output all words with version numbers with
\ gforth ds2texi.fs -e "args-gforth-versions print-word-versions bye" doc/words/*-words|sort |less

require hold-number-line.fs

#0. 2value gforth-version-string
wordlist constant gforth-versions-wl

: current-wrapper ( wordlist xt -- )
    get-current >r swap set-current catch r> set-current throw ;

: note-word-version ( c-addr u -- )
    \ keep the first version of the word seen
    2dup gforth-versions-wl find-name-in if
        2drop exit then
    gforth-versions-wl
    [: nextname gforth-version-string 2constant ;] current-wrapper ;

: each-word ( c-addr u xt -- )
    \ perform xt for every word in string c-addr u
    [{: xt: xt :}l begin
            parse-name dup while
                xt repeat
    2drop ;] execute-parsing ;

: file-gforth-versions {: c-addr u :}
    \ c-addr u is a file name of the form "...doc/words/<version>-words".
    \ Associates all words in the file with <version> unless the words
    \ already have an earlier association.
    c-addr u "doc/words/" dup >r search 0= #-32 and throw
    r> /string "-words" nip - to gforth-version-string
    c-addr u slurp-file `note-word-version each-word ;

: args-gforth-versions ( -- )
    begin
        next-arg dup while
            file-gforth-versions repeat
    2drop ;

: print-word-versions ( -- )
    \ shows in which version the words have appeared
    gforth-versions-wl
    [: dup cr name>string type space name>interpret >body 2@ type ;]
    map-wordlist ;

\ #line stuff

2variable ds-filename
variable ds-linenumber
1 ds-linenumber !

: .#line ( -- )
    ." #line " ds-linenumber ? '"' emit ds-filename 2@ type '"' emit cr ;

\ deal with .fd files

script? [IF]
    warnings off
[THEN]

wordlist constant documentation

struct
    cell% 2* field doc-name
    cell% 2* field doc-stack-effect
    cell% 2* field doc-wordset
    cell% 2* field doc-pronounciation
    cell% 2* field doc-description
    cell%    field doc-count
    cell% 2* field doc-loc
end-struct doc-entry

create description-buffer 4096 chars allot

: get-description ( -- addr u )
    description-buffer
    begin
	refill
    while
	source nip
    while
	source swap >r 2dup r> -rot cmove
	chars +
	#lf over c! char+
    repeat then
    description-buffer tuck - ;

: skip-prefix ( c-addr1 u1 -- c-addr2 u2 )
    2dup s" --" string-prefix?
    IF
	'-' skip '-' scan 1 /string
    THEN ;

: replace-_ ( c-addr u -- )
    \ replaces _ with -
    chars bounds
    +DO
	i c@ '_' =
	if
	    '-' i c!
	endif
	1 chars
    +loop ;
    
: condition-stack-effect ( c-addr1 u1 -- c-addr2 u2 )
    save-mem 2dup replace-_ ;

wordlist constant wordsets
get-current wordsets set-current
\ true means that a word with that wordset must occur in the
\ documentation.  false means that the wordset is known, but the word
\ need not occur in the documentation.

\ standard wordsets
true  constant block
true  constant block-ext
true  constant core
true  constant core,block
true  constant core,exception-ext
true  constant core,file
true  constant core,search
true  constant core,tools-ext
true  constant core,xchar-ext
true  constant core-ext
true  constant core-ext,block-ext
true  constant core-ext,block-ext,file-ext
true  constant core-ext,file
true  constant core-ext,file-ext
true  constant core-ext,xchar-ext
false constant core-ext-obsolescent
true  constant double
true  constant double-ext
true  constant exception
true  constant exception-ext
true  constant facility
true  constant facility-ext
true  constant file
true  constant file-ext
true  constant floating
true  constant floating-ext
true  constant local
true  constant local-ext
true  constant memory
true  constant memory-ext
true  constant search
true  constant search-ext
true  constant string
true  constant string-ext
true  constant tools
true  constant tools-ext
true  constant xchar
true  constant xchar-ext
true  constant recognizer

\ environment query names
true  constant environment

\ wordsets for non-standard words
true  constant gforth
true  constant gforth-environment
true  constant gforth-experimental
false constant gforth-internal
false constant gforth-obsolete

\ libraries independent of Gforth
true  constant mini-oof
true  constant mini-oof2
true  constant minos2
true  constant minos2-bidi
true  constant objects
true  constant oof
true  constant regexp-cg
true  constant regexp-pattern
true  constant regexp-replace
true  constant cilk
set-current

: check-wordset ( c-addr u -- )
    wordsets find-name-in 0= if
        [: cr current-view .sourceview ." :unknown wordset"
        cr source type ;] stderr outfile-execute
    then ;
    
: condition-wordset ( c-addr1 u1 -- c-addr2 u2 )
    dup 0=
    if
	2drop s" unknown"
    else
        save-mem 2dup check-wordset
    endif ;

: condition-pronounciation ( c-addr1 u1 -- c-addr2 u2 )
    save-mem 2dup replace-_ ;

\ name>pronounciation

33  constant pronounciation-table-low
127 constant pronounciation-table-hi+1

align here pronounciation-table-hi+1 pronounciation-table-low - 2* cells ( a u )
dup allot 2dup erase
drop pronounciation-table-low 2* cells - constant pronounciation-table

: pronounce! ( c c-addr u -- )
    rot
    dup pronounciation-table-hi+1 pronounciation-table-low within #-24 and throw
    2* cells pronounciation-table + 2! ;

'!' "store"         pronounce!
'"' "quote"         pronounce!
'#' "number"        pronounce!
'$' "dollar"        pronounce!
'%' "percent"       pronounce!
'&' "and"           pronounce!
''' "tick"          pronounce!
'(' "paren"         pronounce!
')' "close-paren"   pronounce!
'*' "star"          pronounce!
'+' "plus"          pronounce!
',' "comma"         pronounce!
'.' "dot"           pronounce!
'/' "slash"         pronounce!
'0' "zero"          pronounce!
'1' "one"           pronounce!
'2' "two"           pronounce!
'3' "three"         pronounce!
'4' "four"          pronounce!
'5' "five"          pronounce!
'6' "six"           pronounce!
'7' "seven"         pronounce!
'8' "eight"         pronounce!
'9' "nine"          pronounce!
':' "colon"         pronounce!
';' "semicolon"     pronounce!
'<' "less"          pronounce!
'=' "equals"        pronounce!
'>' "to"            pronounce!
'?' "question"      pronounce!
'@' "fetch"         pronounce!
'[' "left-bracket"  pronounce!
'\' "backslash"     pronounce!
']' "right-bracket" pronounce!
'^' "caret"         pronounce!
'`' "backtick"      pronounce!
'{' "left-brace"    pronounce!
'|' "bar"           pronounce!
'}' "right-brace"   pronounce!
'~' "tilde"         pronounce!

: name>pronounciation-char ( dash?1 c -- dash?2 )
    dup >r pronounciation-table-low pronounciation-table-hi+1 within if
        r@ 2* cells pronounciation-table + 2@ dup if ( dash?1 c-addr u )
            rot if '-' hold then
            holds -1 rdrop exit
        then
        2drop
    then
    0< if '-' hold then
    r> hold 1 ;

: name>pronounciation ( c-addr1 u1 -- c-addr2 u2 )
    \G c-addr2 u2 is a guess at the pronounciation for c-addr1 u1; if
    \G you want a different pronounciation, give it explicitly.
    dup >r assert( dup )
    <<# 0 -rot over + 1- do ( dashflag )
        i c@ name>pronounciation-char
    -1 +loop
    drop 0 0 #>
    dup r> <> if
        save-mem
    else
        2drop 0 0
    then
    #>> ;
    
: rest-of-line-ok? ( -- flag )
    source >in @ /string s" -- " search if
        s" )" search if
            -trailing nip 1 > exit then
    then
    2drop 0 ;

: make-doc ( -- )
    sourceline# 0 <<# hold#line #> save-mem #>> {: d: syncline :}
    parse-name 2dup documentation find-name-in if
        2drop get-description 2drop exit then
    rest-of-line-ok? 0= if
        2drop get-description 2drop exit then
    nextname get-current documentation set-current
    create
	latest name>string skip-prefix 2,		\ name
	')' parse save-mem 2,	\ stack-effect
	parse-name condition-wordset 2,	\ wordset
	parse-name dup	\ pronounciation
	if
	    condition-pronounciation
	else
	    2drop latest name>string skip-prefix name>pronounciation
	endif
	2,
        get-description save-mem 2,
        0 , \ doc-count
        syncline 2, \ doc-loc
    set-current ;

: emittexi ( c -- )
    case
        ',' of ." @comma{}" endof
        '\' of ." @backslashchar{}" endof
        '#' of ." @hashchar{}" endof
        s" @{}&" third scan nip 0<> ?of '@' emit emit endof
        dup emit
    endcase ;

: typetexi ( addr u -- )
    0
    ?do
	dup c@ emittexi
	char+
    loop
    drop ;

: emittexi-min ( c -- )
    \ only deal with @ { }
    case
        s" @{}" third scan nip 0<> ?of '@' emit emit endof
        dup emit
    endcase ;

: typetexi-min ( addr u -- )
    0
    ?do
	dup c@ emittexi-min
	char+
    loop
    drop ;

: typeuntexi ( c-addr u -- )
    bounds u+do
        1 i i c@ '@' = if
            s" @{}" i 1+ c@ scan nip 0<> if
                1+ swap 1+ swap then then
        c@ emit
    +loop ;

: untexi ( c-addr1 u1 -- c-addr2 u2 )
    ['] typeuntexi >string-execute ;

: type-alpha-dash ( c-addr u -- )
    \ replace all non-letters with "-"
    bounds ?do
        i c@ dup toupper 'A' 'Z' 1+ within 0= if drop '-' then emit
    loop ;

: doc-pronounciation-string {: doc -- c-addr u :}
    \ pronounciation of doc, if present, otherwise wordname
    doc doc-pronounciation 2@ dup 0= if
        2drop doc doc-name 2@
    then ;

: typeword ( addr u -- )
    2dup documentation find-name-in dup if
        name>interpret >body >r
        texinfo-link if
            ." @link{" r@ doc-wordset 2@ type-alpha-dash ." --"
                       r@ doc-pronounciation-string type ." ,"
        then
        typetexi rdrop
        texinfo-link if
            ." }"
        then
    else
        drop typetexi
    then ;

: typetexi1 ( c-addr u -- )
    \ like typetexi, but prints every word with typeword
    case {: d: str1 :}
        str1 (parse-white) {: d: str2 :}
        str2 nip 0= ?of endof
        str2 s" \" str= ?of endof
        str1 drop str2 drop over - type str2 typeword
        str2 + str1 + over -
    next-case
    str1 typetexi ;

: type-replace@word ( addr u -- )
    \ replace @word{<word>}... (terminated by white space) with
    \ @code{<word>}... where <word> is typetexi'd.
    s" @word{" {: d: w :}
    begin {: d: s :}
        s w search while {: d: match :}
            s drop match drop over - type
            ." @code{"
            match w nip /string {: d: match1 :}
            match1 (parse-white) '}' scan-back 1- {: d: word :}
            word typeword
            match1 word nip /string            
    repeat
    type ;

: print-wordset ( doc-entry -- )
    dup >r doc-wordset 2@ 2dup type "gforth" str= if
        r@ doc-name 2@ gforth-versions-wl find-name-in ?dup-if
            '-' emit name>interpret execute type then then
    rdrop ;

: print-short ( doc-entry -- )
    >r
    ." @findex "
    r@ doc-name 2@ typetexi
    ."  ( @var{ " r@ doc-stack-effect 2@ type ."  } ) "
    r@ print-wordset
    cr
    ." @cindex "
    ." @code{" r@ doc-name 2@ typetexi ." }"
    cr
    r@ doc-name 2@ drop c@ ':' <> if
	\ cut out words starting with :, info-lookup cannot handle them
	\ !! deal with : by replacing it here and in info-lookup?
	." @kindex "
	r@ doc-name 2@ typetexi
	cr
    endif
    ." @format" cr
    \ The generated @anchors lead to TeX problems with TeXinfo 6.8.
    \ They are only needed when @link works, so just disable them when not
    texinfo-link if
        ." @anchor{" r@ doc-wordset 2@ type-alpha-dash ." --"
                     r@ doc-pronounciation-string typetexi ." }"
    then
    ." @code{" r@ doc-name 2@ typetexi ." } "
    ." ( @i{" r@ doc-stack-effect 2@ type ." }) "
    r@ print-wordset 
    r@ doc-pronounciation 2@ dup if
        2dup ."  ``" type ." ''" then
    2drop rdrop
    cr ." @end format" cr ;

: print-doc ( doc-entry -- )
    >r
    r@ doc-loc 2@ type cr
    r@ print-short
    1 r@ doc-count +!
    r@ doc-description 2@ dup 0<>
    if
	\ ." @iftex" cr ." @vskip-0ex" cr ." @end iftex" cr
	type-replace@word cr cr
	\ ." @ifinfo" cr ." @*" cr ." @end ifinfo" cr cr
    else
	2drop cr
    endif
    rdrop ;

: do-doc ( addr1 u1 addr2 u2 xt -- f )
    \ xt is the word to be executed if addr1 u1 is a string starting
    \ with the prefix addr2 u2 and continuing with a word in the
    \ wordlist `documentation'. f is true if xt is executed.
    >r dup >r
    fourth over str=
    if \ addr2 u2 is a prefix of addr1 u1
	r> safe/string -trailing documentation search-wordlist
	if \ the rest of addr1 u1 is in documentation
	    execute r> execute .#line true
	else
	    rdrop false
	endif
    else
	2drop 2rdrop false
    endif ;

defer type-ds ( c-addr u )
' type-replace@word is type-ds \ typetexi1 between @source and @end source

: process-line ( addr u -- )
    case
        2dup s" doc-"   ['] print-doc   do-doc ?of endof
        2dup s" short-" ['] print-short do-doc ?of endof
        2dup s" @source" string-prefix? ?of
            .\" @example\n" `typetexi1 is type-ds endof
        2dup s" @end source" string-prefix? ?of
            .\" @end example\n" `type-replace@word is type-ds endof
        2dup type-ds cr
        0 endcase
    2drop ;

1024 constant doclinelength

create docline doclinelength chars allot

: ds2texi ( file-id -- )
    >r .#line
    begin
        docline doclinelength r@ read-line throw
    while
            dup doclinelength = abort" docline too long"
            docline swap process-line
            1 ds-linenumber +!
    repeat
    drop rdrop ;

: filename-ds2texi ( c-addr u -- )
    2dup ds-filename 2!
    r/o open-file throw ds2texi ;

: checkword {: D: wordname D: wordset D: pronounciation -- :}
    wordname documentation search-wordlist
    if
	execute { doc }
	wordset doc doc-wordset 2@ capscompare
	if 
	    ." wordset: " wordname type ." : '"  doc print-wordset ." ' instead of '" wordset type ." '" cr
	endif
	pronounciation doc doc-pronounciation-string capscompare
	if
            ." pronounciation: " wordname type ." : '"
            doc doc-pronounciation-string type ." ' instead of '"
            pronounciation type ." '" cr
	endif
    else
	." undocumented: " wordname type cr
    endif ;

: answord ( "name wordset pronounciation" -- )
    \ check the documentaion of an ans word
    parse-name parse-name parse-name checkword ;

: hyphenate ( c-addr u -- )
    \ replace spaces with hyphens
    bounds ?do
        i c@ bl = if '-' i c! then
    loop ;

: input-stream-checkwords ( -- )
    \ the input stream consists of tab-separated records, one record per line
    begin
        #tab parse save-mem {: D: section :}
        #tab parse save-mem {: D: name :}
        #tab parse dup if 1 /string 1- save-mem else name then
           {: D: pronounciation :}
        #tab parse save-mem 2dup hyphenate {: D: wordset :}
        name wordset pronounciation checkword
        \ cr ." answord " name type ." |" wordset type ." |" pronounciation type
    refill 0= until ;

: file-checkwords ( c-addr u -- )
    \ check all the words in the file named c-addr u
    \ The file is tab-separated: section name "pronounciation" wordset
    r/o open-file throw ['] input-stream-checkwords execute-parsing-file ;

: report-#use {: nt -- f :}
    nt name>interpret >body {: doc :}
    doc doc-count @ {: #use :}
    doc doc-wordset 2@ wordsets find-name-in {: wordset-nt :}
    wordset-nt if
        wordset-nt name>interpret execute  #use 1 <> and if
            #use . nt name>string type cr then then
    true ;

: report-#uses ( -- )
    \ print all documented words with count!=1
    ['] report-#use documentation traverse-wordlist ;

: report-#uses-file ( c-addr u -- )
    \ print all documented words with count!=1 in the file with the
    \ name c-addr u
    w/o create-file throw ['] report-#uses swap outfile-execute ;
