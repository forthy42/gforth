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
// prep: sed -e 's/swigFunctionPointer.*{((\([^*]*\)\*)ptr)->\([^}]*\)}.*/if(offsetof(\1, \2) >= 0) \0/g' -e 's/enum \$unnamed[0-9]*\$/int/g' -e 's/^\(.*\(now_to_iso8601\).*\)$/#if GPS_MIN_VER(9,1)`\1`#endif/g' -e 's/^\(.*\(iso8601_to_timespec\|timespec_to_iso8601\).*\)$/#if 0`\1`#endif/g' -e 's/^\(.*\(enum RTCM3_\|TR_\|GR_\|PR_\|INTERP_\|struct[ure]* baseline_t\|struct[ure]* gps_log_t\|struct[ure]* rtk_sat_t\|struct[ure]* rtcm3_msm_sat\|struct[ure]* rtcm3_msm_sig\|struct[ure]* rtcm3_msm_hdr\|struct[ure]* rtcm3_network_rtk_header\|struct[ure]* rtcm3_1025_t\|struct[ure]* rtcm3_1023_t\|struct[ure]* rtcm3_1021_t\|struct[ure]* orbit\|struct[ure]* subframe_t\|struct[ure]* attitude_t\|gps_data_t-[^f]\|gps_fix_t-status\|gps_fix_t-altitude\|gps_fix_t-wanglem\|gps_fix_t-wangler\|gps_fix_t-wanglet\|gps_fix_t-wspeedr\|gps_fix_t-wspeedt\|gps_fix_t-base\|gps_fix_t-wtemp\|satellite_t-qualityInd\|satellite_t-prRes\|attitude_t-mheading\|attitude_t-rot\|fixsource_t\|privdata_t\|watch_t\| RESERVED,\| CORRECT,\| WIDELANE,\| UNCERTAIN,\).*\)$/#if GPS_MIN_VER(14,0)`\1`#endif/g'| tr '`' '\n'
