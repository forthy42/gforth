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

:noname >body ['] lit@ peephole-compile, , ;        ' noop ' value! vtable: Value
:noname >body @ ['] lit peephole-compile, , ;       ' noop ' no-to  vtable: Constant
:noname >body 2@ swap
    ['] lit peephole-compile, ,
    ['] lit peephole-compile, , ;                   ' noop ' no-to  vtable: 2Constant    
:noname >body ['] call peephole-compile, , ;        ' noop ' no-to  vtable: :-dummy
:noname >body ['] lit peephole-compile, , ;         ' noop ' no-to  vtable: Variable
:noname >body @ ['] useraddr peephole-compile, , ;  ' noop ' no-to  vtable: User
:noname >body ['] lit-perform peephole-compile, , ; ' noop ' value! vtable: Defer
:noname >body @ ['] lit+ peephole-compile, , ;      ' noop ' no-to  vtable: Field
:noname >body ['] abi-call peephole-compile, , ;    ' noop ' no-to  vtable: (abi-code)
:noname ['] ;abi-code-exec peephole-compile, , ;    ' noop ' no-to  vtable: (;abi-code)
' peephole-compile,                                 ' noop ' no-to  vtable: prim-dummy
:noname ['] does-exec peephole-compile, , ;         ' noop ' no-to  vtable: does>-dummy
' extra, ' noop ' no-to vtable: extra>-dummy

AVariable vtable-list
