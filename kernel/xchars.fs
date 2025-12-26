\ extended characters (either 8bit or UTF-8, possibly other encodings)
\ and their fixed-size variant

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2005,2006,2007,2008,2011,2012,2015,2016,2019,2021,2022,2023 Free Software Foundation, Inc.

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
\G Adds the size of the xchar stored at @var{xc-addr1} to this address,
\G giving @var{xc-addr2}.
umethod xchar- ( xc-addr1 -- xc-addr2 ) \ xchar-ext x-char-minus
\G Goes backward from @var{xc_addr1} until it finds an xchar so that
\G the size of this xchar added to @var{xc_addr2} gives
\G @var{xc_addr1}.
umethod +x/string ( xc-addr1 u1 -- xc-addr2 u2 ) \ xchar-ext plus-x-slash-string
\G Step forward by one xchar in the buffer defined by address
\G @var{xc-addr1}, size @var{u1} chars. @var{xc-addr2} is the address
\G and u2 the size in chars of the remaining buffer after stepping
\G over the first xchar in the buffer.
umethod x\string- ( xc-addr u1 -- xc-addr u2 ) \ xchar-ext x-backslash-string-minus
\G Step backward by one xchar in the buffer defined by address
\G @var{xc-addr} and size @var{u1} in chars, starting at the end of
\G the buffer. @var{xc-addr} is the address and @var{u2} the size in
\G chars of the remaining buffer after stepping backward over the
\G last xchar in the buffer.
umethod xc@ ( xc-addr -- xc ) \ xchar-ext	xc-fetch
\G Fetchs the xchar @var{xc} at @var{xc-addr1}.
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
\G @var{u2} equal @var{xc-addr1} @var{u1}. XC!+?  is safe for buffer
\G overflows, and therefore preferred over XC!+.
umethod xc@+ ( xc-addr1 -- xc-addr2 xc ) \ xchar	x-c-fetch-plus
\G Fetchs the xchar @var{xc} at @var{xc-addr1}. @var{xc-addr2} points
\G to the first memory location after @var{xc}.
umethod xc-size ( xc -- u ) \ xchar x-c-size
\G Computes the memory size of the xchar @var{xc} in chars.
umethod x-size ( xc-addr u1 -- u2 ) \ xchar
\G Computes the memory size of the first xchar stored at @var{xc-addr}
\G in chars.
umethod x-width ( xc-addr u -- n ) \ xchar-ext
\G @var{n} is the number of monospace ASCII chars that take the same
\G space to display as the the xchar string starting at @var{xc-addr},
\G using @var{u} chars; assuming a monospaced display font,
\G i.e. char width is always an integer multiple of the width of an
\G ASCII char.
umethod -trailing-garbage ( xc-addr u1 -- xc-addr u2 ) \ xchar-ext minus-trailing-garbage
\G Examine the last XCHAR in the buffer @var{xc-addr} @var{u1}---if
\G the encoding is correct and it repesents a full char, @var{u2}
\G equals @var{u1}, otherwise, @var{u2} represents the string without
\G the last (garbled) xchar.
umethod xc@+? ( xc-addr1 u1 -- xc-addr2 u2 xc ) \ gforth-experimental x-c-fetch-plus-query
\G Fetchs the first xchar @var{xc} of the string @var{xc-addr1
\G u1}. @var{xc-addr2 u2} is the remaining string after @var{xc}.
2drop

\ derived words, faster implementations are probably possible

: x@+/string ( xc-addr1 u1 -- xc-addr2 u2 xc )
    \ !! check for errors?
    over >r +x/string
    r> xc@ ;

: xhold ( xc -- ) \ xchar-ext x-hold
    \G Used between @code{<<#} and @code{#>}. Prepend @var{xc} to the
    \G pictured numeric output string.  Alternatively, use @code{holds}.
    dup xc-size dup +hold swap xc!+? 2drop drop ;

: xc, ( xchar -- ) \ xchar x-c-comma
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

$FFFD Constant invalid-char

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
