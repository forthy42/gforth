\ define words corresponding to the host OS name

\ Authors: Bernd Paysan
\ Copyright (C) 2023 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

e? os-type s" darwin" string-prefix? [IF] [IFUNDEF] darwin : darwin ; [THEN] [THEN]
e? os-type s" bsd" search nip nip [IF] [IFUNDEF] darwin : darwin ; [THEN]
    [IFUNDEF] bsd : bsd ; [THEN] [THEN]
e? os-type s" linux-android" string-prefix? [IF] [IFUNDEF] android : android ; [THEN] [THEN]
e? os-type s" cygwin" string-prefix? [IF] [IFUNDEF] cygwin : cygwin ; [THEN] [THEN]
e? os-type s" linux-gnu" string-prefix?
e? os-type s" linux-musl" string-prefix? or [IF]
    [IFUNDEF] linux : linux ; [THEN]
    s" /proc/partitions" file-status nip 0< [IF] : mslinux ; [THEN]
[THEN]
