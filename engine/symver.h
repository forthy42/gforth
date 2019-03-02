/* specific sym versions for a few symbols */

#ifdef FORCE_SYMVER
__asm__(".symver pow,pow@GLIBC_2.2.5");
__asm__(".symver exp,exp@GLIBC_2.2.5");
__asm__(".symver log,log@GLIBC_2.2.5");
#endif
