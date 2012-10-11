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

:noname >body ['] lit@ peephole-compile, , ;        0 vtable: Value
:noname >body @ ['] lit peephole-compile, , ;       0 vtable: Constant
:noname >body 2@ swap
    ['] lit peephole-compile, ,
    ['] lit peephole-compile, , ;                   0 vtable: 2Constant    
\ :noname >body ['] call peephole-compile, , ;        0 vtable: :
:noname >body ['] lit peephole-compile, , ;         0 vtable: Variable
:noname >body @ ['] useraddr peephole-compile, , ;  0 vtable: User
:noname >body ['] lit-perform peephole-compile, , ; 0 vtable: Defer
:noname >body @ ['] lit+ peephole-compile, , ;      0 vtable: Field
:noname >body ['] abi-call peephole-compile, , ;    0 vtable: (abi-code)
:noname ['] ;abi-code-exec peephole-compile, , ;    0 vtable: (;abi-code)
:noname ['] does-exec peephole-compile, , ;         0 vtable: input-var
:noname ['] does-exec peephole-compile, , ;         0 vtable: input-method
:noname >body @ peephole-compile, ;                 0 vtable: interpret/compile: