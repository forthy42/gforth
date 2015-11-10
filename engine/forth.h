/* common header file

  Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014 Free Software Foundation, Inc.

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
#include <stdint.h>
#include <unistd.h>
#ifndef STANDALONE
#if defined(HAVE_LIBLTDL)
# include <ltdl.h>
#endif
#endif
#include <setjmp.h>
#ifdef HAVE_MCHECK
# include <mcheck.h>
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
#define DOABICODE	8
#define DOSEMIABICODE   9
#define DOEXTRA	10
#define DOCOLLOC 11
#define DODOESXT 12
#define DOER_MAX        12

#include "machine.h"

/* C interface data types */

typedef WYDE_TYPE Wyde;
typedef TETRABYTE_TYPE Tetrabyte;
typedef OCTABYTE_TYPE Octabyte;
typedef unsigned WYDE_TYPE UWyde;
typedef unsigned TETRABYTE_TYPE UTetrabyte;
typedef unsigned OCTABYTE_TYPE UOctabyte;

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

#define FLAG(b) (-(Cell)(b))
#define FILEIO(error)	(FLAG(error) & -37)
#define FILEEXIST(error)	(FLAG(error) & -38)

#define F_TRUE (FLAG(0==0))
#define F_FALSE (FLAG(0!=0))

typedef struct {
  Cell* sp;
  Float* fp;
} ptrpair;

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
#define D_IS(x,y,z) ({ (x).hi=(y); (x).lo=(z); })

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
#define D_IS(x,y,z) ({ Double_Store _d; _d.cells.high=(y); _d.cells.low =(z); (x)=_d.d; })

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

#define DOES_CODE1(cfa)	 ((Xt *)(cfa[1]))
#define DOES_CODEXT(cfa) ((Xt)(cfa[1]))

/* MAKE_CF creates an appropriate code field at the cfa;
   ca is the code address */
#define VTLINK 0
#define VTCOMPILE 1
#define VTLIT 2
#define VTEXTRA 3
#define VTTO 4
#define EXTRA_CODE(cfa) ((Xt *)(((Cell **)cfa)[-1][VTEXTRA]))
#define MAKE_CF(cfa,ca) ((*(Label *)(cfa)) = ((Label)ca))
/* make a code field for a defining-word-defined word */

#define CF(const)	(-const-2)

#define CF_NIL	-1

#ifndef FLUSH_ICACHE
#warning flush-icache probably will not work (see manual)
#	define FLUSH_ICACHE(addr,size)
#warning no FLUSH_ICACHE, turning off dynamic native code by default
#undef NO_DYNAMIC_DEFAULT
#define NO_DYNAMIC_DEFAULT 1
#endif

#ifndef CHECK_PRIM
#define CHECK_PRIM(start,len) 0
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
  Cell next_task;
  Cell prev_task;
  Cell save_task;
  Cell* sp0;
  Cell* rp0;
  Float* fp0;
  Address lp0;
  Xt *throw_entry;
} user_area;

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
  Xt *quit_entry;
  Xt *execute_entry;
  Xt *find_entry;
  Label *xt_base;         /* base of DOUBLE_INDIRECT xts[], for comp-i.fs */
  Label *label_base;      /* base of DOUBLE_INDIRECT labels[], for comp-i.fs */
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

#ifdef OLD_HEADER
# define LONGNAME_COUNT(np)	((np)->countetc & (((~((UCell)0))<<4)>>4))
# define LONGNAME_NAME(np)      ((Char *)((np)->name))
# define LONGNAME_NEXT(np)      ((np)->next)
#else
# define LONGNAME_COUNT(np)     ((((Cell*)np)[-3]) & (((~((UCell)0))<<4)>>4))
# define LONGNAME_NAME(np)      ((Char *)(np)-3*sizeof(Cell)-LONGNAME_COUNT(np))
# define LONGNAME_NEXT(np)      ((struct Longname*)(((Cell*)np)[-2]))
#endif

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

typedef struct _hash128 {
  uint64_t a;
  uint64_t b;
} hash128;

typedef struct {
  Cell magic;
  Cell *spx;
  Cell *rpx;
  Address lpx;
  Float *fpx;
  user_area* upx;
  Xt *s_ip;
  Cell *s_rp;
  jmp_buf * throw_jumpptr;
} stackpointers;

extern PER_THREAD stackpointers gforth_SPs;

#define IOR(flag)	((flag)? -512-errno : 0)
#define sr_proto , stackpointers *in_SPs
#define sr_call  , &gforth_SPs

Label *gforth_engine(Xt *ip sr_proto);
Label *gforth_engine2(Xt *ip sr_proto);
Label *gforth_engine3(Xt *ip sr_proto);

Cell gforth_main(int argc, char **argv, char **env);
void gforth_args(int argc, char ** argv, char ** path, char ** imagename);
ImageHeader* gforth_loader(char* imagename, char* path);
user_area* gforth_stacks(Cell dsize, Cell rsize, Cell fsize, Cell lsize);
void gforth_free_stacks(user_area* t);
void gforth_free();
Cell gforth_go(Xt* ip0);
Cell gforth_boot(int argc, char** argv, char* path);
Cell gforth_start(int argc, char ** argv);
Cell gforth_quit();
Xt gforth_find(Char * name);
Cell gforth_execute(Xt xt);
void gforth_cleanup();
void gforth_printmetrics();
#if defined(DOUBLY_INDIRECT)
Cell gforth_make_image(int debugflag);
#endif
#ifdef HAVE_MCHECK
void gforth_abortmcheck(enum mcheck_status reason);
#endif
void gforth_free(void * ptr);
void gforth_sigset(sigset_t *set, ...);

