// this file is in the public domain
%module i2c
%insert("include")
%{
#include <linux/i2c.h>
#include <linux/i2c-dev.h>
%}

// exec: sed -e 's/s" i2c" add-lib//g'

%include <linux/i2c.h>
%include <linux/i2c-dev.h>
