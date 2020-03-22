// this file is in the public domain
%module gps
%insert("include")
%{
#include <gps.h>
%}

%apply long { time_t, size_t }
%apply unsigned long long { gps_mask_t }

%include <gps.h>

// exec: sed -e 's/add-lib/add-lib`s" a" vararg$ $!/g' -e 's/\(c-function .*iso8601.*\)/\\ \1/g' | tr '`' '\n'
// prep: sed -e 's/enum \$unnamed[0-9]*\$/int/g'
