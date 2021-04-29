// this file is in the public domain
%module i2c
%insert("include")
%{
#include <linux/i2c.h>
#include <linux/i2c-dev.h>
%}

%include <linux/i2c.h>
%include <linux/i2c-dev.h>
