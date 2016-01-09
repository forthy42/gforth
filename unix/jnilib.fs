\ jni library wrapper

Vocabulary jni

get-current also jni definitions

c-library jnilib
    s" ((struct JNI:*(Cell*)(sp[arg0])" ptr-declare $+[]!
    \c #include <jni.h>
    include unix/jni.fs
    
end-c-library

previous set-current