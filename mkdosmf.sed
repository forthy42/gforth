s%/bin/sh%command.com%g
s% rm% del%g
s% cp% copy%g
s% ln -s% copy%g
s% ./gforth% gforth%g
s%if [ ! -f gforth ]; then $(MAKE) first; fi%-if not exist gforth.exe $(MAKE) first%g
s%@gforth_fi@%kernl32l.fi%g
s%@KERNAL@%kernl16l.fi kernl16b.fi kernl32l.fi kernl32b.fi kernl64l.fi kernl64b.fi%g
s%@LIBOBJS@%ecvt.o select.o%g
s%@getopt_long@%getopt.o getopt1.o%g
s%@host@%dos%g
s%@MAKE_EXE@%coff2exe $@%g
s%@INSTALL@%install-sh%g
s%@INSTALL_PROGRAM@%install-sh%g
s%@INSTALL_DATA@%install-sh%g
s%@LN_S@%copy%g
s%@CFLAGS@%-fforce-mem -fforce-addr -fomit-frame-pointer%g
s%@DEFS@% -DSIZEOF_CHAR_P=4 -DHAVE_LIBM=1 -DHAVE_RINT=1 -DHAVE_EXPM1=1 -DHAVE_LOG1P=1 %g
s%@DEBUGFLAG@%%g
s%@LDFLAGS@%%g
s%@GCCLDFLAGS@%%g
s%@LIBS@%-lm -lpc%g
s%@prefix@%%g
s%@exec_prefix@%%g
s%@srcdir@%%g
s%@LINK_KERNL16B@%%g
s%@LINK_KERNL16L@%%g
s%@LINK_KERNL32B@%%g
s%@LINK_KERNL32L@%-$(CP) kernl32l.fi gforth.fi%g
s%@LINK_KERNL64B@%%g
s%@LINK_KERNL64L@%%g