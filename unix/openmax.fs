\ openvg wrapper

Vocabulary openmax

get-current also openmax definitions

c-library openmax
    s" ((struct XA:*(Cell*)(x.spx[arg0])" ptr-declare $+[]!

    include unix/omxal.fs
    
0	constant XA_BOOLEAN_FALSE
1	constant XA_BOOLEAN_TRUE
32767	constant XA_MILLIBEL_MAX
-32768	constant XA_MILLIBEL_MIN
-1	constant XA_MILLIHERTZ_MAX
2147483647	constant XA_MILLIMETER_MAX
0	constant XA_RESULT_SUCCESS
1	constant XA_RESULT_PRECONDITIONS_VIOLATED
2	constant XA_RESULT_PARAMETER_INVALID
3	constant XA_RESULT_MEMORY_FAILURE
4	constant XA_RESULT_RESOURCE_ERROR
5	constant XA_RESULT_RESOURCE_LOST
6	constant XA_RESULT_IO_ERROR
7	constant XA_RESULT_BUFFER_INSUFFICIENT
8	constant XA_RESULT_CONTENT_CORRUPTED
9	constant XA_RESULT_CONTENT_UNSUPPORTED
10	constant XA_RESULT_CONTENT_NOT_FOUND
11	constant XA_RESULT_PERMISSION_DENIED
12	constant XA_RESULT_FEATURE_UNSUPPORTED
13	constant XA_RESULT_INTERNAL_ERROR
14	constant XA_RESULT_UNKNOWN_ERROR
15	constant XA_RESULT_OPERATION_ABORTED
16	constant XA_RESULT_CONTROL_LOST
-2147483648	constant XA_PRIORITY_LOWEST
-1610612736	constant XA_PRIORITY_VERYLOW
-1073741824	constant XA_PRIORITY_LOW
-536870912	constant XA_PRIORITY_BELOWNORMAL
0	constant XA_PRIORITY_NORMAL
536870912	constant XA_PRIORITY_ABOVENORMAL
1073741824	constant XA_PRIORITY_HIGH
1610612736	constant XA_PRIORITY_VERYHIGH
2147483647	constant XA_PRIORITY_HIGHEST
1	constant XA_OBJECT_EVENT_RUNTIME_ERROR
2	constant XA_OBJECT_EVENT_ASYNC_TERMINATION
3	constant XA_OBJECT_EVENT_RESOURCES_LOST
4	constant XA_OBJECT_EVENT_RESOURCES_AVAILABLE
5	constant XA_OBJECT_EVENT_ITF_CONTROL_TAKEN
6	constant XA_OBJECT_EVENT_ITF_CONTROL_RETURNED
7	constant XA_OBJECT_EVENT_ITF_PARAMETERS_CHANGED
1	constant XA_OBJECT_STATE_UNREALIZED
2	constant XA_OBJECT_STATE_REALIZED
3	constant XA_OBJECT_STATE_SUSPENDED
1	constant XA_DYNAMIC_ITF_EVENT_RUNTIME_ERROR
2	constant XA_DYNAMIC_ITF_EVENT_ASYNC_TERMINATION
3	constant XA_DYNAMIC_ITF_EVENT_RESOURCES_LOST
4	constant XA_DYNAMIC_ITF_EVENT_RESOURCES_LOST_PERMANENTLY
5	constant XA_DYNAMIC_ITF_EVENT_RESOURCES_AVAILABLE
1	constant XA_DATAFORMAT_MIME
2	constant XA_DATAFORMAT_PCM
3	constant XA_DATAFORMAT_RAWIMAGE
1	constant XA_DATALOCATOR_URI
2	constant XA_DATALOCATOR_ADDRESS
3	constant XA_DATALOCATOR_IODEVICE
4	constant XA_DATALOCATOR_OUTPUTMIX
5	constant XA_DATALOCATOR_NATIVEDISPLAY
6	constant XA_DATALOCATOR_RESERVED6
7	constant XA_DATALOCATOR_RESERVED7
1	constant XA_CONTAINERTYPE_UNSPECIFIED
2	constant XA_CONTAINERTYPE_RAW
3	constant XA_CONTAINERTYPE_ASF
4	constant XA_CONTAINERTYPE_AVI
5	constant XA_CONTAINERTYPE_BMP
6	constant XA_CONTAINERTYPE_JPG
7	constant XA_CONTAINERTYPE_JPG2000
8	constant XA_CONTAINERTYPE_M4A
9	constant XA_CONTAINERTYPE_MP3
10	constant XA_CONTAINERTYPE_MP4
11	constant XA_CONTAINERTYPE_MPEG_ES
12	constant XA_CONTAINERTYPE_MPEG_PS
13	constant XA_CONTAINERTYPE_MPEG_TS
14	constant XA_CONTAINERTYPE_QT
15	constant XA_CONTAINERTYPE_WAV
16	constant XA_CONTAINERTYPE_XMF_0
17	constant XA_CONTAINERTYPE_XMF_1
18	constant XA_CONTAINERTYPE_XMF_2
19	constant XA_CONTAINERTYPE_XMF_3
20	constant XA_CONTAINERTYPE_XMF_GENERIC
21	constant XA_CONTAINERTYPE_AMR
22	constant XA_CONTAINERTYPE_AAC
23	constant XA_CONTAINERTYPE_3GPP
24	constant XA_CONTAINERTYPE_3GA
25	constant XA_CONTAINERTYPE_RM
26	constant XA_CONTAINERTYPE_DMF
27	constant XA_CONTAINERTYPE_SMF
28	constant XA_CONTAINERTYPE_MOBILE_DLS
29	constant XA_CONTAINERTYPE_OGG
1	constant XA_BYTEORDER_BIGENDIAN
2	constant XA_BYTEORDER_LITTLEENDIAN
8000000	constant XA_SAMPLINGRATE_8
11025000	constant XA_SAMPLINGRATE_11_025
12000000	constant XA_SAMPLINGRATE_12
16000000	constant XA_SAMPLINGRATE_16
22050000	constant XA_SAMPLINGRATE_22_05
24000000	constant XA_SAMPLINGRATE_24
32000000	constant XA_SAMPLINGRATE_32
44100000	constant XA_SAMPLINGRATE_44_1
48000000	constant XA_SAMPLINGRATE_48
64000000	constant XA_SAMPLINGRATE_64
88200000	constant XA_SAMPLINGRATE_88_2
96000000	constant XA_SAMPLINGRATE_96
192000000	constant XA_SAMPLINGRATE_192
1	constant XA_SPEAKER_FRONT_LEFT
2	constant XA_SPEAKER_FRONT_RIGHT
4	constant XA_SPEAKER_FRONT_CENTER
8	constant XA_SPEAKER_LOW_FREQUENCY
16	constant XA_SPEAKER_BACK_LEFT
32	constant XA_SPEAKER_BACK_RIGHT
64	constant XA_SPEAKER_FRONT_LEFT_OF_CENTER
128	constant XA_SPEAKER_FRONT_RIGHT_OF_CENTER
256	constant XA_SPEAKER_BACK_CENTER
512	constant XA_SPEAKER_SIDE_LEFT
1024	constant XA_SPEAKER_SIDE_RIGHT
2048	constant XA_SPEAKER_TOP_CENTER
4096	constant XA_SPEAKER_TOP_FRONT_LEFT
8192	constant XA_SPEAKER_TOP_FRONT_CENTER
16384	constant XA_SPEAKER_TOP_FRONT_RIGHT
32768	constant XA_SPEAKER_TOP_BACK_LEFT
65536	constant XA_SPEAKER_TOP_BACK_CENTER
131072	constant XA_SPEAKER_TOP_BACK_RIGHT
8	constant XA_PCMSAMPLEFORMAT_FIXED_8
16	constant XA_PCMSAMPLEFORMAT_FIXED_16
20	constant XA_PCMSAMPLEFORMAT_FIXED_20
24	constant XA_PCMSAMPLEFORMAT_FIXED_24
28	constant XA_PCMSAMPLEFORMAT_FIXED_28
32	constant XA_PCMSAMPLEFORMAT_FIXED_32
0	constant XA_COLORFORMAT_UNUSED
1	constant XA_COLORFORMAT_MONOCHROME
2	constant XA_COLORFORMAT_8BITRGB332
3	constant XA_COLORFORMAT_12BITRGB444
4	constant XA_COLORFORMAT_16BITARGB4444
5	constant XA_COLORFORMAT_16BITARGB1555
6	constant XA_COLORFORMAT_16BITRGB565
7	constant XA_COLORFORMAT_16BITBGR565
8	constant XA_COLORFORMAT_18BITRGB666
9	constant XA_COLORFORMAT_18BITARGB1665
10	constant XA_COLORFORMAT_19BITARGB1666
11	constant XA_COLORFORMAT_24BITRGB888
12	constant XA_COLORFORMAT_24BITBGR888
13	constant XA_COLORFORMAT_24BITARGB1887
14	constant XA_COLORFORMAT_25BITARGB1888
15	constant XA_COLORFORMAT_32BITBGRA8888
16	constant XA_COLORFORMAT_32BITARGB8888
17	constant XA_COLORFORMAT_YUV411PLANAR
19	constant XA_COLORFORMAT_YUV420PLANAR
21	constant XA_COLORFORMAT_YUV420SEMIPLANAR
22	constant XA_COLORFORMAT_YUV422PLANAR
24	constant XA_COLORFORMAT_YUV422SEMIPLANAR
25	constant XA_COLORFORMAT_YCBYCR
26	constant XA_COLORFORMAT_YCRYCB
27	constant XA_COLORFORMAT_CBYCRY
28	constant XA_COLORFORMAT_CRYCBY
29	constant XA_COLORFORMAT_YUV444INTERLEAVED
30	constant XA_COLORFORMAT_RAWBAYER8BIT
31	constant XA_COLORFORMAT_RAWBAYER10BIT
32	constant XA_COLORFORMAT_RAWBAYER8BITCOMPRESSED
33	constant XA_COLORFORMAT_L2
34	constant XA_COLORFORMAT_L4
35	constant XA_COLORFORMAT_L8
36	constant XA_COLORFORMAT_L16
37	constant XA_COLORFORMAT_L24
38	constant XA_COLORFORMAT_L32
41	constant XA_COLORFORMAT_18BITBGR666
42	constant XA_COLORFORMAT_24BITARGB6666
43	constant XA_COLORFORMAT_24BITABGR6666
1	constant XA_IODEVICE_AUDIOINPUT
2	constant XA_IODEVICE_LEDARRAY
3	constant XA_IODEVICE_VIBRA
4	constant XA_IODEVICE_CAMERA
5	constant XA_IODEVICE_RADIO
-1	constant XA_DEFAULTDEVICEID_AUDIOINPUT
-2	constant XA_DEFAULTDEVICEID_AUDIOOUTPUT
-3	constant XA_DEFAULTDEVICEID_LED
-4	constant XA_DEFAULTDEVICEID_VIBRA
-5	constant XA_DEFAULTDEVICEID_CAMERA
1	constant XA_ENGINEOPTION_THREADSAFE
2	constant XA_ENGINEOPTION_LOSSOFCONTROL
1	constant XA_OBJECTID_ENGINE
2	constant XA_OBJECTID_LEDDEVICE
3	constant XA_OBJECTID_VIBRADEVICE
4	constant XA_OBJECTID_MEDIAPLAYER
5	constant XA_OBJECTID_MEDIARECORDER
6	constant XA_OBJECTID_RADIODEVICE
7	constant XA_OBJECTID_OUTPUTMIX
8	constant XA_OBJECTID_METADATAEXTRACTOR
9	constant XA_OBJECTID_CAMERADEVICE
1	constant XA_PROFILES_MEDIA_PLAYER
2	constant XA_PROFILES_MEDIA_PLAYER_RECORDER
4	constant XA_PROFILES_PLUS_MIDI
-1	constant XA_TIME_UNKNOWN
1	constant XA_PLAYEVENT_HEADATEND
2	constant XA_PLAYEVENT_HEADATMARKER
4	constant XA_PLAYEVENT_HEADATNEWPOS
8	constant XA_PLAYEVENT_HEADMOVING
16	constant XA_PLAYEVENT_HEADSTALLED
1	constant XA_PLAYSTATE_STOPPED
2	constant XA_PLAYSTATE_PAUSED
3	constant XA_PLAYSTATE_PLAYING
1	constant XA_PREFETCHEVENT_STATUSCHANGE
2	constant XA_PREFETCHEVENT_FILLLEVELCHANGE
1	constant XA_PREFETCHSTATUS_UNDERFLOW
2	constant XA_PREFETCHSTATUS_SUFFICIENTDATA
3	constant XA_PREFETCHSTATUS_OVERFLOW
1	constant XA_SEEKMODE_FAST
2	constant XA_SEEKMODE_ACCURATE
1	constant XA_RATEPROP_STAGGEREDVIDEO
2	constant XA_RATEPROP_SMOOTHVIDEO
256	constant XA_RATEPROP_SILENTAUDIO
512	constant XA_RATEPROP_STAGGEREDAUDIO
1024	constant XA_RATEPROP_NOPITCHCORAUDIO
2048	constant XA_RATEPROP_PITCHCORAUDIO
1	constant XA_IMAGEEFFECT_MONOCHROME
2	constant XA_IMAGEEFFECT_NEGATIVE
3	constant XA_IMAGEEFFECT_SEPIA
4	constant XA_IMAGEEFFECT_EMBOSS
5	constant XA_IMAGEEFFECT_PAINTBRUSH
6	constant XA_IMAGEEFFECT_SOLARIZE
7	constant XA_IMAGEEFFECT_CARTOON
1	constant XA_VIDEOMIRROR_NONE
2	constant XA_VIDEOMIRROR_VERTICAL
3	constant XA_VIDEOMIRROR_HORIZONTAL
4	constant XA_VIDEOMIRROR_BOTH
1	constant XA_VIDEOSCALE_STRETCH
2	constant XA_VIDEOSCALE_FIT
3	constant XA_VIDEOSCALE_CROP
0	constant XA_RENDERINGHINT_NONE
1	constant XA_RENDERINGHINT_ANTIALIASING
1	constant XA_RECORDEVENT_HEADATLIMIT
2	constant XA_RECORDEVENT_HEADATMARKER
4	constant XA_RECORDEVENT_HEADATNEWPOS
8	constant XA_RECORDEVENT_HEADMOVING
16	constant XA_RECORDEVENT_HEADSTALLED
32	constant XA_RECORDEVENT_BUFFER_FULL
1	constant XA_RECORDSTATE_STOPPED
2	constant XA_RECORDSTATE_PAUSED
3	constant XA_RECORDSTATE_RECORDING
-1	constant XA_NODE_PARENT
2147483647	constant XA_ROOT_NODE_ID
1	constant XA_NODETYPE_UNSPECIFIED
2	constant XA_NODETYPE_AUDIO
3	constant XA_NODETYPE_VIDEO
4	constant XA_NODETYPE_IMAGE
0	constant XA_CHARACTERENCODING_UNKNOWN
1	constant XA_CHARACTERENCODING_BINARY
2	constant XA_CHARACTERENCODING_ASCII
3	constant XA_CHARACTERENCODING_BIG5
4	constant XA_CHARACTERENCODING_CODEPAGE1252
5	constant XA_CHARACTERENCODING_GB2312
6	constant XA_CHARACTERENCODING_HZGB2312
7	constant XA_CHARACTERENCODING_GB12345
8	constant XA_CHARACTERENCODING_GB18030
9	constant XA_CHARACTERENCODING_GBK
10	constant XA_CHARACTERENCODING_IMAPUTF7
11	constant XA_CHARACTERENCODING_ISO2022JP
11	constant XA_CHARACTERENCODING_ISO2022JP1
12	constant XA_CHARACTERENCODING_ISO88591
13	constant XA_CHARACTERENCODING_ISO885910
14	constant XA_CHARACTERENCODING_ISO885913
15	constant XA_CHARACTERENCODING_ISO885914
16	constant XA_CHARACTERENCODING_ISO885915
17	constant XA_CHARACTERENCODING_ISO88592
18	constant XA_CHARACTERENCODING_ISO88593
19	constant XA_CHARACTERENCODING_ISO88594
20	constant XA_CHARACTERENCODING_ISO88595
21	constant XA_CHARACTERENCODING_ISO88596
22	constant XA_CHARACTERENCODING_ISO88597
23	constant XA_CHARACTERENCODING_ISO88598
24	constant XA_CHARACTERENCODING_ISO88599
25	constant XA_CHARACTERENCODING_ISOEUCJP
26	constant XA_CHARACTERENCODING_SHIFTJIS
27	constant XA_CHARACTERENCODING_SMS7BIT
28	constant XA_CHARACTERENCODING_UTF7
29	constant XA_CHARACTERENCODING_UTF8
30	constant XA_CHARACTERENCODING_JAVACONFORMANTUTF8
31	constant XA_CHARACTERENCODING_UTF16BE
32	constant XA_CHARACTERENCODING_UTF16LE
1	constant XA_METADATA_FILTER_KEY
2	constant XA_METADATA_FILTER_LANG
4	constant XA_METADATA_FILTER_ENCODING
1	constant XA_METADATATRAVERSALMODE_ALL
2	constant XA_METADATATRAVERSALMODE_NODE
1	constant XA_CAMERA_APERTUREMODE_MANUAL
2	constant XA_CAMERA_APERTUREMODE_AUTO
1	constant XA_CAMERA_AUTOEXPOSURESTATUS_SUCCESS
2	constant XA_CAMERA_AUTOEXPOSURESTATUS_UNDEREXPOSURE
3	constant XA_CAMERA_AUTOEXPOSURESTATUS_OVEREXPOSURE
1	constant XA_CAMERACBEVENT_ROTATION
2	constant XA_CAMERACBEVENT_FLASHREADY
3	constant XA_CAMERACBEVENT_FOCUSSTATUS
4	constant XA_CAMERACBEVENT_EXPOSURESTATUS
5	constant XA_CAMERACBEVENT_WHITEBALANCELOCKED
6	constant XA_CAMERACBEVENT_ZOOMSTATUS
1	constant XA_CAMERACAP_FLASH
2	constant XA_CAMERACAP_AUTOFOCUS
4	constant XA_CAMERACAP_CONTINUOUSAUTOFOCUS
8	constant XA_CAMERACAP_MANUALFOCUS
16	constant XA_CAMERACAP_AUTOEXPOSURE
32	constant XA_CAMERACAP_MANUALEXPOSURE
64	constant XA_CAMERACAP_AUTOISOSENSITIVITY
128	constant XA_CAMERACAP_MANUALISOSENSITIVITY
256	constant XA_CAMERACAP_AUTOAPERTURE
512	constant XA_CAMERACAP_MANUALAPERTURE
1024	constant XA_CAMERACAP_AUTOSHUTTERSPEED
2048	constant XA_CAMERACAP_MANUALSHUTTERSPEED
4096	constant XA_CAMERACAP_AUTOWHITEBALANCE
8192	constant XA_CAMERACAP_MANUALWHITEBALANCE
16384	constant XA_CAMERACAP_OPTICALZOOM
32768	constant XA_CAMERACAP_DIGITALZOOM
65536	constant XA_CAMERACAP_METERING
131072	constant XA_CAMERACAP_BRIGHTNESS
262144	constant XA_CAMERACAP_CONTRAST
524288	constant XA_CAMERACAP_GAMMA
1	constant XA_CAMERA_EXPOSUREMODE_MANUAL
2	constant XA_CAMERA_EXPOSUREMODE_AUTO
4	constant XA_CAMERA_EXPOSUREMODE_NIGHT
8	constant XA_CAMERA_EXPOSUREMODE_BACKLIGHT
16	constant XA_CAMERA_EXPOSUREMODE_SPOTLIGHT
32	constant XA_CAMERA_EXPOSUREMODE_SPORTS
64	constant XA_CAMERA_EXPOSUREMODE_SNOW
128	constant XA_CAMERA_EXPOSUREMODE_BEACH
256	constant XA_CAMERA_EXPOSUREMODE_LARGEAPERTURE
512	constant XA_CAMERA_EXPOSUREMODE_SMALLAPERTURE
1024	constant XA_CAMERA_EXPOSUREMODE_PORTRAIT
2048	constant XA_CAMERA_EXPOSUREMODE_NIGHTPORTRAIT
1	constant XA_CAMERA_FLASHMODE_OFF
2	constant XA_CAMERA_FLASHMODE_ON
4	constant XA_CAMERA_FLASHMODE_AUTO
8	constant XA_CAMERA_FLASHMODE_REDEYEREDUCTION
16	constant XA_CAMERA_FLASHMODE_REDEYEREDUCTION_AUTO
32	constant XA_CAMERA_FLASHMODE_FILLIN
64	constant XA_CAMERA_FLASHMODE_TORCH
1	constant XA_CAMERA_FOCUSMODE_MANUAL
2	constant XA_CAMERA_FOCUSMODE_AUTO
4	constant XA_CAMERA_FOCUSMODE_CENTROID
8	constant XA_CAMERA_FOCUSMODE_CONTINUOUS_AUTO
16	constant XA_CAMERA_FOCUSMODE_CONTINUOUS_CENTROID
1	constant XA_CAMERA_FOCUSMODESTATUS_OFF
2	constant XA_CAMERA_FOCUSMODESTATUS_REQUEST
3	constant XA_CAMERA_FOCUSMODESTATUS_REACHED
4	constant XA_CAMERA_FOCUSMODESTATUS_UNABLETOREACH
5	constant XA_CAMERA_FOCUSMODESTATUS_LOST
1	constant XA_CAMERA_ISOSENSITIVITYMODE_MANUAL
2	constant XA_CAMERA_ISOSENSITIVITYMODE_AUTO
1	constant XA_CAMERA_LOCK_AUTOFOCUS
2	constant XA_CAMERA_LOCK_AUTOEXPOSURE
4	constant XA_CAMERA_LOCK_AUTOWHITEBALANCE
1	constant XA_CAMERA_METERINGMODE_AVERAGE
2	constant XA_CAMERA_METERINGMODE_SPOT
4	constant XA_CAMERA_METERINGMODE_MATRIX
1	constant XA_CAMERA_SHUTTERSPEEDMODE_MANUAL
2	constant XA_CAMERA_SHUTTERSPEEDMODE_AUTO
1	constant XA_CAMERA_WHITEBALANCEMODE_MANUAL
2	constant XA_CAMERA_WHITEBALANCEMODE_AUTO
4	constant XA_CAMERA_WHITEBALANCEMODE_SUNLIGHT
8	constant XA_CAMERA_WHITEBALANCEMODE_CLOUDY
16	constant XA_CAMERA_WHITEBALANCEMODE_SHADE
32	constant XA_CAMERA_WHITEBALANCEMODE_TUNGSTEN
64	constant XA_CAMERA_WHITEBALANCEMODE_FLUORESCENT
128	constant XA_CAMERA_WHITEBALANCEMODE_INCANDESCENT
256	constant XA_CAMERA_WHITEBALANCEMODE_FLASH
512	constant XA_CAMERA_WHITEBALANCEMODE_SUNSET
50	constant XA_CAMERA_ZOOM_SLOW
100	constant XA_CAMERA_ZOOM_NORMAL
200	constant XA_CAMERA_ZOOM_FAST
-1	constant XA_CAMERA_ZOOM_FASTEST
1	constant XA_FOCUSPOINTS_ONE
2	constant XA_FOCUSPOINTS_THREE_3X1
3	constant XA_FOCUSPOINTS_FIVE_CROSS
4	constant XA_FOCUSPOINTS_SEVEN_CROSS
5	constant XA_FOCUSPOINTS_NINE_SQUARE
6	constant XA_FOCUSPOINTS_ELEVEN_CROSS
7	constant XA_FOCUSPOINTS_TWELVE_3X4
8	constant XA_FOCUSPOINTS_TWELVE_4X3
9	constant XA_FOCUSPOINTS_SIXTEEN_SQUARE
10	constant XA_FOCUSPOINTS_CUSTOM
1	constant XA_ORIENTATION_UNKNOWN
2	constant XA_ORIENTATION_OUTWARDS
3	constant XA_ORIENTATION_INWARDS
1	constant XA_DEVCONNECTION_INTEGRATED
256	constant XA_DEVCONNECTION_ATTACHED_WIRED
512	constant XA_DEVCONNECTION_ATTACHED_WIRELESS
1024	constant XA_DEVCONNECTION_NETWORK
1	constant XA_DEVLOCATION_HANDSET
2	constant XA_DEVLOCATION_HEADSET
3	constant XA_DEVLOCATION_CARKIT
4	constant XA_DEVLOCATION_DOCK
5	constant XA_DEVLOCATION_REMOTE
1	constant XA_DEVSCOPE_UNKNOWN
2	constant XA_DEVSCOPE_ENVIRONMENT
3	constant XA_DEVSCOPE_USER
65535	constant XA_EQUALIZER_UNDEFINED
1	constant XA_FREQRANGE_FMEUROAMERICA
2	constant XA_FREQRANGE_FMJAPAN
3	constant XA_FREQRANGE_AMLW
4	constant XA_FREQRANGE_AMMW
5	constant XA_FREQRANGE_AMSW
1	constant XA_RADIO_EVENT_ANTENNA_STATUS_CHANGED
2	constant XA_RADIO_EVENT_FREQUENCY_CHANGED
3	constant XA_RADIO_EVENT_FREQUENCY_RANGE_CHANGED
4	constant XA_RADIO_EVENT_PRESET_CHANGED
5	constant XA_RADIO_EVENT_SEEK_COMPLETED
0	constant XA_STEREOMODE_MONO
1	constant XA_STEREOMODE_STEREO
2	constant XA_STEREOMODE_AUTO
1	constant XA_RDS_EVENT_NEW_PI
2	constant XA_RDS_EVENT_NEW_PTY
4	constant XA_RDS_EVENT_NEW_PS
8	constant XA_RDS_EVENT_NEW_RT
16	constant XA_RDS_EVENT_NEW_RT_PLUS
32	constant XA_RDS_EVENT_NEW_CT
64	constant XA_RDS_EVENT_NEW_TA
128	constant XA_RDS_EVENT_NEW_TP
256	constant XA_RDS_EVENT_NEW_ALARM
0	constant XA_RDSPROGRAMMETYPE_RDSPTY_NONE
1	constant XA_RDSPROGRAMMETYPE_RDSPTY_NEWS
2	constant XA_RDSPROGRAMMETYPE_RDSPTY_CURRENTAFFAIRS
3	constant XA_RDSPROGRAMMETYPE_RDSPTY_INFORMATION
4	constant XA_RDSPROGRAMMETYPE_RDSPTY_SPORT
5	constant XA_RDSPROGRAMMETYPE_RDSPTY_EDUCATION
6	constant XA_RDSPROGRAMMETYPE_RDSPTY_DRAMA
7	constant XA_RDSPROGRAMMETYPE_RDSPTY_CULTURE
8	constant XA_RDSPROGRAMMETYPE_RDSPTY_SCIENCE
9	constant XA_RDSPROGRAMMETYPE_RDSPTY_VARIEDSPEECH
10	constant XA_RDSPROGRAMMETYPE_RDSPTY_POPMUSIC
11	constant XA_RDSPROGRAMMETYPE_RDSPTY_ROCKMUSIC
12	constant XA_RDSPROGRAMMETYPE_RDSPTY_EASYLISTENING
13	constant XA_RDSPROGRAMMETYPE_RDSPTY_LIGHTCLASSICAL
14	constant XA_RDSPROGRAMMETYPE_RDSPTY_SERIOUSCLASSICAL
15	constant XA_RDSPROGRAMMETYPE_RDSPTY_OTHERMUSIC
16	constant XA_RDSPROGRAMMETYPE_RDSPTY_WEATHER
17	constant XA_RDSPROGRAMMETYPE_RDSPTY_FINANCE
18	constant XA_RDSPROGRAMMETYPE_RDSPTY_CHILDRENSPROGRAMMES
19	constant XA_RDSPROGRAMMETYPE_RDSPTY_SOCIALAFFAIRS
20	constant XA_RDSPROGRAMMETYPE_RDSPTY_RELIGION
21	constant XA_RDSPROGRAMMETYPE_RDSPTY_PHONEIN
22	constant XA_RDSPROGRAMMETYPE_RDSPTY_TRAVEL
23	constant XA_RDSPROGRAMMETYPE_RDSPTY_LEISURE
24	constant XA_RDSPROGRAMMETYPE_RDSPTY_JAZZMUSIC
25	constant XA_RDSPROGRAMMETYPE_RDSPTY_COUNTRYMUSIC
26	constant XA_RDSPROGRAMMETYPE_RDSPTY_NATIONALMUSIC
27	constant XA_RDSPROGRAMMETYPE_RDSPTY_OLDIESMUSIC
28	constant XA_RDSPROGRAMMETYPE_RDSPTY_FOLKMUSIC
29	constant XA_RDSPROGRAMMETYPE_RDSPTY_DOCUMENTARY
30	constant XA_RDSPROGRAMMETYPE_RDSPTY_ALARMTEST
31	constant XA_RDSPROGRAMMETYPE_RDSPTY_ALARM
0	constant XA_RDSPROGRAMMETYPE_RBDSPTY_NONE
1	constant XA_RDSPROGRAMMETYPE_RBDSPTY_NEWS
2	constant XA_RDSPROGRAMMETYPE_RBDSPTY_INFORMATION
3	constant XA_RDSPROGRAMMETYPE_RBDSPTY_SPORTS
4	constant XA_RDSPROGRAMMETYPE_RBDSPTY_TALK
5	constant XA_RDSPROGRAMMETYPE_RBDSPTY_ROCK
6	constant XA_RDSPROGRAMMETYPE_RBDSPTY_CLASSICROCK
7	constant XA_RDSPROGRAMMETYPE_RBDSPTY_ADULTHITS
8	constant XA_RDSPROGRAMMETYPE_RBDSPTY_SOFTROCK
9	constant XA_RDSPROGRAMMETYPE_RBDSPTY_TOP40
10	constant XA_RDSPROGRAMMETYPE_RBDSPTY_COUNTRY
11	constant XA_RDSPROGRAMMETYPE_RBDSPTY_OLDIES
12	constant XA_RDSPROGRAMMETYPE_RBDSPTY_SOFT
13	constant XA_RDSPROGRAMMETYPE_RBDSPTY_NOSTALGIA
14	constant XA_RDSPROGRAMMETYPE_RBDSPTY_JAZZ
15	constant XA_RDSPROGRAMMETYPE_RBDSPTY_CLASSICAL
16	constant XA_RDSPROGRAMMETYPE_RBDSPTY_RHYTHMANDBLUES
17	constant XA_RDSPROGRAMMETYPE_RBDSPTY_SOFTRHYTHMANDBLUES
18	constant XA_RDSPROGRAMMETYPE_RBDSPTY_LANGUAGE
19	constant XA_RDSPROGRAMMETYPE_RBDSPTY_RELIGIOUSMUSIC
20	constant XA_RDSPROGRAMMETYPE_RBDSPTY_RELIGIOUSTALK
21	constant XA_RDSPROGRAMMETYPE_RBDSPTY_PERSONALITY
22	constant XA_RDSPROGRAMMETYPE_RBDSPTY_PUBLIC
23	constant XA_RDSPROGRAMMETYPE_RBDSPTY_COLLEGE
24	constant XA_RDSPROGRAMMETYPE_RBDSPTY_UNASSIGNED1
25	constant XA_RDSPROGRAMMETYPE_RBDSPTY_UNASSIGNED2
26	constant XA_RDSPROGRAMMETYPE_RBDSPTY_UNASSIGNED3
27	constant XA_RDSPROGRAMMETYPE_RBDSPTY_UNASSIGNED4
28	constant XA_RDSPROGRAMMETYPE_RBDSPTY_UNASSIGNED5
29	constant XA_RDSPROGRAMMETYPE_RBDSPTY_WEATHER
30	constant XA_RDSPROGRAMMETYPE_RBDSPTY_EMERGENCYTEST
31	constant XA_RDSPROGRAMMETYPE_RBDSPTY_EMERGENCY
1	constant XA_RDSRTPLUS_ITEMTITLE
2	constant XA_RDSRTPLUS_ITEMALBUM
3	constant XA_RDSRTPLUS_ITEMTRACKNUMBER
4	constant XA_RDSRTPLUS_ITEMARTIST
5	constant XA_RDSRTPLUS_ITEMCOMPOSITION
6	constant XA_RDSRTPLUS_ITEMMOVEMENT
7	constant XA_RDSRTPLUS_ITEMCONDUCTOR
8	constant XA_RDSRTPLUS_ITEMCOMPOSER
9	constant XA_RDSRTPLUS_ITEMBAND
10	constant XA_RDSRTPLUS_ITEMCOMMENT
11	constant XA_RDSRTPLUS_ITEMGENRE
12	constant XA_RDSRTPLUS_INFONEWS
13	constant XA_RDSRTPLUS_INFONEWSLOCAL
14	constant XA_RDSRTPLUS_INFOSTOCKMARKET
15	constant XA_RDSRTPLUS_INFOSPORT
16	constant XA_RDSRTPLUS_INFOLOTTERY
17	constant XA_RDSRTPLUS_INFOHOROSCOPE
18	constant XA_RDSRTPLUS_INFODAILYDIVERSION
19	constant XA_RDSRTPLUS_INFOHEALTH
20	constant XA_RDSRTPLUS_INFOEVENT
21	constant XA_RDSRTPLUS_INFOSZENE
22	constant XA_RDSRTPLUS_INFOCINEMA
23	constant XA_RDSRTPLUS_INFOTV
24	constant XA_RDSRTPLUS_INFODATETIME
25	constant XA_RDSRTPLUS_INFOWEATHER
26	constant XA_RDSRTPLUS_INFOTRAFFIC
27	constant XA_RDSRTPLUS_INFOALARM
28	constant XA_RDSRTPLUS_INFOADVISERTISEMENT
29	constant XA_RDSRTPLUS_INFOURL
30	constant XA_RDSRTPLUS_INFOOTHER
31	constant XA_RDSRTPLUS_STATIONNAMESHORT
32	constant XA_RDSRTPLUS_STATIONNAMELONG
33	constant XA_RDSRTPLUS_PROGRAMNOW
34	constant XA_RDSRTPLUS_PROGRAMNEXT
35	constant XA_RDSRTPLUS_PROGRAMPART
36	constant XA_RDSRTPLUS_PROGRAMHOST
37	constant XA_RDSRTPLUS_PROFRAMEDITORIALSTAFF
38	constant XA_RDSRTPLUS_PROGRAMFREQUENCY
39	constant XA_RDSRTPLUS_PROGRAMHOMEPAGE
40	constant XA_RDSRTPLUS_PROGRAMSUBCHANNEL
41	constant XA_RDSRTPLUS_PHONEHOTLINE
42	constant XA_RDSRTPLUS_PHONESTUDIO
43	constant XA_RDSRTPLUS_PHONEOTHER
44	constant XA_RDSRTPLUS_SMSSTUDIO
45	constant XA_RDSRTPLUS_SMSOTHER
46	constant XA_RDSRTPLUS_EMAILHOTLINE
47	constant XA_RDSRTPLUS_EMAILSTUDIO
48	constant XA_RDSRTPLUS_EMAILOTHER
49	constant XA_RDSRTPLUS_MMSOTHER
50	constant XA_RDSRTPLUS_CHAT
51	constant XA_RDSRTPLUS_CHATCENTER
52	constant XA_RDSRTPLUS_VOTEQUESTION
53	constant XA_RDSRTPLUS_VOTECENTER
54	constant XA_RDSRTPLUS_OPENCLASS45
55	constant XA_RDSRTPLUS_OPENCLASS55
56	constant XA_RDSRTPLUS_OPENCLASS56
57	constant XA_RDSRTPLUS_OPENCLASS57
58	constant XA_RDSRTPLUS_OPENCLASS58
59	constant XA_RDSRTPLUS_PLACE
60	constant XA_RDSRTPLUS_APPOINTMENT
61	constant XA_RDSRTPLUS_IDENTIFIER
62	constant XA_RDSRTPLUS_PURCHASE
63	constant XA_RDSRTPLUS_GETDATA
1	constant XA_RATECONTROLMODE_CONSTANTBITRATE
2	constant XA_RATECONTROLMODE_VARIABLEBITRATE
1	constant XA_AUDIOCODEC_PCM
2	constant XA_AUDIOCODEC_MP3
3	constant XA_AUDIOCODEC_AMR
4	constant XA_AUDIOCODEC_AMRWB
5	constant XA_AUDIOCODEC_AMRWBPLUS
6	constant XA_AUDIOCODEC_AAC
7	constant XA_AUDIOCODEC_WMA
8	constant XA_AUDIOCODEC_REAL
9	constant XA_AUDIOCODEC_VORBIS
1	constant XA_AUDIOPROFILE_PCM
1	constant XA_AUDIOPROFILE_MPEG1_L3
2	constant XA_AUDIOPROFILE_MPEG2_L3
3	constant XA_AUDIOPROFILE_MPEG25_L3
1	constant XA_AUDIOCHANMODE_MP3_MONO
2	constant XA_AUDIOCHANMODE_MP3_STEREO
3	constant XA_AUDIOCHANMODE_MP3_JOINTSTEREO
4	constant XA_AUDIOCHANMODE_MP3_DUAL
1	constant XA_AUDIOPROFILE_AMR
1	constant XA_AUDIOSTREAMFORMAT_CONFORMANCE
2	constant XA_AUDIOSTREAMFORMAT_IF1
3	constant XA_AUDIOSTREAMFORMAT_IF2
4	constant XA_AUDIOSTREAMFORMAT_FSF
5	constant XA_AUDIOSTREAMFORMAT_RTPPAYLOAD
6	constant XA_AUDIOSTREAMFORMAT_ITU
1	constant XA_AUDIOPROFILE_AMRWB
1	constant XA_AUDIOPROFILE_AMRWBPLUS
1	constant XA_AUDIOPROFILE_AAC_AAC
1	constant XA_AUDIOMODE_AAC_MAIN
2	constant XA_AUDIOMODE_AAC_LC
3	constant XA_AUDIOMODE_AAC_SSR
4	constant XA_AUDIOMODE_AAC_LTP
5	constant XA_AUDIOMODE_AAC_HE
6	constant XA_AUDIOMODE_AAC_SCALABLE
7	constant XA_AUDIOMODE_AAC_ERLC
8	constant XA_AUDIOMODE_AAC_LD
9	constant XA_AUDIOMODE_AAC_HE_PS
10	constant XA_AUDIOMODE_AAC_HE_MPS
1	constant XA_AUDIOSTREAMFORMAT_MP2ADTS
2	constant XA_AUDIOSTREAMFORMAT_MP4ADTS
3	constant XA_AUDIOSTREAMFORMAT_MP4LOAS
4	constant XA_AUDIOSTREAMFORMAT_MP4LATM
5	constant XA_AUDIOSTREAMFORMAT_ADIF
6	constant XA_AUDIOSTREAMFORMAT_MP4FF
7	constant XA_AUDIOSTREAMFORMAT_RAW
1	constant XA_AUDIOPROFILE_WMA7
2	constant XA_AUDIOPROFILE_WMA8
3	constant XA_AUDIOPROFILE_WMA9
4	constant XA_AUDIOPROFILE_WMA10
1	constant XA_AUDIOMODE_WMA_LEVEL1
2	constant XA_AUDIOMODE_WMA_LEVEL2
3	constant XA_AUDIOMODE_WMA_LEVEL3
4	constant XA_AUDIOMODE_WMA_LEVEL4
5	constant XA_AUDIOMODE_WMAPRO_LEVELM0
6	constant XA_AUDIOMODE_WMAPRO_LEVELM1
7	constant XA_AUDIOMODE_WMAPRO_LEVELM2
8	constant XA_AUDIOMODE_WMAPRO_LEVELM3
1	constant XA_AUDIOPROFILE_REALAUDIO
1	constant XA_AUDIOMODE_REALAUDIO_G2
2	constant XA_AUDIOMODE_REALAUDIO_8
3	constant XA_AUDIOMODE_REALAUDIO_10
4	constant XA_AUDIOMODE_REALAUDIO_SURROUND
1	constant XA_AUDIOPROFILE_VORBIS
1	constant XA_AUDIOMODE_VORBIS
1	constant XA_IMAGECODEC_JPEG
2	constant XA_IMAGECODEC_GIF
3	constant XA_IMAGECODEC_BMP
4	constant XA_IMAGECODEC_PNG
5	constant XA_IMAGECODEC_TIFF
6	constant XA_IMAGECODEC_RAW
1	constant XA_VIDEOCODEC_MPEG2
2	constant XA_VIDEOCODEC_H263
3	constant XA_VIDEOCODEC_MPEG4
4	constant XA_VIDEOCODEC_AVC
5	constant XA_VIDEOCODEC_VC1
1	constant XA_VIDEOPROFILE_MPEG2_SIMPLE
2	constant XA_VIDEOPROFILE_MPEG2_MAIN
3	constant XA_VIDEOPROFILE_MPEG2_422
4	constant XA_VIDEOPROFILE_MPEG2_SNR
5	constant XA_VIDEOPROFILE_MPEG2_SPATIAL
6	constant XA_VIDEOPROFILE_MPEG2_HIGH
1	constant XA_VIDEOLEVEL_MPEG2_LL
2	constant XA_VIDEOLEVEL_MPEG2_ML
3	constant XA_VIDEOLEVEL_MPEG2_H14
4	constant XA_VIDEOLEVEL_MPEG2_HL
1	constant XA_VIDEOPROFILE_H263_BASELINE
2	constant XA_VIDEOPROFILE_H263_H320CODING
3	constant XA_VIDEOPROFILE_H263_BACKWARDCOMPATIBLE
4	constant XA_VIDEOPROFILE_H263_ISWV2
5	constant XA_VIDEOPROFILE_H263_ISWV3
6	constant XA_VIDEOPROFILE_H263_HIGHCOMPRESSION
7	constant XA_VIDEOPROFILE_H263_INTERNET
8	constant XA_VIDEOPROFILE_H263_INTERLACE
9	constant XA_VIDEOPROFILE_H263_HIGHLATENCY
1	constant XA_VIDEOLEVEL_H263_10
2	constant XA_VIDEOLEVEL_H263_20
3	constant XA_VIDEOLEVEL_H263_30
4	constant XA_VIDEOLEVEL_H263_40
5	constant XA_VIDEOLEVEL_H263_45
6	constant XA_VIDEOLEVEL_H263_50
7	constant XA_VIDEOLEVEL_H263_60
8	constant XA_VIDEOLEVEL_H263_70
1	constant XA_VIDEOPROFILE_MPEG4_SIMPLE
2	constant XA_VIDEOPROFILE_MPEG4_SIMPLESCALABLE
3	constant XA_VIDEOPROFILE_MPEG4_CORE
4	constant XA_VIDEOPROFILE_MPEG4_MAIN
5	constant XA_VIDEOPROFILE_MPEG4_NBIT
6	constant XA_VIDEOPROFILE_MPEG4_SCALABLETEXTURE
7	constant XA_VIDEOPROFILE_MPEG4_SIMPLEFACE
8	constant XA_VIDEOPROFILE_MPEG4_SIMPLEFBA
9	constant XA_VIDEOPROFILE_MPEG4_BASICANIMATED
10	constant XA_VIDEOPROFILE_MPEG4_HYBRID
11	constant XA_VIDEOPROFILE_MPEG4_ADVANCEDREALTIME
12	constant XA_VIDEOPROFILE_MPEG4_CORESCALABLE
13	constant XA_VIDEOPROFILE_MPEG4_ADVANCEDCODING
14	constant XA_VIDEOPROFILE_MPEG4_ADVANCEDCORE
15	constant XA_VIDEOPROFILE_MPEG4_ADVANCEDSCALABLE
1	constant XA_VIDEOLEVEL_MPEG4_0
2	constant XA_VIDEOLEVEL_MPEG4_0b
3	constant XA_VIDEOLEVEL_MPEG4_1
4	constant XA_VIDEOLEVEL_MPEG4_2
5	constant XA_VIDEOLEVEL_MPEG4_3
6	constant XA_VIDEOLEVEL_MPEG4_4
7	constant XA_VIDEOLEVEL_MPEG4_4a
8	constant XA_VIDEOLEVEL_MPEG4_5
1	constant XA_VIDEOPROFILE_AVC_BASELINE
2	constant XA_VIDEOPROFILE_AVC_MAIN
3	constant XA_VIDEOPROFILE_AVC_EXTENDED
4	constant XA_VIDEOPROFILE_AVC_HIGH
5	constant XA_VIDEOPROFILE_AVC_HIGH10
6	constant XA_VIDEOPROFILE_AVC_HIGH422
7	constant XA_VIDEOPROFILE_AVC_HIGH444
1	constant XA_VIDEOLEVEL_AVC_1
2	constant XA_VIDEOLEVEL_AVC_1B
3	constant XA_VIDEOLEVEL_AVC_11
4	constant XA_VIDEOLEVEL_AVC_12
5	constant XA_VIDEOLEVEL_AVC_13
6	constant XA_VIDEOLEVEL_AVC_2
7	constant XA_VIDEOLEVEL_AVC_21
8	constant XA_VIDEOLEVEL_AVC_22
9	constant XA_VIDEOLEVEL_AVC_3
10	constant XA_VIDEOLEVEL_AVC_31
11	constant XA_VIDEOLEVEL_AVC_32
12	constant XA_VIDEOLEVEL_AVC_4
13	constant XA_VIDEOLEVEL_AVC_41
14	constant XA_VIDEOLEVEL_AVC_42
15	constant XA_VIDEOLEVEL_AVC_5
16	constant XA_VIDEOLEVEL_AVC_51
1	constant XA_VIDEOLEVEL_VC1_SIMPLE
2	constant XA_VIDEOLEVEL_VC1_MAIN
3	constant XA_VIDEOLEVEL_VC1_ADVANCED
1	constant XA_VIDEOLEVEL_VC1_LOW
2	constant XA_VIDEOLEVEL_VC1_MEDIUM
3	constant XA_VIDEOLEVEL_VC1_HIGH
4	constant XA_VIDEOLEVEL_VC1_L0
5	constant XA_VIDEOLEVEL_VC1_L1
6	constant XA_VIDEOLEVEL_VC1_L2
7	constant XA_VIDEOLEVEL_VC1_L3
8	constant XA_VIDEOLEVEL_VC1_L4
1	constant XA_STREAMCBEVENT_PROPERTYCHANGE
6	constant XA_ANDROID_VIDEOCODEC_VP8
1	constant XA_ANDROID_VIDEOPROFILE_VP8_MAIN
1	constant XA_ANDROID_VIDEOLEVEL_VP8_VERSION0
2	constant XA_ANDROID_VIDEOLEVEL_VP8_VERSION1
3	constant XA_ANDROID_VIDEOLEVEL_VP8_VERSION2
4	constant XA_ANDROID_VIDEOLEVEL_VP8_VERSION3
0	constant XA_ANDROID_ITEMKEY_NONE
1	constant XA_ANDROID_ITEMKEY_EOS
2	constant XA_ANDROID_ITEMKEY_DISCONTINUITY
3	constant XA_ANDROID_ITEMKEY_BUFFERQUEUEEVENT
4	constant XA_ANDROID_ITEMKEY_FORMAT_CHANGE
0	constant XA_ANDROIDBUFFERQUEUEEVENT_NONE
1	constant XA_ANDROIDBUFFERQUEUEEVENT_PROCESSED
-2147481666	constant XA_DATALOCATOR_ANDROIDBUFFERQUEUE
-2147481668	constant XA_DATALOCATOR_ANDROIDFD
$FFFFFFFF.FFFFFFFF 2constant XA_DATALOCATOR_ANDROIDFD_USE_FILE_SIZE
: XA_ANDROID_MIME_MP2TS ( -- addr ) \ gforth-internal
    "video/mp2ts\0" drop ;

