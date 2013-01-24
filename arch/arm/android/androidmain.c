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

const char sha256sum[]="sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha2";

int checksha256sum(void)
{
  int checkdir;
  char sha256buffer[64];
  int checkread;

  checkdir=open("/sdcard/gforth/" PACKAGE_VERSION, O_RDONLY);
  if(checkdir==-1) return 0; // directory not there
  close(checkdir);
  checkdir=open("/sdcard/gforth/" PACKAGE_VERSION "/sha256sum", O_RDONLY);
  if(checkdir==-1) return 0; // sha256sum not there
  checkread=read(checkdir, sha256buffer, 64);
  close(checkdir);
  if(checkread!=64) return 0;
  if(memcmp(sha256buffer, sha256sum, 64)) return 0;
  return 1;
}

void android_main(struct android_app* state)
{
  char statepointer[2*sizeof(char*)+3]; // 0x+hex digits+trailing 0
  char *argv[] = { "gforth", "starta.fs" };
  const int argc = sizeof(argv)/sizeof(char*);
  int retvalue;
  int checkdir;
  int epipe[2];

  if((checkdir=open("/sdcard/gforth/", O_RDONLY))==-1) {
    mkdir("/sdcard/gforth/", S_IRWXU | S_IRWXG | S_IROTH | S_IXOTH);
  } else { close(checkdir); }
  if((checkdir=open("/sdcard/gforth/home", O_RDONLY))==-1) {
    mkdir("/sdcard/gforth/home", S_IRWXU | S_IRWXG | S_IROTH | S_IXOTH);
  } else { close(checkdir); }

  freopen("/sdcard/gforth/home/aout.log", "w+", stdout);
  freopen("/sdcard/gforth/home/aerr.log", "w+", stderr);
  pipe(epipe);
  fileno(stdin)=epipe[0];

  if(!checksha256sum()) {
    chdir("/sdcard");
    zexpand("/data/data/gnu.gforth/lib/libgforthgz.so");
    checkdir=creat("/sdcard/gforth/" PACKAGE_VERSION "/sha256sum", O_WRONLY);
    write(checkdir, sha256sum, 64);
    close(checkdir);
  }

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
  checkdir=open("/sdcard/gforth/" PACKAGE_VERSION "/gforth.fi", O_RDONLY);
  if(checkdir==-1) {
    chdir("/sdcard/gforth/" PACKAGE_VERSION);
    retvalue=gforth_make_image(0);
    exit(retvalue);
  } else {
    close(checkdir);
  }
#endif

  chdir("/sdcard/gforth/home");

  retvalue=gforth_start(argc, argv);
  
  if(retvalue == -56) {
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
