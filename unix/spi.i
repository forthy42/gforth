// this file is in the public domain
%module spi
%insert("include")
%{
#include <linux/spi/spidev.h>
%}

%include <linux/spi/spidev.h>
