\ extended characters (either 8bit or UTF-8, possibly other encodings)
\ and their fixed-size variant

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2005,2006,2007,2008,2011,2012,2015,2016,2019,2021,2022,2023,2025 Free Software Foundation, Inc.

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

\ We can do some of these (and possibly faster) by just using the
\ utf-8 words with an appropriate setting of max-single-byte, but I
\ like to see how an 8bit setting without UTF-8 stuff looks like.

user-o xc-vector
0 0
umethod xemit ( xc -- ) \ xchar x-emit
\G Display extended char @i{xc}.

umethod xkey ( -- xc ) \ xchar x-key
\G Reads an extended character @i{xc} xchar from the terminal without
\G printing it. This will discard all input events until all bytes of
\G @i{xc} have been received.

umethod xchar+ ( xc-addr1 -- xc-addr2 ) \ xchar x-char-plus
\G @i{xc-addr2} is the address of the next xchar behind the one pointed
\G to by @i{xc-addr}.

umethod xchar- ( xc-addr1 -- xc-addr2 ) \ xchar-ext x-char-minus
\G @i{xc-addr2} is the address of the previous xchar in front of the
\G one pointed to by @i{xc-addr}.

umethod +x/string ( xc-addr1 u1 -- xc-addr2 u2 ) \ xchar-ext plus-x-slash-string
\G @i{xc-addr1 u1} is a string of @i{u1} chars.  @var{xc-addr2} is the
\G address of the next xchar behind the one pointed to by @i{xc-addr}.
\G @i{u2} is the size (in chars) of the rest of the string.

umethod x\string- ( xc-addr u1 -- xc-addr u2 ) \ xchar-ext x-backslash-string-minus
\G @i{xc-addr1 u1} is a string of @i{u1} chars.  @i{u2} is the size of
\G the string without its last xchar.

umethod xc@ ( xc-addr -- xc ) \ xchar-ext	x-c-fetch
\G @i{xc} is the xchar starting at @var{xc-addr1}.

umethod xc!+ ( xc xc-addr1 -- xc-addr2 ) \ xchar	x-c-store
\G Stores the xchar @var{xc} at @var{xc-addr1}. @var{xc-addr2} is the
\G next unused address in the buffer.  Note that this writes up to 4
\G bytes, so you need at least 3 bytes of padding after the end of the
\G buffer to avoid overwriting useful data if you only check the
\G address against the end of the buffer.

umethod xc!+? ( xc xc-addr1 u1 -- xc-addr2 u2 f ) \ xchar x-c-store-plus-query
\G Stores the xchar @var{xc} into the buffer starting at address
\G @var{xc-addr1}, @var{u1} chars large. @var{xc-addr2} points to the
\G first memory location after @var{xc}, @var{u2} is the remaining
\G size of the buffer. If the xchar @var{xc} did fit into the buffer,
\G @var{f} is true, otherwise @var{f} is false, and @var{xc-addr2}
\G @var{u2} equal @var{xc-addr1} @var{u1}. XC!+?  is safe against buffer
\G overflows, and therefore preferred over XC!+.

umethod xc@+ ( xc-addr1 -- xc-addr2 xc ) \ xchar	x-c-fetch-plus
\G @i{xc} is the xchar starting at @var{xc-addr1}.  @i{xc-addr2} points
\G to the first memory location after @i{xc}.

umethod xc-size ( xc -- u ) \ xchar x-c-size
\G The xchar @i{xc} occupies @i{u} chars in memory.

umethod x-size ( xc-addr u1 -- u2 ) \ xchar
\G The first xchar at @i{xc-addr} occupies @i{u2} chars; if @i{xc-addr
\G u1} does not contain a complete xchar, @i{u2} is @i{u1}.

umethod x-width ( xc-addr u -- n ) \ xchar-ext
\G @i{n} is the number of monospace ASCII chars that take the same
\G space to display as @var{xc-addr u} needs on a monospaced display.

