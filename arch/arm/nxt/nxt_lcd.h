/*
 * This file is copied from the leJOS NXT project and comes
 * under the Mozilla public license (see file LICENSE in this directory)
 */

#ifndef __NXT_LCD_H__
#  define __NXT_LCD_H__

#  include "mytypes.h"

#  define NXT_LCD_WIDTH 100
#  define NXT_LCD_DEPTH 8

void nxt_lcd_init(void);
void nxt_lcd_power_up(void);
void nxt_lcd_power_down(void);
void nxt_lcd_data(const U8 *buffer);


#endif
