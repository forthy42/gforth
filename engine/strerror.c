/* a strerror implemenation

  Copyright (C) 1995,2000,2003,2007 Free Software Foundation, Inc.

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
