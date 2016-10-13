#configure with
#./configure --host=aarch64-darwin --with-cross=ios --prefix=/usr --with-ditc=gforth-ditc CC="$(xcrun --sdk iphoneos --find clang) -isysroot $(xcrun --sdk iphoneos --show-sdk-path) -arch arm64 -std=gnu89 -Wno-trigraphs"
#and finally create an apk in this directory
#./build.sh
echo "Config for arm64-ios"
skipcode=".skip 16"
kernel_fi=kernl64l.fi
ac_cv_sizeof_void_p=8
ac_cv_sizeof_char_p=8
ac_cv_sizeof_char=1
ac_cv_sizeof_short=2
ac_cv_sizeof_int=4
ac_cv_sizeof_long=8
ac_cv_sizeof_long_long=8
ac_cv_sizeof_intptr_t=8
ac_cv_sizeof_int128_t=16
ac_cv_c_bigendian=no
ac_cv_func_memcmp_working=yes
ac_cv_func_memmove=yes
#ac_cv_func_getpagesize=no
ac_cv_file___arch_arm64_asm_fs=yes
ac_cv_file___arch_arm64_disasm_fs=yes
ac_cv_func_dlopen=yes
HOSTCC="clang -m64 -D__arm64__"
GNU_LIBTOOL="/Applications/Xcode.app/Contents/Developer/usr/bin/arm64-ios-libtool"
build_libcc_named=build-libcc-named
#extraccdir=/data/data/gnu.gforth/lib
asm_fs=arch/arm64/asm.fs
disasm_fs=arch/arm64/disasm.fs
EC_MODE="false"
NO_EC=""
EC=""
STACK_CACHE_REGS="1"
engine2='engine2$(OPT).o'
engine_fast2='engine-fast2$(OPT).o'
no_dynamic=""
image_i=""
LIBS=""
signals_o="io.o signals.o $XLIBS"

