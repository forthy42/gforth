dnl Process this file with autoconf to produce a configure script.

#Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014,2015 Free Software Foundation, Inc.

#This file is part of Gforth.

#Gforth is free software; you can redistribute it and/or
#modify it under the terms of the GNU General Public License
#as published by the Free Software Foundation, either version 3
#of the License, or (at your option) any later version.

#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
#GNU General Public License for more details.

#You should have received a copy of the GNU General Public License
#along with this program. If not, see http://www.gnu.org/licenses/.


#We use some automake macros here,
#but don't use automake for creating Makefile.in
AC_INIT([gforth],[0.7.9_20160825],[https://savannah.gnu.org/bugs/?func=addbug&group=gforth])
AC_PREREQ(2.54)
AC_CONFIG_MACRO_DIR([m4])
#snapshots have numbers major.minor.release_YYYYMMDD
#note that lexicographic ordering must be heeded.
#I.e., 0.4.1_YYYYMMDD must not exist before 0.4.1!
UPDATED="August 25, 2016"
AC_SUBST(UPDATED)
AC_CONFIG_HEADERS(engine/config.h)
AC_USE_SYSTEM_EXTENSIONS
LT_INIT([disable-static])

#remnants from having ltdl as convenience library
LTDLDEPS=""
LTDLINCL=""
LIBLTDL=""
LTDL_LDLIBS=""
AC_SUBST(LTDLDEPS)
AC_SUBST(LTDLINCL)
AC_SUBST(LIBLTDL)
AC_SUBST(LTDL_LDLIBS)

#suppress the "-g -O2" default
test "$CFLAGS" || CFLAGS=-O2

AC_ARG_ENABLE(force-cdiv,
	AC_HELP_STRING([--enable-force-cdiv],
		       [  Use the native C division - symmetric - instead of
		          floored division (default disabled).]),
	,enable_force_cdiv=no)
test "$enable_force_cdiv" = "no"||
  AC_DEFINE(FORCE_CDIV,,[Define if you want to use explicit symmetric division for better performance])

AC_SUBST(PROFEXES)
AC_SUBST(PROFOBJS)
AC_ARG_ENABLE(prof,
	AC_HELP_STRING([--enable-prof],
			[ Build gforth-prof, which outputs frequently occuring
                          sequences of primitives.]),
	,enable_prof=no)
if test "$enable_prof" != "no"; then
  PROFEXES='gforth-prof$(OPT)$(EXE)'; PROFOBJS='engine-prof$(OPT).o main-prof$(OPT).o profile$(OPT).o'
fi

AC_ARG_WITH(debug,
[  --with-debug		  specifies option -g to compile with debug info
  --without-debug	  omits the -g switch and creates smaller images on
			  machines where "strip" has problems with gcc style
			  debugging informations.],
if test "$withval" = "yes"; then DEBUGFLAG=-g; fi)

NO_CHECK=""
NO_FAIL=""
COMMENT="#"

AC_ARG_WITH(check,
[  --with-check		  enables sanity check during build (default)
  --without-check	  disables sanity check during build],
if test "$withval" = "no"; then NO_CHECK="$COMMENT"; fi)

GCC_LD="\$(GCC)"
EC_MODE="false"
NO_EC=""
EC=""
engine2='engine2$(OPT).o'
engine_fast2='engine-fast2$(OPT).o'
no_dynamic=""
image_i=""
signals_o="io.o signals.o"

AC_CANONICAL_HOST
AC_ARG_WITH(ec,
	AC_HELP_STRING([--with-ec=<arch>],
			[  Build gforth for systems without OS.]),
[if test "$withval" = "no"; then
  echo "defining hosted system"
else
  echo "defining standalone system (${withval})"
  AC_DEFINE(STANDALONE,,[Define if you want a Gforth without OS])
  EC_MODE="true"
  EC="-ec"
  NO_EC="#"
  NO_CHECK="true"
  NO_CHECKX="#"
  NO_CROSS="#"
  engine2=""
  engine_fast2=""
  no_dynamic="-DNO_DYNAMIC"
  image_i="image.i"
  if test "$withval" != "yes"; then
    signals_o="io-${withval}.o"
  else
    signals_o="io.o"
  fi
  cpu=$host_cpu
  case "$cpu" in
     i*86)
	cpu=386
     ;;
     x86_64)
        cpu=amd64
     ;;
     arm64|aarch64)
	cpu=arm64
     ;;
  esac

  if test -f "$srcdir/arch/${cpu}/${withval}/config.sh"; then
    source "$srcdir/arch/${cpu}/${withval}/config.sh"
  fi
  GCC_PATH=$(which $CC)
  LIB_PATH=${GCC_PATH%/*/*}
  GCC_LD="\$(LD)"
  platform=${withval}
fi])

libbench='#'
AC_ARG_ENABLE(lib,
	AC_HELP_STRING([--enable-lib],
		       [  Compile Gforth as shared library (default enabled).]),
	,enable_lib=yes)

AC_ARG_WITH(cross,
	AC_HELP_STRING([--with-cross=<arch>],
			[  Build gforth using a cross compiler.]),
[if test "$withval" = "no"; then
  echo "defining hosted system"
else
  echo "defining cross compiled system (${withval})"
  cpu=$host_cpu
  case "$cpu" in
     i*86)
	cpu=386
     ;;
     x86_64)
        cpu=amd64
     ;;
     arm64|aarch64)
	cpu=arm64
     ;;
  esac

  if test -f "$srcdir/arch/${cpu}/${withval}/config.sh"; then
    source "$srcdir/arch/${cpu}/${withval}/config.sh"
  fi
  GCC_PATH=$(which $CC)
  LIB_PATH=${GCC_PATH%/*/*}
  platform=${withval}
  NO_EC=""
  NO_CHECK="true"
  NO_CHECKX="#"
  NO_CROSS="#"
  NO_FAIL="-"
fi])

EXTRAS=""
AC_ARG_WITH(extras,
	AC_HELP_STRING([--with-extras=<path>],
			[  add extra stuff from <path>]),
[if test "$withval" != "no"; then
  EXTRAS+="${withval} "
fi])
AC_SUBST(EXTRAS)

DITCENGINE="./gforth-ditc"
AC_ARG_WITH(ditc,
	AC_HELP_STRING([--with-ditc=<engine>],
			[  Use this engine for compilation of gforth.fi]),
[if test $"withval" != "no"; then
  DITCENGINE=${withval}
fi])
AC_SUBST(DITCENGINE)

#variables mentioned in INSTALL
AC_ARG_VAR(SH, [The shell])
AC_ARG_VAR(CC, [The C compiler (must support GNU C 2.x).])
AC_ARG_VAR(HOSTCC, [The C compiler on the host (must have the same byte order and word size as target CC).])
AC_ARG_VAR(FORTHSIZES, [Gforth command line options for the default stack and dictionary sizes (see INSTALL).])
AC_ARG_VAR(STACK_CACHE_REGS, [number of registers in the maximum stack cache state for gforth-fast and gforth-native (default platform-dependent).])
AC_ARG_VAR(STACK_CACHE_DEFAULT_FAST, [number of registers in the default stack cache state for gforth-fast and gforth-native (default 1).])
AC_ARG_VAR(GCC_PR15242_WORKAROUND, [Force the enabling (1) or disabling (0) of a workaround for a gcc-3.x performance bug (default unset: use workaround for gcc-3.x)])
AC_ARG_VAR(LIBCC_BUILD_SRC, [(Additional) libcc interface source files that should be processed on building and installation (default none)])

