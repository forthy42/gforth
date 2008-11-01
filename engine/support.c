/* Gforth support functions

  Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2006,2007,2008 Free Software Foundation, Inc.

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
#include "forth.h"
#include "io.h"
#include <stdlib.h>
#include <string.h>
#include <sys/time.h>
#include <unistd.h>
#include <pwd.h>
#include <assert.h>
#ifndef STANDALONE
#include <dirent.h>
#include <math.h>
#include <ctype.h>
#include <errno.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <time.h>
#endif
#if defined(HAVE_LIBDL) || defined(HAVE_DLOPEN) /* what else? */
#include <dlfcn.h>
#endif

#ifdef HAS_FILE
char *cstr(Char *from, UCell size, int clear)
/* return a C-string corresponding to the Forth string ( FROM SIZE ).
   the C-string lives until the next call of cstr with CLEAR being true */
{
  static struct cstr_buffer {
    char *buffer;
    size_t size;
  } *buffers=NULL;
  static int nbuffers=0;
  static int used=0;
  struct cstr_buffer *b;

  if (buffers==NULL)
    buffers=malloc(0);
  if (clear)
    used=0;
  if (used>=nbuffers) {
    buffers=realloc(buffers,sizeof(struct cstr_buffer)*(used+1));
    buffers[used]=(struct cstr_buffer){malloc(0),0};
    nbuffers=used+1;
  }
  b=&buffers[used];
  if (size+1 > b->size) {
    b->buffer = realloc(b->buffer,size+1);
    b->size = size+1;
  }
  memcpy(b->buffer,from,size);
  b->buffer[size]='\0';
  used++;
  return b->buffer;
}

char *tilde_cstr(Char *from, UCell size, int clear)
/* like cstr(), but perform tilde expansion on the string */
{
  char *s1,*s2;
  int s1_len, s2_len;
  struct passwd *getpwnam (), *user_entry;

  if (size<1 || from[0]!='~')
    return cstr(from, size, clear);
  if (size<2 || from[1]=='/') {
    s1 = (char *)getenv ("HOME");
    if(s1 == NULL)
#if defined(_WIN32) || defined (MSDOS)
      s1 = (char *)getenv ("TEMP");
      if(s1 == NULL)
         s1 = (char *)getenv ("TMP");
         if(s1 == NULL)
#endif
      s1 = "";
    s2 = (char *)from+1;
    s2_len = size-1;
  } else {
    UCell i;
    for (i=1; i<size && from[i]!='/'; i++)
      ;
    if (i==2 && from[1]=='+') /* deal with "~+", i.e., the wd */
      return cstr(from+3, size<3?0:size-3,clear);
    {
      char user[i];
      memcpy(user,from+1,i-1);
      user[i-1]='\0';
      user_entry=getpwnam(user);
    }
    if (user_entry==NULL)
      return cstr(from, size, clear);
    s1 = user_entry->pw_dir;
    s2 = (char *)from+i;
    s2_len = size-i;
  }
  s1_len = strlen(s1);
  if (s1_len>1 && s1[s1_len-1]=='/')
    s1_len--;
  {
    char path[s1_len+s2_len];
    memcpy(path,s1,s1_len);
    memcpy(path+s1_len,s2,s2_len);
    return cstr((Char *)path,s1_len+s2_len,clear);
  }
}

Cell opencreate_file(char *s, Cell wfam, int flags, Cell *wiorp)
{
  Cell fd;
  Cell wfileid;
  fd = open(s, flags|ufileattr[wfam], 0666);
  if (fd != -1) {
    wfileid = (Cell)fdopen(fd, fileattr[wfam]);
    *wiorp = IOR(wfileid == 0);
  } else {
    wfileid = 0;
    *wiorp = IOR(1);
  }
  return wfileid;
}
#endif /* defined(HAS_FILE) */

DCell timeval2us(struct timeval *tvp)
{
#ifndef BUGGY_LONG_LONG
  return (tvp->tv_sec*(DCell)1000000)+tvp->tv_usec;
#else
  DCell d2;
  DCell d1=mmul(tvp->tv_sec,1000000);
  d2.lo = d1.lo+tvp->tv_usec;
  d2.hi = d1.hi + (d2.lo<d1.lo);
  return d2;
#endif
}

