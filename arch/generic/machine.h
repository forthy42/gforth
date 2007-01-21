/*
  This is a generic file for 32-bit machines with IEEE FP arithmetic (no VMS).
  It only supports indirect threading.

  Copyright (C) 1995,1998,1999,2003 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.
*/

/* define SYSCALL */

#ifndef SYSCALL
#define SYSCALL
#endif

#ifndef SYSSIGNALS
#define SYSSIGNALS
#endif

#ifndef USE_FTOS
#ifndef USE_NO_FTOS
/* keep top of FP stack in register. Since most processors have FP
   registers and they are hardly used in gforth, this is usually a
   good idea.  The 88100 has no separate FP regs, but many general
   purpose regs, so it should be ok */
#define USE_FTOS
#endif
#endif

#ifndef USE_TOS
#ifndef USE_NO_TOS
/* keep top of data stack in register.  Usually a good idea unless registers are very scarce */
#define USE_TOS
#endif
#endif

#ifndef INDIRECT_THREADED
#ifndef DIRECT_THREADED
#define DIRECT_THREADED
#endif
#endif

/* Types: these types are used as Forth's internal types */

/* define this if IEEE singles and doubles are available as C data types */
#define IEEE_FP

/* the IEEE types are used only for loading and storing */
/* the IEEE double precision type */
typedef double DFloat;
/* the IEEE single precision type */
typedef float SFloat;

typedef CELL_TYPE Cell;
typedef unsigned CELL_TYPE UCell;
typedef Cell Bool;
typedef unsigned char Char;
typedef double Float;
typedef Char *Address;

#if defined(DOUBLY_INDIRECT)
typedef void **Label;
#else /* !defined(DOUBLY_INDIRECT) */
typedef void *Label;
#endif /* !defined(DOUBLY_INDIRECT) */

/* feature defines, these setting should be identical to the ones in machpc.fs */

#ifndef STANDALONE
#warning hosted system
#define HAS_FILE
#define HAS_FLOATING
#define HAS_OS
#define HAS_DEBUG
#ifndef HAS_PEEPHOLE
#define HAS_PEEPHOLE
#endif
#else
#warning standalone system
#undef HAS_FILE
#undef HAS_FLOATING
#undef HAS_OS
#undef HAS_DEBUG
#endif
#define HAS_DCOMPS
#define HAS_GLOCALS
#define HAS_HASH
#define HAS_XCONDS
#define HAS_STANDARDTHREADING

#define RELINFOBITS	8
