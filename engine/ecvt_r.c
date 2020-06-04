/* ecvt_r adapted from glibc-2.31/misc/efgcvt_r.c

  Authors: Bernd Paysan, Anton Ertl
  Copyright (C) 1998,2000,2007,2014,2015,2016,2017,2019 Free Software Foundation, Inc.

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

#define __ECVT_R ecvt_r
#define __FCVT_R fcvt_r
#define SNPRINTF snprintf
#ifdef __set_errno
#undef __set_errno
#endif
#define __set_errno(x) (void)0
#include "efgcvt-dbl-macros.h"
#include "efgcvt_r-template.c"
