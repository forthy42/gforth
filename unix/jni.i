// this file is in the public domain
%module jni
%insert("include")
%{
#define JNINativeInterface_ JNINativeInterface
#define JNIInvokeInterface_ JNIInvokeInterface
#include <jni.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

// #define SWIG_FORTH_OPTIONS ""

#if defined(host_os_linux_android) || defined(host_os_linux_androideabi)
#define __attribute__(x)
#define __ANDROID__
#define ANDROID
#define __NDK_FPABI__
#endif
#if defined(host_os_linux_gnu) || defined(host_os_linux_gnueabi) || defined(host_os_linux_gnueabihf)
#define __GNUC__
#define JNIEXPORT
#define JNICALL
#define _CLASSPATH_JNIIMPEXP
#endif
#ifdef host_os_darwin
#define __GNUC__
#define JNIEXPORT
#define JNICALL
#define _CLASSPATH_JNIIMPEXP
#endif
#define JNINativeInterface_ JNINativeInterface
#define JNIInvokeInterface_ JNIInvokeInterface

// exec: sed -e s/JNINativeInterface-/JNIEnv-/g -e s/JNIInvokeInterface-/JavaVM-/g -e 's/\(c-function .*\)/\\ \1/g' -e 's/\(ReleaseStringUTFChars.*\) a a s/\1 a a a/g'

%apply char { jbyte, jboolean };
%apply short { jshort, jchar };
%apply int { jint, jsize };
%apply long long { jlong };
%apply float { jfloat };
%apply double { jdouble };
%apply SWIGTYPE * { jobject, va_list };

%include "jni.h"
