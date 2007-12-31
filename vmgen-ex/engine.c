/* vm interpreter wrapper

  Copyright (C) 2001,2002,2003,2007 Free Software Foundation, Inc.

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

#include "mini.h"

#define USE_spTOS 1

#ifdef USE_spTOS
#define IF_spTOS(x) x
#else
#define IF_spTOS(x)
#endif

#ifdef VM_DEBUG
#define NAME(_x) if (vm_debug) {fprintf(vm_out, "%p: %-20s, ", ip-1, _x); fprintf(vm_out,"fp=%p, sp=%p", fp, sp);}
#else
#define NAME(_x)
#endif

/* different threading schemes for different architectures; the sparse
   numbering is there for historical reasons */

/* here you select the threading scheme; I have only set this up for
   386 and generic, because I don't know what preprocessor macros to
   test for (Gforth uses config.guess instead).  Anyway, it's probably
   best to build them all and select the fastest instead of hardwiring
   a specific scheme for an architecture.  E.g., scheme 8 is fastest
   for Gforth "make bench" on a 486, whereas scheme 5 is fastest for
   "mini fib.mini" on an Athlon */
#ifndef THREADING_SCHEME
#define THREADING_SCHEME 5
#endif /* defined(THREADING_SCHEME) */

#ifdef __GNUC__
#if THREADING_SCHEME==1
/* direct threading scheme 1: autoinc, long latency (HPPA, Sharc) */
#  define NEXT_P0	({cfa=*ip++;})
#  define IP		(ip-1)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({goto *cfa;})
#endif

#if THREADING_SCHEME==3
/* direct threading scheme 3: autoinc, low latency (68K) */
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	({cfa=*ip++;})
#  define NEXT_P2	({goto *cfa;})
#endif

#if THREADING_SCHEME==5
/* direct threading scheme 5: early fetching (Alpha, MIPS) */
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=ip[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto *cfa;})
#endif

#if THREADING_SCHEME==8
/* direct threading scheme 8: i386 hack */
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*IP)
#  define INC_IP(const_inc)	({ ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto **(ip-1);})
#endif

#if THREADING_SCHEME==9
/* direct threading scheme 9: prefetching (for PowerPC) */
/* note that the "cfa=next_cfa;" occurs only in NEXT_P1, because this
   works out better with the capabilities of gcc to introduce and
   schedule the mtctr instruction. */
#  define NEXT_P0
#  define IP		ip
#  define SET_IP(p)	({ip=(p); next_cfa=*ip; NEXT_P0;})
#  define NEXT_INST	(next_cfa)
#  define INC_IP(const_inc)	({next_cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	
#  define NEXT_P1	({cfa=next_cfa; ip++; next_cfa=*ip;})
#  define NEXT_P2	({goto *cfa;})
#  define MORE_VARS	Inst next_cfa;
#endif

#if THREADING_SCHEME==10
/* direct threading scheme 10: plain (no attempt at scheduling) */
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({cfa=*ip++; goto *cfa;})
#endif


#define NEXT ({DEF_CA NEXT_P1; NEXT_P2;})
#define IPTOS NEXT_INST

#define INST_ADDR(name) (Label)&&I_##name
#define LABEL(name) I_##name:
#else /* !defined(__GNUC__) */
/* use switch dispatch */
#define DEF_CA
#define NEXT_P0
#define NEXT_P1
#define NEXT_P2 goto next_inst;
#define SET_IP(p)	(ip=(p))
#define IP              ip
#define NEXT_INST	(*ip)
#define INC_IP(const_inc)	(ip+=(const_inc))
#define IPTOS NEXT_INST
#define INST_ADDR(name) I_##name
#define LABEL(name) case I_##name:

#endif /* !defined(__GNUC__) */

#define LABEL2(x)

#ifdef VM_PROFILING
#define SUPER_END  vm_count_block(IP)
#else
#define SUPER_END
#endif

#ifndef __GNUC__
enum {
#include "mini-labels.i"
};
#endif

#if defined(__GNUC__) && ((__GNUC__==2 && defined(__GNUC_MINOR__) && __GNUC_MINOR__>=7)||(__GNUC__>2))
#define MAYBE_UNUSED __attribute__((unused))
#else
#define MAYBE_UNUSED
#endif

/* the return type can be anything you want it to */
Cell engine(Inst *ip0, Cell *sp, char *fp)
{
  /* VM registers (you may want to use gcc's "Explicit Reg Vars" here) */
  Inst * ip;
  Inst * cfa;
#ifdef USE_spTOS
  Cell   spTOS;
#else
#define spTOS (sp[0])
#endif
  static Label labels[] = {
#include "mini-labels.i"
  };
#ifdef MORE_VARS
  MORE_VARS
#endif

  if (vm_debug)
      fprintf(vm_out,"entering engine(%p,%p,%p)\n",ip0,sp,fp);
  if (ip0 == NULL) {
    vm_prim = labels;
    return 0;
  }

  /* I don't have a clue where these things come from,
     but I've put them in macros.h for the moment */
  IF_spTOS(spTOS = sp[0]);

  SET_IP(ip0);
  SUPER_END;  /* count the BB starting at ip0 */

#ifdef __GNUC__
  NEXT;
#include "mini-vm.i"
#else  
 next_inst:
  switch(*ip++) {
#include "mini-vm.i"
  default:
    fprintf(stderr,"unknown instruction %d at %p\n", ip[-1], ip-1);
    exit(1);
  }
#endif
}
