/* Gforth virtual machine (aka inner interpreter)

  Copyright (C) 1995,1996,1997,1998,2000,2003 Free Software Foundation, Inc.

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

#include "config.h"
#include "forth.h"
#include <ctype.h>
#include <stdio.h>
#include <string.h>
#include <math.h>
#include <assert.h>
#include <stdlib.h>
#include <errno.h>
#include "io.h"
#include "threaded.h"
#ifndef STANDALONE
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <time.h>
#include <sys/time.h>
#include <unistd.h>
#include <pwd.h>
#include <dirent.h>
#include <sys/resource.h>
#ifdef HAVE_FNMATCH_H
#include <fnmatch.h>
#else
#include "fnmatch.h"
#endif
#else
#include "systypes.h"
#endif

#if defined(HAVE_LIBDL) || defined(HAVE_DLOPEN) /* what else? */
#include <dlfcn.h>
#endif
#if defined(_WIN32)
#include <windows.h>
#endif
#ifdef hpux
#include <dl.h>
#endif

#ifdef HAS_FFCALL
#include <avcall.h>
#include <callback.h>
#endif

#ifndef SEEK_SET
/* should be defined in stdio.h, but some systems don't have it */
#define SEEK_SET 0
#endif

#ifndef HAVE_FSEEKO
#define fseeko fseek
#endif

#ifndef HAVE_FTELLO
#define ftello ftell
#endif

#define NULLC '\0'

#ifdef MEMCMP_AS_SUBROUTINE
extern int gforth_memcmp(const char * s1, const char * s2, size_t n);
#define memcmp(s1,s2,n) gforth_memcmp(s1,s2,n)
#endif

#define NEWLINE	'\n'

/* conversion on fetch */

#define vm_Cell2f(_cell,_x)		((_x)=(Bool)(_cell))
#define vm_Cell2c(_cell,_x)		((_x)=(Char)(_cell))
#define vm_Cell2n(_cell,_x)		((_x)=(Cell)(_cell))
#define vm_Cell2w(_cell,_x)		((_x)=(Cell)(_cell))
#define vm_Cell2u(_cell,_x)		((_x)=(UCell)(_cell))
#define vm_Cell2a_(_cell,_x)		((_x)=(Cell *)(_cell))
#define vm_Cell2c_(_cell,_x)		((_x)=(Char *)(_cell))
#define vm_Cell2f_(_cell,_x)		((_x)=(Float *)(_cell))
#define vm_Cell2df_(_cell,_x)		((_x)=(DFloat *)(_cell))
#define vm_Cell2sf_(_cell,_x)		((_x)=(SFloat *)(_cell))
#define vm_Cell2xt(_cell,_x)		((_x)=(Xt)(_cell))
#define vm_Cell2f83name(_cell,_x)	((_x)=(struct F83Name *)(_cell))
#define vm_Cell2longname(_cell,_x)	((_x)=(struct Longname *)(_cell))
#define vm_Float2r(_float,_x)		(_x=_float)

/* conversion on store */

#define vm_f2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_c2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_n2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_w2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_u2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_a_2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_c_2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_f_2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_df_2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_sf_2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_xt2Cell(_x,_cell)		((_cell)=(Cell)(_x))
#define vm_f83name2Cell(_x,_cell)	((_cell)=(Cell)(_x))
#define vm_longname2Cell(_x,_cell)	((_cell)=(Cell)(_x))
#define vm_r2Float(_x,_float)		(_float=_x)

#define vm_Cell2Cell(_x,_y)		(_y=_x)

#ifdef NO_IP
#define IMM_ARG(access,value)		(VARIANT(value))
#else
#define IMM_ARG(access,value)		(access)
#endif

/* if machine.h has not defined explicit registers, define them as implicit */
#ifndef IPREG
#define IPREG
#endif
#ifndef SPREG
#define SPREG
#endif
#ifndef RPREG
#define RPREG
#endif
#ifndef FPREG
#define FPREG
#endif
#ifndef LPREG
#define LPREG
#endif
#ifndef CFAREG
#define CFAREG
#endif
#ifndef UPREG
#define UPREG
#endif
#ifndef TOSREG
#define TOSREG
#endif
#ifndef FTOSREG
#define FTOSREG
#endif

#ifndef CPU_DEP1
# define CPU_DEP1 0
#endif

/* instructions containing SUPER_END must be the last instruction of a
   super-instruction (e.g., branches, EXECUTE, and other instructions
   ending the basic block). Instructions containing SET_IP get this
   automatically, so you usually don't have to write it.  If you have
   to write it, write it after IP points to the next instruction.
   Used for profiling.  Don't write it in a word containing SET_IP, or
   the following block will be counted twice. */
#ifdef VM_PROFILING
#define SUPER_END  vm_count_block(IP)
#else
#define SUPER_END
#endif
#define SUPER_CONTINUE

#ifdef GFORTH_DEBUGGING
#define NAME(string) { saved_ip=ip; asm(""); }
/* the asm here is to avoid reordering of following stuff above the
   assignment; this is an old-style asm (no operands), and therefore
   is treated like "asm volatile ..."; i.e., it prevents most
   reorderings across itself.  We want the assignment above first,
   because the stack loads may already cause a stack underflow. */
#elif DEBUG
#       define  NAME(string)    fprintf(stderr,"%08lx depth=%3ld: "string"\n",(Cell)ip,sp0+3-sp);
#else
#	define	NAME(string)
#endif

