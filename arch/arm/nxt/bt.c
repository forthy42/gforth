/*
 * This file is copied from the leJOS NXT project and comes
 * under the Mozilla public license (see file LICENSE in this directory)
 */

#include "mytypes.h"
#include "AT91SAM7.h"
#include "uart.h"
#include "bt.h"
#include "aic.h"
#include  <string.h>

static U8 in_buf[2][128];
static U8 in_buf_in_ptr, out_buf_ptr;
static U8 out_buf[2][256];

static U8* buf_ptr;

static int in_buf_idx = 0;

#define BAUD_RATE 460800
#define CLOCK_RATE 48054850
	
void bt_init(void)
{
  U8 trash;
  
  in_buf_in_ptr = out_buf_ptr = 0; 
  in_buf_idx = 0;
  
  *AT91C_PMC_PCER = (1 << AT91C_PERIPHERAL_ID_US1); 
  
  *AT91C_PIOA_PDR = BT_RX_PIN | BT_TX_PIN | BT_SCK_PIN | BT_RTS_PIN | BT_CTS_PIN; 
  *AT91C_PIOA_ASR = BT_RX_PIN | BT_TX_PIN | BT_SCK_PIN | BT_RTS_PIN | BT_CTS_PIN; 
  
  *AT91C_US1_CR   = AT91C_US_RSTSTA;
  *AT91C_US1_CR   = AT91C_US_STTTO;
  *AT91C_US1_RTOR = 10000; 
  *AT91C_US1_IDR  = AT91C_US_TIMEOUT;
  *AT91C_US1_MR = (AT91C_US_USMODE_HWHSH & ~AT91C_US_SYNC) | AT91C_US_CLKS_CLOCK | AT91C_US_CHRL_8_BITS | AT91C_US_PAR_NONE | AT91C_US_NBSTOP_1_BIT | AT91C_US_OVER;
  *AT91C_US1_BRGR = ((CLOCK_RATE/8/BAUD_RATE) | (((CLOCK_RATE/8) - ((CLOCK_RATE/8/BAUD_RATE) * BAUD_RATE)) / ((BAUD_RATE + 4)/8)) << 16);
  *AT91C_US1_PTCR = (AT91C_PDC_RXTDIS | AT91C_PDC_TXTDIS); 
  *AT91C_US1_RCR  = 0; 
  *AT91C_US1_TCR  = 0; 
  *AT91C_US1_RNPR = 0;
  *AT91C_US1_TNPR = 0;
  
  aic_mask_off(AT91C_PERIPHERAL_ID_US1);
  aic_clear(AT91C_PERIPHERAL_ID_US1);

  trash = *AT91C_US1_RHR;
  trash = *AT91C_US1_CSR;
  
  *AT91C_US1_RPR  = (unsigned int)&(in_buf[0][0]); 
  *AT91C_US1_RCR  = 128;
  *AT91C_US1_RNPR = (unsigned int)&(in_buf[1][0]);
  *AT91C_US1_RNCR = 128;
  *AT91C_US1_CR   = AT91C_US_RXEN | AT91C_US_TXEN; 
  *AT91C_US1_PTCR = (AT91C_PDC_RXTEN | AT91C_PDC_TXTEN); 
  
  *AT91C_PIOA_PDR = BT_RX_PIN | BT_TX_PIN | BT_SCK_PIN | BT_RTS_PIN | BT_CTS_PIN; 
  *AT91C_PIOA_ASR = BT_RX_PIN | BT_TX_PIN | BT_SCK_PIN | BT_RTS_PIN | BT_CTS_PIN; 
  *AT91C_PIOA_PER   = BT_CS_PIN | BT_RST_PIN; 
  *AT91C_PIOA_OER   = BT_CS_PIN | BT_RST_PIN; 
  *AT91C_PIOA_SODR  = BT_CS_PIN | BT_RST_PIN;
  *AT91C_PIOA_PPUDR = BT_ARM7_CMD_PIN;
  *AT91C_PIOA_PER   = BT_ARM7_CMD_PIN; 
  *AT91C_PIOA_CODR  = BT_ARM7_CMD_PIN;
  *AT91C_PIOA_OER   = BT_ARM7_CMD_PIN; 

  *AT91C_ADC_MR  = 0;
  *AT91C_ADC_MR |= 0x00003F00;
  *AT91C_ADC_MR |= 0x00020000;
  *AT91C_ADC_MR |= 0x09000000;
  *AT91C_ADC_CHER  = AT91C_ADC_CH6 | AT91C_ADC_CH4; 
  
  buf_ptr = &(in_buf[0][0]);
}

