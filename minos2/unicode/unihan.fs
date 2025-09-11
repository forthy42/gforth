\ unihan simplified <-> traditional chinese

\ Authors: Bernd Paysan
\ Copyright (C) 2021,2022,2024 Free Software Foundation, Inc.

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

: rec-unihan ( addr u -- )
    bounds xc@+ { sc } xc@+ { tc }
    tc sc >sc
    sc tc >tc
    U+DO
	I xc@+ to tc
	tc sc >sc
	tc >tc2
    I - +LOOP ;

' rec-unihan Constant unihan-recognizer

s" unihan.db" open-fpath-file throw save-mem 2constant unihan.db
close-file throw
s" japanese.db" open-fpath-file throw save-mem 2constant japanese.db
close-file throw
s" japanese-tc.db" open-fpath-file throw save-mem 2constant japanese-tc.db
close-file throw

[IFUNDEF] recognize-execute
    : recognize-execute ( xt recognizer -- )
	['] rec-forth rot wrap-xt ;
[THEN]

: read-unihan ( -- )
    [: unihan.db   included ;] ['] unihan-recognizer recognize-execute ;
: read-japanese
    [: japanese.db included ;] ['] unihan-recognizer recognize-execute ;
: read-japanese-tc
    [: japanese-tc.db included ;] ['] unihan-recognizer recognize-execute ;
