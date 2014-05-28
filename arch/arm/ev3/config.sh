echo -e "\033[35;48m" 
echo
echo
echo "Starte config.sh" 
echo "You should configure with"
echo "./configure --host=arm-none-linux-gnueabi --with-cross=ev3 --with-ditc=gforth-ditc-x32"
echo -e "\033[0m"
skipcode=".skip 4\n.skip 4\n.skip 4\n.skip 4"
kernel_fi=kernl32l.fi
ac_cv_sizeof_void_p=4
ac_cv_sizeof_char_p=4
ac_cv_sizeof_char=1
ac_cv_sizeof_short=2
ac_cv_sizeof_int=4
ac_cv_sizeof_long=4
ac_cv_sizeof_long_long=8
ac_cv_sizeof_intptr_t=4
ac_cv_sizeof_int128_t=0
ac_cv_c_bigendian=no
ac_cv_func_memcmp_working=yes
ac_cv_func_memmove=yes
ac_cv_file___arch_arm_asm_fs=yes
ac_cv_file___arch_arm_disasm_fs=yes
ac_cv_func_dlopen=yes
ac_export_dynamic=yes
asm_fs=arch/arm/asm.fs
disasm_fs=arch/arm/disasm.fs
GNU_LIBTOOL=arm-none-linux-gnueabi-libtool
CROSS_PREFIX=arm-none-linux-gnueabi-
CFLAGS="-mtune=arm926ej-s -march=armv5te"
LDLAGS="-mtune=arm926ej-s -march=armv5te"
EC_MODE="false"
NO_EC=""
EC=""
engine2='engine2$(OPT).o'
engine_fast2='engine-fast2$(OPT).o'
no_dynamic=""
image_i=""
LIBS="-ldl"
echo "Ich wurde benutzt! " >erfolg.txt
