/* select replacement for DOS computers for ms only

  Copyright (C) 1995 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/


#include <sys/time.h>

int select(int n, fd_set *a, fd_set *b, fd_set *c, struct timeval * timeout)
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
