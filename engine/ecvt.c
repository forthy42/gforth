/* cheap ecvt replacement

  Copyright (C) 1998,2000,2007 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation, either version 3
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, see http://www.gnu.org/licenses/.
*/

#include <stdio.h>
#include "config.h"
#include <math.h>
extern double floor(double);
extern double pow10(double);

#define MAXCONV 0x40
char scratch[MAXCONV];

char* ecvt(double x, int len, int* exp, int* sign)
{
   int i, j;
   double z;
   
   if (isnan(x)) {
     *sign=0;
     *exp=0;
     return "nan";
   }
   if (isinf(x)) {
     *sign=0; /* this mimics the glibc ecvt */
     *exp=0;
     if (x<0)
       return "-inf";
     else
       return "inf";
   }
       
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
	if(z<0) z = 0;
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
	if(j<0)
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
   char * conv=ecvt(9e0,20,&a,&b);
   
   printf("ecvt Test: %f -> %s, %d, %d\n",9e0,conv,a,b);
}
#endif

