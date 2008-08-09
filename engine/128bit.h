/* supply [u]int128_t as DOUBLE_CELL_TYPEs under some conditions

  Copyright (C) 2005,2007,2008 Free Software Foundation, Inc.

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

#if !defined(DOUBLE_CELL_TYPE) || !defined(DOUBLE_UCELL_TYPE)
#if (SIZEOF_CHAR_P==8)
#ifdef FORCE_LL
/* #warning hand-defined int128_t */
typedef int int128_t __attribute__((__mode__(TI)));
typedef unsigned int uint128_t __attribute__((__mode__(TI)));
#define DOUBLE_CELL_TYPE int128_t
#define DOUBLE_UCELL_TYPE uint128_t
#else /* !defined(FORCE_LL) */
#define BUGGY_LONG_LONG
#endif /* !defined(FORCE_LL) */
#else /* (SIZEOF_CHAR_P!=8) */
#define BUGGY_LONG_LONG
#endif /* (SIZEOF_CHAR_P==8) */
#endif
