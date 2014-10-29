%module jni
%insert("include")
%{
#include <jni.h>
%}

#define SWIG_FORTH_OPTIONS "no-callbacks"

#define __ANDROID__
#define __GNUC__
#define ANDROID
#define JNIEXPORT
#define JNICALL
#define _CLASSPATH_JNIIMPEXP

%apply char { jbyte, jboolean };
%apply short { jshort, jchar };
%apply int { jint, jsize };
%apply long long { jlong };
%apply float { jfloat };
%apply double { jdouble };
%apply SWIGTYPE * { jobject, va_list };

%include "jni.h"
