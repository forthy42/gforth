/* Input driver header */

#include <setjmp.h>

unsigned char getkey(FILE *);
long key_avail(FILE *);
void prep_terminal();
void deprep_terminal();
void install_signal_handlers(void);

extern jmp_buf throw_jmp_buf;

#define key()		getkey(stdin)
#define key_query	-(!!key_avail(stdin)) /* !! FLAG(...)? - anton */
         		/* flag was originally wrong -- lennart */

