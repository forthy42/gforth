/* uncompress using zlib

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
  along with this program; if not, see http://www.gnu.org/licenses/. */

#include <stdio.h>
#include <zlib.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <unistd.h>
#ifdef __ANDROID__
#include <android/log.h>

#define LOGI(...) \
  __android_log_print(ANDROID_LOG_INFO, "Gforth", __VA_ARGS__);
#define LOGE(...) \
  __android_log_print(ANDROID_LOG_ERROR, "Gforth", __VA_ARGS__);
#else
#define LOGI(...) fprintf(stderr, __VA_ARGS__);
#define LOGE(...) fprintf(stderr, __VA_ARGS__);
#endif

void zexpand(char * zfile)
{
  int32_t sizebuf, filesize;
  gzFile file=gzopen(zfile, "rb");
  FILE* out;

  while(gzread(file, &sizebuf, sizeof(int32_t))==sizeof(int32_t)) {
    char filename[sizebuf];
    int len1=gzread(file, filename, sizebuf);
    int len2=gzread(file, &filesize, sizeof(int32_t));
    // LOGI("File %c: %s size %d\n", filename[0], filename+1, filesize);

    if((len1==sizebuf) && (len2==sizeof(int32_t))) {
      char filebuf[filesize];
      int len3=(filesize==0) ? 0 : gzread(file, filebuf, filesize);
      
      if((len3==filesize)) {
	switch(filename[0]) {
	case 'f': // file
	  LOGI("file %s, size %d\n", filename+1, filesize);
	  out=fopen(filename+1, "w+");
	  fwrite(filebuf, filesize, 1, out);
	  fclose(out);
	  break;
	case 'd': // directory
	  LOGI("dir %s\n", filename+1);
	  mkdir(filename+1, S_IRWXU | S_IRWXG | S_IROTH | S_IXOTH);
	  break;
	case 'h': // hard link
	  LOGI("hardlink %s\n", filename+1);
	  link(filebuf, filename+1);
	  break;
	case 's': // symlink
	  LOGI("symlink %s\n", filename+1);
	  symlink(filebuf, filename+1);
	  break;
	}
      }
    }
  }
  gzclose(file);
}

#ifdef TEST
int main(int argc, char** argv, char** env)
{
  zexpand(argv[1]);
  return 0;
}
#endif