AC_ARG_VAR(ac_cv_sizeof_char_p, [sizeof(char *)])
AC_ARG_VAR(ac_cv_sizeof_void_p, [sizeof(void *)])
AC_ARG_VAR(ac_cv_sizeof_char, [sizeof(char)])
AC_ARG_VAR(ac_cv_sizeof_short, [sizeof(short)])
AC_ARG_VAR(ac_cv_sizeof_int, [sizeof(int)])
AC_ARG_VAR(ac_cv_sizeof_long, [sizeof(long)])
AC_ARG_VAR(ac_cv_sizeof_long_long, [sizeof(long long)])
AC_ARG_VAR(ac_cv_sizeof_intptr_t, [sizeof(intptr_t)])
AC_ARG_VAR(ac_cv_sizeof_int128_t, [sizeof(int128_t)])
AC_ARG_VAR(ac_cv_sizeof_uint128_t, [sizeof(uint128_t)])
AC_ARG_VAR(ac_cv_c_bigendian, [Is the target big-endian ("yes" or "no")?])
AC_ARG_VAR(no_dynamic_default, [run gforth with --dynamic (0) or --no-dynamic (1) by default])
AC_ARG_VAR(condbranch_opt, [enable (1) or disable (0) using two dispatches for conditional branches])
AC_ARG_VAR(skipcode, [assembly code for skipping 16 bytes of code])
AC_ARG_VAR(asmcomment, [assembler comment start string])
AC_ARG_VAR(arm_cacheflush, [file containing ARM cacheflush function (without .c)])
AC_ARG_VAR(LTDL_LIBRARY_PATH, [additional directories for libltdl (for some 64-bit platforms)])

test -z "$HOSTCC" && HOSTCC='$(GCC)'

#set up feature test macros, so the tests get them right:
# turn on all POSIX, SUSv3, and GNU features if available
AC_GNU_SOURCE
#AC_DEFINE_UNQUOTED([_GNU_SOURCE],1,[feature test macro])

#Don't define _POSIX_SOURCE etc. because some OSs (in particular
#MacOSX) disable some features then (MacOSX checks for _POSIX_SOURCE,
#but not for _XOPEN_SOURCE)
#AC_DEFINE_UNQUOTED([_POSIX_SOURCE],1,[feature test macro])
#AC_DEFINE_UNQUOTED([_POSIX_C_SOURCE],199506L,[feature test macro])
#AC_DEFINE_UNQUOTED([_XOPEN_SOURCE],600,[feature test macro])
# turn on large file support with 64-bit off_t where available
AC_SYS_LARGEFILE
#AC_DEFINE_UNQUOTED([_LARGEFILE_SOURCE],1,[feature test macro])
#AC_DEFINE_UNQUOTED([_FILE_OFFSET_BITS],64,[feature test macro])

#currently we force direct threading this way.  Eventually we should
#setup in the arch and engine files right

AC_PROG_CC

test "$GCC" = "yes" || AC_MSG_ERROR(Gforth uses GNU C extensions and requires GCC 2.0 or higher)

test "x$SH" = "x" && SH="/bin/sh"

AC_MSG_CHECKING([whether to use two dispatches per conditional branch])
test x$condbranch_opt = x && 
if ($CC -v 2>&1 |grep 'gcc version 3' >/dev/null); then
   condbranch_opt=0
else
   condbranch_opt=1
fi
AC_MSG_RESULT($condbranch_opt)
AC_SUBST(condbranch_opt)
AC_SUBST(host_cpu)

AC_SUBST(SH)
AC_SUBST(CC)
AC_SUBST(GCC_LD)
AC_SUBST(DEBUGFLAG)
AC_SUBST(EC)
AC_SUBST(NO_EC)
AC_SUBST(NO_CHECK)
AC_SUBST(NO_CHECKX)
AC_SUBST(NO_FAIL)
AC_SUBST(NO_CROSS)
AC_SUBST(EC_MODE)
AC_SUBST(engine2)
AC_SUBST(engine_fast2)
AC_SUBST(no_dynamic)
AC_SUBST(image_i)
AC_SUBST(signals_o)
AC_SUBST(mi_prefix)
AC_SUBST(extraccdir)
AC_DEFINE_UNQUOTED([EXTRACCDIR], ["$extraccdir"], [Extra directory where to look for shared libraries])

#this is used to disable some (not generally essential) part of the
#Makefile that some makes don't grok.  It would be better to test for
#this specific Makefile feature than the make version.
AC_MSG_CHECKING(make type)
make_type=`make -n -v 2>&1|grep 'ake'|sed 's/ake .*/ake/'`
GNUMAKE='#'
test "$make_type" = "GNU Make" && GNUMAKE=''
AC_MSG_RESULT($make_type)
AC_SUBST(GNUMAKE)

if test "$ac_export_dynamic" != no; then
  if test "$ac_export_dynamic" != yes; then
    AC_MSG_CHECKING([whether the linker accepts -Wl,--export-dynamic])
    OLDLDFLAGS=$LDFLAGS
    LDFLAGS="$LDFLAGS -Wl,--export-dynamic"
    #AC_TRY_LINK gives false positive on rs6000-ibm-aix4.2.1.0
    #AC_TRY_LINK(,,ac_export_dynamic=yes,ac_export_dynamic=no)
    AC_TRY_RUN(main(){exit(0);},ac_export_dynamic=yes,ac_export_dynamic=no,ac_export_dynamic=no)
    LDFLAGS=$OLDLDFLAGS
  fi
fi

test $ac_export_dynamic = yes && LDFLAGS="$LDFLAGS -Wl,--export-dynamic"
test $ac_export_dynamic = yes && AC_DEFINE(HAS_BACKLINK,,[Define if you have backlinks])
AC_MSG_RESULT($ac_export_dynamic)

