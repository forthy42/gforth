void cacheflush(void * address, int size, int linewidth)
{
  int i;

  address=(void *)((int)address & (-linewidth));

  for(i=1-linewidth; i<size; i+=linewidth)
    asm volatile("fdc (%0)\n\t"
		 "sync\n\t"
		 "fic,m %1(%0)\n\t"
		 "sync" : : "r" (address), "r" (linewidth) : "memory" );
}
