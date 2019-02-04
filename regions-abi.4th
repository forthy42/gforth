\ trivial implementation of region API

\ Copyright (C) 2018 Free Software Foundation, Inc.

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

\ implementation stuff

begin-structure region-node
    field: rn-next
    maxaligned
    0 +field rn-data
end-structure

uvalue current-region

\ API

\ low-level region API

\ region references are stored in single cells, whose addresses
\ (raddr) are passed

: init-region ( raddr -- )
    \ or you can just initialize addr with 0
    off ;

: region-alloc ( usize raddr -- addr )
    >r region-node + allocate throw ( addr0 r: raddr )
    r@ @ over rn-next !
    dup r> !
    rn-data ;

: free-region ( raddr -- )
    dup @ 0 rot ! begin ( addr )
	dup while
	    dup rn-next @ swap free throw
    repeat
    drop ;

\ higher-level API using context wrappers

: ralloc ( usize -- addr )
    current-region region-alloc ;

: with-region ( ... raddr xt -- ... )
    \ make raddr the current region while executing xt ( ... -- ... )
    current-region >r
    [: swap ->current-region execute ;] catch
    r> ->current-execute throw ;

: do-region ( ... xt -- ... )
    \ create a region, pass it's reference to xt ( ... raddr -- ... ),
    \ and free the region
    0 {: w^ r :} r swap catch r free-region throw ;

: do-with-region ( ... xt -- ... )
    \ create a region, make it the current region, execute xt ( ... --
    \ ... ), and free the region
    ['] with-region do-region ;

\ allocate resize free interactions

0 [if] \ not yet implemented

: rallocate ( u -- c-addr ior )
    \ allocate interface for current region
    ... ;

: rresize ( addr1 u -- addr2 ior )
    \ resize interface for current region; the first allocation must
    \ happen with "0 <size> RESIZE".
    ... ;

: rfree ( addr -- ior )
    \ free interface for current region
    drop 0 ;

\ aliases for heap words

synonym hallocate allocate ( u -- c-addr ior )
synonym hresize resize ( addr1 u -- addr2 ior )
synonym hfree free ( addr -- ior )

[then]
