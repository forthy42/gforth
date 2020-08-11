// this file is in the public domain
%module example
%insert("include")
%{
  #include "example.h"
%}

// exec: sed -e 's/s" example" add-lib/s" ." add-incdir/g'

%include "example.h"
