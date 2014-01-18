/* Android activity for Gforth on Android

  Copyright (C) 2013 Free Software Foundation, Inc.

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
import android.content.pm.ActivityInfo;
import android.content.pm.PackageManager;
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
import android.view.ViewTreeObserver.OnGlobalLayoutListener;
import android.view.WindowManager;
import android.app.Activity;
import android.util.Log;
import java.lang.Object;
import java.lang.Runnable;
import java.lang.String;
import java.io.File;

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
    private Gforth gforth;
    private LocationManager locationManager;
    private SensorManager sensorManager;

    public Handler handler;
    public Runnable startgps;
    public Runnable stopgps;
    public Runnable startsensor;
    public Runnable stopsensor;

    private static final String META_DATA_LIB_NAME = "android.app.lib_name";
    private static final String TAG = "Gforth";

    public native void onEventNative(int type, Object event);
    public native void onEventNative(int type, int event);
    public native void callForth(int xt); // !! use long for 64 bits !!
    public native void startForth();

    public class RunForth implements Runnable {
	int xt;
	RunForth(int initxt) {
	    xt = initxt;
	}
	public void run() {
	    callForth(xt);
	}
    }

    static class ContentView extends View {
        Gforth mActivity;

        public ContentView(Context context) {
            super(context);
        }
    }
    ContentView mContentView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        ActivityInfo ai;
        String libname = "gforth";
	gforth=this;

        getWindow().takeSurface(this);
        // getWindow().setFormat(PixelFormat.RGB_565);
        getWindow().setSoftInputMode(
                WindowManager.LayoutParams.SOFT_INPUT_STATE_UNSPECIFIED
                | WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE);

        mContentView = new ContentView(this);
        mContentView.mActivity = this;
        setContentView(mContentView);
        mContentView.requestFocus();
        mContentView.getViewTreeObserver().addOnGlobalLayoutListener(this);

	try {
            ai = getPackageManager().getActivityInfo(getIntent().getComponent(), PackageManager.GET_META_DATA);
            if (ai.metaData != null) {
                String ln = ai.metaData.getString(META_DATA_LIB_NAME);
                if (ln != null) libname = ln;
            }
        } catch (PackageManager.NameNotFoundException e) {
            throw new RuntimeException("Error getting activity info", e);
        }
	System.loadLibrary(libname);
	super.onCreate(savedInstanceState);
    }

    @Override protected void onStart() {
	super.onStart();
	locationManager=(LocationManager)getSystemService(Context.LOCATION_SERVICE);
	sensorManager=(SensorManager)getSystemService(Context.SENSOR_SERVICE);
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
	startForth();
    }
   
    @Override
    public boolean dispatchKeyEvent (KeyEvent event) {
	onEventNative(0, event);
	return true;
    }
    @Override
    public boolean onTouchEvent(MotionEvent event) {
	onEventNative(1, event);
	return true;
    }

    @Override
    public void onVideoSizeChanged(MediaPlayer mp, int width, int height) {
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
	onEventNative(7, holder.getSurface());
    }

    // global layout
    public void onGlobalLayout() {
	onEventNative(8, 0);
    }
}
