/* a strsignal implementation

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

#include <stdio.h>
#include <signal.h>


char *strsignal(int sig)
{
  static char errbuf[16];

#if defined(HAVE_SYS_SIGLIST) && defined(NSIG)
  extern char *sys_siglist[];

  if (sig>0 && sig<NSIG)
    return sys_siglist[sig];
#endif
  sprintf(errbuf,"signal %d",sig);
  return errbuf;
}
