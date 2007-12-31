\ test signal catching

\ Copyright (C) 2002,2003,2007 Free Software Foundation, Inc.

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


\ the following sequence crashes gforth-0.5.0 on Linux-PPC
\ but testing it can cause a bug on Solaris-i386, because apparently
\ some code from the stack is executed that corrupts something, so
\ interpreting the next line crashes
\ : down 1- ;
\ ' down 1- ' execute catch 2drop

\ test if SIGSEGV handler really works (in particular sigaltstack)
0 ' @ catch 2drop
