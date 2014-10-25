\ openvg wrapper

Vocabulary openmax

get-current also openmax definitions

c-library openmax
    s" OpenMAXAL" add-lib
    \c #include <OMXAL/OpenMAXAL_Platform.h>
    \c #include <OMXAL/OpenMAXAL.h>
    \c #include <OMXAL/OpenMAXAL_Android.h>

    include omxal.fs
end-c-library

previous set-current
