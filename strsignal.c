#include <stdio.h>

char *strsignal(int sig)
{
  /* !! use sys_siglist; how do I find out how many sigs there are? */
  static char errbuf[50];
  sprintf(errbuf,"signal %d",sig);
  return errbuf;
}
