// this file is in the public domain
%module gps
%insert("include")
%{
#include <gps.h>
#define GPS_MIN_VER(x, y) (GPSD_API_MAJOR_VERSION > x || (GPSD_API_MAJOR_VERSION == x && GPSD_API_MINOR_VERSION >= y))
%}

%apply long { time_t, size_t }
%apply unsigned long long { gps_mask_t }

%include <gps.h>

// exec: sed -e 's/add-lib/add-lib`s" a" vararg$ $!/g' | tr '`' '\n'
// prep: sed -e 's/swigFunctionPointer.*{((\([^*]*\)\*)ptr)->\([^}]*\)}.*/if(offsetof(\1, \2) >= 0) \0/g' -e 's/enum \$unnamed[0-9]*\$/int/g' -e 's/^\(.*\(now_to_iso8601\).*\)$/#if GPS_MIN_VER(9,1)`\1`#endif/g' -e 's/^\(.*\(iso8601_to_timespec\|timespec_to_iso8601\).*\)$/#if 0`\1`#endif/g' | tr '`' '\n'
