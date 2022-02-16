// this file is in the public domain
%module v4l2
%insert("include")
%{
#include <linux/videodev2.h>
#include <linux/v4l2-mediabus.h>
#include <linux/v4l2-subdev.h>
#include <linux/v4l2-dv-timings.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

// prep: sed -e '0,/_stop;/{s/_raw;/_raw_enc;/}' -e '/_stop;/,/v4l2_format_fmt/{s/_raw;/_raw_dec;/}' -e '/begin-structure v4l2_encoder_cmd/,/end-structure/{s/sizeof( _raw )/sizeof( _raw_enc )/}' -e '/begin-structure v4l2_decoder_cmd/,/end-structure/{s/sizeof( _raw )/sizeof( _raw_dec )/}'

#define SWIG_FORTH_OPTIONS "no-pre-postfix"
#define __inline__
#define __user
#define __attribute__(x)

%include <linux/videodev2.h>
%include <linux/v4l2-common.h>
%include <linux/v4l2-controls.h>
%include <linux/v4l2-mediabus.h>
%include <linux/v4l2-subdev.h>
%include <linux/v4l2-dv-timings.h>

