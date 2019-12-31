\ Bitmap for needed things

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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

1
?need: ?sync      \ sync screen needed
?need: ?show      \ show hidden object needed
?need: ?keyboard  \ show keyboard needed
?need: ?lang      \ change language needed
?need: ?textures  \ reload textures needed
?need: ?resize
?need: ?colors
?need: ?vpsync    \ sync viewport
dup Constant ?config#
dup negate Constant config-mux
0 swap 1 [DO] 1+ [I] [+LOOP] Constant config>>

1
+need: +sync
+need: +show
+need: +keyboard
+need: +lang
+need: +textures
+need: +resize
+need: +colors
+need: +vpsync    \ sync viewport
drop

1
-need: -sync
-need: -show
-need: -keyboard
-need: -lang
-need: -textures
-need: -resize
-need: -colors
-need: -vpsync    \ sync viewport
drop

: ?config ( -- flag ) need-mask @ config>> arshift 0> ;
: +config ( -- flag ) ?config# 4 lshift need-mask @ config-mux mux need-mask ! ;
: 1+config ( -- flag ) ?config# need-mask +! ;
: 1config ( -- flag ) ?config# need-mask @ config-mux mux need-mask ! ;
: -config ( -- flag ) ?config IF  config-mux need-mask +!  THEN ;
: 0-config ( -- flag ) need-mask @ config-mux invert and need-mask ! ;
