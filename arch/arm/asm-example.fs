\ Usage examples for ARM assembler

\ Author: David Kühling <dvdkhlng AT gmx DOT de>
\ Created: January 2008

\ Copyright (C) 2000,2007,2008 Free Software Foundation, Inc.

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

' noop >code-address .
here .

\ Branching to the code of 'noop' should be safer since we don't guess
\ about interpreter register.  But using branch instructions we can
\ only branch +/-32MB, which might not work.  Instead we directly load
\ the 32bit branch target into the program-counter, using LDR.  The
\ source of the LDR will be a nearby constant for which we use the [#]
\ addressing mode that generates PC-relative memory address.
' noop >code-address constant 'noop-code
CODE asm-noop2
    ' 'noop-code >body [#]  PC LDR,
END-CODE

\ actually the ARM-assembler already defines NEXT, which does something
\ similar to the above.
CODE asm-noop3
    NEXT,
END-CODE


\ Now we try to access the stack.  This implements 'DROP'.  The forth
\ stack pointer register is 'r9' here, again, look at the disassembly
\ to be sure.  (also the top of stack might be register cached, which
\ was not the case here)
CODE mydrop
   R9 4 #	R9	ADD,
   NEXT,
END-CODE

\ Implement 'DUP'.  It is not safe to clobber R3 here.  Again, check the
\ disassebly...
CODE mydup
   R9 0 #]	R3	LDR,
   R9 -4 #]!	R3	STR,
   NEXT,
END-CODE

\ Implement '+'
CODE my+ ( n1 n2 --  n3 )
   R9 IA!	{ R2 R3 }	LDM,
   R2	R3	R3	ADD,
   R9 -4 #]!	R3	STR,
   NEXT,
end-code
