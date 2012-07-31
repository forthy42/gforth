/* Android main() for Gforth on Android

  Copyright (C) 2012 Free Software Foundation, Inc.

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

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h> 
#include <sys/stat.h>
#include <fcntl.h>
#include <stdarg.h>

#include "forth.h"

#ifdef __ANDROID__
#include <android/log.h>
#include "android_native_app_glue.h"

struct input_states {
  int flag;
  int count;
  int x0, y0;
  int x1, y1;
  int x2, y2;
  int x3, y3;
  int x4, y4;
};

struct input_states app_input_state;

static int32_t engine_handle_input(struct android_app* app, AInputEvent* event) 
{
  if (AInputEvent_getType(event) == AINPUT_EVENT_TYPE_MOTION) {
    app_input_state.count=AMotionEvent_getPointerCount(event);
    switch(app_input_state.count) {
    case 5:
      app_input_state.x4=AMotionEvent_getX(event, 4);
      app_input_state.y4=AMotionEvent_getY(event, 4);
    case 4:
      app_input_state.x3=AMotionEvent_getX(event, 3);
      app_input_state.y3=AMotionEvent_getY(event, 3);
    case 3:
      app_input_state.x2=AMotionEvent_getX(event, 2);
      app_input_state.y2=AMotionEvent_getY(event, 2);
    case 2:
      app_input_state.x1=AMotionEvent_getX(event, 1);
      app_input_state.y1=AMotionEvent_getY(event, 1);
    case 1:
      app_input_state.x0=AMotionEvent_getX(event, 0);
      app_input_state.y0=AMotionEvent_getY(event, 0);
    }
    app_input_state.flag = AInputEvent_getType(event);
    return 1;
  }
  // pretend we handled that, too?
  fprintf(stderr, "Input event of type %d\n", AInputEvent_getType(event));
  return 0;
}

static void engine_handle_cmd(struct android_app* app, int32_t cmd)
{
  fprintf(stderr, "App cmd %d\n", cmd);
  switch (cmd) {
  case APP_CMD_SAVE_STATE:
    fprintf(stderr, "app save\n");
    // The system has asked us to save our current state.  Do so.
    break;
  case APP_CMD_INIT_WINDOW:
    fprintf(stderr, "app window %p\n", app->window);
    // The window is being shown, get it ready.
    if (app->window != NULL) {
      // now you can do something with the window
    }
    break;
  case APP_CMD_TERM_WINDOW:
    fprintf(stderr, "app window close\n");
    // The window is being hidden or closed, clean it up.
    break;
  case APP_CMD_GAINED_FOCUS:
    fprintf(stderr, "app window focus\n");
    // When our app gains focus, we start doing something
    break;
  case APP_CMD_LOST_FOCUS:
    fprintf(stderr, "app window defocus\n");
    // When our app loses focus, we stop doing something
    break;
  case APP_CMD_DESTROY:
    fprintf(stderr, "app window destroyed\n");
    exit(0);
    break;
  }
}

void android_main(struct android_app* state)
{
  char statepointer[30];
  char *argv[] = { "gforth", "-i", "kernl32l.fi", "exboot.fs", "startup.fs", "arch/arm/asm.fs", "arch/arm/disasm.fs", "starta.fs" };
  const int argc = sizeof(argv)/sizeof(char*);
  char *env[] = { "HOME=/sdcard/gforth/home",
		  "SHELL=/system/bin/sh",
		  "libccdir=/data/data/gnu.gforth/lib",
                  statepointer,
		  NULL };
  int retvalue;
  int checkdir;

  freopen("/sdcard/gforth/home/aout.log", "w+", stdout);
  freopen("/sdcard/gforth/home/aerr.log", "w+", stderr);

  checkdir=open("/sdcard/gforth/" PACKAGE_VERSION, O_RDONLY);
  if(checkdir==-1) {
    chdir("/sdcard");
    zexpand("/data/data/gnu.gforth/lib/libgforthgz.so");
  } else {
    close(checkdir);
  }
  chdir("/sdcard/gforth/home");

  state->userData = (void*)&app_input_state;
  state->onAppCmd = engine_handle_cmd;
  state->onInputEvent = engine_handle_input;

  snprintf(statepointer, sizeof(statepointer), "APP_STATE=%p", state);
  setenv("HOME", "/sdcard/gforth/home", 1);
  setenv("SHELL", "/system/bin/sh", 1);
  setenv("libccdir", "/data/data/gnu.gforth/lib", 1);
  setenv("APP_STATE", statepointer+10, 1);
  
  app_dummy();

  retvalue=gforth_start(argc, argv);

  if(retvalue > 0) {
    gforth_execute(gforth_find("bootmessage"));
    retvalue = gforth_quit();
  }
  exit(retvalue);
}
#else
int main(int argc, char ** argv, char ** env)
{
  return gforth_main(argc, argv, env);
}
#endif
