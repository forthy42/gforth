
#include "nxt_spi.h"
#include "interrupts.h"
#include "AT91SAM7.h"

#include "byte_fifo.h"

#include "aic.h"


/*
 * Note that this is not a normal SPI interface, 
 * it is a bodged version as used by the NXT's 
 * display.
 *
 * The display does not use MISO because you can
 * only write to it in serial mode.
 *
 * Instead, the MISO pin is not used by the SPI
 * and is instead driven as a PIO pin for controlling CD.
 */


#define CS_PIN	(1<<10)
#define CD_PIN  (1<<12)

extern void spi_isr_entry(void);

void
spi_isr_C(void)
{
}


void
nxt_spi_init(void)
{
  int i_state = interrupts_get_and_disable();

  /* Get clock */
  *AT91C_PMC_PCER = (1 << AT91C_PERIPHERAL_ID_PIOA) |	/* Need PIO too */
    (1 << AT91C_PERIPHERAL_ID_SPI);	/* SPI clock domain */
  /* Get pins, oly MOSI and clock */
  *AT91C_PIOA_PDR = /* (1<< 12) | */ (1 << 13) | (1 << 14);
  *AT91C_PIOA_ASR = /* (1<< 12) | */ (1 << 13) | (1 << 14);


  /* Set up MISO as an output to control CD.
   * Set up CS pin
   */
  *AT91C_PIOA_SODR = CS_PIN | CD_PIN;
  *AT91C_PIOA_PER = CS_PIN | CD_PIN;
  *AT91C_PIOA_OER = CS_PIN | CD_PIN;


  /* Set up SPI peripheral */
  *AT91C_SPI_CR = 1;		/* Enable */
  *AT91C_SPI_MR = 0x06000001;
  *AT91C_SPI_IDR = ~0;		/* Disable all interrupts */
  AT91C_SPI_CSR[0] = 0x18181801;
  AT91C_SPI_CSR[1] = 0x18181801;
  AT91C_SPI_CSR[2] = 0x18181801;
  AT91C_SPI_CSR[3] = 0x18181801;

  /* Todo set up interrupt */

  if (i_state)
    interrupts_enable();

}

void
nxt_spi_write(U32 CD, const U8 *data, U32 nBytes)
{
  U32 status;
  U32 cd_mask = (CD ? 0x100 : 0);

  *AT91C_PIOA_PER = CS_PIN;
  *AT91C_PIOA_SODR = CS_PIN;	/* Set high */
  *AT91C_PIOA_CODR = CS_PIN;	/* Set Low */

  if (CD)
    *AT91C_PIOA_SODR = CD_PIN;
  else
    *AT91C_PIOA_CODR = CD_PIN;


  while (nBytes) {
    *AT91C_SPI_TDR = (*data | cd_mask);
    data++;
    nBytes--;
    /* Wait until byte sent */
    do {
      status = *AT91C_SPI_SR;
    } while (!(status & 0x200));

  }
  *AT91C_PIOA_SODR = CS_PIN;	/* Set high */

}
