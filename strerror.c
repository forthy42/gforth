#include <errno.h>
#include <stdio.h>

extern char *sys_errlist[];
extern int sys_nerr;

char *strerror(int err)
{
  if (err<sys_nerr) /* !! or check with <= ? */
    return sys_errlist[err];
  else {
    static char errbuf[50];
    sprintf(errbuf,"Unknown system error %d",err);
    return errbuf;
  }
}
