\ Etags support for GNU Forth.

\ Copyright (C) 1995,1998,2001,2003,2007 Free Software Foundation, Inc.

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


\ This does not work like etags; instead, the TAGS file is updated
\ during the normal Forth interpretation/compilation process.

\ The present version has several shortcomings: It always overwrites
\ the TAGS file instead of just the parts corresponding to the loaded
\ files, but you can have several tag tables in emacs. Every load
\ creates a new etags file and the user has to confirm that she wants
\ to use it.

\ Communication of interactive programs like emacs and Forth over
\ files is clumsy. There should be better cooperation between them
\ (e.g. via shared memory)

\ This is ANS Forth with the following serious environmental
\ dependences: the variable LAST must contain a pointer to the last
\ header, NAME>STRING must convert that pointer to a string, and
\ HEADER must be a deferred word that is called to create the name.

\ Changes by David: Removed the blanks before and after the explicit
\ tag name, since that conflicts with Emacs' auto-completition. In
\ fact those blanks are not necessary, since search is performed on
\ the tag-text, rather than the tag name.

require search.fs
require environ.fs
require extend.fs

: tags-file-name ( -- c-addr u )
    \ for now I use just TAGS; this may become more flexible in the
    \ future
    s" TAGS" ;

variable tags-file 0 tags-file !

create tags-line 128 chars allot
    
: skip-tags ( file-id -- )
    \ reads in file until it finds the end or the loadfilename
    drop ;

: tags-file-id ( -- file-id )
    tags-file @ 0= if
	tags-file-name w/o create-file throw
\ 	2dup file-status
\ 	if \ the file does not exist
\ 	    drop w/o create-file throw
\ 	else
\ 	    drop r/w open-file throw
\ 	    dup skip-tags
\ 	endif
	tags-file !
    endif
    tags-file @ ;

2variable last-loadfilename 0 0 last-loadfilename 2!

: put-load-file-name ( file-id -- )
    >r
    sourcefilename last-loadfilename 2@ d<>
    if
	#ff r@ emit-file throw
	#lf r@ emit-file throw
	sourcefilename 2dup
	r@ write-file throw
	last-loadfilename 2!
	s" ,0" r@ write-line throw
    endif
    rdrop ;

: put-tags-entry ( -- )
    \ write the entry for the last name to the TAGS file
    \ if the input is from a file and it is not a local name
    source-id dup 0<> swap -1 <> and	\ input from a file
    current @ locals-list <> and	\ not a local name
    latest 0<> and	\ not an anonymous (i.e. noname) header
    if
	tags-file-id >r 
	r@ put-load-file-name
	source drop >in @ r@ write-file throw
	127 r@ emit-file throw
\	bl r@ emit-file throw
	latest name>string r@ write-file throw
\	bl r@ emit-file throw
	1 r@ emit-file throw
	base @ decimal sourceline# 0 <# #s #> r@ write-file throw base !
	s" ,0" r@ write-line throw
	\ the character position in the file; not strictly necessary AFAIK
	\ instead of using 0, we could use file-position and subtract
	\ the line length
	rdrop
    endif ;

: (tags-header) ( -- )
    defers header
    put-tags-entry ;

' (tags-header) IS header
