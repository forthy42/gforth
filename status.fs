\ display status in a separate X-terminal

\ Copyright (C) 2004,2007 Free Software Foundation, Inc.

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


\ Activate by loading status.fs.  No deactivation, and you have to
\ kill the xterm yourself after exiting gforth.

s" xterm -g 100x4 -Sab0" w/o open-pipe throw constant status-fid

\ This does not work:

\ :noname ( -- )
\     status-fid close-pipe throw drop
\     defers bye ;
\ is bye

: .status1 ( -- )
    #cr emit cr ." base= " base @ dec.
    #cr emit cr .s
    #cr emit cr f.s
    #cr emit cr order ;

: .status2 ( -- )
    outfile-id >r
    status-fid to outfile-id
    ['] .status1 catch
    r> to outfile-id
    throw
    status-fid flush-file throw
    defers .status ;

' .status2 is .status
