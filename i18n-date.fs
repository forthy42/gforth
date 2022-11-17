\ Internationalization and localization of time and date

\ Authors: Bernd Paysan
\ Copyright (C) 2022 Free Software Foundation, Inc.

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

\ This implementation keeps everything in memory, LSIDs are linked
\ together in lists. Each LSID has also a number, which is used to go
\ from native to local LSID.

require date.fs
require i18n.fs

[IFUNDEF] unix-day0
    1970 1 1 ymd2day Constant unix-day0
[THEN]
[IFUNDEF] .##
    : .## ( u -- ) 0 <# # # #> type ;
[THEN]
[IFUNDEF] .####
    : .#### ( u -- ) 0 <# # # # # #> type ;
[THEN]

: localized.day ( day -- )
    unix-day0 + day2ymd
    dup ['] .## $tmp s" DD" replaces
    0 ['] .r $tmp s" day" replaces
    dup ['] .## $tmp s" MM" replaces
    0 ['] .r $tmp s" month" replaces
    dup ['] .## $tmp s" YY" replaces
    dup ['] .#### $tmp s" YYYY" replaces
    dup 0 ['] .r $tmp s" year" replaces
    #1911 - 0 ['] .r $tmp s" twyear" replaces
    l" %YYYY%-%MM%-%DD%T" locale@ .substitute drop ;

: localized.hms ( seconds -- )
    #60 /mod #60 /mod
    dup dup #12 >= IF  #12 - s" pm "  ELSE  s" am "  THEN  s" a/pm" replaces
    dup 0= IF  #12 +  THEN  0 ['] .r $tmp s" 12h" replaces
    ['] .## $tmp s" hh" replaces
    ['] .## $tmp s" mm" replaces
    ['] .## $tmp s" ss" replaces
    l" %hh%:%mm%:%ss%" locale@ .substitute drop ;
