#!/bin/bash

#Authors: Bernd Paysan, Anton Ertl
#Copyright (C) 2000,2003,2006,2007,2009,2011,2012,2016,2017,2018 Free Software Foundation, Inc.

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

VERSION=$(./gforth --version 2>&1 | cut -f2 -d' ')
machine=$(./gforth --version 2>&1 | cut -f3 -d' ')
SF=$(./gforth -e 'cell 8 = [IF] ." 64" [THEN] bye')
CYGWIN=cygwin$SF
CYGWIN64=cygwin64
CYGWIN32=cygwin
X64=$(./gforth -e 'cell 8 = [IF] ." x64" [THEN] bye')

fsis=$(for i in unix/*.i; do echo -n "^${i%.i}\.fs\$|"; done)
fsis="'"${fsis%|}"'"

ln -fs /cygdrive/c/cygwin$(pwd)/lib/gforth/$VERSION/386 lib/gforth/$VERSION/

for m in amd64 386
do
    for i in lib/gforth/$VERSION/$m/libcc-named/*.la
    do
	sed "s/dependency_libs='.*'/dependency_libs=''/g" <$i >$i+
	mv $i+ $i
    done
done

make doc pdf install.TAGS >&2

#cp /bin/cygwin1.dll cygwin-copy.dll
#./gforth fixpath.fs cygwin-copy.dll "/bin/cygwin-console-helper.exe" "./cygwin-console-helper.exe" 1>&2
#./gforth fixpath.fs cygwin-copy.dll "/usr/bin/sh" "./sh" 1>&2
#./gforth fixpath.fs cygwin-copy.dll "/bin/sh" "./sh" 1>&2
#./gforth fixpath.fs cygwin-copy.dll "/bin/sh" "./sh" 1>&2

cat <<EOF
; This is the setup script for Gforth on Windows
; Setup program is Inno Setup

[Setup]
AppName=Gforth
AppVersion=$VERSION
AppCopyright=Copyright © 1995-2019 Free Software Foundation
DefaultDirName={pf}\gforth
DefaultGroupName=Gforth
AllowNoIcons=1
InfoBeforeFile=COPYING
Compression=lzma
DisableStartupPrompt=yes
ChangesEnvironment=yes
OutputBaseFilename=gforth-$VERSION
AppPublisher=Free Software Foundation, Gforth team
AppPublisherURL=https://gforth.org/
SignTool=sha1
SignTool=sha256
; add the following sign tools:
; sha1=signtool sign /a /fd sha1 /tr http://timestamp.entrust.net/TSS/RFC3161sha2TS /td sha1 $f
; sha256=signtool sign /a /as /fd sha256 /tr http://timestamp.entrust.net/TSS/RFC3161sha2TS /td sha256 $f
SetupIconFile=gforth.ico
UninstallDisplayIcon={app}\\gforth.ico
ArchitecturesInstallIn64BitMode=$X64

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
$(make distfiles -f Makedist | tr ' ' '\n' | grep -v CVS | grep -v android | (while read i; do
  while [ ! -z "$i" ]
  do
    if [ -d $i ]; then echo $i; fi
    if [ "${i%/*}" != "$i" ]; then i="${i%/*}"; else i=""; fi
  done
done) | sort -u | sed \
  -e 's:/:\\:g' \
  -e 's,^\(..*\)$,Name: "{app}\\\1",g')
Name: "{app}\\doc\\gforth"
Name: "{app}\\doc\\vmgen"
Name: "{app}\\lib\\gforth\\$VERSION\\amd64\\libcc-named"; Check: Is64BitInstallMode
Name: "{app}\\lib\\gforth\\$VERSION\\386\\libcc-named"; Check: not Is64BitInstallMode
Name: "{app}\\include\\gforth\\$VERSION\\amd64"; Check: Is64BitInstallMode
Name: "{app}\\include\\gforth\\$VERSION\\386"; Check: not Is64BitInstallMode
Name: "{app}\\..\\bin"
Name: "{app}\\..\\tmp"; Permissions: users-modify

[Files]
; Parameter quick reference:
;   "Source filename", "Dest. filename", Copy mode, Flags
Source: "README.txt"; DestDir: "{app}"; Flags: isreadme
Source: "C:\\$CYGWIN64\\bin\\sh.exe"; DestDir: "{app}\\..\\bin"; Check: Is64BitInstallMode
Source: "C:\\$CYGWIN64\\bin\\nproc.exe"; DestDir: "{app}\\..\\bin"; Check: Is64BitInstallMode
Source: "C:\\$CYGWIN64\\bin\\cygwin-console-helper.exe"; DestDir: "{app}\\..\\bin"; Check: Is64BitInstallMode
Source: "C:\\$CYGWIN64\\bin\\cygwin1.dll"; DestDir: "{app}\\..\\bin"; Check: Is64BitInstallMode
Source: "C:\\$CYGWIN64\\bin\\cyggcc_s-seh-1.dll"; DestDir: "{app}\\..\\bin"; Check: Is64BitInstallMode
Source: "C:\\$CYGWIN64\\bin\\cygintl-8.dll"; DestDir: "{app}\\..\\bin"; Check: Is64BitInstallMode
Source: "C:\\$CYGWIN64\\bin\\cygiconv-2.dll"; DestDir: "{app}\\..\\bin"; Check: Is64BitInstallMode
Source: "C:\\$CYGWIN64\\bin\\cygreadline7.dll"; DestDir: "{app}\\..\\bin"; Check: Is64BitInstallMode
Source: "C:\\$CYGWIN64\\bin\\cygncursesw-10.dll"; DestDir: "{app}\\..\\bin"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\cyggcc_s-seh-1.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\cygwin1.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\cygintl-8.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\cygiconv-2.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\cygltdl-7.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\cygreadline7.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\cygncursesw-10.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\cygffi-6.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\mintty.exe"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\run.exe"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN64\\bin\\env.exe"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\sh.exe"; DestDir: "{app}\\..\\bin"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\nproc.exe"; DestDir: "{app}\\..\\bin"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygwin-console-helper.exe"; DestDir: "{app}\\..\\bin"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygwin1.dll"; DestDir: "{app}\\..\\bin"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cyggcc_s-1.dll"; DestDir: "{app}\\..\\bin"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygintl-8.dll"; DestDir: "{app}\\..\\bin"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygiconv-2.dll"; DestDir: "{app}\\..\\bin"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygreadline7.dll"; DestDir: "{app}\\..\\bin"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygncursesw-10.dll"; DestDir: "{app}\\..\\bin"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cyggcc_s-1.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygwin1.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygintl-8.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygiconv-2.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygltdl-7.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygreadline7.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygncursesw-10.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\cygffi-6.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\mintty.exe"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\run.exe"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "c:\\$CYGWIN32\\bin\\env.exe"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "gforthmi.sh"; DestDir: "{app}"
$(ls doc/gforth | sed -e 's,^\(..*\)$,Source: "doc\\gforth\\\1"; DestDir: "{app}\\doc\\gforth"; Components: help,g' -e 's:/:\\:g' )
$(ls doc/vmgen | sed -e 's,^\(..*\)$,Source: "doc\\vmgen\\\1"; DestDir: "{app}\\doc\\vmgen"; Components: help,g' -e 's:/:\\:g' )
$(ls lib/gforth/$VERSION/amd64/libcc-named/*.la | sed -e 's,^\(..*\)$,Source: "\1"; DestDir: "{app}\\lib\\gforth\\'$VERSION'\\amd64\\libcc-named"; Check: Is64BitInstallMode,g' -e 's:/:\\:g' )
$(ls lib/gforth/$VERSION/amd64/libcc-named/.libs/*.dll | sed -e 's,^\(..*\)$,Source: "\1"; DestDir: "{app}\\lib\\gforth\\'$VERSION'\\amd64\\libcc-named\\.libs"; Check: Is64BitInstallMode,g' -e 's:/:\\:g')
$(ls lib/gforth/$VERSION/386/libcc-named/*.la | sed -e 's,^\(..*\)$,Source: "C:\\cygwin'$(pwd)'\\\1"; DestDir: "{app}\\lib\\gforth\\'$VERSION'\\386\\libcc-named"; Check: not Is64BitInstallMode,g' -e 's:/:\\:g')
$(ls lib/gforth/$VERSION/386/libcc-named/.libs/*.dll | sed -e 's,^\(..*\)$,Source: "C:\\cygwin'$(pwd)'\\\1"; DestDir: "{app}\\lib\\gforth\\'$VERSION'\\386\\libcc-named\.libs"; Check: not Is64BitInstallMode,g' -e 's:/:\\:g')
$(make distfiles -f Makedist EXE=.exe | tr ' ' '\n' | grep -v engine.*exe | grep -v -E $fsis | (while read i; do
  if [ -d $i ]; then echo -n ""; else if [ -L $i ]; then echo "$i -> $(readlink $i)"; else echo $i; fi; fi
done) | sed \
  -e 's:/:\\:g' \
  -e 's,^\(..*\)\\\([^\\]*\) -> \([^ ]*\)$,Source: "\1\\\3"; DestDir: "{app}\\\1",g' \
  -e 's,^\([^\\ ]*\) -> \([^ ]*\)$,Source: "\2"; DestDir: "{app}",g' \
  -e 's,^\([^ ][^ ]*\)\\\([^\\ ]*\)$,Source: "\1\\\2"; DestDir: "{app}\\\1",g' \
  -e 's,^\([^\\ ]*\)$,Source: "\1"; DestDir: "{app}",g' \
  -e 's,^\(.*\.[oibc]".*\),\1; Components: objects,g' \
  -e 's,^\(.*\.p\)s\(".*\),\1df\2; Components: print,g' \
  -e 's,^\(.*\.info.*".*\),\1; Components: info,g')
$(make distfiles -f Makedist EXE=.exe | tr ' ' '\n' | grep -v engine.*exe | grep -E $fsis | (while read i; do
  if [ ! -d $i ]; then echo $i; fi
done) | sed \
  -e 's,^\(..*\)/\([^\\]*\)$,Source: "\1/\2"; DestDir: "{app}/\1"; Check: Is64BitInstallMode,g' \
  -e 's:/:\\:g')
$(make distfiles -f Makedist EXE=.exe | tr ' ' '\n' | grep -v engine.*exe | grep -E $fsis | (while read i; do
  if [ ! -d $i ]; then echo $i; fi
done) | sed \
  -e 's,^\(..*\)/\([^\\]*\)$,Source: "C:\\cygwin'$(pwd)'/\1/\2"; DestDir: "{app}/\1"; Check: not Is64BitInstallMode,g' \
  -e 's:/:\\:g')

[Icons]
; Parameter quick reference:
;   "Icon title", "File name", "Parameters", "Working dir (can leave blank)",
;   "Custom icon filename (leave blank to use the default icon)", Icon index
Name: "{group}\Gforth"; Filename: "{app}\\run.exe"; Parameters: "./env HOME=%HOMEDRIVE%%HOMEPATH% ./mintty ./gforth"; WorkingDir: "{app}"; IconFilename: "{app}\\gforth.ico"
Name: "{group}\Gforth-fast"; Filename: "{app}\\run.exe"; Parameters: "./env HOME='%HOMEDRIVE%%HOMEPATH%' ./mintty ./gforth-fast"; WorkingDir: "{app}"; IconFilename: "{app}\\gforth.ico"
Name: "{group}\Gforth-ditc"; Filename: "{app}\\run.exe"; Parameters: "./env HOME='%HOMEDRIVE%%HOMEPATH%' ./mintty ./gforth-ditc"; WorkingDir: "{app}"; IconFilename: "{app}\\gforth.ico"
Name: "{group}\Gforth-itc"; Filename: "{app}\\run.exe"; Parameters: "./env HOME='%HOMEDRIVE%%HOMEPATH%' ./mintty ./gforth-itc"; WorkingDir: "{app}"; IconFilename: "{app}\\gforth.ico"
Name: "{group}\Gforth Manual"; Filename: "{app}\doc\gforth\index.html"; WorkingDir: "{app}"; Components: help
Name: "{group}\Gforth Manual (PDF)"; Filename: "{app}\doc\gforth.pdf"; WorkingDir: "{app}"; Components: help
Name: "{group}\VMgen Manual"; Filename: "{app}\doc\vmgen\index.html"; WorkingDir: "{app}"; Components: help
Name: "{group}\Bash"; Filename: "{app}\\run.exe"; Parameters: "./env HOME='%HOMEDRIVE%%HOMEPATH%' ./mintty /bin/sh"; WorkingDir: "{app}"; Flags: runminimized
Name: "{group}\Uninstall Gforth"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\\..\\bin\\sh.exe"; WorkingDir: "{app}"; Parameters: "-c ""./wininst.sh '{app}' || (printf '\e[0;31;49mAn error occured, pess return to quit'; read)"""

[UninstallDelete]
Type: files; Name: "{app}\gforth.fi"
Type: files; Name: "{app}\temp-image.fi1"
Type: files; Name: "{app}\temp-image.fi2"

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

[Tasks]
Name: modifypath; Description: Add application directory to your environmental path; Flags: unchecked

[Code]
// Utility functions for Inno Setup
//   used to add/remove programs from the windows firewall rules
// Code originally from http://news.jrsoftware.org/news/innosetup/msg43799.html

const
    ModPathName = 'modifypath';
    ModPathType = 'user';
    NET_FW_SCOPE_ALL = 0;
    NET_FW_IP_VERSION_ANY = 2;

function ModPathDir(): TArrayOfString;
begin
    setArrayLength(Result, 1)
    Result[0] := ExpandConstant('{app}');
end;
#include "modpath.iss"
#include "firewall.iss"

// event called at install
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep=ssPostInstall then begin
     SetFirewallException('Gforth', ExpandConstant('{app}')+'\gforth.exe');
     SetFirewallException('Gforth', ExpandConstant('{app}')+'\gforth-fast.exe');
     SetFirewallException('Gforth', ExpandConstant('{app}')+'\gforth-itc.exe');
     SetFirewallException('Gforth', ExpandConstant('{app}')+'\gforth-ditc.exe');
     CurStepChangedPath();
  end;
end;

// event called at uninstall
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then begin
     CurUninstallStepChangedPath();
  end;
  if CurUninstallStep=usPostUninstall then begin
     RemoveFirewallException(ExpandConstant('{app}')+'\gforth.exe');
     RemoveFirewallException(ExpandConstant('{app}')+'\gforth-fast.exe');
     RemoveFirewallException(ExpandConstant('{app}')+'\gforth-itc.exe');
     RemoveFirewallException(ExpandConstant('{app}')+'\gforth-ditc.exe');
  end;
end;

EOF

sed -e 's/$/\r/' <README >README.txt
