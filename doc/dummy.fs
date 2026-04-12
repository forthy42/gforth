\ dummy definitions for documenting words that may or may not be
\ defined in a specific gforth port or where the definition may come
\ from an architecture-dependent file.

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2026 Free Software Foundation, Inc.

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

: disasm2 ( c-addr u -- ) \ gforth
    \G Disassemble code block starting at @i{c-addr} with @i{u} bytes
    \G length using @file{libopcodes} from GNU binutils.  This word is
    \G only defined if Gforth was built with @file{libopcodes} support.
    abort ;

: intel-syntax ( -- ) \ gforth
    \G Change @word{disasm2} to output Intel syntax.  This word is
    \G only defined on AMD64 and IA-32 machines and only if Gforth was
    \G built with @file{libopcodes} support.
    abort ;

: at&t-syntax ( -- ) \ gforth
    \G Change @word{disasm2} to output AT&T syntax.  This word is
    \G only defined on AMD64 and IA-32 machines and only if Gforth was
    \G built with @file{libopcodes} support.
    abort ;

: disasm ( c-addr u -- ) \ gforth
    \G Disassemble code block starting at @i{c-addr} with @i{u} bytes
    \G length using a hand-written disassembler.  This word is only
    \G defined on architectures for which a hand-written disassembler
    \G exists.
    abort ;
