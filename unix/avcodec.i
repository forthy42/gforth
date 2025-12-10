// this file is in the public domain
%module avcodec
%insert("include")
%{
#include <libavcodec/avcodec.h>
#include <libavutil/pixfmt.h>
#undef gforth_d2ll
#define gforth_d2ll(x1,x2) av_make_q(x1,x2)
%}

#define attribute_deprecated

// prep: sed -e 's/\(swigFunctionPointer.*{((\([^*]*\)\*)ptr)->\([^}]*\)}.*\)/if(offsetof(\2, \3) >= 0) \1/g'
// exec: sed -e 's/add-lib/add-lib\ns" ((struct AV:x.spx[arg0]" ptr-declare $+[]!/g' -e 's/^\(.*get_encode_buffer.*\)$/\\ \1/g' -e 's/^\(.*\(av_codec_get_pkt_timebase|av_codec_set_pkt_timebase\|av_packet_rescale_ts\|avcodec_decode_audio4\|avcodec_decode_video2\|avcodec_encode_audio2\|avcodec_encode_video2\|avcodec_find_best_pix_fmt2\).*\)/\\ \1/g'

%include <libavcodec/avcodec.h>
%include <libavutil/pixfmt.h>
