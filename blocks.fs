\ A less simple implementation of the blocks wordset. 

\ Copyright (C) 1995 Free Software Foundation, Inc.

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
    1           cells: field buffer-block   \ the block number
    1           cells: field buffer-fid     \ the block's fid
    1           cells: field buffer-dirty   \ the block dirty flag
    chars/block chars: field block-buffer   \ the data
    0           cells: field next-buffer
end-struct buffer-struct

Variable block-buffers
Variable last-block

$20 Value buffers

User block-fid

: block-cold
    defers 'cold  block-fid off  last-block off
    buffers buffer-struct drop * allocate throw dup block-buffers !
    buffers buffer-struct drop * erase ;

' block-cold IS 'cold

block-cold

Defer flush-file

: open-blocks ( addr u -- ) \ gforth
    \g use the file, whose name is given by @var{addr u}, as blocks file 
    2dup ['] open-path-file catch 0<>
    if
	2drop r/w bin create-file throw
    else
	rot close-file throw  2dup file-status throw bin open-file throw
	>r 2drop r>
    then
    block-fid @ IF  flush-file block-fid @ close-file throw  THEN
    block-fid ! ;

: use ( "file" -- ) \ gforth
    \g use @var{file} as blocks file
    name open-blocks ;

\ the file is opened as binary file, since it either will contain text
\ without newlines or binary data
: get-block-fid ( -- fid )
    block-fid @ 0=
    if
	s" blocks.fb" open-blocks
    then
    block-fid @ ;

: block-position ( u -- )
    \ positions the block file to the start of block u
    1- chars/block chars um* get-block-fid reposition-file throw ;

: update ( -- )
    last-block @ ?dup IF  buffer-dirty on  THEN ;

: save-buffer ( buffer -- ) >r
    r@ buffer-dirty @ r@ buffer-block @ 0<> and
    if
	r@ buffer-block @ block-position
	r@ block-buffer chars/block  r@ buffer-fid @  write-file throw
	r@ buffer-dirty off
    endif
    rdrop ;

: empty-buffer ( buffer -- )
    buffer-block off ;

: save-buffers  ( -- )    block-buffers @
    buffers 0 ?DO  dup save-buffer  next-buffer  LOOP  drop ;

: empty-buffers ( -- )    block-buffers @
    buffers 0 ?DO  dup empty-buffer  next-buffer  LOOP  drop ;

: flush ( -- )
    save-buffers
    empty-buffers ;

' flush IS flush-file

: get-buffer ( n -- a-addr )
    buffers mod buffer-struct drop * block-buffers @ + ;

: block ( u -- a-addr )
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

: buffer ( u -- a-addr )
    \ reading in the block is unnecessary, but simpler
    block ;

User scr 0 scr !

: updated?  ( n -- f )   scr @ buffer
    [ 0 buffer-dirty 0 block-buffer - ] Literal + @ ;

: list ( u -- )
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

: load ( i*x n -- j*x )
  push-file
  dup loadline ! blk ! >in off ( ['] ) interpret ( catch )
  pop-file ( throw ) ;

: thru ( i*x n1 n2 -- j*x )
  1+ swap 0 ?DO  I load  LOOP ;

: +load ( i*x n -- j*x )  blk @ + load ;

: +thru ( i*x n1 n2 -- j*x )
  1+ swap 0 ?DO  I +load  LOOP ;

: --> ( -- )  refill drop ; immediate

: block-included ( addr u -- )
    block-fid @ >r block-fid off open-blocks
    1 load block-fid @ close-file throw flush
    r> block-fid ! ;

: include ( "name" -- )
    name 2dup dup 3 - /string s" .fb" compare
    0= IF  block-included  ELSE  included  THEN ;

get-current environment-wordlist set-current
true constant block
true constant block-ext
set-current

: bye  ['] flush catch drop bye ;