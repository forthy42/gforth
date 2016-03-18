\ Java Native Interface toolkit

require unix/jnilib.fs
require mini-oof2.fs \ we only need o for now

get-current also android also jni definitions

JavaVMAttachArgs buffer: vmAA

JNI_VERSION_1_6        vmAA JavaVMAttachArgs-version !
"NativeThread\0" drop  vmAA JavaVMAttachArgs-name !
0                      vmAA JavaVMAttachArgs-group !

app app-vm @ value vm
uvalue env
app app-env @ to env

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

attach \ attach this thread

\ call java

\ characters used: ZBCSIJFDL

-1 floats 0 +field arg-  drop

: >z ( c addr -- addr )  arg- tuck c! ;
: >b ( c addr -- addr )  arg- tuck c! ;
: >c ( utf16 addr -- addr )  arg- tuck w! ;
: >s ( n addr -- addr )  arg- tuck w! ;
: >i ( n addr -- addr )  arg- tuck l! ;
: >j ( d addr -- addr )  arg- >r r@ xd! r> ;
: >f ( r addr -- addr )  arg- dup sf! ;
: >d ( r addr -- addr )  arg- dup df! ;
: >l ( object addr -- addr )  arg- tuck ! ;
: >[ ( array addr -- addr ) arg- tuck ! ;

Create 'args '[' 1+ 'A'
[DO] ">x" 2dup + 1- [i] swap c! current @ search-wordlist 0= [IF] ' nip [THEN] , [LOOP]

: >args ( x1 .. xn addr u -- ) dup floats callargs @ + -rot
    swap 1- swap bounds swap U-DO
	I c@ 'A' - cells 'args + perform
    1 -LOOP  drop ;

: args, ( addr u -- )  dup floats ]] callargs @ Literal + [[
    swap 1- swap bounds swap U-DO
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

: z() ( jobject jmid -- c )  callenv JNIEnv-CallBooleanMethodA() ?jnithrow ;
: b() ( jobject jmid -- c )  callenv JNIEnv-CallByteMethodA() ?jnithrow ;
: c() ( jobject jmid -- utf16 )  callenv JNIEnv-CallCharMethodA() ?jnithrow ;
: s() ( jobject jmid -- n )  callenv JNIEnv-CallShortMethodA() ?jnithrow ;
: i() ( jobject jmid -- n )  callenv JNIEnv-CallIntMethodA() ?jnithrow ;
: j() ( jobject jmid -- d )  callenv JNIEnv-CallLongMethodA() ?jnithrow ;
: f() ( jobject jmid -- r )  callenv JNIEnv-CallFloatMethodA() ?jnithrow ;
: d() ( jobject jmid -- r )  callenv JNIEnv-CallDoubleMethodA() ?jnithrow ;
: l() ( jobject jmid -- object )  callenv JNIEnv-CallObjectMethodA() ?jnithrow ;
: v() ( jobject jmid -- )  callenv JNIEnv-CallVoidMethodA() ?jnithrow ;

Create 'calls '[' 1+ 'A'
[DO] "x()" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

: z()s ( jclass jmid -- c )  callenv JNIEnv-CallStaticBooleanMethodA() ?jnithrow ;
: b()s ( jclass jmid -- c )  callenv JNIEnv-CallStaticByteMethodA() ?jnithrow ;
: c()s ( jclass jmid -- utf16 )  callenv JNIEnv-CallStaticCharMethodA() ?jnithrow ;
: s()s ( jclass jmid -- n )  callenv JNIEnv-CallStaticShortMethodA() ?jnithrow ;
: i()s ( jclass jmid -- n )  callenv JNIEnv-CallStaticIntMethodA() ?jnithrow ;
: j()s ( jclass jmid -- d )  callenv JNIEnv-CallStaticLongMethodA() ?jnithrow ;
: f()s ( jclass jmid -- r )  callenv JNIEnv-CallStaticFloatMethodA() ?jnithrow ;
: d()s ( jclass jmid -- r )  callenv JNIEnv-CallStaticDoubleMethodA() ?jnithrow ;
: l()s ( jclass jmid -- object )  callenv JNIEnv-CallStaticObjectMethodA() ?jnithrow ;
: v()s ( jclass jmid -- )  callenv JNIEnv-CallStaticVoidMethodA() ?jnithrow ;

Create 's-calls '[' 1+ 'A'
[DO] "x()s" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

: new() ( jclass jmid -- )  callenv JNIEnv-NewObjectA() ;

: fieldenv ( jobject jfid -- env jobject jmid )  env -rot ;

: z@f ( jobject jfid -- c )  fieldenv JNIEnv-GetBooleanField() ;
: b@f ( jobject jfid -- c )  fieldenv JNIEnv-GetByteField() ;
: c@f ( jobject jfid -- utf16 )  fieldenv JNIEnv-GetCharField() ;
: s@f ( jobject jfid -- n )  fieldenv JNIEnv-GetShortField() ;
: i@f ( jobject jfid -- n )  fieldenv JNIEnv-GetIntField() ;
: j@f ( jobject jfid -- d )  fieldenv JNIEnv-GetLongField() ;
: f@f ( jobject jfid -- r )  fieldenv JNIEnv-GetFloatField() ;
: d@f ( jobject jfid -- r )  fieldenv JNIEnv-GetDoubleField() ;
: l@f ( jobject jfid -- object )  fieldenv JNIEnv-GetObjectField() ;
' l@f alias [@f

: z!f ( c jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetBooleanField() ;
: b!f ( c jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetByteField() ;
: c!f ( utf16 jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetCharField() ;
: s!f ( n jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetShortField() ;
: i!f ( n jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetIntField() ;
: j!f ( d jobject jfid -- )  2swap 2>r fieldenv 2r> JNIEnv-SetLongField() ;
: f!f ( r jobject jfid -- )  fieldenv JNIEnv-SetFloatField() ;
: d!f ( r jobject jfid -- )  fieldenv JNIEnv-SetDoubleField() ;
: l!f ( object jobject jfid -- )  rot >r fieldenv r> JNIEnv-SetObjectField() ;
' l!f alias [!f

Create 'field@ '[' 1+ 'A'
[DO] "x@f" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

Create 'field! '[' 1+ 'A'
[DO] "x!f" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

: z@' ( jclass jfid -- c )  fieldenv JNIEnv-GetStaticBooleanField() ;
: b@' ( jclass jfid -- c )  fieldenv JNIEnv-GetStaticByteField() ;
: c@' ( jclass jfid -- utf16 )  fieldenv JNIEnv-GetStaticCharField() ;
: s@' ( jclass jfid -- n )  fieldenv JNIEnv-GetStaticShortField() ;
: i@' ( jclass jfid -- n )  fieldenv JNIEnv-GetStaticIntField() ;
: j@' ( jclass jfid -- d )  fieldenv JNIEnv-GetStaticLongField() ;
: f@' ( jclass jfid -- r )  fieldenv JNIEnv-GetStaticFloatField() ;
: d@' ( jclass jfid -- r )  fieldenv JNIEnv-GetStaticDoubleField() ;
: l@' ( jclass jfid -- object )  fieldenv JNIEnv-GetStaticObjectField() ;

Create 'sfield@ '[' 1+ 'A'
[DO] "x@'" over [i] swap c! current @ search-wordlist 0= [IF] ' 2drop [THEN] , [LOOP]

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
comp: drop ]] o ]ref o> [[ ;
: gref> ( object -- ) o ]gref r> o> >r ;
comp: drop ]] o ]gref o> [[ ;
: wgref> ( object -- ) o ]wgref r> o> >r ;
comp: drop ]] o ]wgref o> [[ ;
: xref> ( object -- ) o ]xref r> o> >r ;
comp: drop ]] o ]xref o> [[ ;

: xref! ( xref addr -- )  dup @ ?dup-IF  ]xref  THEN ! ;
: jvalue! ( xref xt -- )  >body xref! ;
comp: drop >body postpone ALiteral postpone xref! ;

: JValue ( "name" -- ) 0 Value ['] jvalue! set-to ;

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

: cstr" ( -- addr )  parse-name ;
: cstr1" ( -- addr ) parse-name 2dup cstring1 $! ;
: make-jstring ( addr u -- jstring-addr )
    env -rot JNIEnv-NewStringUTF() dup to-jstring ;
: js" ( -- addr )  '"' parse make-jstring ;
comp: drop '"' parse ]] SLiteral make-jstring [[ ;

Variable iscopy
2Variable to-release
: jfree ( -- )
    to-release 2@ 2dup d0= IF  2drop  EXIT  THEN  0. to-release 2!
    over >r fieldenv JNIEnv-ReleaseStringUTFChars() r> ]xref ;
: jstring>sstring ( string -- addr u )  jfree
    dup >r iscopy fieldenv JNIEnv-GetStringUTFChars()
    r> over to-release 2! cstring>sstring ;
: .jstring ( string -- ) jstring>sstring type jfree ;

0 Value jniclass
0 Value gjniclass \ global reference, only created when needed

"Java identifier not found" exception Constant !!javanf!!

: ?javanf ( id -- id )  dup 0= !!javanf!! and throw ;

: jni-class: ( "name" -- )
    env cstr" JNIEnv-FindClass() ?javanf to jniclass  0 to gjniclass ;
: jniclass, ( -- class )
    gjniclass 0= IF
	env jniclass JNIEnv-NewGlobalRef() to gjniclass  THEN
    gjniclass postpone Literal ;
: jni-mid ( "name" "signature" -- methodid )
    env jniclass cstr" cstr1" JNIEnv-GetMethodID() ?javanf ;
: jni-smid ( "name" "signature" -- methodid )
    env jniclass cstr" cstr1" JNIEnv-GetStaticMethodID() ?javanf ;
: jni-new ( "signatur" -- methodid )
    env jniclass s" <init>" cstr1" JNIEnv-GetMethodID() ?javanf ;
: jni-fid ( "name" "signature" -- methodid )
    env jniclass cstr" cstr1" JNIEnv-GetFieldID() ?javanf ;
: jni-sfid ( "name" "signature" -- methodid )
    env jniclass cstr" cstr1" JNIEnv-GetStaticFieldID() ?javanf ;

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
    cstring1 $@ >argstring args, postpone o r> postpone literal
    cstring1 $@ >retchar 'A' - cells 'calls + @ compile, postpone ; ;

: jni-static: ( "forth-name" "name" "signature" -- )
    : ( args -- retval ) jni-smid >r
    cstring1 $@ >argstring args, jniclass, r> postpone literal
    cstring1 $@ >retchar 'A' - cells 's-calls + @ compile, postpone ; ;

: jni-new: ( "forth-name" "signature" -- )
    : ( args -- jobject ) jni-new >r
    cstring1 $@ >argstring args,
    jniclass, r> postpone literal
    postpone new() postpone ; ;

: cstring@1 ( -- index ) cstring1 $@ drop c@ 'A' - cells ;

: field-to, ( xt1 xt2 -- ) >r postpone literal r> :, ;

: jni-field: ( "forth-name" "name" "signature" -- )
    >in @ parse-name 2drop jni-fid >in @ { old-in fid new-in }
    :noname postpone drop postpone o fid postpone Literal
    cstring@1 'field! + @ compile, postpone ; >r ['] field-to, set-compiler
    old-in >in !
    : ( o:jobject -- retval ) postpone o fid postpone Literal
    cstring@1 'field@ + @ compile, postpone ;
    r> set-to  new-in >in ! ;

: jni-sfield: ( "forth-name" "name" "signature" -- )
    : ( o:jobject -- retval )
    jniclass, jni-sfid postpone Literal
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