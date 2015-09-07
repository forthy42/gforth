// this file is in the public domain
%module cpufeatureslib
%insert("include")
%{
#include "cpu-features.h"
%}

#define __BEGIN_CDECLS
#define __END_CDECLS

%include "cpu-features.h"
