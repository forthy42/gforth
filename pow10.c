#include <math.h>

#ifndef M_LN10
#define M_LN10      2.30258509299404568402
#endif

/* this should be defined by math.h; If it is not, the miranda
   prototype would be wrong; Since we prefer compile-time errors to
   run-time errors, it's declared here. */
extern double exp(double);

double pow10(double x)
{
  return exp(x*M_LN10);
}
