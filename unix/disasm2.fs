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
    \c #include <stddef.h>
    \c #include "config.h"
    \c #include <dis-asm.h>
    \c 
    \c int vaasprintf(char **str, const char *format, ...) {
    \c   int n, len;
    \c   unsigned int offset;
    \c   va_list ap;
    \c   va_start(ap,format);
    \c   len = vsnprintf(NULL, 0, format, ap);
    \c   va_end(ap);
    \c   offset = *str ? strlen(*str) : 0;
    \c   *str = realloc(*str, offset+len+1);
    \c   va_start(ap,format);
    \c   n = vsnprintf(*str+offset, len+1, format, ap);
    \c   va_end(ap);
    \c   return n;
    \c }
    \c int vaasprintf_styled(char **str, enum disassembler_style style,
    \c                       const char *format, ...) {
    \c   int n, len;
    \c   unsigned int offset;
    \c   va_list ap;
    \c   va_start(ap,format);
    \c   len = vsnprintf(NULL, 0, format, ap);
    \c   va_end(ap);
    \c   offset = *str ? strlen(*str) : 0;
    \c   *str = realloc(*str, offset+len+1);
    \c   va_start(ap,format);
    \c   n = vsnprintf(*str+offset, len+1, format, ap);
    \c   va_end(ap);
    \c   return n;
    \c }
    \c disassemble_info disasm_info;
    \c 
    \c void init_info(char ** str) {
    \c   init_disassemble_info(&disasm_info, str, (fprintf_ftype) vaasprintf,
    \c                         (fprintf_styled_ftype) vaasprintf_styled);
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
end-c-library

0 Value disasm()
Variable opcodes-str

: disline2 ( addr -- instsize )
    dup 2 cells hex.r ." : "
    disasm() disline_opcodes
    0 opcodes-str !@ dup cstring>sstring type  free throw ;
: disasm2 ( addr u -- ) \ gforth
    disasm() 0= IF  opcodes-str init_opcodes_info  THEN
    2dup init_opcodes_region to disasm()
    [: bounds u+do  cr i disline2 +loop  cr ;] $10 base-execute ;

:is 'image  0 to disasm() defers 'image ;

' disasm2 is discode
