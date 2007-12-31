@ECHO OFF
REM Copyright (C) 1995,1996,1997,1998,2000,2003,2007 Free Software Foundation, Inc.
REM
REM This file is part of Gforth.
REM
REM Gforth is free software; you can redistribute it and/or
REM modify it under the terms of the GNU General Public License
REM as published by the Free Software Foundation, either version 3
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
COPY MAKEFILE.DOS MAKEFILE
CD ENGINE
COPY MAKEFILE.DOS MAKEFILE
CD ..
COPY KERNL32L.FI KERNEL.FI
COPY ENVOS.DOS ENVOS.FS
COPY DOSCONF.H ENGINE\CONFIG.H
IF "%THREAD%"=="i" ECHO #ifndef INDIRECT_THREADED >>ENGINE\CONFIG.H
IF "%THREAD%"=="i" ECHO #define INDIRECT_THREADED 1 >>ENGINE\CONFIG.H
IF "%THREAD%"=="i" ECHO #endif >>ENGINE\CONFIG.H
IF "%THREAD%"=="d" ECHO #ifndef DIRECT_THREADED >>ENGINE\CONFIG.H
IF "%THREAD%"=="d" ECHO #define DIRECT_THREADED 1 >>ENGINE\CONFIG.H
IF "%THREAD%"=="d" ECHO #endif >>ENGINE\CONFIG.H
IF "%FREGS%"=="y" ECHO #ifndef FORCE_REG >>ENGINE\CONFIG.H
IF "%FREGS%"=="y" ECHO #define FORCE_REG 1 >>ENGINE\CONFIG.H
IF "%FREGS%"=="y" ECHO #endif >>ENGINE\CONFIG.H
ECHO static char gforth_version[]="0.5.0"; >version.h1
ECHO : version-string s" 0.5.0" ; >version.fs1
COPY version.fs1 kernel\version.fs
