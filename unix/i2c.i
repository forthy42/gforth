// this file is in the public domain
%module i2c
%insert("include")
%{
#include <linux/i2c.h>
#include <linux/i2c-dev.h>
#ifdef __gnu_linux__
#include <bits/types/FILE.h>
#undef stderr
extern FILE *stderr;
#endif
%}

#define __user
#define SWIG_FORTH_OPTIONS "no-pre-postfix"

%include <linux/i2c.h>
%include <linux/i2c-dev.h>
