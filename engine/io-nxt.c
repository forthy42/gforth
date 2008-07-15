/* direct key io driver for NXT brick

  Copyright (C) 2007,2008 Free Software Foundation, Inc.

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
static char bt_buf[0x100];
static char udp_buf[0x100];
int bt_index;
int udp_index;
int udp_size;

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
    if(cmd[0]) {
      display_goto_xy(0,1);
      display_hex(cmd[0],2);
      display_goto_xy(3,1);
      display_hex(cmd[1],2);
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
	  type_chars("Gforth NXT\n", 11);
	}
	//	  bt_state = 1;
      } else {
	display_char('(');
      }
      break;
    case 0x20: // discoverableack
      if(cmd[2]) {
	display_char('?');
	cmd[1] = 0x03; bt_send_cmd(cmd); // open port query
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
  return bt_mode;
}

void prep_terminal ()
{
  char cmd[30];

  aic_initialise();
  interrupts_enable();
  systick_init();
  sound_init();
  nxt_avr_init();
  nxt_motor_init();
  i2c_init();
  bt_init();
  udp_init();
  display_init();
  show_splash(2000);
  display_goto_xy(0,0);
  display_string("BT Reset ");
  display_update();
  //  bt_reset();
  display_string("ok");
  display_update();
  bt_buf[0] = 0;
  bt_buf[1] = 0;
  bt_index = 0;
  //  bt_start_ad_converter();
  //  do {
  //    bt_receive(cmd);
  //  } while((cmd[0] != 3) && (cmd[1] != 0x14));
  //  cmd[1] = 0x36; // break stream mode
  //  cmd[2] = 0;
  //  bt_send_cmd(cmd);
  // cmd[1] = 0x1C; cmd[2] = 1; bt_send_cmd(cmd); // make visible
  display_string(".");
  display_goto_xy(0,1);
  display_update();
  while(!do_bluetooth() && !udp_configured());

  terminal_prepped = 1;
}

void deprep_terminal ()
{
  terminal_prepped = 0;
}

long key_avail ()
{
  if(!terminal_prepped) prep_terminal();

  if(do_bluetooth()) {
    return bt_buf[0] - bt_index;
  } else if(udp_configured()) {
    return udp_size - udp_index;
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
  
  do {
    if(do_bluetooth()) {
      while(bt_index >= bt_buf[0]) {
	bt_receive(bt_buf);
	bt_index = 0;
      }
      key = bt_buf[bt_index++];
    } else if(udp_configured()) {
      if(udp_index < udp_size) {
	key = udp_buf[udp_index++];
      } else {
	udp_size = udp_read(udp_buf, 0x100);
	udp_index = 0;
	key = 0;
      }
    } else {
      key = 0;
    }
  } while(!key);
    
  display_char(key); display_update();
  
  return key;
}

void emit_char(char x)
{
  char buf[3];
  if(!terminal_prepped) prep_terminal();
  if(bt_mode) {
    buf[0] = 1;
    buf[1] = 0;
    buf[2] = x;
    bt_send(buf, 3);
  }
}

void type_chars(char *addr, unsigned int l)
{
  int i;
  char buf[0x100];
  if(bt_mode) {
    buf[0]=l;
    buf[1]=0;
    for(i=0; i<l; i++) {
      buf[2+i]=addr[i];
    }
    bt_send(buf, l+2);
  }
}

volatile unsigned char gMakeRequest;
