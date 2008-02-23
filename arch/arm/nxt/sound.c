#include "mytypes.h"
#include "sound.h"
#include "AT91SAM7.h"
#include "aic.h"
#include "nxt_avr.h"
#include <string.h>

/* Buffer length must be a multiple of 8 and at most 64 (preferably as long as possible) */
#define PWM_BUFFER_LENGTH 64

extern void sound_isr_entry(void);

enum {
  SOUND_MODE_NONE,
  SOUND_MODE_TONE,
  SOUND_MODE_PCM
};

#if 0 /* Introduced with leJOS 0.3 but not used so far */
const U32 load_tone_pattern[16] = 
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
  
const U32 medium_tone_pattern[16] =
  {
    0xF0F0F0F0,0xF0F0F0F0,                        
    0xF8F8F8F8,0xF8F8FCFC,
    0xF8F8FCFC,0xFCFCFCFC,
    0xFCFCF8F8,0xF8F8F8F8,
    0xF0F0F0F0,0xF0F0F0F0,
    0xE0E0E0E0,0xE0E0C0C0,
    0xE0E0C0C0,0xC0C0C0C0,
    0xC0C0E0E0,0xE0E0E0E0
  };
#endif /*0*/

/*
  This pattern is only useful from frequencies higher than ca. 120 Hz,
  because PWM volume control enters the audible range below that,
  adding a rather irritating high-pitched component.

  The problem can be solved by using a higher sample rate for lower
  frequencies, and a longer waveform, of course.

  It would be probably a good idea to calculate the samples on the fly
  so as to allow volume control and better frequency control as well.
*/
const U32 tone_pattern_low[32] =
  {
    0xAAAAAAAA,0xAAAAAAAA,0xAAAAAAAA,0xAAAAAAAA,
    0xAAAAAAAA,0xAAAAAAAA,0xAAAAB6B6,0xB6B6B6B6,
    0xAAAAAAAA,0xB6B6B6B6,0xB6B6B6B6,0xAAAAAAAA,
    0xB6B6B6B6,0xB6B6AAAA,0xAAAAAAAA,0xAAAAAAAA,
    0xAAAAAAAA,0xAAAAAAAA,0xAAAAAAAA,0xAAAAAAAA,
    0xAAAAAAAA,0xAAAAAAAA,0xAAAA9292,0x92929292,
    0xAAAAAAAA,0x92929292,0x92929292,0xAAAAAAAA,
    0x92929292,0x9292AAAA,0xAAAAAAAA,0xAAAAAAAA
  };

/*
  Pattern for higher frequencies to prevent halving the maximum
  frequency. Very noisy below 250 Hz.
 */
const U32 tone_pattern_high[16] =
  {
    0xAAAAAAAA,0xAAAAAAAA,
    0xAAAAAAAA,0xAAB6B6B6,
    0xAAAAB6B6,0xB6B6AAAA,
    0xB6B6B6AA,0xAAAAAAAA,
    0xAAAAAAAA,0xAAAAAAAA,
    0xAAAAAAAA,0xAA929292,
    0xAAAA9292,0x9292AAAA,
    0x929292AA,0xAAAAAAAA
  };

/* Numbers with 0-32 evenly spaced bits set */
const U32 sample_pattern[33] =
  {
    0x00000000, 0x80000000, 0x80008000, 0x80200400,
    0x80808080, 0x82081040, 0x84208420, 0x88442210,
    0x88888888, 0x91224488, 0x92489248, 0xa4924924,
    0xa4a4a4a4, 0xa94a5294, 0xaa54aa54, 0xaaaa5554,
    0xaaaaaaaa, 0xd555aaaa, 0xd5aad5aa, 0xd6b5ad6a,
    0xdadadada, 0xdb6db6da, 0xedb6edb6, 0xeeddbb76,
    0xeeeeeeee, 0xf7bbddee, 0xfbdefbde, 0xfdf7efbe,
    0xfefefefe, 0xffdffbfe, 0xfffefffe, 0xfffffffe,
    0xffffffff
  };

U32 tone_cycles;
U32 *tone_pattern;
U8 tone_length;
U8 sound_mode = SOUND_MODE_NONE;

struct {
  // The number of samples ahead
  S32 count;
  // Pointer to the next sample
  U8* ptr;
  // 0 or 1, identifies the current buffer
  U8 buf_id;
  // Double buffer
  U32 buf1[PWM_BUFFER_LENGTH], buf2[PWM_BUFFER_LENGTH];
  // Amplification LUT
  U8 amp[256];
  // Chosen frequency (1/1024 Hz)
  S32 cfreq;
  // Actual frequency (1/1024 Hz)
  S32 afreq;
  // Frequency counter
  S32 fcnt;
} sample;

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
		 (U32)sound_isr_entry); /*PG*/
}

