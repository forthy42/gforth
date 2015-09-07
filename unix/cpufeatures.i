// this file is in the public domain
%module cpufeatureslib
%insert("include")
%{
#include "cpu-features.h"
%}

#define __BEGIN_DECLS
#define __END_DECLS

%include "cpu-features.h"
