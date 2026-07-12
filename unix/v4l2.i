// this file is in the public domain
%module v4l2
%insert("include")
%{
#include <linux/videodev2.h>
#include <linux/v4l2-mediabus.h>
#include <linux/v4l2-subdev.h>
#include <linux/v4l2-dv-timings.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

// prep: sed -e '0,/_stop;/{s/_raw;/_raw_enc;/}' -e '/_stop;/,/v4l2_format_fmt/{s/_raw;/_raw_dec;/}' -e '/begin-structure v4l2_encoder_cmd/,/end-structure/{s/sizeof( _raw )/sizeof( _raw_enc )/}' -e '/begin-structure v4l2_decoder_cmd/,/end-structure/{s/sizeof( _raw )/sizeof( _raw_dec )/}' -e 's/c-function/\\\\ c-function/g'

#define SWIG_FORTH_OPTIONS "no-pre-postfix"
#define __inline__
#define __user
#define __attribute__(x)

%constant VIDIOC_QUERYCAP		=_IOR('V',  0, struct v4l2_capability);
%constant VIDIOC_ENUM_FMT		=_IOWR('V',  2, struct v4l2_fmtdesc);
%constant VIDIOC_G_FMT			=_IOWR('V',  4, struct v4l2_format);
%constant VIDIOC_S_FMT			=_IOWR('V',  5, struct v4l2_format);
%constant VIDIOC_REQBUFS		=_IOWR('V',  8, struct v4l2_requestbuffers);
%constant VIDIOC_QUERYBUF		=_IOWR('V',  9, struct v4l2_buffer);
%constant VIDIOC_G_FBUF			=_IOR('V', 10, struct v4l2_framebuffer);
%constant VIDIOC_S_FBUF			=_IOW('V', 11, struct v4l2_framebuffer);
%constant VIDIOC_OVERLAY		=_IOW('V', 14, int);
%constant VIDIOC_QBUF			=_IOWR('V', 15, struct v4l2_buffer);
%constant VIDIOC_EXPBUF			=_IOWR('V', 16, struct v4l2_exportbuffer);
%constant VIDIOC_DQBUF			=_IOWR('V', 17, struct v4l2_buffer);
%constant VIDIOC_STREAMON		=_IOW('V', 18, int);
%constant VIDIOC_STREAMOFF		=_IOW('V', 19, int);
%constant VIDIOC_G_PARM			=_IOWR('V', 21, struct v4l2_streamparm);
%constant VIDIOC_S_PARM			=_IOWR('V', 22, struct v4l2_streamparm);
%constant VIDIOC_G_STD			=_IOR('V', 23, v4l2_std_id);
%constant VIDIOC_S_STD			=_IOW('V', 24, v4l2_std_id);
%constant VIDIOC_ENUMSTD		=_IOWR('V', 25, struct v4l2_standard);
%constant VIDIOC_ENUMINPUT		=_IOWR('V', 26, struct v4l2_input);
%constant VIDIOC_G_CTRL			=_IOWR('V', 27, struct v4l2_control);
%constant VIDIOC_S_CTRL			=_IOWR('V', 28, struct v4l2_control);
%constant VIDIOC_G_TUNER		=_IOWR('V', 29, struct v4l2_tuner);
%constant VIDIOC_S_TUNER		=_IOW('V', 30, struct v4l2_tuner);
%constant VIDIOC_G_AUDIO		=_IOR('V', 33, struct v4l2_audio);
%constant VIDIOC_S_AUDIO		=_IOW('V', 34, struct v4l2_audio);
%constant VIDIOC_QUERYCTRL		=_IOWR('V', 36, struct v4l2_queryctrl);
%constant VIDIOC_QUERYMENU		=_IOWR('V', 37, struct v4l2_querymenu);
%constant VIDIOC_G_INPUT		=_IOR('V', 38, int);
%constant VIDIOC_S_INPUT		=_IOWR('V', 39, int);
%constant VIDIOC_G_EDID			=_IOWR('V', 40, struct v4l2_edid);
%constant VIDIOC_S_EDID			=_IOWR('V', 41, struct v4l2_edid);
%constant VIDIOC_G_OUTPUT		=_IOR('V', 46, int);
%constant VIDIOC_S_OUTPUT		=_IOWR('V', 47, int);
%constant VIDIOC_ENUMOUTPUT		=_IOWR('V', 48, struct v4l2_output);
%constant VIDIOC_G_AUDOUT		=_IOR('V', 49, struct v4l2_audioout);
%constant VIDIOC_S_AUDOUT		=_IOW('V', 50, struct v4l2_audioout);
%constant VIDIOC_G_MODULATOR		=_IOWR('V', 54, struct v4l2_modulator);
%constant VIDIOC_S_MODULATOR		=_IOW('V', 55, struct v4l2_modulator);
%constant VIDIOC_G_FREQUENCY		=_IOWR('V', 56, struct v4l2_frequency);
%constant VIDIOC_S_FREQUENCY		=_IOW('V', 57, struct v4l2_frequency);
%constant VIDIOC_CROPCAP		=_IOWR('V', 58, struct v4l2_cropcap);
%constant VIDIOC_G_CROP			=_IOWR('V', 59, struct v4l2_crop);
%constant VIDIOC_S_CROP			=_IOW('V', 60, struct v4l2_crop);
%constant VIDIOC_G_JPEGCOMP		=_IOR('V', 61, struct v4l2_jpegcompression);
%constant VIDIOC_S_JPEGCOMP		=_IOW('V', 62, struct v4l2_jpegcompression);
%constant VIDIOC_QUERYSTD		=_IOR('V', 63, v4l2_std_id);
%constant VIDIOC_TRY_FMT		=_IOWR('V', 64, struct v4l2_format);
%constant VIDIOC_ENUMAUDIO		=_IOWR('V', 65, struct v4l2_audio);
%constant VIDIOC_ENUMAUDOUT		=_IOWR('V', 66, struct v4l2_audioout);
%constant VIDIOC_G_PRIORITY		=_IOR('V', 67, __u32); /* enum v4l2_priority */
%constant VIDIOC_S_PRIORITY		=_IOW('V', 68, __u32); /* enum v4l2_priority */
%constant VIDIOC_G_SLICED_VBI_CAP	=_IOWR('V', 69, struct v4l2_sliced_vbi_cap);
%constant VIDIOC_LOG_STATUS		=_IO('V', 70);
%constant VIDIOC_G_EXT_CTRLS		=_IOWR('V', 71, struct v4l2_ext_controls);
%constant VIDIOC_S_EXT_CTRLS		=_IOWR('V', 72, struct v4l2_ext_controls);
%constant VIDIOC_TRY_EXT_CTRLS		=_IOWR('V', 73, struct v4l2_ext_controls);
%constant VIDIOC_ENUM_FRAMESIZES	=_IOWR('V', 74, struct v4l2_frmsizeenum);
%constant VIDIOC_ENUM_FRAMEINTERVALS	=_IOWR('V', 75, struct v4l2_frmivalenum);
%constant VIDIOC_G_ENC_INDEX		=_IOR('V', 76, struct v4l2_enc_idx);
%constant VIDIOC_ENCODER_CMD		=_IOWR('V', 77, struct v4l2_encoder_cmd);
%constant VIDIOC_TRY_ENCODER_CMD	=_IOWR('V', 78, struct v4l2_encoder_cmd);

/*
 * Experimental, meant for debugging, testing and internal use.
 * Only implemented if CONFIG_VIDEO_ADV_DEBUG is defined.
 * You must be root to use these ioctls. Never use these in applications!
 */
%constant VIDIOC_DBG_S_REGISTER		=_IOW('V', 79, struct v4l2_dbg_register);
%constant VIDIOC_DBG_G_REGISTER		=_IOWR('V', 80, struct v4l2_dbg_register);

%constant VIDIOC_S_HW_FREQ_SEEK		=_IOW('V', 82, struct v4l2_hw_freq_seek);
%constant VIDIOC_S_DV_TIMINGS		=_IOWR('V', 87, struct v4l2_dv_timings);
%constant VIDIOC_G_DV_TIMINGS		=_IOWR('V', 88, struct v4l2_dv_timings);
%constant VIDIOC_DQEVENT		=_IOR('V', 89, struct v4l2_event);
%constant VIDIOC_SUBSCRIBE_EVENT	=_IOW('V', 90, struct v4l2_event_subscription);
%constant VIDIOC_UNSUBSCRIBE_EVENT	=_IOW('V', 91, struct v4l2_event_subscription);
%constant VIDIOC_CREATE_BUFS		=_IOWR('V', 92, struct v4l2_create_buffers);
%constant VIDIOC_PREPARE_BUF		=_IOWR('V', 93, struct v4l2_buffer);
%constant VIDIOC_G_SELECTION		=_IOWR('V', 94, struct v4l2_selection);
%constant VIDIOC_S_SELECTION		=_IOWR('V', 95, struct v4l2_selection);
%constant VIDIOC_DECODER_CMD		=_IOWR('V', 96, struct v4l2_decoder_cmd);
%constant VIDIOC_TRY_DECODER_CMD	=_IOWR('V', 97, struct v4l2_decoder_cmd);
%constant VIDIOC_ENUM_DV_TIMINGS	=_IOWR('V', 98, struct v4l2_enum_dv_timings);
%constant VIDIOC_QUERY_DV_TIMINGS	=_IOR('V', 99, struct v4l2_dv_timings);
%constant VIDIOC_DV_TIMINGS_CAP		=_IOWR('V', 100, struct v4l2_dv_timings_cap);
%constant VIDIOC_ENUM_FREQ_BANDS	=_IOWR('V', 101, struct v4l2_frequency_band);

/*
 * Experimental, meant for debugging, testing and internal use.
 * Never use this in applications!
 */
%constant VIDIOC_DBG_G_CHIP_INFO	=_IOWR('V', 102, struct v4l2_dbg_chip_info);

%constant VIDIOC_QUERY_EXT_CTRL		=_IOWR('V', 103, struct v4l2_query_ext_ctrl);
%constant VIDIOC_REMOVE_BUFS		=_IOWR('V', 104, struct v4l2_remove_buffers);


%include <linux/videodev2.h>
%include <linux/v4l2-common.h>
%include <linux/v4l2-controls.h>
%include <linux/v4l2-mediabus.h>
%include <linux/v4l2-subdev.h>
%include <linux/v4l2-dv-timings.h>

