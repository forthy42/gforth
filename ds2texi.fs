\ documentation source to texi format converter

\ documentation source can contain lines in the form `doc-word' and
\ `short-word'. These are converted to appropriate full or short
\ (without the description) glossary entries for word.

\ The glossary entries are generated from data present in the wordlist
\ `documentation'. Each word resides there under its own name.

script? [IF]
warnings off
include search-order.fs
include struct.fs
include debugging.fs
[THEN]

wordlist constant documentation

struct
    2 cells: field doc-name
    2 cells: field doc-stack-effect
    2 cells: field doc-wordset
    2 cells: field doc-pronounciation
    2 cells: field doc-description
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

: make-doc ( -- )
    get-current documentation set-current
    create
	last @ name>string 2,		\ name
	[char] ) parse save-string 2,	\ stack-effect
	bl parse-word save-string 2,	\ wordset
	bl parse-word dup		\ pronounciation
	if
	    save-string
	else
	    2drop last @ name>string
	endif
	2,
	get-description save-string 2,
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
    >r ." @format" cr
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
	type ." @*" cr
    else
	2drop cr
    endif
    rdrop ;

: do-doc ( addr1 u1 addr2 u2 xt -- f )
    \ xt is the word to be executed if addr1 u1 is a string starting
    \ with the prefix addr2 u2 and continuing with a word in the
    \ wordlist `documentation'. f is true if xt is executed.
    >r dup >r
    3 pick over compare 0=
    if \ addr2 u2 is a prefix of addr1 u1
	r> /string documentation search-wordlist
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

script? [IF]
include prims2x.fs
s" primitives.b" ' register-doc process-file
require doc.fd
require crossdoc.fd
s" gforth.ds" r/o open-file throw ds2texi bye
[THEN]
