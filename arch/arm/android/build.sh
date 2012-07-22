#!/bin/bash
ant debug
ant release
jarsigner -verbose -sigalg MD5withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth.apk bernd