/* config.h.  Generated automatically by configure.  */
/* config.h.in.  Generated automatically from configure.in by autoheader.  */

/* Define if `sys_siglist' is declared by <signal.h>.  */
#undef SYS_SIGLIST_DECLARED

/* Define if your processor stores words with the most significant
   byte first (like Motorola and SPARC, unlike Intel and VAX).  */
/* #undef WORDS_BIGENDIAN */

/* Package name */
#define PACKAGE "gforth"

/* Package version */
#define VERSION "0.5.0"

/* an integer type that is as long as a pointer */
#define CELL_TYPE int

/* an integer type that is twice as long as a pointer */
#define DOUBLE_CELL_TYPE long long

/* a path separator character */
#define PATHSEP ';'

/* define this if there is no working DOUBLE_CELL_TYPE on your machine */
/* #undef BUGGY_LONG_LONG */

/* The number of bytes in a char *.  */
#define SIZEOF_CHAR_P 4

/* The number of bytes in a int.  */
#define SIZEOF_INT 4

/* The number of bytes in a long.  */
#define SIZEOF_LONG 4

/* The number of bytes in a long long.  */
#define SIZEOF_LONG_LONG 8

/* The number of bytes in a short.  */
#define SIZEOF_SHORT 2

/* Define if you have the expm1 function.  */
#undef HAVE_EXPM1

/* Define if you have the log1p function.  */
#undef HAVE_LOG1P

/* Define if you have the rint function.  */
#undef HAVE_RINT

/* Define if you have the sys_siglist function.  */
#undef HAVE_SYS_SIGLIST

/* Define if you have the m library (-lm).  */
#define HAVE_LIBM 1
/* Of course, sys_siglist is a variable, not a function */

/* Define if you want to force a direct threaded code implementation
   (does not work on all machines */
/* Define if you want to force an indirect threaded code implementation */
/* Define if you want to use explicit register declarations for better
   performance or for more convenient CODE words (does not work with
   all GCC versions on all machines) */
