/* uncompress using zlib */

#include <stdio.h>
#include <zlib.h>
#include <sys/stat.h>
#include <sys/types.h>

#define align4(x) (((x)+3)&-4)

void zexpand(char * zfile);
{
  int32_t sizebuf, filesize;
  gzFile file=gzopen(zfile, "rb");
  FILE* out;

  while(gzread(file, &sizebuf, 4)==4) {
    char filename[align4(sizebuf)];
    int len1=gzread(file, filename, align4(sizebuf));
    int len2=gzread(file, &filesize, 4);
    // fprintf(stderr, "File %c: %s size %d\n", filename[0], filename+1, filesize);

    if((len1==align4(sizebuf)) && (len2==4)) {
      char filebuf[align4(filesize)];
      int len3=(filesize==0) ? 0 : gzread(file, filebuf, align4(filesize));
      
      if((len3==align4(filesize))) {
	switch(filename[0]) {
	case 'f':
	  out=fopen(filename+1, "w+");
	  fwrite(filebuf, filesize, 1, out);
	  fclose(out);
	  break;
	case 'd':
	  mkdir(filename+1, S_IRWXU | S_IRWXG | S_IROTH | S_IXOTH);
	  break;
	} 
      }
    }
  }
}

#ifdef TEST
int main(int argc, char** argv, char** env)
{
  zexpand(argv[1]);
}
#endif
