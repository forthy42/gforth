%module jni
%insert("include")
%{
#include <jni.h>
%}

#define SWIG_FORTH_OPTIONS "no-callbacks"

#ifdef host_os_linux_android
#define __attribute__(x)
#define __ANDROID__
#define ANDROID
#define __NDK_FPABI__
#endif
#ifdef host_os_linux
#define __GNUC__
#define JNIEXPORT
#define JNICALL
#define _CLASSPATH_JNIIMPEXP
#endif

%apply char { jbyte, jboolean };
%apply short { jshort, jchar };
%apply int { jint, jsize };
%apply long long { jlong };
%apply float { jfloat };
%apply double { jdouble };
%apply SWIGTYPE * { jobject, va_list };

%include "jni.h"
