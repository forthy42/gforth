/* uncompress using zlib

  Authors: Bernd Paysan, Anton Ertl
  Copyright (C) 2012,2013,2014,2015,2017,2018,2019 Free Software Foundation, Inc.

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

#include "config.h"
#include <stdio.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <unistd.h>
#include <errno.h>
#include <string.h>
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

#ifdef USE_BROTLI
#include <brotli/decode.h>

struct brFile {
  BrotliDecoderState* s;
  FILE *file;
  BrotliDecoderResult result;
} brFile;

brFile* bropen(const char* file, const char* mode) {
  BROTLI_BOOL is_ok = BROTLI_TRUE;
  brFile* br = malloc(sizeof(brFile));
  if (!br) {
    LOGE("out of memory\n");
    return 0;
  }
  br->s = BrotliDecoderCreateInstance(NULL, NULL, NULL);
  if (!br->s) {
    LOGE("out of memory\n");
    free(br);
    return 0;
  }
  /* This allows decoding "large-window" streams. Though it creates
     fragmentation (new builds decode streams that old builds don't),
     it is better from used experience perspective. */
  BrotliDecoderSetParameter(br->s, BROTLI_DECODER_PARAM_LARGE_WINDOW, 1u);
  br->result = BROTLI_DECODER_RESULT_NEEDS_MORE_INPUT;
  br->file = fopen(file, mode);
  (if !br->file) {
    LOGE("can't open file\n");
    BrotliDecoderDestroyInstance(br->s);
    free(br);
    return 0;
  }
  return br;
}

static const size_t kFileBufferSize = 1 << 19;

int brread(brFile *br, char *buf, int size) {
  for (;;) {
    if (br->result == BROTLI_DECODER_RESULT_NEEDS_MORE_INPUT) {
      if (!HasMoreInput(context)) {
        fprintf(stderr, "corrupt input [%s]\n",
                PrintablePath(context->current_input_path));
        return BROTLI_FALSE;
      }
      if (!ProvideInput(context)) return BROTLI_FALSE;
    } else if (result == BROTLI_DECODER_RESULT_NEEDS_MORE_OUTPUT) {
      if (!ProvideOutput(context)) return BROTLI_FALSE;
    } else if (result == BROTLI_DECODER_RESULT_SUCCESS) {
      if (!FlushOutput(context)) return BROTLI_FALSE;
      int has_more_input =
          (context->available_in != 0) || (fgetc(context->fin) != EOF);
      if (has_more_input) {
        fprintf(stderr, "corrupt input [%s]\n",
                PrintablePath(context->current_input_path));
        return BROTLI_FALSE;
      }
      if (context->verbosity > 0) {
        fprintf(stderr, "Decompressed ");
        PrintFileProcessingProgress(context);
        fprintf(stderr, "\n");
      }
      return BROTLI_TRUE;
    } else {
      fprintf(stderr, "corrupt input [%s]\n",
              PrintablePath(context->current_input_path));
      return BROTLI_FALSE;
    }

    result = BrotliDecoderDecompressStream(s, &context->available_in,
        &context->next_in, &context->available_out, &context->next_out, 0);
  }
}

void brclose(brFile* br) {
  BrotliDecoderDestroyInstance(br->s);
  fclose(br->file);
  free(br);
}

#define zread(file, buf, size) brread(file, buf, size)
#define zopen(name, mode) bropen(name, mode)
#define zclose(file) brclose(file)
#define zFile brFile
#else
#include <zlib.h>
#define zread(file, buf, size) gzread(file, buf, size)
#define zopen(name, mode) gzopen(name, mode)
#define zclose(file) gzclose(file)
#define zFile gzFile
#endif

void zexpand(char * zfile)
{
  int32_t sizebuf, filesize;
  zFile file=zopen(zfile, "rb");
  FILE* out;

  while(zread(file, &sizebuf, sizeof(int32_t))==sizeof(int32_t)) {
    char filename[sizebuf];
    int len1=zread(file, filename, sizebuf);
    int len2=zread(file, &filesize, sizeof(int32_t));
    // LOGI("File %c: %s size %d\n", filename[0], filename+1, filesize);

    if((len1==sizebuf) && (len2==sizeof(int32_t))) {
      char *filebuf=malloc(filesize);
      int len3=(filesize==0) ? 0 : zread(file, filebuf, filesize);
      
      if((len3==filesize)) {
	switch(filename[0]) {
	case 'f': // file
	  LOGI("file %s, size %d\n", filename+1, filesize);
	  if(NULL==(out=fopen(filename+1, "w+"))) {
	    LOGE("fopen error on file %s: %s\n", filename+1, strerror(errno));
	  } else {
	    fwrite(filebuf, filesize, 1, out);
	    if(ferror(out)) {
	      LOGE("write error on file %s: %s\n", filename+1, strerror(errno));
	    }
	    fclose(out);
	  }
	  break;
	case 'd': // directory
	  LOGI("dir %s\n", filename+1);
	  if(mkdir(filename+1, S_IRWXU | S_IRWXG | S_IROTH | S_IXOTH)) {
	    if(errno != EEXIST)
	      LOGE("mkdir(%s) failed: %s\n", filename+1, strerror(errno));
	  }
	  break;
	case 'h': // hard link
	  LOGI("hardlink %s\n", filename+1);
	  if(link(filebuf, filename+1)) {
	    LOGE("link(%s) failed: %s\n", filename+1, strerror(errno));
	  }
	  break;
	case 's': // symlink
	  LOGI("symlink %s\n", filename+1);
	  if(symlink(filebuf, filename+1)) {
	    LOGE("symlink(%s) failed: %s\n", filename+1, strerror(errno));
	  }
	  break;
	}
      }
      free(filebuf);
    }
  }
  zclose(file);
}

#ifdef TEST
int main(int argc, char** argv, char** env)
{
  zexpand(argv[1]);
  return 0;
}
#endif
