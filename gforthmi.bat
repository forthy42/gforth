REM @ECHO OFF
REM Copyright (C) 1997-1998 Free Software Foundation, Inc.

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

if not x%1==x goto makeit
if not x%1==x--help" goto makeit
if not x%1==x-h" goto makeit
  echo usage: gforth-makeimage target-name [gforth-options]
  echo   environment: GFORTHD: the Gforth binary used (default: gforth-d)
  echo creates a relocatable image 'target-name'
  goto end
:makeit
set outfile=%1
shift
set GFORTHPAR=
:accupars
if x%1==x goto accudone
set GFORTHPAR=%GFORTHPAR% %1
shift
goto accupars
:accudone
if not x%GFORTHD%==x goto doit
set GFORTHD=gforth-d
:doit
echo savesystem tmp.fi1 bye >tmp.fs
%GFORTHD% -c -n %GFORTHPAR% tmp.fs
echo savesystem tmp.fi2 bye >tmp.fs
%GFORTHD% -c -o %GFORTHPAR% tmp.fs
echo comp-image tmp.fi1 tmp.fi2 %outfile% bye >tmp.fs
%GFORTHD% -i kernl32l.fi startup.fs  comp-i.fs tmp.fs
del tmp.fs
del tmp.fi1
del tmp.fi2
:end
