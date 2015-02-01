\ video recorder, can be used to capture a video stream
\ or just the preview image

also jni

jni-class: android/media/MediaPlayer

jni-new: new-MediaPlayer ()V
jni-method: mp-prepare prepare ()V
jni-method: mp-start start ()V
jni-method: mp-stop stop ()V
jni-method: mp-pause pause ()V
jni-method: mp-reset reset ()V
jni-method: mp-release release ()V
jni-method: setSurface setSurface (Landroid/view/Surface;)V
jni-method: setVolume setVolume (FF)V

jni-class: android/graphics/SurfaceTexture

jni-new: new-SurfaceTexture (I)V
jni-method: updateTexImage updateTexImage ()V
jni-method: getTimestamp getTimestamp ()J
jni-method: setDefaultBufferSize setDefaultBufferSize (II)V
jni-method: getTransformMatrix getTransformMatrix ([F)V
jni-method: st-release release ()V

jni-class: android/view/Surface

jni-new: new-Surface (Landroid/graphics/SurfaceTexture;)V
jni-method: release release ()V
jni-method: isValid isValid ()Z

jni-class: android/media/MediaRecorder

jni-new: new-MediaRecorder ()V
jni-method: mr-prepare prepare ()V
jni-method: mr-release release ()V
jni-method: mr-reset reset ()V
jni-method: mr-start start ()V
jni-method: mr-stop stop ()V
jni-method: setAudioSource setAudioSource (I)V
jni-method: setAudioSamplingRate setAudioSamplingRate (I)V
jni-method: setAudioEncoder setAudioEncoder (I)V
jni-method: setAudioEncodingBitRate setAudioEncodingBitRate (I)V
jni-method: setAudioChannels setAudioChannels (I)V
jni-method: setCamera setCamera (Landroid/hardware/Camera;)V
jni-method: setOutputFile setOutputFile (Ljava/lang/String;)V
jni-method: setOutputFormat setOutputFormat (I)V
jni-method: setOrientationHint setOrientationHint (I)V
jni-method: setVideoEncoder setVideoEncoder (I)V
jni-method: setVideoEncodingBitRate setVideoEncodingBitRate (I)V
jni-method: setVideoFrameRate setVideoFrameRate (I)V
jni-method: setVideoSize setVideoSize (II)V
jni-method: setVideoSource setVideoSource (I)V
jni-method: mr-setPreviewDisplay setPreviewDisplay (Landroid/view/Surface;)V
jni-method: setMaxDuration setMaxDuration (I)V
jni-method: setMaxFileSize setMaxFileSize (J)V
jni-method: setProfile setProfile (Landroid/media/CamcorderProfile;)V

jni-class: android/hardware/Camera

jni-method: getParameters getParameters ()Landroid/hardware/Camera$Parameters;
jni-method: setParameters setParameters (Landroid/hardware/Camera$Parameters;)V
jni-method: startPreview startPreview ()V
jni-method: stopPreview stopPreview ()V
jni-method: setPreviewTexture setPreviewTexture (Landroid/graphics/SurfaceTexture;)V
jni-method: c-setPreviewDisplay setPreviewDisplay (Landroid/view/SurfaceHolder;)V

jni-static: c-open-back open ()Landroid/hardware/Camera;
jni-static: c-open open (I)Landroid/hardware/Camera;
jni-method: c-lock lock ()V
jni-method: c-unlock unlock ()V
jni-method: c-release release ()V

jni-class: android/hardware/Camera$Parameters

jni-method: setPictureFormat setPictureFormat (I)V
jni-method: setPictureSize setPictureSize (II)V
jni-method: setPreviewFormat setPreviewFormat (I)V
jni-method: setPreviewSize setPreviewSize (II)V
jni-method: setPreviewFpsRange setPreviewFpsRange (II)V
jni-method: setVideoStabilization setVideoStabilization (Z)V
jni-method: setFocusMode setFocusMode (Ljava/lang/String;)V

jni-method: getSupportedPreviewFormats getSupportedPreviewFormats ()Ljava/util/List;
jni-method: getSupportedPreviewFpsRange getSupportedPreviewFpsRange ()Ljava/util/List;
jni-method: getSupportedPreviewSizes getSupportedPreviewSizes ()Ljava/util/List;

jni-method: getSupportedPictureFormats getSupportedPictureFormats ()Ljava/util/List;
jni-method: getSupportedPictureSizes getSupportedPictureSizes ()Ljava/util/List;

jni-method: getSupportedJpegThumbnailSizes getSupportedJpegThumbnailSizes ()Ljava/util/List;
jni-method: getSupportedSceneModes getSupportedSceneModes ()Ljava/util/List;
jni-method: getSupportedVideoSizes getSupportedVideoSizes ()Ljava/util/List;
jni-method: getPreferredPreviewSizeForVideo getPreferredPreviewSizeForVideo ()Landroid/hardware/Camera$Size;

jni-class: android/media/CamcorderProfile

jni-static: cp-get get (I)Landroid/media/CamcorderProfile;
jni-static: cp-get(id) get (II)Landroid/media/CamcorderProfile;

jni-class: android/hardware/Camera$Size

jni-field: height height I
jni-field: width width I

SDK_INT 16 u>= [IF]
jni-class: android/media/MediaCodec

jni-static: createByCodecName createByCodecName (Ljava/lang/String;)Landroid/media/MediaCodec;
jni-static: createDecoderByType createDecoderByType (Ljava/lang/String;)Landroid/media/MediaCodec;
jni-static: createEncoderByType createEncoderByType (Ljava/lang/String;)Landroid/media/MediaCodec;

jni-method: configure configure (Landroid/media/MediaFormat;Landroid/view/Surface;Landroid/media/MediaCrypto;I)V
jni-method: createInputSurface createInputSurface ()Landroid/view/Surface;
jni-method: dequeueInputBuffer dequeueInputBuffer (J)I
jni-method: dequeueOutputBuffer dequeueOutputBuffer (Landroid/media/MediaCodec$BufferInfo;J)I
jni-method: mc-flush flush ()V
jni-method: getCodecInfo getCodecInfo ()Landroid/media/MediaCodecInfo;
jni-method: getInputBuffers getInputBuffers ()[Ljava/nio/ByteBuffer;
jni-method: mc-getName getName ()Ljava/lang/String;
jni-method: getOutputBuffers getOutputBuffers ()[Ljava/nio/ByteBuffer;
jni-method: getOutputFormat getOutputFormat ()Landroid/media/MediaFormat;
jni-method: queueInputBuffer queueInputBuffer (IIIJI)V
jni-method: mc-release release ()V
jni-method: releaseOutputBuffer releaseOutputBuffer (IZ)V
jni-method: setVideoScalingMode setVideoScalingMode (I)V
jni-method: signalEndOfInputStream signalEndOfInputStream ()V
jni-method: mc-start start ()V
jni-method: mc-stop stop ()V

jni-class: android/media/MediaCodec$BufferInfo

jni-new: new-MediaCodec.BufferInfo ()V

jni-field: mcbi-flags flags I
jni-field: mcbi-offset offset I
jni-field: mcbi-presentationTimeUs presentationTimeUs J
jni-field: mcbi-size size I
jni-method: mcbi-set set (IIJI)V

jni-class: android/media/MediaFormat

jni-static: createAudioFormat createAudioFormat (Ljava/lang/String;II)Landroid/media/MediaFormat;
\ jni-static: createSubtitleFormat createSubtitleFormat (Ljava/lang/String;Ljava/lang/String;)Landroid/media/MediaFormat;
\ only API 19
jni-static: createVideoFormat createVideoFormat (Ljava/lang/String;II)Landroid/media/MediaFormat;
[THEN]

JValue media-sft

previous

: create-sft ( -- )
    media-tex current-tex new-SurfaceTexture to media-sft ;
