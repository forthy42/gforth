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
#include "../arch/arm/nxt/bt.h"
#include "../arch/arm/nxt/display.h"
#include "../arch/arm/nxt/aic.h"
#include "../arch/arm/nxt/systick.h"
#include "../arch/arm/nxt/sound.h"
#include "../arch/arm/nxt/interrupts.h"
#include "../arch/arm/nxt/nxt_avr.h"
#include "../arch/arm/nxt/nxt_motors.h"
#include "../arch/arm/nxt/i2c.h"

int terminal_prepped = 0;

void
show_splash(U32 milliseconds)
{
  display_clear(0);
  display_goto_xy(6, 6);
  display_string("Gforth");
  display_update();

  systick_wait_ms(milliseconds);
}

void prep_terminal ()
{
  aic_initialise();
  interrupts_enable();
  systick_init();
  sound_init();
  nxt_avr_init();
  display_init();
  nxt_motor_init();
  i2c_init();
  bt_init();
  display_goto_xy(0,0);
  display_clear(1);

  terminal_prepped = 1;
}

void deprep_terminal ()
{
  terminal_prepped = 0;
}

long key_avail ()
{
  if(!terminal_prepped) prep_terminal();
  return bt_avail();
}

Cell getkey()
{
  if(!terminal_prepped) prep_terminal();
  return bt_getkey();
}

void emit_char(char x)
{
  if(!terminal_prepped) prep_terminal();
  display_char(x);
  display_update();
  bt_send(&x, 1);
}

void type_chars(char *addr, unsigned int l)
{
  int i;
  for(i=0; i<l; i++)
    emit_char(addr[i]);
}
