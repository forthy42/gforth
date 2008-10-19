/* common header file

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

#include "config.h"
#include "128bit.h"
#include <stdio.h>
#include <sys/time.h>
#include <unistd.h>
#ifndef STANDALONE
#if defined(HAVE_LIBLTDL)
#include <ltdl.h>
#endif
#endif

#if !defined(FORCE_LL) && !defined(BUGGY_LONG_LONG)
#define BUGGY_LONG_LONG
#endif

#if defined(DOUBLY_INDIRECT)||defined(INDIRECT_THREADED)||defined(VM_PROFILING)
#define NO_DYNAMIC
#endif

#if defined(DOUBLY_INDIRECT)
#  undef DIRECT_THREADED
#  undef INDIRECT_THREADED
#  define INDIRECT_THREADED
#endif

#if defined(GFORTH_DEBUGGING) || defined(INDIRECT_THREADED) || defined(DOUBLY_INDIRECT) || defined(VM_PROFILING)
#  undef USE_TOS
#  undef USE_FTOS
#  undef USE_NO_TOS
#  undef USE_NO_FTOS
#  define USE_NO_TOS
#  define USE_NO_FTOS

#define PRIM_I "prim.i"
#define PRIM_LAB_I "prim_lab.i"
#define PRIM_NAMES_I "prim_names.i"
#define PRIM_SUPEREND_I "prim_superend.i"
#define PRIM_NUM_I "prim_num.i"
#define PRIM_GRP_I "prim_grp.i"
#define COSTS_I "costs.i"
#define SUPER2_I "super2.i"
/* #define PROFILE_I "profile.i" */

#else
/* gforth-fast or gforth-native */
#  undef USE_TOS
#  undef USE_FTOS
#  undef USE_NO_TOS
#  undef USE_NO_FTOS
#  define USE_TOS

#define PRIM_I "prim-fast.i"
#define PRIM_LAB_I "prim_lab-fast.i"
#define PRIM_NAMES_I "prim_names-fast.i"
#define PRIM_SUPEREND_I "prim_superend-fast.i"
#define PRIM_NUM_I "prim_num-fast.i"
#define PRIM_GRP_I "prim_grp-fast.i"
#define COSTS_I "costs-fast.i"
#define SUPER2_I "super2-fast.i"
/* profile.c uses profile.i but does not define VM_PROFILING */
/* #define PROFILE_I "profile-fast.i" */

#endif



#include <limits.h>

#if defined(NeXT)
#  include <libc.h>
#endif /* NeXT */

/* symbol indexed constants */

#define DOCOL	0
#define DOCON	1
#define DOVAR	2
#define DOUSER	3
#define DODEFER	4
#define DOFIELD	5
#define DOVAL	6
#define DODOES	7
#define DOESJUMP	8

/* the size of the DOESJUMP, which resides between DOES> and the does-code */
#define DOES_HANDLER_SIZE	(2*sizeof(Cell))

#include "machine.h"

/* C interface data types */

typedef WYDE_TYPE Wyde;
typedef TETRABYTE_TYPE Tetrabyte;
typedef unsigned WYDE_TYPE UWyde;
typedef unsigned TETRABYTE_TYPE UTetrabyte;

/* Forth data types */
/* Cell and UCell must be the same size as a pointer */
#define CELL_BITS	(sizeof(Cell) * CHAR_BIT)
#define CELL_MIN (((Cell)1)<<(sizeof(Cell)*CHAR_BIT-1))

#define HALFCELL_BITS	(CELL_BITS/2)
#define HALFCELL_MASK   ((~(UCell)0)>>HALFCELL_BITS)
#define UH(x)		(((UCell)(x))>>HALFCELL_BITS)
#define LH(x)		((x)&HALFCELL_MASK)
#define L2U(x)		(((UCell)(x))<<HALFCELL_BITS)
#define HIGHBIT(x)	(((UCell)(x))>>(CELL_BITS-1))

#define FLAG(b) (-(b))
#define FILEIO(error)	(FLAG(error) & -37)
#define FILEEXIST(error)	(FLAG(error) & -38)

#define F_TRUE (FLAG(0==0))
#define F_FALSE (FLAG(0!=0))

/* define this false if you want native division */
#ifdef FORCE_CDIV
#define FLOORED_DIV 0
#else
#define FLOORED_DIV ((1%-3)>0)
#endif

#if defined(BUGGY_LONG_LONG)

#define BUGGY_LL_CMP    /* compares not possible */
#define BUGGY_LL_MUL    /* multiplication not possible */
#define BUGGY_LL_DIV    /* division not possible */
#define BUGGY_LL_ADD    /* addition not possible */
#define BUGGY_LL_SHIFT  /* shift not possible */
#define BUGGY_LL_D2F    /* to float not possible */
#define BUGGY_LL_F2D    /* from float not possible */
#define BUGGY_LL_SIZE   /* long long "too short", so we use something else */

