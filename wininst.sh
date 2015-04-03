#!/bin/bash
./gforthmi.sh || exit 1
./gforth fixpath.fs gforth-fast.exe || exit 1
./gforth fixpath.fs gforth-ditc.exe || exit 1
./gforth fixpath.fs gforth-itc.exe || exit 1
./gforth-fast fixpath.fs gforth.exe || exit 1
echo "Everything fine, press key to exit"
read