\ ------===< values >===-------
c-value XA_IID_NULL XA_IID_NULL -- a
c-value XA_IID_OBJECT XA_IID_OBJECT -- a
c-value XA_IID_CONFIGEXTENSION XA_IID_CONFIGEXTENSION -- a
c-value XA_IID_DYNAMICINTERFACEMANAGEMENT XA_IID_DYNAMICINTERFACEMANAGEMENT -- a
c-value XA_IID_ENGINE XA_IID_ENGINE -- a
c-value XA_IID_THREADSYNC XA_IID_THREADSYNC -- a
c-value XA_IID_PLAY XA_IID_PLAY -- a
c-value XA_IID_PLAYBACKRATE XA_IID_PLAYBACKRATE -- a
c-value XA_IID_PREFETCHSTATUS XA_IID_PREFETCHSTATUS -- a
c-value XA_IID_SEEK XA_IID_SEEK -- a
c-value XA_IID_VOLUME XA_IID_VOLUME -- a
c-value XA_IID_IMAGECONTROLS XA_IID_IMAGECONTROLS -- a
c-value XA_IID_IMAGEEFFECTS XA_IID_IMAGEEFFECTS -- a
c-value XA_IID_VIDEOPOSTPROCESSING XA_IID_VIDEOPOSTPROCESSING -- a
c-value XA_IID_RECORD XA_IID_RECORD -- a
c-value XA_IID_SNAPSHOT XA_IID_SNAPSHOT -- a
c-value XA_IID_METADATAEXTRACTION XA_IID_METADATAEXTRACTION -- a
c-value XA_IID_METADATAINSERTION XA_IID_METADATAINSERTION -- a
c-value XA_IID_METADATATRAVERSAL XA_IID_METADATATRAVERSAL -- a
c-value XA_IID_DYNAMICSOURCE XA_IID_DYNAMICSOURCE -- a
c-value XA_IID_CAMERACAPABILITIES XA_IID_CAMERACAPABILITIES -- a
c-value XA_IID_CAMERA XA_IID_CAMERA -- a
c-value XA_IID_AUDIOIODEVICECAPABILITIES XA_IID_AUDIOIODEVICECAPABILITIES -- a
c-value XA_IID_DEVICEVOLUME XA_IID_DEVICEVOLUME -- a
c-value XA_IID_EQUALIZER XA_IID_EQUALIZER -- a
c-value XA_IID_OUTPUTMIX XA_IID_OUTPUTMIX -- a
c-value XA_IID_RADIO XA_IID_RADIO -- a
c-value XA_IID_RDS XA_IID_RDS -- a
c-value XA_IID_VIBRA XA_IID_VIBRA -- a
c-value XA_IID_LED XA_IID_LED -- a
c-value XA_IID_AUDIODECODERCAPABILITIES XA_IID_AUDIODECODERCAPABILITIES -- a
c-value XA_IID_AUDIOENCODER XA_IID_AUDIOENCODER -- a
c-value XA_IID_AUDIOENCODERCAPABILITIES XA_IID_AUDIOENCODERCAPABILITIES -- a
c-value XA_IID_IMAGEENCODERCAPABILITIES XA_IID_IMAGEENCODERCAPABILITIES -- a
c-value XA_IID_IMAGEDECODERCAPABILITIES XA_IID_IMAGEDECODERCAPABILITIES -- a
c-value XA_IID_IMAGEENCODER XA_IID_IMAGEENCODER -- a
c-value XA_IID_VIDEODECODERCAPABILITIES XA_IID_VIDEODECODERCAPABILITIES -- a
c-value XA_IID_VIDEOENCODER XA_IID_VIDEOENCODER -- a
c-value XA_IID_VIDEOENCODERCAPABILITIES XA_IID_VIDEOENCODERCAPABILITIES -- a
c-value XA_IID_STREAMINFORMATION XA_IID_STREAMINFORMATION -- a
c-value XA_IID_ANDROIDBUFFERQUEUESOURCE XA_IID_ANDROIDBUFFERQUEUESOURCE -- a

end-c-library

previous set-current