typedef struct {
  Cell hi;
  UCell lo;
} DCell;

typedef struct {
  UCell hi;
  UCell lo;
} UDCell;

#define DHI(x) (x).hi
#define DLO(x) (x).lo
#define DHI_IS(x,y) (x).hi=(y)
#define DLO_IS(x,y) (x).lo=(y)

#define UD2D(ud)	({UDCell _ud=(ud); (DCell){_ud.hi,_ud.lo};})
#define D2UD(d)		({DCell _d1=(d); (UDCell){_d1.hi,_d1.lo};})

/* shifts by less than CELL_BITS */
#define DLSHIFT(d,u) ({DCell _d=(d); UCell _u=(u); \
                       ((_u==0) ? \
                        _d : \
                        (DCell){(_d.hi<<_u)|(_d.lo>>(CELL_BITS-_u)), \
                                 _d.lo<<_u});})

#define UDLSHIFT(ud,u) D2UD(DLSHIFT(UD2D(ud),u))

#if SMALL_OFF_T
#define OFF2UD(o) ({UDCell _ud; _ud.hi=0; _ud.lo=(Cell)(o); _ud;})
#define UD2OFF(ud) ((ud).lo)
#else /* !SMALL_OFF_T */
#define OFF2UD(o) ({UDCell _ud; off_t _o=(o); _ud.hi=_o>>CELL_BITS; _ud.lo=(Cell)_o; _ud;})
#define UD2OFF(ud) ({UDCell _ud=(ud); (((off_t)_ud.hi)<<CELL_BITS)+_ud.lo;})
#endif /* !SMALL_OFF_T */
#define DZERO		((DCell){0,0})

#else /* !defined(BUGGY_LONG_LONG) */

/* DCell and UDCell must be twice as large as Cell */
typedef DOUBLE_CELL_TYPE DCell;
typedef DOUBLE_UCELL_TYPE UDCell;

#define DHI(x) ({ Double_Store _d; _d.d=(x); _d.cells.high; })
#define DLO(x) ({ Double_Store _d; _d.d=(x); _d.cells.low;  })

/* beware with the assignment: x is referenced twice! */
#define DHI_IS(x,y) ({ Double_Store _d; _d.d=(x); _d.cells.high=(y); (x)=_d.d; })
#define DLO_IS(x,y) ({ Double_Store _d; _d.d=(x); _d.cells.low =(y); (x)=_d.d; })

#define UD2D(ud)	((DCell)(ud))
#define D2UD(d)		((UDCell)(d))
#define OFF2UD(o)	((UDCell)(o))
#define UD2OFF(ud)	((off_t)(ud))
#define DZERO		((DCell)0)
/* shifts by less than CELL_BITS */
#define DLSHIFT(d,u)  ((d)<<(u))
#define UDLSHIFT(d,u)  ((d)<<(u))

#endif /* !defined(BUGGY_LONG_LONG) */

typedef union {
  struct {
#if defined(WORDS_BIGENDIAN)||defined(BUGGY_LONG_LONG)
    Cell high;
    UCell low;
#else
    UCell low;
    Cell high;
#endif
  } cells;
  DCell d;
  UDCell ud;
} Double_Store;

#define FETCH_DCELL_T(d_,lo,hi,t_)	({ \
				     Double_Store _d; \
				     _d.cells.low = (lo); \
				     _d.cells.high = (hi); \
				     (d_) = _d.t_; \
				 })

#define STORE_DCELL_T(d_,lo,hi,t_)	({ \
				     Double_Store _d; \
				     _d.t_ = (d_); \
				     (lo) = _d.cells.low; \
				     (hi) = _d.cells.high; \
				 })

#define vm_twoCell2d(lo,hi,d_)  FETCH_DCELL_T(d_,lo,hi,d);
#define vm_twoCell2ud(lo,hi,d_) FETCH_DCELL_T(d_,lo,hi,ud);

#define vm_d2twoCell(d_,lo,hi)  STORE_DCELL_T(d_,lo,hi,d);
#define vm_ud2twoCell(d_,lo,hi) STORE_DCELL_T(d_,lo,hi,ud);

typedef Label *Xt;

/* PFA gives the parameter field address corresponding to a cfa */
#define PFA(cfa)	(((Cell *)cfa)+2)
/* PFA1 is a special version for use just after a NEXT1 */
#define PFA1(cfa)	PFA(cfa)
/* CODE_ADDRESS is the address of the code jumped to through the code field */
#define CODE_ADDRESS(cfa)	(*(Xt)(cfa))

