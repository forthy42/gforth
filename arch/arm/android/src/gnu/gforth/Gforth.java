/* Android activity for Gforth on Android

  Authors: Bernd Paysan, Anton Ertl
  Copyright (C) 2013,2014,2015,2016,2017,2018,2019 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation, either version 3
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, see http://www.gnu.org/licenses/.
*/

package gnu.gforth;

import android.os.Bundle;
import android.os.Handler;
import android.os.Build;
import android.os.Environment;
import android.os.PowerManager;
import android.os.PowerManager.WakeLock;
import android.text.ClipboardManager;
import android.content.BroadcastReceiver;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.ActivityInfo;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.content.res.AssetFileDescriptor;
import android.media.AudioManager;
import android.media.MediaPlayer;
import android.media.MediaPlayer.OnVideoSizeChangedListener;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.content.Context;
import android.view.View;
import android.view.Window;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.Surface;
import android.view.SurfaceView;
import android.view.SurfaceHolder;
import android.view.OrientationEventListener;
import android.view.ViewTreeObserver.OnGlobalLayoutListener;
import android.view.WindowManager;
import android.view.inputmethod.InputMethodManager;
import android.view.inputmethod.BaseInputConnection;
import android.view.inputmethod.CompletionInfo;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputConnection;
import android.view.inputmethod.ExtractedText;
import android.view.inputmethod.ExtractedTextRequest;
import android.text.Editable;
import android.text.InputType;
import android.text.SpannableStringBuilder;
import android.app.Activity;
import android.app.ProgressDialog;
import android.app.AlarmManager;
import android.app.PendingIntent;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.NotificationChannel;
import android.net.ConnectivityManager;
import android.net.Uri;
import android.util.Log;
import android.util.AttributeSet;
import android.widget.Toast;
import android.content.pm.PackageManager;
import android.Manifest;
import java.lang.Object;
import java.lang.Runnable;
import java.lang.String;
import java.io.File;
import java.io.InputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.Locale;
import gnu.gforth.R;

