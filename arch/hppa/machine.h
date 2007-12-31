/* This is the machine-specific part for a HPPA running HP-UX

  Copyright (C) 1995,1996,1997,1998,1999,2003,2005,2007 Free Software Foundation, Inc.

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

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

#include "../generic/machine.h"

/* cache flush stuff */
extern void cacheflush(void *, size_t, size_t);
#ifdef DEBUG
#  define FLUSH_ICACHE(addr,size) \
({ \
   fprintf(stderr,"Flushing Cache at %08x:%08x\n",(int) addr, size); \
   fflush(stderr); \
   cacheflush((void *)(addr), (size_t)(size), 32); \
   fprintf(stderr,"Cache flushed\n");  })
#else
#  define FLUSH_ICACHE(addr,size) \
     cacheflush((void *)(addr), (size_t)(size), 32)
#endif

/* #undef HAVE_LOG1P */
/* #undef HAVE_RINT */

#ifdef FORCE_REG
#define IPREG asm("%r10")
#define SPREG asm("%r9")
#define RPREG asm("%r8")
#define LPREG asm("%r7")
#define CFAREG asm("%r6")
#define TOSREG asm("%r11")
#endif /* FORCE_REG */
