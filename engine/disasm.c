/* disassembler based on libopcodes

  Authors: Bernd Paysan, Anton Ertl
  Copyright (C) 2026 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation, either version 3
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, see http://www.gnu.org/licenses/.
*/

/* This work is based on
   <https://blog.yossarian.net/2019/05/18/Basic-disassembly-with-libopcodes>

   The author mentions other disassembler libraries, but they all
   suffer from supporting fewer architectures than libopcodes.  The
   one with the second-best support for architectures seems to be
   Capstone <https://www.capstone-engine.org/>, but Capstone does not
   support Alpha, HPPA, and IA-64, which are supported by Gforth.
*/

#include <stddef.h>
#include "config.h"

#ifdef HAVE_LIBOPCODES
#include <dis-asm.h>
/* machine.h is where BFD_ARCH and BFD_MACH come from */
#include "machine.h"

/* libopcodes calls a function with the following prototype; this
   implementation just calls vfprintf */
int fprintf_styled(void *file, enum disassembler_style style,
                   const char *format, ...) {
  int n;
  va_list ap;
  va_start(ap,format);
  n = vfprintf((FILE *)file, format, ap);
  va_end(ap);
  return n;
}
#endif

/* return 1 on success and 0 on failure */
int disassemble(unsigned char *addr, size_t u) {
#if defined(HAVE_LIBOPCODES) && defined(BFD_ARCH)
  struct disassemble_info disasm_info;
  disassembler_ftype disasm;
  size_t i=0;

  init_disassemble_info(&disasm_info, stdout, (fprintf_ftype) fprintf,
                        fprintf_styled);
  disasm_info.arch = BFD_ARCH;
  disasm_info.mach = BFD_MACH;
  /* buffer_read_memory() is a convenience function declared in dis-asm.h */
  disasm_info.read_memory_func = buffer_read_memory;
  disasm_info.buffer = addr;
  disasm_info.buffer_vma = (bfd_vma)addr;
  disasm_info.buffer_length = u;
  disassemble_init_for_target(&disasm_info);

  disasm = disassembler(BFD_ARCH,
/* WORDS_BIGENDIAN comes from AC_C_BIGENDIAN */
#ifdef WORDS_BIGENDIAN
                        1,
#else
                        0,
#endif
                        BFD_MACH, NULL);
  if (disasm == NULL)
    return 0;
  putchar('\n');
  while (i<u) {
    int instsize;

    printf("  %p: ",addr+i);
    instsize = disasm((bfd_vma)(addr+i), &disasm_info);
    if (instsize<0) {
      /* in theory this shows code that is not a valid instruction; in
         practice on AMD64, libopcodes disassembles such bytes as
         "(bad)" and returns intsize=1 */
      printf("%02x\n",addr[i]);
      i++;
    } else {
      putchar('\n');
      i += instsize;
    }
  }
  return 1;
#else
  return 0;
#endif
}
    
