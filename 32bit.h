/*
  Copyright 1992 by the ANSI figForth Development Group

  This is a generic file for 32-bit machines with IEEE FP arithmetic (no VMS).
  It only supports indirect threading.
*/

/* Cell and UCell must be the same size as a pointer */
typedef long Cell;
typedef unsigned long UCell;

/* DCell and UDCell must be twice as large as Cell */
typedef long long DCell;
typedef unsigned long long UDCell;

/* define this if IEEE singles and doubles are available as C data types */
#define IEEE_FP

/* the IEEE types are used only for loading and storing */
/* the IEEE double precision type */
typedef double DFloat;
/* the IEEE single precision type */
typedef float SFloat;

#ifdef DIRECT_THREADED
/* If you want direct threading, write a .h file for your processor! */
/* We could put some stuff here that causes a compile error, but then
   we could not use this file in the other machine.h files */
#endif