FAST=no
#terminology is a bit unusual here: The host is the system on which
#gforth will run; the system on which configure will run is the `build'
case "$host_cpu" in
	arm64*|aarch64*)
		machine=arm64
		CFLAGS="$CFLAGS -fomit-frame-pointer"
                test x$STACK_CACHE_REGS = x && STACK_CACHE_REGS=3
		if test -z $arm_cacheflush; then
		   AC_LIBOBJ(arm64-cacheflush)
		fi
		;;
	arm*)
		machine=arm
		CFLAGS="$CFLAGS -fomit-frame-pointer"
		case "$host" in
		    *gnueabi*)
			# don't build for thumb
			CFLAGS="$CFLAGS -marm"
			;;
		esac
		if test x$platform = xnxt; then
		   CFLAGS="$CFLAGS -mthumb -mthumb-interwork"
		fi
		case "$host_os" in
		     *android*)
			CFLAGS="$CFLAGS -march=armv5 -mfloat-abi=softfp -mfpu=vfp"
			FAST="yes" # build extra fast libraries
			;;
		esac
		case "$host_os" in
		     *linux*)
			AC_LIBOBJ(arm-cacheflush-linux)
			;;
		     *)   
			no_dynamic_default=1
			AC_LIBOBJ(arm-cacheflush0)
			AC_MSG_WARN([No I-cache flush code known, disabling dynamic native code generation])
			;;
                esac
                #longer skipcodes lead to problems on ARM, and it uses
                #only 4-byte alignment anyway
		test "$skipcode" || skipcode="nop"
		;;
	hppa*)
		machine=hppa
		AC_LIBOBJ(hppa-cache)
		#-N needed for --dynamic <M4U3b.3790$9d.317@news.cpqcorp.net>
		LDFLAGS="$LDFLAGS -Xlinker -N"
		LIBS="$LIBS -L/lib/pa1.1/"
		;;
	sparc*)
		machine=sparc
		;;
	i386)
		machine=386
		CFLAGS="$CFLAGS -fomit-frame-pointer -fforce-addr"
		case "$host_os" in
		     *android*)
			FAST="yes" # build extra fast libraries
			;;
		esac
		;;
	i486)
		machine=386
		CFLAGS="$CFLAGS -fomit-frame-pointer -fforce-addr"
		CFLAGS_1="$CFLAGS"
		CFLAGS="$CFLAGS -march=i486"
		AC_TRY_COMPILE(,,,CFLAGS="$CFLAGS_1 -m486")
		AC_TRY_COMPILE(,,,CFLAGS="$CFLAGS_1")
		case "$host_os" in
		     *android*)
			FAST="yes" # build extra fast libraries
			;;
		esac
		;;
	i*86)
		machine=386
		CFLAGS="$CFLAGS -fomit-frame-pointer -fforce-addr"
		CFLAGS_1="$CFLAGS"
		CFLAGS="$CFLAGS -march=pentium -mtune=generic"
		AC_TRY_COMPILE(,,,CFLAGS="$CFLAGS_1 -march=pentium")
		AC_TRY_COMPILE(,,,CFLAGS="$CFLAGS_1 -m486")
		AC_TRY_COMPILE(,,,CFLAGS="$CFLAGS_1")
		case "$host_os" in
		     *android*)
			FAST="yes" # build extra fast libraries
			;;
		esac
		;;
	x86_64)
		case $CC
		in
		    *-m32*)
		    	machine=386
			CFLAGS="$CFLAGS -fomit-frame-pointer -fforce-addr"
			CFLAGS_1="$CFLAGS"
			CFLAGS="$CFLAGS -march=athlon64"
			;;
		    *)
		    	machine=amd64
			;;
		esac
		;;
	ia64*)
		machine=ia64
		AC_LIBOBJ(ia64_flush_icache_block)
		test "$skipcode" || skipcode="nop.i 0"
		#".skip 16" passes the test below,
		# but gives an assembler error in engine
		;;
        m68k)
		machine=m68k
		CFLAGS="$CFLAGS -fomit-frame-pointer"
		if test "$host_os" = "nextstep3"
		then
			AC_LIBOBJ(termios)
		fi
		;;
	mips*)
		machine=mips
		AC_LIBOBJ(mips_check_prim)
		#dynamic native code has the following problems on MIPS:
		#
		#1) J/JAL seems relocatable, but is are only
		#relocatable within a 256MB-segment.  While we try to
		#get the linker to arrange this, there is no guarantee
		#that this will succeed (and if the user uses a lot of
		#memory, it is likely to fail).
		#
		#2) The way we generate dynamic native code may
		#violate MIPS architectural restrictions (in
		#particular, the delay slots of LW, MFLO, etc.)
		#
		#Therefore we disable dynamic native code by default:
		if test -z "$no_dynamic_default"; then
		    no_dynamic_default=1
		    AC_MSG_WARN([Disabling default dynamic native code generation (relocation and delay slot issues)])
		fi
		;;
	alpha*)
		machine=alpha
		#full IEEE FP support for more uniformity across platforms:
		CFLAGS="$CFLAGS -mieee"
		;;
	power*|rs6000)
		machine=power
		AC_CHECK_FUNC(_sync_cache_range,[true],[AC_LIBOBJ(power_sync_cache_range)])
		#long long is broken on (at least) gcc-2.95.* for PPC
		test x$ac_cv_sizeof_long_long = x && 
		($CC -v 2>&1 |grep 'gcc version 2.95' >/dev/null) &&
                     ac_cv_sizeof_long_long=0
                #One of the few architectures with enough callee-saved registers
                test x$STACK_CACHE_REGS = x && STACK_CACHE_REGS=3
                #or use 2, hardly slower at run-time and starts up faster

                #the skipcode that is normally selected below does not
                #work with gcc-3.[34]
		test "$skipcode" || skipcode="nop\nnop\nnop\nnop"
		;;
	*)
		AC_MSG_WARN([Using a generic machine description])
		AC_MSG_WARN([Assuming C floats and doubles are IEEE floats and doubles (for SF@ DF@ SF! DF!)])
		AC_MSG_WARN([FLUSH-ICACHE will do nothing, so END-CODE may not work properly!])
		machine=generic
		#I-cache flushing would be needed for dynamic code generation
		if test -z $no_dynamic_default; then
		    no_dynamic_default=1
		    AC_MSG_WARN([No I-cache flush code known, disabling dynamic native code generation])
		fi
esac
AC_SUBST(host)
AC_SUBST(FAST)

MAKEINC=""
GFORTH_EXE="true"

echo "Check for arch/$machine/$platform/gforth.ld ($EC_MODE)"
if test x$EC_MODE = xtrue
then
	echo "Check for arch/$machine/$platform/gforth.ld"
	if test -f arch/$machine/$platform/gforth.ld
	then
		LDFLAGS="-T ../arch/$machine/$platform/gforth.ld -Map \$@.map -cref --gc-sections $LDFLAGS"
		if test x$platform = xnxt; then
			LIBS="$LIB_PATH/lib/gcc/arm-elf/$($CC --version | grep GCC | cut -d' ' -f3)/interwork/libgcc.a $LIB_PATH/arm-elf/lib/interwork/libc.a $LIBS"
		fi
	fi
	if test -f arch/$machine/$platform/make.inc
	then
		MAKEINC="include ../arch/$machine/$platform/make.inc"
		GFORTH_EXE="\$(MAKE) -f arch/$machine/$platform/make.inc \$@.exe"
	fi
fi
AC_SUBST(MAKEINC)

AC_ARG_VAR(STACK_CACHE_REGS, [number of registers in the maximum stack cache state for gforth-fast and gforth-native (default platform-dependent).])

test x$STACK_CACHE_REGS = x && STACK_CACHE_REGS=1
AC_DEFINE_UNQUOTED(STACK_CACHE_REGS, $STACK_CACHE_REGS,
		   [number of registers in the maximum stack cache state for gforth-fast and gforth-native])
test x$STACK_CACHE_DEFAULT_FAST = x && STACK_CACHE_DEFAULT_FAST=1
AC_DEFINE_UNQUOTED(STACK_CACHE_DEFAULT_FAST, $STACK_CACHE_DEFAULT_FAST,
		   [number of registers in the default stack cache state for gforth-fast and gforth-native])

test x$GCC_PR15242_WORKAROUND = x ||
AC_DEFINE_UNQUOTED(GCC_PR15242_WORKAROUND, $GCC_PR15242_WORKAROUND,
                   [force (1) or forbid (0) use of a workaround for a gcc performance bug])

#the following macro produces a warning with autoconf-2.1
AC_CHECK_SIZEOF(char *)
case "$ac_cv_sizeof_char_p" in
  2)
    wordsize=16
    ;;
  4)
    wordsize=32
    ;;
  8)
    wordsize=64
    ;;
esac

AC_CHECK_SIZEOF(void *)
case "$ac_cv_sizeof_void_p" in
  2)
    vwordsize=16
    ;;
  4)
    vwordsize=32
    ;;
  8)
    vwordsize=64
    ;;
esac

AC_CHECK_SIZEOF(char)
AC_CHECK_SIZEOF(short)
AC_CHECK_SIZEOF(int)
AC_CHECK_SIZEOF(long)
AC_CHECK_SIZEOF(long long)
AC_CHECK_SIZEOF(intptr_t)
AC_CHECK_SIZEOF(int128_t)
AC_CHECK_SIZEOF(uint128_t)

AC_MSG_CHECKING([for a C type for cells])
ac_cv_int_type_cell=none
case "$ac_cv_sizeof_char_p" in
  $ac_cv_sizeof_int)
    ac_cv_int_type_cell=int
    ;;
  $ac_cv_sizeof_short)
    ac_cv_int_type_cell=short
    ;;
  $ac_cv_sizeof_char)
    ac_cv_int_type_cell=char
    ;;
  $ac_cv_sizeof_long)
    ac_cv_int_type_cell=long
    ;;
  $ac_cv_sizeof_long_long)
    ac_cv_int_type_cell="long long"
    ;;
  $ac_cv_sizeof_intptr_t)
    ac_cv_int_type_cell="intptr_t"
    ;;
esac
AC_MSG_RESULT($ac_cv_int_type_cell)
AC_DEFINE_UNQUOTED(CELL_TYPE,$ac_cv_int_type_cell,[an integer type that is as long as a pointer])

AC_MSG_CHECKING([for a C type for wydes])
ac_cv_wyde_type_cell=none
case 2 in
  $ac_cv_sizeof_int)
    ac_cv_wyde_type_cell=int
    ;;
  $ac_cv_sizeof_short)
    ac_cv_wyde_type_cell=short
    ;;
  $ac_cv_sizeof_char)
    ac_cv_wyde_type_cell=char
    ;;
  $ac_cv_sizeof_long)
    ac_cv_wyde_type_cell=long
    ;;
  $ac_cv_sizeof_long_long)
    ac_cv_wyde_type_cell="long long"
    ;;
  $ac_cv_sizeof_intptr_t)
    ac_cv_wyde_type_cell="intptr_t"
    ;;
