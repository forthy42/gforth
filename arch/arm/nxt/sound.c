#include "mytypes.h"
#include "sound.h"
#include "AT91SAM7.h"
#include "aic.h"

extern void sound_isr_entry(void);

const U32 tone_pattern[16] = 
  {
    0xF0F0F0F0,0xF0F0F0F0,
    0xFCFCFCFC,0xFCFCFDFD,
    0xFFFFFFFF,0xFFFFFFFF,
    0xFDFDFCFC,0xFCFCFCFC,
    0xF0F0F0F0,0xF0F0F0F0,
    0xC0C0C0C0,0xC0C08080,
    0x00000000,0x00000000,
    0x8080C0C0,0xC0C0C0C0
  };
  
U32 tone_cycles;

void sound_init()
{
  sound_interrupt_disable();
  sound_disable();
  
  *AT91C_PMC_PCER = (1 << AT91C_PERIPHERAL_ID_SSC);

  //*AT91C_PIOA_ODR = AT91C_PA17_TD;
  //*AT91C_PIOA_OWDR = AT91C_PA17_TD;
  //*AT91C_PIOA_MDDR = AT91C_PA17_TD;
  //*AT91C_PIOA_PPUDR = AT91C_PA17_TD;
  //*AT91C_PIOA_IFDR = AT91C_PA17_TD;
  //*AT91C_PIOA_CODR = AT91C_PA17_TD;
  
  *AT91C_SSC_CR = AT91C_SSC_SWRST;
  *AT91C_SSC_TCMR = AT91C_SSC_CKS_DIV + AT91C_SSC_CKO_CONTINOUS + AT91C_SSC_START_CONTINOUS;
  *AT91C_SSC_TFMR = 31 + (7 << 8) + AT91C_SSC_MSBF; // 8 32-bit words
  *AT91C_SSC_CR = AT91C_SSC_TXEN;                                        

  aic_mask_on(AT91C_PERIPHERAL_ID_SSC);
  aic_clear(AT91C_PERIPHERAL_ID_SSC);
  aic_set_vector(AT91C_PERIPHERAL_ID_SSC, AT91C_AIC_PRIOR_LOWEST | AT91C_AIC_SRCTYPE_INT_EDGE_TRIGGERED,
		 sound_isr_entry);
}

void sound_freq(U32 freq, U32 ms)
{
  *AT91C_SSC_CMR = ((96109714 / 1024) / freq) + 1;
  *AT91C_SSC_PTCR = AT91C_PDC_TXTEN;
  tone_cycles = (freq * ms) / 2000 - 1;
  sound_interrupt_enable();
}

void sound_interrupt_enable()
{
  *AT91C_SSC_IER = AT91C_SSC_ENDTX;
}

void sound_interrupt_disable()
{
  *AT91C_SSC_IDR = AT91C_SSC_ENDTX;
}

void sound_enable()
{
  *AT91C_PIOA_PDR = AT91C_PA17_TD;
}

void sound_disable()
{
  *AT91C_PIOA_PER = AT91C_PA17_TD;
}

void sound_isr_C()
{
  if (tone_cycles--)
  {
    *AT91C_SSC_TNPR = (unsigned int) tone_pattern;
    *AT91C_SSC_TNCR = 16;
    sound_enable();
  }
  else
  {
  	sound_disable();
  	sound_interrupt_disable();
  }
}
