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

static int32_t engine_handle_input(struct android_app* app, AInputEvent* event) 
{
  static Xt ainput=0;

  if(!ainput) {
    ainput=gforth_find("ainput");
  }
  if(ainput) {
    *--gforth_SP=event;
    gforth_execute(ainput);
    return 1;
  }
  fprintf(stderr, "Input event of type %d\n", AInputEvent_getType(event));
  return 0;
}

static void engine_handle_cmd(struct android_app* app, int32_t cmd)
{
  static Xt acmd=0;

  if(!acmd) {
    acmd=gforth_find("acmd");
  }
  if(acmd) {
    *--gforth_SP=cmd;
    gforth_execute(acmd);
    return 1;
  }
  fprintf(stderr, "App cmd %d\n", cmd);
}

void android_main(struct android_app* state)
{
  char statepointer[2*sizeof(char*)+3]; // 0x+hex digits+trailing 0
  char *argv[] = { "gforth", "-i", "kernl32l.fi", "exboot.fs", "startup.fs", "arch/arm/asm.fs", "arch/arm/disasm.fs", "starta.fs" };
  const int argc = sizeof(argv)/sizeof(char*);
  int retvalue;
  int checkdir;
  int epipe[2];

  freopen("/sdcard/gforth/home/aout.log", "w+", stdout);
  freopen("/sdcard/gforth/home/aerr.log", "w+", stderr);
  pipe(epipe);
  fileno(stdin)=epipe[0];

  checkdir=open("/sdcard/gforth/" PACKAGE_VERSION, O_RDONLY);
  if(checkdir==-1) {
    chdir("/sdcard");
    zexpand("/data/data/gnu.gforth/lib/libgforthgz.so");
  } else {
    close(checkdir);
  }
  chdir("/sdcard/gforth/home");

  state->onAppCmd = engine_handle_cmd;
  state->onInputEvent = engine_handle_input;

  snprintf(statepointer, sizeof(statepointer), "%p", state);
  setenv("HOME", "/sdcard/gforth/home", 1);
  setenv("SHELL", "/system/bin/sh", 1);
  setenv("libccdir", "/data/data/gnu.gforth/lib", 1);
  setenv("LANG", "en_US.UTF-8", 1);
  setenv("APP_STATE", statepointer, 1);
  
  app_dummy();

#ifdef DOUBLY_INDIRECT
  retvalue=gforth_make_image(0);
#else
  retvalue=gforth_start(argc, argv);

  if(retvalue > 0) {
    gforth_execute(gforth_find("bootmessage"));
    retvalue = gforth_quit();
  }
#endif
  exit(retvalue);
}
#else
int main(int argc, char ** argv, char ** env)
{
  return gforth_main(argc, argv, env);
}
#endif
