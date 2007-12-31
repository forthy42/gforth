# Generic Makefile for word libraries			11may99jaw

#Copyright (C) 1999,2000,2003,2007 Free Software Foundation, Inc.

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

%.c: %.pri
	cat $*.h > $@
	$(GFORTHHOME)/gforth -e "include $(GFORTHHOME)/prims2cl.fs file $< main bye" >> $@

%.o: %.c
	$(GCC) -c -I$(GFORTHHOME) $(CFLAGS) $< -o $@

%.so: %.o
	ld -shared $(LIBS) $< -o $@
	rm -rf $<

clean:
	rm -rf *.so *.o
