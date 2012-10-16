\ vtables.fs does the intelligent compile, vtable handling

\ Copyright (C) 2012 Free Software Foundation, Inc.

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

:noname >body ['] lit@ peephole-compile, , ;        ' :dovalue 0 vtable: Value
:noname >body @ ['] lit peephole-compile, , ;       ' :docon   0 vtable: Constant
:noname >body 2@ swap
    ['] lit peephole-compile, ,
    ['] lit peephole-compile, , ;                   ' :dodoes  0 vtable: 2Constant    
:noname >body ['] call peephole-compile, , ;        ' :docol   0 vtable: :-dummy
:noname >body ['] lit peephole-compile, , ;         ' :dovar   0 vtable: Variable
:noname >body @ ['] useraddr peephole-compile, , ;  ' :douser  0 vtable: User
:noname >body ['] lit-perform peephole-compile, , ; ' :dodefer 0 vtable: Defer
:noname >body @ ['] lit+ peephole-compile, , ;      ' :dofield 0 vtable: Field
:noname >body ['] abi-call peephole-compile, , ;    ' :doabicode 0 vtable: (abi-code)
:noname ['] ;abi-code-exec peephole-compile, , ;    ' :do;abicode 0 vtable: (;abi-code)
:noname >body @ peephole-compile, ;                 ' :docol  0 vtable: interpret/compile:
:noname peephole-compile, ;                         0         0 vtable: prim-dummy
:noname ['] does-exec peephole-compile, , ;         ' :dodoes 0 vtable: does>-dummy
