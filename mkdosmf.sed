# We use the bash and shell and file utilities with dos
s%SHELL%# SHELL%g
# s% rm% del%g
# s% cp% copy%g
s% ln -s% cp%g
s%@mach_h@%386%g
s%-pipe %%g
s% ./gforth% gforth%g
s%io.o %%g
s%-DDEFAULTPATH=\\".*"%%g
s%@kernel_fi@%kernl32l.fi%g
s%@KERNEL@%kernl16l.fi kernl16b.fi kernl32l.fi kernl32b.fi kernl64l.fi kernl64b.fi%g
s%@LIBOBJS@%ecvt.o io.o strsig.o%g
s%@getopt_long@%getopt.o getopt1.o%g
s%@host@%dos%g
s%@CC@%gcc%g
s%@GFORTH_EXE@%coff2exe $@%g
s%@GFORTHDITC_EXE@%coff2exe $@%g
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
# s%echo "static char gforth_version.*;" >$@%$(CP) version.h1 engine\version.h%g
# s%echo ": version-string.*;" >$@%$(CP) version.fs1 kernel\version.fs%g
s%$(srcdir)/config.h.in:	stamp-h.in%#$(srcdir)/config.h.in:	stamp-h.in%g
s%engine/config.h:	stamp-h%#engine/config.h:	stamp-h%g
s%$(FORTHPATH)$(PATHSEP)%%g
s%@FORTHSIZES@%%g
s%test x'$(VERSION)' = x`cat $@` || %%g
s%--clear-dictionary%-c%g
s%.$(PATHSEP)$(srcdir)%.%g
# s% -I$(srcdir)%%g
s%GFORTHD="./gforth-ditc -p .* $(srcdir)/%%g
s%gforth-ditc%gforth-d%g
s%engine-ditc%engine-d%g
s%main-ditc%main-d%g
s%@OSCLASS@%dos%g
s%$(MAKE) -C%bash makein.bsh%g
