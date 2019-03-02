/* specific sym versions for a few symbols */

#define TOSTRING(x) #x
#define STRINGIFY(x) TOSTRING(x)
#ifdef FORCE_SYMVER
__asm__(".symver pow,pow@GLIBC_" STRINGIFY(FORCE_SYMVER));
__asm__(".symver exp,exp@GLIBC_" STRINGIFY(FORCE_SYMVER));
__asm__(".symver log,log@GLIBC_" STRINGIFY(FORCE_SYMVER));
#endif