DCell double2ll(Float r)
{
#ifndef BUGGY_LONG_LONG
  return (DCell)(r);
#else
  double ldexp(double x, int exp);
  DCell d;
  if (r<0) {
    d.hi = ldexp(-r,-(int)(CELL_BITS));
    d.lo = (-r)-ldexp((Float)d.hi,CELL_BITS);
    return dnegate(d);
  }
  d.hi = ldexp(r,-(int)(CELL_BITS));
  d.lo = r-ldexp((Float)d.hi,CELL_BITS);
  return d;
#endif
}

void cmove(Char *c_from, Char *c_to, UCell u)
{
  while (u-- > 0)
    *c_to++ = *c_from++;
}

void cmove_up(Char *c_from, Char *c_to, UCell u)
{
  while (u-- > 0)
    c_to[u] = c_from[u];
}

Cell compare(Char *c_addr1, UCell u1, Char *c_addr2, UCell u2)
{
  Cell n;

  n = memcmp(c_addr1, c_addr2, u1<u2 ? u1 : u2);
  if (n==0)
    n = u1-u2;
  if (n<0)
    n = -1;
  else if (n>0)
    n = 1;
  return n;
}

Cell memcasecmp(const Char *s1, const Char *s2, Cell n)
{
  Cell i;

  for (i=0; i<n; i++) {
    Char c1=toupper(s1[i]);
    Char c2=toupper(s2[i]);
    if (c1 != c2) {
      if (c1 < c2)
	return -1;
      else
	return 1;
    }
  }
  return 0;
}

Cell capscompare(Char *c_addr1, UCell u1, Char *c_addr2, UCell u2)
{
  Cell n;

  n = memcasecmp(c_addr1, c_addr2, u1<u2 ? u1 : u2);
  if (n==0)
    n = u1-u2;
  if (n<0)
    n = -1;
  else if (n>0)
    n = 1;
  return n;
}

struct Longname *listlfind(Char *c_addr, UCell u, struct Longname *longname1)
{
  for (; longname1 != NULL; longname1 = (struct Longname *)(longname1->next))
    if ((UCell)LONGNAME_COUNT(longname1)==u &&
	memcasecmp(c_addr, (Char *)(longname1->name), u)== 0 /* or inline? */)
      break;
  return longname1;
}

struct Longname *hashlfind(Char *c_addr, UCell u, Cell *a_addr)
{
  struct Longname *longname1;

  while(a_addr != NULL) {
    longname1=(struct Longname *)(a_addr[1]);
    a_addr=(Cell *)(a_addr[0]);
    if ((UCell)LONGNAME_COUNT(longname1)==u &&
	memcasecmp(c_addr, (Char *)(longname1->name), u)== 0 /* or inline? */) {
      return longname1;
    }
  }
  return NULL;
}

struct Longname *tablelfind(Char *c_addr, UCell u, Cell *a_addr)
{
  struct Longname *longname1;
  while(a_addr != NULL) {
    longname1=(struct Longname *)(a_addr[1]);
    a_addr=(Cell *)(a_addr[0]);
    if ((UCell)LONGNAME_COUNT(longname1)==u &&
	memcmp(c_addr, longname1->name, u)== 0 /* or inline? */) {
      return longname1;
    }
  }
  return NULL;
}

UCell hashkey1(Char *c_addr, UCell u, UCell ubits)
/* this hash function rotates the key at every step by rot bits within
   ubits bits and xors it with the character. This function does ok in
   the chi-sqare-test.  Rot should be <=7 (preferably <=5) for
   ASCII strings (larger if ubits is large), and should share no
   divisors with ubits.
*/
{
  static char rot_values[] = {5,0,1,2,3,4,5,5,5,5,3,5,5,5,5,7,5,5,5,5,7,5,5,5,5,6,5,5,5,5,7,5,5};
  unsigned rot = rot_values[ubits];
  Char *cp = c_addr;
  UCell ukey;

  for (ukey=0; cp<c_addr+u; cp++)
    ukey = ((((ukey<<rot) | (ukey>>(ubits-rot))) 
	     ^ toupper(*cp))
	    & ((1<<ubits)-1));
  return ukey;
}

