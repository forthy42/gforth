dnl AC_CHECK_INT_TYPE macro

dnl Copyright (C) 1996,1997,1998 Free Software Foundation, Inc.

dnl This file is part of Gforth.

dnl Gforth is free software; you can redistribute it and/or
dnl modify it under the terms of the GNU General Public License
dnl as published by the Free Software Foundation; either version 2
dnl of the License, or (at your option) any later version.

dnl This program is distributed in the hope that it will be useful,
dnl but WITHOUT ANY WARRANTY; without even the implied warranty of
dnl MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.dnl See the
dnl GNU General Public License for more details.

dnl You should have received a copy of the GNU General Public License
dnl along with this program; if not, write to the Free Software
dnl Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

dnl AC_CHECK_INT_TYPE(SIZE, DESCRIPTION [, CROSS-SIZE])
AC_DEFUN(AC_CHECK_INT_TYPE,
[changequote(<<, >>)dnl
dnl The name to #define.
define(<<AC_TYPE_NAME>>, translit($2_TYPE, [a-z *], [A-Z_P]))dnl
dnl The cache variable name.
define(<<AC_CV_NAME>>, translit(ac_cv_int_type_$2, [ *], [_p]))dnl
changequote([, ])dnl
AC_MSG_CHECKING(integer type for $2)
AC_CACHE_VAL(AC_CV_NAME,
[AC_TRY_RUN([#include <stdio.h>
main()
{
  FILE *f=fopen("conftestval", "w");
  if (!f) exit(1);
#define check_size(type) if (sizeof(type)==($1)) fputs(#type, f), exit(0)
  check_size(int);
  check_size(short);
  check_size(char);
  check_size(long);
  check_size(long long);
  fputs("none",f), exit(0);
}], AC_CV_NAME=`cat conftestval`, AC_CV_NAME=0, ifelse([$3], , , AC_CV_NAME=$3))])dnl
AC_MSG_RESULT($AC_CV_NAME)
AC_DEFINE_UNQUOTED(AC_TYPE_NAME, $AC_CV_NAME)
undefine([AC_TYPE_NAME])dnl
undefine([AC_CV_NAME])dnl
])
