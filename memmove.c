char *memmove(char *dest, const char *src, long n)
{
  int i;

  if (dest<src)
    for (i=0; i<n; i++)
      dest[i]=src[i];
  else
    for(i=n-1; i>=0; i--)
      dest[i]=src[i];
  return dest;
}
