/* Input driver header */

unsigned char getkey(FILE *);
int key_avail(FILE *);
void prep_terminal();
void deprep_terminal();
void install_signal_handlers(void);

#define key()		getkey(stdin)
#define key_query	-(!!key_avail(stdin)) /* !! FLAG(...)? - anton */
         		/* flag was originally wrong -- lennart */

