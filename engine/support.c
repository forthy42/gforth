/* Gforth support functions

  Copyright (C) 1995,1996,1997,1998,2000 Free Software Foundation, Inc.

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
#include <stdlib.h>
#include <string.h>
#include <sys/time.h>
#include <unistd.h>
#include <pwd.h>
#include <dirent.h>

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
      s1 = "";
    s2 = from+1;
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
    s2 = from+i;
    s2_len = size-i;
  }
  s1_len = strlen(s1);
  if (s1_len>1 && s1[s1_len-1]=='/')
    s1_len--;
  {
    char path[s1_len+s2_len];
    memcpy(path,s1,s1_len);
    memcpy(path+s1_len,s2,s2_len);
    return cstr(path,s1_len+s2_len,clear);
  }
}
#endif

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

Xt *primtable(Label symbols[], Cell size)
     /* used in primitive primtable for peephole optimization */
{
  Xt *xts = (Xt *)malloc(size*sizeof(Xt));
  Cell i;

  for (i=0; i<size; i++)
    xts[i] = &symbols[i];
  return xts;
}
