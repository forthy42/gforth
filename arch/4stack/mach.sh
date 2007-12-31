#!/bin/sh
# Copyright (C) 2001,2003,2004,2007 Free Software Foundation, Inc.

# This file is part of Gforth.

# Gforth is free software; you can redistribute it and/or
# modify it under the terms of the GNU General Public License
# as published by the Free Software Foundation, either version 3
# of the License, or (at your option) any later version.

# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.

# You should have received a copy of the GNU General Public License
# along with this program; if not, see http://www.gnu.org/licenses/.
./gforth arch/4stack/relocate.fs \
 -e "s\" $1-\" read-gforth s\" arch/4stack/gforth.4o\" write-gforth bye"
cp $1- $1
