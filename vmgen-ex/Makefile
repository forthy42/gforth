#Makefile for vmgen example

#Copyright (C) 2001 Free Software Foundation, Inc.

#This file is part of Gforth.

#Gforth is free software; you can redistribute it and/or
#modify it under the terms of the GNU General Public License
#as published by the Free Software Foundation; either version 2
#of the License, or (at your option) any later version.

#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.#See the
#GNU General Public License for more details.

#You should have received a copy of the GNU General Public License
#along with this program; if not, write to the Free Software
#Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

LEX=flex -l
YACC=bison
#YACC=yacc
VMGEN=vmgen
GCC=gcc -g -Wall
CC=$(GCC)

mini: mini.tab.o support.o peephole.o profile.o disasm.o engine.o engine-debug.o
	$(CC) $^ -o $@

lex.yy.c: mini.l
	$(LEX) mini.l

mini.tab.c: mini.y
	$(YACC) -b mini mini.y

mini-vm.i mini-disasm.i mini-gen.i mini-labels.i mini-profile.i mini-peephole.i: mini.vmg
	$(VMGEN) mini.vmg

mini.tab.o: mini.tab.c mini-gen.i lex.yy.c mini.h

support.o: support.c mini.h

peephole.o: peephole.c mini-peephole.i mini.h

profile.o: profile.c mini-profile.i mini.h

disasm.o: disasm.c mini-disasm.i mini.h

engine.o: engine.c mini-vm.i mini-labels.i mini.h

engine-debug.o: engine.c mini-vm.i mini-labels.i mini.h
	$(GCC) -DVM_DEBUG -DVM_PROFILING -Dengine=engine_debug -c -o $@ $<

clean:
	rm -f *.o mini mini-*.i lex.yy.c mini.tab.c