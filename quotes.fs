\ quote: S\" and .\" words

\ Copyright (C) 2002,2003 Free Software Foundation, Inc.

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

: char/ ; immediate

: parse-num-x  ( c-addr1 base -- c-addr2 c )
    base !
    0. rot source chars + over - char/ >number
    drop rot rot drop ;

: parse-num ( c-addr1 base -- c-addr2 c )
    base @ >r
    ['] parse-num-x catch
    r> base ! throw ;

create \-escape-table
    7 c,        8 c,  char c c,  char d c,      27 c,      12 c,  char g c,
char h c,  char i c,  char j c,  char k c,  char l c,  char m c,      10 c,
char o c,  char p c,  char " c,      13 c,  char s c,       9 c,  char u c,
    11 c,

: \-escape ( c-addr1 -- c-addr2 c )
    \ c-addr1 points at a char right after a '\', c-addr2 points right
    \ after the whole sequence, c is the translated char
    dup c@
    dup [char] x = if
	drop char+ 16 parse-num exit
    endif
    dup [char] 0 [char] 8 within if
	drop 8 parse-num exit
    endif
    dup [char] n = if
	\ \-escapes were designed to translate to one character, so
	\ this is quite ugly: copy all but the last char right away
	drop newline 1-
	2dup here swap chars dup allot move
	chars + c@
    else
	dup [char] a [char] w within if
	    [char] a - chars \-escape-table + c@
	endif
    endif
    1 chars under+ ;

: \"-parse ( "string"<"> -- c-addr u )
\G parses string, translating @code{\}-escapes to characters (as in
\G C).  The resulting string resides at @code{here char+}.  The
\G supported @code{\-escapes} are: @code{\a} BEL (alert), @code{\b}
\G BS, @code{\e} ESC (not in C99), @code{\f} FF, @code{\n} newline,
\G @code{\r} CR, @code{\t} HT, @code{\v} VT, @code{\"} ",
\G @code{\}[0-7]+ octal numerical character value, @code{\x}[0-9a-f]+
\G hex numerical character value; a @code{\} before any other
\G character represents that character (only ', \, ? in C99).
    here >r
    >in @ chars source chars over + >r + begin ( parse-area R: here parse-end )
	dup r@ < while
	    dup c@ [char] " <> while
		dup c@ dup [char] \ = if ( parse-area c R: here parse-end )
		    drop char+ dup r@ = abort" unfinished \-escape"
		    \-escape c,
		else
		    c, char+
		endif
	repeat then
    char+ source >r - r> min char/ >in !
    r> drop
    here r> - dup negate allot
    here swap char/ ;

:noname \"-parse save-mem ;
:noname \"-parse save-mem 2dup postpone sliteral drop free throw ;
interpret/compile: s\" ( compilation 'ccc"' -- ; run-time -- c-addr u )	\ gforth	s-backslash-quote
\G Like @code{S"}, but translates C-like \-escape-sequences into
\G single characters.  See @code{\"-parse} for details.

:noname \"-parse type ;
:noname postpone s\" postpone type ;
interpret/compile: .\" ( compilation 'ccc"' -- ; run-time -- )	\ gforth	dot-backslash-quote

0 [if] \ test
    s" 123" drop 10 parse-num-x 123 <> throw drop .s
    s" 123a" drop 10 parse-num   123 <> throw drop .s
    s" x1fg" drop \-escape 31 <> throw drop .s
    s" 00129" drop \-escape 10 <> throw drop .s
    s" a" drop \-escape 7 <> throw drop .s
    \"-parse " s" " str= 0= throw .s
    \"-parse \a\b\c\e\f\n\r\t\v\100\x40xabcde" dump
    s\" \a\bcd\e\fghijklm\12op\"\rs\tu\v" \-escape-table over str= 0= throw
    s\" \w\0101\x041\"\\" name wAA"\ str= 0= throw
    s\" s\\\" \\" ' evaluate catch 0= throw
[endif]