esac
AC_MSG_RESULT($ac_cv_wyde_type_cell)
AC_DEFINE_UNQUOTED(WYDE_TYPE,$ac_cv_wyde_type_cell,[an integer type that is 2 bytes long])

AC_MSG_CHECKING([for a C type for tetrabytes])
ac_cv_tetrabyte_type_cell=none
case 4 in
  $ac_cv_sizeof_int)
    ac_cv_tetrabyte_type_cell=int
    ;;
  $ac_cv_sizeof_short)
    ac_cv_tetrabyte_type_cell=short
    ;;
  $ac_cv_sizeof_char)
    ac_cv_tetrabyte_type_cell=char
    ;;
  $ac_cv_sizeof_long)
    ac_cv_tetrabyte_type_cell=long
    ;;
  $ac_cv_sizeof_long_long)
    ac_cv_tetrabyte_type_cell="long long"
    ;;
  $ac_cv_sizeof_intptr_t)
    ac_cv_tetrabyte_type_cell="intptr_t"
    ;;
esac
AC_MSG_RESULT($ac_cv_tetrabyte_type_cell)
AC_DEFINE_UNQUOTED(TETRABYTE_TYPE,$ac_cv_tetrabyte_type_cell,[an integer type that is 4 bytes long])

AC_MSG_CHECKING([for a C type for octabytes])
ac_cv_octabyte_type_cell=none
case 8 in
  $ac_cv_sizeof_int)
    ac_cv_octabyte_type_cell=int
    ;;
  $ac_cv_sizeof_short)
    ac_cv_octabyte_type_cell=short
    ;;
  $ac_cv_sizeof_char)
    ac_cv_octabyte_type_cell=char
    ;;
  $ac_cv_sizeof_long)
    ac_cv_octabyte_type_cell=long
    ;;
  $ac_cv_sizeof_long_long)
    ac_cv_octabyte_type_cell="long long"
    ;;
  $ac_cv_sizeof_intptr_t)
    ac_cv_octabyte_type_cell="intptr_t"
    ;;
esac
AC_MSG_RESULT($ac_cv_octabyte_type_cell)
AC_DEFINE_UNQUOTED(OCTABYTE_TYPE,$ac_cv_octabyte_type_cell,[an integer type that is 8 bytes long])

AC_MSG_CHECKING([for a C type for double-cells])
ac_cv_int_type_double_cell=none
case `expr 2 '*' "$ac_cv_sizeof_char_p"` in
  $ac_cv_sizeof_short)
    ac_cv_int_type_double_cell=short
    ;;
  $ac_cv_sizeof_int)
    ac_cv_int_type_double_cell=int
    ;;
  $ac_cv_sizeof_long)
    ac_cv_int_type_double_cell=long
    ;;
  $ac_cv_sizeof_long_long)
    ac_cv_int_type_double_cell="long long"
    ;;
  $ac_cv_sizeof_intptr_t)
    ac_cv_int_type_double_cell="intptr_t"
    ;;
  $ac_cv_sizeof_int128_t)
    ac_cv_int_type_double_cell="int128_t"
    ;;
esac
AC_MSG_RESULT($ac_cv_int_type_double_cell)

AC_MSG_CHECKING([for a C type for unsigned double-cells])
ac_cv_int_type_double_ucell=none
case `expr 2 '*' "$ac_cv_sizeof_char_p"` in
  $ac_cv_sizeof_short)
    ac_cv_int_type_double_ucell="unsigned short"
    ;;
  $ac_cv_sizeof_int)
    ac_cv_int_type_double_ucell="unsigned int"
    ;;
  $ac_cv_sizeof_long)
    ac_cv_int_type_double_ucell="unsigned long"
    ;;
  $ac_cv_sizeof_long_long)
    ac_cv_int_type_double_ucell="unsigned long long"
    ;;
  $ac_cv_sizeof_intptr_t)
    ac_cv_int_type_double_ucell="unsigned intptr_t"
    ;;
  $ac_cv_sizeof_uint128_t)
    ac_cv_int_type_double_ucell="uint128_t"
    ;;
esac
AC_MSG_RESULT($ac_cv_int_type_double_ucell)

if test "$ac_cv_int_type_double_cell" != none && \
   test "$ac_cv_int_type_double_ucell" != none
then
	AC_DEFINE_UNQUOTED(DOUBLE_CELL_TYPE,$ac_cv_int_type_double_cell,[an integer type that is twice as long as a pointer])
	AC_DEFINE_UNQUOTED(DOUBLE_UCELL_TYPE,$ac_cv_int_type_double_ucell,[an unsigned integer type that is twice as long as a pointer])
        OPTS=-ll
else
        if test "$ac_cv_sizeof_char_p" = 8; then
           OPTS="-ll -noll"
        else
           OPTS=-noll
        fi
fi

if grep FORCE_REG $srcdir/arch/$machine/machine.h >/dev/null; then
   OPTS2=''
   for i in $OPTS; do OPTS2="$OPTS2 $i-reg"; done
   OPTS="$OPTS2 $OPTS"
else
  AC_DEFINE_UNQUOTED(FORCE_REG_UNNECESSARY,,[defined if the platform does not need FORCE_REG])
fi
AC_SUBST(OPTS)

AC_TYPE_OFF_T
AC_CHECK_SIZEOF(off_t)
test $ac_cv_sizeof_off_t -gt $ac_cv_sizeof_char_p
ac_small_off_t=$?
AC_DEFINE_UNQUOTED(SMALL_OFF_T,$ac_small_off_t,[1 if off_t fits in a Cell])

ENGINE_FLAGS=
AC_SUBST(ENGINE_FLAGS)

# Try if GCC understands -fno-gcse

CHECKFLAGS="no-gcse caller-saves no-defer-pop no-inline wrapv char-unsigned no-strict-aliasing no-cse-follow-jumps no-reorder-blocks no-reorder-blocks-and-partition no-toplevel-reorder no-trigraphs align-labels=1 align-loops=1 align-jumps=1 no-delete-null-pointer-checks"

for i in $CHECKFLAGS
do
    AC_MSG_CHECKING([if $CC understands -f$i])
    ac_i=$(echo ac_$i | tr '=\-' '__')
    CFLAGS_1="$CFLAGS"
    CFLAGS="$CFLAGS -f$i"
    AC_TRY_COMPILE(,,eval $ac_i=yes;ENGINE_FLAGS="$ENGINE_FLAGS -f$i",eval ac_$ibar=no)
    CFLAGS="$CFLAGS_1"
    ac_result=$(eval echo "\$$ac_i")
    AC_MSG_RESULT($ac_result)
done

# Try if GCC understands -pthread
AC_MSG_CHECKING([if $CC understands -pthread])
CFLAGS_1="$CFLAGS"
LDFLAGS_1="$LDFLAGS"
CFLAGS="$CFLAGS -pthread"
LDFLAGS="$LDFLAGS -pthread"
AC_TRY_COMPILE(,,ac_pthread=yes;ENGINE_FLAGS="$ENGINE_FLAGS -pthread",ac_pthread=no)
test "$ac_pthread" == yes || CFLAGS="$CFLAGS_1"
test "$ac_pthread" == yes || LDFLAGS="$LDFLAGS_1"
AC_MSG_RESULT($ac_pthread)

if test -z "$LIBTOOL_CC"
then
  LIBTOOL_CC="$CC"
  # Try if GCC understands -Wimplicit-function-declaration
  AC_MSG_CHECKING([if $CC understands -Wimplicit-function-declaration])
  CFLAGS_1="$CFLAGS"
  CFLAGS="$CFLAGS -Wimplicit-function-declaration"
  AC_TRY_COMPILE(,,LIBTOOL_CC="$CC -Wimplicit-function-declaration",)
  CFLAGS="$CFLAGS_1"
  AC_MSG_RESULT($ac_align_jumps)
