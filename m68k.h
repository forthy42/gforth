/* This is the machine-specific part for the 68000 and family

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

#include "32bit.h"

#define FLUSH_ICACHE(addr,size)    cache_$clear()
/* Clearing the whole cache is a bit drastic, but this is the only
   cache control available on the apollo.
*/

#ifdef DIRECT_THREADED
#warning untested
/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
#define PFA1(cfa)	PFA(cfa)
/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa)	(*(Label *)(((char *)(cfa))+2))
/* MAKE_CF creates an appropriate code field at the cfa;
   ca is the code address */
#define MAKE_CF(cfa,ca)		({short * _cfa = (short *)cfa; \
				  _cfa[0] = 0x4ef9; /* jmp.l */ \
				  *(long *)(_cfa+1) = (long)(ca);})

/* this is the point where the does code starts if label points to the
 * jump dodoes */
#define DOES_CODE(label)	((Xt *)(((char *)CODE_ADDRESS(label))+DOES_HANDLER_SIZE))

/* this is a special version of DOES_CODE for use in dodoes */
#define DOES_CODE1(label)	DOES_CODE(label)

/* this stores a call dodoes at addr */
#define MAKE_DOES_HANDLER(addr) MAKE_CF(addr,symbols[DODOES])

#define DOES_HANDLER_SIZE       8

#define MAKE_DOES_CF(addr,doesp)   MAKE_CF(addr,((int)(doesp)-8))
#endif

