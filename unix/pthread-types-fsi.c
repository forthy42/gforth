#define _XOPEN_SOURCE 500
#include <pthread.h>
#include <limits.h>
#include <sys/mman.h>
#include <unistd.h>
#include <setjmp.h>
#include <stdio.h>
#include <signal.h>
#ifndef FIONREAD
#include <sys/socket.h>
#endif

int main()
{
  printf("\\ struct pthread_t\n");
  printf( "begin-structure pthread_t\n" );
  printf( "drop %zu end-structure\n", sizeof( pthread_t ) );
  printf("\\ struct pthread_mutex_t\n");
  printf( "begin-structure pthread_mutex_t\n" );
  printf( "drop %zu end-structure\n", sizeof( pthread_mutex_t ) );
  printf("\\ struct pthread_cond_t\n");
  printf( "begin-structure pthread_cond_t\n" );
  printf( "drop %zu end-structure\n", sizeof( pthread_cond_t ) );
  return 0;
}
