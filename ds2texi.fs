\ documentation source to texi format converter

\ Copyright (C) 1995,1996,1997,1998,1999,2003,2005,2007,2008 Free Software Foundation, Inc.

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
	[char] - skip [char] - scan 1 /string
    THEN ;

: replace-_ ( c-addr u -- )
    \ replaces _ with -
    chars bounds
    +DO
	i c@ [char] _ =
	if
	    [char] - i c!
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
    get-current documentation set-current
    create
	latest name>string skip-prefix 2,		\ name
	[char] ) parse save-mem 2,	\ stack-effect
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
	[char] @ emit
    endif
    drop r> emit ;

: typetexi ( addr u -- )
    0
    ?do
	dup c@ emittexi
	char+
    loop
    drop ;

: print-short ( doc-entry -- )
    >r
    ." @findex "
    r@ doc-name 2@ typetexi
    ."  @var{ " r@ doc-stack-effect 2@ type ."  }  "
    r@ doc-wordset 2@ type
    cr
    ." @cindex "
    ." @code{" r@ doc-name 2@ typetexi ." }"
    cr
    r@ doc-name 2@ drop c@ [char] : <> if
	\ cut out words starting with :, info-lookup cannot handle them
	\ !! deal with : by replacing it here and in info-lookup?
	." @kindex "
	r@ doc-name 2@ typetexi
	cr
    endif
    ." @format" cr
    ." @code{" r@ doc-name 2@ typetexi ." }       "
    ." @i{" r@ doc-stack-effect 2@ type ." }       "
    r@ doc-wordset 2@ type ."        ``"
    r@ doc-pronounciation 2@ type ." ''" cr ." @end format" cr
    rdrop ;

: print-doc ( doc-entry -- )
    >r
    r@ print-short
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
    3 pick over str=
    if \ addr2 u2 is a prefix of addr1 u1
	r> /string -trailing documentation search-wordlist
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

: compare-ci ( addr1 u1 addr2 u2 -- n )
    \ case insensitive string compare
    \ !! works correctly only for comparing for equality
    2 pick swap -
    ?dup-0=-if
        capscomp
    else
	nip nip nip
	0<
	if
	    -1
	else
	    1
	endif
    endif  ;

: answord ( "name wordset pronounciation" -- )
    \ check the documentaion of an ans word
    name { D: wordname }
    name { D: wordset }
    name { D: pronounciation }
    wordname documentation search-wordlist
    if
	execute { doc }
	wordset doc doc-wordset 2@ compare-ci
	if 
	    ." wordset: " wordname type ." : '"  doc doc-wordset 2@ type ." ' instead of '" wordset type ." '" cr
	endif
	pronounciation doc doc-pronounciation 2@ compare-ci
	if
	    ." pronounciation: " wordname type ." : '" doc doc-pronounciation 2@ type ." ' instead of '" pronounciation type ." '" cr
	endif
    else
	." undocumented: " wordname type cr
    endif ;