struct Cellpair parse_white(Char *c_addr1, UCell u1)
{
  /* use !isgraph instead of isspace? */
  struct Cellpair result;
  Char *c_addr2;
  Char *endp = c_addr1+u1;
  while (c_addr1<endp && isspace(*c_addr1))
    c_addr1++;
  if (c_addr1<endp) {
    for (c_addr2 = c_addr1; c_addr1<endp && !isspace(*c_addr1); c_addr1++)
      ;
    result.n1 = (Cell)c_addr2;
    result.n2 = c_addr1-c_addr2;
  } else {
    result.n1 = (Cell)c_addr1;
    result.n2 = 0;
  }
  return result;
}

#ifdef HAS_FILE
Cell rename_file(Char *c_addr1, UCell u1, Char *c_addr2, UCell u2)
{
  char *s1=tilde_cstr(c_addr2, u2, 1);
  return IOR(rename(tilde_cstr(c_addr1, u1, 0), s1)==-1);
}

struct Cellquad read_line(Char *c_addr, UCell u1, Cell wfileid)
{
  UCell u2, u3;
  Cell flag, wior;
  Cell c;
  struct Cellquad r;

  flag=-1;
  u3=0;
  for(u2=0; u2<u1; u2++) {
    c = getc((FILE *)wfileid);
    u3++;
    if (c=='\n') break;
    if (c=='\r') {
      if ((c = getc((FILE *)wfileid))!='\n')
	ungetc(c,(FILE *)wfileid);
      else
	u3++;
      break;
    }
    if (c==EOF) {
      flag=FLAG(u2!=0);
      break;
    }
    c_addr[u2] = (Char)c;
  }
  wior=FILEIO(ferror((FILE *)wfileid));
  r.n1 = u2;
  r.n2 = flag;
  r.n3 = u3;
  r.n4 = wior;
  return r;
}

struct Cellpair file_status(Char *c_addr, UCell u)
{
  struct Cellpair r;
  Cell wfam;
  Cell wior;
  char *filename=tilde_cstr(c_addr, u, 1);

  if (access (filename, F_OK) != 0) {
    wfam=0;
    wior=IOR(1);
  }
  else if (access (filename, R_OK | W_OK) == 0) {
    wfam=2; /* r/w */
    wior=0;
  }
  else if (access (filename, R_OK) == 0) {
    wfam=0; /* r/o */
    wior=0;
  }
  else if (access (filename, W_OK) == 0) {
    wfam=4; /* w/o */
    wior=0;
  }
  else {
    wfam=1; /* well, we cannot access the file, but better deliver a
	       legal access mode (r/o bin), so we get a decent error
	       later upon open. */
    wior=0;
  }
  r.n1 = wfam;
  r.n2 = wior;
  return r;
}

