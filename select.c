#include <sys/time.h>

/* select replacement for DOS computers for ms only */
void select(int n, int a, int b, int c, struct timeval * timeout)
{
   struct timeval time1;
   struct timeval time2;
   struct timezone zone1;

   gettimeofday(&time1,&zone1);
   time1.tv_sec += timeout->tv_sec;
   time1.tv_usec += timeout->tv_usec;
   if(time1.tv_usec >= 1000000)
     {
	time1.tv_sec += time1.tv_usec / 1000000;
	time1.tv_usec %= 1000000;
     }
   do
     {
	gettimeofday(&time2,&zone1);
     }
   while(time2.tv_sec < time1.tv_sec);

   do
     {
	gettimeofday(&time2,&zone1);
     }
   while(time2.tv_usec < time1.tv_usec &&
	 time2.tv_sec == time1.tv_sec);

}

/* cheap ecvt replacement */

