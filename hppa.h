/* This is the machine-specific part for a HPPA running HP-UX

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
#define DIRECT_THREADED
#endif
#endif

#define LONG_LATENCY

/* cache flush stuff */
extern void cacheflush(void *, int, int);
#ifdef DEBUG
#  define FLUSH_ICACHE(addr,size) \
({ \
   fprintf(stderr,"Flushing Cache at %08x:%08x\n",(int) addr, size); \
   fflush(stderr); \
   cacheflush((void *)(addr), (int)(size), 32); \
   fprintf(stderr,"Cache flushed\n");  })
#else
#  define FLUSH_ICACHE(addr,size) \
     cacheflush((void *)(addr), (int)(size), 32)
#endif

#include "32bit.h"

#ifdef DIRECT_THREADED
   /* PFA gives the parameter field address corresponding to a cfa */
#  define PFA(cfa)	(((Cell *)cfa)+2)
   /* PFA1 is a special version for use just after a NEXT1 */
   /* the improvement here is that we may destroy cfa before using PFA1 */
#  define PFA1(cfa)       PFA(cfa)
   /* HPPA uses register 2 for branch and link */

   /* CODE_ADDRESS is the address of the code jumped to through the code field */

   /* MAKE_CF creates an appropriate code field at the cfa; ca is the code address */
   /* we use ble and a register, since 'bl' only has 21 bits displacement */
#endif

#ifdef DIRECT_THREADED

#  ifdef DEBUG
#	  define DOUT(a,b,c,d)  fprintf(stderr,a,b,c,d)
#  else
#    define DOUT(a,b,c,d)
#  endif

#  define ASS17(n)(((((n) >> 13) & 0x1F) << 16)| /* first 5 bits */ \
		   ((((n) >>  2) & 0x3FF) << 3)| /* second 11 bits */ \
		   ((((n) >> 12) & 0x1) << 2)  | /* lo sign (aaarg!) */ \
		   (((n) < 0) << 0)) /* sign bit */

#  define DIS17(n)(((((n) >> 16) & 0x1F) << 13)| /* first 5 bits */ \
		   ((((n) >>  3) & 0x3FF) << 2)| /* second 11 bits */ \
		   ((((n) >>  2) & 0x1) << 12) | /* lo sign (aaarg!) */ \
		   (-((n) & 1) << 18)) /* sign bit */

#  define CODE_ADDRESS(cfa)\
((Label)({ \
	     unsigned int *_cfa=(unsigned int *)(cfa); unsigned _ca; \
	     if((_cfa[0] & 0xFFE0E002) == 0xE8000000) /* relative branch */ \
	     { \
		 _ca = _cfa[0]; \
		 _ca = DIS17(_ca); \
		 _ca += (int) (_cfa + 1); \
	     } \
	     else if((_cfa[0] & 0xFFE0E002) == 0xE0000000) /* absolute branch */ \
	     { \
		 _ca = _cfa[0]; \
		 _ca = DIS17(_ca)-4; \
	     } \
	     else \
	     { \
		 _ca = _cfa[0]; \
		 _ca = (_ca<<31) | \
		 ((_ca>>1 ) & 0x00001800) | \
		 ((_ca>>3 ) & 0x0003E000) | \
		 ((_ca<<4 ) & 0x000C0000) | \
		 ((_ca<<19) & 0x7FF00000) |  \
		 ((_cfa[1]>>1) & 0xFFC); \
	     } \
	     /* printf("code-address at %08x: %08x\n",_ca,_cfa); */ \
	     _ca; \
	 }))

#  define MAKE_CF(cfa,ca) \
({ \
     long *_cfa   = (long *)(cfa); \
     int _ca      = (int)(ca)+4; \
     int _dp      = _ca-(int)(_cfa+2); \
     \
     if(_ca < 0x40000) /* Branch absolute */ \
     { \
	 _cfa[0] =((0x38 << 26) | /* major opcode */ \
		   (   0 << 21) | /* register */ \
		   (   0 << 13) | /* space register */ \
		   (   0 <<  1) | /* if 1, don't execute delay slot */ \
		   ASS17(_ca)); \
	 _cfa[1] = ((long *)(_ca))[-1]; /* or %r0,%r0,%r0 */; \
     } \
     else if(_dp < 0x40000 || _dp >= -0x40000) \
     { \
	 _cfa[0] =((0x3A << 26) | /* major opcode */ \
		   (   0 << 21) | /* register */ \
		   (   0 << 13) | /* space register */ \
		   (   0 <<  1) | /* if 1, don't execute delay slot */ \
		   ASS17(_dp)); \
	 _cfa[1] = ((long *)(_ca))[-1]; /* 0x08000240 or %r0,%r0,%r0 */; \
     } \
     else \
     { \
	 _ca -= 4; \
	 _cfa[0] = (0x08 << 26) | \
	 ((int)_ca<0) | \
	 (_ca & 0x00001800)<<1 | \
	 (_ca & 0x0003E000)<<3 | \
	 (_ca & 0x000C0000)>>4 | \
	 (_ca & 0x7FF00000)>>19; \
	 _ca &= 0x3FF; \
	 _cfa[1] =((0x38 << 26) | /* major opcode */ \
		   (   1 << 21) | /* register */ \
		   (   0 << 13) | /* space register */ \
		   (   1 <<  1) | /* if 1, don't execute delay slot */ \
		   ASS17(_ca)); \
     } \
     DOUT("%08x: %08x,%08x\n",(int)_cfa,_cfa[0],_cfa[1]); \
 })