/* DOES_CODE is the Forth code does jumps to */
#if !defined(DOUBLY_INDIRECT)
#  define DOES_CA (symbols[DODOES])
#else /* defined(DOUBLY_INDIRECT) */
#  define DOES_CA ((Label)&xts[DODOES])
#endif /* defined(DOUBLY_INDIRECT) */



#define DOES_CODE1(cfa)	((Xt *)(cfa[1]))
/* MAKE_CF creates an appropriate code field at the cfa;
   ca is the code address */
#define MAKE_CF(cfa,ca) ((*(Label *)(cfa)) = ((Label)ca))
/* make a code field for a defining-word-defined word */
#define MAKE_DOES_CF(cfa,does_code)  ({MAKE_CF(cfa,DOES_CA);	\
				       ((Cell *)cfa)[1] = (Cell)(does_code);})

#define CF(const)	(-const-2)

#define CF_NIL	-1

#ifndef FLUSH_ICACHE
#warning flush-icache probably will not work (see manual)
#	define FLUSH_ICACHE(addr,size)
#warning no FLUSH_ICACHE, turning off dynamic native code by default
#undef NO_DYNAMIC_DEFAULT
#define NO_DYNAMIC_DEFAULT 1
#endif

#if defined(GFORTH_DEBUGGING) || defined(INDIRECT_THREADED) || defined(DOUBLY_INDIRECT) || defined(VM_PROFILING)
#define STACK_CACHE_DEFAULT 0
#else
#define STACK_CACHE_DEFAULT STACK_CACHE_DEFAULT_FAST
#endif

#ifdef USE_FTOS
#define IF_fpTOS(x) x
#else
#define IF_fpTOS(x)
#define fpTOS (fp[0])
#endif

#define IF_rpTOS(x)
#define rpTOS (rp[0])

typedef struct {
  Address base;		/* base address of image (0 if relocatable) */
  UCell checksum;	/* checksum of ca's to protect against some
			   incompatible	binary/executable combinations
			   (0 if relocatable) */
  UCell image_size;	/* all sizes in bytes */
  UCell dict_size;
  UCell data_stack_size;
  UCell fp_stack_size;
  UCell return_stack_size;
  UCell locals_stack_size;
  Xt *boot_entry;	/* initial ip for booting (in BOOT) */
  Xt *throw_entry;	/* ip after signal (in THROW) */
  Cell unused1;		/* possibly tib stack size */
  Label *xt_base;         /* base of DOUBLE_INDIRECT xts[], for comp-i.fs */
  Address data_stack_base; /* this and the following fields are initialized by the loader */
  Address fp_stack_base;
  Address return_stack_base;
  Address locals_stack_base;
} ImageHeader;
/* the image-header is created in main.fs */

#ifdef HAS_F83HEADERSTRING
struct F83Name {
  struct F83Name *next;  /* the link field for old hands */
  char		countetc;
  char		name[0];
};

#define F83NAME_COUNT(np)	((np)->countetc & 0x1f)
#endif
struct Longname {
  struct Longname *next;  /* the link field for old hands */
  Cell		countetc;
  char		name[0];
};

#define LONGNAME_COUNT(np)	((np)->countetc & (((~((UCell)0))<<3)>>3))

struct Cellpair {
  Cell n1;
  Cell n2;
};

struct Cellquad {
  Cell n1;
  Cell n2;
  Cell n3;
  Cell n4;
};

#define IOR(flag)	((flag)? -512-errno : 0)

#ifdef GFORTH_DEBUGGING
#if defined(GLOBALS_NONRELOC)
/* if globals cause non-relocatable primitives, keep saved_ip and rp
   in a structure and access it through locals */
typedef struct saved_regs {
  Xt *sr_saved_ip;
  Cell *sr_rp;
} saved_regs;
extern saved_regs saved_regs_v, *saved_regs_p;
#define saved_ip (saved_regs_p->sr_saved_ip)
#define rp       (saved_regs_p->sr_rp)
/* for use in gforth_engine header */
#error sr_proto not passed in fflib.fs callbacks (solution: disable GLOBALS_NONRELOC)
#define sr_proto , struct saved_regs *saved_regs_p0
#define sr_call  , saved_regs_p
#else /* !defined(GLOBALS_NONRELOC) */
extern Xt *saved_ip;
extern Cell *rp;
#define sr_proto
#define sr_call
#endif /* !defined(GLOBALS_NONRELOC) */
#else /* !defined(GFORTH_DEBUGGING) */
#define sr_proto
#define sr_call
#endif /* !defined(GFORTH_DEBUGGING) */

Label *gforth_engine(Xt *ip, Cell *sp, Cell *rp0, Float *fp, Address lp sr_proto);
Label *gforth_engine2(Xt *ip, Cell *sp, Cell *rp0, Float *fp, Address lp sr_proto);
Label *gforth_engine3(Xt *ip, Cell *sp, Cell *rp0, Float *fp, Address lp sr_proto);

