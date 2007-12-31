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
typedef Cell *FILE;

#define stdin  ((FILE)0L)
#define stdout ((FILE)1L)
#define stderr ((FILE)2L)

#define O_RDONLY 0
#define O_RDWR   1
#define O_WRONLY 2
