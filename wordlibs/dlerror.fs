
require ./../wordlib.fs

WordLibrary dlerror.so ./dlerror.so

: .dlerror dlerror dup -1 0 scan drop over - type ;
