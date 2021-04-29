// this file is in the public domain
%module spi
%insert("include")
%{
#include <linux/spi/spidev.h>
%}

// exec: sed -e 's/s" spi" add-lib//g'

%include <linux/spi/spidev.h>
