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
// prep: sed -e 's/enum \$unnamed[0-9]*\$/int/g' -e 's/^\(.*\(iso8601_to_timespec\|timespec_to_iso8601\|now_to_iso8601\).*\)$/#if GPS_MIN_VER(9,1)`\1`#endif/g'  -e 's/^\(.*\(RTCM3_\|TR_\|GR_\|PR_\|INTERP_\|struct baseline_t\|struct gps_fix_t\|struct gps_log_t\|struct rtcm2_t\|struct rtk_sat_t\|struct rtcm3_msm_sat\|struct rtcm3_msm_sig\|struct rtcm3_msm_hdr\|struct rtcm3_network_rtk_header\|struct rtcm3_1025_t\|struct rtcm3_1023_t\|struct rtcm3_1021_t\|struct orbit\|struct subframe_t\|struct attitude_t\|struct gps_data_t\| RESERVED,\| CORRECT,\| WIDELANE,\| UNCERTAIN,\).*\)$/#if GPS_MIN_VER(14,0)`\1`#endif/g'| tr '`' '\n'
