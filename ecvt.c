#include <sys/time.h>
#include <setjmp.h>

jmp_buf throw_jmp_buf;

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

#include <math.h>

#define MAXCONV 0x40
char scratch[MAXCONV];

char* ecvt(double x, int len, int* exp, int* sign)
{
   int i, j;
   double z;
   
   if(len > (MAXCONV-1)) len = MAXCONV-1;
   
   if(x<0)
     {
	*sign = 1;
	x = -x;
     }
   else
     {
	*sign = 0;
     }
   
   if(x==0)
     {
	*exp=0;
	return "0";
     }
   
   *exp=(int)floor(log10(x));
   x = x / pow10((double)*exp);
   
   *exp += 1;
   
   for(i=0; i < len; i++)
     {
	z=floor(x);
	scratch[i]='0'+(char)((int)z);
	x = (x-z)*10;
     }
   
   if((x >= 5) && i)
     {
	for(j=i-1; j>=0; j--)
	  {
	     if(scratch[j]!='9')
	       {
		  scratch[j]+=1; break;
	       }
	     else
	       {
		  scratch[j]='0';
	       }
	  }
	if(j==0)
	  {
	     scratch[0]='1';
	     *exp += 1;
	  }
     }
   
   scratch[i]='\0';
   
   return scratch;
}

#ifdef TEST
int main(int argc, char ** argv)
{
   int a, b;
   char * conv=ecvt(PI*1e10,20,&a,&b);
   
   printf("ecvt Test: %f -> %s, %d, %d\n",PI,conv,a,b);
}
#endif