#ifdef DEBUG
#define CFA_TO_NAME(__cfa) \
      Cell len, i; \
      char * name = __cfa; \
      for(i=0; i<32; i+=sizeof(Cell)) { \
        len = ((Cell*)name)[-1]; \
        if(len < 0) { \
	  len &= 0x1F; \
          if((len+sizeof(Cell)) > i) break; \
	} len = 0; \
	name -= sizeof(Cell); \
      }
#endif

#ifdef HAS_FFCALL
#define SAVE_REGS IF_spTOS(sp[0]=spTOS); IF_fpTOS(fp[0]=fpTOS); SP=sp; FP=fp; RP=rp; LP=lp;
#define REST_REGS sp=SP; fp=FP; rp=RP; lp=LP; IF_spTOS(spTOS=sp[0]); IF_fpTOS(fpTOS=fp[0]);
#endif

#if !defined(ENGINE)
/* normal engine */
#define VARIANT(v)	(v)
#define JUMP(target)	goto I_noop
#define LABEL(name) J_##name: asm(""); I_##name:

#elif ENGINE==2
/* variant with padding between VM instructions for finding out
   cross-inst jumps (for dynamic code) */
#define engine engine2
#define VARIANT(v)	(v)
#define JUMP(target)	goto I_noop
#define LABEL(name) J_##name: SKIP16; I_##name:
#define IN_ENGINE2

#elif ENGINE==3
/* variant with different immediate arguments for finding out
   immediate arguments (for native code) */
#define engine engine3
#define VARIANT(v)	((v)^0xffffffff)
#define JUMP(target)	goto K_lit
#define LABEL(name) J_##name: asm(""); I_##name:
#else
#error illegal ENGINE value
#endif /* ENGINE */

#define LABEL2(name) K_##name:

Label *engine(Xt *ip0, Cell *sp0, Cell *rp0, Float *fp0, Address lp0)
/* executes code at ip, if ip!=NULL
   returns array of machine code labels (for use in a loader), if ip==NULL
*/
{
#ifndef GFORTH_DEBUGGING
  register Cell *rp RPREG;
#endif
#ifndef NO_IP
  register Xt *ip IPREG = ip0;
#endif
  register Cell *sp SPREG = sp0;
  register Float *fp FPREG = fp0;
  register Address lp LPREG = lp0;
  register Xt cfa CFAREG;
#ifdef MORE_VARS
  MORE_VARS
#endif
#ifdef HAS_FFCALL
  av_alist alist;
  extern va_alist clist;
  float frv;
  int irv;
  double drv;
  long long llrv;
  void * prv;
#endif
  register Address up UPREG = UP;
  IF_spTOS(register Cell spTOS TOSREG;)
  IF_fpTOS(register Float fpTOS FTOSREG;)
#if defined(DOUBLY_INDIRECT)
  static Label *symbols;
  static void *routines[]= {
#define MAX_SYMBOLS (sizeof(routines)/sizeof(routines[0]))
#else /* !defined(DOUBLY_INDIRECT) */
  static Label symbols[]= {
#define MAX_SYMBOLS (sizeof(symbols)/sizeof(symbols[0]))
#endif /* !defined(DOUBLY_INDIRECT) */
#define INST_ADDR(name) ((Label)&&I_##name)
#include "prim_lab.i"
#undef INST_ADDR
    (Label)0,
#define INST_ADDR(name) ((Label)&&K_##name)
#include "prim_lab.i"
#undef INST_ADDR
#define INST_ADDR(name) ((Label)&&J_##name)
#include "prim_lab.i"
#undef INST_ADDR
    (Label)&&after_last
  };
#ifdef CPU_DEP2
  CPU_DEP2
#endif

  rp = rp0;
#ifdef DEBUG
  fprintf(stderr,"ip=%x, sp=%x, rp=%x, fp=%x, lp=%x, up=%x\n",
          (unsigned)ip0,(unsigned)sp,(unsigned)rp,
	  (unsigned)fp,(unsigned)lp,(unsigned)up);
#endif

  if (ip0 == NULL) {
#if defined(DOUBLY_INDIRECT)
#define CODE_OFFSET (26*sizeof(Cell))
#define XT_OFFSET (22*sizeof(Cell))
    int i;
    Cell code_offset = offset_image? CODE_OFFSET : 0;
    Cell xt_offset = offset_image? XT_OFFSET : 0;

    symbols = (Label *)(malloc(MAX_SYMBOLS*sizeof(Cell)+CODE_OFFSET)+code_offset);
    xts = (Label *)(malloc(MAX_SYMBOLS*sizeof(Cell)+XT_OFFSET)+xt_offset);
    for (i=0; i<DOESJUMP+1; i++)
      xts[i] = symbols[i] = (Label)routines[i];
    for (; routines[i]!=0; i++) {
      if (i>=MAX_SYMBOLS) {
	fprintf(stderr,"gforth-ditc: more than %ld primitives\n",(long)MAX_SYMBOLS);
	exit(1);
      }
      xts[i] = symbols[i] = &routines[i];
    }
#endif /* defined(DOUBLY_INDIRECT) */
    return symbols;
  }

  IF_spTOS(spTOS = sp[0]);
  IF_fpTOS(fpTOS = fp[0]);
/*  prep_terminal(); */
#ifdef NO_IP
  goto *(*(Label *)ip0);
#else
  SET_IP(ip);
  SUPER_END; /* count the first block, too */
  NEXT;
#endif

#ifdef CPU_DEP3
  CPU_DEP3
#endif

#include "prim.i"
  after_last: return (Label *)0;
  /*needed only to get the length of the last primitive */
}
