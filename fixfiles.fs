\ fix loaded files

\ Copyright (C) 2016 Free Software Foundation, Inc.

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

s" GFORTHDESTDIR" getenv ." Fix prefix: '" 2dup type ." '" cr
included-files $[]# 0 [?DO]  [I] included-files $[]@ over [IF] open-fpath-file 0= [IF] rot close-file throw 2over 2over 2swap string-prefix? [IF] 2 pick /string compact-filename [THEN] [I] included-files $[] off [I] included-files $[]! [THEN] [ELSE] 2drop [THEN] [LOOP] 2drop
