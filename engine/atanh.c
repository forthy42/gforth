/* replacement for asinh, acosh, and atanh */

#include <math.h>

double atanh(double r1)
{
  double r2=r1 < 0 ? -r1 : r1;
  double r3=log((r2/(1.0-r2)*2)+1)/2;

  return r1 < 0 ? -r3 : r3;
  /* fdup f0< >r fabs 1. d>f fover f- f/  f2* flnp1 f2/
     r> IF  fnegate  THEN ;
     */
}

double asinh(double r1)
{
  return atanh(r1/sqrt(1.0+r1*r1));
  /* fdup fdup f* 1. d>f f+ fsqrt f/ fatanh ; */
}

double acosh(double r1)
{
  return(log(r1+sqrt(r1*r1-1.0)));
  /* fdup fdup f* 1. d>f f- fsqrt f+ fln ; */
}
