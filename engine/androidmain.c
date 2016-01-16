/* Android main() for Gforth on Android

  Copyright (C) 2012,2013,2014,2015 Free Software Foundation, Inc.

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

#define LOGI(...) \
  __android_log_print(ANDROID_LOG_INFO, "Gforth", __VA_ARGS__);
#define LOGE(...) \
  __android_log_print(ANDROID_LOG_ERROR, "Gforth", __VA_ARGS__);

// static Xt ainput=0;
// static Xt acmd=0;
// static Xt akey=0;

typedef struct { long type; jobject event; } sendEvent;
typedef struct { long type; long event; } sendInt;

typedef struct {
  JavaVM * vm;
  JNIEnv * env;
  jobject obj;
  jclass cls;
  pthread_t id;
  int ke_fd[2];
  void* win;
  char* libdir;
  char* locale;
  char* startfile;
} jniargs;

jniargs startargs;

JNIEXPORT void JNI_onEventNative(JNIEnv * env, jobject obj, jint type, jobject event)
{
  sendEvent ke = { type, (*env)->NewGlobalRef(env, event) };
  if(startargs.ke_fd[1])
    write(startargs.ke_fd[1], &ke, sizeof(ke));
  else
    LOGE("pipe not opened\n");
}

JNIEXPORT void JNI_onEventNativeInt(JNIEnv * env, jobject obj, jint type, jint event)
{
  sendInt ke = { type, event };
  if(startargs.ke_fd[1])
    write(startargs.ke_fd[1], &ke, sizeof(ke));
  else
    LOGE("pipe not opened\n");
}

const char sha256sum[]="sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha2";
const char sha256arch[]="sha256archsum-sha256archsum-sha256archsum-sha256archsum-sha256ar";

int checksha256sum(char * sha256, char* sha256file)
{
  int checkdir;
  char sha256buffer[64];
  int checkread;

  checkdir=open(sha256file, O_RDONLY);
  if(checkdir==-1) {
    LOGI("cksha256: file '%s' not here\n", sha256file);
    return 0; // sha256sum not there
  }
  checkread=read(checkdir, sha256buffer, 64);
  close(checkdir);
  if(checkread!=64) {
    LOGI("cksha256: size %d wrong\n", checkread);
    return 0;
  }
  if(memcmp(sha256buffer, sha256, 64)) return 0;
  return 1;
}

void post(char * doprog)
{
  JNIEnv *env=startargs.env;
  jobject clazz=startargs.obj;
  jclass cls=startargs.cls;
  jclass handlercls=(*env)->FindClass(env, "android/os/Handler");;
  jfieldID handler, showprog;
  jmethodID post;
  
  post=(*env)->GetMethodID(env, handlercls, "post", "(Ljava/lang/Runnable;)Z");
  handler=(*env)->GetFieldID(env, cls, "handler", "Landroid/os/Handler;");
  showprog=(*env)->GetFieldID(env, cls, doprog, "Ljava/lang/Runnable;");
  if(showprog) {
    (*env)->CallBooleanMethod(env, (*env)->GetObjectField(env, clazz, handler),
			      post, (*env)->GetObjectField(env, clazz, showprog));
  }
}

void unpackFiles()
{
  int checkdir, writeout;
  int libdirlen=strlen(startargs.libdir)+strlen("/libgforthgz.so")+1;
  char libdir[libdirlen];
  post("showprog");
  snprintf(libdir, libdirlen, "%s%s", startargs.libdir, "/libgforthgz.so");
  zexpand(libdir);
  checkdir=creat("gforth/" PACKAGE_VERSION "/sha256sum", O_WRONLY);
  LOGI("sha256sum '%s'=>%d\n", "gforth/" PACKAGE_VERSION "/sha256sum", checkdir);
  writeout=write(checkdir, sha256sum, 64);
  LOGI("sha256sum: '%64s'\n", sha256sum);
  close(checkdir);
  if(writeout==64) {
    post("hideprog");
  } else {
    post("errprog");
  }
}

void unpackArchFiles()
{
  int checkdir, writeout;
  int libdirlen=strlen(startargs.libdir)+strlen("/libgforth-" ARCH "gz.so")+1;
  char libdir[libdirlen];
  post("showprog");
  snprintf(libdir, libdirlen, "%s%s", startargs.libdir, "/libgforth-" ARCH "gz.so");
  zexpand(libdir);
  checkdir=creat("gforth-" ARCH "/gforth/" PACKAGE_VERSION "/sha256sum", O_WRONLY);
  LOGI("sha256sum '%s'=>%d\n", "gforth-" ARCH "/gforth/" PACKAGE_VERSION "/sha256sum", checkdir);
  writeout=write(checkdir, sha256arch, 64);
  LOGI("sha256sum: '%64s'\n", sha256arch);
  close(checkdir);
  if(writeout==64) {
    post("hideprog");
  } else {
    post("errprog");
  }
}

char ** argv=NULL;
int argc=0;

void addarg(char* arg, size_t len)
{
  char * newarg = malloc(len+1);

  memcpy(newarg, arg, len);
  newarg[len]='\0';
  argc++;

  if(argv==NULL) {
    argv=malloc(argc*sizeof(char*));
  } else {
    argv=realloc(argv, argc*sizeof(char*));
  }
  argv[argc-1] = newarg;
}

#define ADDRLEN(x) x, strlen(x)

void addfileargs(char* filename)
{
  FILE *argfile=fopen(filename, "r");
  char *line=NULL, *arg;
  size_t n=0;

  if(argfile==NULL) return; // no file, nothing to do

  while((line=fgetln(argfile, &n))) {
    if(n > 0 && line[n-1]=='\n') n--;
    addarg(line, n);
  }
}

static const char *paths[] = { "--path=/sdcard/gforth/" PACKAGE_VERSION ":/sdcard/gforth" ARCH "/gforth/" PACKAGE_VERSION ":/sdcard/gforth/site-forth",
			       "--path=/mnt/sdcard/gforth/" PACKAGE_VERSION ":/mnt/sdcard/gforth-" ARCH "/gforth/" PACKAGE_VERSION ":/mnt/sdcard/gforth/site-forth",
			       "--path=/data/data/gnu.gforth/files/gforth/" PACKAGE_VERSION ":/data/data/gnu.gforth/files/gforth-" ARCH "/gforth/" PACKAGE_VERSION ":/data/data/gnu.gforth/files/gforth/site-forth" };
static const char *folder[] = { "/sdcard", "/mnt/sdcard", "/data/data/gnu.gforth/files" };

int checkFiles(char ** patharg)
{
  int i;

  for(i=0; i<=2; i++) {
    *patharg=paths[i];
    if(!chdir(folder[i])) break;
  }

  LOGI("chdir(%s)\n", folder[i]);
  LOGI("Extra arg: %s\n", *patharg);

  return checksha256sum(sha256sum, "gforth/" PACKAGE_VERSION "/sha256sum") &
    checksha256sum(sha256arch, "gforth-" ARCH "/gforth/" PACKAGE_VERSION "/sha256sum");
}

void startForth(jniargs * startargs)
{
  char statepointer[2*sizeof(char*)+3]; // 0x+hex digits+trailing 0
  char* patharg;
  int retvalue;
  int epipe[2];
  JavaVM *vm=startargs->vm;
  JNIEnv *env;
  JavaVMAttachArgs vmAA = { JNI_VERSION_1_6, "NativeThread", 0 };

  pipe(epipe);
  stdin->_file=epipe[0];

  (*vm)->AttachCurrentThread(vm, &env, &vmAA);
  
  startargs->env = env;

  if(!checkFiles(&patharg)) {
    unpackFiles();
    unpackArchFiles();
  }

  snprintf(statepointer, sizeof(statepointer), "%p", startargs);
  setenv("HOME", "/sdcard/gforth/home", 1);
  setenv("SHELL", "/system/bin/sh", 1);
  setenv("libccdir", startargs->libdir, 1);
  setenv("LANG", startargs->locale, 1);
  setenv("LC_ALL", startargs->locale, 1);
  setenv("APP_STATE", statepointer, 1);
  
  chdir("gforth/home");

  addarg(ADDRLEN("gforth"));
  addfileargs(".options");
  addarg(ADDRLEN(patharg));
  addarg(ADDRLEN(startargs->startfile));

  LOGI("Starting Gforth...\n");
  retvalue=gforth_start(argc, argv);
  LOGI("Started, rval=%d\n", retvalue);

  if(retvalue == -56) {
    gforth_setwinch();
    gforth_bootmessage();
    LOGI("starting gforth_quit\n");
    retvalue = gforth_quit();
  } else {
    LOGI("booting not successful...\n");
    unlink("../" PACKAGE_VERSION "/sha256sum");
  }
  post("appexit");

  exit(retvalue);
}

pthread_attr_t * pthread_detach_attr(void)
{
  static pthread_attr_t attr;
  pthread_attr_init(&attr);
  pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED);
  return &attr;
}

char *getjstring(JNIEnv * env, jstring string)
{
  char *s1, *s2;
  // Java's string lifetime is unknown, better copy the string and release it
  s1=(*env)->GetStringUTFChars(env, string, NULL);
  s2=malloc(strlen(s1)+1);
  strncpy(s2, s1, strlen(s1)+1);
  (*env)->ReleaseStringUTFChars(env, string, s1);
  return s2;
}

void JNI_startForth(JNIEnv * env, jobject obj, jstring libdir, jstring locale, jstring startfile)
{
  char *getlibdir, *getlocale, *getstartfile;
  startargs.obj = (*env)->NewGlobalRef(env, obj);
  startargs.win = 0; // is a native window
  startargs.libdir = getjstring(env, libdir);
  startargs.locale = getjstring(env, locale);
  startargs.startfile = getjstring(env, startfile);

  pthread_create(&(startargs.id), pthread_detach_attr(), startForth, (void*)&startargs);
}

#define GFSS 0x80 // 128 cells for callback stack

void JNI_callForth(JNIEnv * env, jlong xt)
{
  Cell stack[GFSS], rstack[GFSS], lstack[GFSS]; Float fstack[GFSS];
  Cell *oldsp=gforth_SP;
  Cell *oldrp=gforth_RP;
  char *oldlp=gforth_LP;
  Float *oldfp=gforth_FP;
  user_area *oldup=gforth_UP;

  gforth_SP=stack+GFSS-1;
  gforth_RP=rstack+GFSS;
  gforth_LP=(char*)(lstack+GFSS);
  gforth_FP=fstack+GFSS-1;
  gforth_UP=gforth_main_UP;

  gforth_execute((Xt)xt);

  gforth_SP=oldsp;
  gforth_RP=oldrp;
  gforth_LP=oldlp;
  gforth_FP=oldfp;
  gforth_UP=oldup;
;
}

int checkFile_flag=0;

static JNINativeMethod GforthMethods[] = {
  {"onEventNative", "(ILjava/lang/Object;)V", (void*) JNI_onEventNative},
  {"onEventNative", "(II)V",                  (void*) JNI_onEventNativeInt},
  {"callForth",     "(J)V",                   (void*) JNI_callForth},
  {"startForth",    "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V",  (void*) JNI_startForth},
};

#define alen(array)  sizeof(array)/sizeof(array[0])

JNIEXPORT jint JNI_OnLoad(JavaVM* vm, void* reserved)
{
  int i, n=alen(GforthMethods);
  jmethodID jid;
  jclass cls;
  JNIEnv * env;
  
  LOGI("Enter onload\n");
  
  freopen("/sdcard/gforthout.log", "w+", stdout);
  freopen("/sdcard/gfortherr.log", "w+", stderr);

  pipe(startargs.ke_fd);

  startargs.vm = vm;

  if((*vm)->GetEnv(vm, (void**)&env, JNI_VERSION_1_6) != JNI_OK)
    return -1;

  cls = (*env)->FindClass(env, "gnu/gforth/Gforth");
  startargs.cls = (*env)->NewGlobalRef(env, cls);

  LOGI("Registering native methods\n");

  for(i=0; i<n; i++) {
    jid=(*env)->GetMethodID(env, cls, GforthMethods[i].name, GforthMethods[i].signature);
    if(jid==0) LOGI("Can't find method %s %s\n",
		    GforthMethods[i].name, GforthMethods[i].signature);
  }

  if((*env)->RegisterNatives(env, cls, GforthMethods, n)<0) {
    LOGI("Register Natives failed\n");
    fflush(stderr);
  }

  return JNI_VERSION_1_6;
}
