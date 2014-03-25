/* Gforth virtual machine (aka inner interpreter)

  Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008 Free Software Foundation, Inc.

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

#if defined(GFORTH_DEBUGGING) || defined(INDIRECT_THREADED) || defined(DOUBLY_INDIRECT) || defined(VM_PROFILING)
#define USE_NO_TOS
#else
#define USE_TOS
#endif

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
#ifdef HAVE_WCHAR_H
#include <wchar.h>
#endif
#include <sys/resource.h>
#ifdef HAVE_FNMATCH_H
#include <fnmatch.h>
#else
#include "fnmatch.h"
#endif
#else
/* #include <systypes.h> */
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

/* These two flags control whether divisions are checked by software.
   The CHECK_DIVISION_SW is for those cases where the event is a
   division by zero or overflow on the C level, and might be reported
   by hardware; we might check forr that in autoconf and set the
   switch appropriately, but currently don't.  The CHECK_DIVISION flag
   is for the other cases. */
#ifdef GFORTH_DEBUGGING
#define CHECK_DIVISION_SW 1
#define CHECK_DIVISION 1
#else
#define CHECK_DIVISION_SW 0
#define CHECK_DIVISION 0
#endif

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
#ifndef CAREG
#define CAREG
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
#ifndef spbREG
#define spbREG
#endif
#ifndef spcREG
#define spcREG
#endif
#ifndef spdREG
#define spdREG
#endif
#ifndef speREG
#define speREG
#endif
#ifndef spfREG
#define spfREG
#endif
#ifndef spgREG
#define spgREG
#endif
#ifndef sphREG
#define sphREG
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

#ifdef ASMCOMMENT
/* an individualized asm statement so that (hopefully) gcc's optimizer
   does not do cross-jumping */
#define asmcomment(string) asm(ASMCOMMENT string)
#else
/* we don't know how to do an asm comment, so we just do an empty asm */
#define asmcomment(string) asm("")
#endif

#ifdef GFORTH_DEBUGGING
#if DEBUG
#define NAME(string) { saved_ip=ip; asmcomment(string); fprintf(stderr,"%08lx depth=%3ld tos=%016lx: "string"\n",(Cell)ip,sp0+3-sp,sp[0]);}
#else /* !DEBUG */
#define NAME(string) { saved_ip=ip; asm(""); }
/* the asm here is to avoid reordering of following stuff above the
   assignment; this is an old-style asm (no operands), and therefore
   is treated like "asm volatile ..."; i.e., it prevents most
   reorderings across itself.  We want the assignment above first,
   because the stack loads may already cause a stack underflow. */
#endif /* !DEBUG */
#elif DEBUG
#       define  NAME(string)    {Cell __depth=sp0+3-sp; int i; fprintf(stderr,"%08lx depth=%3ld: "string,(Cell)ip,sp0+3-sp); for (i=__depth-1; i>0; i--) fprintf(stderr, " $%lx",sp[i]); fprintf(stderr, " $%lx\n",spTOS); }
#else
#	define	NAME(string) asmcomment(string);
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

#ifdef STANDALONE
jmp_buf throw_jmp_buf;

void throw(int code)
{
  longjmp(throw_jmp_buf,code); /* !! or use siglongjmp ? */
}
#endif

#if defined(HAS_FFCALL) || defined(HAS_LIBFFI)
#define SAVE_REGS IF_fpTOS(fp[0]=fpTOS); gforth_SP=sp; gforth_FP=fp; gforth_RP=rp; gforth_LP=lp;
#define REST_REGS sp=gforth_SP; fp=gforth_FP; rp=gforth_RP; lp=gforth_LP; IF_fpTOS(fpTOS=fp[0]);
#endif

#if !defined(ENGINE)
/* normal engine */
#define VARIANT(v)	(v)
#define JUMP(target)	goto I_noop
#define LABEL(name) H_##name: asm(""); I_##name:
#define LABEL3(name) J_##name: asm("");

#elif ENGINE==2
/* variant with padding between VM instructions for finding out
   cross-inst jumps (for dynamic code) */
#define gforth_engine gforth_engine2
#define VARIANT(v)	(v)
#define JUMP(target)	goto I_noop
#define LABEL(name) H_##name: SKIP16; I_##name:
/* the SKIP16 after LABEL3 is there, because the ARM gcc may place
   some constants after the final branch, and may refer to them from
   the code before label3.  Since we don't copy the constants, we have
   to make sure that such code is recognized as non-relocatable. */
#define LABEL3(name) J_##name: SKIP16;

#elif ENGINE==3
/* variant with different immediate arguments for finding out
   immediate arguments (for native code) */
