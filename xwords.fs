\ extension words from CfVs on www.forth200x.org

\ Copyright (C) 2006,2007 Free Software Foundation, Inc.

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


\ xword name extension

\ X:deferred
xword DEFER     X:deferred
xword IS        X:deferred
xword DEFER@    X:deferred
xword DEFER!    X:deferred
xword ACTION-OF X:deferred

\ X:extension-query no new words

\ X:parse-name

xword PARSE-NAME X:parse-name

\ X:defined

xword [defined]   X:defined
xword [undefined] X:defined

\ X:required

xword REQUIRED X:required
xword REQUIRE  X:required
xword INCLUDE  X:required

\ X:ekeys

xword EKEY>FKEY    X:ekeys
xword K-LEFT	   X:ekeys
xword K-RIGHT	   X:ekeys
xword K-UP	   X:ekeys
xword K-DOWN	   X:ekeys
xword K-HOME	   X:ekeys
xword K-END	   X:ekeys
xword K-PRIOR	   X:ekeys
xword K-NEXT	   X:ekeys
xword K-INSERT	   X:ekeys
xword K-DELETE	   X:ekeys
xword K-F1	   X:ekeys
xword K-F2	   X:ekeys
xword K-F3	   X:ekeys
xword K-F4	   X:ekeys
xword K-F5	   X:ekeys
xword K-F6	   X:ekeys
xword K-F7	   X:ekeys
xword K-F8	   X:ekeys
xword K-F9	   X:ekeys
xword K-F10	   X:ekeys
xword K-F11	   X:ekeys
xword K-F12	   X:ekeys
xword K-SHIFT-MASK X:ekeys
xword K-CTRL-MASK  X:ekeys
xword K-ALT-MASK   X:ekeys

\ X:fp-stack no new words