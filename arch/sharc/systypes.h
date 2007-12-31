/* types needed for a standalone system

  Copyright (C) 1998,2000,2003,2004,2007 Free Software Foundation, Inc.

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

typedef Cell time_t;

#define SHARC

#ifdef __DOUBLES_ARE_FLOATS__
#define double float
#endif

struct timeval {
  Cell tv_sec;        /* seconds */
  Cell tv_usec;  /* microseconds */
};

struct timezone {
  Cell tz_minuteswest;
  /* minutes west of Greenwich */
  Cell tz_dsttime;
  /* type of dst correction */
};

struct tm
{
  Cell    tm_sec;         /* seconds */
  Cell    tm_min;         /* minutes */
  Cell    tm_hour;        /* hours */
  Cell    tm_mday;        /* day of the month */
  Cell    tm_mon;         /* month */
  Cell    tm_year;        /* year */
  Cell    tm_wday;        /* day of the week */
  Cell    tm_yday;        /* day in the year */
  Cell    tm_isdst;       /* daylight saving time */
};

struct stat {
  Cell st_inode;
  time_t st_time;
  Cell st_size;
};

#define F_OK 0
#define W_OK 2
#define R_OK 4

#include <io.h>
#include <fcntl.h>

double atanh(double r1);
double asinh(double r1);
double acosh(double r1);
char* ecvt(double x, int len, int* exp, int* sign);
char *strsignal(int sig);

#define AUTO_INCREMENT 1
#undef HAVE_RINT
#undef HAVE_ECVT
#undef HAVE_SYS_MMAN_H
#undef HAVE_MMAP
#undef HAVE_GETPAGESIZE
#undef HAVE_SYSCONF

#define PAGESIZE 0x1000

#define perror(string) fprintf(stderr, "%s: error %d\n", string, errno)
