void *cacheflush(void * address, int size, int linewidth)
{
	int i;

	address=(void *)((int)address & (-linewidth));

	for(i=4-linewidth; i<size; i+=linewidth)
		asm("\
		fdc (%r28)\n\
		sync\n\
		fic,m %r24(%r28)\n\
		sync");

	return address;
}
