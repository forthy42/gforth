/* Descriptions and defaults for C preprocessor symbols for config.h.in

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

/* this, together with configure.in, is the source file for config.h.in */

/* Define if you want to force a direct threaded code implementation
   (does not work on all machines */
#ifndef DIRECT_THREADED
#undef DIRECT_THREADED
#endif

/* Define if you want to force an indirect threaded code implementation */
#ifndef INDIRECT_THREADED
#undef INDIRECT_THREADED
#endif

/* Define if you want to use explicit register declarations for better
   performance or for more convenient CODE words (does not work with
   all GCC versions on all machines) */
#ifndef FORCE_REG
#undef FORCE_REG
#endif

/* an integer type that is as long as a pointer */
#define CELL_TYPE long

/* an integer type that is twice as long as a pointer */
#define DOUBLE_CELL_TYPE none

/* a path separator character */
#define PATHSEP ':'

/* define this if there is no working DOUBLE_CELL_TYPE on your machine */
#undef BUGGY_LONG_LONG

@BOTTOM@
/* Of course, sys_siglist is a variable, not a function */