umethod -trailing-garbage ( xc-addr u1 -- xc-addr u2 ) \ xchar-ext minus-trailing-garbage
\G @i{xc-addr1 u1} is a string of @i{u1} chars.  @i{u2} is the size of
\G the string after removing the chars from the end that do not
\G constitute a complete, valid xchar.@*The idea here is that if you
\G read a fixed number of chars, e.g., with @word{read-file}, there
\G may be an incomplete xchar at the end; you eliminate that with
\G @word{-trainling-garbage}, leaving a valid xchar string for
\G processing (if the string starts with a complete xchar and only
\G contains valid xchars).  You prepend the eliminated chars to the
\G next read block of chars so you do not miss any parts.

umethod xc@+? ( xc-addr1 u1 -- xc-addr2 u2 xc ) \ gforth-experimental x-c-fetch-plus-query
\G @i{xc} is the xchar starting at @var{xc-addr1}.  @var{xc-addr2 u2}
\G is the remaining string behind @var{xc}.  If the start of
\G @i{xc-addr1 u1} contains no valid xchar, @i{xc} is
\G @word{invalid-char}, and @i{xc-addr2 u2} is the remaining string
\G after skipping at least one byte.  If @i{u1}=0, the current
\G behaviour does not make much sense and may change in the future:
\G @i{xc-addr2}=@i{xc-addr1}+1, @i{u2}=MAX-U, and @i{xc} is either 0
\G or @word{invalid-char}.

2drop

\ derived words, faster implementations are probably possible

: x@+/string ( xc-addr1 u1 -- xc-addr2 u2 xc )
    \ !! check for errors?
    over >r +x/string
    r> xc@ ;

: xhold ( xc -- ) \ xchar-ext x-hold
    \G Used between @code{<<#} and @code{#>}. Prepend @var{xc} to the
    \G pictured numeric output string.  We recommend that you use
    \G @word{holds} instead.
    dup xc-size dup +hold swap xc!+? 2drop drop ;

: xc, ( xc -- ) \ xchar x-c-comma
    \G Reserve data space for @i{xc}, and store @i{xc} in that space.
    here unused xc!+? 2drop ->here ;

\ fixed-size versions of these words

' 1- alias char- ( c-addr1 -- c-addr2 ) \ gforth char-minus
\G @code{1 chars -}

: +string ( c-addr1 u1 -- c-addr2 u2 )
    1 /string ;
: string- ( c-addr1 u1 -- c-addr1 u2 )
    1- ;

: c!+ ( c c-addr1 -- c-addr2 )
    dup 1+ >r c! r> ;

: c!+? ( c c-addr1 u1 -- c-addr2 u2 f )
    dup 1 chars u< if \ or use < ?
	rot drop false
    else
	>r dup >r c!
	r> r> 1 /string true
    then ;

$FFFD Constant invalid-char ( -- xc ) \ gforth-experimental
\G Unicode code point returned for cases where the string does not
\G contain a valid Unicode encoding.  Current value: the Unicode
\G replacement character U+FFFD.

: c@+? ( c-addr1 u1 -- c-addr2 u2 c )
    dup 0= IF  1 /string invalid-char  EXIT  THEN
    >r count r> 1- swap ;

: c-size ( c -- 1 )
    drop 1 ;

: ca-size ( addr u -- 1 )
    2drop 1 ;

here
' emit A,
' key A,
' char+ A,
' char- A,
' +string A,
' string- A,
' c@ A,
' c!+ A,
' c!+? A,
' count A,
' c-size A,
' ca-size A,
' nip A,
' noop A,
' c@+? A,
A, here AConstant fixed-width

: set-encoding ( addr -- ) xc-vector ! ;
: set-encoding-fixed-width ( -- )
    fixed-width set-encoding ;

fixed-width xc-vector !

' xkey is edit-key
