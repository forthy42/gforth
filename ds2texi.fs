\ documentation source to texi format converter

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 1995,1996,1997,1998,1999,2003,2005,2007,2008,2013,2018,2019,2021 Free Software Foundation, Inc.

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
    
: condition-wordset ( c-addr1 u1 -- c-addr2 u2 )
    dup 0=
    if
	2drop s" unknown"
    else
	save-mem
    endif ;

: condition-pronounciation ( c-addr1 u1 -- c-addr2 u2 )
    save-mem 2dup replace-_ ;

: make-doc ( -- )
    parse-name 2dup documentation find-name-in if
        2drop get-description 2drop exit then
    nextname get-current documentation set-current
    create
	latest name>string skip-prefix 2,		\ name
	')' parse save-mem 2,	\ stack-effect
	bl sword condition-wordset 2,	\ wordset
	bl sword dup	\ pronounciation
	if
	    condition-pronounciation
	else
	    2drop latest name>string skip-prefix
	endif
	2,
	get-description save-mem 2,
    set-current ;

: emittexi ( c -- )
    >r
    s" @{}" r@ scan 0<>
    if
	'@' emit
    endif
    drop r> emit ;

: typetexi ( addr u -- )
    0
    ?do
	dup c@ emittexi
	char+
    loop
    drop ;

: print-wordset ( doc-entry -- )
    dup >r doc-wordset 2@ 2dup type "gforth" str= if
        r@ doc-name 2@ gforth-versions-wl find-name-in ?dup-if
            '-' emit name>interpret execute type then then
    rdrop ;

: print-short ( doc-entry -- )
    >r
    ." @findex "
    r@ doc-name 2@ typetexi
    ." ( @var{ " r@ doc-stack-effect 2@ type ."  } ) "
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
    ." @code{" r@ doc-name 2@ typetexi ." } "
    ." ( @i{" r@ doc-stack-effect 2@ type ." }) "
    r@ print-wordset ."  ``"
    r@ doc-pronounciation 2@ type ." ''" cr ." @end format" cr
    rdrop ;

: print-doc ( doc-entry -- )
    dup
    >r print-short
    r@ doc-description 2@ dup 0<>
    if
	\ ." @iftex" cr ." @vskip-0ex" cr ." @end iftex" cr
	type cr cr
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
	    execute r> execute true
	else
	    rdrop false
	endif
    else
	2drop 2rdrop false
    endif ;

: process-line ( addr u -- )
    2dup s" doc-" ['] print-doc do-doc 0=
    if
	2dup s" short-" ['] print-short do-doc 0=
	if
	    type cr EXIT
	endif
    endif
    2drop ;

1024 constant doclinelength

create docline doclinelength chars allot

: ds2texi ( file-id -- )
    >r
    begin
	docline doclinelength r@ read-line throw
    while
	dup doclinelength = abort" docline too long"
	docline swap process-line
    repeat
    drop rdrop ;

: answord ( "name wordset pronounciation" -- )
    \ check the documentaion of an ans word
    name { D: wordname }
    name { D: wordset }
    name { D: pronounciation }
    wordname documentation search-wordlist
    if
	execute { doc }
	wordset doc doc-wordset 2@ capscompare
	if 
	    ." wordset: " wordname type ." : '"  doc print-wordset ." ' instead of '" wordset type ." '" cr
	endif
	pronounciation doc doc-pronounciation 2@ capscompare
	if
	    ." pronounciation: " wordname type ." : '" doc doc-pronounciation 2@ type ." ' instead of '" pronounciation type ." '" cr
	endif
    else
	." undocumented: " wordname type cr
    endif ;
