@echo off
/* Copyright 1995 Free Software Foundation, Inc.

 This file is part of Gforth.

 Gforth is free software; you can redistribute it and/or
 modify it under the terms of the GNU General Public License
 as published by the Free Software Foundation; either version 2
 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program; if not, write to the Free Software
 Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/
echo *** Configuring for OS/2 with EMX 3.0 GNU C ***
THREAD = i
FREGS = n
:switches
IF "%1"=="--enable-direct-threaded" then THREAD=d
IF "%1"=="--enable-indirect-threaded" then THREAD=i
IF "%1"=="--enable-force-reg" then FREGS=y
shift
if "%1"!="" goto switches
copy makefile.os2 makefile
copy kernl32l.fi kernel.fi
copy 386.h machine.h
copy os2conf.h config.h
copy startup.fs startup.unx
copy startup.dos startup.fs
copy history.dos history.fs
if THREAD == 'i' then do
	ECHO #ifndef INDIRECT_THREADED >>config.h
	ECHO #define INDIRECT_THREADED 1 >>config.h
	ECHO #endif >>config.h
end
if THREAD == 'd' then do
	ECHO #ifndef DIRECT_THREADED >>config.h
	ECHO #define DIRECT_THREADED 1 >>config.h
	ECHO #endif >>config.h
end
if FREGS == 'y' then do
	ECHO #ifndef FORCE_REG >>config.h
	ECHO #define FORCE_REG 1 >>config.h
	ECHO #endif >>config.h
end
