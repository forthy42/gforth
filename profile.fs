\ count execution of control-flow edges

\ Copyright (C) 2004 Free Software Foundation, Inc.

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


\ relies on some Gforth internals

\ !! assumption: each file is included only once; otherwise you get
\ the counts for just one of the instances of the file.  This can be
\ fixed by making sure that every source position occurs only once as
\ a profile point.

struct
    cell%    field profile-next
    cell% 2* field profile-count
    cell% 2* field profile-sourcepos
    cell%    field profile-char \ character position in line
end-struct profile% \ profile point

variable profile-points \ linked list of profile%
0 profile-points !

: new-profile-point ( -- addr )
    profile% %alloc >r
    0. r@ profile-count 2!
    current-sourcepos r@ profile-sourcepos 2!
    >in @ r@ profile-char !
    profile-points @ r@ profile-next !
    r@ profile-points !
    r> ;

: dinc ( d-addr -- )
    \ increment double pointed to by d-addr
    dup 2@ 1. d+ rot 2! ;

: profile-this ( -- )
    new-profile-point profile-count POSTPONE literal POSTPONE dinc ;

: profile-:-hook ( -- )
    defers :-hook profile-this ;

: print-profile ( -- )
    profile-points @ begin
	dup while
	    dup >r
	    r@ profile-sourcepos 2@ .sourcepos ." :"
	    r@ profile-char @ 0 .r ." : "
	    r@ profile-count 2@ 0 d.r cr
	    r> profile-next @
    repeat
    drop ;

' profile-:-hook is :-hook
