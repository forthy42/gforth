// this file is in the public domain
%module i2c
%insert("include")
%{
#include <linux/i2c.h>
#include <linux/i2c-dev.h>
%}

#define SWIG_FORTH_OPTIONS "no-pre-postfix"

%include <linux/i2c.h>
%include <linux/i2c-dev.h>
