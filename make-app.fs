\ change --image-file option in image to --appl-image

\ Copyright (C) 1999,2000,2003,2007 Free Software Foundation, Inc.

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


\ upon entry we get an ( addr u ) of the name of the image file

r/w bin open-file throw constant image-file
200 constant buf-size 
create buf buf-size allot
buf buf-size image-file read-file throw constant this-size
buf this-size s" --image-file" search 0= -12 and throw
drop buf - 0 ( u->ud ) image-file reposition-file throw
s" --appl-image" image-file write-file throw
image-file close-file throw
bye




