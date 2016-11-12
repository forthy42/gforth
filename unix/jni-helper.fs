\ show/hide keyboard using jni tools

require unix/jni-tools.fs

also android also jni

app obj @ Value clazz

: gforth-class: ( -- )
    clazz env swap JNIEnv-getObjectClass() to jniclass  0 to gjniclass ;

gforth-class:

\ jni-sfield: INPUT_METHOD_SERVICE INPUT_METHOD_SERVICE Ljava/lang/String;
\ jni-sfield: POWER_SERVICE POWER_SERVICE Ljava/lang/String;
: INPUT_METHOD_SERVICE js" input_method" ;
: POWER_SERVICE        js" power" ;
: NOTIFICATION_SERVICE js" notification" ;

jni-method: getSystemService getSystemService (Ljava/lang/String;)Ljava/lang/Object;
jni-method: getWindow getWindow ()Landroid/view/Window;
jni-method: getResources getResources ()Landroid/content/res/Resources;
jni-method: showIME showIME ()V
jni-method: hideIME hideIME ()V
jni-method: get_SDK get_SDK ()I
jni-method: setEditLine setEditLine (Ljava/lang/String;I)V
jni-method: set_alarm set_alarm (J)V
jni-method: screen_on screen_on (I)V
jni-field: clipboardManager clipboardManager Landroid/text/ClipboardManager;
jni-field: connectivityManager connectivityManager Landroid/net/ConnectivityManager;
jni-field: gforthintent gforthintent Landroid/app/PendingIntent;
jni-field: hideprog hideprog Ljava/lang/Runnable;
jni-field: gforth-handler handler Landroid/os/Handler;

: SDK_INT clazz .get_SDK ;

jni-class: android/os/Handler
jni-method: post post (Ljava/lang/Runnable;)Z

: post-it ( runable-xt -- )
    clazz >o execute gforth-handler >o post ref> drop o> ;

jni-class: android/app/Activity
jni-method: getWindowManager getWindowManager ()Landroid/view/WindowManager;

jni-class: android/view/WindowManager
jni-method: getDefaultDisplay getDefaultDisplay ()Landroid/view/Display;

jni-class: android/view/Display
jni-method: getRotation getRotation ()I
SDK_INT 13 >= [IF]
    jni-method: getSizeD getSize (Landroid/graphics/Point;)V
[ELSE]
    jni-method: getWidth getWidth ()I
    jni-method: getHeight getHeight ()I
[THEN]
jni-method: getMetrics getMetrics (Landroid/util/DisplayMetrics;)V

jni-class: android/graphics/Point
jni-new: newPoint ()V
jni-field: x x I
jni-field: y y I

jni-class: android/util/DisplayMetrics
jni-new: newDisplayMetrics ()V
jni-field: heightPixels heightPixels I
jni-field: widthPixels widthPixels I
jni-field: densityDpi densityDpi I
jni-field: xdpi xdpi F
jni-field: ydpi ydpi F
jni-field: density density F
jni-field: scaledDensity scaledDensity F

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
jni-method: ke_getMetaState getMetaState ()I
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

jni-class: android/net/ConnectivityManager
jni-method: getActiveNetworkInfo getActiveNetworkInfo ()Landroid/net/NetworkInfo;

jni-class: android/net/NetworkInfo
jni-method: getState getState ()Landroid/net/NetworkInfo$State;
jni-method: getType getType ()I
jni-method: getTypeName getTypeName ()Ljava/lang/String;
jni-method: isConnected isConnected ()Z

SDK_INT 11 >= [IF]
    jni-class: android/app/Notification$Builder
    jni-new: newNotification.Builder (Landroid/content/Context;)V
    jni-method: setContentTitle setContentTitle (Ljava/lang/CharSequence;)Landroid/app/Notification$Builder;
    jni-method: setContentText setContentText (Ljava/lang/CharSequence;)Landroid/app/Notification$Builder;
    jni-method: setTicker setTicker (Ljava/lang/CharSequence;)Landroid/app/Notification$Builder;
    SDK_INT 21 >= [IF]
	jni-method: addPerson addPerson (Ljava/lang/String;)Landroid/app/Notification$Builder;
    [THEN]
    jni-method: setAutoCancel setAutoCancel (Z)Landroid/app/Notification$Builder;
    jni-method: setSmallIcon setSmallIcon (I)Landroid/app/Notification$Builder;
    jni-method: setLights setLights (III)Landroid/app/Notification$Builder;
    jni-method: setDefaults setDefaults (I)Landroid/app/Notification$Builder;
    jni-method: setSound setSound (Landroid/net/Uri;I)Landroid/app/Notification$Builder;
    jni-method: setContentIntent setContentIntent (Landroid/app/PendingIntent;)Landroid/app/Notification$Builder;
    SDK_INT 16 >= [IF]
	jni-method: build build ()Landroid/app/Notification;
    [ELSE]
	jni-method: build getNotification ()Landroid/app/Notification;
    [THEN]
