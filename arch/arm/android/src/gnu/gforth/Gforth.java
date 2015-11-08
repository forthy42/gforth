/* Android activity for Gforth on Android

  Copyright (C) 2013,2014 Free Software Foundation, Inc.

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
import android.text.ClipboardManager;
import android.content.BroadcastReceiver;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.ActivityInfo;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
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
import android.view.SurfaceHolder;
import android.view.OrientationEventListener;
import android.view.ViewTreeObserver.OnGlobalLayoutListener;
import android.view.WindowManager;
import android.view.inputmethod.InputConnection;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.BaseInputConnection;
import android.view.inputmethod.InputMethodManager;
import android.view.inputmethod.CompletionInfo;
import android.text.InputType;
import android.text.SpannableStringBuilder;
import android.text.Editable;
import android.app.Activity;
import android.app.ProgressDialog;
import android.app.AlarmManager;
import android.app.PendingIntent;
import android.net.ConnectivityManager;
import android.util.Log;
import java.lang.Object;
import java.lang.Runnable;
import java.lang.String;
import java.io.File;
import java.util.Locale;

public class Gforth
    extends android.app.Activity
    implements KeyEvent.Callback,
	       OnVideoSizeChangedListener,
	       LocationListener,
	       SensorEventListener,
	       SurfaceHolder.Callback2,
	       OnGlobalLayoutListener /* ,
	       ClipboardManager.OnPrimaryClipChangedListener */ {
    private long argj0=1000; // update every second
    private double argf0=10;    // update every 10 meters
    private String args0="gps";
    private Sensor argsensor;
    private Gforth gforth;
    private LocationManager locationManager;
    private SensorManager sensorManager;
    private ClipboardManager clipboardManager;
    private AlarmManager alarmManager;
    private ConnectivityManager connectivityManager;
    private BroadcastReceiver recKeepalive, recConnectivity;

    private PendingIntent pintent, gforthintent;

    private boolean started=false;
    private boolean libloaded=false;
    private boolean surfaced=false;
    private int activated;
    private String beforec="", afterc="";

    public Handler handler;
    public Runnable startgps;
    public Runnable stopgps;
    public Runnable startsensor;
    public Runnable stopsensor;
    public Runnable showprog;
    public Runnable hideprog;
    public Runnable errprog;
    public Runnable appexit;
    public ProgressDialog progress;

    private static final String META_DATA_LIB_NAME = "android.app.lib_name";
    private static final String TAG = "Gforth";

    public native void onEventNative(int type, Object event);
    public native void onEventNative(int type, int event);
    public native void callForth(long xt); // !! use long for 64 bits !!
    public native void startForth(String libdir, String locale);

    // own subclasses
    public class RunForth implements Runnable {
	long xt;
	RunForth(long initxt) {
	    xt = initxt;
	}
	public void run() {
	    callForth(xt);
	}
    }

    static class MyInputConnection extends BaseInputConnection {
	private SpannableStringBuilder mEditable;
	private ContentView mView;
	
	public MyInputConnection(View targetView, boolean fullEditor) {
	    super(targetView, fullEditor);
	    mView = (ContentView) targetView;
	}
	
	public Editable getEditable() {
	    if (mEditable == null) {
		mEditable = (SpannableStringBuilder) Editable.Factory.getInstance()
		    .newEditable("");
	    }
	    return mEditable;
	}

	public void setEditLine(String line, int curpos) {
	    Log.v(TAG, "IC.setEditLine: \"" + line + "\" at: " + curpos);
	    getEditable().clear();
	    getEditable().append(line);
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
		send+="\b";
	    }
	    for(i=0; i<after; i++) {
		send+="\033[3~";
	    }
	    mView.mActivity.onEventNative(12, send); 
	    return true;
	}
	public boolean setComposingRegion (int start, int end) {
	    end-=start;
	    if(end < 0) {
		start+=end;
		end = -end;
	    }
	    mView.mActivity.onEventNative(19, start);
	    mView.mActivity.onEventNative(20, end);
	    return super.setComposingRegion(start, start+end);
	}
	public boolean sendKeyEvent (KeyEvent event) {
	    mView.mActivity.onEventNative(0, event);
	    return true;
	}
    }

    static class ContentView extends View {
        Gforth mActivity;
	InputMethodManager mManager;
	EditorInfo moutAttrs;
	MyInputConnection mInputConnection;

        public ContentView(Gforth context) {
            super(context);
	    mActivity=context;
	    mManager = (InputMethodManager)context.getSystemService(Context.INPUT_METHOD_SERVICE);
	    setFocusable(true);
	    setFocusableInTouchMode(true);
        }
	public void showIME() {
	    mManager.showSoftInput(this, 0);
	}
	public void hideIME() {
	    mManager.hideSoftInputFromWindow(getWindowToken(), 0);
	}

	@Override
	public boolean onCheckIsTextEditor () {
	    return true;
	}
	@Override
	public InputConnection onCreateInputConnection (EditorInfo outAttrs) {
	    moutAttrs=outAttrs;
	    outAttrs.inputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_AUTO_COMPLETE | InputType.TYPE_TEXT_FLAG_AUTO_CORRECT;
	    outAttrs.initialSelStart = 1;
	    outAttrs.initialSelEnd = 1;
	    outAttrs.packageName = "gnu.gforth";
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
	/* @Override
	public boolean onKeyPreIme (int keyCode, KeyEvent event) {
	    mActivity.onEventNative(0, event);
	    return true;
	    } */
    }
    ContentView mContentView;

    public void hideProgress() {
	if(progress!=null) {
	    progress.dismiss();
	    progress=null;
	}
    }
    public void showProgress() {
	progress = ProgressDialog.show(this, "Unpacking files",
				       "please wait", true);
    }
    public void doneProgress() {
	progress.setMessage("Done; restart Gforth");
    }
    public void errProgress() {
	progress.setMessage("error: no space left");
    }

    public void showIME() {
	mContentView.showIME();
    }
    public void hideIME() {
	mContentView.hideIME();
    }
    public void setEditLine(String line, int curpos) {
	Log.v(TAG, "setEditLine: \"" + line + "\" at: " + curpos);
	mContentView.mInputConnection.setEditLine(line, curpos);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        ActivityInfo ai;
        String libname = "gforth";

	gforth=this;
	progress=null;

        getWindow().takeSurface(this);
        // getWindow().setFormat(PixelFormat.RGB_565);
        getWindow().setSoftInputMode(
                WindowManager.LayoutParams.SOFT_INPUT_STATE_UNSPECIFIED
                | WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE);

        mContentView = new ContentView(this);
        setContentView(mContentView);
        mContentView.requestFocus();
        mContentView.getViewTreeObserver().addOnGlobalLayoutListener(this);
	// setRetainInstance(true);

	try {
            ai = getPackageManager().getActivityInfo(getIntent().getComponent(), PackageManager.GET_META_DATA);
            if (ai.metaData != null) {
                String ln = ai.metaData.getString(META_DATA_LIB_NAME);
                if (ln != null) libname = ln;
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
	connectivityManager = (ConnectivityManager) getSystemService(Context.CONNECTIVITY_SERVICE);

	handler=new Handler();
	startgps=new Runnable() {
		public void run() {
		    locationManager.requestLocationUpdates(args0, argj0, (float)argf0, (LocationListener)gforth);
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
	
	recKeepalive = new BroadcastReceiver() {
		@Override public void onReceive(Context context, Intent foo)
		{
		    // Log.v(TAG, "alarm received");
		    onEventNative(21, 0);
		}
	    };
	registerReceiver(recKeepalive, new IntentFilter("gnu.gforth.keepalive") );
	
	pintent = PendingIntent.getBroadcast(this, 0, new Intent("gnu.gforth.keepalive"), 0);
	gforthintent = PendingIntent.getActivity(this,
						 1, new Intent(this, Gforth.class),
						 PendingIntent.FLAG_UPDATE_CURRENT);
	gforthintent.setFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);

	recConnectivity = new BroadcastReceiver() {
		public void onReceive(Context context, Intent intent) {
		    // boolean metered = connectivityManager.isActiveNetworkMetered();
		    onEventNative(22, 0);
		}
	    };

	registerReceiver(recConnectivity, new IntentFilter("android.net.conn.CONNECTIVITY_CHANGE"));
    }

    @Override protected void onStart() {
	super.onStart();
	if(!started) {
	    startForth(getApplicationInfo().nativeLibraryDir,
		       Locale.getDefault().toString() + ".UTF-8");
	    started=true;
	}
	activated = -1;
	if(surfaced) onEventNative(18, activated);
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
    /*
    @Override
    public void onPrimaryClipChanged() {
	onEventNative(16, 0);
    }
    */
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
}