void sound_freq(U32 freq, U32 ms)
{
  if (freq < 500) {
    *AT91C_SSC_CMR = ((96109714 / 1024) / (freq << 1)) + 1;
    tone_pattern = (U32*)tone_pattern_low;
    tone_length = 32;
  } else {
    *AT91C_SSC_CMR = ((96109714 / 1024) / freq) + 1;
    tone_pattern = (U32*)tone_pattern_high;
    tone_length = 16;
  }
  *AT91C_SSC_PTCR = AT91C_PDC_TXTEN;
  tone_cycles = (freq * ms) / 2000 - 1;

  sound_mode = SOUND_MODE_TONE;
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

void sound_fill_sample_buffer() {
  U32 *sbuf = sample.buf_id ? sample.buf1 : sample.buf2;
  U8 i;
  /* Each 8-bit sample is turned into 8 32-bit numbers, i.e. 256 bits altogether */
  for (i = 0; i < PWM_BUFFER_LENGTH >> 3; i++) {
    U8 smp = sample.amp[*sample.ptr];
    U8 msk = "\x00\x10\x22\x4a\x55\x6d\x77\x7f"[smp & 7];
    U8 s3 = smp >> 3;
    *sbuf++ = sample_pattern[s3 + (msk & 1)]; msk >>= 1;
    *sbuf++ = sample_pattern[s3 + (msk & 1)]; msk >>= 1;
    *sbuf++ = sample_pattern[s3 + (msk & 1)]; msk >>= 1;
    *sbuf++ = sample_pattern[s3 + (msk & 1)]; msk >>= 1;
    *sbuf++ = sample_pattern[s3 + (msk & 1)]; msk >>= 1;
    *sbuf++ = sample_pattern[s3 + (msk & 1)]; msk >>= 1;
    *sbuf++ = sample_pattern[s3 + (msk & 1)];
    *sbuf++ = sample_pattern[s3];

    /*
      An alternative that doesn't need a sample_pattern array:

      U32 msb = 0xffffffff << (32 - (smp >> 3));
      *sbuf++ = msb | (msk & 1); msk >>= 1;
      *sbuf++ = msb | (msk & 1); msk >>= 1;
      *sbuf++ = msb | (msk & 1); msk >>= 1;
      *sbuf++ = msb | (msk & 1); msk >>= 1;
      *sbuf++ = msb | (msk & 1); msk >>= 1;
      *sbuf++ = msb | (msk & 1); msk >>= 1;
      *sbuf++ = msb | (msk & 1);
      *sbuf++ = msb;
    */

    /* Bresenham to the save */
    for (sample.fcnt += sample.cfreq; sample.fcnt >= sample.afreq; sample.fcnt -= sample.afreq) {
      sample.ptr++;
      sample.count--;
    }
  }
}

void sound_play_sample(U8 *data, U32 length, U32 freq, U32 amp)
{
  S16 i;

  //U32 cdiv = 96109714 / 2048 / freq;
  //if (cdiv < 4) cdiv = 4;

  /* Constant hardware frequency */
  U32 cdiv = 4;

  *AT91C_SSC_CMR = cdiv;
  *AT91C_SSC_PTCR = AT91C_PDC_TXTEN;
  sample.count = length;
  sample.buf_id = 0;
  sample.ptr = data;

  /* Frequency correction */
  sample.cfreq = freq << 10;
  sample.afreq = 96109714 / cdiv;
  sample.fcnt = 0;

  /* Simple linear amplification */
  for (i = 0; i < 256; i++) {
    S32 a = (i - 128) * (S32)amp / 1000 + 128;
    if (a < 0) a = 0;
    if (a > 255) a = 255;
    sample.amp[i] = a;
  }

  sound_fill_sample_buffer();

  sound_mode = SOUND_MODE_PCM;
  sound_interrupt_enable();
}

void sound_isr_C()
{
  switch (sound_mode) {
  case SOUND_MODE_TONE:
    if (tone_cycles--) {
      *AT91C_SSC_TNPR = (unsigned int)tone_pattern;
      *AT91C_SSC_TNCR = tone_length;
      sound_enable();
    } else {
      sound_disable();
      sound_interrupt_disable();
      sound_mode = SOUND_MODE_NONE;
    }
    break;
  case SOUND_MODE_PCM:
    if (sample.count > 0) {
      *AT91C_SSC_TNPR = (unsigned int)(sample.buf_id ? sample.buf1 : sample.buf2);
      *AT91C_SSC_TNCR = PWM_BUFFER_LENGTH;
      sample.buf_id ^= 1;
      sound_fill_sample_buffer();
      sound_enable();
    } else {
      sound_disable();
      sound_interrupt_disable();
      sound_mode = SOUND_MODE_NONE;
    }
    break;
  default:
    sound_disable();
    sound_interrupt_disable();
    sound_mode = SOUND_MODE_NONE;
  }
}