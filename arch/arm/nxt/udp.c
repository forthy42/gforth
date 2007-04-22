
#include "mytypes.h"
#include "udp.h"
#include "interrupts.h"
#include "AT91SAM7.h"

#include "aic.h"


#define EP_OUT	1
#define EP_IN	2


static unsigned currentConfig;
static unsigned currentConnection;
static unsigned currentRxBank;

extern void udp_isr_entry(void);

void
udp_isr_C(void)
{

}


int
udp_init(void)
{
  int i_state;

  /* Make sure the USB PLL and clock are set up */
  *AT91C_CKGR_PLLR |= AT91C_CKGR_USBDIV_1;
  *AT91C_PMC_SCER = AT91C_PMC_UDP;
  *AT91C_PMC_PCER = (1 << AT91C_ID_UDP);

  /* Enable the UDP pull up by outputting a zero on PA.16 */
  *AT91C_PIOA_PER = (1 << 16);
  *AT91C_PIOA_OER = (1 << 16);
  *AT91C_PIOA_CODR = (1 << 16);

  /* Set up default state */

  currentConfig = 0;
  currentConnection = 0;
  currentRxBank = 0;

  i_state = interrupts_get_and_disable();

  aic_mask_off(AT91C_PERIPHERAL_ID_UDP);
  aic_set_vector(AT91C_PERIPHERAL_ID_UDP, AIC_INT_LEVEL_NORMAL,
		 (U32) udp_isr_entry);
  aic_mask_on(AT91C_PERIPHERAL_ID_UDP);


  if (i_state)
    interrupts_enable();

  return 1;
}

void
udp_close(U32 u)
{
  /* Nothing */
}
