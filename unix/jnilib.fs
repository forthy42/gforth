\ jni library wrapper

Vocabulary jni

get-current also jni definitions

c-library jni
    s" ((struct JNI:*(Cell*)(sp[arg0])" ptr-declare $+[]!
    \c #include <jni.h>
    include jni.fs
    
end-c-library

previous set-current