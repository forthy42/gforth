%module jni
%insert("include")
%{
#include <jni.h>
%}

#define __ANDROID__
#define ANDROID
#define JNIEXPORT

%include "jni.h"
