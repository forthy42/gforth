# no interpreter here
function fail {
    printf '\e[0;31;49mGforth postinstall failed!\e[0m\n'
    exit 1
}
printf "\e[0;32;49mGforth postinstall started\e[0m\n"
./gforthmi.sh || fail
printf "\e[0;32;49mGforth image generated, now fixing paths\e[0m\n"
./gforth fixpath.fs gforth-fast.exe "$1" || fail
./gforth fixpath.fs gforth-ditc.exe "$1" || fail
./gforth fixpath.fs gforth-itc.exe "$1" || fail
./gforth-fast fixpath.fs gforth.exe "$1" || fail
printf "\e[0;32;49mGforth postinstall succeeded, press key to exit\e[0m\n"
./gforth -e "key bye"
