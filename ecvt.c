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
	*exp=-1;
   else
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