[THEN]

jni-class: android/app/NotificationManager
jni-method: notify notify (ILandroid/app/Notification;)V

SDK_INT 10 <= [IF] \ 2.3.x uses a different clipboard manager
    jni-class: android/text/ClipboardManager

    jni-method: hasText hasText ()Z
    jni-method: getText getText ()Ljava/lang/CharSequence;
    jni-method: setText setText (Ljava/lang/CharSequence;)V
[ELSE]
    jni-class: android/content/ClipboardManager
    
    jni-method: getPrimaryClip getPrimaryClip ()Landroid/content/ClipData;
    jni-method: hasPrimaryClip hasPrimaryClip ()Z
    jni-method: setPrimaryClip setPrimaryClip (Landroid/content/ClipData;)V
    jni-method: setText setText (Ljava/lang/CharSequence;)V
    
    jni-class: android/content/ClipData

    jni-method: getItemCount getItemCount ()I
    jni-method: getItemAt getItemAt (I)Landroid/content/ClipData$Item;
    jni-static: newPlainText newPlainText (Ljava/lang/CharSequence;Ljava/lang/CharSequence;)Landroid/content/ClipData;
    
    jni-class: android/content/ClipData$Item
    
    jni-method: getText getText ()Ljava/lang/CharSequence;
    jni-method: getIntent getIntent ()Landroid/content/Intent;
    jni-method: getUri getUri ()Landroid/net/Uri;
    jni-method: coerceToText coerceToText (Landroid/content/Context;)Ljava/lang/CharSequence;
    SDK_INT 16 u>= [IF]
	jni-method: getHtmlText getHtmlText ()Ljava/lang/String;
    [THEN]
[THEN]

jni-class: java/lang/Object
jni-method: toString toString ()Ljava/lang/String;

jni-class: android/content/res/Resources
jni-method: getIdentifier getIdentifier (Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)I

jvalue res clazz .getResources to res
: R.id ( addr u -- id ) make-jstring 0 0 res .getIdentifier ;

: l[] ( n list -- object )  >o l-get o> ;
: l# ( list -- n )  >o l-size o> ;

: l-map ( xt list -- )  >o { xt } ( -- )
    l-size 0 ?DO  I l-get >o xt execute ref>  LOOP o> ;

Variable kbflag     kbflag off

: hidekb ( -- )  clazz >o hideIME o> kbflag off ;
: showkb ( -- )  clazz >o showIME o> kbflag on ;
: hidestatus ( -- )
    [ SDK_INT 16 < ] [IF]
	clazz >o getWindow >o $400 addFlags ref> o>
    [ELSE]
	clazz >o getWindow >o getDecorView >o $1004 setSystemUiVisibility
	ref> ref> o>
    [THEN] ;
: showstatus ( -- )
    [ SDK_INT 16 < ] [IF]
	clazz >o getWindow >o $400 clearFlags ref> o>
    [ELSE]
	clazz >o getWindow >o getDecorView >o 0 setSystemUiVisibility
	ref> ref> o>
    [THEN] ;

: togglekb ( -- )
    kbflag @ IF  hidekb  ELSE  showkb  THEN ;

SDK_INT 10 u<= [IF]
    : getclip? ( -- addr u / 0 0 )
	clazz .clipboardManager >o
	hasText IF
	    getText >o toString jstring>sstring ref>
	ELSE  0 0  THEN  ref> ;
    : setclip ( addr u -- )
	make-jstring clazz .clipboardManager >o setText ref> ;
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
    : setclip ( addr u -- )
	make-jstring clazz .clipboardManager >o setText ref> ;
\	make-jstring clazz .clipboardManager >o
\	js" text" swap newPlainText setPrimaryClip
\	ref> ;
[THEN]
: paste ( -- )
    getclip? dup IF  paste$ $! ctrl Y inskey  ELSE  2drop  THEN ;
: android-paste! ( addr u -- )
    2dup defers paste! setclip ;
' android-paste! is paste!

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