/* specific sym versions for a few symbols, this is in the public domain
 *
 * Reason: glibc 2.29 changed the symbol version of pow/exp/log, apparently
 * for performance reasons.  Instead of just changing the code, they kept the
 * old versions available, and introduced a new symbol version for the faster
 * code; probably fearing that old programs might not be happy with the new
 * code.
 *
 * So you can't compile code on a new OS that already has the new glibc, and
 * run it on an old OS that doesn't.  Installing several glibcs alongside is
 * also not supported well (essentially, you need a chroot or a container for
 * that).  So well, as workaround, just like people did for the memmove case
 * in glibc 2.14, you can define the symver explicitely.  Note that only a few
 * platforms actually support that stuff, and they have different versions for
 * th older symbol.  Therefore, we don't activate that here, you need a GCC
 * option.
 *
 * For x86 architecture, configure with CC="gcc -m32 -DFORCE_SYMVER=2.0".
 * For amd64 architecture, configure with CC="gcc -DFORCE_SYMVER=2.2.5".
 * For armhf architecture, configure with CC="gcc -DFORCE_SYMVER=2.4".
 * For arm64 architecture, configure with CC="gcc -DFORCE_SYMVER=2.17".
 */

#define TOSTRING(x) #x
#define STRINGIFY(x) TOSTRING(x) /* Two stages necessary */
#ifdef FORCE_SYMVER
__asm__(".symver pow,pow@GLIBC_" STRINGIFY(FORCE_SYMVER));
__asm__(".symver exp,exp@GLIBC_" STRINGIFY(FORCE_SYMVER));
__asm__(".symver log,log@GLIBC_" STRINGIFY(FORCE_SYMVER));
#ifdef __x86_64
__asm__(".symver memcpy,memcpy@GLIBC_" STRINGIFY(FORCE_SYMVER));
#endif
#endif
