// this file is in the public domain
%module gobject
%insert("include")
%{
#include <glib.h>
#include <glib-object.h>
%}

// exec: sed -e 's/s" gobject"/s" gobject-2.0"/g' -e 's/\(c-function g_once_init\)/\\ \1/g' -e 's/\(c-function [^ ]*_valist\)/\\ \1/g' -e 's/\(c-function g_[^_]*_object\)/\\ \1/g' -e 's/\(c-function g_clear_handle_id\)/\\ \1/g' -e 's/\(c-function g_main_context_wait\)/\\ \1/g' -e 's/\(c-function g_source_get_current_time\)/\\ \1/g' -e 's/\(c-function g_[^_]*_weak_pointer\)/\\ \1/g' -e 's/\(c-function g_object_compat_control\)/\\ \1/g' -e 's/\(c-function g_value_set_object_take_ownership\)/\\ \1/g' -e 's/\(c-function g_object_newv\)/\\ \1/g' -e 's/add-lib/add-lib`s" a 0" vararg$ $!/g' -e 's/g_clear_signal_handler a a/g_clear_signal_handler a{(gulong*)} a/g' | tr '`' '\n'
// prep: sed -e 's/swigFunctionPointer.*{((\([^*]*\)\*)ptr)->\([^}]*\)}.*/if(offsetof(\1, \2) >= 0) \0/g'

%apply SWIGTYPE * { gpointer, GClosureNotify, GCallback, GSignalCVaMarshaller, GSignalCMarshaller, GDuplicateFunc, GPollFunc };

