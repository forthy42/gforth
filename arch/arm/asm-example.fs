\ Usage examples for ARM assembler

\ Author: David Kühling <dvdkhlng AT gmx DOT de>
\ Created: January 2008

\ Copyright (C) 2000,2007 Free Software Foundation, Inc.

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


\ this shows how the interpreter 'next' works in gforth on ARM.  But don't
\ take that for granted! may depend on your configure settings, compiler
\ version etc.  If in doubt, look at 'gforth_engine' diassembly by running:
\
\ objdump -dt $(which gforth)
\
CODE asm-noop
   FP 4 ]#	IP	LDR,
END-CODE

\ Now we try to access the stack.  This implements 'DUP'.  The forth stack
\ pointer register is 'r9' here, again, look at the disassembly to be sure.
CODE asm-dup
   R9 0 #]	R3	LDR,
   R9 -4 ]#	R3	STR,
   FP 4 ]#	IP	LDR,
END-CODE
