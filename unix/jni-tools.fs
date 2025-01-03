\ Java Native Interface toolkit

require unix/jni.fs
require mini-oof2.fs \ we only need o for now
require set-compsem.fs

get-current also android also jni definitions

JavaVMAttachArgs buffer: vmAA

JNI_VERSION_1_6        vmAA JavaVMAttachArgs-version !
"NativeThread\0" drop  vmAA JavaVMAttachArgs-name !
0                      vmAA JavaVMAttachArgs-group !

host? [IF] app app-vm @ [ELSE] 0 [THEN] value vm
uvalue env
host? [IF] app app-env @ [ELSE] 0 [THEN] to env

16 Constant maxargs#

User callargs

: attach ( -- ) \ jni
    \G attach the current thread to the JVM
    vm [ user' env ]l up@ + vmAA JavaVM-AttachCurrentThread() drop
    maxargs# floats allocate throw callargs ! ;
: detach ( -- ) \ jni
    \G detach the current thread from the JVM
    vm JavaVM-DetachCurrentThread() drop
    callargs @ free throw ;

host? [IF] attach .( attach this thread) cr [THEN] \ attach this thread

\ call java

\ characters used: ZBCSIJFDL

-1 floats 0 +field arg-  drop

: >Z ( c addr -- addr )  arg- tuck c! ;
: >B ( c addr -- addr )  arg- tuck c! ;
: >C ( utf16 addr -- addr )  arg- tuck w! ;
: >S ( n addr -- addr )  arg- tuck w! ;
: >I ( n addr -- addr )  arg- tuck l! ;
: >J ( d addr -- addr )  arg- dup >r xd! r> ;
: >F ( r addr -- addr )  arg- dup sf! ;
: >D ( r addr -- addr )  arg- dup df! ;
: >L ( object addr -- addr )  arg- tuck ! ;
: >[ ( array addr -- addr ) arg- tuck ! ;

Create 'args '[' 1+ 'A'
[DO] ">X" 2dup + 1- [i] swap c! current @ search-wordlist 0= [IF] ' nip [THEN] , [LOOP]

: >args ( x1 .. xn addr u -- )
    dup floats callargs @ + -rot
    -1 under+ bounds swap U-DO
	I c@ 'A' - cells 'args + perform
    1 -LOOP  drop ;

: args, ( addr u -- )
    dup floats ]] callargs @ Literal + [[
    -1 under+ bounds swap U-DO
	I c@ 'A' - cells 'args + @ compile,
    1 -LOOP  postpone drop ;

: callenv ( jobject jmid -- env jobject jmid callargs )
    env -rot callargs @ ;

s" Java Exception" exception Constant !!jni!!

: ?jnithrow ( -- )  env JNIEnv-ExceptionCheck()
    IF
	env JNIEnv-ExceptionDescribe()
	env JNIEnv-ExceptionClear()  !!jni!! throw
    THEN ;

: Z() ( jobject jmid -- c )  callenv JNIEnv-CallBooleanMethodA() ?jnithrow ;
: B() ( jobject jmid -- c )  callenv JNIEnv-CallByteMethodA() ?jnithrow ;
: C() ( jobject jmid -- utf16 )  callenv JNIEnv-CallCharMethodA() ?jnithrow ;
: S() ( jobject jmid -- n )  callenv JNIEnv-CallShortMethodA() ?jnithrow ;
: I() ( jobject jmid -- n )  callenv JNIEnv-CallIntMethodA() ?jnithrow ;
: J() ( jobject jmid -- d )  callenv JNIEnv-CallLongMethodA() ?jnithrow ;
: F() ( jobject jmid -- r )  callenv JNIEnv-CallFloatMethodA() ?jnithrow ;
: D() ( jobject jmid -- r )  callenv JNIEnv-CallDoubleMethodA() ?jnithrow ;
: L() ( jobject jmid -- object )  callenv JNIEnv-CallObjectMethodA() ?jnithrow ;
: V() ( jobject jmid -- )  callenv JNIEnv-CallVoidMethodA() ?jnithrow ;

Create 'calls '[' 1+ 'A'
[DO] "X()" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

: Z()S ( jclass jmid -- c )  callenv JNIEnv-CallStaticBooleanMethodA() ?jnithrow ;
: B()S ( jclass jmid -- c )  callenv JNIEnv-CallStaticByteMethodA() ?jnithrow ;
: C()S ( jclass jmid -- utf16 )  callenv JNIEnv-CallStaticCharMethodA() ?jnithrow ;
: S()S ( jclass jmid -- n )  callenv JNIEnv-CallStaticShortMethodA() ?jnithrow ;
: I()S ( jclass jmid -- n )  callenv JNIEnv-CallStaticIntMethodA() ?jnithrow ;
: J()S ( jclass jmid -- d )  callenv JNIEnv-CallStaticLongMethodA() ?jnithrow ;
: F()S ( jclass jmid -- r )  callenv JNIEnv-CallStaticFloatMethodA() ?jnithrow ;
: D()S ( jclass jmid -- r )  callenv JNIEnv-CallStaticDoubleMethodA() ?jnithrow ;
: L()S ( jclass jmid -- object )  callenv JNIEnv-CallStaticObjectMethodA() ?jnithrow ;
: V()S ( jclass jmid -- )  callenv JNIEnv-CallStaticVoidMethodA() ?jnithrow ;

Create 's-calls '[' 1+ 'A'
[DO] "X()S" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

: new() ( jclass jmid -- )  callenv JNIEnv-NewObjectA() ;

: fieldenv ( jobject jfid -- env jobject jmid )  env -rot ;

: Z@F ( jobject jfid -- c )  fieldenv JNIEnv-GetBooleanField() ;
: B@F ( jobject jfid -- c )  fieldenv JNIEnv-GetByteField() ;
: C@F ( jobject jfid -- utf16 )  fieldenv JNIEnv-GetCharField() ;
: S@F ( jobject jfid -- n )  fieldenv JNIEnv-GetShortField() ;
: I@F ( jobject jfid -- n )  fieldenv JNIEnv-GetIntField() ;
: J@F ( jobject jfid -- d )  fieldenv JNIEnv-GetLongField() ;
: F@F ( jobject jfid -- r )  fieldenv JNIEnv-GetFloatField() ;
: D@F ( jobject jfid -- r )  fieldenv JNIEnv-GetDoubleField() ;
: L@F ( jobject jfid -- object )  fieldenv JNIEnv-GetObjectField() ;
' L@F alias [@F

: Z!F ( c jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetBooleanField() ;
: B!F ( c jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetByteField() ;
: C!F ( utf16 jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetCharField() ;
: S!F ( n jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetShortField() ;
: I!F ( n jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetIntField() ;
: J!F ( d jobject jfid -- )  2swap 2>r fieldenv 2r> JNIEnv-SetLongField() ;
: F!F ( r jobject jfid -- )  fieldenv JNIEnv-SetFloatField() ;
: D!F ( r jobject jfid -- )  fieldenv JNIEnv-SetDoubleField() ;
: L!F ( object jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetObjectField() ;
' L!F alias [!F

Create 'field@ '[' 1+ 'A'
[DO] "X@F" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

Create 'field! '[' 1+ 'A'
[DO] "X!F" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

: Z@' ( jclass jfid -- c )  fieldenv JNIEnv-GetStaticBooleanField() ;
: B@' ( jclass jfid -- c )  fieldenv JNIEnv-GetStaticByteField() ;
: C@' ( jclass jfid -- utf16 )  fieldenv JNIEnv-GetStaticCharField() ;
: S@' ( jclass jfid -- n )  fieldenv JNIEnv-GetStaticShortField() ;
: I@' ( jclass jfid -- n )  fieldenv JNIEnv-GetStaticIntField() ;
: J@' ( jclass jfid -- d )  fieldenv JNIEnv-GetStaticLongField() ;
: F@' ( jclass jfid -- r )  fieldenv JNIEnv-GetStaticFloatField() ;
: D@' ( jclass jfid -- r )  fieldenv JNIEnv-GetStaticDoubleField() ;
: L@' ( jclass jfid -- object )  fieldenv JNIEnv-GetStaticObjectField() ;

Create 'sfield@ '[' 1+ 'A'
[DO] "X@'" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

\ global ref handling - you should ]gref every global ref after usage

: ]ref ( object -- )  env swap JNIEnv-DeleteLocalRef() ;
: ]gref ( object -- )  env swap JNIEnv-DeleteGlobalRef() ;
: ]wgref ( object -- )  env swap JNIEnv-DeleteWeakGlobalRef() ;

Create ]ref-table ' drop , ' ]ref , ' ]gref , ' ]wgref ,

: ]xref ( object -- )
    \G do away with any ref, regardless of ref type
    env over JNIEnv-GetObjectRefType()
    dup 4 < and cells ]ref-table + perform ;

: ref> ( object -- ) o ]ref r> o> >r ;
opt: drop ]] o ]ref o> [[ ;
: gref> ( object -- ) o ]gref r> o> >r ;
opt: drop ]] o ]gref o> [[ ;
: wgref> ( object -- ) o ]wgref r> o> >r ;
opt: drop ]] o ]wgref o> [[ ;
: xref> ( object -- ) o ]xref r> o> >r ;
opt: drop ]] o ]xref o> [[ ;

: xref! ( xref addr -- )  dup @ ?dup-IF  ]xref  THEN ! ;
to-table: xref!-table xref!
' >body xref!-table to-class: j-to

: JValue ( "name" -- ) 0 Value ['] j-to set-to ;

Variable cstring1

\ round robin store for four active jstrings

JValue jstring0
JValue jstring1
JValue jstring2
JValue jstring3
Variable jstring#
: to-jstring ( value -- )
    1 jstring# +!  jstring# 3 and case
	0 of  to jstring0  endof
	1 of  to jstring1  endof
	2 of  to jstring2  endof
	3 of  to jstring3  endof
    endcase ;

: cstr" ( -- addr u )  parse-name ;
: cstr1" ( -- addr u ) parse-name 2dup cstring1 $! ;
: make-jstring ( addr u -- jstring-addr )
    fieldenv JNIEnv-NewStringUTF() dup to-jstring ;
: js" ( -- addr )  '"' parse make-jstring ;
compsem: '"' parse ]] SLiteral make-jstring [[ ;

Variable iscopy
2Variable to-release
: jfree ( -- )
    to-release 2@ 2dup d0= IF  2drop  EXIT  THEN  #0. to-release 2!
    over >r fieldenv JNIEnv-ReleaseStringUTFChars() r> ]xref ;
: jstring>sstring ( string -- addr u )  jfree
    dup >r iscopy fieldenv JNIEnv-GetStringUTFChars()
    r> over to-release 2! cstring>sstring ;
: .jstring ( string -- ) jstring>sstring type jfree ;

0 Value jniclass
0 Value gjniclass \ global reference, only created when needed

"Java identifier not found" exception Constant !!javanf!!

: ?javanf ( id -- id )  dup 0= !!javanf!! and throw ;

host? [IF]
: jni-class: ( "name" -- )
    env cstr" JNIEnv-FindClass() ?javanf to jniclass  0 to gjniclass ;
: jniclass@ ( -- class )
    gjniclass 0= IF
	env jniclass JNIEnv-NewGlobalRef() to gjniclass  THEN
    gjniclass ;
: jni-mid ( "name" "signature" -- methodid )
    env jniclass cstr" cstr1" JNIEnv-GetMethodID() ?javanf ;
: jni-smid ( "name" "signature" -- methodid )
    env jniclass@ cstr" cstr1" JNIEnv-GetStaticMethodID() ?javanf ;
: jni-new ( "signatur" -- methodid )
    env jniclass s" <init>" cstr1" JNIEnv-GetMethodID() ?javanf ;
: jni-fid ( "name" "signature" -- methodid )
    env jniclass cstr" cstr1" JNIEnv-GetFieldID() ?javanf ;
: jni-sfid ( "name" "signature" -- methodid )
    env jniclass@ cstr" cstr1" JNIEnv-GetStaticFieldID() ?javanf ;
[ELSE]
    : jni-class: ( "name" -- )
	parse-name 2drop 0 to jniclass  0 to gjniclass ;
    : jniclass@ ( -- class ) 0 ;
    : jni-mid ( "name" "signature" -- methodid )
	parse-name 2drop parse-name cstring1 $! 0 ;
    synonym jni-smid jni-mid
    synonym jni-fid jni-mid
    synonym jni-sfid jni-mid
    : jni-new ( "signatur" -- methodid )
	parse-name cstring1 $! 0 ;
[THEN]

Variable argstring
: >argstring ( addr1 u1 -- addr2 u2 )
    s" " argstring $!  '(' skip
    BEGIN
	over c@ ')' <> WHILE
	    over c@ dup 'A' '[' 1+ within IF  argstring c$+!  ELSE  drop  THEN
	    '[' skip
	    over c@ 'L' = IF  ';' scan  THEN  1 /string
    REPEAT  2drop argstring $@ ;

: >retchar ( addr1 u1 -- char )
    ')' scan 1 /string drop c@ ;

: jni-method: ( "forth-name" "name" "signature" -- )
    : ( o:jobject args -- retval ) jni-mid >r
    cstring1 $@ >argstring args, postpone o r> lit,
    cstring1 $@ >retchar 'A' - cells 'calls + @ compile, postpone ; ;

: jni-static: ( "forth-name" "name" "signature" -- )
    : ( args -- retval ) jni-smid >r
    cstring1 $@ >argstring args, jniclass@ lit, r> lit,
    cstring1 $@ >retchar 'A' - cells 's-calls + @ compile, postpone ; ;

: jni-new: ( "forth-name" "signature" -- )
    : ( args -- jobject ) jni-new >r
    cstring1 $@ >argstring args,
    jniclass@ lit, r> lit,
    postpone new() postpone ; ;

: cstring@1 ( -- index ) cstring1 $@ drop c@ 'A' - cells ;

to-table: jni-table [noop]
: jni-field: ( "forth-name" "name" "signature" -- )
    >in @ parse-name 2drop jni-fid >in @ { old-in fid new-in }
    :noname postpone drop postpone o fid lit,
    cstring@1 'field! + @ compile, postpone ; jni-table
    noname to-class: latestxt >r
    old-in >in !
    : ( o:jobject -- retval ) postpone o fid lit,
    cstring@1 'field@ + @ compile, postpone ;
    r> set-to  new-in >in ! ;

: jni-sfield: ( "forth-name" "name" "signature" -- )
    : ( o:jobject -- retval )
    jniclass@ lit, jni-sfid lit,
    cstring@1 'sfield@ + @ compile, postpone ; ;

\ array access: you can access one array at a time

Variable jnibuffer

: [len ( array -- n )  env swap JNIEnv-GetArrayLength() ;

: >buffer ( size -- buffer )  jnibuffer $!len jnibuffer $@ drop ;
: buffer@ ( -- addr u )  jnibuffer $@ ;

: [z@ ( array -- addr n )  >r env r@ 0 r@ [len dup >buffer
    JNIEnv-GetBooleanArrayRegion() buffer@ r> ]xref ;
: [b@ ( array -- addr n )  >r env r@ 0 r@ [len dup >buffer
    JNIEnv-GetByteArrayRegion() buffer@ r> ]xref ;
: [c@ ( array -- addr n )  >r env r@ 0 r@ [len dup 2* >buffer
    JNIEnv-GetCharArrayRegion() buffer@ r> ]xref ;
: [s@ ( array -- addr n )  >r env r@ 0 r@ [len dup 2* >buffer
    JNIEnv-GetShortArrayRegion() buffer@ r> ]xref ;
: [i@ ( array -- addr n )  >r env r@ 0 r@ [len dup sfloats >buffer
    JNIEnv-GetIntArrayRegion() buffer@ r> ]xref ;
: [j@ ( array -- addr n )  >r env r@ 0 r@ [len dup dfloats >buffer
    JNIEnv-GetLongArrayRegion() buffer@ r> ]xref ;
: [f@ ( array -- addr n )  >r env r@ 0 r@ [len dup sfloats >buffer
    JNIEnv-GetFloatArrayRegion() buffer@ r> ]xref ;
: [d@ ( array -- addr n )  >r env r@ 0 r@ [len dup dfloats >buffer
    JNIEnv-GetDoubleArrayRegion() buffer@ r> ]xref ;

previous previous set-current
