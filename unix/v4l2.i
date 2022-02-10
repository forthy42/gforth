// this file is in the public domain
%module v4l2
%insert("include")
%{
#include <linux/v4l2-common.h>
#include <linux/v4l2-controls.h>
#include <linux/v4l2-mediabus.h>
#include <linux/v4l2-subdev.h>
#include <linux/v4l2-dv-timings.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

#define SWIG_FORTH_OPTIONS "no-pre-postfix"

%include <linux/v4l2-common.h>
%include <linux/v4l2-controls.h>
%include <linux/v4l2-mediabus.h>
%include <linux/v4l2-subdev.h>
%include <linux/v4l2-dv-timings.h>

