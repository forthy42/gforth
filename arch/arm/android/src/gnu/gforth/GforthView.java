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

import android.view.SurfaceView;
import gnu.gforth.Gforth;

class GforthView extends SurfaceView {
    Gforth mActivity;
    EditorInfo moutAttrs;
    MyInputConnection mInputConnection;
    
    public GforthView(Gforth context) {
	super(context);
	mActivity=context;
	setFocusable(true);
	setFocusableInTouchMode(true);
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

