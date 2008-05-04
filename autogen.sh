#!/bin/sh

# Procedures to build gforth from the CVS sources

#Copyright (C) 2008 Free Software Foundation, Inc.

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

# makes a aclocal.m4 which includes the automake macros for autconf

aclocal &&

# generate configure script

autoconf &&

# generate header files like engine/config.h.in

autoheader &&

echo timestamp >stamp-h.in &&

#a local gforth.fi probably does not work with $GFORTH
rm -f gforth.fi