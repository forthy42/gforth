@ECHO OFF
REM Copyright 1995 Free Software Foundation, Inc.
REM
REM This file is part of Gforth.
REM
REM Gforth is free software; you can redistribute it and/or
REM modify it under the terms of the GNU General Public License
REM as published by the Free Software Foundation; either version 2
REM of the License, or (at your option) any later version.
REM
REM This program is distributed in the hope that it will be useful,
REM but WITHOUT ANY WARRANTY; without even the implied warranty of
REM MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
REM GNU General Public License for more details.
REM
REM You should have received a copy of the GNU General Public License
REM along with this program; if not, write to the Free Software
REM Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
ECHO *** Configuring for MS-DOS with DJGPP GNU C ***
COPY MAKEFILE.DOS MAKEFILE
COPY KERNL32L.FI KERNEL.FI
COPY 386.H MACHINE.H
COPY STARTUP.FS STARTUP.UNX
COPY STARTUP.DOS STARTUP.FS
COPY HISTORY.DOS HISTORY.FS
COPY KERNL32L.FI KERNAL.FI
