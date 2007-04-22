#ifndef SOUND_H_
#define SOUND_H_

#include "mytypes.h"

void sound_init();
void sound_interrupt_enable();
void sound_interrupt_disable();
void sound_enable();
void sound_disable();
void sound_isr_C();

void sound_freq(U32 freq, U32 ms);

#endif /*SOUND_H_*/
