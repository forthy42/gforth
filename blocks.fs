\ A less simple implementation of the blocks wordset. 

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

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
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.


\ A more efficient implementation would use mmap on OSs that
\ provide it and many buffers on OSs that do not provide mmap.

\ Now, the replacement algorithm is "direct mapped"; change to LRU
\ if too slow. Using more buffers helps, too.

\ I think I avoid the assumption 1 char = 1 here, but I have not tested this

\ 1024 constant chars/block \ mandated by the standard

require struct.fs

struct
    cell%		field buffer-block   \ the block number
    cell%		field buffer-fid     \ the block's fid
    cell%		field buffer-dirty   \ the block dirty flag
    char% chars/block * field block-buffer   \ the data
    cell% 0 *		field next-buffer
end-struct buffer-struct

Variable block-buffers
Variable last-block

$20 Value buffers

User block-fid

: block-cold ( -- )
    block-fid off  last-block off
    buffer-struct buffers * %alloc dup block-buffers ! ( addr )
    buffer-struct %size buffers * erase ;

' block-cold INIT8 chained

block-cold

Defer flush-blocks

: open-blocks ( addr u -- ) \ gforth
    \g use the file, whose name is given by @var{addr u}, as blocks file 
    2dup open-fpath-file 0<>
    if
	r/w bin create-file throw
    else
	rot close-file throw  2dup file-status throw bin open-file throw
	>r 2drop r>
    then
    block-fid @ IF  flush-blocks block-fid @ close-file throw  THEN
    block-fid ! ;

: use ( "file" -- ) \ gforth
    \g use @var{file} as blocks file
    name open-blocks ;

\ the file is opened as binary file, since it either will contain text
\ without newlines or binary data
: get-block-fid ( -- fid ) \ gforth
    block-fid @ 0=
    if
	s" blocks.fb" open-blocks
    then
    block-fid @ ;

: block-position ( u -- ) \ block
    \G Position the block file to the start of block u.
    1- chars/block chars um* get-block-fid reposition-file throw ;

: update ( -- ) \ block
    last-block @ ?dup IF  buffer-dirty on  THEN ;

: save-buffer ( buffer -- ) \ gforth
    >r
    r@ buffer-dirty @ r@ buffer-block @ 0<> and
    if
	r@ buffer-block @ block-position
	r@ block-buffer chars/block  r@ buffer-fid @  write-file throw
	r@ buffer-dirty off
    endif
    rdrop ;

: empty-buffer ( buffer -- ) \ gforth
    buffer-block off ;

: save-buffers  ( -- ) \ block
    block-buffers @
    buffers 0 ?DO  dup save-buffer  next-buffer  LOOP  drop ;

: empty-buffers ( -- ) \ block
    block-buffers @
    buffers 0 ?DO  dup empty-buffer  next-buffer  LOOP  drop ;

: flush ( -- ) \ block
    save-buffers
    empty-buffers ;

' flush IS flush-blocks

: get-buffer ( n -- a-addr ) \ gforth
    buffers mod buffer-struct %size * block-buffers @ + ;

: block ( u -- a-addr ) \ block- block
  \G u identifies a block number. Assign a block buffer to u,
  \G make it the current block buffer and return its start
  \G address, a-addr.
    dup 0= -35 and throw
    dup get-buffer >r
    dup r@ buffer-block @ <>
    r@ buffer-fid @ block-fid @ <> or
    if
	r@ save-buffer
	dup block-position
	r@ block-buffer chars/block get-block-fid read-file throw
	\ clear the rest of the buffer if the file is too short
	r@ block-buffer over chars + chars/block rot chars - blank
	r@ buffer-block !
	get-block-fid r@ buffer-fid !
    else
	drop
    then
    r> dup last-block ! block-buffer ;

: buffer ( u -- a-addr ) \ block
    \ reading in the block is unnecessary, but simpler
    block ;

User scr ( -- a-addr ) \ block-ext
    \G USER VARIABLE a-addr is the address of a cell containing
    \G the block number of the block most recently processed by
    \G @code{LIST}.
    0 scr !

: updated?  ( n -- f ) \ gforth
    scr @ buffer
    [ 0 buffer-dirty 0 block-buffer - ] Literal + @ ;

: list ( u -- ) \ block
    \ calling block again and again looks inefficient but is necessary
    \ in a multitasking environment
    dup scr !
    ." Screen " u.
    updated?  0= IF ." not "  THEN  ." modified     " cr
    16 0
    ?do
	i 2 .r space scr @ block i 64 * chars + 64 type cr
    loop ;

: (source)  ( -- addr len )
  blk @ ?dup
  IF    block chars/block
  ELSE  tib #tib @
  THEN ;

' (source) IS source

: load ( i*x n -- j*x ) \ block
  push-file
  dup loadline ! blk ! >in off ['] interpret catch
  pop-file throw ;

: thru ( i*x n1 n2 -- j*x ) \ block
  1+ swap ?DO  I load  LOOP ;

: +load ( i*x n -- j*x ) \ block
    blk @ + load ;

: +thru ( i*x n1 n2 -- j*x ) \ block
  1+ swap ?DO  I +load  LOOP ;

: --> ( -- ) \ block- block
    \G If this symbol is encountered whilst loading block @var{n},
    \G discard the remainder of the block and load block @var{n+1}. Used
    \G for chaining multiple blocks together as a single loadable unit.
    refill drop ; immediate

: block-included ( addr u -- ) \ gforth
    block-fid @ >r block-fid off open-blocks
    1 load block-fid @ close-file throw flush
    r> block-fid ! ;

\ thrown out because it may provide unpleasant surprises - anton
\ : include ( "name" -- )
\     name 2dup dup 3 - /string s" .fb" compare
\     0= IF  block-included  ELSE  included  THEN ;

get-current environment-wordlist set-current
true constant block
true constant block-ext
set-current

: bye ( -- ) \ tools-ext
  \G Return control to the host operating system (if any).
  ['] flush catch drop bye ;
