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
ECHO *** Configuring for MS-DOS with DJGPP 2.0 GNU C ***
set THREAD=i
set FREGS=n
:SWITCHES
IF "%1"=="--enable-direct-threaded" set THREAD=d
IF "%1"=="--enable-indirect-threaded" set THREAD=i
IF "%1"=="--enable-force-reg" set FREGS=y
shift
IF NOT "%1"=="" GOTO SWITCHES
MD MACHINE
COPY MAKEFILE.DOS MAKEFILE
COPY KERNL32L.FI KERNEL.FI
COPY ARCH\386\MACHINE.H MACHINE\MACHINE.H
COPY DOSCONF.H CSRC\CONFIG.H
COPY STARTUP.FS STARTUP.UNX
COPY STARTUP.DOS STARTUP.FS
COPY HISTORY.DOS HISTORY.FS
IF "%THREAD%"=="i" ECHO #ifndef INDIRECT_THREADED >>CSRC\CONFIG.H
IF "%THREAD%"=="i" ECHO #define INDIRECT_THREADED 1 >>CSRC\CONFIG.H
IF "%THREAD%"=="i" ECHO #endif >>CSRC\CONFIG.H
IF "%THREAD%"=="d" ECHO #ifndef DIRECT_THREADED >>CSRC\CONFIG.H
IF "%THREAD%"=="d" ECHO #define DIRECT_THREADED 1 >>CSRC\CONFIG.H
IF "%THREAD%"=="d" ECHO #endif >>CSRC\CONFIG.H
IF "%FREGS%"=="y" ECHO #ifndef FORCE_REG >>CSRC\CONFIG.H
IF "%FREGS%"=="y" ECHO #define FORCE_REG 1 >>CSRC\CONFIG.H
IF "%FREGS%"=="y" ECHO #endif >>CSRC\CONFIG.H
ECHO static char gforth_version[]="0.3.0"; >version.h1
ECHO : version-string s" 0.3.0" ; >version.fs1
