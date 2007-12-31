/* Input driver header

  Copyright (C) 1995,1996,1997,1998,1999,2003,2007 Free Software Foundation, Inc.

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

#include <setjmp.h>

extern jmp_buf throw_jmp_buf;

#if defined(MSDOS) || defined(SHARC)
#  define prep_terminal()
#  define deprep_terminal()
#  include <conio.h>

#  define key(stdin)		getch()
#  define key_query(stdin)	FLAG(kbhit())
#else
Cell getkey(FILE *);
long key_avail(FILE *);
void prep_terminal(void);
void deprep_terminal(void);
void get_winsize(void);

#  define key(stdin)		getkey(stdin)
#  define key_query(stdin)	-(!!key_avail(stdin)) /* !! FLAG(...)? - anton */
         		/* flag was originally wrong -- lennart */
#endif

extern UCell rows, cols;
extern int terminal_prepped;