Cell to_float(Char *c_addr, UCell u, Float *rp)
{
  /* convertible string := <significand>[<exponent>]
     <significand> := [<sign>]{<digits>[.<digits0>] | .<digits> }
     <exponent>    := <marker><digits0>
     <marker>      := {<e-form> | <sign-form>}
     <e-form>      := <e-char>[<sign-form>]
     <sign-form>   := { + | - }
     <e-char>      := { D | d | E | e }
  */
  Char *s = c_addr;
  Char c;
  Char *send = c_addr+u;
  UCell ndigits = 0;
  UCell ndots = 0;
  UCell edigits = 0;
  char cnum[u+3]; /* append at most "e0\0" */
  char *t=cnum;
  char *endconv;
  Float r;
  
  if (s >= send) /* treat empty string as 0e */
    goto return0;
  switch ((c=*s)) {
  case ' ':
    /* "A string of blanks should be treated as a special case
       representing zero."*/
    for (s++; s<send; )
      if (*s++ != ' ')
        goto error;
    goto return0;
  case '-':
  case '+': *t++ = c; s++; goto aftersign;
  }
  aftersign: 
  if (s >= send)
    goto exponent;
  switch (c=*s) {
  case '0' ... '9': *t++ = c; ndigits++; s++; goto aftersign;
  case '.':         *t++ = c; ndots++;   s++; goto aftersign;
  default:                                    goto exponent;
  }
 exponent:
  if (ndigits < 1 || ndots > 1)
    goto error;
  *t++ = 'E';
  if (s >= send)
    goto done;
  switch (c=*s) {
  case 'D':
  case 'd':
  case 'E':
  case 'e': s++; break;
  }
  if (s >= send)
    goto done;
  switch (c=*s) {
  case '+':
  case '-': *t++ = c; s++; break;
  }
 edigits0:
  if (s >= send)
    goto done;
  switch (c=*s) {
  case '0' ... '9': *t++ = c; s++; edigits++; goto edigits0;
  default: goto error;
  }
 done:
  if (edigits == 0)
    *t++ = '0';
  *t++ = '\0';
  assert(t-cnum <= u+3);
  r = strtod(cnum, &endconv);
  assert(*endconv == '\0');
  *rp = r;
  return -1;
 return0:
  *rp = 0.0;
  return -1;
 error:
  *rp = 0.0;
  return 0;
}
#endif

#ifdef HAS_FLOATING
Float v_star(Float *f_addr1, Cell nstride1, Float *f_addr2, Cell nstride2, UCell ucount)
{
  Float r;

  for (r=0.; ucount>0; ucount--) {
    r += *f_addr1 * *f_addr2;
    f_addr1 = (Float *)(((Address)f_addr1)+nstride1);
    f_addr2 = (Float *)(((Address)f_addr2)+nstride2);
  }
  return r;
}

void faxpy(Float ra, Float *f_x, Cell nstridex, Float *f_y, Cell nstridey, UCell ucount)
{
  for (; ucount>0; ucount--) {
    *f_y += ra * *f_x;
    f_x = (Float *)(((Address)f_x)+nstridex);
    f_y = (Float *)(((Address)f_y)+nstridey);
  }
}
#endif

UCell lshift(UCell u1, UCell n)
{
  return u1 << n;
}

UCell rshift(UCell u1, UCell n)
{
  return u1 >> n;
}

#ifndef STANDALONE
int gforth_system(Char *c_addr, UCell u)
{
  int retval;
  char *prefix = getenv("GFORTHSYSTEMPREFIX") ? : DEFAULTSYSTEMPREFIX;
  size_t prefixlen = strlen(prefix);
  char buffer[prefixlen+u+1];
#ifndef MSDOS
  int old_tp=terminal_prepped;
  deprep_terminal();
#endif
  memcpy(buffer,prefix,prefixlen);
  memcpy(buffer+prefixlen,c_addr,u);
  buffer[prefixlen+u]='\0';
  retval=system(buffer); /* ~ expansion on first part of string? */
#ifndef MSDOS
  if (old_tp)
    prep_terminal();
#endif
  return retval;
}

void gforth_ms(UCell u)
{
#ifdef HAVE_NANOSLEEP
  struct timespec time_req;
  time_req.tv_sec=u/1000;
  time_req.tv_nsec=1000000*(u%1000);
  while(nanosleep(&time_req, &time_req));
#else /* !defined(HAVE_NANOSLEEP) */
  struct timeval timeout;
  timeout.tv_sec=u/1000;
  timeout.tv_usec=1000*(u%1000);
  (void)select(0,0,0,0,&timeout);
#endif /* !defined(HAVE_NANOSLEEP) */
}

