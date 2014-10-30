%module omxal
%insert("include")
%{
#include <OMXAL/OpenMAXAL_Platform.h>
#include <OMXAL/OpenMAXAL.h>
#include <OMXAL/OpenMAXAL_Android.h>
%}

#define __ANDROID__
#define ANDROID
#define XA_API
#define const

%include <OMXAL/OpenMAXAL_Platform.h>
%include "OMXAL/OpenMAXAL.h"
%include "OMXAL/OpenMAXAL_Android.h"
