/* stub main to create executables with libgforth


  Author: Bernd Paysan
  Copyright (C) 2012,2019 Free Software Foundation, Inc.

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

#include "config.h"
#include "forth.h"
#include <stdlib.h>
#include <signal.h>

int main(int argc, char **argv, char **env)
{
  int retval=gforth_main(argc, argv, env);
  switch(retval) {
  case 0: return EXIT_SUCCESS;
  case -9:  return 0x80|SIGSEGV;
  case -28: return 0x80|SIGINT;
  case -55: return 0x80|SIGFPE;
#ifdef SIGPIPE
  case -2049: return 0x80|SIGPIPE;
#endif
  case -0x11F ... -0x100: return 0x80|((-retval) & 0x1F);
  case 1 ... 0xFF: return retval;
  default: return EXIT_FAILURE;
  }
}
