// this file is in the public domain
%module gst
%insert("include")
%{
#define GST_USE_UNSTABLE_API
#include <gst/gst.h>
#include <gst/gl/gl.h>
#include <gst/gl/egl/gstgldisplay_egl.h>
#include <gst/gl/x11/gstgldisplay_x11.h>
%}

#define GST_API
#define GST_GL_API
#define GST_DEPRECATED_FOR(x)
#define GST_USE_UNSTABLE_API
#define G_BEGIN_DECLS
#define G_END_DECLS
#define G_GNUC_MALLOC
#define GST_EXPORT
#define G_GNUC_WARN_UNUSED_RESULT
#define G_GNUC_NULL_TERMINATED
#define G_GNUC_WARN_UNUSED_RESULT
#define G_GNUC_PRINTF(a,b)
#define G_GNUC_NO_INSTRUMENT
#define G_GNUC_CONST const
#define G_GNUC_INTERNAL
#define gchar char

// exec: sed -e 's/\(c-function .*_valist\)/\\ \1/g' -e 's/\(c-function _gst_gl_feature_check\)/\\ \1/g' -e 's/add-lib/add-lib`s" gstgl-1.0" add-lib`8 to callback#`s" a 0" vararg$ $!/g' -e 's/end-c-library/2 to callback#`end-c-library/g' -e 's/\(.* callbacks .*\)/\1`c-callback reshapeCallback: u u a -- void`c-callback drawCallback: u u u a -- void/g' -e 's/c-function gst_clear_/\\ c-function gst_clear_/g' | tr '`' '\n'

%apply unsigned char { guint8 }
%apply int { gboolean, gint, GLint, gint32 }
%apply unsigned int { GType, guint, gsize, GstFormat, GLuint, GstGLFormat, guint32, GstGLTextureTarget, guint16 }
%apply unsigned long { guintptr }
%apply long long { gint64 }
%apply unsigned long long { guint64 }
%apply SWIGTYPE * { gpointer, gconstpointer, GDestroyNotify, GstMiniObjectNotify, GCompareFunc, GCompareDataFunc, va_list, GCallback, GstPluginFeatureFilter, GstGLAllocationParamsFreeFunc, GstGLAllocationParamsCopyFunc }
%apply float { gfloat }
%apply double { gdouble }

#define SWIG_FORTH_GFORTH_LIBRARY "gstreamer-1.0"

%include <gst/gst.h>
%include <gst/glib-compat.h>
%include <gst/gstenumtypes.h>
%include <gst/gstversion.h>
%include <gst/gstatomicqueue.h>
%include <gst/gstbin.h>
%include <gst/gstbuffer.h>
%include <gst/gstbufferlist.h>
%include <gst/gstbufferpool.h>
%include <gst/gstbus.h>
%include <gst/gstcaps.h>
%include <gst/gstcapsfeatures.h>
%include <gst/gstchildproxy.h>
%include <gst/gstclock.h>
%include <gst/gstcontrolsource.h>
%include <gst/gstdatetime.h>
%include <gst/gstdebugutils.h>
%include <gst/gstdevice.h>
%include <gst/gstdevicemonitor.h>
%include <gst/gstdeviceprovider.h>
 // %include <gst/gstdynamictypefactory.h>
%include <gst/gstelement.h>
%include <gst/gstelementmetadata.h>
%include <gst/gsterror.h>
%include <gst/gstevent.h>
%include <gst/gstghostpad.h>
%include <gst/gstiterator.h>
%include <gst/gstmessage.h>
%include <gst/gstmemory.h>
%include <gst/gstmeta.h>
%include <gst/gstminiobject.h>
%include <gst/gstobject.h>
 // %include <gst/gststreamcollection.h>
%include <gst/gstpad.h>
%include <gst/gstparamspecs.h>
%include <gst/gstpipeline.h>
%include <gst/gstplugin.h>
%include <gst/gstpoll.h>
%include <gst/gstpreset.h>
 // %include <gst/gstprotection.h>
%include <gst/gstquery.h>
%include <gst/gstregistry.h>
%include <gst/gstsample.h>
%include <gst/gstsegment.h>
 // %include <gst/gststreams.h>
%include <gst/gststructure.h>
%include <gst/gstsystemclock.h>
%include <gst/gsttaglist.h>
%include <gst/gsttagsetter.h>
%include <gst/gsttask.h>
%include <gst/gsttaskpool.h>
%include <gst/gsttoc.h>
%include <gst/gsttocsetter.h>
 // %include <gst/gsttracer.h>
 // %include <gst/gsttracerfactory.h>
 // %include <gst/gsttracerrecord.h>
%include <gst/gsttypefind.h>
%include <gst/gsttypefindfactory.h>
%include <gst/gsturi.h>
%include <gst/gstutils.h>
%include <gst/gstvalue.h>
%include <gst/gstparse.h>
%include <gst/gl/gl.h>
%include <gst/gl/gstgl_fwd.h>
%include <gst/gl/gstglconfig.h>
%include <gst/gl/gstglapi.h>
%include <gst/gl/gstgldisplay.h>
%include <gst/gl/gstglcontext.h>
%include <gst/gl/gstglfeature.h>
 // %include <gst/gl/gstglformat.h>
%include <gst/gl/gstglutils.h>
%include <gst/gl/gstglwindow.h>
 // %include <gst/gl/gstglslstage.h>
%include <gst/gl/gstglshader.h>
 // %include <gst/gl/gstglshaderstrings.h>
 // %include <gst/gl/gstglcolorconvert.h>
%include <gst/gl/gstglupload.h>
 // %include <gst/gl/gstglbasememory.h>
 // %include <gst/gl/gstglbuffer.h>
%include <gst/gl/gstglmemory.h>
 // %include <gst/gl/gstglmemorypbo.h>
 // %include <gst/gl/gstglrenderbuffer.h>
%include <gst/gl/gstglbufferpool.h>
%include <gst/gl/gstglframebuffer.h>
 // %include <gst/gl/gstglbasefilter.h>
 // %include <gst/gl/gstglviewconvert.h>
%include <gst/gl/gstglfilter.h>
%include <gst/gl/gstglsyncmeta.h>
 // %include <gst/gl/gstgloverlaycompositor.h>
%include <gst/gl/egl/gstgldisplay_egl.h>
%include <gst/gl/x11/gstgldisplay_x11.h>
