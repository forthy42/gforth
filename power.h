/* This is the machine-specific part for the Power (incl. PPC) architecture

  Copyright (C) 1995 Free Software Foundation, Inc.

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
  Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

#ifndef INDIRECT_THREADED
#ifndef DIRECT_THREADED
/* #define DIRECT_THREADED */
#endif
#endif

#include "32bit.h"

/* cache flush stuff */
#warning If you get assembly errors, here is the reason why
#define FLUSH_ICACHE(addr,size)   asm("icbi (%0); isync"::"b"(addr))
/* this assumes size=4 */
/* the mnemonics are for the PPC and the syntax is a wild guess; for
   Power the mnemonic for the isync instruction is "ics" and I have
   not found an equivalent for the icbi instruction in my reference.
*/

#ifdef DIRECT_THREADED
#warning Direct threading for Power has not been tested

/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
/* the improvement here is that we may destroy cfa before using PFA1 */
#define PFA1(cfa)       PFA(cfa)

/* I'll assume the code resides in the lower (or upper) 32M of the
   address space and use absolute addressing in the jumps to the
   handlers. This makes it possible to use the full address space for
   direct threaded Forth (even on 64-bit PowerPCs). However, the
   linker has to ensure that this really happens */

#define JUMP_TARGET_BITS 0
/* assuming the code is in the lower 32M; if it is in the upper 32M,
   define JUMP_TARGET_BITS as ~0x3ffffff */
#define JUMP_MASK	0x3fffffc

/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa)	((Label)(((*(unsigned *)(cfa))&JUMP_MASK)|JUMP_TARGET_BITS))

/* MAKE_CF creates an appropriate code field at the cfa; ca is the
   code address. For those familiar with assembly, this is a `ba'
   instruction in both Power and PowerPC assembly languages */
#define MAKE_CF(cfa,ca)	(*(long *)(cfa) = 0x48000002|(ca))

/* this is the point where the does code for the word with the xt cfa
   starts. Since a branch is only a cell on Power, we can use the
   second cell of the cfa for storing the does address */
#define DOES_CODE(cfa) \
     ({ unsigned *_cfa=(unsigned *)(cfa); \
	_cfa[0]==(0x48000002|&&docol) ? DOES_CODE1(_cfa) : 0; })
   

	DOES_CODE(label)
/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(cfa)	((Xt *)(((long *)(cfa))[1]))

/* the does handler resides between DOES> and the following Forth
   code. Since the code-field jumps directly to dodoes, the
   does-handler is not needed for the Power architecture */
#define MAKE_DOES_HANDLER(addr)   0

/* This makes a code field for a does-defined word. doesp is the
   address of the does-code. On the PPC, the code field consists of a
   jump to dodoes and the address of the does code */
#define MAKE_DOES_CF(cfa,doesp) ({Xt *_cfa = (Xt *)(cfa); \
				    MAKE_CF(_cfa, symbols[DODOES]); \
				    _cfa[1] = (doesp); })
#endif
