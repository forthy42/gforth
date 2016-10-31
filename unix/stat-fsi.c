#include "../engine/config.h"
#include <stdio.h>
#include <unistd.h>
#include <stddef.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/time.h>

int main()
{
  printf("begin-structure file-stat\n");
  printf("  drop %lu %lu +field st_dev\n", offsetof(struct stat, st_dev), sizeof(dev_t));
  printf("  drop %lu %lu +field st_ino\n", offsetof(struct stat, st_ino), sizeof(ino_t));
  printf("  drop %lu %lu +field st_mode\n", offsetof(struct stat, st_mode), sizeof(mode_t));
  printf("  drop %lu %lu +field st_uid\n", offsetof(struct stat, st_uid), sizeof(uid_t));
  printf("  drop %lu %lu +field st_gid\n", offsetof(struct stat, st_gid), sizeof(gid_t));
  printf("  drop %lu %lu +field st_rdev\n", offsetof(struct stat, st_rdev), sizeof(dev_t));
  printf("  drop %lu %lu +field st_size\n", offsetof(struct stat, st_size), sizeof(off_t));
  printf("  drop %lu %lu +field st_blksize\n", offsetof(struct stat, st_blksize), sizeof(blksize_t));
  printf("  drop %lu %lu +field st_blocks\n", offsetof(struct stat, st_blocks), sizeof(blkcnt_t));
  printf("  drop %lu %lu +field st_atime\n", offsetof(struct stat, st_atime), sizeof(struct timespec));
  printf("  drop %lu %lu +field st_mtime\n", offsetof(struct stat, st_mtime), sizeof(struct timespec));
  printf("  drop %lu %lu +field st_ctime\n", offsetof(struct stat, st_ctime), sizeof(
struct timespec));
  printf("drop %lu end-structure\n", sizeof(struct stat));
  return 0;
}
