./gforthmi.sh || exit 1
./gforth fixpath.fs gforth-fast.exe || exit 1
./gforth fixpath.fs gforth-ditc.exe || exit 1
./gforth fixpath.fs gforth-itc.exe || exit 1
./gforth-fast fixpath.fs gforth.exe || exit 1
printf "\e[0;32;49mGforth postinstall succeeded, press key to exit\e[0;39;49m\n"
read
