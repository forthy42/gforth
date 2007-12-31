#Copyright (C) 1995,1996,1997,1998,2000,2003,2007 Free Software Foundation, Inc.

#This file is part of Gforth.

#Gforth is free software; you can redistribute it and/or
#modify it under the terms of the GNU General Public License
#as published by the Free Software Foundation, either version 3
#of the License, or (at your option) any later version.

#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.#See the
#GNU General Public License for more details.

#You should have received a copy of the GNU General Public License
#along with this program. If not, see http://www.gnu.org/licenses/.

# We use the bash and shell and file utilities with dos
s%SHELL%# SHELL%g
s% rm -rf% deltree%g
s% rm% del%g
s% cp -p% copy%g
s% ln -s% cp%g
s%\($(CP).*\)/%\1\\%g
s%\($(CP).*\)/%\1\\%g
s%@mach_h@%386%g
s%-pipe %%g
s% ./gforth% gforth%g
s%io.o %%g
s%-DDEFAULTPATH=\\".*"%%g
s%@kernel_fi@%kernl32l.fi%g
s%@KERNEL@%kernl16l.fi kernl16b.fi kernl32l.fi kernl32b.fi kernl64l.fi kernl64b.fi%g
s%@LIBOBJS@%ecvt.o io.o strsig.o getopt.o getopt1.o select.o%g
s%@host@%dos%g
s%@CC@%gcc%g
s%@GFORTH_EXE@%%g
s%@GFORTHDITC_EXE@%%g
s%@INSTALL@%install-sh%g
s%@INSTALL_PROGRAM@%install-sh%g
s%@INSTALL_DATA@%install-sh%g
s%@LN_S@%copy%g
s%@CFLAGS@%-fforce-mem -fforce-addr -fomit-frame-pointer%g
s%@ENGINE_FLAGS@%%g
s%@DEFS@%%g
s%@DEBUGFLAG@%%g
s%@LDFLAGS@%%g
s%@GCCLDFLAGS@%%g
s%@LIBS@%-lm -lpc%g
s%@prefix@%%g
s%@exec_prefix@%%g
s%@srcdir@%.%g
s%@LINK_KERNL@%-$(CP) kernl32l.fi kernel.fi%g
s%@PATHSEP@%;%g
s%-fforce-mem -fforce-addr %%g
s%echo ": version-string s\\" $(VERSION)\\" ;" > kernel/version.fs%$(CP) version.fs1 kernel\\version.fs%g
s%config.h.in ../config.status%%g
s%cd .. && CONFIG_FILES=$@ CONFIG_HEADERS=engine/config.h ./config.status%echo I hope you configured your system%g
s%$(FORTHPATH)$(PATHSEP)%%g
s%@FORTHSIZES@%%g
s%if test -r $@ && test x'$(VERSION)' = x`cat $@` ; then true ; else \(.*\) ; fi%\1%g
s%Makefile \(.*\) config.status%Makefile \1%
s%--clear-dictionary%-c%g
s%".$(PATHSEP)~+$(PATHSEP)$(srcdir)"%"~+"%g
# s% -I$(srcdir)%%g
s%GFORTHD=.*gforth.fi%gforthmi gforth.fi%g
s%gforth-ditc%gforth-d%g
s%gforth-fast%gforth-f%g
s%engine-ditc%engine-d%g
s%engine-fast%engine-f%g
s%main-ditc%main-d%g
s%main-fast%main-f%g
s%@OSCLASS@%dos%g
s%@machine@%386%g
s%@PACKAGE_VERSION@%0.5.9%g
s%@EXEEXT@%.exe%g
s%--die-on-signal%-s%g
s%cd engine && $(MAKE)%$(MAKE) -C engine%g
s%\(CFLAGS2.*\) -O4%\1%g
s%gforthmi gforth\$%gforthmi.bat gforth\$%g
s%engine/Makefile configure%engine/Makefile%g
s%$(exec_prefix)/bin%%g
s%@asm_fs@%./arch/386/asm.fs%g
s%@disasm_fs@%./arch/386/disasm.fs%g
