int memcmp(const char *s1, const char *s2, long n)
{
  int i;

  for (i=0; i<n; i++)
    if (s1[i] != s2[i])
      return s1[i]-s2[i];
  return 0;
}
