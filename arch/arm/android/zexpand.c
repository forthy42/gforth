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
	  out=fopen(filename+1, "w+");
	  fwrite(filebuf, filesize, 1, out);
	  fclose(out);
	  break;
	case 'd': // directory
	  mkdir(filename+1, S_IRWXU | S_IRWXG | S_IROTH | S_IXOTH);
	  break;
	case 'h': // hard link
	  link(filebuf, filename);
	  break;
	case 's': // symlink
	  symlink(filebuf, filename);
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
