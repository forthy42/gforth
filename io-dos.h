/* Input driver header */

#define prep_terminal()
#define deprep_terminal()
#define install_signal_handlers()

#include <conio.h>
#include <setjmp.h>

extern jmp_buf throw_jmp_buf;

#define key()		getch()
#define key_query       FLAG(kbhit())
