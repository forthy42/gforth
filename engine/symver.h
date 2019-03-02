/* specific sym versions for a few symbols */

#ifdef FORCE_SYMVER
__asm__(".symver pow,pow@GLIBC_" #FORCE_SYMVER);
__asm__(".symver exp,exp@GLIBC_" #FORCE_SYMVER);
__asm__(".symver log,log@GLIBC_" #FORCE_SYMVER);
#endif
