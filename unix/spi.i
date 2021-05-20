// this file is in the public domain
%module spi
%insert("include")
%{
#include <linux/spi/spidev.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

#define SWIG_FORTH_OPTIONS "no-pre-postfix"

%include <linux/spi/spidev.h>
