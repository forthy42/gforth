#!/bin/bash

#Copyright (C) 2000,2003,2006,2007 Free Software Foundation, Inc.

#This file is part of Gforth.

#Gforth is free software; you can redistribute it and/or
#modify it under the terms of the GNU General Public License
#as published by the Free Software Foundation, either version 3
#of the License, or (at your option) any later version.

#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.#See the
#GNU General Public License for more details.

#You should have received a copy of the GNU General Public License
#along with this program; if not, see http://www.gnu.org/licenses/.

# This is the horror shell script to create an automatic install for
# Windoze.
# Note that I use sed to create a setup file

# use iss.sh >gforth.iss
# copy the resulting *.iss to the location of your Windows installation
# of Gforth, and start the setup compiler there.

VERSION=$(cat version)

cat <<EOT
; This is the setup script for Gforth on Windows
; Setup program is Inno Setup

[Setup]
AppName=Gforth
AppVerName=Gforth $VERSION
AppCopyright=Copyright © 1995,1996,1997,1998,2000,2003,2006,2007,2008 Free Software Foundation
DefaultDirName={pf}\gforth
DefaultGroupName=Gforth
AllowNoIcons=1
InfoBeforeFile=COPYING
Compression=bzip
DisableStartupPrompt=yes
OutputBaseFilename=gforth-$VERSION

[Messages]
WizardInfoBefore=License Agreement
InfoBeforeLabel=Gforth is free software.
InfoBeforeClickLabel=You don't have to accept the GPL to run the program. You only have to accept this license if you want to modify, copy, or distribute this program.

[Components]
Name: "help"; Description: "HTML Documentation"; Types: full
Name: "info"; Description: "GNU info Documentation"; Types: full
Name: "print"; Description: "Postscript Documentation for printout"; Types: full
Name: "objects"; Description: "Compiler generated intermediate stuff"; Types: full

[Dirs]
$(make distfiles -f Makedist | tr ' ' '\n' | grep -v CVS | (while read i; do
  while [ ! -z "$i" ]
  do
    if [ -d $i ]; then echo $i; fi
    if [ "${i%/*}" != "$i" ]; then i="${i%/*}"; else i=""; fi
  done
done) | sort -u | sed \
  -e 's:/:\\:g' \
  -e 's,^\(..*\)$,Name: "{app}\\\1",g')
Name: "{app}\doc\gforth"
Name: "{app}\doc\vmgen"
Name: "{app}\lib\gforth\\$VERSION\libcc-named"

[Files]
; Parameter quick reference:
;   "Source filename", "Dest. filename", Copy mode, Flags
Source: "README.txt"; DestDir: "{app}"; Flags: isreadme
Source: "cygwin1.dll"; DestDir: "{app}"
Source: "sh.exe"; DestDir: "{app}"
Source: "cygintl-8.dll"; DestDir: "{app}"
Source: "cygiconv-2.dll"; DestDir: "{app}"
Source: "cygltdl-3.dll"; DestDir: "{app}"
Source: "cygreadline6.dll"; DestDir: "{app}"
Source: "cygncurses-8.dll"; DestDir: "{app}"
Source: "cygffi-2-00-beta.dll"; DestDir: "{app}"
Source: "gforth.fi"; DestDir: "{app}"
$(ls doc/gforth | sed -e 's:/:\\:g' -e 's,^\(..*\)$,Source: "doc\\gforth\\\1"; DestDir: "{app}\\doc\\gforth"; Components: help,g')
$(ls doc/vmgen | sed -e 's:/:\\:g' -e 's,^\(..*\)$,Source: "doc\\vmgen\\\1"; DestDir: "{app}\\doc\\vmgen"; Components: help,g')
$(ls lib/gforth/$VERSION/libcc-named | sed -e 's:/:\\:g' -e 's,^\(..*\)$,Source: "lib\\gforth\\'$VERSION'\\libcc-named\\\1"; DestDir: "{app}\\lib\\gforth\\'$VERSION'\\libcc-named",g')
$(make distfiles -f Makedist EXE=.exe | tr ' ' '\n' | grep -v engine.*exe | (while read i; do
  if [ ! -d $i ]; then echo $i; fi
done) | sed \
  -e 's:/:\\:g' \
  -e 's,^\(..*\)\\\([^\\]*\)$,Source: "\1\\\2"; DestDir: "{app}\\\1",g' \
  -e 's,^\([^\\]*\)$,Source: "\1"; DestDir: "{app}",g' \
  -e 's,^\(.*\.[oib]".*\),\1; Components: objects,g' \
  -e 's,^\(.*\.p\)s\(".*\),\1df\2; Components: print,g' \
  -e 's,^\(.*\.info.*".*\),\1; Components: info,g')