UCell gforth_dlopen(Char *c_addr, UCell u)
{
  char * file=tilde_cstr(c_addr, u, 1);
  UCell lib;
#if defined(HAVE_LIBLTDL)
  lib = (UCell)lt_dlopen(file);
  if(lib) return lib;
#elif defined(HAVE_LIBDL) || defined(HAVE_DLOPEN)
#ifndef RTLD_GLOBAL
#define RTLD_GLOBAL 0
#endif
  lib = (UCell)dlopen(file, RTLD_GLOBAL | RTLD_LAZY);
  if(lib) return lib;
#elif defined(_WIN32)
  lib = (UCell) GetModuleHandle(file);
  if(lib) return lib;
#endif
  return 0;
}

#endif /* !defined(STANDALONE) */


/* mixed division support; should usually be faster than gcc's
   double-by-double division (and gcc typically does not generate
   double-by-single division because of exception handling issues. If
   the architecture has double-by-single division, you should define
   ASM_SM_SLASH_REM and ASM_UM_SLASH_MOD appropriately. */

/* Type definitions for longlong.h (according to the comments at the start):
   declarations taken from libgcc2.h */

typedef unsigned int UQItype	__attribute__ ((mode (QI)));
typedef 	 int SItype	__attribute__ ((mode (SI)));
typedef unsigned int USItype	__attribute__ ((mode (SI)));
typedef		 int DItype	__attribute__ ((mode (DI)));
typedef unsigned int UDItype	__attribute__ ((mode (DI)));
typedef UCell UWtype;
#if (SIZEOF_CHAR_P == 4)
typedef unsigned int UHWtype __attribute__ ((mode (HI)));
#endif
#if (SIZEOF_CHAR_P == 8)
typedef USItype UHWtype;
#endif
#ifndef BUGGY_LONG_LONG
typedef UDCell UDWtype;
#endif
#define W_TYPE_SIZE (SIZEOF_CHAR_P * 8)

#include "longlong.h"


#if defined(udiv_qrnnd) && !defined(__alpha) && UDIV_NEEDS_NORMALIZATION

#if defined(count_leading_zeros)
const UQItype __clz_tab[256] =
{
  0,1,2,2,3,3,3,3,4,4,4,4,4,4,4,4,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
  6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
  7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
  7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
  8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
  8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
  8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
  8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8
};
#endif

static Cell MAYBE_UNUSED nlz(UCell x)
     /* number of leading zeros, adapted from "Hacker's Delight" */
{
   Cell n;

#if !defined(COUNT_LEADING_ZEROS_0)
   if (x == 0) return(CELL_BITS);
#endif
#if defined(count_leading_zeros)
   count_leading_zeros(n,x);
#else
   n = 0;
#if (SIZEOF_CHAR_P > 4)
   if (x <= 0xffffffff) 
     n+=32;
   else
     x >>= 32;
#endif
   if (x <= 0x0000FFFF) {n = n +16; x = x <<16;}
   if (x <= 0x00FFFFFF) {n = n + 8; x = x << 8;}
   if (x <= 0x0FFFFFFF) {n = n + 4; x = x << 4;}
   if (x <= 0x3FFFFFFF) {n = n + 2; x = x << 2;}
   if (x <= 0x7FFFFFFF) {n = n + 1;}
#endif
   return n;
}
#endif /*defined(udiv_qrnnd) && !defined(__alpha) && UDIV_NEEDS_NORMALIZATION*/

