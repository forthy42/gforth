/* common header file

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

#include "config.h"
#include <limits.h>

typedef void *Label;

/* symbol indexed constants */

#define DOCOL	0
#define DOCON	1
#define DOVAR	2
#define DOUSER	3
#define DODEFER	4
#define DOFIELD	5
#define DODOES	6
#define DOESJUMP	7

#include "machine.h"

/* Forth data types */
/* Cell and UCell must be the same size as a pointer */
typedef CELL_TYPE Cell;
typedef unsigned CELL_TYPE UCell;
#define CELL_BITS	(sizeof(Cell) * CHAR_BIT)
typedef Cell Bool;
#define FLAG(b) (-(b))
#define FILEIO(error)	(FLAG(error) & -37)
#define FILEEXIST(error)	(FLAG(error) & -38)

#define F_TRUE (FLAG(0==0))
#define F_FALSE (FLAG(0!=0))

typedef unsigned char Char;
typedef double Float;
typedef char *Address;

#ifdef BUGGY_LONG_LONG
typedef struct {
  Cell hi;
  UCell lo;
} DCell;

typedef struct {
  UCell hi;
  UCell lo;
} UDCell;

#define FETCH_DCELL(d,lo,hi)	((d)=(typeof(d)){(hi),(lo)})
#define STORE_DCELL(d,low,high)	({ \
				     typeof(d) _d = (d); \
				     (low) = _d.lo; \
				     (high)= _d.hi; \
				 })

#define LONG2UD(l)	({UDCell _ud; _ud.hi=0; _ud.lo=(Cell)(l); _ud;})
#define UD2LONG(ud)	((long)(ud.lo))
#define DZERO		((DCell){0,0})

#else /* ! defined(BUGGY_LONG_LONG) */

/* DCell and UDCell must be twice as large as Cell */
typedef DOUBLE_CELL_TYPE DCell;
typedef unsigned DOUBLE_CELL_TYPE UDCell;

typedef union {
  struct {
#ifdef WORDS_BIGENDIAN
    Cell high;
    UCell low;
#else
    UCell low;
    Cell high;
#endif;
  } cells;
  DCell dcell;
} Double_Store;

#define FETCH_DCELL(d,lo,hi)	({ \
				     Double_Store _d; \
				     _d.cells.low = (lo); \
				     _d.cells.high = (hi); \
				     (d) = _d.dcell; \
				 })

#define STORE_DCELL(d,lo,hi)	({ \
				     Double_Store _d; \
				     _d.dcell = (d); \
				     (lo) = _d.cells.low; \
				     (hi) = _d.cells.high; \
				 })

#define LONG2UD(l)	((UDCell)(l))
#define UD2LONG(ud)	((long)(ud))
#define DZERO		((DCell)0)

#endif /* ! defined(BUGGY_LONG_LONG) */

#ifdef DIRECT_THREADED
typedef Label Xt;
#else
typedef Label *Xt;
#endif

#ifndef DIRECT_THREADED
/* i.e. indirect threaded */
/* the direct threaded version is machine dependent and resides in machine.h */

/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
#define PFA1(cfa)	PFA(cfa)
/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa)	(*(Label *)(cfa))
      /* DOES_CODE is the Forth code does jumps to */
#define DOES_CODE(cfa)           (cfa[1])
#define DOES_CODE1(cfa)          DOES_CODE(cfa)
/* MAKE_CF creates an appropriate code field at the cfa;
   ca is the code address */
#define MAKE_CF(cfa,ca) ((*(Label *)(cfa)) = ((Label)ca))
/* make a code field for a defining-word-defined word */
#define MAKE_DOES_CF(cfa,does_code)	({MAKE_CF(cfa,symbols[DODOES]);	\
					  ((Cell *)cfa)[1] = (Cell)does_code;})
/* the does handler resides between DOES> and the following Forth code */
#define DOES_HANDLER_SIZE	(2*sizeof(Cell))
#define MAKE_DOES_HANDLER(addr)	0 /* do nothing */
#endif /* !defined(DIRECT_THREADED) */

#ifdef DEBUG
#	define	NAME(string)	fprintf(stderr,"%08lx: "string"\n",(Cell)ip);
#else
#	define	NAME(string)
#endif

#define CF(const)	(-const-2)

#define CF_NIL	-1

#ifndef FLUSH_ICACHE
#warning flush-icache probably will not work (see manual)
#	define FLUSH_ICACHE(addr,size)
#endif

#ifdef DIRECT_THREADED
#define CACHE_FLUSH(addr,size) FLUSH_ICACHE(addr,size)
#else
#define CACHE_FLUSH(addr,size)
#endif

Label *engine(Xt *ip, Cell *sp, Cell *rp, Float *fp, Address lp);

/* dblsub routines */
DCell dnegate(DCell d1);
UDCell ummul (UCell a, UCell b);
DCell mmul (Cell a, Cell b);
UDCell umdiv (UDCell u, UCell v);
DCell smdiv (DCell num, Cell denom);
DCell fmdiv (DCell num, Cell denom);

