/* Android activity for Gforth on Android

  Copyright (C) 2015 Free Software Foundation, Inc.

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

import android.view.KeyEvent;
import android.view.SurfaceView;
import android.view.inputmethod.BaseInputConnection;
import android.view.inputmethod.CompletionInfo;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputConnection;
import android.text.Editable;
import android.text.InputType;
import android.text.SpannableStringBuilder;
import android.util.Log;
import android.util.AttributeSet;
import gnu.gforth.Gforth;

public class GforthView extends SurfaceView {
    Gforth mActivity;
    EditorInfo moutAttrs;
    MyInputConnection mInputConnection;
    private static final String TAG = "GforthView";
    
    static class MyInputConnection extends BaseInputConnection {
	private SpannableStringBuilder mEditable;
	private GforthView mView;
	
	public MyInputConnection(GforthView targetView, boolean fullEditor) {
	    super(targetView, fullEditor);
	    mView = targetView;
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

    private init(Gforth context) {
	mActivity=context;
	setFocusable(true);
	setFocusableInTouchMode(true);
    }

    public GforthView(Gforth context) {
	super(context);
	init(context);
    }

    public GforthView(Gforth context, AttributeSet attrs) {
	super(context, attrs);
	init(context);
    }

    public GforthView(Gforth context, AttributeSet attrs, int defStyle) {
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
}