/* engine/prim support routines */
Address gforth_alloc(Cell size);
char *cstr(Char *from, UCell size, int clear);
char *tilde_cstr(Char *from, UCell size, int clear);
Cell opencreate_file(char *s, Cell wfam, int flags, Cell *wiorp);
DCell timeval2us(struct timeval *tvp);
void cmove(Char *c_from, Char *c_to, UCell u);
void cmove_up(Char *c_from, Char *c_to, UCell u);
Cell compare(Char *c_addr1, UCell u1, Char *c_addr2, UCell u2);
struct Longname *listlfind(Char *c_addr, UCell u, struct Longname *longname1);
struct Longname *hashlfind(Char *c_addr, UCell u, Cell *a_addr);
struct Longname *tablelfind(Char *c_addr, UCell u, Cell *a_addr);
UCell hashkey1(Char *c_addr, UCell u, UCell ubits);
struct Cellpair parse_white(Char *c_addr1, UCell u1);
Cell rename_file(Char *c_addr1, UCell u1, Char *c_addr2, UCell u2);
struct Cellquad read_line(Char *c_addr, UCell u1, Cell wfileid);
struct Cellpair file_status(Char *c_addr, UCell u);
Cell to_float(Char *c_addr, UCell u, Float *r_p);
Float v_star(Float *f_addr1, Cell nstride1, Float *f_addr2, Cell nstride2, UCell ucount);
void faxpy(Float ra, Float *f_x, Cell nstridex, Float *f_y, Cell nstridey, UCell ucount);
UCell lshift(UCell u1, UCell n);
UCell rshift(UCell u1, UCell n);
int gforth_system(Char *c_addr, UCell u);
void gforth_ms(UCell u);
UCell gforth_dlopen(Char *c_addr, UCell u);
Cell capscompare(Char *c_addr1, UCell u1, Char *c_addr2, UCell u2);

/* signal handler stuff */
void install_signal_handlers(void);
void throw(int code);
/* throw codes */
#define BALL_DIVZERO     -10
#define BALL_RESULTRANGE -11

typedef void Sigfunc(int);
Sigfunc *bsd_signal(int signo, Sigfunc *func);

/* dblsub routines */
DCell dnegate(DCell d1);
UDCell ummul (UCell a, UCell b);
DCell mmul (Cell a, Cell b);
UDCell umdiv (UDCell u, UCell v);
DCell smdiv (DCell num, Cell denom);
DCell fmdiv (DCell num, Cell denom);

Cell memcasecmp(const Char *s1, const Char *s2, Cell n);

void vm_print_profile(FILE *file);
void vm_count_block(Xt *ip);

/* dynamic superinstruction stuff */
void compile_prim1(Cell *start);
void finish_code(void);
int forget_dyncode(Address code);
Label decompile_code(Label prim);

extern int offset_image;
extern int die_on_signal;
extern int ignore_async_signals;
extern UCell pagesize;
extern ImageHeader *gforth_header;
extern Label *vm_prims;
extern Label *xts;
extern Cell npriminfos;

#ifdef HAS_DEBUG
extern int debug;
#else
# define debug 0
#endif

extern Cell *gforth_SP;
extern Cell *gforth_RP;
extern Address gforth_LP;
extern Float *gforth_FP;
extern Address gforth_UP;
#ifndef HAS_LINKBACK
extern void * gforth_pointers[];
#endif

#ifdef HAS_FFCALL
extern Cell *gforth_RP;
extern Address gforth_LP;
extern void gforth_callback(Xt* fcall, void * alist);
#endif

#ifdef NO_IP
extern Label next_code;
#endif

#ifdef HAS_FILE
extern char* fileattr[6];
extern char* pfileattr[6];
extern int ufileattr[6];
#endif

#ifdef PRINT_SUPER_LENGTHS
Cell prim_length(Cell prim);
void print_super_lengths();
#endif

/* declare all the functions that are missing */
#ifndef HAVE_ATANH
extern double atanh(double r1);
extern double asinh(double r1);
extern double acosh(double r1);
#endif
#ifndef HAVE_ECVT
/* extern char* ecvt(double x, int len, int* exp, int* sign);*/
#endif
#ifndef HAVE_MEMMOVE
/* extern char *memmove(char *dest, const char *src, long n); */
#endif
#ifndef HAVE_POW10
extern double pow10(double x);
#endif
#ifndef HAVE_STRERROR
extern char *strerror(int err);
#endif
#ifndef HAVE_STRSIGNAL
extern char *strsignal(int sig);
#endif
#ifndef HAVE_STRTOUL
extern unsigned long int strtoul(const char *nptr, char **endptr, int base);
#endif

#define GROUP(x, n)
#define GROUPADD(n)
