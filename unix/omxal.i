%module omxal
%insert("include")
%{
#include <OMXAL/OpenMAXAL_Platform.h>
#include <OMXAL/OpenMAXAL.h>
#include <OMXAL/OpenMAXAL_Android.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

#define __ANDROID__
#define ANDROID
#define XA_API
#define const

// exec: sed -e s/Itf_-/Itf-/g -e s/ID_-/ID-/g -e s/c-callback/c-callback-thread/g

%include "OMXAL/OpenMAXAL_Platform.h"
%include "OMXAL/OpenMAXAL.h"
%include "OMXAL/OpenMAXAL_Android.h"
