// this file is in the public domain
%module gobject
%insert("include")
%{
#include <glib.h>
#include <glib-object.h>
%}

// exec: sed -e 's/s" gobject"/s" gobject-2.0"/g' -e 's/\(c-function g_once_init\)/\\ \1/g' -e 's/\(c-function [^ ]*_valist\)/\\ \1/g' -e 's/\(c-function g_[^_]*_object\)/\\ \1/g' -e 's/\(c-function g_[^_]*_weak_pointer\)/\\ \1/g' -e 's/\(c-function g_object_compat_control\)/\\ \1/g' -e 's/\(c-function g_value_set_object_take_ownership\)/\\ \1/g' -e 's/\(c-function g_object_newv\)/\\ \1/g'

%apply SWIGTYPE * { gpointer, GClosureNotify, GCallback, GSignalCVaMarshaller, GSignalCMarshaller, GDuplicateFunc };

#define GLIB_AVAILABLE_IN_ALL
#define GLIB_AVAILABLE_IN_2_32
#define GLIB_AVAILABLE_IN_2_34
#define GLIB_AVAILABLE_IN_2_36
#define GLIB_AVAILABLE_IN_2_54
#define GLIB_DEPRECATED_IN_2_54_FOR(x)
#define GLIB_DEPRECATED_FOR(x)
#define GLIB_DEPRECATED
#define G_BEGIN_DECLS
#define G_END_DECLS
#define G_GNUC_CONST const
#define G_GNUC_NULL_TERMINATED

%include <glib.h>
%include <glib/gtypes.h>
%include <glib/gquark.h>
%include <glib-object.h>
%include <glib/gthread.h>
%include <gobject/gobject.h>
%include <gobject/gsignal.h>
