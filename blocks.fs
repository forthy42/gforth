\ A simple immplementation of the blocks wordset. 

\ This implementation uses only a single buffer and will therefore be a
\ little slow. An efficient implementation would use mmap on OSs that
\ provide it and many buffers on OSs that do not provide mmap.

\ I think I avoid the assumption 1 char = 1 here, but I have not tested this

\ 1024 constant chars/block \ mandated by the standard

create block-buffer chars/block chars allot

variable buffer-block 0 buffer-block ! \ the block currently in the buffer
variable block-fid 0 block-fid ! \ the file id of the current block file
variable buffer-dirty buffer-dirty off


: get-block-fid ( -- fid )
    block-fid @ 0=
    if
	s" blocks.fb" r/w open-file 0<>
	if
	    s" blocks.fb" r/w create-file .s throw
	then
	block-fid !
    then
    block-fid @ ;

: block-position ( u -- )
    \ positions the block file to the start of block u
    1- chars/block chars um* get-block-fid reposition-file .s throw ;

: update ( -- )
    buffer-dirty on ;

: save-buffers ( -- )
    buffer-dirty @
    if
	buffer-block @ block-position
	block-buffer chars/block get-block-fid write-file throw
	buffer-dirty off
    endif ;

: empty-buffers ( -- )
    0 buffer-block ! ;

: flush ( -- )
    save-buffers
    empty-buffers ;

: block ( u -- a-addr )
    dup 0= -35 and throw
    dup buffer-block @ <>
    if
	save-buffers
	dup block-position
	block-buffer chars/block get-block-fid read-file .s throw
	\ clear the rest of the buffer if the file is too short
	block-buffer over chars + chars/block rot - blank
	buffer-block !    
    else
	drop
    then
    block-buffer ;

: buffer ( u -- a-addr )
    \ reading in the block is unnecessary, but simpler
    block ;

User scr 0 scr !

: list ( u -- )
    \ calling block again and again looks inefficient but is necessary
    \ in a multitasking environment
    dup scr !
    ." Screen " u. cr
    16 0
    ?do
	scr @ block i 64 * chars + 64 type cr
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
