\ Complex wordset test suite

\ Author: Bernd Paysan
\ Copyright (C) 2024 Free Software Foundation, Inc.

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

testing complex wordset
decimal

{ 1e+0ei -> 1e 0e }
{ 3e+5ei -> 3e 5e }
{ 2e+-4ei -> 2e -4e }
{ -6.3e+2.13ei -> -6.3e 2.13e }
{ 3e+5ei -6.3e+2.13ei z+ -> -3.3e+7.13ei }
{ 3e+5ei -6.3e+2.13ei z- -> 9.3e+2.87ei }
{ 3e+5ei -6.3e+2.13ei z* -> -29.55e+-25.11ei }
{ 3e+5ei -6.3e+2.13ei z/ -> -0.186538057155261e+-0.856718422498525ei }

{ 0e+0ei 0e+0ei z** -> 1e+0ei }
{ 0e+0ei 1e+0ei z** -> 0e+0ei }
{ 0e+0ei -1e+0ei z** -> infinity 0e }
{ 1e+1ei 0e+0ei z** -> 1e+0ei }
{ 1e+1ei 0e+1ei z** -> 0.428829006294368e+0.154871752464247ei }
\ { 0e+0ei 0e+1ei z** -> NaN NaN }
{ 0e+0ei 1e+1ei z** -> 0e+0ei }
\ { 0e+0ei -1e+1ei z** -> NaN NaN }
{ -1e+0ei 0.5e+0ei z** -> 0.0000000000000000612323399573677e+1ei }
{ -1e+0ei 0e+0ei z** -> 1e+0ei }
{ -1e+1ei 0e+0ei z** -> 1e+0ei }
{ -1e+1ei 1e+0ei z** -> -1e+1ei }
{ -1e+1ei 1e+1ei z** -> -0.121339466446359e+0.0569501178644237ei }
{ 1e-309+0ei -1e+1ei z** -> infinity -infinity }
{ 1e-308+0ei -1e+1ei z** -> 69402536101458000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000e+71995055265525700000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000ei }
