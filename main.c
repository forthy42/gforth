/*
  $Id: main.c,v 1.1 1994-02-11 16:30:46 anton Exp $
  Copyright 1993 by the ANSI figForth Development Group
*/

#include <ctype.h>
#include <stdio.h>
#include <string.h>
#include <math.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <assert.h>
#include <stdlib.h>
#include "forth.h"

#ifdef DIRECT_THREADED
#define CA(n)	(symbols[(n)])
#else
#define CA(n)	((int)(symbols+(n)))
#endif

/* image file format:
 *   size of image with stacks without tags (in bytes)
 *   size of image without stacks and tags (in bytes)
 *   size of data and FP stack (in bytes)
 *   pointer to start of code
 *   data (size in image[1])
 *   tags (1 bit/data cell)
 *
 * tag==1 mean that the corresponding word is an address;
 * If the word is >=0, the address is within the image;
 * addresses within the image are given relative to the start of the image.
 * If the word is =-1, the address is NIL,
 * If the word is between -2 and -4, it's a CFA (:, Create, Constant)
 * If the word is -5, it's a DOES> CFA
 * If the word is <-5, it's a primitive
 */

void relocate(int *image, char *bitstring, int size, Label symbols[])
{
	int i;
	static char bits[8]={0x80,0x40,0x20,0x10,0x08,0x04,0x02,0x01};
	Label DODOES=symbols[3];

	for(i=0;i<size/sizeof(Cell);i++)
		if(bitstring[i >> 3] & bits[i & 7])
			if(image[i]<0)
				if(image[i]==-1)
					image[i]=0;
				else if(image[i]>-5)
					MAKE_CF(image+i,symbols[-image[i]-2]);
				else if(image[i]==-5)
				{
					MAKE_DOES_CF(image+i,image[i+1]+((int)image));
					i++; /* is this necessary? */
				}
				else
					image[i]=(Cell)CA(-image[i]-2);
			else
				image[i]+=(Cell)image;
}

int* loader(const char* filename)
{	int header[2];
	FILE *imagefile;
	int *image;

	if(!(int)(imagefile=fopen(filename,"rb")))
	{
		fprintf(stderr,"Can't open image file '%s'",filename);
		exit(1);
	}

	fread(header,1,2*sizeof(int),imagefile);

	image=malloc(header[0]+((header[0]-1)/sizeof(Cell))/8+1);

	memset(image,0,header[0]+((header[0]-1)/sizeof(Cell))/8+1);

	image[0]=header[0];
	image[1]=header[1];

	fread(image+2,1,header[1]-2*sizeof(Cell),imagefile);
	fread(((void *)image)+header[0],1,((header[1]-1)/sizeof(Cell))/8+1,
	      imagefile);
	fclose(imagefile);

	relocate(image,(char *)image+header[0],header[1],engine(0,0,0,0));

	return(image);
}

int go_forth(int *image, int stack, Cell *entries)
{
	Cell* rp=(Cell*)((void *)image+image[0]);
	double* fp=(double*)((void *)rp-image[2]);
	Cell* sp=(Cell*)((void *)fp-image[2]);
	Cell* ip=(Cell*)(image[3]);

	for(;stack>0;stack--)
		*--sp=entries[stack-1];

	install_signal_handlers(); /* right place? */

	return((int)engine(ip,sp,rp,fp));
}

int main(int argc, char **argv, char **env)
{
	char imagefile[256];
	Cell environ[3] = {(Cell)argc, (Cell)argv, (Cell)env};
	char* imagepath;

	if((int)(imagepath=getenv("FORTHBIN")))
	{
		strcpy(imagefile,imagepath);

		if(imagefile[strlen(imagefile)-1]!='/')
			imagefile[strlen(imagefile)]='/';
	}
	else
		imagefile[0]='\0';

	if(argc>1 && argv[1][0]=='@')
	{
		if(argv[1][1]=='/')
			strcpy(imagefile,argv[1]+1);
		else
			strcpy(imagefile+strlen(imagefile),argv[1]+1);

		environ[0]-=1;
		environ[1]+=sizeof(argv);
		argv[1]=argv[0];
	}
	else
		strcpy(imagefile+strlen(imagefile),"kernal.fi");

	exit(go_forth(loader(imagefile),3,environ));
}
