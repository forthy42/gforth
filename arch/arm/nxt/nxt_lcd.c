/*
 * This file is copied from the leJOS NXT project and comes
 * under the Mozilla public license (see file LICENSE in this directory)
 */

#include "nxt_lcd.h"
#include "nxt_spi.h"
#include "systick.h"
#include "string.h"



void
nxt_lcd_command(U8 cmd)
{
  U8 tmp = cmd;

  nxt_spi_write(0, &tmp, 1);
}

void
nxt_lcd_set_col(U32 coladdr)
{
  nxt_lcd_command(0x00 | (coladdr & 0xF));
  nxt_lcd_command(0x10 | ((coladdr >> 4) & 0xF));
}

void
nxt_lcd_set_multiplex_rate(U32 mr)
{
  nxt_lcd_command(0x20 | (mr & 3));
}

void
nxt_lcd_set_temp_comp(U32 tc)
{
  nxt_lcd_command(0x24 | (tc & 3));
}

void
nxt_lcd_set_panel_loading(U32 hi)
{
  nxt_lcd_command(0x28 | ((hi) ? 1 : 0));
}

void
nxt_lcd_set_pump_control(U32 pc)
{
  nxt_lcd_command(0x2c | (pc & 3));
}

void
nxt_lcd_set_scroll_line(U32 sl)
{
  nxt_lcd_command(0x40 | (sl & 0x3f));
}

void
nxt_lcd_set_page_address(U32 pa)
{
  nxt_lcd_command(0xB0 | (pa & 0xf));
}

void
nxt_lcd_set_pot(U32 pot)
{
  nxt_lcd_command(0x81);
  nxt_lcd_command(pot & 0xff);
}

void
nxt_lcd_set_ram_address_control(U32 ac)
{
  nxt_lcd_command(0x88 | (ac & 7));
}

void
nxt_lcd_set_frame_rate(U32 fr)
{
  nxt_lcd_command(0xA0 | (fr & 1));
}

void
nxt_lcd_set_all_pixels_on(U32 on)
{
  nxt_lcd_command(0xA4 | ((on) ? 1 : 0));
}

void
nxt_lcd_inverse_display(U32 on)
{
  nxt_lcd_command(0xA6 | ((on) ? 1 : 0));
}

void
nxt_lcd_enable(U32 on)
{
  nxt_lcd_command(0xAE | ((on) ? 1 : 0));
}

void
nxt_lcd_set_map_control(U32 map_control)
{
  nxt_lcd_command(0xC0 | ((map_control & 3) << 1));
}

void
nxt_lcd_reset(void)
{
  nxt_lcd_command(0xE2);
}

void
nxt_lcd_set_bias_ratio(U32 ratio)
{
  nxt_lcd_command(0xE8 | (ratio & 3));
}

void
nxt_lcd_set_cursor_update(U32 on)
{
  nxt_lcd_command(0xEE | ((on) ? 1 : 0));
}


void
nxt_lcd_data(const U8 *data)
{
  int i;

  for (i = 0; i < NXT_LCD_DEPTH; i++) {
    nxt_lcd_set_col(0);
    nxt_lcd_set_page_address(i);

    nxt_spi_write(1, data, NXT_LCD_WIDTH);
    data += NXT_LCD_WIDTH;
  }
}

void
nxt_lcd_power_up(void)
{

  systick_wait_ms(20);
  nxt_lcd_reset();
  systick_wait_ms(20);
  nxt_lcd_set_multiplex_rate(3);	// 1/65
  nxt_lcd_set_bias_ratio(3);	// 1/9
  nxt_lcd_set_pot(0x60);	// ?? 9V??

  nxt_lcd_set_ram_address_control(0);
  nxt_lcd_set_map_control(0x02);

  nxt_lcd_enable(1);

}

void
nxt_lcd_init(void)
{

  nxt_spi_init();

  nxt_lcd_power_up();

}
