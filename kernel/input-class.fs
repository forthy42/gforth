\ Input handling (object oriented)                      22oct00py

\ Copyright (C) 2000,2003,2004,2005,2006,2007,2011,2013,2014,2015,2016 Free Software Foundation, Inc.

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

\ input handling structure:

user-o current-input

0 0
umethod source ( -- addr u ) \ core source
    \G Return address @i{addr} and length @i{u} of the current input
    \G buffer
umethod refill ( -- flag ) \ core-ext,block-ext,file-ext
    \G Attempt to fill the input buffer from the input source.  When
    \G the input source is the user input device, attempt to receive
    \G input into the terminal input device. If successful, make the
    \G result the input buffer, set @code{>IN} to 0 and return true;
    \G otherwise return false. When the input source is a block, add 1
    \G to the value of @code{BLK} to make the next block the input
    \G source and current input buffer, and set @code{>IN} to 0;
    \G return true if the new value of @code{BLK} is a valid block
    \G number, false otherwise. When the input source is a text file,
    \G attempt to read the next line from the file. If successful,
    \G make the result the current input buffer, set @code{>IN} to 0
    \G and return true; otherwise, return false.  A successful result
    \G includes receipt of a line containing 0 characters.
umethod source-id ( -- 0 | -1 | fileid ) \ core-ext,file source-i-d
    \G Return 0 (the input source is the user input device), -1 (the
    \G input source is a string being processed by @code{evaluate}) or
    \G a @i{fileid} (the input source is the file specified by
    \G @i{fileid}).
| umethod (save-input) ( -- x1 .. xn n ) \ gforth
| umethod (restore-input) ( x1 .. xn n -- ) \ gforth

cell uvar >in ( -- addr ) \ core to-in
    \G @code{uvar} variable -- @i{a-addr} is the address of a
    \G cell containing the char offset from the start of the input
    \G buffer to the start of the parse area.
2 cells uvar input-lexeme ( -- a-addr ) \ gforth-internal
    \G @code{uvar} variable -- @i{a-addr} is the address of two
    \G cells containing the string (in c-addr u form) parsed with
    \G @code{parse}, @code{parse-name} or @code{word}.  If you do your
    \G own parsing, you can set it with @code{input-lexeme!}.
cell uvar #tib ( -- addr ) \ core-ext-obsolescent number-t-i-b
    \G @code{uvar} variable -- @i{a-addr} is the address of a
    \G cell containing the number of characters in the terminal input
    \G buffer. OBSOLESCENT: @code{source} superceeds the function of
    \G this word.
cell uvar max#tib ( -- addr ) \ gforth max-number-t-i-b
    \G @code{uvar} variable -- This cell contains the maximum
    \G size of the current tib.
cell uvar old-input ( -- addr ) \ gforth
    \G @code{uvar} variable -- This cell contains the pointer to
    \G the previous input buffer
cell uvar loadline ( -- addr ) \ gforth
    \G @code{uvar} variable -- This cell contains the line that's
    \G currently loaded from
has? file [IF]
cell uvar loadfile ( -- addr ) \ gforth
    \G @code{uvar} variable -- This cell contains the file the
    \G input buffer is associated with (0 if none)
cell uvar blk ( -- addr ) \ block b-l-k
    \G @code{uvar} variable -- This cell contains the current
    \G block number (or 0 if the current input source is not a block).
cell uvar #fill-bytes ( -- addr ) \ gforth
    \G @code{uvar} variable -- number of bytes read via
    \G (read-line) by the last refill
cell uvar loadfilename# ( -- addr ) \ gforth
    \G @code{uvar} variable -- n describes index of currently
    \G interpreted input into loaded filenames
[THEN]
0 uvar tib ( -- addr ) \ core-ext-obsolescent t-i-b

Constant tib+
drop