#if SIZEOF_CHAR_P == 4
#define GFORTH_MAGIC 0x3B3C51D5U
#else
#define GFORTH_MAGIC 0x1E059AF1785E72D4ULL
#endif

/* for ABI-CODE and ;ABI-CODE */
typedef Cell *abifunc(Cell *sp, Float **fpp);
typedef Cell *semiabifunc(Cell *sp, Float **fpp, Address body);

/* engine/prim support routines */
Address gforth_alloc(Cell size);
char *cstr(Char *from, UCell size);
char *tilde_cstr(Char *from, UCell size);
Cell opencreate_file(char *s, Cell wfam, int flags, Cell *wiorp);
DCell timeval2us(struct timeval *tvp);
DCell timespec2ns(struct timespec *tvp);
void cmove(Char *c_from, Char *c_to, UCell u);
void cmove_up(Char *c_from, Char *c_to, UCell u);
Cell compare(Char *c_addr1, UCell u1, Char *c_addr2, UCell u2);
struct Longname *listlfind(Char *c_addr, UCell u, struct Longname *longname1);
struct Longname *hashlfind(Char *c_addr, UCell u, Cell *a_addr);
struct Longname *tablelfind(Char *c_addr, UCell u, Cell *a_addr);
UCell hashkey1(Char *c_addr, UCell u, UCell ubits);
void hashkey2(Char *c_addr, UCell u, uint64_t upmask, hash128 * h);
UCell hashkey2a(Char *s, UCell n);
struct Cellpair parse_white(Char *c_addr1, UCell u1);
Cell rename_file(Char *c_addr1, UCell u1, Char *c_addr2, UCell u2);
struct Cellquad read_line(Char *c_addr, UCell u1, FILE *wfileid);
struct Cellpair file_status(Char *c_addr, UCell u);
Cell to_float(Char *c_addr, UCell u, Float *r_p, Char dot);
Float v_star(Float *f_addr1, Cell nstride1, Float *f_addr2, Cell nstride2, UCell ucount);
void faxpy(Float ra, Float *f_x, Cell nstridex, Float *f_y, Cell nstridey, UCell ucount);
UCell lshift(UCell u1, UCell n);
UCell rshift(UCell u1, UCell n);
int gforth_system(Char *c_addr, UCell u);
void gforth_ms(UCell u);
UCell gforth_dlopen(Char *c_addr, UCell u);
void gforth_dlclose(UCell lib);
Cell capscompare(Char *c_addr1, UCell u1, Char *c_addr2, UCell u2);
int gf_ungetc(int c, FILE *stream);
void gf_regetc(FILE *stream);
int gf_ungottenc(FILE *stream);

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
extern Label *labels;
extern Cell npriminfos;

#ifdef HAS_DEBUG
extern int debug, debug_mcheck;
#else
# define debug 0
# define debug_mcheck 0
#endif

#define gforth_SP (gforth_SPs.spx)
#define gforth_RP (gforth_SPs.rpx)
#define gforth_LP (gforth_SPs.lpx)
#define gforth_FP (gforth_SPs.fpx)
#define gforth_UP (gforth_SPs.upx)
#define gforth_magic (gforth_SPs.magic)
#define saved_ip (gforth_SPs.s_ip)
#define saved_rp (gforth_SPs.s_rp)
#define throw_jmp_handler (gforth_SPs.throw_jumpptr)

extern user_area* gforth_main_UP;

extern Cell const * gforth_pointers(Cell n);

#ifdef HAS_FFCALL
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
#ifndef HAVE_SINCOS
extern void sincos(double x, double *s, double *c);
#endif
#ifndef HAVE_ECVT_R
extern int ecvt_r(double x, int ndigits, int* exp, int* sign, char *buf, size_t len);
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
extern Cell negate(Cell n);

// For systems where you need it
void zexpand(char * zfile);

#define GROUP(x, n)
#define GROUPADD(n)

#ifdef HAVE_ENDIAN_H
#include <endian.h>
#else
#define BSWAP16(x) ((((x) >> 8) & 0xff | (((x) & 0xff) << 8)))
#define BSWAP32(x) ((BSWAP16((x) >> 16) | (BSWAP16(x) << 16)))
#define BSWAP64(x) ((BSWAP32((x) >> 32) | (BSWAP32(x) << 32)))
#ifdef WORDS_BIGENDIAN
#define htobe16(x) (x)
#define htobe32(x) (x)
#define htobe64(x) (x)
#define be16toh(x) (x)
#define be32toh(x) (x)
#define be64toh(x) (x)
#define htole16(x) BSWAP16(x)
#define htole32(x) BSWAP32(x)
#define htole64(x) BSWAP64(x)
#define le16toh(x) BSWAP16(x)
#define le32toh(x) BSWAP32(x)
#define le64toh(x) BSWAP64(x)
#else
#define htobe16(x) BSWAP16(x)
#define htobe32(x) BSWAP32(x)
#define htobe64(x) BSWAP64(x)
#define be16toh(x) BSWAP16(x)
#define be32toh(x) BSWAP32(x)
#define be64toh(x) BSWAP64(x)
#define htole16(x) (x)
#define htole32(x) (x)
#define htole64(x) (x)
#define le16toh(x) (x)
#define le32toh(x) (x)
#define le64toh(x) (x)
#endif
#endif
