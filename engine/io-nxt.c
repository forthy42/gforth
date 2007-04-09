/* direct key io driver for NXT brick

  Copyright (C) 2007 Free Software Foundation, Inc.

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

  The following is stolen from the readline library for bash
*/

#include "config.h"
#include "forth.h"
#include "../arch/arm/nxt/AT91SAM7.h"

long key_avail ()
{
  return 0;
}

Cell getkey()
{
  return 0;
}

int terminal_prepped = 0;

void prep_terminal ()
{
  terminal_prepped = 1;
}

void deprep_terminal ()
{
  terminal_prepped = 0;
}

void emit_char(char x)
{
}

void type_chars(char *addr, unsigned int l)
{
}
