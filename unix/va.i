// this file is in the public domain
%module va
%insert("include")
%{
#include <va/va.h>
#include <va/va_vpp.h>
#include <va/va_backend.h>
// #include <va/va_dec_av1.h>
#include <va/va_dec_hevc.h>
#include <va/va_dec_jpeg.h>
#include <va/va_dec_vp8.h>
#include <va/va_dec_vp9.h>
// #include <va/va_enc_av1.h>
#include <va/va_enc_h264.h>
#include <va/va_enc_hevc.h>
#include <va/va_enc_jpeg.h>
#include <va/va_enc_mpeg2.h>
#include <va/va_enc_vp8.h>
#include <va/va_enc_vp9.h>
%}

%apply long long { int64_t };
%apply unsigned long long { uint64_t };
%apply int { int16_t, int32_t };
%apply unsigned int { uint16_t, uint32_t };
%apply unsigned char { uint8_t };
%apply SWIGTYPE * { VADisplay };

// exec: sed -e 's/add-lib/add-lib\ns" ((struct VAD:x.spx[arg0]" ptr-declare $+[]!/g' -e 's/^c-library/cs-vocabulary va\nget-current >r also va definitions\n\nc-library/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g'
// prep: sed -e 's/swigFunctionPointer.*{((\([^*]*\)\*)ptr)->\([^}]*\)}.*/if(offsetof(\1, \2) >= 0) \0/g' -e 's,\(^ *[^} ].*_bit.*$\),// \1,g' -e 's,\(^ *[^} ].*_fields.*$\),// \1,g' -e 's,\(^ *[^} ].*_flags.*$\),// \1,g' -e 's,\(^ *[^} ].*_mb.*$\),// \1,g' -e 's,\(^ *[^} ].*_VAEncFEIMVPredictor.*_ref_idx.*$\),// \1,g' -e 's,\(^ *[^} ].*VADriverVTable.*$\),// \1,g'

%include <va/va.h>
%include <va/va_vpp.h>
%include <va/va_backend.h>
%include <va/va_backend_wayland.h>
%include <va/va_backend_glx.h>
%include <va/va_backend_vpp.h>
// %include <va/va_dec_av1.h>
%include <va/va_dec_hevc.h>
%include <va/va_dec_jpeg.h>
%include <va/va_dec_vp8.h>
%include <va/va_dec_vp9.h>
// %include <va/va_enc_av1.h>
%include <va/va_enc_h264.h>
%include <va/va_enc_hevc.h>
%include <va/va_enc_jpeg.h>
%include <va/va_enc_mpeg2.h>
%include <va/va_enc_vp8.h>
%include <va/va_enc_vp9.h>
