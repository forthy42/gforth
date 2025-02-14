\ quote: S\" and .\" words

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2002,2003,2005,2007,2008,2018,2019,2022,2023,2024 Free Software Foundation, Inc.

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

?: umin ( u1 u2 -- u )
    2dup u>
    if
	swap
    then
    drop ;

: char/ ; immediate

: parse-num-x  ( c-addr1 umax -- c-addr2 c )
    >r #0. rot source chars + over - char/ r> umin >number
    drop rot rot drop ;

: parse-num ( c-addr1 umax base -- c-addr2 c )
    ['] parse-num-x swap base-execute ;

create \-escape-table
 7 c, #bs c,  'c c,   'd c, #esc c,   #ff c,   'g c,
'h c,  'i c,  'j c,   'k c,  #lf c,   #lf c,  #lf c,
'o c,  'p c,  '" c,  #cr c,   's c,  #tab c,   'u c,
11 c,  'w c,  'x c,   'y c,    0 c,

: \-escape, ( c-addr1 -- c-addr2 )
    \ c-addr1 points at a char right after a '\', c-addr2 points right after
    \ the whole sequence, the translated chars are appended to the dictionary.
    dup c@
    dup 'U' = if
	drop char+ 8 16 parse-num xc, exit
    endif
    dup 'u' = if
	drop char+ 4 16 parse-num
	dup $DC00 and $D800 = if
	    >r  count '\' = >r count 'u' = r> and if
		4 16 parse-num dup $DC00 and $DC00 = if
		    $3FF and r> $3FF and #10 lshift or $10000 +
		    xc,  exit
		else
		    r> xc, xc,  exit
		endif
	    endif
	    -2 + r>
	endif
	xc, exit
    endif
    dup 'x' = if
	drop char+ 2 16 parse-num c, exit
    endif
    dup '0' '8' within if
	drop 3 8 parse-num c, exit
    endif
    dup 'n' = if
	\ \-escapes were designed to translate to one character, so
	\ this is quite ugly: copy all but the last char right away
	drop newline 1-
	2dup here swap chars dup allot move
	chars + c@
    else
        dup 'm' = if \ crlf; ugly, because it's two characters
            #cr c, \ first half, the rest follows below
        endif
	dup 'a' '{' within if
	    dup 'a' - chars \-escape-table + c@
	    tuck = IF '\' c, THEN
	endif
    endif
    c, char+ ;

Defer string-lineend
$Variable mlstringpos

s" End of string expected" exception >r

: singleline-strings ( -- never ) \ gforth-experimental
    \G set strings to end within a line (default).
    [: [ r@ ]L throw ;] is string-lineend ;

: multiline-strings ( -- parse-area' parse-end ) \ gforth-experimental
    \G set strings to span multiple lines
    [:  #lf c,
	source-id 0= IF
	    success-color ."  string" default-color cr
	    input-color  THEN
	refill  IF  source  ELSE
	    mlstringpos get-stack 2 - -rot 2>r restore-input drop
	    2r> source drop + swap input-lexeme!  [ r> ]L throw  THEN
	over + ;] is string-lineend ;

singleline-strings

: \"-parse ( "string"<"> -- c-addr u ) \ gforth-internal  backslash-quote-parse
\G parses string, translating @code{\}-escapes to characters (as in
\G C).  The resulting string resides at @code{here}.  See @code{S\"}
\G for the supported @code{\-escapes}.
    here >r
    save-input input-lexeme 2@ swap source drop - rot 2 + mlstringpos set-stack
    >in @ chars source chars over + >r + begin ( parse-area R: here parse-end )
	dup r@ u>= IF
	    drop rdrop string-lineend >r
	THEN
	dup c@ '" <> while \ hardcoded string end, might need hook here
	    dup c@ dup '\ = if ( parse-area c R: here parse-end )
		drop char+ dup r@ = abort" unfinished \-escape"
		\-escape,
	    else
		c, char+
	    endif
    repeat
    char+ source >r - r> min char/ >in !
    r> drop
    here r> - dup negate allot
    here swap char/ ;

:noname \"-parse save-mem ;
:noname \"-parse save-mem 2dup postpone sliteral drop free throw ;
interpret/compile: s\" ( Interpretation 'ccc"' -- c-addr u )	\ core-ext,file-ext s-backslash-quote
\G Interpretation: Parse the string @i{ccc} delimited by a @code{"}
\G (but not @code{\"}), and convert escaped characters as described
\G above.  Store the resulting string in newly allocated heap memory,
\G and push its descriptor @i{c-addr u}.
\G @*
\G Compilation @code{( '@i{ccc}"' -- )}: Parse the string @i{ccc}
\G delimited by a @code{"} (but not @code{\"}), and convert escaped
\G characters as described above.  Append the run-time semantics below
\G to the current definition.
\G @*
\G Run-time @code{( -- c-addr u )}: Push a descriptor for the
\G resulting string.

:noname \"-parse type ;
:noname postpone s\" postpone type ;
interpret/compile: .\" ( compilation 'ccc"' -- ; run-time -- )	\ gforth	dot-backslash-quote
\G Like @code{."}, but translates C-like \-escape-sequences (see
\G @code{S\"}).

0 [if] \ test
    s" 123" drop 10 parse-num-x 123 <> throw drop .s
    s" 123a" drop 10 parse-num   123 <> throw drop .s
    s" x1fg" drop \-escape, here 1- c@ 31 <> throw drop .s
    s" 00129" drop \-escape, here 1- c@ 10 <> throw drop .s
    s" a" drop \-escape, here 1- c@ 7 <> throw drop .s
    \"-parse " s" " str= 0= throw .s
    \"-parse \a\b\c\e\f\n\r\t\v\100\x40xabcde" dump
    s\" \a\bcd\e\fghijklm\12op\"\rs\tu\v" \-escape-table over str= 0= throw
    s\" \w\0101\x041\"\\" name wAA"\ str= 0= throw
    s\" s\\\" \\" ' evaluate catch 0= throw
[endif]
