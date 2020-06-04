// this file is in the public domain
%module va
%insert("include")
%{
#include <va/va.h>
#include <va/va_vpp.h>
#include <va/va_backend.h>
%}

%apply long long { int64_t };
%apply int { int16_t, int32_t };
%apply unsigned int { uint16_t, uint32_t };
%apply SWIGTYPE * { VADisplay };

// exec: sed -e 's/add-lib/add-lib`s" ((struct VAD:x.spx[arg0]" ptr-declare $+[]!/g' -e 's/^c-library/vocabulary va``get-current also va definitions``c-library/g' -e 's/^end-c-library/end-c-library`previous set-current/g' | tr '`' '\n'
// prep: sed -e 's,\(^ *[^} ].*_bit.*$\),// \1,g' -e 's,\(^ *[^} ].*_fields.*$\),// \1,g' -e 's,\(^ *[^} ].*_flags.*$\),// \1,g' -e 's,\(^ *[^} ].*_mb.*$\),// \1,g' -e 's,\(^ *[^} ].*_VAEncFEIMVPredictor.*_ref_idx.*$\),// \1,g' -e 's,\(^ *[^} ].*VADriverVTable.*$\),// \1,g'

%include <va/va.h>
%include <va/va_vpp.h>
%include <va/va_backend.h>
%include <va/va_backend_wayland.h>
%include <va/va_backend_glx.h>
%include <va/va_backend_vpp.h>
