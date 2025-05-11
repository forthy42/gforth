\ Value with ADDR capability (Variable+Value=Varue)
\
\ Authors: Bernd Paysan
\ Copyright (C) 2022,2023,2024 Free Software Foundation, Inc.

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

obsolete-mask 2/ Constant addressable-mask

: addressable ( -- ) \ gforth-internal
    \G Mark the last word (if named) as addressable
    latest IF  addressable-mask lastflags or!  THEN ;

: addressable: ( -- ) \ gforth-experimental
    \G @code{Addressable:} should be used in front of a defining word
    \G for a value-flavoured word (e.g., @code{value}).  It allows to
    \G use @code{addr} on the word defined by that defining word.
    addressable-mask header-flags or! ;

get-current also locals-types definitions
synonym addressable: addressable:
previous set-current

: .addr-warning ( xt -- xt ) \ gforth-internal
    <<# s"  defined here" holds dup name>string holds s" : " holds
    dup name>view ['] .sourceview $tmp holds #lf hold
    s"  doesn't support ADDR" holds dup name>string holds #0. #>
    hold 1- c(warning") #>> ;

: ?addr ( xt -- xt ) \ gforth-internal
    dup >f+c @ addressable-mask and 0=
    warnings @ abs 1 > and IF  .addr-warning  THEN ;

:noname record-name 4 (') ?addr [ ' (to) :, ] ;
:noname record-name 4 (') ?addr (to), ;
interpret/compile: addr ( "name" -- addr ) \ gforth
\g @i{Addr} is the address where the value of @i{name} is stored.
\g @i{Name} has to be defined with any value-flavoured defining word
\g (e.g. @code{value}) preceded by @code{addressable:}.

4 to-access: >addr ( ... xt -- addr ) \ gforth-internal  to-addr
    \G Obtain the address @var{addr} of the addressible
    \G value-flavoured word @var{xt}.  For some value-flavoured words,
    \G additional inputs may be consumed.

synonym &of addr \ for SwiftForth compatibility

\ obsolete part:

: Varue  ( w "name" -- ) \ gforth-obsolete
    \G Like @code{value}, but you can also use @code{addr @i{name}};
    \G varues may be less efficient than values.
    addressable: Value ;

: 2Varue ( x1 x2 "name" -- ) \ gforth-obsolete
    \G Like @code{2value}, but you can also use @code{addr @i{name}};
    \G 2varues may be less efficient than 2values.
    addressable: 2Value ;

: fvarue ( r "name" -- ) \ gforth-obsolete
    \G Like @code{fvalue}, but you can also use @code{addr @i{name}};
    \G fvarues may be less efficient than fvalues.
    addressable: FValue ;

\ Locals with addrs

get-current also locals-types definitions
: WA: ( compilation "name" -- a-addr xt; run-time x -- ) \ gforth-obsolete w-a-colon
    \G Define varue-flavoured cell local @i{name} @code{( -- x1 )}
    addressable: w: ;
: DA: ( compilation "name" -- a-addr xt; run-time x1 x2 -- ) \ gforth-obsolete w-a-colon
    \G Define varue-flavoured double local @i{name} @code{( -- x3 x4 )}
    addressable: d: ;
: CA: ( compilation "name" -- a-addr xt; run-time c -- ) \ gforth-obsolete c-a-colon
    \G Define varue-flavoured char local @i{name} @code{( -- c1 )}
    addressable: c: ;
: FA: ( compilation "name" -- a-addr xt; run-time f -- ) \ gforth-obsolete f-a-colon
    \G Define varue-flavoured float local @i{name} @code{( -- r1 )}
    addressable: f: ;
: XTA: ( compilation "name" -- a-addr xt; run-time ... -- ... ) \ gforth-obsolete x-t-a-colon
    \G Define a defer-flavoured local @i{name} on which @code{addr}
    \G can be used.
    addressable: xt: ;

previous set-current

also locals-types
: default-wa: ( -- ) \ gforth-obsolete
    \G Allow @code{addr} on locals defined without a type specifyer.
    \G On other words, define locals without a type specifyer using
    \G @code{wa:}.
    ['] wa: is default: ;

: default-w: ( -- ) \ gforth-obsolete
    \G Forbid @code{addr} on locals defined without a type specifyer.
    \G On other words, define locals without a type specifyer using
    \G @code{w:}.
    ['] w:  is default: ;
previous
