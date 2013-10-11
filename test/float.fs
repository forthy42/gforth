\ float wordset test suite

\ Copyright (C) 2002,2006,2007 Free Software Foundation, Inc.

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

decimal

\ d>f d>d

{  4503599627370497. 2dup d>f f>d d= -> true }
{ -4503599627370497. 2dup d>f f>d d= -> true }
{  9007199254740991. 2dup d>f f>d d= -> true }
{ -9007199254740991. 2dup d>f f>d d= -> true }

\ fround
{ 0.5e fround 0e f= -> true }
{ 1.5e fround 2e f= -> true }
{ 2.5e fround 2e f= -> true }
{ -0.5e fround -0e f= -> true }
{ -1.5e fround -2e f= -> true }
{ -2.5e fround -2e f= -> true }
{ 4503599627370495. d>f 0.5e f+ fround 4503599627370496. d>f f= -> true }
{ 4503599627370494. d>f 0.5e f+ fround 4503599627370494. d>f f= -> true }
{ 4503599627370497. d>f fdup fround f= -> true }
{ 9007199254740991. d>f fdup fround f= -> true }
{ -4503599627370495. d>f -0.5e f+ fround -4503599627370496. d>f f= -> true }
{ -4503599627370495. d>f  0.5e f+ fround -4503599627370494. d>f f= -> true }
{ -4503599627370497. d>f fdup fround f= -> true }
{ -9007199254740991. d>f fdup fround f= -> true }
{ 1.49999e fround 1e f= -> true }

\ >float (very incomplete, just a regression test)
{ s" -" >float -> false }
{ s" +" >float -> false }
{ s"   " >float 0e f= -> true true }
{ s" 2e+3e" >float -> false }
{ s" 2+3" >float -> 2000e true }

set-near
\ transcendenal and other functions, mainly test effect on TOS (not FTOS)
{ 12345 2e 3e f** -> 12345 8e }
{ 12345 1.8e floor -> 12345 1e }
{ 12345 1.8e fround -> 12345 2e }
{ 12345 -1.8e fabs -> 12345 1.8e }
{ 12345 1e facos -> 12345 0e }
{ 12345 1e fasin -> 12345 pi 2e f/ }
{ 12345 0e fatan -> 12345 0e }
{ 12345 1e 0e fatan2 -> 12345 pi 2e f/ }
{ 12345 pi fcos -> 12345 -1e }
{ 12345 0e fexp -> 12345 1e }
{ 12345 0e fexpm1 -> 12345 0e }
{ 12345 1e fln -> 12345 0e }
{ 12345 0e flnp1 -> 12345 0e }
{ 12345 1e flog -> 12345 0e }
{ 12345 0e falog -> 12345 1e }
{ 12345 pi f2/ fsin -> 12345 1e }
{ 12345 0e fsincos -> 12345 0e 1e }
{ 12345 4e fsqrt -> 12345 2e }
{ 12345 pi 4e f/ ftan -> 12345 1e }
{ 12345 0e fsinh -> 12345 0e }
{ 12345 0e fcosh -> 12345 1e }
{ 12345 0e ftanh -> 12345 0e }
{ 12345 0e fasinh -> 12345 0e }
{ 12345 1e facosh -> 12345 0e }
{ 12345 0e fatanh -> 12345 0e }

