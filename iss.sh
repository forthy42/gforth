#!/bin/bash

#Copyright (C) 2000 Free Software Foundation, Inc.

#This file is part of Gforth.

#Gforth is free software; you can redistribute it and/or
#modify it under the terms of the GNU General Public License
#as published by the Free Software Foundation; either version 2
#of the License, or (at your option) any later version.

#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.#See the
#GNU General Public License for more details.

#You should have received a copy of the GNU General Public License
#along with this program; if not, write to the Free Software
#Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

# This is the horror shell script to create an automatic install for
# Windoze.
# Note that I use sed to create a setup file

# use iss.sh >iss.txt
# copy the resulting iss.txt to the location of your Windows installation
# of Gforth, and start the setup compiler there.

cat <<EOT
; This is the setup script for 4stack on Windows
; Setup program is Inno Setup

[Setup]
Bits=32
AppName=Gforth
AppVerName=gforth 0.5.0
AppCopyright=Copyright © 1995,1996,1997,1998,2000 Free Software Foundation
DefaultDirName=gforth
DefaultGroupName=Gforth
AllowNoIcons=1
LicenseFile=COPYING

[Dirs]
$(make distfiles -f Makedist | tr ' ' '\n' | (while read i; do
  while [ ! -z "$i" ]
  do
    if [ -d $i ]; then echo $i; fi
    if [ "${i%/*}" != "$i" ]; then i="${i%/*}"; else i=""; fi
  done
done) | sort -u | sed \
  -e 's:/:\\:g' \
  -e 's:^\(..*\)$:{app}\\\1:g')

[Files]
; Parameter quick reference:
;   "Source filename", "Dest. filename", Copy mode, Flags
"README.txt", "{app}\README.txt", copy_normal, flag_isreadme
"cygwin1.dll", "{app}\cygwin1.dll", copy_normal,
"gforth.fi", "{app}\gforth.fi", copy_normal,
$(make distfiles -f Makedist | tr ' ' '\n' | (while read i; do
  if [ ! -d $i ]; then echo $i; fi
done) | sed \
  -e 's:/:\\:g' \
  -e 's:^\(..*\)$:"\1", "{app}\\\1", copy_normal,:g')

[Icons]
; Parameter quick reference:
;   "Icon title", "File name", "Parameters", "Working dir (can leave blank)",
;   "Custom icon filename (leave blank to use the default icon)", Icon index
"Gforth", "{app}\gforth.exe", "", "{app}", , 0
"Gforth-fast", "{app}\gforth-fast.exe", "", "{app}", , 0

[Run]
"{app}\gforth.exe", "{app}\fixpath.fs {app} gforth-fast.exe",
"{app}\gforth.exe", "{app}\fixpath.fs {app} gforth-ditc.exe",
"{app}\gforth-fast.exe", "{app}\fixpath.fs {app} gforth.exe",

[Registry]
; Parameter quick reference:
;   "Root key", "Subkey", "Value name", Data type, "Data", Flags
HKCR, ".fs", "", STRING, "forthstream",
HKCR, ".fs", "Content Type", STRING, "application/forth",
HKCR, ".fb", "", STRING, "forthblock",
HKCR, ".fb", "Content Type", STRING, "application/forth-block",
HKCR, "forthstream", "", STRING, "Forth Source",
HKCR, "forthstream", "EditFlags", DWORD, "00000000",
HKCR, "forthstream\DefaultIcon", "", STRING, "{sys}\System32\shell32.dll,61"
HKCR, "forthstream\Shell", "", STRING, ""
HKCR, "forthstream\Shell\Open\command", "", STRING, "{app}\gforth.exe %1"
HKCR, "forthblock", "", STRING, "Forth Block",
HKCR, "forthblock", "EditFlags", DWORD, "00000000",
HKCR, "forthblock\DefaultIcon", "", STRING, "{sys}\System32\shell32.dll,61"
EOT
