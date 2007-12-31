/*
  This is the machine-specific part for Intel 386 compatible processors

  Copyright (C) 1995,2000,2003,2005,2007 Free Software Foundation, Inc.

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

/* define SYSCALL */

#ifndef SYSCALL
#define SYSCALL
#endif

#ifdef SYSSIGNALS
#undef SYSSIGNALS
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
/* I don't do the same for the data stack (i.e. USE_TOS), since this
   loses on processors with few registers. USE_TOS might be defined in
   the processor-specific files */
#define USE_TOS

#ifdef DIRECT_THREADED
/* If you want direct threading, write a .h file for your processor! */
/* We could put some stuff here that causes a compile error, but then
   we could not use this file in the other machine.h files */
#undef DIRECT_THREADED
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
typedef void * pm *Label;
#else /* !defined(DOUBLY_INDIRECT) */
typedef void pm *Label;
#endif /* !defined(DOUBLY_INDIRECT) */

/* The SHARC has separate program and data memory, so no instruction
   write is possible from Forth */
#define FLUSH_ICACHE(addr,size)

/* #define ALIGNMENT_CHECK 1 */

#undef HAVE_LIBDL
#undef HAVE_DLOPEN

#undef HAS_DCOMPS
#undef HAS_FILE
#undef HAS_FLOATING
#undef HAS_GLOCALS
#undef HAS_HASH
#undef HAS_OS
#undef HAS_XCONDS

#define RELINFOBITS	32
#define BUGGY_LONG_LONG

#define SHARC

#define PUTC(x)  fwrite8(&x, 1, 1, stdout)
#define TYPE(x, l) fwrite8(x, 1, l, stdout)
