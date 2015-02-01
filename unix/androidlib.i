%module androidlib
%insert("include")
%{
#include <android/input.h>
#include <android/keycodes.h>
#include <android/native_window.h>
#include <android/native_window_jni.h>
#include <android/native_activity.h>
#include <android/looper.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

#define __ANDROID__
#define ANDROID
#define __attribute__(x)
#define __NDK_FPABI__

// exec: sed -e 's/\(c-function ANativeActivity_onCreate.*\)/\\ \1/g'

%apply int { int32_t, uint32_t, size_t };
%apply long long { int64_t, uint64_t };
%apply SWIGTYPE * { ALooper_callbackFunc };

%include <android/input.h>
%include <android/keycodes.h>
%include <android/native_window.h>
%include <android/native_window_jni.h>
%include <android/native_activity.h>
%include <android/looper.h>