#define gforth_engine gforth_engine3
#define VARIANT(v)	((v)^0xffffffff)
#define JUMP(target)	goto K_lit
#define LABEL(name) H_##name: asm(""); I_##name:
#define LABEL3(name) J_##name: asm("");
#else
#error illegal ENGINE value
#endif /* ENGINE */

/* the asm(""); is there to get a stop compiled on Itanium */
#define LABEL2(name) K_##name: asm("");

Label *gforth_engine(Xt *ip0, Cell *sp0, Cell *rp0, Float *fp0, Address lp0 sr_proto)
/* executes code at ip, if ip!=NULL
   returns array of machine code labels (for use in a loader), if ip==NULL
*/
{
#if defined(GFORTH_DEBUGGING)
#if defined(GLOBALS_NONRELOC)
  register saved_regs *saved_regs_p TOSREG = saved_regs_p0;
#endif /* defined(GLOBALS_NONRELOC) */
#else /* !defined(GFORTH_DEBUGGING) */
  register Cell *rp RPREG;
#endif /* !defined(GFORTH_DEBUGGING) */
#ifndef NO_IP
  register Xt *ip IPREG = ip0;
#endif
  register Cell *sp SPREG = sp0;
  register Float *fp FPREG = fp0;
  register Address lp LPREG = lp0;
  register Xt cfa CFAREG;
  register Label real_ca CAREG;
#ifdef MORE_VARS
  MORE_VARS
#endif
#ifdef HAS_FFCALL
  av_alist alist;
  extern va_alist gforth_clist;
  float frv;
  int irv;
  double drv;
  long long llrv;
  void * prv;
#endif
  register Address up UPREG = gforth_UP;
#if !defined(GFORTH_DEBUGGING)
  register Cell MAYBE_UNUSED spTOS TOSREG;
  register Cell MAYBE_UNUSED spb spbREG;
  register Cell MAYBE_UNUSED spc spcREG;
  register Cell MAYBE_UNUSED spd spdREG;
  register Cell MAYBE_UNUSED spe speREG;
  register Cell MAYBE_UNUSED spf speREG;
  register Cell MAYBE_UNUSED spg speREG;
  register Cell MAYBE_UNUSED sph speREG;
  IF_fpTOS(register Float fpTOS FTOSREG;)
#endif /* !defined(GFORTH_DEBUGGING) */
#if defined(DOUBLY_INDIRECT)
  static Label *symbols;
  static void *routines[]= {
#define MAX_SYMBOLS (sizeof(routines)/sizeof(routines[0]))
#else /* !defined(DOUBLY_INDIRECT) */
  static Label symbols[]= {
#define MAX_SYMBOLS (sizeof(symbols)/sizeof(symbols[0]))
#endif /* !defined(DOUBLY_INDIRECT) */
#define INST_ADDR(name) ((Label)&&I_##name)
#include PRIM_LAB_I
#undef INST_ADDR
    (Label)0,
#define INST_ADDR(name) ((Label)&&K_##name)
#include PRIM_LAB_I
#undef INST_ADDR
#define INST_ADDR(name) ((Label)&&J_##name)
#include PRIM_LAB_I
#undef INST_ADDR
    (Label)&&after_last,
    (Label)&&before_goto,
    (Label)&&after_goto,
/* just mention the H_ labels, so the SKIP16s are not optimized away */
#define INST_ADDR(name) ((Label)&&H_##name)
#include PRIM_LAB_I
#undef INST_ADDR
  };
#ifdef STANDALONE
#define INST_ADDR(name) ((Label)&&I_##name)
#include "image.i"
#undef INST_ADDR
#endif
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
#ifdef STANDALONE
    return image;
#else
    return symbols;
#endif
  }

#if !(defined(GFORTH_DEBUGGING) || defined(INDIRECT_THREADED) || defined(DOUBLY_INDIRECT) || defined(VM_PROFILING))
  sp += STACK_CACHE_DEFAULT-1;
  /* some of those registers are dead, but its simpler to initialize them all */  spTOS = sp[0];
  spb = sp[-1];
  spc = sp[-2];
  spd = sp[-3];
  spe = sp[-4];
  spf = sp[-5];
  spg = sp[-6];
  sph = sp[-7];
#endif

  IF_fpTOS(fpTOS = fp[0]);
/*  prep_terminal(); */
#ifdef NO_IP
  goto *(*(Label *)ip0);
  before_goto:
  goto *real_ca;
  after_goto:;
#else
  SET_IP(ip);
  SUPER_END; /* count the first block, too */
  NEXT;
#endif

#ifdef CPU_DEP3
  CPU_DEP3
#endif

#include PRIM_I
  after_last:   FIRST_NEXT;
  /*needed only to get the length of the last primitive */

  return (Label *)0;
}
