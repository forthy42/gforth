/* This is the machine-specific part for a SPARC

  Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

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

#ifndef THREADING_SCHEME
#define THREADING_SCHEME 5
#endif

#if !defined(USE_TOS) && !defined(USE_NO_TOS)
#define USE_TOS
#endif

#if !defined(INDIRECT_THREADED) && !defined(DIRECT_THREADED)
#define DIRECT_THREADED
#endif

#include "../generic/machine.h"

#define FLUSH_ICACHE(addr,size) \
  ({void *_addr=(addr); void *_end=_addr+((Cell)(size)); \
    for (_addr=((long)_addr)&~7; _addr<_end; _addr += 8) \
       asm("iflush %0+0"::"r"(_addr)); \
   })
/* the +0 in the iflush instruction is needed by gas */

#ifdef DIRECT_THREADED
#ifndef WORDS_BIGENDIAN
#error Direct threading only supported for big-endian SPARCs.
/* little endian SPARCs still store instructions in big-endian format,
   so you would have to reverse the instructions stores in the following
*/
#endif

/* call to dest+4, because dest will reside in delay slot */
#define CALLD(dest,source) (0x40000000|((((unsigned)(dest))+4-(unsigned)(source))>>2))

/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
/* the improvement here is that we may destroy cfa before using PFA1 */
#define PFA1(cfa)	PFA(cfa)
#ifdef undefined
#define PFA1(cfa)	/* PFA(cfa) */ \
			({register Cell *pfa asm("%o7"); \
			  pfa+2; })
#endif
/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa)	({unsigned _cfa = (unsigned)(cfa); \
				    (Label)(_cfa+((*(unsigned *)_cfa)<<2)-4);})
/* MAKE_CF creates an appropriate code field at the cfa; ca is the code address */
/* we use call, since 'branch always' only has 22 bits displacement */
#define MAKE_CF(cfa,ca)	({long *_cfa        = (long *)(cfa); \
			  unsigned _ca = (unsigned)(ca); \
			  _cfa[0] = CALLD(_ca,_cfa); \
			  _cfa[1] = *(long *)_ca; /* delay slot */})

/* this is the point where the does code starts if cfa1 points to a
   code field of a does>-defined word */
/* the +4 is due to the fact, that the does_cf jumps directly to the
   code address, whereas CODE_ADDRESS expects a jump to
   code_address+4, and corrects for that (which is countercorrected by
   the +4) */
#define DOES_CODE(cfa1) \
     ({ Xt _cfa1=(Xt)(cfa1); \
	unsigned _ca; \
	((*(unsigned *)_cfa1)&0xc0000000) == 0x40000000 && \
	(_ca=((unsigned)CODE_ADDRESS(_cfa1))+4 , \
	 ((*(unsigned *)(_ca)) == CALLD(symbols[DODOES],_ca))) \
	? _ca+DOES_HANDLER_SIZE : 0; })

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(label)	((Xt *)(CODE_ADDRESS(label)+4+DOES_HANDLER_SIZE))
#ifdef undefined
#define DOES_CODE1(label)	({register Xt *_does_code asm("%o7"); \
			  	_does_code+2; })
#endif

/* this stores a call dodoes at addr */
#define MAKE_DOES_HANDLER(addr) MAKE_CF(addr,symbols[DODOES])

#define MAKE_DOES_CF(addr,doesp) ({long *_addr        = (long *)(addr); \
			  unsigned _doesp = (unsigned)(doesp); \
			  _addr[0] = 0x40000000|((_doesp-8-(unsigned)_addr)>>2); /* CALL doesp-8 */ \
			  _addr[1] = 0x01000000; /* nop */})
#endif

