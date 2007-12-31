\ asmbasic.fs basic assebmler definitions

\ Copyright (C) 1998,2000,2003,2007 Free Software Foundation, Inc.

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

[IFUNDEF] assembler Vocabulary assembler [THEN]
require ./target.fs
[IFUNDEF] chained require chains.fs [THEN]

\ ---------- Basic Definitions

\ (code) and (end-code) are used from interpreter or cross-compiler
\ Between (code) and (end-code) must be finished cpu-instructions
\ asm[ ... ]asm should or can be used to interrupt assembling
\ mode while assembling, the words should switch off and on
\ the assembler vocabulary or a special pasring mode
\ When using ]asm asm[ a cpu instruction has not to be finished

defer ]asm		\ turns on assembler mode
defer asm[		\ turns off assebmler mode

defer (code)		\ starts up a assembler passage
defer (end-code)	\ ends an assembler passage

\ Chains

\ Numref registers in propper8 (to reset some flags) and in
\ end-code8 (for resolving)
\ propper should be executed before at assembling start
\ and when an intstruction is finished.

Variable code8		\ starts assembling
0 code8 !

Variable end-code8	\ ends assembling
0 end-code8 !

Variable propper8	\ clean up flags for new cpu instructions
0 propper8 !

: propper propper8 chainperform ;

: ]asm-1
  also assembler
  get-order >r = ABORT" Assembler is activated!"
  r> 2 - 0 ?DO drop LOOP 
  ;			' ]asm-1 IS ]asm

: asm[-1
  also assembler
  get-order >r <> ABORT" Assembler isn't activated!"
  r> 2 - 0 ?DO drop LOOP 
  previous previous ;		' asm[-1 IS asm[

: (code)-1
\ the next input is assebler code witch is stored at dp  
  ]asm
  propper
  code8 chainperform 
  ; 			' (code)-1 IS (code)

: (end-code)-1
\ the next code are normal forth definitions
  end-code8 chainperform
  asm[ ;		' (end-code)-1 IS (end-code)

\ for test purposes

[IFUNDEF] there
: code create (code) ;
: end-code (end-code) ;
[THEN]

also assembler also definitions forth

variable asm-current

: end-label (end-code) asm-current @ set-current ;
: end-macros previous previous asm-current @ set-current ;

\ Macros

: : : ;
: ; postpone ; ; immediate

: label there constant ;
: equ   constant ;

forth definitions

: start-macros  get-current asm-current ! also forth also assembler definitions ;
: label         (code) label ;

previous previous
