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
: addressable ( -- ) \ gforth-experimental
    \G Mark the last word (if named) as addressable
    latest IF  addressable-mask lastflags or!  THEN ;
: addressable: ( -- ) \ gforth-experimental
    addressable-mask value-flags or! ;

get-current also locals-types definitions
synonym addressable: addressable:
previous set-current

: Varue  ( w "name" -- ) \ gforth-obsolete
    \G Like @code{value}, but you can also use @code{addr @i{name}};
    \G varues may be less efficient than values.
    Value addressable ;

: 2Varue ( x1 x2 "name" -- ) \ gforth-obsolete
    \G Like @code{2value}, but you can also use @code{addr @i{name}};
    \G 2varues may be less efficient than 2values.
    2Value addressable ;

: fvarue ( r "name" -- ) \ gforth-obsolete
    \G Like @code{fvalue}, but you can also use @code{addr @i{name}};
    \G fvarues may be less efficient than fvalues.
    FValue addressable ;

\ Locals with addrs

get-current also locals-types definitions
: WA: ( compilation "name" -- a-addr xt; run-time x -- ) \ gforth w-a-colon
    \G Define varue-flavoured cell local @i{name} @code{( -- x1 )}
    addressable: w: ;
: DA: ( compilation "name" -- a-addr xt; run-time x1 x2 -- ) \ gforth w-a-colon
    \G Define varue-flavoured double local @i{name} @code{( -- x3 x4 )}
    addressable: d: ;
: CA: ( compilation "name" -- a-addr xt; run-time c -- ) \ gforth c-a-colon
    \G Define varue-flavoured char local @i{name} @code{( -- c1 )}
    addressable: c: ;
: FA: ( compilation "name" -- a-addr xt; run-time f -- ) \ gforth f-a-colon
    \G Define varue-flavoured float local @i{name} @code{( -- r1 )}
    addressable: f: ;
: XTA: ( compilation "name" -- a-addr xt; run-time ... -- ... ) \ gforth x-t-a-colon
    \G Define a defer-flavoured local @i{name} on which @code{addr}
    \G can be used.
    addressable: xt: ;

previous set-current

also locals-types
: default-wa: ( -- ) \ gforth-experimental
    \G Allow @code{addr} on locals defined without a type specifyer.
    \G On other words, define locals without a type specifyer using
    \G @code{wa:}.
    ['] wa: is default: ;

: default-w: ( -- ) \ gforth-experimental
    \G Forbid @code{addr} on locals defined without a type specifyer.
    \G On other words, define locals without a type specifyer using
    \G @code{w:}.
    ['] w:  is default: ;
previous

: .addr-warning ( xt -- xt ) \ gforth-internal
    <<# s"  defined here" holds dup name>string holds s" : " holds
    dup name>view ['] .sourceview $tmp holds #lf hold
    s"  doesn't support ADDR" holds dup name>string holds #0. #>
    hold 1- c(warning") #>> ;

: ?addr ( xt -- xt ) \ gforth-internal
    dup >f+c @ addressable-mask and 0=
    warnings @ abs 1 > and IF  .addr-warning  THEN ;

:noname record-name 2 (') ?addr [ ' (to) :, ] ;
:noname record-name 2 (') ?addr (to), ;
interpret/compile: addr ( "name" -- addr ) \ gforth
\g @i{Addr} is the address where the value of @i{name} is stored.
\g @i{Name} is defined with @code{varue}, @code{2varue}, @code{fvarue}
\g or (in a locals definition) with one of @code{wa: ca: da: fa:
\g xta:}.

2 to-access: >addr ( xt-varue -- addr ) \ gforth-internal  to-addr
    \G Obtain the address @var{addr} of the varue @var{xt-varue}

synonym &of addr \ for SwiftForth compatibility
