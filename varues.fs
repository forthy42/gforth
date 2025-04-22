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

[IFUNDEF] !a-table
    !-table >to+addr-table: !a-table
    defer-table >to+addr-table: defera-table
    2!-table >to+addr-table: 2!a-table
    c!-table >to+addr-table: c!a-table
    f!-table >to+addr-table: f!a-table
[THEN]

' >body !a-table to-class: varue-to ( n value-xt -- ) \ gforth-internal

0 Value dummy-varue
' varue-to set-to

: Varue  ( w "name" -- ) \ gforth
    \G Like @code{value}, but you can also use @code{addr @i{name}};
    \G varues may be less efficient than values.
    ['] dummy-varue create-from reveal , ;

' >body 2!a-table to-class: 2varue-to ( addr -- ) \ gforth-internal

0 0 2value dummy-2varue
    ' 2varue-to set-to

: 2varue ( x1 x2 "name" -- ) \ gforth
    \G Like @code{2value}, but you can also use @code{addr @i{name}};
    \G 2varues may be less efficient than 2values.
    ['] dummy-2varue create-from reveal 2, ;

' >body f!a-table to-class: fvarue-to ( r xt-fvalue -- ) \ gforth-internal

0e fvalue dummy-fvarue
    ' fvarue-to set-to

: fvarue ( r "name" -- ) \ gforth
    \G Like @code{fvalue}, but you can also use @code{addr @i{name}};
    \G fvarues may be less efficient than fvalues.
    ['] dummy-fvarue create-from reveal f, ;

\ Locals with addrs

!a-table locals-to-class: to-wa:
defera-table locals-to-class: to-xta:
2!a-table locals-to-class: to-da:
c!a-table locals-to-class: to-ca:
f!a-table locals-to-class: to-fa:

get-current also locals-types definitions
: WA: ( compilation "name" -- a-addr xt; run-time x -- ) \ gforth w-a-colon
    \G Define varue-flavoured cell local @i{name} @code{( -- x1 )}
    [: ( Compilation: -- ) ( Run-time: -- w )
	\ compiles a local variable access
	@ lp-offset compile-@local ;]
    ['] to-wa: create-local
    \ xt produces the appropriate locals pushing code when executed
    ['] compile-pushlocal-w ;
: DA: ( compilation "name" -- a-addr xt; run-time x1 x2 -- ) \ gforth w-a-colon
    \G Define varue-flavoured double local @i{name} @code{( -- x3 x4 )}
    [: ( Compilation: -- ) ( Run-time: -- x3 x4 )
	@ laddr#, postpone 2@ ;]
    ['] to-da: create-local
    ['] compile-pushlocal-d ;
: CA: ( compilation "name" -- a-addr xt; run-time c -- ) \ gforth c-a-colon
    \G Define varue-flavoured char local @i{name} @code{( -- c1 )}
    [: ( Compilation: -- ) ( Run-time: -- c1 )
	@ laddr#, postpone c@ ;]
    ['] to-ca: create-local
    ['] compile-pushlocal-c ;
: FA: ( compilation "name" -- a-addr xt; run-time f -- ) \ gforth f-a-colon
    \G Define varue-flavoured float local @i{name} @code{( -- r1 )}
    [: ( Compilation: -- ) ( Run-time: -- r1 )
	@ lp-offset compile-f@local ;]
    ['] to-fa: create-local
    ['] compile-pushlocal-f ;
: XTA: ( compilation "name" -- a-addr xt; run-time ... -- ... ) \ gforth x-t-a-colon
    \G Define a defer-flavoured local @i{name} on which @code{addr}
    \G can be used.
    xt: ['] to-xta: set-to ;

ca: some-calocal 2drop
da: some-dalocal 2drop
fa: some-falocal 2drop
wa: some-walocal 2drop
xta: some-xtalocal 2drop

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

2 to: addr ( "name" -- addr ) \ gforth
\g @i{Addr} is the address where the value of @i{name} is stored.
\g @i{Name} is defined with @code{varue}, @code{2varue}, @code{fvarue}
\g or (in a locals definition) with one of @code{wa: ca: da: fa:
\g xta:}.

2 to-access: >addr ( xt-varue -- addr ) \ gforth-internal  to-addr
    \G Obtain the address @var{addr} of the varue @var{xt-varue}

synonym &of addr \ for SwiftForth compatibility
