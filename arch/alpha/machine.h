/* DEC Alpha

  Copyright (C) 1995,1996,1997,1998,2000 Free Software Foundation, Inc.

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

/* Be careful: long long on Alpha are 64 bit :-(( */

#ifndef THREADING_SCHEME
#define THREADING_SCHEME 5
#endif

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

#ifndef INDIRECT_THREADED
#ifndef DIRECT_THREADED
#define DIRECT_THREADED
#endif
#endif

#define FLUSH_ICACHE(addr,size)		asm("call_pal 0x86") /* imb (instruction-memory barrier) */

#include "../generic/machine.h"

#ifdef DIRECT_THREADED
#ifdef WORDS_BIGENDIAN
#error Direct threading only supported for little-endian Alphas.
/* big-endian Alphas still store instructions in little-endian format,
   so you would have to reverse the instruction accesses in the following
*/
#endif
#if SIZEOF_CHAR_P != 8
#error Direct threading only supported for Alphas with 64-bit Cells.
/* some of the stuff below assumes that the first cell in a code field
   can contain 2 instructions

   A simple way around this problem would be to have _alpha_docol
   contain &&dodoes. This would slow down colon defs, however.

   Another way is to use a special DOES_HANDLER, like most other CPUs */
#endif

#warning Direct threading for Alpha may not work with all gcc versions
#warning ;CODE does not work on the Alpha with direct threading
/* ;CODE puts a jump to the code after ;CODE into the defined
   word. The code generated for the jump can only jump to targets near
   docol (near means: within 32KB). Because the code is far from
   docol, this does not work.

   Solution: let the code be: x=cfa[1]; goto *x;
   */

typedef int Int32;
typedef short Int16;

/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
/* the improvement here is that we may destroy cfa before using PFA1 */
#define PFA1(cfa)       PFA(cfa)

/*
   On the Alpha, code (in the text segment) typically cannot be
   reached from the dictionary (in the data segment) with a normal
   branch. It also usually takes too long (and too much space on
   32-bit systems) to load the address as literal and jump indirectly.
   
   So, what we do is this: a pointer into our code (at docol, to be
   exact) is kept in a register: _alpha_docol. When the inner
   interpreter jumps to the word address of a variable etc., the
   destination address is computed from that with a lda instruction
   and stored in another register: _alpha_ca. Then an indirect jump
   through _alpha_ca is performed. For docol, we need not compute
   _alpha_ca first.

   How do we tell gcc all this? We declare the registers as variables:
   _alpha_docol as explicit variable, to avoid spilling; _alpha_ca is
   so short-lived, so it hopefully won't be spilled. A
   pseudo-primitive cpu_dep is created with code that lets gcc's data
   flow analysis know that _alpha_docol is used and that _alpha_ca may
   be defined and used after any NEXT and before any primitive.  We
   let gcc choose the register for _alpha_ca and simply change the
   code gcc produces for the cpu_dep routine.
*/

/* if you change this, also change _DOCOL_LABEL below */
#define DO_BASE		(&&docol)

#define CPU_DEP2	register Label _alpha_docol asm("$9")=DO_BASE; \
			register Label _alpha_ca;

#define CPU_DEP3	cpu_dep: asm("lda %0, 500(%1)":"=r"(_alpha_ca):"r"(_alpha_docol)); goto *_alpha_ca;

#define CPU_DEP1	(&&cpu_dep)


/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(wa) ({ \
	Int32 *_wa=(Int32 *)(wa); \
	(_wa[0]&0xfc000000)==0x68000000 ? /*JMP?*/\
	 DO_BASE : \
	 ((((_wa[0]^((Int32 *)_CPU_DEP_LABEL)[0]) & 0xffff0000)==0 && \
	   ((_wa[1]^((Int32 *)_CPU_DEP_LABEL)[1]) & 0xffffc000)==0 ) ? \
	  (DO_BASE+((Int16 *)_wa)[0]) : \
	  (Label)_wa); })

#define _CPU_DEP_LABEL	(symbols[DOESJUMP])
#define _DOCOL_LABEL	(symbols[DOCOL])

/* MAKE_CF creates an appropriate code field at the wa; ca is the code
   address. For the Alpha, this is a lda followed by a jmp (or just a
   jmp, if ca==DO_BASE).  We patch the jmp with a good hint (on the
   21064A this saves 5 cycles!) */
#define MAKE_CF(wa,ca)	({ \
	Int32 *_wa=(Int32 *)(wa); \
	Label _ca=(Label)(ca); \
	if (_ca==_DOCOL_LABEL)  \
	    _wa[0]=(((0x1a<<26)|(31<<21)|(9<<16))| \
	            (((((Cell)_ca)-((Cell)_wa)-4) & 0xffff)>>2)); \
	else { \
	    _wa[0]=((((Int32 *)_CPU_DEP_LABEL)[0] & 0xffff0000)| \
		    ((((Cell)_ca)-((Cell)_DOCOL_LABEL)) & 0xffff)); \
	    _wa[1]=((((Int32 *)_CPU_DEP_LABEL)[1] & 0xffffc000)| \
		    (((((Cell)_ca)-((Cell)_wa)-8) & 0xffff)>>2));  \
	} \
    })

/* this is the point where the does code for the word with the xt cfa
   starts. Because the jump to the code field takes only one cell on
   64-bit systems we can use the second cell of the cfa for storing
   the does address */
#define DOES_CODE(cfa) \
     ({ Int32 *_wa=(cfa); \
	(_wa[0] == ((((Int32 *)_CPU_DEP_LABEL)[0] & 0xffff0000)| \
		    ((((Cell)&&dodoes)-((Cell)DO_BASE)) & 0xffff)) && \
	 (_wa[1]&0xffffc000) == (((Int32 *)_CPU_DEP_LABEL)[1] & 0xffffc000)) \
	? DOES_CODE1(_wa) : 0; })

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(cfa)	((Xt *)(((Cell *)(cfa))[1]))

/* the does handler resides between DOES> and the following Forth
   code. Since the code-field jumps directly to dodoes, the
   does-handler is not needed for the Alpha architecture */
#define MAKE_DOES_HANDLER(addr)   ((void)0)

/* This makes a code field for a does-defined word. doesp is the
   address of the does-code. On the Alpha, the code field consists of
   a jump to dodoes and the address of the does code */
#define MAKE_DOES_CF(cfa,doesp) ({Xt *_cfa = (Xt *)(cfa); \
				    MAKE_CF(_cfa, symbols[DODOES]); \
				    _cfa[1] = (doesp); })
#endif

#ifdef FORCE_REG
/* $9-$14 are callee-saved, $1-$8 and $22-$25 are caller-saved */
#define IPREG asm("$10")
#define SPREG asm("$11")
#define RPREG asm("$12")
#define LPREG asm("$13")
#define TOSREG asm("$14")
/* #define CFAREG asm("$22") egcs-1.0.3 crashes with any caller-saved
   register decl */
#endif /* FORCE_REG */
