#!/bin/bash
#Copyright (C) 2016 Free Software Foundation, Inc.

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
#along with this program. If not, see http://www.gnu.org/licenses/.

function extra_apps {
    for i in $EXTRADIRS
    do
	test -f $i/AndroidManifest/apps && cat $i/AndroidManifest/apps
    done
}

function extra_perms {
    for i in $EXTRADIRS
    do
	test -f $i/AndroidManifest/perms && cat $i/AndroidManifest/perms
    done
}

function extra_features {
    for i in $EXTRADIRS
    do
	test -f $i/AndroidManifest/app && cat $i/AndroidManifest/features
    done
}

. build.local

EXTRAS=""
EXTRADIRS=""
for i in $@
do
    EXTRAS+=" -with-extras=$i"
    EXTRADIRS+=" $i"
done

ENGINES="gforth-fast gforth-itc"

GFORTH_VERSION=$($GFORTH_DITC --version 2>&1 | cut -f2 -d' ')
APP_VERSION=$[$(cat ~/.app-version)+1]
echo $APP_VERSION >~/.app-version

SRC=../../..
LIBCCNAMED=lib/$($GFORTH_DITC --version 2>&1 | cut -f1-2 -d ' ' | tr ' ' '/')/$machine/libcc-named/.libs

(cd $SRC
 rm -rf debian/sdcard
 make setup-debdist || exit 1) || exit 1

mkdir -p $SRC/debian/sdcard/gforth/$machine/gforth/site-forth
mkdir -p res/raw
(cd $SRC/debian/sdcard
 mkdir -p gforth/home gforth/site-forth
 gforth archive.fs gforth/home/ gforth/site-forth/ $(find gforth/$GFORTH_VERSION -type f) $(find gforth/site-forth -type f)) | gzip -9 >res/raw/gforth
(cd $SRC/debian/sdcard
 rm gforth/$machine/lib*
 rm -rf gforth/$machine/gforth/$GFORTH_VERSION/$machine/libcc-named
 gforth archive.fs $machine/gforth/site-forth/ $(find gforth/$machine/gforth -type f)) | gzip -9 >$LIBS/libgforth-${machine}gz.so

SHA256=$(sha256sum res/raw/gforth | cut -f1 -d' ')
SHA256ARCH=$(sha256sum $LIBS/libgforth-${machine}gz.so | cut -f1 -d' ')

for i in $ENGINES
do
    sed -e "s/sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha2/$SHA256/" -e "s/sha256archsum-sha256archsum-sha256archsum-sha256archsum-sha256ar/$SHA256ARCH/" $SRC/engine/.libs/lib$i.so >$LIBS/lib$i.so
done

for i in $EXTRADIRS
do
    test -d $i/res && (cd $i/res; tar cf - .) | (cd res; tar xf -)
    test -d $i/src && (cd $i/src; tar cf - .) | (cd src; tar xf -)
done

#ant debug
ant release || exit 1
cp bin/Gforth-release.apk bin/Gforth.apk
cp bin/Gforth-release.apk bin/Gforth-$(date +%Y%m%d).apk
