/* this file is in the public domain
 *
 * This is an example of how to embed Gforth in a C program and call back
 * C functions within that program
 */

extern double fadd(double, double);
extern int iadd(int, int);
typedef struct { int a; int b; } twoint;
extern twoint tadd(twoint, twoint);
