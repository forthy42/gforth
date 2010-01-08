/* serial IO for the beagle board

  Copyright (C) 2010 Free Software Foundation, Inc.

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

/* This file is a stub for now */

int terminal_prepped = 0;

void prep_terminal ()
{
  serial_init();

  terminal_prepped = 1;
}

void deprep_terminal ()
{
  terminal_prepped = 0;
}

long key_avail ()
{
  if(!terminal_prepped) prep_terminal();

  return serial_tstc();
}

long getkey()
{
  if(!terminal_prepped) prep_terminal();

  return key_avail() ? serial_getc() : 0;
}

void emit_char(char x)
{
  if(!terminal_prepped) prep_terminal();

  serial_putc(x);
}

void type_chars(char *addr, unsigned int l)
{
  if(!terminal_prepped) prep_terminal();

  while(l--) serial_putc(*addr++);
}

/* Stubs for interrupts */

void do_undefined_instruction()
{
}

void do_software_interrupt()
{
}

void do_prefetch_abort()
{
}

void do_data_abort()
{
}

void do_not_used()
{
}

void do_irq()
{
}

void do_fiq()
{
}

#define ADDRLEN(x) x, strlen(x)

void start_armboot()
{
  char **argv = { "gforth-ec", 0L };

  // type_chars(ADDRLEN("Welcome to Gforth on Beagle Board\n"));
  main(1, argv, 0L);
}

void lowlevel_init()
{
}

void cpy_clk_code()
{
}

void *memset(char *s, int c, unsigned int n)
{
  while(n--) *s++ = c;
  return s;
}
