#include <stdio.h>

char *strsignal(int sig)
{
  /* !! use sys_siglist; how do I find out how many sigs there are? */
  static char errbuf[50];
  sprintf(errbuf,"siganl %d",sig);
  return errbuf;
}
