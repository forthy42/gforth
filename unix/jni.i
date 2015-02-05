%module jni
%insert("include")
%{
#include <jni.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

// #define SWIG_FORTH_OPTIONS ""

#ifdef host_os_linux_android
#define __attribute__(x)
#define __ANDROID__
#define ANDROID
#define __NDK_FPABI__
#endif
#ifdef host_os_linux_gnu
#define __GNUC__
#define JNIEXPORT
#define JNICALL
#define _CLASSPATH_JNIIMPEXP
#endif
#ifdef host_os_darwin14.0.0
#define __GNUC__
#define JNIEXPORT
#define JNICALL
#define _CLASSPATH_JNIIMPEXP
#endif

// exec: sed -e s/JNINativeInterface_-/JNIEnv-/g -e s/JNINativeInterface-/JNIEnv-/g -e s/JNIInvokeInterface_-/JavaVM-/g -e s/JNIInvokeInterface-/JavaVM-/g -e 's/\(c-function .*\)/\\ \1/g'

%apply char { jbyte, jboolean };
%apply short { jshort, jchar };
%apply int { jint, jsize };
%apply long long { jlong };
%apply float { jfloat };
%apply double { jdouble };
%apply SWIGTYPE * { jobject, va_list };

%include "jni.h"
