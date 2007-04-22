
#include "mytypes.h"
#include "twi.h"
#include "interrupts.h"
#include "AT91SAM7.h"

#include "systick.h"

#include "byte_fifo.h"

#include "aic.h"



extern void twi_isr_entry(void);


static enum {
  TWI_UNINITIALISED = 0,
  TWI_IDLE,
  TWI_TX_BUSY,
  TWI_TX_DONE,
  TWI_RX_BUSY,
  TWI_RX_DONE,
  TWI_FAILED
} twi_state;

static U32 twi_pending;
static U8 *twi_ptr;

static struct {
  U32 rx_done;
  U32 tx_done;
  U32 bytes_tx;
  U32 bytes_rx;
  U32 unre;
  U32 ovre;
  U32 nack;
} twi_stats;




int
twi_busy(void)
{
  return (twi_state == TWI_TX_BUSY || twi_state == TWI_RX_BUSY);
}

int
twi_ok(void)
{
  return (twi_state >= TWI_IDLE && twi_state <= TWI_RX_DONE);
}

void
twi_isr_C(void)
{
  U32 status = *AT91C_TWI_SR;

  if ((status & AT91C_TWI_RXRDY) && twi_state == TWI_RX_BUSY) {


    if (twi_pending) {
      twi_stats.bytes_rx++;
      *twi_ptr = *AT91C_TWI_RHR;
      twi_ptr++;
      twi_pending--;
      if (twi_pending == 1) {
	/* second last byte -- issue a stop on the next byte */
	*AT91C_TWI_CR = AT91C_TWI_STOP;
      }
      if (!twi_pending) {
	twi_stats.rx_done++;
	twi_state = TWI_RX_DONE;
      }
    }

  }

  if ((status & AT91C_TWI_TXRDY) && twi_state == TWI_TX_BUSY) {
    if (twi_pending) {
      /* Still Stuff to send */
      *AT91C_TWI_CR = AT91C_TWI_MSEN | AT91C_TWI_START;
      if (twi_pending == 1) {
	*AT91C_TWI_CR = AT91C_TWI_STOP;
      }
      *AT91C_TWI_THR = *twi_ptr;
      twi_stats.bytes_tx++;

      twi_ptr++;
      twi_pending--;

    } else {
      /* everything has been sent */
      twi_state = TWI_TX_DONE;
      *AT91C_TWI_IDR = ~0;
      twi_stats.tx_done++;
    }
  }

  if (status & AT91C_TWI_OVRE) {
    /* */
    twi_stats.ovre++;
    *AT91C_TWI_CR = AT91C_TWI_STOP;
    *AT91C_TWI_IDR = ~0;
    twi_state = TWI_FAILED;

  }

  if (status & AT91C_TWI_UNRE) {
    /* */
    twi_stats.unre++;
    *AT91C_TWI_IDR = ~0;
    twi_state = TWI_FAILED;
  }

  if (status & AT91C_TWI_NACK) {
    /* */
    twi_stats.nack++;
    *AT91C_TWI_IDR = ~0;
    twi_state = TWI_UNINITIALISED;
  }
}



void
twi_reset(void)
{
  U32 clocks = 9;

  *AT91C_TWI_IDR = ~0;

  *AT91C_PMC_PCER = (1 << AT91C_PERIPHERAL_ID_PIOA) |	/* Need PIO too */
    (1 << AT91C_PERIPHERAL_ID_TWI);	/* TWI clock domain */

  /* Set up pin as an IO pin for clocking till clean */
  *AT91C_PIOA_MDER = (1 << 3) | (1 << 4);
  *AT91C_PIOA_PER = (1 << 3) | (1 << 4);
  *AT91C_PIOA_ODR = (1 << 3);
  *AT91C_PIOA_OER = (1 << 4);

  while (clocks > 0 && !(*AT91C_PIOA_PDSR & (1 << 3))) {

    *AT91C_PIOA_CODR = (1 << 4);
    systick_wait_ns(1500);
    *AT91C_PIOA_SODR = (1 << 4);
    systick_wait_ns(1500);
    clocks--;
  }

  *AT91C_PIOA_PDR = (1 << 3) | (1 << 4);
  *AT91C_PIOA_ASR = (1 << 3) | (1 << 4);

  *AT91C_TWI_CR = 0x88;		/* Disable & reset */

  *AT91C_TWI_CWGR = 0x020f0f;	/* Set for 380kHz */
  *AT91C_TWI_CR = 0x04;		/* Enable as master */
}

int
twi_init(void)
{
  int i_state;

  i_state = interrupts_get_and_disable();

  /* Todo: set up interrupt */
  *AT91C_TWI_IDR = ~0;		/* Disable all interrupt sources */
  aic_mask_off(AT91C_PERIPHERAL_ID_TWI);
  aic_set_vector(AT91C_PERIPHERAL_ID_TWI, AIC_INT_LEVEL_ABOVE_NORMAL,
		 twi_isr_entry);
  aic_mask_on(AT91C_PERIPHERAL_ID_TWI);


  twi_reset();

  /* Init peripheral */

  twi_state = TWI_IDLE;

  if (i_state)
    interrupts_enable();

  return 1;
}



void
twi_start_read(U32 dev_addr, U32 int_addr_bytes, U32 int_addr, U8 *data,
	       U32 nBytes)
{
  U32 mode =
    ((dev_addr & 0x7f) << 16) | ((int_addr_bytes & 3) << 8) | (1 << 12);
  U32 dummy;

  if (!twi_busy()) {

    twi_state = TWI_RX_BUSY;
    *AT91C_TWI_IDR = ~0;	/* Disable all interrupts */
    twi_ptr = data;
    twi_pending = nBytes;
    dummy = *AT91C_TWI_SR;
    dummy = *AT91C_TWI_RHR;
//      *AT91C_AIC_ICCR = ( 1<< AT91C_PERIPHERAL_ID_TWI);
    *AT91C_TWI_MMR = mode;
    *AT91C_TWI_CR = AT91C_TWI_START | AT91C_TWI_MSEN;
//      dummy = *AT91C_TWI_SR;
    *AT91C_TWI_IER = 0x01C2;
  }

}

void
twi_start_write(U32 dev_addr, U32 int_addr_bytes, U32 int_addr,
		const U8 *data, U32 nBytes)
{
  U32 mode = ((dev_addr & 0x7f) << 16) | ((int_addr_bytes & 3) << 8);

  if (!twi_busy()) {
    twi_state = TWI_TX_BUSY;
    *AT91C_TWI_IDR = ~0;	/* Disable all interrupts */
    twi_ptr = data;
    twi_pending = nBytes;

    *AT91C_TWI_MMR = mode;
    *AT91C_TWI_CR = AT91C_TWI_START | AT91C_TWI_MSEN;
    *AT91C_TWI_IER = 0x1C4;
  }

}
