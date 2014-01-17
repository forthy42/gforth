/* Android main() for Gforth on Android

  Copyright (C) 2012,2013 Free Software Foundation, Inc.

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
#include <pthread.h>

#include "forth.h"

#include <jni.h>
#include <android/log.h>

static Xt ainput=0;
static Xt acmd=0;
static Xt akey=0;

typedef struct { int type; jobject event; } sendEvent;
typedef struct { int type; int event; } sendInt;

typedef struct {
  JavaVM * vm;
  JNIEnv * env;
  jobject obj;
  jclass cls;
  pthread_t id;
  int ke_fd[2];
} jniargs;

jniargs startargs;

JNIEXPORT void JNI_onEventNative(JNIEnv * env, jobject obj, jint type, jobject event)
{
  sendEvent ke = { type, (*env)->NewGlobalRef(env, event) };
  if(startargs.ke_fd[1])
    write(startargs.ke_fd[1], &ke, sizeof(ke));
}

JNIEXPORT void JNI_onEventNativeInt(JNIEnv * env, jobject obj, jint type, jint event)
{
  sendInt ke = { type, event };
  if(startargs.ke_fd[1])
    write(startargs.ke_fd[1], &ke, sizeof(ke));
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

void startForth(jniargs * startargs)
{
  char statepointer[2*sizeof(char*)+3]; // 0x+hex digits+trailing 0
  char * argv[] = { "gforth", "--", "starta.fs" };
  const int argc=3;
  static const char *folder[] = { "/sdcard", "/mnt/sdcard", "/data/data/gnu.gforth/files" };
  static const char *paths[] = { "--",
				 "--path=/mnt/sdcard/gforth/" PACKAGE_VERSION ":/mnt/sdcard/gforth/site-forth",
				 "--path=/data/data/gnu.gforth/files/gforth/" PACKAGE_VERSION ":/data/data/gnu.gforth/files/gforth/site-forth" };
  int retvalue, checkdir, i;
  int epipe[2];

  for(i=0; i<=2; i++) {
    argv[1]=paths[i];
    if(!chdir(folder[i])) break;
  }

  fprintf(stderr, "chdir(%s)\n", folder[i]);

  fprintf(stderr, "Starting %s %s %s\n",
	  argv[0], argv[1], argv[2]);

  pipe(epipe);
  pipe(startargs->ke_fd);
  fileno(stdin)=epipe[0];

  if(!checksha256sum()) {
    zexpand("/data/data/gnu.gforth/lib/libgforthgz.so");
    checkdir=creat("gforth/" PACKAGE_VERSION "/sha256sum", O_WRONLY);
    write(checkdir, sha256sum, 64);
    close(checkdir);
  }

  snprintf(statepointer, sizeof(statepointer), "%p", startargs);
  setenv("HOME", "/sdcard/gforth/home", 1);
  setenv("SHELL", "/system/bin/sh", 1);
  setenv("libccdir", "/data/data/gnu.gforth/lib", 1);
  setenv("LANG", "en_US.UTF-8", 1);
  setenv("APP_STATE", statepointer, 1);
  
  chdir("gforth/home");

  fflush(stderr);
  retvalue=gforth_start(argc, argv);

  ainput=gforth_find("ainput");
  acmd=gforth_find("acmd");
  akey=gforth_find("akey");
  
  if(retvalue == -56) {
    Xt bootmessage=gforth_find((Char*)"bootmessage");
    if(bootmessage != 0)
      gforth_execute(bootmessage);
    retvalue = gforth_quit();
  }
  exit(retvalue);
}

pthread_attr_t * pthread_detach_attr(void)
{
  static pthread_attr_t attr;
  pthread_attr_init(&attr);
  pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED);
  return &attr;
}

void JNI_startForth(JNIEnv * env, jobject obj)
{
  startargs.env = env;
  startargs.obj = obj;

  pthread_create(&(startargs.id), pthread_detach_attr(), startForth, (void*)&startargs);
}

void JNI_callForth(JNIEnv * env, jint xt)
{
  gforth_execute((Xt)xt);
}

static JNINativeMethod GforthMethods[] = {
  {"onEventNative", "(ILjava/lang/Object;)V", (void*) JNI_onEventNative},
  {"onEventNative", "(II)V",                  (void*) JNI_onEventNativeInt},
  {"callForth",     "(I)V",                   (void*) JNI_callForth},
  {"startForth",    "()V",                    (void*) JNI_startForth},
};

#define alen(array)  sizeof(array)/sizeof(array[0])

JNIEXPORT jint JNI_OnLoad(JavaVM* vm, void* reserved)
{
  int i, n=alen(GforthMethods);
  jmethodID jid;
  jclass cls;
  JNIEnv * env;

  freopen("/sdcard/gforthout.log", "w+", stdout);
  freopen("/sdcard/gfortherr.log", "w+", stderr);

  startargs.vm = vm;

  if((*vm)->GetEnv(vm, (void**)&env, JNI_VERSION_1_6) != JNI_OK)
    return -1;

  cls = (*env)->FindClass(env, "gnu/gforth/Gforth");
  startargs.cls = cls;

  fprintf(stderr, "Registering native methods\n");

  for(i=0; i<n; i++) {
    jid=(*env)->GetMethodID(env, cls, GforthMethods[i].name, GforthMethods[i].signature);
    if(jid==0) fprintf(stderr, "Can't find method %s %s\n",
		       GforthMethods[i].name, GforthMethods[i].signature);
  }

  if((*env)->RegisterNatives(env, cls, GforthMethods, n)<0) {
    fprintf(stderr, "Register Natives failed\n");
    fflush(stderr);
  }

  return JNI_VERSION_1_6;
}