fi
# remove prefix for cross compilation
LIBTOOL_CC=${LIBTOOL_CC#$CROSS_PREFIX}
AC_SUBST(LIBTOOL_CC)
AC_SUBST(CROSS_PREFIX)


# Try if GCC understands __attribute__((unused))
AC_MSG_CHECKING([how to suppress 'unused variable' warnings])
AC_TRY_COMPILE(,[int __attribute__((unused)) foo;], MAYBE_UNUSED='__attribute__((unused))',)
AC_DEFINE_UNQUOTED(MAYBE_UNUSED,$MAYBE_UNUSED,[attribute for possibly unused variables])
AC_MSG_RESULT($MAYBE_UNUSED)

# Try if GCC understands __thread
AC_CACHE_CHECK([whether gcc understands '__thread'], [ac_cv_thread],
     [AC_TRY_COMPILE(,[extern __thread int foo;],
                        [ac_cv_thread=yes],
                        [ac_cv_thread=no])])
test x$ac_cv_thread = xyes && PER_THREAD='__thread'
AC_DEFINE_UNQUOTED(PER_THREAD,$PER_THREAD,[storage class for thread-local variables])

#try if m4 understands -s
AC_MSG_CHECKING([how to invoke m4])
if m4 -s /dev/null >/dev/null 2>&1; then
 M4="m4 -s"
else
 M4=m4
fi
AC_SUBST(M4)
AC_MSG_RESULT($M4)

#echo "machine='$machine'"

#AC_CHECK_PROG(asm_fs,asm.fs,arch/$machine/asm.fs,,$srcdir/arch/$machine)
if test x$EC_MODE = xfalse
then
  AS_IF([test -f $srcdir/arch/$machine/asm.fs],
        [asm_fs=arch/$machine/asm.fs])
fi
AC_SUBST(asm_fs)

#AC_CHECK_PROG(disasm_fs,disasm.fs,arch/$machine/disasm.fs,,$srcdir/arch/$machine)
if test x$EC_MODE = xfalse
then
  AS_IF([test -f $srcdir/arch/$machine/disasm.fs],
   	[disasm_fs=arch/$machine/disasm.fs])
fi
AC_SUBST(disasm_fs)

AC_PATH_PROG(INSTALL_INFO,install-info,[echo '>>>>Please make info dir entry:'],$PATH:/sbin:/usr/sbin:/usr/local/sbin)

RM=rm # usually, rm is called rm

case "$host_os" in
	*ios*)
		#Darwin uses some funny preprocessor by default; eliminate it:
		AC_MSG_CHECKING([if $CC understands -no-cpp-precomp on Darwin])
		CFLAGS_1="$CFLAGS"
		CFLAGS="$CFLAGS -no-cpp-precomp"
		AC_TRY_COMPILE(,,ac_no_cpp_precomp=yes;ENGINE_FLAGS="$ENGINE_FLAGS -no-cpp-precomp",ac_no_cpp_precomp=no)
		test "$ac_no_cpp_precomp" == yes || CFLAGS="$CFLAGS_1"
		AC_MSG_RESULT($ac_no_cpp_precomp)
		DIRSEP="/"
		PATHSEP=":"
		DEFAULTSYSTEMPREFIX=""
		# Mac OS X hides the X stuff in /opt, if you installed
		# XQuartz
                #the following magic value was suggested by
                #http://mail.python.org/pipermail/pythonmac-sig/2005-October/015190.html
                AC_DEFINE_UNQUOTED(MACOSX_DEPLOYMENT_TARGET,"10.10",[an environment variable value needed by libtool on some MacOS X versions])
		;;
	*darwin*)
		#dar*win* must be first, not to confuse with cygwin, win32 or similar
		#Darwin uses some funny preprocessor by default; eliminate it:
		AC_MSG_CHECKING([if $CC understands -no-cpp-precomp on Darwin])
		CFLAGS_1="$CFLAGS"
		CFLAGS="$CFLAGS -no-cpp-precomp"
		AC_TRY_COMPILE(,,ac_no_cpp_precomp=yes;ENGINE_FLAGS="$ENGINE_FLAGS -no-cpp-precomp",ac_no_cpp_precomp=no)
		test "$ac_no_cpp_precomp" == yes || CFLAGS="$CFLAGS_1"
		AC_MSG_RESULT($ac_no_cpp_precomp)
		DIRSEP="/"
		PATHSEP=":"
		DEFAULTSYSTEMPREFIX=""
		# Mac OS X hides the X stuff in /opt, if you installed
		# XQuartz
		case $platform in
			ios)
				host_os=ios # remove version number
				CC=${CC-xcrun -sdk iphoneos clang -arch $cpu -framework OpenGLES}
			;;
			*)
				host_os=darwin # remove version number
				test -d /opt/X11/include && CFLAGS="$CFLAGS -I/opt/X11/include"
				test -d /opt/X11/lib && LDFLAGS="$LDFLAGS -L/opt/X11/lib"
				test -d /sw/include && CFLAGS="$CFLAGS -I/sw/include"
				test -d /sw/lib && LDFLAGS="$LDFLAGS -L/sw/lib"
			;;
		esac
                #the following magic value was suggested by
                #http://mail.python.org/pipermail/pythonmac-sig/2005-October/015190.html
                AC_DEFINE_UNQUOTED(MACOSX_DEPLOYMENT_TARGET,"10.10",[an environment variable value needed by libtool on some MacOS X versions])
		;;
	*win*)
		# !!!FIXME!!! problems with cygwin and ';' as path separator
		DIRSEP="/" # normal dirsep
		PATHSEP=":" # colon as pathsep
		#we don't want the builtins of command.com/cmd.exe and its
		# handling of .com files.
		#$COMSPEC contains the name of the Windows shell;
		# the ./ is there, because the bash does not recognize
		# absolute DOS filenames
		DEFAULTSYSTEMPREFIX="" #"./${COMSPEC//\\/\\\\\\\\} /c "
		no_chmod="#"
		RM='$GFORTH -e ": rm BEGIN  argc @ 1 >  WHILE  next-arg delete-file throw REPEAT ; rm bye"' # on Windows, use Gforth to delete files
		;;
	*linux*)
		DIRSEP="/"
		PATHSEP=":"
		DEFAULTSYSTEMPREFIX=""
		if test -z "$LTDL_LIBRARY_PATH" -a "$wordsize" = 64; then
		   LTDL_LIBRARY_PATH=`/sbin/ldconfig -p |tail -n +2 |sed 's/^.* => //'|sed 's|/[[^/]]*$||'| grep 64 | sort -u | tr '\n' : | sed -e 's/:$//'`
		fi
		;;
	*)
		DIRSEP="/"
		PATHSEP=":"
		DEFAULTSYSTEMPREFIX=""
		;;
esac
AC_SUBST(no_chmod)
AC_SUBST(RM)
AC_SUBST(DIRSEP)
AC_DEFINE_UNQUOTED(DIRSEP,'$DIRSEP',[a directory separator character])
AC_SUBST(PATHSEP)
AC_DEFINE_UNQUOTED(PATHSEP,'$PATHSEP',[a path separator character])
AC_SUBST(DEFAULTSYSTEMPREFIX)
AC_DEFINE_UNQUOTED(DEFAULTSYSTEMPREFIX,"$DEFAULTSYSTEMPREFIX",[default for environment variable GFORTHSYSTEMPREFIX])
if test -n "$LTDL_LIBRARY_PATH"; then
   AC_DEFINE_UNQUOTED(LTDL_LIBRARY_PATH,"$LTDL_LIBRARY_PATH",[Define LTDL_LIBRARY_PATH for 64 bit Linux])
fi                                                                

#work around SELinux brain damage (from Andrew Haley <12t8f3jakb74g2c@news.supernews.com>)
#This magic incantation  seems to be completely undocumented.
AC_CHECK_PROG([MASSAGE_EXE],[chcon],[chcon -t unconfined_execmem_exec_t],[true])

#Now a little support for DOS/DJGCC
GFORTHFAST_EXE="$GFORTH_EXE"
GFORTHITC_EXE="$GFORTH_EXE"
GFORTHDITC_EXE="$GFORTH_EXE"
AC_SUBST(GFORTH_EXE)
AC_SUBST(GFORTHFAST_EXE)
AC_SUBST(GFORTHITC_EXE)
AC_SUBST(GFORTHDITC_EXE)