void bt_start_ad_converter()
{
  *AT91C_ADC_CR = AT91C_ADC_START;
}

U32 bt_get_mode()
{
  return (U32) *AT91C_ADC_CDR6;
}

void bt_send(U8 *buf, U32 len)
{
  while (*AT91C_US1_TNCR != 0);

  memcpy(&(out_buf[out_buf_ptr][0]), buf, len);
  *AT91C_US1_TNPR = (unsigned int) &(out_buf[out_buf_ptr][0]);
  *AT91C_US1_TNCR = len;
  out_buf_ptr = (out_buf_ptr+1) % 2;
}

void bt_clear_arm7_cmd(void)
{
  *AT91C_PIOA_CODR  = BT_ARM7_CMD_PIN;
}

void bt_set_arm7_cmd(void)
{
  *AT91C_PIOA_SODR  = BT_ARM7_CMD_PIN;
}

void bt_set_reset_high(void)
{
  *AT91C_PIOA_SODR = BT_RST_PIN;
}

int bt_avail(void)
{
  if (*AT91C_US1_RNCR == 0)
    return 256 - *AT91C_US1_RCR - in_buf_idx;
  else
    return 128 - *AT91C_US1_RCR - in_buf_idx;
}

int bt_getkey(void)
{
  int out, total_bytes_ready;

  while(bt_avail()==0);

  out=buf_ptr[in_buf_idx++];

  if (in_buf_idx >= 128 && *AT91C_US1_RNCR == 0)
  {
  	// Switch current buffer, and set up next 
  	
  	in_buf_idx -= 128;
  	*AT91C_US1_RNPR = (unsigned int) buf_ptr;
  	*AT91C_US1_RNCR = 128;
  	in_buf_in_ptr = (in_buf_in_ptr+1) % 2;
  	buf_ptr = &(in_buf[in_buf_in_ptr][0]);
  }   

  return out;
}

void bt_receive(U8 * buf)
{
  int bytes_ready, total_bytes_ready;
  int cmd_len, i;
  U8* tmp_ptr;
  
  buf[0] = 0;
  buf[1] = 0;
  
  if (*AT91C_US1_RNCR == 0) {
  	bytes_ready = 128;
  	total_bytes_ready = 256 - *AT91C_US1_RCR;
  }
  else total_bytes_ready = bytes_ready = 128 - *AT91C_US1_RCR;
  
  // At least 2 bytes ready to be processed?
  
  if (total_bytes_ready > in_buf_idx + 1)
  {
  	cmd_len = (int) buf_ptr[in_buf_idx];
  	
  	// Data mode kludge - data cannot be more than 255 bytes
  	
  	if (in_buf_idx < 127)
  	{
  		if (buf_ptr[in_buf_idx+1] == 0) cmd_len++;
  	} 
  	else
  	{
  	  tmp_ptr = &(in_buf[(in_buf_in_ptr+1)%2][0]);
      if (tmp_ptr[0] == 0) cmd_len++;
  	}

    // Is whole command in the buffer?
  
    if (bytes_ready >= in_buf_idx + cmd_len + 1)
    { 	
  	  for(i=0;i<cmd_len+1;i++) buf[i] = buf_ptr[in_buf_idx++];
    }
    else
    {
      if (total_bytes_ready >= in_buf_idx + cmd_len + 1)
      {
      	for(i=0;i<cmd_len+1 && in_buf_idx < 128;i++) buf[i] = buf_ptr[in_buf_idx++];
      	in_buf_idx = 0;
      	tmp_ptr = &(in_buf[(in_buf_in_ptr+1)%2][0]);
      	for(;i<cmd_len+1;i++) buf[i] = tmp_ptr[in_buf_idx++];
      	in_buf_idx += 128;
      }
      else return; // wait for all bytes to be ready
    } 
  }
  
  // Current buffer full and fully processed
  
  if (in_buf_idx >= 128 && *AT91C_US1_RNCR == 0)
  { 	
  	// Switch current buffer, and set up next 
  	
  	in_buf_idx -= 128;
  	*AT91C_US1_RNPR = (unsigned int) buf_ptr;
  	*AT91C_US1_RNCR = 128;
  	in_buf_in_ptr = (in_buf_in_ptr+1) % 2;
  	buf_ptr = &(in_buf[in_buf_in_ptr][0]);
  }   
}

void bt_set_reset_low(void)
{
  *AT91C_PIOA_CODR = BT_RST_PIN;
}
	
