/* header file for libcc-generated C code

  Authors: Anton Ertl, Bernd Paysan
  Copyright (C) 2006,2007,2008,2012,2013,2014,2015,2016,2017,2019,2020,2023,2024,2025 Free Software Foundation, Inc.

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
  along with this program. If not, see http://www.gnu.org/licenses/.
*/

#ifdef  __cplusplus
extern "C" {
#endif

#include "config.h"
#include <stddef.h>
#include <signal.h>
#include <setjmp.h>
#include <string.h>
#include <stdlib.h>
#if defined (HAVE_ALLOCA_H)
# include <alloca.h>
#endif
#include <wchar.h>

#if defined(_WIN32) || defined(__WIN32__) || defined(__CYGWIN__) || defined(__ANDROID__)
#undef HAS_BACKLINK
#endif

typedef CELL_TYPE Cell;
typedef unsigned CELL_TYPE UCell;
typedef unsigned char Char;
typedef double Float;
typedef Char *Address;
typedef void *Label;
typedef Label *Xt;

#define Clongest long long
typedef unsigned Clongest UClongest;

typedef struct {
  Address base;		/* base address of image (0 if relocatable) */
  UCell dict_size;
  Address image_dp;	/* all sizes in bytes */
  Address sect_name;
  Address sect_locs;
  Address sect_primbits;
  Address sect_targets;
  Address sect_codestart;
  Address sect_last_header;
  UCell data_stack_size;
  UCell fp_stack_size;
  UCell return_stack_size;
  UCell locals_stack_size;
  Xt *boot_entry;	/* initial ip for booting (in BOOT) */
  Xt *quit_entry;
  Xt *execute_entry;
  Xt *find_entry;
  Label *xt_base;         /* base of DOUBLE_INDIRECT xts[], for comp-i.fs */
  Label *label_base;      /* base of DOUBLE_INDIRECT labels[], for comp-i.fs */
} ImageHeader;
/* the image-header is created in main.fs */

typedef struct {
  Cell next_task;
  Cell prev_task;
  Cell save_task;
  Cell* sp0;
  Cell* rp0;
  Float* fp0;
  Address lp0;
  Xt *throw_entry;
  ImageHeader *current_section;
} user_area;

typedef struct {
  Cell* spx;
  Float* fpx;
} gforth_stackpointers;

#define ARGN(s, f) int MAYBE_UNUSED arg0=s, farg0=f

typedef struct {
  Cell magic;
  Cell *handler;
  Cell first_throw;
  Cell *wraphandler; /* experimental */
  jmp_buf * throw_jumpptr;
  Cell *spx;
  Cell *rpx;
  Address lpx;
  Float *fpx;
  user_area* upx;
  Cell *s_ip;
  Cell *s_rp;
} stackpointers;

#ifdef HAS_BACKLINK
#define gforth_magic (gforth_SPs.magic)
#define gforth_SP (gforth_SPs.spx)
#define gforth_RP (gforth_SPs.rpx)
#define gforth_LP (gforth_SPs.lpx)
#define gforth_FP (gforth_SPs.fpx)
#define gforth_UP (gforth_SPs.upx)
#define saved_ip (gforth_SPs.s_ip)
#define saved_rp (gforth_SPs.s_rp)
#define throw_jmp_handler (gforth_SPs.throw_jumpptr)

extern PER_THREAD stackpointers gforth_SPs;
#define get_gforth_SPs() (&gforth_SPs)
#define sr_call , get_gforth_SPs()
extern void *gforth_engine(Xt *, stackpointers *);
extern char *cstr(char *from, Cell size);
extern char *tilde_cstr(char *from, Cell size);
extern user_area* gforth_stacks(Cell dsize, Cell rsize, Cell fsize, Cell lsize);
extern void gforth_free_stacks(user_area* t);
extern user_area *gforth_main_UP;
extern Cell gforth_go(Xt *ip);
extern void gforth_sigset(sigset_t* set, ...);
extern void gforth_setstacks(user_area*);
extern void gforth_fail();
  /* #define GFORTH_ARGS gforth_stackpointers x, void* cdesc */
#define GFORTH_ARGS gforth_stackpointers x
gforth_stackpointers gforth_libcc_init(GFORTH_ARGS)
{
  x.spx++;
  return x;
}
#else
#define gforth_SPs ((stackpointers *)(gforth_pointers(0)))
#define get_gforth_SPs() ((stackpointers *)(gforth_pointers(0)))
#define sr_call , get_gforth_SPs()

#define gforth_magic (gforth_SPs->magic)
#define gforth_SP (gforth_SPs->spx)
#define gforth_RP (gforth_SPs->rpx)
#define gforth_LP (gforth_SPs->lpx)
#define gforth_FP (gforth_SPs->fpx)
#define gforth_UP (gforth_SPs->upx)
#define saved_ip (gforth_SPs->s_ip)
#define saved_rp (gforth_SPs->s_rp)
#define throw_jmp_handler (gforth_SPs->throw_jumpptr)

#define gforth_engine ((char *(*)(Xt*, stackpointers *))gforth_pointers(1))
#define cstr ((char *(*)(char *, Cell))gforth_pointers(2))
#define tilde_cstr ((char *(*)(char *, Cell))gforth_pointers(3))
#define gforth_stacks ((user_area *(*)(Cell, Cell, Cell, Cell))gforth_pointers(4))
#define gforth_free_stacks ((void(*)(user_area*))gforth_pointers(5))
#define gforth_main_UP *((user_area **)(gforth_pointers(6)))
#define gforth_go ((Cell(*)(Xt*))gforth_pointers(7))
#define gforth_sigset ((void(*)(sigset_t*, ...))gforth_pointers(8))
#define gforth_setstacks ((void(*)(user_area*))gforth_pointers(9))
#define GFORTH_ARGS gforth_stackpointers x

static Cell *(*gforth_pointers)(Cell);
gforth_stackpointers gforth_libcc_init(GFORTH_ARGS)
{
  gforth_pointers=(Cell *(*)(Cell))*x.spx++;
  return x;
}
#endif

#if SIZEOF_CHAR_P == 4
#define GFORTH_MAGIC 0x3B3C51D5U
#else
#define GFORTH_MAGIC 0x1E059AF1785E72D4ULL
#endif

#define CELL_BITS	(sizeof(Cell) * 8)

#define gforth_d2ll(lo,hi) \
  (Clongest)((sizeof(Cell) < sizeof(Clongest))		\
   ? (((UClongest)(lo))|(((UClongest)(hi))<<CELL_BITS)) \
   : (lo))

#define gforth_ud2ll(lo,hi) \
  (UClongest)((sizeof(Cell) < sizeof(Clongest))		\
   ? (((UClongest)(lo))|(((UClongest)(hi))<<CELL_BITS)) \
   : (lo))

#define gforth_ll2d(ll,lo,hi) \
  do { \
    Clongest _ll = (ll); \
    (lo) = (Cell)_ll; \
    (hi) = ((sizeof(Cell) < sizeof(Clongest)) \
            ? (_ll >> CELL_BITS) \
            : 0); \
  } while (0);

#define c_str2gforth_str(str,addr,u) \
    (addr) = (Cell) str; \
    (u) = (addr) ? strlen((char*)(addr)) : 0;

#define gforth_ll2ud(ll,lo,hi) \
  do { \
    UClongest _ll = (ll); \
    (lo) = (Cell)_ll; \
    (hi) = ((sizeof(Cell) < sizeof(Clongest)) \
            ? (_ll >> CELL_BITS) \
            : 0); \
  } while (0);

static __thread int gforth_strs_i;
static __thread void* gforth_strs[0x10];

#define ROLLSTR(type,size)					   \
  type * str= malloc(sizeof(type)*(size));			   \
  gforth_strs_i++; gforth_strs_i &= 0x0F; 			   \
  if(gforth_strs[gforth_strs_i]) free(gforth_strs[gforth_strs_i]); \
  gforth_strs[gforth_strs_i]=(void*)str;

static __attribute__((unused)) char * gforth_str2c(Char* addr, UCell u)
{
  if(addr == NULL) {
    return (char*)u; // pass direct values
  } else {
    ROLLSTR(char, u+1);
    memmove(str, addr, u);
    str[u]='\0'; // add zero terminator
    return str;
  }
}

static __attribute__((unused)) wchar_t * gforth_str2wc(Char* addr, UCell u)
{
  if(addr == NULL) {
    return (wchar_t*)u;
  } else {
    UCell i=0, c;
    Char x, y, z; // for further parts of the UTF-8 char
    Char* end=addr+u;
    ROLLSTR(wchar_t,u+1);
    while(addr<end) {
      switch((c=*addr++)) {
      case 0xC2 ... 0xDF: x = *addr++;
	c = ((c & 0x1F) << 6) | (x & 0x3F);
	// fprintf(stderr, "2c[%d]: %03x\n", i, c);
	break;
      case 0xE0 ... 0xEF: x = *addr++; y = *addr++;
	c = ((c & 0x0F) << 12) | ((x & 0x3F) << 6) | (y & 0x3F);
	// fprintf(stderr, "3c[%d]: %04x\n", i, c);
	break;
      case 0xF0 ... 0xF7: x = *addr++; y = *addr++; z = *addr++;
	c = ((c & 0x07) << 18) | ((x & 0x3F) << 12) | ((y & 0x3F) << 6) | (z & 0x3F);
	// fprintf(stderr, "4c[%d]: %05x\n", i, c);
	if(sizeof(wchar_t) < 4) {
	  /* this is surrogate territory; if the UTF-8 is well-formed,
	   * it's a surrogate in UTF-16 */
	  c -= 0x10000;
	  str[i++] = 0xD800 | ((c >> 10) & 0x3FF);
	  c = 0xDC00 | (c & 0x3FF);
	} break;
      default:
	// fprintf(stderr, "1c[%d]: %02x\n", i, c);
	break; // ASCII or invalid character
      }
      str[i++] = c;
    }
    str[i]=0; // add zero terminator
    return str;
  }
}

static __attribute__((unused)) void wc_str2gforth_str(wchar_t* wstring, Char ** addr, UCell* u)
{
  wchar_t wc;
  wchar_t *wstring_count=wstring;
  Char *strend;
  UCell surrogate=0;
  if(wstring == NULL) {
    *u=0;
    *addr=0;
    return;
  }
  ROLLSTR(Char,(wcslen(wstring)*4));
  strend=str;
  for(;;) {
    switch((wc=*wstring_count++)) {
    case 0x0000: break;
    case 0x0001 ... 0x007F:
      *strend++=(Char)wc; continue;
    case 0x0080 ... 0x07FF:
      *strend++=0xC0 | (Char)(wc >> 6);
      *strend++=0x80 | (Char)(wc & 0x3F);
      continue;
    case 0x0800 ... 0xD7FF:
    case 0xE000 ... 0xFFFF:
      *strend++=0xE0 | (Char)(wc >> 12);
      *strend++=0x80 | (Char)((wc >> 6) & 0x3F);
      *strend++=0x80 | (Char)(wc & 0x3F);
      continue;
    case 0xD800 ... 0xDBFF:
      surrogate = (wc & 0x3FF) << 10;
      continue;
    case 0xDC00 ... 0xDFFF:
      wc = (surrogate | (wc & 0x3FF)) + 0x10000;
      surrogate = 0; // surrogate pair=4 bytes, fall through
    case 0x10000 ... 0x10FFFF:
      *strend++=0xF0 | (Char)((wc >> 18) & 0x7);
      *strend++=0x80 | (Char)((wc >> 12) & 0x3F);
      *strend++=0x80 | (Char)((wc >> 6) & 0x3F);
      *strend++=0x80 | (Char)(wc & 0x3F);
      continue;
    default: // undefined code point 0xFFFD
      *strend++=0xEF;
      *strend++=0xBF;
      *strend++=0xBD;
      continue;
    }
    break;
  }
  *u=(UCell)(strend-str);
  *addr=str;
}

typedef Char hash_128[16];

#define GFSS 0x80 /* stack sizes */

#ifdef  __cplusplus
}
#endif
