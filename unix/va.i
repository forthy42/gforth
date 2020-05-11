// this file is in the public domain
%module va
%insert("include")
%{
#include <va/va.h>
#include <va/va_backend.h>
%}

%apply long long { int64_t }

// exec: sed -e 's/^c-library/vocabulary va``get-current also va definitions``c-library/g' -e 's/^end-c-library/end-c-library`previous set-current/g' | tr '`' '\n'
// prep: sed -e 's,\(^ *[^} ].*_bit.*$\),// \1,g' -e 's,\(^ *[^} ].*_fields.*$\),// \1,g' -e 's,\(^ *[^} ].*_flags.*$\),// \1,g' -e 's,\(^ *[^} ].*_mb.*$\),// \1,g' -e 's,\(^ *[^} ].*_VAEncFEIMVPredictor.*_ref_idx.*$\),// \1,g' -e 's,\(^ *[^} ].*VADriverVTable.*$\),// \1,g'

%include <va/va.h>
%include <va/va_dec_hevc.h>
%include <va/va_dec_jpeg.h>
%include <va/va_dec_vp8.h>
%include <va/va_dec_vp9.h>
%include <va/va_dec_av1.h>
%include <va/va_enc_hevc.h>
%include <va/va_fei_hevc.h>
%include <va/va_enc_h264.h>
%include <va/va_enc_jpeg.h>
%include <va/va_enc_mpeg2.h>
%include <va/va_enc_vp8.h>
%include <va/va_enc_vp9.h>
%include <va/va_fei.h>
%include <va/va_fei_h264.h>
%include <va/va_vpp.h>
%include <va/va_backend.h>
%include <va/va_backend_wayland.h>
%include <va/va_backend_glx.h>
%include <va/va_backend_vpp.h>