#if !defined(ASM_UM_SLASH_MOD)
UDCell umdiv (UDCell u, UCell v)
/* Divide unsigned double by single precision using shifts and subtracts.
   Return quotient in lo, remainder in hi. */
{
  UDCell res;
#if defined(udiv_qrnnd) && !defined(__alpha)
#if 0
   This code is slower on an Alpha (timings with gcc-3.3.5):
          other     this
   */      5205 ms  5741 ms 
   */mod   5167 ms  5717 ms 
   fm/mod  5467 ms  5312 ms 
   sm/rem  4734 ms  5278 ms 
   um/mod  4490 ms  5020 ms 
   m*/    15557 ms 17151 ms
#endif /* 0 */
  UCell q,r,u0,u1;
  UCell MAYBE_UNUSED lz;
  
  vm_ud2twoCell(u,u0,u1);
  if (v==0)
    throw(BALL_DIVZERO);
  if (u1>=v)
    throw(BALL_RESULTRANGE);
#if UDIV_NEEDS_NORMALIZATION
  lz = nlz(v);
  v <<= lz;
  u = UDLSHIFT(u,lz);
  vm_ud2twoCell(u,u0,u1);
#endif
  udiv_qrnnd(q,r,u1,u0,v);
#if UDIV_NEEDS_NORMALIZATION
  r >>= lz;
#endif
  vm_twoCell2ud(q,r,res);
#else /* !(defined(udiv_qrnnd) && !defined(__alpha)) */
  /* simple restoring subtract-and-shift algorithm, might be faster on Alpha */
  int i = CELL_BITS, c = 0;
  UCell q = 0;
  UCell h, l;

  vm_ud2twoCell(u,l,h);
  if (v==0)
    throw(BALL_DIVZERO);
  if (h>=v)
    throw(BALL_RESULTRANGE);
  for (;;)
    {
      if (c || h >= v)
	{
	  q++;
	  h -= v;
	}
      if (--i < 0)
	break;
      c = HIGHBIT (h);
      h <<= 1;
      h += HIGHBIT (l);
      l <<= 1;
      q <<= 1;
    }
  vm_twoCell2ud(q,h,res);
#endif /* !(defined(udiv_qrnnd) && !defined(__alpha)) */
  return res;
}
#endif

#if !defined(ASM_SM_SLASH_REM)
#if  defined(ASM_UM_SLASH_MOD)
/* define it if it is not defined above */
static UDCell MAYBE_UNUSED umdiv (UDCell u, UCell v)
{
  UDCell res;
  UCell u0,u1;
  vm_ud2twoCell(u,u0,u1);
  ASM_UM_SLASH_MOD(u0,u1,v,r,q);
  vm_twoCell2ud(q,r,res);
  return res;
}
#endif /* defined(ASM_UM_SLASH_MOD) */

#ifndef BUGGY_LONG_LONG
#define dnegate(x) (-(x))
#endif

DCell smdiv (DCell num, Cell denom)
     /* symmetric divide procedure, mixed prec */
{
  DCell res;
#if defined(sdiv_qrnnd)
  /* #warning "using sdiv_qrnnd" */
  Cell u1,q,r
  UCell u0;
  UCell MAYBE_UNUSED lz;
  
  vm_d2twoCell(u,u0,u1);
  if (v==0)
    throw(BALL_DIVZERO);
  if (u1>=v)
    throw(BALL_RESULTRANGE);
  sdiv_qrnnd(q,r,u1,u0,v);
  vm_twoCell2d(q,r,res);
#else
  UDCell ures;
  UCell l, q, r;
  Cell h;
  Cell denomsign=denom;

  vm_d2twoCell(num,l,h);
  if (h < 0)
    num = dnegate (num);
  if (denomsign < 0)
    denom = -denom;
  ures = umdiv(D2UD(num), denom);
  vm_ud2twoCell(ures,q,r);
  if ((h^denomsign)<0) {
    q = -q;
    if (((Cell)q) > 0) /* note: == 0 is possible */
      throw(BALL_RESULTRANGE);
  } else {
    if (((Cell)q) < 0)
      throw(BALL_RESULTRANGE);
  }
  if (h<0)
    r = -r;
  vm_twoCell2d(q,r,res);
#endif
  return res;
}

DCell fmdiv (DCell num, Cell denom)
     /* floored divide procedure, mixed prec */
{
  /* I have this technique from Andrew Haley */
  DCell res;
  UDCell ures;
  Cell denomsign=denom;
  Cell numsign;
  UCell q,r;

  if (denom < 0) {
    denom = -denom;
    num = dnegate(num);
  }
  numsign = DHI(num);
  if (numsign < 0)
    DHI_IS(num,DHI(num)+denom);
  ures = umdiv(D2UD(num),denom);
  vm_ud2twoCell(ures,q,r);
  if ((numsign^((Cell)q)) < 0)
    throw(BALL_RESULTRANGE);
  if (denomsign<0)
    r = -r;
  vm_twoCell2d(q,r,res);
  return res;
}
#endif
