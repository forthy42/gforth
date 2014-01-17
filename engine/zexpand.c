/* uncompress using zlib */

#include <stdio.h>
#include <zlib.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <unistd.h>

void zexpand(char * zfile)
{
  int32_t sizebuf, filesize;
  gzFile file=gzopen(zfile, "rb");
  FILE* out;

  while(gzread(file, &sizebuf, sizeof(int32_t))==sizeof(int32_t)) {
    char filename[sizebuf];
    int len1=gzread(file, filename, sizebuf);
    int len2=gzread(file, &filesize, sizeof(int32_t));
    // fprintf(stderr, "File %c: %s size %d\n", filename[0], filename+1, filesize);

    if((len1==sizebuf) && (len2==sizeof(int32_t))) {
      char filebuf[filesize];
      int len3=(filesize==0) ? 0 : gzread(file, filebuf, filesize);
      
      if((len3==filesize)) {
	switch(filename[0]) {
	case 'f': // file
	  fprintf(stderr, "file %s, size %d\n", filename+1, filesize);
	  out=fopen(filename+1, "w+");
	  fwrite(filebuf, filesize, 1, out);
	  fclose(out);
	  break;
	case 'd': // directory
	  fprintf(stderr, "dir %s\n", filename+1);
	  mkdir(filename+1, S_IRWXU | S_IRWXG | S_IROTH | S_IXOTH);
	  break;
	case 'h': // hard link
	  fprintf(stderr, "hardlink %s\n", filename+1);
	  link(filebuf, filename+1);
	  break;
	case 's': // symlink
	  fprintf(stderr, "symlink %s\n", filename+1);
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
