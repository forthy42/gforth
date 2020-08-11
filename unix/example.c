/* this file is in the public domain
 *
 * This is an example of how to embed Gforth in a C program and call back
 * C functions within that program
 */

#include <stdio.h>
#include <gforth.h>

double fadd(double x, double y)
{
  double z=x+y;
  printf("Sum: %f=%f+%f\n", z, x, y);
  return z;
}

int iadd(int x, int y)
{
  int z=x+y;
  printf("Sum: %i=%i+%i\n", z, x, y);
  return z;
}

int main(int argc, char** argv, char** env)
{
  Cell retvalue;
  retvalue=gforth_start(argc, argv);
  if(retvalue == -56) { // success is "quit"
    gforth_setwinch();
    gforth_bootmessage();
    retvalue = gforth_quit();
  }
  gforth_cleanup();
  return 0;
}
