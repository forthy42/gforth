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
s%@LIBOBJS@%ecvt.o io.o strsig.o getopt.o getopt1.o select.o%g
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
s%echo ": version-string s\\" $(VERSION)\\" ;" > kernel/version.fs%$(CP) version.fs1 kernel\\version.fs%g
s%config.h.in ../config.status%%g
s%cd .. && CONFIG_FILES=$@ CONFIG_HEADERS=engine/config.h ./config.status%echo I hope you configured your system%g
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
s%@machine@%386%g
s%@VERSION@%0.4.0%g
s%@EXE@%.exe%g
s%cd engine && $(MAKE)%$(MAKE) -C engine%g
s%\(CFLAGS2.*\) -O4%\1%g