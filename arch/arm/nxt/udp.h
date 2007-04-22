#ifndef __UDP_H__
#  define __UDP_H__

#  include "mytypes.h"

void udp_isr_C(void);
int udp_init(void);
void uart_close(U32 u);

#endif
