@ECHO OFF
REM Copyright (C) 1997 Free Software Foundation, Inc.

REM This file is part of Gforth.

REM Gforth is free software; you can redistribute it and/or
REM modify it under the terms of the GNU General Public License
REM as published by the Free Software Foundation; either version 2
REM of the License, or (at your option) any later version.

REM This program is distributed in the hope that it will be useful,
REM but WITHOUT ANY WARRANTY; without even the implied warranty of
REM MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.REM See the
REM GNU General Public License for more details.

REM You should have received a copy of the GNU General Public License
REM along with this program; if not, write to the Free Software
REM Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

if not "%1"=="" goto makeit
if not "%1"=="--help" goto makeit
if not "%1"=="-h" goto makeit
  echo usage: gforth-makeimage target-name [gforth-options]
  echo   environment: GFORTHD: the Gforth binary used (default: gforth-ditc)
  echo creates a relocatable image 'target-name'
  goto end
:makeit
set outfile=%1
shift
if not "%GFORTHD%"=="" goto doit
set GFORTHD=gforth-ditc
:doit
%GFORTHD% --clear-dictionary --no-offset-im %1 %2 %3 %4 %5 -e "savesystem tmp.fi1 bye"
%GFORTHD% --clear-dictionary --offset-image %1 %2 %3 %4 %5 -e "savesystem tmp.fi2 bye"
%GFORTHD% -i kernel.fi startup.fs  comp-image.fs -e "comp-image tmp.fi1 tmp.fi2 %outfile% bye"
del tmp.fi1
del tmp.fi2
:end
