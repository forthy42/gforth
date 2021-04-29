// this file is in the public domain
%module spi
%insert("include")
%{
#include <linux/spi/spidev.h>
%}

#define SWIG_FORTH_OPTIONS "no-pre-postfix"

%include <linux/spi/spidev.h>
