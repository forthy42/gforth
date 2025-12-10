// this file is in the public domain
%module gps
%insert("include")
%{
#include <gps.h>
#define GPS_MIN_VER(x, y) (GPSD_API_MAJOR_VERSION > x || (GPSD_API_MAJOR_VERSION == x && GPSD_API_MINOR_VERSION >= y))
%}

%apply long { time_t, size_t, ssize_t, watch_t }
%apply unsigned long long { gps_mask_t }
%apply long long { timespec_t }

%include <gps.h>

// exec: sed -e 's/add-lib/add-lib\ns" a" vararg$ $!/g'
// prep: sed -e 's/\(swigFunctionPointer.*{((\([^*]*\)\*)ptr)->\([^}]*\)}.*\)/if(offsetof(\2, \3) >= 0) \1/g' -e 's/enum \$unnamed[0-9]*\$/int/g' -e 's/^\(.*\(now_to_iso8601\).*\)$/#if GPS_MIN_VER(9,1)\n\1\n#endif/g' -e 's/ d / t{timespec_t} /g' -e 's/ -- d/ -- t{*(timespec_t*)}/g'
