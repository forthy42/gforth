\ Bitmap for needed things

\ Copyright (C) 2015,2016 Free Software Foundation, Inc.

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

Variable need-root
need-root Value need-mask

: ?need: ( mask -- flag )
    Create dup , 2* DOES> @ need-mask @ and 0<> ;
: +need: ( mask -- )
    Create dup , 2* DOES> @ need-mask @ or need-mask ! ;
: -need: ( mask -- )
    Create dup invert , 2* DOES> @ need-mask @ and need-mask ! ;

$100 Constant ?config#

1
?need: ?sync
?need: ?show
?need: ?keyboard
?need: ?glyphs
drop

1
+need: +sync
+need: +show
+need: +keyboard
+need: +glyphs
drop

1
-need: -sync
-need: -show
-need: -keyboard
-need: -glyphs
drop

: ?config ( -- flag ) need-mask @ 8 rshift 0> ;
: +config ( -- flag ) $1000 need-mask @ -$100 mux need-mask ! ;
: 1+config ( -- flag ) $100 need-mask +! ;
: 1config ( -- flag ) $100 need-mask @ -$100 mux need-mask ! ;
: -config ( -- flag ) ?config IF  -$100 need-mask +!  THEN ;
: 0-config ( -- flag ) need-mask @ $FF and need-mask ! ;
