#Copyright 1992 by the ANSI figForth Development Group

RM	= echo 'Trying to remove'
GCC	= gcc
FORTH	= ansforth
CC	= gcc
SWITCHES = -D_POSIX_VERSION -DUSE_TOS -DUSE_FTOS -DDEFAULTBIN='"'`pwd`'"' -DDIRECT_THREADED #-DNDEBUG #turn off assertions
CFLAGS	= -O4 -Wall -g $(SWITCHES)

#-Xlinker -n puts text and data into the same 256M region
#John Wavrik should use -Xlinker -N to get a writable text (executable)
LDFLAGS	= -g -Xlinker -N
LDLIBS = -lm

EMACS	= emacs

INCLUDES = forth.h io.h

FORTH_SRC = cross.fs debug.fs environ.fs errore.fs extend.fs \
	filedump.fs glosgen.fs kernal.fs look.fs machine32b.fs \
	machine32l.fs main.fs other.fs search-order.fs see.fs sieve.fs \
	struct.fs tools.fs toolsext.fs vars.fs wordinfo.fs

SOURCES	= Makefile primitives primitives2c.el engine.c main.c io.c \
	apollo68k.h decstation.h 386.h hppa.h sparc.h \
	$(INCLUDES) $(FORTH_SRC)

RCS_FILES = $(SOURCES) INSTALL ToDo model high-level

GEN = ansforth

GEN_PRECIOUS = primitives.i prim_labels.i primitives.b prim_alias.4th aliases.fs

OBJECTS = engine.o io.o main.o

# things that need a working forth system to be generated
# this is used for antidependences,
FORTH_GEN = primitives.i prim_labels.i prim_alias.4th kernal.fi

all:	ansforth aliases.fs

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
		-rm $(GEN)

distclean:	clean
		-rm machine.h

realclean:	distclean
		-rm $(GEN_PRECIOUS)

current:	$(RCS_FILES)

ansforth:	$(OBJECTS) $(FORTH_GEN)
		-cp ansforth ansforth.old
		$(GCC) $(LDFLAGS) $(OBJECTS) $(LDLIBS) -o $@

kernal.fi:	search-order.fs cross.fs aliases.fs vars.fs add.fs \
		environ.fs errore.fs kernal.fs extend.fs tools.fs toolsext.fs \
                $(FORTH_GEN)
		-cp kernal.32limg kernal.32limg.old
		$(FORTH) main.fs


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

#primitives.4th:	primitives.b primitives2c.el
#		$(EMACS) -batch -load primitives2c.el -funcall make-forth

#GNU make default rules
% ::		RCS/%,v
		co $@
%.o :		%.c $(INCLUDES)
		$(CC) $(CFLAGS) -c $< -o $@
