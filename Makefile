#Copyright 1992 by the ANSI figForth Development Group

RM	= echo 'Trying to remove'
GCC	= gcc
FORTH	= gforth
CC	= gcc
SWITCHES = \
	-fno-defer-pop -fcaller-saves \
	-DUSE_TOS -DUSE_FTOS -DDEFAULTBIN='"'`pwd`'"' \
	-DDIRECT_THREADED -D_POSIX_VERSION -DUSE_GETOPT #-DNDEBUG #turn off assertions
CFLAGS	= -O4 -Wall -g $(SWITCHES)

#-Xlinker -n puts text and data into the same 256M region
#John Wavrik should use -Xlinker -N to get a writable text (executable)
LDFLAGS	= -g -Xlinker -N
LDLIBS = -lm

EMACS	= emacs

INCLUDES = forth.h io.h

FORTH_SRC = add.fs assert.fs blocks.fs bufio.fs cross.fs debug.fs \
	debugging.fs environ.fs errore.fs etags.fs extend.fs filedump.fs \
	float.fs glocals.fs glosgen.fs gray.fs hash.fs kernal.fs \
	locals-test.fs look.fs mach32b.fs mach32l.fs main.fs other.fs \
	prims2x.fs search-order.fs see.fs sieve.fs startup.fs struct.fs \
	test2.fs tools.fs toolsext.fs vars.fs vt100.fs wordinfo.fs

SOURCES	= Makefile primitives primitives2c.el engine.c main.c io.c \
	apollo68k.h decstation.h 386.h hppa.h sparc.h gforth.ds \
	$(INCLUDES) $(FORTH_SRC)

RCS_FILES = $(SOURCES) INSTALL ToDo model high-level

GEN = gforth

GEN_PRECIOUS = primitives.i prim_labels.i primitives.b aliases.fs

OBJECTS = engine.o io.o main.o

# things that need a working forth system to be generated
# this is used for antidependences,
FORTH_GEN = primitives.i prim_labels.i prim_alias.4th \
	kernl32l.fi kernl32b.fi gforth.texi

all:	gforth

#from the gcc Makefile: 
#"Deletion of files made during compilation.
# There are four levels of this:
#   `mostlyclean', `clean', `distclean' and `realclean'.
# `mostlyclean' is useful while working on a particular type of machine.
# It deletes most, but not all, of the files made by compilation.
# It does not delete libgcc.a or its parts, so it won't have to be recompiled.
# `clean' deletes everything made by running `make all'.
# `distclean' also deletes the files made by config.
# `realclean' also deletes everything that could be regenerated automatically."

clean:		
		-rm $(GEN) *.o *.s

distclean:	clean
		-rm machine.h machine.fs

realclean:	distclean
		-rm $(GEN_PRECIOUS)

#does not work
#gforth.tar.gz:	$(SOURCES) $(GEN_PRECIOUS) CVS
#		cd ..; tar cvf gforth/gforth.tar gforth/{$^}; gzip -9 gforth/gforth.tar

gforth:	$(OBJECTS) $(FORTH_GEN)
		-cp gforth gforth~
		$(GCC) $(LDFLAGS) $(OBJECTS) $(LDLIBS) -o $@

kernl32l.fi:	main.fs search-order.fs cross.fs aliases.fs vars.fs add.fs \
		errore.fs kernal.fs extend.fs tools.fs toolsext.fs \
		mach32l.fs $(FORTH_GEN)
		-cp kernl32l.fi kernl32l.fi~
		$(FORTH) -e 's" mach32l.fs"' main.fs

kernl32b.fi:	main.fs search-order.fs cross.fs aliases.fs vars.fs add.fs \
		errore.fs kernal.fs extend.fs tools.fs toolsext.fs \
		mach32b.fs $(FORTH_GEN)
		-cp kernl32b.fi kernl32b.fi~
		$(FORTH) -e 's" mach32b.fs"' main.fs

engine.s:	engine.c primitives.i prim_labels.i machine.h $(INCLUDES)
		$(GCC) $(CFLAGS) -S engine.c

engine.o:	engine.c primitives.i prim_labels.i machine.h $(INCLUDES)

primitives.b:	primitives
		m4 primitives >$@ 

primitives.i :	primitives.b prims2x.fs
		$(FORTH) prims2x.fs -e "s\" primitives.b\" ' output-c process-file bye" >$@

prim_labels.i :	primitives.b prims2x.fs
		$(FORTH) prims2x.fs -e "s\" primitives.b\" ' output-label process-file bye" >$@

aliases.fs:	primitives.b prims2x.fs
		$(FORTH) prims2x.fs -e "s\" primitives.b\" ' output-alias process-file bye" >$@

primitives.fs:	primitives.b prims2x.fs
		$(FORTH) prims2x.fs -e "s\" primitives.b\" ' output-forth process-file bye" >$@

gforth.texi:	gforth.ds primitives.b ds2texi.fs prims2x.fs
		$(FORTH) search-order.fs struct.fs debugging.fs ds2texi.fs prims2x.fs -e "s\" primitives.b\" ' register-doc process-file s\" gforth.ds\" r/o open-file throw ds2texi bye" >$@

gforth.dvi:	gforth.texi
		tex gforth.texi

#primitives.4th:	primitives.b primitives2c.el
#		$(EMACS) -batch -load primitives2c.el -funcall make-forth

#GNU make default rules
#% ::		RCS/%,v
#		co $@

%.s :		%.c $(INCLUDES)
		$(CC) $(CFLAGS) -S $< -o $@

%.o :		%.s
		$(CC) $(CFLAGS) -c $< -o $@

