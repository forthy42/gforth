#Makefile for vmgen example

#Copyright (C) 2001,2003,2007,2008 Free Software Foundation, Inc.

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
#along with this program; if not, see http://www.gnu.org/licenses/.

LEX=flex -l
YACC=bison -y
#YACC=yacc
VMGEN=vmgen
#GCC=gcc -g -Wall
GCC=gcc -O3 -fomit-frame-pointer -Wall
CC=$(GCC)
#M4=m4 -s #recommended if supported
M4=m4
OBJECTS_MINI=mini.tab.o support.o peephole.o profile.o disasm.o engine.o engine-debug.o

mini: $(OBJECTS_MINI)
	$(CC) $(OBJECTS_MINI) -o $@

lex.yy.c: mini.l
	$(LEX) mini.l

mini.tab.c: mini.y
	$(YACC) mini.y && mv y.tab.c $@

mini-vm.i mini-disasm.i mini-gen.i mini-labels.i mini-profile.i mini-peephole.i: mini.vmg
	$(VMGEN) mini.vmg

mini.vmg: mini-inst.vmg mini-super.vmg
	$(M4) mini-inst.vmg >$@

mini.tab.o: mini.tab.c mini-gen.i lex.yy.c mini.h

support.o: support.c mini.h

peephole.o: peephole.c mini-peephole.i mini.h

profile.o: profile.c mini-profile.i mini.h

disasm.o: disasm.c mini-disasm.i mini.h

engine.o: engine.c mini-vm.i mini-labels.i mini.h

engine-debug.o: engine.c mini-vm.i mini-labels.i mini.h
	$(CC) -DVM_DEBUG -DVM_PROFILING -Dengine=engine_debug -c -o $@ engine.c

clean:
	rm -f *.o mini mini-*.i lex.yy.c mini.tab.c mini.vmg

check:  mini
	./mini test.mini | tr -d '\015' | diff - test.out

checkall:
	for i in 1 3 5 8 9 10; do make clean; echo $$i; make check VMGEN=../vmgen CC="gcc -O3 -fomit-frame-pointer -Wall -DTHREADING_SCHEME=$$i"; echo $$i >>xxx-bench; \time mini fib.mini 2>>xxx-bench; done; cat xxx-bench; rm xxx-bench	
#make profiles
%.prof: %.mini mini
	./mini -p $< 2>$@
