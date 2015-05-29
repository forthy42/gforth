\ show/hide keyboard using jni tools

require unix/jni-tools.fs

also android also jni

app obj @ Value clazz

: gforth-class: ( -- )
    clazz env swap JNIEnv-getObjectClass() to jniclass ;

jni-class: android/os/Build$VERSION

jni-sfield: SDK_INT SDK_INT I

gforth-class:

\ jni-sfield: INPUT_METHOD_SERVICE INPUT_METHOD_SERVICE Ljava/lang/String;
\ jni-sfield: POWER_SERVICE POWER_SERVICE Ljava/lang/String;
: INPUT_METHOD_SERVICE js" input_method" ;
: POWER_SERVICE        js" power" ;

jni-method: getSystemService getSystemService (Ljava/lang/String;)Ljava/lang/Object;
jni-method: getWindow getWindow ()Landroid/view/Window;
jni-method: hideProgress hideProgress ()V
jni-method: showIME showIME ()V
jni-method: hideIME hideIME ()V
jni-field: clipboardManager clipboardManager Landroid/text/ClipboardManager;

jni-class: android/app/Activity
jni-method: getWindowManager getWindowManager ()Landroid/view/WindowManager;

jni-class: android/view/WindowManager
jni-method: getDefaultDisplay getDefaultDisplay ()Landroid/view/Display;

jni-class: android/view/Display
jni-method: getRotation getRotation ()I

jni-class: android/view/inputmethod/InputMethodManager

jni-method: toggleSoftInput toggleSoftInput (II)V

jni-class: android/view/Window
jni-method: getDecorView getDecorView ()Landroid/view/View;
jni-method: addFlags addFlags (I)V
jni-method: clearFlags clearFlags (I)V
jni-method: getForcedWindowFlags getForcedWindowFlags ()I
jni-method: takeSurface takeSurface (Landroid/view/SurfaceHolder$Callback2;)V

jni-class: android/view/KeyEvent
jni-new: newKeyEvent (II)V
jni-method: getUnicodeChar(I) getUnicodeChar (I)I
jni-method: getUnicodeChar getUnicodeChar ()I
jni-method: getKeyCode getKeyCode ()I
jni-method: getCharacters getCharacters ()Ljava/lang/String;
jni-method: getAction getAction ()I
jni-method: getMetaState getMetaState ()I
jni-method: isLongPress isLongPress ()Z

jni-class: android/view/MotionEvent
jni-method: getPointerCount getPointerCount ()I
jni-method: getX getX (I)F
jni-method: getY getY (I)F
jni-method: me-getAction getAction ()I
jni-method: getFlags getFlags ()I
jni-method: getEdgeFlags getEdgeFlags ()I
jni-method: getEventTime getEventTime ()J
jni-method: getDownTime getDownTime ()J
jni-method: getMetaState getMetaState ()I
jni-method: getSize getSize (I)F
jni-method: getPressure getPressure (I)F

jni-class: java/util/List

jni-method: l-get get (I)Ljava/lang/Object;
jni-method: l-size size ()I

cell 8 = [IF] false [ELSE] SDK_INT 10 u<= [THEN] [IF] \ 2.3.x uses a different clipboard manager
    jni-class: android/text/ClipboardManager

    jni-method: hasText hasText ()Z
    jni-method: getText getText ()Ljava/lang/CharSequence;
[ELSE]
    jni-class: android/content/ClipboardManager
    
    jni-method: getPrimaryClip getPrimaryClip ()Landroid/content/ClipData;
    jni-method: hasPrimaryClip hasPrimaryClip ()Z
    
    jni-class: android/content/ClipData

    jni-method: getItemCount getItemCount ()I
    jni-method: getItemAt getItemAt (I)Landroid/content/ClipData$Item;
    
    jni-class: android/content/ClipData$Item
    
    jni-method: getText getText ()Ljava/lang/CharSequence;
    jni-method: getIntent getIntent ()Landroid/content/Intent;
    jni-method: getUri getUri ()Landroid/net/Uri;
    jni-method: coerceToText coerceToText (Landroid/content/Context;)Ljava/lang/CharSequence;
    cell 8 = [IF] true [ELSE] SDK_INT 16 u>= [THEN] [IF]
	jni-method: getHtmlText getHtmlText ()Ljava/lang/String;
    [THEN]
[THEN]

jni-class: java/lang/CharSequence

jni-method: toString toString ()Ljava/lang/String;

: l[] ( n list -- object )  >o l-get o> ;
: l# ( list -- n )  >o l-size o> ;

: l-map ( xt list -- )  >o { xt } ( -- )
    l-size 0 ?DO  I l-get >o xt execute ref>  LOOP o> ;

Variable kbflag kbflag on

: hidekb ( -- )  clazz >o hideIME o> kbflag off ;
: showkb ( -- )  clazz >o showIME o> kbflag on ;

: togglekb ( -- )
    kbflag @ IF  hidekb  ELSE  showkb  THEN ;

cell 8 = [IF] false [ELSE] SDK_INT 10 u<= [THEN] [IF]
    : getclip? ( -- addr u / 0 0 )
	clazz .clipboardManager >o
	hasText IF
	    getText >o toString jstring>sstring ref>
	ELSE  0 0  THEN  ref> ;
[ELSE]
    : getclip? ( -- addr u / 0 0 )
	clazz .clipboardManager >o
	hasPrimaryClip IF
	    getPrimaryClip >o
	    getItemCount IF
		0 getItemAt >o
		getText dup IF
		    >o toString jstring>sstring ref>
		ELSE  0  THEN
		ref>
	    ELSE  0 0  THEN
	    ref>
	ELSE 0 0 THEN ref> ;
[THEN]
: paste ( -- )
    getclip? dup IF  inskeys  ELSE  2drop  THEN ;

0 [IF]
jni-class: android/os/PowerManager
jni-method: newWakeLock newWakeLock (ILjava/lang/String;)Landroid/os/PowerManager$WakeLock;

jni-class: android/os/PowerManager$WakeLock
jni-method: wl-acquire acquire ()V
jni-method: wl-release release ()V

$20000000 Constant ON_AFTER_RELEASE
$0000000a Constant SCREEN_BRIGHT_WAKE_LOCK
$00000006 Constant SCREEN_DIM_WAKE_LOCK

: get-wakelock ( type -- pm )
    clazz >o POWER_SERVICE getSystemService o>
    >o js" Gforth wakelock" newWakeLock o> ;

0 Value bright-wl

: >bright-wl ( -- ) bright-wl ?EXIT
    ON_AFTER_RELEASE SCREEN_BRIGHT_WAKE_LOCK or
    get-wakelock to bright-wl ;

: screen+bright ( -- )  >bright-wl bright-wl >o wl-acquire o> ;
: screen-bright ( -- )  >bright-wl bright-wl >o wl-release o> ;
[THEN]

previous previous