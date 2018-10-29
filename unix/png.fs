\ png library

e? os-type s" linux-android" string-prefix? [IF]
    s" libpng16.so" also c-lib open-path-lib drop previous
[THEN]

Vocabulary pnglib
get-current also pnglib definitions

include unix/png16.fs

previous set-current