#define GOBJECT_AVAILABLE_IN_ALL
#define GOBJECT_AVAILABLE_IN_2_32
#define GOBJECT_AVAILABLE_IN_2_34
#define GOBJECT_AVAILABLE_IN_2_36
#define GOBJECT_AVAILABLE_IN_2_38
#define GOBJECT_AVAILABLE_IN_2_42
#define GOBJECT_AVAILABLE_IN_2_44
#define GOBJECT_AVAILABLE_IN_2_54
#define GOBJECT_AVAILABLE_IN_2_56
#define GOBJECT_AVAILABLE_IN_2_62
#define GOBJECT_AVAILABLE_IN_2_64
#define GOBJECT_AVAILABLE_IN_2_66
#define GOBJECT_AVAILABLE_IN_2_68
#define GOBJECT_AVAILABLE_IN_2_70
#define GOBJECT_AVAILABLE_IN_2_72
#define GOBJECT_AVAILABLE_IN_2_74
#define GOBJECT_AVAILABLE_TYPE_IN_2_64
#define GOBJECT_AVAILABLE_TYPE_IN_2_72
#define GOBJECT_DEPRECATED_FOR(x)
#define GOBJECT_DEPRECATED_IN_2_28_FOR(x) GOBJECT_DEPRECATED_FOR(x)
#define GOBJECT_DEPRECATED_IN_2_54_FOR(x) GOBJECT_DEPRECATED_FOR(x)
#define GOBJECT_DEPRECATED_IN_2_58_FOR(x) GOBJECT_DEPRECATED_FOR(x)
#define GOBJECT_DEPRECATED_IN_2_62_FOR(x) GOBJECT_DEPRECATED_FOR(x)
#define GOBJECT_DEPRECATED_TYPE_IN_2_62_FOR(x) GOBJECT_DEPRECATED_FOR(x)
#define GOBJECT_DEPRECATED_IN_2_36
#define GOBJECT_DEPRECATED_TYPE_IN_2_36
#define GOBJECT_DEPRECATED_IN_2_58
#define GOBJECT_DEPRECATED
#define GOBJECT_AVAILABLE_STATIC_INLINE_IN_2_44
#define GOBJECT_AVAILABLE_STATIC_INLINE_IN_2_60
#define GOBJECT_AVAILABLE_STATIC_INLINE_IN_2_62
#define GOBJECT_AVAILABLE_STATIC_INLINE_IN_2_64
#define GOBJECT_AVAILABLE_STATIC_INLINE_IN_2_70
#define GOBJECT_AVAILABLE_ENUMERATOR_IN_2_70
#define GOBJECT_AVAILABLE_ENUMERATOR_IN_2_74
#define GOBJECT_AVAILABLE_ENUMERATOR_IN_2_76
#define GOBJECT_SYSDEF_POLLIN =1
#define GOBJECT_SYSDEF_POLLOUT =4
#define GOBJECT_SYSDEF_POLLPRI =2
#define GOBJECT_SYSDEF_POLLHUP =16
#define GOBJECT_SYSDEF_POLLERR =8
#define GOBJECT_SYSDEF_POLLNVAL =32
#define GOBJECT_VAR extern
#define G_NORETURN
#define GLIB_AVAILABLE_IN_ALL
#define GLIB_AVAILABLE_IN_2_32
#define GLIB_AVAILABLE_IN_2_34
#define GLIB_AVAILABLE_IN_2_36
#define GLIB_AVAILABLE_IN_2_38
#define GLIB_AVAILABLE_IN_2_42
#define GLIB_AVAILABLE_IN_2_44
#define GLIB_AVAILABLE_IN_2_54
#define GLIB_AVAILABLE_IN_2_56
#define GLIB_AVAILABLE_IN_2_62
#define GLIB_AVAILABLE_IN_2_64
#define GLIB_AVAILABLE_IN_2_66
#define GLIB_AVAILABLE_IN_2_68
#define GLIB_AVAILABLE_IN_2_70
#define GLIB_AVAILABLE_IN_2_72
#define GLIB_AVAILABLE_IN_2_74
#define GLIB_AVAILABLE_TYPE_IN_2_64
#define GLIB_AVAILABLE_TYPE_IN_2_72
#define GLIB_DEPRECATED_FOR(x)
#define GLIB_DEPRECATED_IN_2_28_FOR(x) GLIB_DEPRECATED_FOR(x)
#define GLIB_DEPRECATED_IN_2_54_FOR(x) GLIB_DEPRECATED_FOR(x)
#define GLIB_DEPRECATED_IN_2_58_FOR(x) GLIB_DEPRECATED_FOR(x)
#define GLIB_DEPRECATED_IN_2_62_FOR(x) GLIB_DEPRECATED_FOR(x)
#define GLIB_DEPRECATED_TYPE_IN_2_62_FOR(x) GLIB_DEPRECATED_FOR(x)
#define GLIB_DEPRECATED_IN_2_36
#define GLIB_DEPRECATED_TYPE_IN_2_36
#define GLIB_DEPRECATED_IN_2_58
#define GLIB_DEPRECATED
#define GLIB_AVAILABLE_STATIC_INLINE_IN_2_44
#define GLIB_AVAILABLE_STATIC_INLINE_IN_2_60
#define GLIB_AVAILABLE_STATIC_INLINE_IN_2_62
#define GLIB_AVAILABLE_STATIC_INLINE_IN_2_64
#define GLIB_AVAILABLE_STATIC_INLINE_IN_2_70
#define GLIB_AVAILABLE_ENUMERATOR_IN_2_70
#define GLIB_AVAILABLE_ENUMERATOR_IN_2_74
#define G_BEGIN_DECLS
#define G_END_DECLS
#define G_GNUC_BEGIN_IGNORE_DEPRECATIONS
#define G_GNUC_END_IGNORE_DEPRECATIONS
#define G_GNUC_CONST const
#define G_GNUC_NULL_TERMINATED
#define GLIB_SYSDEF_POLLIN =1
#define GLIB_SYSDEF_POLLOUT =4
#define GLIB_SYSDEF_POLLPRI =2
#define GLIB_SYSDEF_POLLHUP =16
#define GLIB_SYSDEF_POLLERR =8
#define GLIB_SYSDEF_POLLNVAL =32
#define GLIB_VAR extern
#define G_GNUC_PURE
#define G_DEFINE_AUTOPTR_CLEANUP_FUNC(x,y)
#define G_DEFINE_AUTO_CLEANUP_FREE_FUNC(x,y,z)

%include <glib.h>
%include <glib/gtypes.h>
%include <glib/gquark.h>
%include <glib/gthread.h>
%include <glib/gmain.h>
%include <glib-object.h>
%include <gobject/gtype.h>
%include <gobject/gobject.h>
%include <gobject/gsignal.h>
