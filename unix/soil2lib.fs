\ soil wrapper

Vocabulary soil

get-current also soil definitions

c-library soil2lib
    s" soil2" add-lib
    s" os-type" environment? [IF]
        2dup s" darwin" str= [IF]
	    s" GL" add-lib
	    s" m" add-lib
        [THEN]
        2dup s" linux-android" string-prefix? [IF]
	    s" EGL" add-lib
	    s" GLESv2" add-lib
	    s" m" add-lib
        [THEN]
	2drop
    [THEN]
    \c #include "SOIL2.h"

    include soil2.fs
end-c-library

previous set-current
