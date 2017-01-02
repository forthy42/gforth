# Gforth README
Gforth is a fast and portable implementation of the ANS Forth
language. It works nicely with the Emacs editor, offers some nice
features such as input completion and history, backtraces, a
decompiler and a powerful locals facility, and it has a comprehensive
manual. Gforth combines traditional implementation techniques with
newer techniques for portability and performance: its inner
interpreter is direct threaded with several optimizations, but you can
also use a traditional-style indirect threaded interpreter.  Gforth is
distributed under the GNU General Public license (see COPYING).

## Supported Systems
Gforth runs under GNU, BSD, and similar systems, MS Windows and MacOS X
and should not be hard to port to other systems supported by GCC. This
version has been tested successfully on the following platforms:

- GNU/Linux
  - amd64
  - arm64
  - armel
  - armhf
  - i386
  - mips
  - mipsel
  - powerpc
- Android/Linux
  - amd64
  - arm64
  - arm
  - i386
  - mips
- Gforth EC(embedded): r8c, 4stack, misc, 8086
- Windows
  - amd64
  - i386
- MacOS
  - amd64
  - i386

## Installation
Read INSTALL for installation instructions from tarball,
or INSTALL.md for from git,
or INSTALL.BINDIST if you have
a binary package distributed as .tar.xz file.
If you received a self-installing executable,
just run it and follow the instructions.

To start the system, just say `gforth` (after installing it).

## Download
You can find new versions of Gforth at 
[gforth.org/gforth](https://gforth.org/gforth)
or at
[ftp://ftp.gnu.org/gnu/gforth/](ftp://ftp.gnu.org/gnu/gforth/)

## Files
On popular request, here are the meanings of unusual file extensions:

*.fs        Forth stream source file (include with "include <file>" from within
            gforth, or start with "gforth <file1> <file2> ...")
*.fi        Forth image files (start with "gforth -i <image file>")
*.fb        Forth blocks file (load with "use <block file> 1 load")
*.i         C include files
*.texi.in   documenation source
*TAGS       etags files

A number of Forth source files are included in this package that are
not necessary for building Gforth. Not all of them are mentioned in
the rest of the documentation, so here's a short overview:

Add-ons:
code.fs random.fs more.fs ansi.fs colorize.fs
oof.fs oofsampl.fs objects.fs blocked.fb tasker.fs

Utilities:
    ans-report.fs etags.fs glosgen.fs filedump.fs

Games:
    tt.fs sokoban.fs

Test programs (for testing Forth systems):
    test/*.fs

Benchmarks:
    bubble.fs siev.fs matrix.fs fib.fs

ANS Forth implementations of Gforth extensions:
    compat/*.fs

C-Bindings:
    unix/*.fs

## Support
For discussions about Gforth, use the Usenet newsgroup
comp.lang.forth.  If you prefer not to post on Usenet, there is also a
mailing list: gforth@gnu.org.  You have to subsribe to post there.
You can subscribe through
<http://lists.gnu.org/mailman/listinfo/gforth>.  The list is archived
at <http://lists.gnu.org/pipermail/gforth/>.

You can also report bugs through these channels, or you can report
them through our bug database:

https://savannah.gnu.org/bugs/?func=addbug&group=gforth

- anton
anton@mips.complang.tuwien.ac.at
http://www.complang.tuwien.ac.at/anton/home.html
-----
Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2006,2007,2008,2009,2016 Free Software Foundation, Inc.

This file is part of Gforth.

Gforth is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.#See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see http://www.gnu.org/licenses/.
