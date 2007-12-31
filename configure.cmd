/*
Copyright (C) 1996,1997,1998,2000,2003,2007 Free Software Foundation, Inc.

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
say "*** Configuring for OS/2 with EMX 3.0 GNU C ***"

parse arg args

THREAD="i"
FREGS="n"

do while args \== ""
   parse var args arg args

   select
      when arg="--enable-direct-threaded" then THREAD="d"
      when arg="--enable-indirect-threaded" then THREAD="i"
      when arg="--enable-force-reg" then FREGS="y"
      when arg="--help" then do
        say "--enable and --with options recognized:"
        say "  --enable-force-reg      Use explicit register declarations if they appear in"
        say "                          the machine.h file. This can cause a good speedup,"
        say "                          but also incorrect code with some gcc versions on"
        say "                          some processors (default disabled)."
        say "  --enable-direct-threaded      Force direct threading. This may not work on"
        say "                                some machines and may cause slowdown on others."
        say "                                (default processor-dependent)"
        say "  --enable-indirect-threaded    Force indirect threading. This can cause a"
        say "                                slowdown on some machines."
        say "                                (default processor-dependent)"
      end
    otherwise
      do
        say "*** Unknown option:" arg
        call Usage
      end
  end

end

copy makefile.os2 makefile
copy "engine\makefile.os2" "engine\makefile"
copy kernl32l.fi kernel.fi
copy envos.os2 envos.fs
copy os2conf.h "engine\config.h"
if THREAD="i" THEN DO
	call lineout "engine\config.h", "#ifndef INDIRECT_THREADED"
	call lineout "engine\config.h", "#define INDIRECT_THREADED 1"
	call lineout "engine\config.h", "#endif"
end
IF THREAD="d" THEN do
	call lineout "engine\config.h", "#ifndef DIRECT_THREADED"
	call lineout "engine\config.h", "#define DIRECT_THREADED 1" 
	call lineout "engine\config.h", "#endif"
end
IF FREGS="y" THEN do
	call lineout "engine\config.h", "#ifndef FORCE_REG"
	call lineout "engine\config.h", "#define FORCE_REG 1"
	call lineout "engine\config.h", "#endif"
end
call lineout version.h1, 'static char gforth_version[]="0.4.0";'
call lineout "kernel\version.fs", ': version-string s" 0.4.0" ;'
call lineout 'version-stamp', '0.4.0'