[Icons]
; Parameter quick reference:
;   "Icon title", "File name", "Parameters", "Working dir (can leave blank)",
;   "Custom icon filename (leave blank to use the default icon)", Icon index
Name: "{group}\Gforth"; Filename: "{app}\gforth.exe"; WorkingDir: "{app}"
Name: "{group}\Gforth-fast"; Filename: "{app}\gforth-fast.exe"; WorkingDir: "{app}"
Name: "{group}\Gforth-dict"; Filename: "{app}\gforth-dict.exe"; WorkingDir: "{app}"
Name: "{group}\Gforth-itc"; Filename: "{app}\gforth-itc.exe"; WorkingDir: "{app}"
Name: "{group}\Gforth-prof"; Filename: "{app}\gforth-prof.exe"; WorkingDir: "{app}"
Name: "{group}\Gforth Manual"; Filename: "{app}\doc\gforth\index.html"; WorkingDir: "{app}"; Components: help
Name: "{group}\Gforth Manual (PDF)"; Filename: "{app}\doc\gforth.pdf"; WorkingDir: "{app}"; Components: help
Name: "{group}\VMgen Manual"; Filename: "{app}\doc\vmgen\index.html"; WorkingDir: "{app}"; Components: help
Name: "{group}\Bash"; Filename: "{app}\sh.exe"; WorkingDir: "{app}"
Name: "{group}\Uninstall Gforth"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\gforth.exe"; WorkingDir: "{app}"; Parameters: "fixpath.fs {app} gforth-fast.exe"
Filename: "{app}\gforth.exe"; WorkingDir: "{app}"; Parameters: "fixpath.fs {app} gforth-ditc.exe"
Filename: "{app}\gforth.exe"; WorkingDir: "{app}"; Parameters: "fixpath.fs {app} gforth-itc.exe"
Filename: "{app}\gforth.exe"; WorkingDir: "{app}"; Parameters: "fixpath.fs {app} gforth-prof.exe"
Filename: "{app}\gforth-fast.exe"; WorkingDir: "{app}"; Parameters: "fixpath.fs {app} gforth.exe"

;[Registry]
;registry commented out
; WorkingDir: "{app}"; Parameter quick reference:
;   "Root key", "Subkey", "Value name", Data type, "Data", Flags
;HKCR, ".fs"; STRING, "forthstream",
;HKCR, ".fs", "Content Type", STRING, "application/forth",
;HKCR, ".fb"; STRING, "forthblock",
;HKCR, ".fb", "Content Type", STRING, "application/forth-block",
;HKCR, "forthstream"; STRING, "Forth Source",
;HKCR, "forthstream", "EditFlags", DWORD, "00000000",
;HKCR, "forthstream\DefaultIcon"; STRING, "{sys}\System32\shell32.dll,61"
;HKCR, "forthstream\Shell"; STRING, ""
;HKCR, "forthstream\Shell\Open\command"; STRING, "{app}\gforth.exe %1"
;HKCR, "forthblock"; STRING, "Forth Block",
;HKCR, "forthblock", "EditFlags", DWORD, "00000000",
;HKCR, "forthblock\DefaultIcon"; STRING, "{sys}\System32\shell32.dll,61"
EOT

sed -e 's/$/\r/' <README >README.txt
