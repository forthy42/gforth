/*
Copyright 1997,2000,2003,2007 Free Software Foundation, Inc.

This file is part of Gforth.

Gforth is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, see http://www.gnu.org/licenses/.
*/
parse arg args

if args \== "" then do
    parse var args arg args
    select
        when arg = "--help" then dohelp=1
        when arg = "-h" then dohelp=1
        otherwise dohelp=0
    end
  end
else
    dohelp=1

if dohelp then do
  say "usage: gforth-makeimage target-name [gforth-options]"
  say "  environment: GFORTHD: the Gforth binary used (default: gforth-ditc)"
  say "creates a relocatable image 'target-name'"
  exit 1
end

'gforth-ditc' '-p' '.' '--clear-dictionary' '--no-offset-im' args '-e' '"savesystem tmp.fi1 bye"'
'gforth-ditc' '-p' '.' '--clear-dictionary' '--offset-image' args '-e' '"savesystem tmp.fi2 bye"'
'gforth-ditc' '-p' '.' '-i' 'kernel.fi' 'startup.fs' 'comp-i.fs' '-e' '"comp-image tmp.fi1 tmp.fi2 'arg' bye"'
del tmp.fi1
del tmp.fi2