public class Gforth
    extends android.app.Activity
    implements KeyEvent.Callback,
	       OnVideoSizeChangedListener,
	       LocationListener,
	       SensorEventListener,
	       SurfaceHolder.Callback2,
	       OnGlobalLayoutListener {
    private long argj0=1000; // update every second
    private double argf0=10;    // update every 10 meters
    private String args0="gps";
    private Sensor argsensor;
    private Notification argnotify;
    private Gforth gforth;
    private LocationManager locationManager;
    private SensorManager sensorManager;
    private ClipboardManager clipboardManager;
    private AlarmManager alarmManager;
    private AudioManager audioManager;
    private ConnectivityManager connectivityManager;
    private InputMethodManager inputMethodManager;
    private BroadcastReceiver recKeepalive, recConnectivity;
    private PendingIntent pintent, gforthintent;
    private PowerManager powerManager;
    private NotificationManager notificationManager;
    private NotificationChannel notificationChannel;
    private WakeLock wl, wl_cpu;
    private GforthView mView;
    private InputStream gforthfd;

    private boolean started=false;
    private boolean libloaded=false;
    private boolean surfaced=false;
    private int activated;
    private String beforec="", afterc="";
    private String startfile = "starta.fs";

    public Handler handler;
    public Runnable startgps;
    public Runnable stopgps;
    public Runnable startsensor;
    public Runnable stopsensor;
    public Runnable showprog;
    public Runnable hideprog;
    public Runnable doneprog;
    public Runnable errprog;
    public Runnable appexit;
    public Runnable rshowstatus;
    public Runnable rhidestatus;
    public Runnable rkeepscreenon;
    public Runnable rkeepscreenoff;
    public Runnable rsecurescreenon;
    public Runnable rsecurescreenoff;
    public Runnable notifyer;
    public Runnable startbrowser;
    public ProgressDialog progress;
    public String cameraPath;

    private static final String META_DATA_LIB_NAME = "android.app.lib_name";
    private static final String META_DATA_STARTFILE = "android.app.startfile";
    private static final String TAG = "Gforth";

    public native void onEventNative(int type, Object event);
    public native void onEventNative(int type, int event);
    public native void callForth(long xt); // !! use long for 64 bits !!
    public native void startForth(String libdir, String locale, String startfile);

    // own subclasses
    static class GforthView extends SurfaceView implements SurfaceHolder.Callback2 {
	Gforth mActivity;
	InputMethodManager mManager;
	EditorInfo moutAttrs;
	MyInputConnection mInputConnection;
	int mcurpos=0, mlen=0;
	
	static class MyInputConnection extends BaseInputConnection {
	    private SpannableStringBuilder mEditable;
	    private GforthView mView;
	    private String mText;
	    private ExtractedTextRequest mExtractedTextRequest;
	    private ExtractedText et;
	    
	    public MyInputConnection(GforthView targetView, boolean fullEditor) {
		super(targetView, fullEditor);
		mView = targetView;
		mExtractedTextRequest = null;
	    }

	    private ExtractedText setET(String text, int curpos, int len) {
		if(et == null) {
		    et = new ExtractedText();
		}
		et.partialStartOffset = 0;
		et.partialEndOffset = (text != null) ? text.length() : 0;
		et.startOffset = 0;
		et.selectionStart = curpos; // getSelectionStart();
		et.selectionEnd = curpos+len; // getSelectionEnd();
		et.flags = 0;
		et.text = text;

		return et;
	    }
	    private int min(int a, int b) { return a < b ? a : b; }
	    private int max(int a, int b) { return a > b ? a : b; }
	    public CharSequence	getTextBeforeCursor(int n, int flags) {
		if(et == null || et.text == null) return "";
		return et.text.subSequence(max(0, et.selectionStart-n),
					   et.selectionStart);
	    }
	    public CharSequence	getTextAfterCursor(int n, int flags) {
		if(et == null || et.text == null) return "";
		return et.text.subSequence(et.selectionStart,
					   min(et.text.length(), et.selectionStart+n));
	    }
	    public Editable getEditable() {
		if (mEditable == null) {
		    mEditable = (SpannableStringBuilder) Editable.Factory.getInstance()
			.newEditable("");
		}
		return mEditable;
	    }
	    
	    public void setEditLine(String line, int curpos, int len) {
		// Log.v(TAG, "IC.setEditLine: \"" + line + "\" at: " + curpos);
		getEditable().clear();
		getEditable().append(line);
		if(mExtractedTextRequest != null) {
		    mView.mManager.
			updateExtractedText(mView, mExtractedTextRequest.token,
					    setET(line, curpos, len));
		}
		setSelection(curpos, curpos);
	    }
	    
	    public boolean commitText(CharSequence text, int newcp) {
		if(text != null) {
		    mView.mActivity.onEventNative(12, text.toString());
		} else {
		    mView.mActivity.onEventNative(12, 0);
		}
		return true;
	    }
	    public boolean setComposingText(CharSequence text, int newcp) {
		if(text != null) {
		    mView.mActivity.onEventNative(13, text.toString());
		} else {
		    mView.mActivity.onEventNative(13, "");
		}
		return true;
	    }
	    public boolean finishComposingText () {
		mView.mActivity.onEventNative(12, 0);
		return true;
	    }
	    public boolean commitCompletion(CompletionInfo text) {
		if(text != null) {
		    mView.mActivity.onEventNative(12, text.toString());
		} else {
		    mView.mActivity.onEventNative(12, 0);
		}
		return true;
	    }
	    public boolean deleteSurroundingText (int before, int after) {
		int i;
		String send="";
		for(i=0; i<before; i++) {
		    send+="\177";
		}
		for(i=0; i<after; i++) {
		    send+="\033[3~";
		}
		mView.mActivity.onEventNative(12, send); 
		return true;
	    }
	    public boolean setComposingRegion (int start, int end) {
		if(start > end) {
		    int start1 = start;
		    start = end;
		    end = start1;
		}
		end -= start;
		if(end > 0) {
		    mView.mActivity.onEventNative(19, start);
		    mView.mActivity.onEventNative(20, end);
		}
		return super.setComposingRegion(start, start+end);
	    }
	    public boolean sendKeyEvent (KeyEvent event) {
		mView.mActivity.onEventNative(0, event);
		return true;
	    }
	    public ExtractedText getExtractedText(ExtractedTextRequest request, int flags) {
		if ((flags & GET_EXTRACTED_TEXT_MONITOR) != 0)
		    mExtractedTextRequest = request;  // mExtractedTextRequest currently doing nothing
		else
		    mExtractedTextRequest = null;

		return setET("", 0, 0);
	    }

	    @Override
	    public boolean performContextMenuAction(int id) {
		mView.mActivity.onEventNative(24, id);
		return true;
	    }
	}
	
	public void init(Context context) {
	    mActivity=(Gforth)context;
	    mManager=(InputMethodManager)context.getSystemService(Context.INPUT_METHOD_SERVICE);

	    setFocusable(true);
	    setFocusableInTouchMode(true);
	}
	
	public GforthView(Context context) {
	    super(context);
	    init(context);
	}
	
	public GforthView(Context context, AttributeSet attrs) {
	    super(context, attrs);
	    init(context);
	}
	
	public GforthView(Context context, AttributeSet attrs, int defStyle) {
	    super(context, attrs, defStyle);
	    init(context);
	}
	
	@Override
	public boolean onCheckIsTextEditor () {
	    return true;
	}
	@Override
	public InputConnection onCreateInputConnection (EditorInfo outAttrs) {
	    moutAttrs=outAttrs;
	    outAttrs.inputType = (InputType.TYPE_CLASS_TEXT | /*
				  InputType.TYPE_TEXT_FLAG_AUTO_COMPLETE | */
				  InputType.TYPE_TEXT_FLAG_AUTO_CORRECT);
	    outAttrs.initialSelStart = mcurpos;
	    outAttrs.initialSelEnd = mcurpos+mlen;
	    outAttrs.packageName = "gnu.gforth";
	    outAttrs.imeOptions = (EditorInfo.IME_FLAG_NO_FULLSCREEN);
	    mInputConnection = new MyInputConnection(this, true);
	    return mInputConnection;
	}
	@Override
	public void onSizeChanged(int w, int h, int oldw, int oldh) {
	    mActivity.onEventNative(14, w);
	    mActivity.onEventNative(15, h);
	}
	@Override
	public boolean dispatchKeyEvent (KeyEvent event) {
	    mActivity.onEventNative(0, event);
	    return true;
	}
	@Override
	public boolean onKeyDown (int keyCode, KeyEvent event) {
	    mActivity.onEventNative(0, event);
	    return true;
	}
	@Override
	public boolean onKeyUp (int keyCode, KeyEvent event) {
	    mActivity.onEventNative(0, event);
	    return true;
	}
	@Override
	public boolean onKeyMultiple (int keyCode, int repeatCount, KeyEvent event) {
	    mActivity.onEventNative(0, event);
	    return true;
	}
	@Override
	public boolean onKeyLongPress (int keyCode, KeyEvent event) {
	    mActivity.onEventNative(0, event);
	    return true;
	}
	@Override
	public void surfaceCreated(SurfaceHolder holder) {
	    mActivity.surfaceCreated(holder);
	}
	@Override
	public void surfaceChanged(SurfaceHolder holder, int format, int width, int height) {
	    mActivity.surfaceChanged(holder, format, width, height);
	}
	@Override
	public void surfaceRedrawNeeded(SurfaceHolder holder) {
	    mActivity.surfaceRedrawNeeded(holder);
	}
	@Override
	public void surfaceDestroyed(SurfaceHolder holder) {
	    mActivity.surfaceDestroyed(holder);
	}
	public void showIME() {
	    mManager.showSoftInput(this, 0);
	}
	public void hideIME() {
	    mManager.hideSoftInputFromWindow(getWindowToken(), 0);
	}
	public void restartIME() {
	    mManager.restartInput(this);
	}
    }

    public class RunForth implements Runnable {
	long xt;
	RunForth(long initxt) {
	    xt = initxt;
	}
	public void run() {
	    callForth(xt);
	}
    }
    
    public void hideProgress() {
	if(progress!=null) {
	    Log.v(TAG, "Dismiss spinner");
	    progress.dismiss();
	    progress=null;
	}
    }
    public void showProgress() {
	if(progress!=null) {
	    progress.setTitle("Unpacking more files");
	    progress.setMessage("please wait a little longer");
	} else {
	    progress = ProgressDialog.show(this, "Unpacking files",
					   "please wait", true, true);
	}
    }
    public void doneProgress() {
	if(progress!=null) {
	    Log.v(TAG, "Done spinner");
	    progress.setTitle("Unpacked files");
	    progress.setMessage("Done; restart Gforth");
	}
    }
    public void errProgress() {
	if(progress!=null) {
	    Log.v(TAG, "Error spinner");
	    progress.setMessage("error: can't write to storage, no permission/space left?");
	}
    }

    public void showIME() {
	if(mView!=null) mView.showIME();
    }
    public void hideIME() {
	if(mView!=null) mView.hideIME();
    }
    public void restartIME() {
	if(mView!=null) mView.restartIME();
    }
    public void showStatus() {
	if (Build.VERSION.SDK_INT < 16) {
	    getWindow().clearFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);
	}
	else {
	    getWindow().getDecorView().setSystemUiVisibility(0);
	}
    }
    public void hideStatus() {
	// Hide Status Bar
	if (Build.VERSION.SDK_INT < 16) {
	    getWindow().addFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);
	}
	else if (Build.VERSION.SDK_INT < 19) {
	    getWindow().getDecorView().setSystemUiVisibility(0x1004);
	    // View.SYSTEM_UI_FLAG_FULLSCREEN | SYSTEM_UI_FLAG_IMMERSIVE_STICKY
	}
	else {
	    getWindow().getDecorView().setSystemUiVisibility(0x806);
	    // View.SYSTEM_UI_FLAG_FULLSCREEN | SYSTEM_UI_FLAG_IMMERSIVE
	}
    }
    public void setEditLine(String line, int curpos, int len) {
	// Log.v(TAG, "setEditLine: \"" + line + "\" at: " + curpos + " len: " + len);
	if(mView!=null && mView.mInputConnection!=null) {
	    mView.mcurpos = curpos;
	    mView.mlen = len;
	    mView.mInputConnection.setEditLine(line, curpos, len);
	}
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        ActivityInfo ai;
        String libname = "gforth";

	gforth=this;
	
	progress=null;
	cameraPath = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).getAbsolutePath();

        getWindow().setSoftInputMode(
                WindowManager.LayoutParams.SOFT_INPUT_STATE_UNSPECIFIED
                | WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE);

        setContentView(R.layout.main);
        mView = (GforthView)findViewById(R.id.surfaceview);
        mView.getHolder().addCallback(this);
        mView.requestFocus();
        mView.getViewTreeObserver().addOnGlobalLayoutListener(this);

	try {
            ai = getPackageManager().getActivityInfo(getIntent().getComponent(), PackageManager.GET_META_DATA);
            if (ai.metaData != null) {
                String ln = ai.metaData.getString(META_DATA_LIB_NAME);
                if (ln != null) libname = ln;
                String sf = ai.metaData.getString(META_DATA_STARTFILE);
                if (sf != null) startfile = sf;
            }
        } catch (PackageManager.NameNotFoundException e) {
            throw new RuntimeException("Error getting activity info", e);
        }
	if(!libloaded) {
	    Log.v(TAG, "open library: " + libname);
	    System.loadLibrary(libname);
	    libloaded=true;
	} else {
	    Log.v(TAG, "Library already loaded");
	}
	super.onCreate(savedInstanceState);

	locationManager=(LocationManager)getSystemService(Context.LOCATION_SERVICE);
	sensorManager=(SensorManager)getSystemService(Context.SENSOR_SERVICE);
	clipboardManager=(ClipboardManager)getSystemService(Context.CLIPBOARD_SERVICE);
	alarmManager=(AlarmManager)getSystemService(Context.ALARM_SERVICE);
	audioManager=(AudioManager)getSystemService(Context.AUDIO_SERVICE);
	connectivityManager=(ConnectivityManager)getSystemService(Context.CONNECTIVITY_SERVICE);
	inputMethodManager=(InputMethodManager)getSystemService(Context.INPUT_METHOD_SERVICE);
	powerManager=(PowerManager)getSystemService(Context.POWER_SERVICE);
	notificationManager=(NotificationManager)getSystemService(Context.NOTIFICATION_SERVICE);
	if (Build.VERSION.SDK_INT >= 26) {
	    notificationChannel=new NotificationChannel("gnu.gforth.notifications", "Messages", NotificationManager.IMPORTANCE_DEFAULT);
	    notificationChannel.enableLights(true);
	    notificationChannel.setShowBadge(true);
	    
	    notificationManager.createNotificationChannel(notificationChannel);
	}
	
	wl = powerManager.newWakeLock(PowerManager.SCREEN_DIM_WAKE_LOCK |PowerManager.ACQUIRE_CAUSES_WAKEUP |PowerManager.ON_AFTER_RELEASE,"gnu.gforth:MyLock");
	wl_cpu = powerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK,"gnu.gforth:MyCpuLock");
	
	handler=new Handler();
	startgps=new Runnable() {
		public void run() {
		    int permission = PackageManager.PERMISSION_GRANTED;
		    if (Build.VERSION.SDK_INT >= 23) { // reliably works with Android 6+
			checkSelfPermission(Manifest.permission.ACCESS_FINE_LOCATION);
		    }
		    if (permission == PackageManager.PERMISSION_GRANTED) {
			locationManager.requestLocationUpdates(args0, argj0, (float)argf0, (LocationListener)gforth);
		    }
		}
	    };
	stopgps=new Runnable() {
		public void run() {
		    locationManager.removeUpdates((LocationListener)gforth);
		}
	    };
	startsensor=new Runnable() {
		public void run() {
		    sensorManager.registerListener((SensorEventListener)gforth, argsensor, (int)argj0);
		}
	    };
	stopsensor=new Runnable() {
		public void run() {
		    sensorManager.unregisterListener((SensorEventListener)gforth, argsensor);
		}
	    };
	showprog=new Runnable() {
		public void run() {
		    showProgress();
		}
	    };
	hideprog=new Runnable() {
		public void run() {
		    hideProgress();
		}
	    };
	doneprog=new Runnable() {
		public void run() {
		    doneProgress();
		}
	    };
	errprog=new Runnable() {
		public void run() {
		    errProgress();
		}
	    };
	appexit=new Runnable() {
		public void run() {
		    finish();
		}
	    };
	rshowstatus=new Runnable() {
		public void run() {
		    showStatus();
		}
	    };
	rhidestatus=new Runnable() {
		public void run() {
		    hideStatus();
		}
	    };
	rkeepscreenon=new Runnable() {
		public void run() {
		    getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
		}
	    };
	rkeepscreenoff=new Runnable() {
		public void run() {
		    getWindow().clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
		}
	    };
	rsecurescreenon=new Runnable() {
		public void run() {
		    getWindow().addFlags(WindowManager.LayoutParams.FLAG_SECURE);
		}
	    };
	rsecurescreenoff=new Runnable() {
		public void run() {
		    getWindow().clearFlags(WindowManager.LayoutParams.FLAG_SECURE);
		}
	    };
	notifyer=new Runnable() {
		public void run() {
		    Log.v(TAG, "show notification");
		    notificationManager.notify((int)argj0, argnotify);
		    Log.v(TAG, "done notification");
		}
	    };
	startbrowser=new Runnable() {
		public void run() {
		    startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse(args0)));
		}
	    };
	
	recKeepalive = new BroadcastReceiver() {
		@Override public void onReceive(Context context, Intent foo)
		{
		    // Log.v(TAG, "alarm received");
		    // wl_cpu.acquire(500); // 500 ms wakelock to handle the alarm
		    onEventNative(21, 0);
		}
	    };
	registerReceiver(recKeepalive, new IntentFilter("gnu.gforth.keepalive") );
	
	pintent = PendingIntent.getBroadcast(this, 0, new Intent("gnu.gforth.keepalive"), 0);

	// intent for notifications
	gforthintent = PendingIntent.getActivity
	    (this, 1,
	     new Intent(this, getClass())
	     .setAction("gnu.gforth.Gforth_n2o.MESSAGE")
	     .setFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT |
		       Intent.FLAG_ACTIVITY_SINGLE_TOP),
	     PendingIntent.FLAG_UPDATE_CURRENT);

	// intent for network connectivity (!!use netlink socket!!)
	recConnectivity = new BroadcastReceiver() {
		public void onReceive(Context context, Intent intent) {
		    // boolean metered = connectivityManager.isActiveNetworkMetered();
		    onEventNative(22, 0);
		}
	    };

	registerReceiver(recConnectivity, new IntentFilter("android.net.conn.CONNECTIVITY_CHANGE"));

	Log.v(TAG, "Open resource input stream");
	gforthfd=getResources().openRawResource(R.raw.gforth);
	Log.v(TAG, "onCreate done");
    }

    @Override protected void onStart() {
	super.onStart();
	if(verifyStoragePermissions(this)) {
	    doStart();
	}
    }

    public void doStart() {
	if(!started) {
	    startForth(getApplicationInfo().nativeLibraryDir,
		       Locale.getDefault().toString() + ".UTF-8",
		       startfile);
	    started=true;
	}
	activated = -1;
	if(surfaced) onEventNative(18, activated);
    }
    
    @Override protected void onNewIntent (Intent intent) {
	super.onNewIntent(intent);
	setIntent(intent);
	activated = -1;
	if(surfaced) onEventNative(18, activated);
	onEventNative(23, intent);
    }

    @Override protected void onResume() {
	super.onResume();
	activated = -2;
	if(surfaced) onEventNative(18, activated);
    }

    @Override protected void onPause() {
	activated = -1;
	if(surfaced) onEventNative(18, activated);
	super.onPause();
    }

    @Override protected void onStop() {
	activated = 0;
	onEventNative(18, activated);
	super.onStop();
    }
    @Override protected void onDestroy() {
	this.unregisterReceiver(recKeepalive);
	this.unregisterReceiver(recConnectivity);
	super.onDestroy();
    }

    @Override
    public boolean dispatchKeyEvent (KeyEvent event) {
	onEventNative(0, event);
	return true;
    }
    @Override
    public boolean onKeyDown (int keyCode, KeyEvent event) {
	onEventNative(0, event);
	return true;
    }
    @Override
    public boolean onKeyUp (int keyCode, KeyEvent event) {
	onEventNative(0, event);
	return true;
    }
    @Override
    public boolean onKeyMultiple (int keyCode, int repeatCount, KeyEvent event) {
	onEventNative(0, event);
	return true;
    }
    @Override
    public boolean onKeyLongPress (int keyCode, KeyEvent event) {
	onEventNative(0, event);
	return true;
    }
    @Override
    public boolean onTouchEvent(MotionEvent event) {
	onEventNative(1, event);
	return true;
    }

    // location requests
    public void onLocationChanged(Location location) {
	// Called when a new location is found by the network location provider.
	onEventNative(2, location);
    }
    public void onStatusChanged(String provider, int status, Bundle extras) {}
    public void onProviderEnabled(String provider) {}
    public void onProviderDisabled(String provider) {}

    // sensor events
    public void onAccuracyChanged(Sensor sensor, int accuracy) {}
    public void onSensorChanged(SensorEvent event) {
	onEventNative(3, event);
    }

    // surface stuff
    public void surfaceCreated(SurfaceHolder holder) {
	onEventNative(4, holder.getSurface());
	surfaced=true;
	onEventNative(18, activated);
    }
    
    public class surfacech {
	Surface surface;
	int format;
	int width;
	int height;

	surfacech(Surface s, int f, int w, int h) {
	    surface=s;
	    format=f;
	    width=w;
	    height=h;
	}
    }

    public void surfaceChanged(SurfaceHolder holder, int format, int width, int height) {
	surfacech sch = new surfacech(holder.getSurface(), format, width, height);

	onEventNative(5, sch);
    }
    
    public void surfaceRedrawNeeded(SurfaceHolder holder) {
	onEventNative(6, holder.getSurface());
    }

    public void surfaceDestroyed(SurfaceHolder holder) {
	surfaced=false;
	onEventNative(7, holder.getSurface());
    }

    // global layout
    public void onGlobalLayout() {
	onEventNative(8, 0);
    }

    // media player
    public class mpch {
	MediaPlayer mediaPlayer;
	int format;
	int width;
	int height;

	mpch(MediaPlayer m, int w, int h) {
	    mediaPlayer=m;
	    width=w;
	    height=h;
	}
    }

    @Override
    public void onVideoSizeChanged(MediaPlayer mp, int width, int height) {
	mpch newmp = new mpch(mp, width, height);

	onEventNative(9, newmp);
    }
    @Override
    public void onConfigurationChanged(Configuration newConfig) {
	Log.v(TAG, "Configuration changed");
	super.onConfigurationChanged(newConfig);
	
	onEventNative(17, newConfig.orientation);
    }

    public int get_SDK() {
	return Build.VERSION.SDK_INT;
    }

    public void set_alarm(long when) {
	// Log.v(TAG, "set alarm");
	alarmManager.set(AlarmManager.RTC_WAKEUP, when, pintent);
    }

    public void screen_on(int ms) {
	boolean isScreenOn = powerManager.isScreenOn();
	
	if(isScreenOn==false) {
	    wl.acquire(ms);
	    wl_cpu.acquire(ms);
	}
    }

    public String get_gforth_gz() {
	String filename=getFilesDir() + "/gforth.gz";
	try {
	    Log.v(TAG, "filename="+filename);
	    FileOutputStream gforthgz=openFileOutput("gforth.gz", Context.MODE_PRIVATE);
	    Log.v(TAG, "Open output stream");
	    int gforthlen = gforthfd.available();
	    Log.v(TAG, "Available bytes="+gforthlen);
	    byte buffer[] = new byte[gforthlen];
	    Log.v(TAG, "read "+gforthlen+" bytes");
	    gforthfd.read(buffer, 0, gforthlen);
	    Log.v(TAG, "write "+gforthlen+" bytes");
	    gforthgz.write(buffer, 0, gforthlen);
	    Log.v(TAG, "close output");
	    gforthgz.close();
	} catch(IOException ex) {
	    Log.v(TAG, "IO Exception "+ex.toString());
	}
	Log.v(TAG, "Return back");
	return filename;
    }

    // Storage Permissions
    private static final int REQUEST_EXTERNAL_STORAGE = 1;
    ;
    
    /**
     * Checks if the app has permission to write to device storage
     *
     * If the app does not has permission then the user will be prompted to grant permissions
     *
     * @param activity
     */
    public static String[] REQUEST_STRING = {
	Manifest.permission.READ_EXTERNAL_STORAGE,
	Manifest.permission.WRITE_EXTERNAL_STORAGE
    };
    public boolean verifyStoragePermissions(Activity activity) {
	// Check if we have write permission
	if (Build.VERSION.SDK_INT >= 23) { // reliably works with Android 6+
	    int permission = checkSelfPermission(Manifest.permission.WRITE_EXTERNAL_STORAGE);

	    if (permission != PackageManager.PERMISSION_GRANTED) {
		// We don't have permission so prompt the user
		requestPermissions(REQUEST_STRING, REQUEST_EXTERNAL_STORAGE);
		return false;
	    }
	}
	return true;
    }

    @Override
    public void onRequestPermissionsResult (int requestCode, 
					    String[] permissions, 
					    int[] grantResults) {
	if(requestCode != REQUEST_EXTERNAL_STORAGE) {
	    for(int i = 0; i <= grantResults.length - 1; i++) {
		if(grantResults[i] == PackageManager.PERMISSION_GRANTED) {
		    onEventNative(26, permissions[i]);
		}
	    }
	    onEventNative(25, requestCode);
	    return;
	}
	for(int i = 0; i <= grantResults.length - 1; i++) {
	    if(grantResults[i] != PackageManager.PERMISSION_GRANTED) return;
	}
	doStart();
    }
}
