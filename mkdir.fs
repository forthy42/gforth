\ mkdir wrapper

\ Copyright (C) 2008 Free Software Foundation, Inc.

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

require cstr.fs
c-library mkdir
\c #include <sys/stat.h>
\c #include <sys/types.h>
c-function mkdir mkdir a n -- n ( pathname\0 mode -- f )
\c #include <errno.h>
\c #define IOR(flag)	((flag)? -512-errno : 0)
c-function f>ior IOR n -- n ( f -- ior )

: =mkdir ( c-addr u mode -- ior )
    >r 1 tilde_cstr r> mkdir f>ior ;
end-c-library