/* HP wins the price for the most obfuscated binary opcode */

/* this is the point where the does code starts if label points to the
 * jump dodoes */

/* this is a special version of DOES_CODE for use in dodoes */
#  define DOES_CODE1(cfa)	((Xt *)(((long *)(cfa))[1]))

#  define DOES_CODE(cfa) \
   (((((*(long *)(cfa)) & 0xF7E0E002) == 0xE0000000) && \
     ((long)(CODE_ADDRESS(CODE_ADDRESS(cfa))) == (long)symbols[DODOES])) ? \
    DOES_CODE1(cfa) : 0L)

/*	({register Xt * _ret asm("%r31"); _ret;}) */

/* HPPA uses register 2 for branch and link */

#  define DOES_HANDLER_SIZE 8
#  define MAKE_DOES_HANDLER(cfa)  ({ *(long *)(cfa)=DODOES; })
#ifdef undefined
#  define MAKE_DOES_HANDLER(cfa) \
({ \
     long *_cfa   = (long *)(cfa); \
     int _ca      = (int)symbols[DODOES]; \
     int _dp      = _ca-(int)(_cfa+2); \
     \
     if(_ca < 0x40000) /* Branch absolute */ \
     { \
	 _cfa[0] =((0x38 << 26) | /* major opcode */ \
		   (   0 << 21) | /* register */ \
		   (   0 << 13) | /* space register */ \
		   (   0 <<  1) | /* if 1, don't execute delay slot */ \
		   ASS17(_ca)); \
	 _cfa[1] = 0x08000240 /* or %r0,%r0,%r0 */; \
     } \
     else if(_dp < 0x40000 || _dp >= -0x40000) \
     { \
	 _cfa[0] =((0x3A << 26) | /* major opcode */ \
		   (   0 << 21) | /* register */ \
		   (   0 << 13) | /* space register */ \
		   (   0 <<  1) | /* if 1, don't execute delay slot */ \
		   ASS17(_dp)); \
	 _cfa[1] = 0x08000240 /* or %r0,%r0,%r0 */; \
     } \
     else \
     { \
	 _ca -= 4; \
	 _cfa[0] = ((0x08 << 26) | \
		    ((int)_ca<0) | \
		    (_ca & 0x00001800)<<1 | \
		    (_ca & 0x0003E000)<<3 | \
		    (_ca & 0x000C0000)>>4 | \
		    (_ca & 0x7FF00000)>>19); \
	 _ca &= 0x3FF; \
	 _cfa[1] =((0x38 << 26) | /* major opcode */ \
		   (   1 << 21) | /* register */ \
		   (   0 << 13) | /* space register */ \
		   (   1 <<  1) | /* if 1, don't execute delay slot */ \
		   ASS17(_ca)); \
     } \
     DOUT("%08x: %08x,%08x\n",(int)_cfa,_cfa[0],_cfa[1]); \
 })
#endif

#  define MAKE_DOES_CF(cfa,ca) \
({ \
     long *_cfa   = (long *)(cfa); \
     int _ca      = (int)symbols[DODOES]; \
     int _dp      = _ca-(int)(_cfa+2); \
     \
     if(_ca < 0x40000) /* Branch absolute */ \
     { \
	 _cfa[0] =((0x38 << 26) | /* major opcode */ \
		   (   0 << 21) | /* register */ \
		   (   0 << 13) | /* space register */ \
		   (   1 <<  1) | /* if 1, don't execute delay slot */ \
		   ASS17(_ca)); \
	 _cfa[1] = (long)(ca); \
     } \
     else if(_dp < 0x40000 || _dp >= -0x40000) \
     { \
	 _cfa[0] =((0x3A << 26) | /* major opcode */ \
		   (   0 << 21) | /* register */ \
		   (   0 << 13) | /* space register */ \
		   (   1 <<  1) | /* if 1, don't execute delay slot */ \
		   ASS17(_dp)); \
	 _cfa[1] = (long)(ca); \
     } \
     else \
     { \
	 fprintf(stderr,"DOESCFA assignment failed, use ITC instead of DTC\n"); exit(1); \
     } \
     DOUT("%08x: %08x,%08x\n",(int)_cfa,_cfa[0],_cfa[1]); \
 })
/* this stores a call dodoes at addr */
#endif

#undef HAVE_LOG1P
#undef HAVE_RINT

#ifdef FORCE_REG
#define IPREG asm("%r10")
#define SPREG asm("%r9")
#define RPREG asm("%r8")
#define LPREG asm("%r7")
#define CFAREG asm("%r6")
#define TOSREG asm("%r11")
#endif /* FORCE_REG */