AC_SUBST(FORTHSIZES)

#if test "$PEEPHOLE" = "yes"
#then
#   PEEPHOLEFLAG="true"
#   AC_DEFINE(HAS_PEEPHOLE,,[Define if you want to use peephole optimization])
#else
#   PEEPHOLEFLAG="false"
#fi
PEEPHOLEFLAG="true"
AC_SUBST(PEEPHOLEFLAG)

#copy commands for systems that don't have links
AC_SUBST(LINK_KERNL)
LINK_KERNL=""

#if test $host_os=dos
#then
#  echo Configuring for DOS!!!
#  MAKE_EXE="coff2exe gforth"
#  LINK_KERNL='$(CP) kernl32l.fi kernel.fi'
#fi

#the following macro produces a warning with autoconf-2.1
AC_C_BIGENDIAN
AC_SUBST(GKERNEL)
#ac_cv_c_bigendian is an undocumented variable of autoconf-2.1
if test $ac_cv_c_bigendian != no; then
  bytesex=b
  GKERNEL="kernl32b.fi kernl32l.fi kernl64b.fi kernl64l.fi"
else
  bytesex=l
  GKERNEL="kernl32l.fi kernl32b.fi kernl64l.fi kernl64b.fi"
fi

#check how to do asm(".skip 16")
#echo "CFLAGS=$CFLAGS"
#echo "ac_link=$ac_link"
AC_MSG_CHECKING([if and how we can waste code space])
if test -z "$skipcode" && test x$cross_compiling = xyes; then
   # we cannot perform the AC_TRY_RUN check when cross-compiling.  
   skipcode=no
   AC_MSG_RESULT([$skipcode, can't run checks since cross-compiling])
elif test -z "$skipcode"; then
    skipcode=no
    CFLAGS_1="$CFLAGS"
    CFLAGS="$CFLAGS $ENGINE_FLAGS"
    for i in ".skip 16" ".block 16" ".org .+16" ".=.+16" ".space 16"
    do
	AC_TRY_RUN(
[int foo(int,int,int);
main()
{
  exit(foo(0,0,0)!=16);
}
int foo(int x, int y, int z)
{
  static void *labels[]={&&label1, &&label2, &&label3};
  if (x) {
    y++; /* workaround for http://gcc.gnu.org/bugzilla/show_bug.cgi?id=12108 */
  label1: asm("$i"); /* or ".space 16" or somesuch */
  label2: asm("$i"); /* or ".space 16" or somesuch */
  label3: ; /* workaround clang */
  }
  {
  if (y) goto *labels[z]; /* workaround for gcc PR12108 */
  return labels[1]-labels[0];
  }
}]
	,skipcode=$i; break
	,,)
    done
    CFLAGS=$CFLAGS_1
fi
AC_MSG_RESULT($skipcode)
if test "$skipcode" = no
then 
    if test -z $no_dynamic_default; then
	no_dynamic_default=1
	AC_MSG_WARN(Disabling default dynamic native code generation)
    fi
    AC_DEFINE_UNQUOTED(SKIP16,asm(""),statement for skipping 16 bytes)
else
    AC_DEFINE_UNQUOTED(SKIP16,asm("$skipcode"),statement for skipping 16 bytes)
fi

AC_MSG_CHECKING([if and how we can do comments in asm statements])
#the point here is to get asm statements that look different to
#gcc's "optimizer"
if test -z "$asmcomment"; then
    asmcomment=no
    CFLAGS_1="$CFLAGS"
    CFLAGS="$CFLAGS $ENGINE_FLAGS"
    for i in '"# "' '"! "' '"; "'; do
	AC_TRY_COMPILE(,[asm($i"fluffystunk");],asmcomment=$i; break,)
    done
    CFLAGS=$CFLAGS_1
fi
AC_MSG_RESULT($asmcomment)
if test "$asmcomment" != no
then 
    AC_DEFINE_UNQUOTED(ASMCOMMENT,$asmcomment,[assembler comment start string])
fi

test "$no_dynamic_default" || no_dynamic_default=0
AC_DEFINE_UNQUOTED(NO_DYNAMIC_DEFAULT,$no_dynamic_default,default value for no_dynamic)

#Checks for programs.
AC_PROG_LN_S
AC_PROG_INSTALL
AC_CHECK_PROGS(TEXI2DVI,texi2dvi4a2ps texi2dvi)

#check for the presence of Emacs
AC_CHECK_PROGS(EMACS,emacs xemacs)
test -n "$EMACS" && gforth_elc=gforth.elc
AC_SUBST(gforth_elc)

#MacOS X has a libtool that does something else
AC_CHECK_PROGS(GNU_LIBTOOL,"glibtool --tag=CC" libtool)
if test -z "$GNU_LIBTOOL"; then
  AC_MSG_WARN([No GNU_LIBTOOL found, using "libtool" as name.])
  AC_MSG_WARN([libcc.fs won't work until you have installed (GNU) libtool.])
  #'
  GNU_LIBTOOL=libtool
fi
GNU_LIBTOOL=${GNU_LIBTOOL#$CROSS_PREFIX}
AC_SUBST(GNU_LIBTOOL)

#Checks for library functions
#This check is just for making later checks link with libm.
#using sin here is no good idea since it is built-into gcc and typechecked
AC_CHECK_LIB(m,asin)
AC_CHECK_LIB(ltdl,lt_dlinit)
#,LIB_SUFFIX=.la,LIB_SUFFIX=.so)
if test "x$ac_cv_lib_ltdl_lt_dlinit" = xyes; then
  LIB_SUFFIX=.la
else
  LIB_SUFFIX=.so
fi
AC_SUBST(LIB_SUFFIX)
AC_CHECK_LIB(rt,clock_gettime)

if test -n "GNU_LIBTOOL" -a $ac_cv_lib_ltdl_lt_dlinit = yes; then
  build_libcc_named=build-libcc-named
elif test -n "GNU_LIBTOOL" -a $ac_cv_func_dlopen = yes; then
  build_libcc_named=build-libcc-named
else
  AC_MSG_WARN([No LIBTOOL found, skip pre-building libcc-based libraries])
  AC_MSG_WARN([No LIBTOOL found, skip building library])
  enable_lib=no
fi
AC_SUBST(build_libcc_named)
AC_SUBST(CONFIG_PREFIX)
if test "$enable_lib" != "no"; then
   AC_DEFINE(HAS_LIB,,[Define if you want to build as shared library])
   libengines=libgforths
   libinstall=libinstall
   libbench=''
   LIB_VERSION=0:8:0
fi
AC_SUBST(libengines)
AC_SUBST(libinstall)
AC_SUBST(libbench)
AC_SUBST(LIB_VERSION)

#check for libffi 2.x
AC_CHECK_HEADER(ffi.h,FFI_H_NAME=ffi.h,)
if test -z "$FFI_H_NAME"; then
  AC_CHECK_HEADER(ffi/ffi.h,FFI_H_NAME=ffi/ffi.h,)
fi
AC_SUBST(FFI_H_NAME)
AC_CHECK_LIB(ffi,ffi_call,LIBS="$LIBS")
if test -n "$FFI_H_NAME" -a $ac_cv_lib_ffi_ffi_call = yes
then
  LIBFFIFLAG="true"
  LIBCC_BUILD_SRC="$LIBCC_BUILD_SRC libffi.fs"
else
  LIBFFIFLAG="false"
fi
#check for ffcall libraries
#unfortunately, these four calls are separated out into a library each.
AC_CHECK_LIB(avcall,__builtin_avcall,LIBS="$LIBS")
#AC_CHECK_LIB(callback,__vacall_r)
#AC_CHECK_LIB(vacall,vacall)
#AC_CHECK_LIB(trampoline,alloc_trampoline)
if test $ac_cv_lib_avcall___builtin_avcall = yes; then
  FFCALLFLAG="true"
  LIBCC_BUILD_SRC="$LIBCC_BUILD_SRC fflib.fs"
else
  FFCALLFLAG="false"
fi
if test $LIBFFIFLAG = false -a $FFCALLFLAG = false; then
  AC_MSG_WARN([The (old) lib.fs foreign function interface needs either libffi or the ffcall libraries])
fi
AC_SUBST(LIBFFIFLAG)
AC_SUBST(FFCALLFLAG)
AC_SUBST(LIBCC_BUILD_SRC)
if test "$host_os" != "nextstep3"
then
	AC_FUNC_MEMCMP
fi
AC_REPLACE_FUNCS(memmove strtoul pow10 sincos strerror strsignal atanh)
AC_FUNC_FSEEKO
AC_CHECK_FUNCS(ftello dlopen sys_siglist getrusage nanosleep clock_gettime)
AC_CHECK_TYPES(stack_t,,,[#include <signal.h>])
AC_DECL_SYS_SIGLIST
AC_CHECK_FUNC(getopt_long,[true],[AC_LIBOBJ(getopt) AC_LIBOBJ(getopt1)])
AC_CHECK_FUNCS(expm1 log1p)
AC_CHECK_FUNCS(mcheck mprobe)
AC_REPLACE_FUNCS(rint ecvt_r)
#No check for select, because our replacement is no good under
#anything but DOS
AC_CHECK_HEADERS(sys/mman.h fnmatch.h alloca.h wchar.h endian.h)
AC_REPLACE_FUNCS(wcwidth)
AC_FUNC_FNMATCH
test $ac_cv_func_fnmatch_works = yes || AC_LIBOBJ(fnmatch)
AC_CHECK_FUNCS(mmap sysconf getpagesize pselect pthread_setaffinity_np)
AM_PATH_LISPDIR
AX_GCC_BUILTIN(__builtin_clzl)
AX_GCC_BUILTIN(__builtin_bswap16)
AX_GCC_BUILTIN(__builtin_bswap32)
AX_GCC_BUILTIN(__builtin_bswap64)
AC_MSG_CHECKING([for atomic builtins (__sync_...)])
AC_LINK_IFELSE([AC_LANG_SOURCE([[
void test(char* addr, long val, long val2) {
   __sync_lock_test_and_set(addr, val);
   __sync_fetch_and_add(addr, val);
   __sync_val_compare_and_swap(addr, val, val2);
   __sync_synchronize();
}
int main(){char a[1]; long l=0; test(a, l, l); return 0; }
]])],[ATOMIC=true; AC_DEFINE(HAS_ATOMIC,,[Define if has atomic operations])],[ATOMIC=false])
AC_MSG_RESULT($ATOMIC)
AC_SUBST(ATOMIC)

# Sanitzie broken AM_PATH_LISPDIR result
lispdir=$(prefix=$ac_default_prefix
lispdirtest=$(eval echo $(eval echo $(eval echo $lispdir)))
test -d $lispdirtest && echo $lispdir && exit
for prefix in /usr /usr/local
do
    lispdirtest=$(eval echo $(eval echo $(eval echo $lispdir)))
    test -d $lispdirtest && echo $lispdirtest && exit
done)

elispstartdir=$(for i in /etc/emacs/site-start.d $lispdir/site-start.d/
do
    test -d $i && echo $i && exit
done)

AC_ARG_WITH(elispstartdir,
	AC_HELP_STRING([--with-elispstartdir=<dir>],
			[  Use this directory to store the autoload file]),
[if test $"withval" != "no"; then
  elispstartdir=${withval}
fi])
AC_SUBST(elispstartdir)

kernel_fi=kernl${vwordsize}${bytesex}.fi
include_fi=kernl${wordsize}${bytesex}${EC}.fi
AC_SUBST(kernel_fi)
AC_SUBST(include_fi)

# Find installed Gforth
AC_MSG_CHECKING([for gforth])
if test -z "$GFORTH"
then
    GFORTH="`cd / && which gforth 2>/dev/null`"
    (cd / && "$GFORTH" -e bye >/dev/null 2>/dev/null) || GFORTH=""
fi
if test -z "$GFORTH"; then
  PREFORTH='echo "You need to configure with a gforth in \$PATH to build this part" && false'
  GFORTHKER='echo "You need to configure with a gforth in \$PATH to build this part" && false'
  kernel_anti_dependence=''
else
  GFORTH_MAGIC="`cd / && "$GFORTH" --debug -e bye 2>&1 |grep \"Magic found:\"`"
  case "$GFORTH_MAGIC" in
        *little\ endian,\ cell=8*)
                KERNL=kernl64l.fi
                ;;
        *little\ endian,\ cell=4*)
                KERNL=kernl32l.fi
                ;;
        *big\ endian,\ cell=8*)
                KERNL=kernl64b.fi
                ;;
        *big\ endian,\ cell=4*)
                KERNL=kernl32b.fi
                ;;
  esac
  GFORTH_FI="`cd / && "$GFORTH" --debug -e bye 2>&1 |grep \"Opened image file: \"|sed 's/Opened image file: //'`"
  PREFORTH="\"$GFORTH\" -i \"$GFORTH_FI\"" ;
  KERNLX_FI1="$(echo $GFORTH_FI | sed -e s/lib[[64]]*/share/g -e s/gforth.fi/$KERNL/g)"
  KERNLX_FI2="$(echo $GFORTH_FI | sed -e s/gforth.fi/$KERNL/g)"
  "$GFORTH" -i "$KERNL" -e bye && KERNLX_FI="$KERNL" || \
  "$GFORTH" -i "$KERNLX_FI1" -e bye && KERNLX_FI="$KERNLX_FI1" || \
  "$GFORTH" -i "$KERNLX_FI2" -e bye && KERNLX_FI="$KERNLX_FI2"
  GFORTHKER="\"$GFORTH\" -i \"$KERNLX_FI\"" ;
  kernel_anti_dependence='$(kernel_fi)'
fi
AC_SUBST(PREFORTH)
AC_SUBST(GFORTHKER)
AC_SUBST(kernel_anti_dependence)
AC_MSG_RESULT($PREFORTH)
AC_DEFINE_UNQUOTED(GKERNEL, "$kernel_fi", [The kernel to load in the image])
AC_DEFINE_UNQUOTED(ARCH, "$machine", [architecture to run on ])
AC_SUBST(machine)

test "x$prefix" = xNONE && prefix=$ac_default_prefix
incdir=`eval echo $includedir`
AC_SUBST(incdir)

#this breaks bindists
## replace srource directory by absolute value
#if test $srcdir = "."; then srcdir=`pwd` 
#fi

# swig stuff
test x$SWIG = x && AC_PATH_PROG(SWIG, swig)
if test -z "$SWIG"; then
   AC_MSG_WARN([*** Could not find swig in your PATH. The library bindings cannot be built.])
fi

has_swig=no
if swig -forth $srcdir/unix/test.i; then
    echo "Swig works in Forth mode"
    has_swig=yes
fi

if test x$has_swig = xyes; then
   > conftest.$ac_ext
   SWIGINCLUDES=`eval $ac_compiler $CFLAGS -E -Wp,-v conftest.$ac_ext 2>&1 | grep '^ ' | sed -e 's/^ \(.*\) (framework directory)$/-F\1/g' -e 's/^ /-I/g' | tr '\n' ' '`
fi

if test x$has_swig = xyes || test -f $srcdir/unix/xlib-fsi.c; then
   AC_CHECK_HEADER(X11/X.h, SWIGMODULES="$SWIGMODULES xlib.fs"
		   SWIGLIBS="$SWIGLIBS x.fs", ,)
fi

if test "$host_os" == "cygwin"
then
    if test x$has_swig = xyes || test -f $srcdir/unix/wingdi-fsi.c; then
      AC_CHECK_HEADER(w32api/wtypes.h,
      AC_CHECK_HEADER(w32api/wingdi.h, SWIGMODULES="$SWIGMODULES wingdi.fs"
      				       SWIGLIBS="$SWIGLIBS gdi.fs", ,
#include <w32api/wtypes.h>
) ,)
    fi
    if test x$has_swig = xyes || test -f $srcdir/unix/winuser-fsi.c; then
      AC_CHECK_HEADER(w32api/wtypes.h,
      AC_CHECK_HEADER(w32api/winuser.h, SWIGMODULES="$SWIGMODULES winuser.fs"
      				        SWIGLIBS="$SWIGLIBS user.fs", ,
#include <w32api/wtypes.h>
) ,)
    fi
      AC_CHECK_HEADER(w32api/wtypes.h,
      AC_CHECK_HEADER(w32api/winbase.h, SWIGLIBS="$SWIGLIBS win32.fs", ,
#include <w32api/wtypes.h>
) ,)
else
    if test x$has_swig = xyes || test -f $srcdir/unix/gl-fsi.c; then
      AC_CHECK_HEADER(GL/gl.h, 
      AC_CHECK_HEADER(GL/glext.h, SWIGMODULES="$SWIGMODULES gl.fs"
      AC_CHECK_HEADER(GL/glx.h, SWIGMODULES="$SWIGMODULES glx.fs"
      SWIGLIBS="$SWIGLIBS opengl.fs", ,), , 
#include <GL/gl.h>
) ,)
    fi
fi
if test x$has_swig = xyes || test -f $srcdir/unix/gles-fsi.c; then
   AC_CHECK_LIB(GLESv2,glClear,
   AC_CHECK_HEADER(GLES2/gl2.h,
   AC_CHECK_HEADER(GLES2/gl2ext.h, SWIGMODULES="$SWIGMODULES gles.fs"
   AC_CHECK_HEADER(EGL/egl.h, SWIGMODULES="$SWIGMODULES egl.fs"
   SWIGLIBS="$SWIGLIBS opengles.fs", ,), ,
#include <GLES2/gl2.h>
) ,))
fi
if test x$has_swig = xyes || test -f $srcdir/unix/gles3-fsi.c; then
   AC_CHECK_LIB(GLESv3,glClear,
   AC_CHECK_HEADER(GLES3/gl3.h,
   AC_CHECK_HEADER(GLES3/gl3ext.h, SWIGMODULES="$SWIGMODULES gles3.fs"
   AC_CHECK_HEADER(EGL/egl.h, SWIGLIBS="$SWIGLIBS opengles3.fs", ,), ,
#include <GLES3/gl3.h>
) ,))
# iOS varaint
fi
if test x$has_swig = xyes || test -f $srcdir/unix/ios-gles-fsi.c; then
   AC_CHECK_HEADER(OpenGLES/ES2/gl.h,
   AC_CHECK_HEADER(OpenGLES/ES2/glext.h, SWIGMODULES="$SWIGMODULES ios-gles.fs" SWIGLIBS="$SWIGLIBS opengles.fs", ,
#include <OpenGLES/ES2/gl.h>
) ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/ios-gles3-fsi.c; then
   AC_CHECK_HEADER(OpenGLES/ES3/gl.h,
   AC_CHECK_HEADER(OpenGLES/ES3/glext.h, SWIGMODULES="$SWIGMODULES ios-gles3.fs" SWIGLIBS="$SWIGLIBS opengles3.fs", ,
#include <OpenGLES/ES3/gl.h>
) ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/jni-fsi.c; then
   AC_CHECK_HEADER(jni.h, SWIGMODULES="$SWIGMODULES jni.fs"
		   SWIGLIBS="$SWIGLIBS jnilib.fs", ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/openvg-fsi.c; then
   AC_CHECK_HEADER(openvg.h, SWIGMODULES="$SWIGMODULES openvg.fs", ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/androidlib-fsi.c; then
    AC_CHECK_HEADER(android/input.h, SWIGMODULES="$SWIGMODULES androidlib.fs"
		    SWIGLIBS="$SWIGLIBS android.fs", ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/omxal-fsi.c; then
    AC_CHECK_HEADER(OMXAL/OpenMAXAL.h, SWIGMODULES="$SWIGMODULES omxal.fs"
		    SWIGLIBS="$SWIGLIBS openmax.fs", ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/pnglib-fsi.c; then
    AC_CHECK_HEADER(libpng/png.h, SWIGMODULES="$SWIGMODULES pnglib.fs"
		    SWIGLIBS="$SWIGLIBS png.fs", ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/soil2-fsi.c; then
    AC_CHECK_HEADER(SOIL2.h, SWIGMODULES="$SWIGMODULES soil2.fs"
		    SWIGLIBS="$SWIGLIBS soil2lib.fs", ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/soil-fsi.c; then
    AC_CHECK_LIB(SOIL,SOIL_load_image,
    AC_CHECK_HEADER(SOIL/SOIL.h, SWIGMODULES="$SWIGMODULES soil.fs"
		    SWIGLIBS="$SWIGLIBS soillib.fs", ,))
fi
if test x$has_swig = xyes || test -f $srcdir/unix/freetype-gl-fsi.c; then
    AC_CHECK_HEADER(freetype-gl.h, SWIGMODULES="$SWIGMODULES freetype-gl.fs"
		    SWIGLIBS="$SWIGLIBS freetype-gllib.fs", ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/cpufeatures-fsi.c; then
    case "$host_os" in
      	linux-android*)
	    SWIGMODULES="$SWIGMODULES cpufeatures.fs"
	    SWIGLIBS="$SWIGLIBS cpufeatureslib.fs"
	    ;;
    esac
fi
if test x$has_swig = xyes || test -f $srcdir/unix/wayland-fsi.c; then
   AC_CHECK_HEADER(wayland/wayland-egl.h, SWIGMODULES="$SWIGMODULES wayland.fs"
		   SWIGLIBS="$SWIGLIBS waylandlib.fs", ,)
fi
if test x$has_swig = xyes || test -f $srcdir/unix/gps-fsi.c; then
   AC_CHECK_LIB(gps,gps_open,
   AC_CHECK_HEADER(gps.h, SWIGMODULES="$SWIGMODULES gps.fs"
		   SWIGLIBS="$SWIGLIBS gpslib.fs", ,))
fi
if test x$has_swig = xyes || test -f $srcdir/unix/vulkan-fsi.c; then
   AC_CHECK_LIB(vulkan,vkGetPhysicalDeviceFeatures,
   AC_CHECK_HEADER(vulkan/vulkan.h, SWIGMODULES="$SWIGMODULES vulkan.fs"
		   SWIGLIBS="$SWIGLIBS vulkanlib.fs", ,))
fi

if test x$has_swig = xno; then
   AC_MSG_WARN([*** Swig without -forth. Building library buildings $SWIGMODULES from tarball.])
fi
AC_SUBST(SWIGMODULES)
AC_SUBST(SWIGINCLUDES)
AC_SUBST(SWIGLIBS)

case $CC
in
	*-*)
		HOSTPREFIX=${CC%-*}
		;;
	*)
		HOSTPREFIX=""
		;;
esac

AC_SUBST(HOSTPREFIX)

EXTRAPREFIX=${EXTRAPREFIX-$prefix}
AC_SUBST(EXTRAPREFIX)

DEBVERSION=`echo $PACKAGE_VERSION | tr '_' '-'`
AC_SUBST(DEBVERSION)

# finally create config files

AC_CONFIG_FILES([
kernel/version.fs
Makefile
Makedist
gforthmi
gforthmi.sh
vmgen
machpc.fs
envos.fs
preforth
libforth
gforthker
engine/Makefile
engine/libcc.h
engine/gforth.h
doc/version.texi
doc/Makefile
unix/Makefile
debian/changelog
debian/control
build-ec ])
AC_CONFIG_COMMANDS([stamp-h],[[echo timestamp > stamp-h
chmod +x gforthmi
chmod +x vmgen
chmod +x build-ec
chmod +x preforth
chmod +x libforth
chmod +x gforthker
test -d kernel||mkdir kernel
if test $srcdir != "."; then ln -sf $srcdir/gforth.el .; fi
$srcdir/mkinstalldirs include/gforth/$PACKAGE_VERSION/$machine
$srcdir/mkinstalldirs arch/$machine
ln -sf $PWD/engine/config.h $PWD/engine/libcc.h include/gforth/$PACKAGE_VERSION/$machine]],
[PACKAGE_VERSION=$PACKAGE_VERSION
 machine=$machine])
AC_OUTPUT
