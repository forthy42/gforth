\ soil wrapper

Vocabulary soil

get-current also soil definitions

c-library soil2lib
    s" soil2" add-lib
    s" os-type" environment? [IF] s" linux-android" str= [IF]
	    s" EGL" add-lib
	    s" GLESv2" add-lib
	    s" m" add-lib
    [THEN] [THEN]
    \c #include "SOIL2.h"

    include soil2.fs
end-c-library

previous set-current
