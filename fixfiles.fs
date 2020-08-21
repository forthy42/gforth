\ fix loaded files

\ Author: Bernd Paysan
\ Copyright (C) 2016,2019 Free Software Foundation, Inc.

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

s" GFORTHDESTDIR" getenv ." Fix prefix: '" type ." '" cr
included-files $[]# 0 [?DO]  [I] included-files $[]@ 2dup s" /kernel" string-prefix? negate /string 2dup s" /compat" string-prefix? negate /string ." Fix file: " 2dup type over [IF] open-fpath-file 0= [IF] rot close-file throw 2dup s" GFORTHDESTDIR" getenv string-prefix? [IF] s" GFORTHDESTDIR" getenv nip /string compact-filename [THEN] ."  with " 2dup type cr [I] included-files $[] off [I] included-files $[]! [THEN] [ELSE] 2drop [THEN] [LOOP]
