\ disassembler based on libopcodes

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

\ This work is based on
\ <https://blog.yossarian.net/2019/05/18/Basic-disassembly-with-libopcodes>

\ The author mentions other disassembler libraries, but they all
\ suffer from supporting fewer architectures than libopcodes.  The
\ one with the second-best support for architectures seems to be
\ Capstone <https://www.capstone-engine.org/>, but Capstone does not
\ support Alpha, HPPA, and IA-64, which are supported by Gforth.

c-library opcodes
    libopcodes-name 2 /string add-lib
    \c #include <stddef.h>
    \c #include "config.h"
    \c #include <dis-asm.h>
    \c
    \c typedef void (*stype_ftype) (char * addr, unsigned int u, int style);
    \c int vasprintf_type(stype_ftype stype, const char *format, ...) {
    \c   int len;
    \c   char * strp=0;
    \c   va_list ap;
    \c   va_start(ap,format);
    \c   len = vsnprintf(NULL, 0, format, ap);
    \c   va_end(ap);
    \c   if(len > 0) {
    \c       strp = malloc(len+1);
    \c       va_start(ap,format);
    \c       len = vsnprintf(strp, len+1, format, ap);
    \c       va_end(ap);
    \c       if(len > 0) stype(strp, len, 0);
    \c       free(strp);
    \c   }
    \c   return len;
    \c }
    \c int vasprintf_type_styled(stype_ftype stype, enum disassembler_style style,
    \c                           const char *format, ...) {
    \c   int len;
    \c   char * strp=0;
    \c   va_list ap;
    \c   va_start(ap,format);
    \c   len = vsnprintf(NULL, 0, format, ap);
    \c   va_end(ap);
    \c   if(len > 0) {
    \c       strp = malloc(len+1);
    \c       va_start(ap,format);
    \c       len = vsnprintf(strp, len+1, format, ap);
    \c       va_end(ap);
    \c       if(len > 0) stype(strp, len, style);
    \c       free(strp);
    \c   }
    \c   return len;
    \c }
    \c disassemble_info disasm_info;
    \c 
    \c void init_info(stype_ftype stype) {
    \c   init_disassemble_info(&disasm_info, stype, (fprintf_ftype) vasprintf_type,
    \c                         (fprintf_styled_ftype) vasprintf_type_styled);
    \c   disasm_info.arch = BFD_ARCH;
    \c   disasm_info.mach = BFD_MACH;
    \c   /* buffer_read_memory() is a convenience function declared in dis-asm.h */
    \c   disasm_info.read_memory_func = buffer_read_memory;
    \c }
    \c disassembler_ftype init_region(unsigned char *addr, size_t u) {
    \c   disasm_info.buffer = addr;
    \c   disasm_info.buffer_vma = (bfd_vma)addr;
    \c   disasm_info.buffer_length = u;
    \c   disassemble_init_for_target(&disasm_info);
    \c 
    \c   return disassembler(BFD_ARCH,
    \c /* WORDS_BIGENDIAN comes from AC_C_BIGENDIAN */
    \c #ifdef WORDS_BIGENDIAN
    \c                       1,
    \c #else
    \c                       0,
    \c #endif
    \c                       BFD_MACH, NULL);
    \c }
    \c int disline_opcodes(unsigned char *addr,
    \c                     disassembler_ftype disasm) {
    \c   int instsize;
    \c 
    \c   instsize = disasm((bfd_vma)(addr), &disasm_info);
    \c   if (instsize<0) {
    \c     /* in theory this shows code that is not a valid instruction; in
    \c        practice on AMD64, libopcodes disassembles such bytes as
    \c        "(bad)" and returns intsize=1 */
    \c     printf("%02x\n",*addr);
    \c     return 1;
    \c   } else {
    \c     return instsize;
    \c   }
    \c }
    c-function init_opcodes_info init_info a -- void ( -- )
    c-function init_opcodes_region init_region a u -- a
    c-function disline_opcodes disline_opcodes a a -- u ( addr disassembler-ftype -- addr1 )
    c-callback opcodes_stylish_type: a u n -- void
end-c-library

Create color-table
' default-color  ,
' info-color     ,
' info-color     ,
' warning-color  ,
' success-color  ,
' input-color    ,
' input-color    ,
' input-color    ,
' default-color  ,

here color-table - cell/ 1- >r

: stylish-type ( addr u style -- )
    [ r> ]L min 0 max cells color-table + perform
    type  default-color ;
' stylish-type opcodes_stylish_type: Value op-stype

0 Value disasm()

: disline2 ( addr -- instsize )
    dup 2 cells hex.r ." : "
    disasm() disline_opcodes ;
: disasm2 ( addr u -- ) \ gforth
    disasm() 0= IF  op-stype init_opcodes_info  THEN
    2dup init_opcodes_region to disasm()
    [: bounds u+do  cr i disline2 +loop  cr ;] $10 base-execute ;

:is 'image  0 to disasm() defers 'image ;
:is 'cold   defers 'cold
    [ ' opcodes_stylish_type: c-lib:ccb-num @ 1+ ]L
    ['] opcodes_stylish_type: c-lib:ccb-num !
    ['] stylish-type opcodes_stylish_type: to op-stype ;

' disasm2 is discode
