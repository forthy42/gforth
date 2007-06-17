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
int needs_update = 0;
int bt_mode = 0;
int bt_state = 0;

void
show_splash(U32 milliseconds)
{
  display_clear(0);
  display_goto_xy(4, 6);
  display_string("Gforth NXT");
  display_update();

  systick_wait_ms(milliseconds);
}

const static bt_lens[0x3C] = { 10, 3, 10, 3,  10, 30, 10, 3,  4, 4, 26, 4,  3, 0, 0, 0,
			       0, 0, 0, 0,    0, 0, 0, 0,     0, 0, 0, 0,   4, 4, 0, 0,
			       0, 19, 0, 4,   0, 3, 0, 3,     0, 3, 3, 3,   0, 0, 0, 3,
			       0, 0, 0, 3, 5, 0, 3, 4, 0,     3, 0, 3, 0 };

void bt_send_cmd(char * cmd)
{
  int len = bt_lens[cmd[1]];
  int i, sum=0;

  cmd[0] = len;
  for(i=1; i<len-2; i++)
    sum += cmd[i];
  sum = -sum;
  cmd[i++] = (char)(sum>>8);
  cmd[i++] = (char)(sum & 0xff);

  //  systick_wait_ms(500);

  bt_send(cmd, len+1);
}

int do_bluetooth ()
{
  if(!bt_mode) {
    char cmd[30];
    
    bt_receive(cmd);
    if(cmd[0] | cmd[1]) {
      display_char('0'+cmd[0]);
      display_char('0'+cmd[1]);
    }
    
    switch(cmd[1]) {
    case 0x16: // request connection
      display_char('-');
      cmd[1] = 0x9; // accept connection
      cmd[2] = 1; // yes, we do
      bt_send_cmd(cmd);
      break;
    case 0x0f: // inquiry result
      display_char('+');
      cmd[1] = 0x05;
      bt_send_cmd(cmd); // add devices as known device
      break;
    case 0x13: // connect result
      if(cmd[2]) {
	int n=0;
	int handle=cmd[3];
	display_char('/'); display_update();
	systick_wait_ms(300);
	bt_receive(cmd);
	if(cmd[0]==0) {
	  cmd[1] = 0xB; // open stream
	  cmd[2] = handle;
	  bt_send_cmd(cmd);
	  systick_wait_ms(100);
	  bt_set_arm7_cmd();
	  bt_mode = 1;
	  display_char(')'); display_update();
	}
	//	  bt_state = 1;
      } else {
	display_char('(');
      }
      break;
    case 0x20: // discoverableack
      if(cmd[2]) {
	display_char('?');
	break;
      }
    case 0x10:
    case 0x14:
      display_char('!');
      cmd[1] = 0x1C; cmd[2] = 1; bt_send_cmd(cmd);
      break;
    default:
      break;
    }
    display_update();
  }
  return 0;
}

void prep_terminal ()
{
  char cmd[30];

  aic_initialise();
  interrupts_enable();
  systick_init();
  sound_init();
  nxt_avr_init();
  display_init();
  nxt_motor_init();
  i2c_init();
  bt_init();
  bt_start_ad_converter();
  do {
    bt_receive(cmd);
  } while((cmd[0] != 3) && (cmd[1] != 0x14));
  //  cmd[1] = 0x36; // break stream mode
  //  cmd[2] = 0;
  //  bt_send_cmd(cmd);
  //  cmd[1] = 0x1C; cmd[2] = 1; bt_send_cmd(cmd); // make visible
  cmd[1] = 0x03; bt_send_cmd(cmd); // open port query
  display_clear(1);
  show_splash(1000);
  display_goto_xy(0,0);

  terminal_prepped = 1;
}

void deprep_terminal ()
{
  terminal_prepped = 0;
}

long key_avail ()
{
  if(!terminal_prepped) prep_terminal();

  do_bluetooth();
  if(bt_mode) {
    return bt_avail();
  } else {
    systick_wait_ms(100);
    return 0;
  }
}

Cell getkey()
{
  int key;

  if(!terminal_prepped) prep_terminal();

  if(needs_update) {
    display_update();
    needs_update = 0;
  }

  while(!key_avail());
  
  while((key=bt_getkey())==0);
  display_char(key); display_update();

  return key;
}

void emit_char(char x)
{
  if(!terminal_prepped) prep_terminal();
  /*  display_char(x);
  if(x == '\n') {
    display_update();
    needs_update = 0;
  } else
  needs_update = 1; */
  if(bt_mode) bt_send(&x, 1);
}

void type_chars(char *addr, unsigned int l)
{
  if(bt_mode) bt_send(addr, l);
  /*  int i;
  for(i=0; i<l; i++)
  emit_char(addr[i]); */
}
