/* types needed for a standalone system */

typedef Cell time_t;

#define SHARC

#ifdef __DOUBLES_ARE_FLOATS__
#define double float
#endif

struct timeval {
  Cell tv_sec;        /* seconds */
  Cell tv_usec;  /* microseconds */
};

struct timezone {
  Cell tz_minuteswest;
  /* minutes west of Greenwich */
  Cell tz_dsttime;
  /* type of dst correction */
};

struct tm
{
  Cell    tm_sec;         /* seconds */
  Cell    tm_min;         /* minutes */
  Cell    tm_hour;        /* hours */
  Cell    tm_mday;        /* day of the month */
  Cell    tm_mon;         /* month */
  Cell    tm_year;        /* year */
  Cell    tm_wday;        /* day of the week */
  Cell    tm_yday;        /* day in the year */
  Cell    tm_isdst;       /* daylight saving time */
};

struct stat {
  Cell st_inode;
  time_t st_time;
  Cell st_size;
};

#define F_OK 0
#define W_OK 2
#define R_OK 4

#include <io.h>
#include <fcntl.h>

double atanh(double r1);
double asinh(double r1);
double acosh(double r1);
char* ecvt(double x, int len, int* exp, int* sign);
char *strsignal(int sig);

#define AUTO_INCREMENT 1
#undef HAVE_RINT
#undef HAVE_ECVT
#undef HAVE_SYS_MMAN_H
#undef HAVE_MMAP
#undef HAVE_GETPAGESIZE
#undef HAVE_SYSCONF

#define PAGESIZE 0x1000

#define perror(string) fprintf(stderr, "%s: error %d\n", string, errno)
