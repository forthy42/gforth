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


\ this shows how the interpreter 'next' works in gforth on ARM.  But
\ don't take that for granted! May depend on your configure settings,
\ compiler version etc.  If in doubt, look at 'gforth_engine'
\ diassembly by running:
\
\ objdump -dt $(which gforth)
\
CODE asm-noop
   FP 4 ]#	PC	LDR,
END-CODE

\ this should be safer since we don't guess about interpreter register
CODE asm-noop2
   ' noop >code-address B,
END-CODE

\ Now we try to access the stack.  This implements 'DROP'.  The forth
\ stack pointer register is 'r9' here, again, look at the disassembly
\ to be sure.  (also the top of stack might be register cached, which
\ was not the case here)
CODE asm-drop
   R9 4 #	R9	ADD,
   FP 4 ]#	PC	LDR,
END-CODE

\ Implement 'DUP'.  It is not safe to clobber R3 here.  Again, check the
\ disassebly...
CODE asm-dup
   R9 0 #]	R3	LDR,
   R9 -4 #]!	R3	STR,
   FP 4 ]#	PC	LDR,
END-CODE

\ Implement '+'
CODE my+ ( n1 n2 --  n3 )
   R9 IA!	{ R2 R3 }	LDM,
   R2	R3	R3	ADD,
   R9 -4 #]!	R3	STR,
   ' noop >code-address B,
end-code
