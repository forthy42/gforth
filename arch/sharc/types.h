/* types needed for a standalone system */

typedef Cell time_t;
typedef Cell *FILE;

#define stdin  ((FILE)0L)
#define stdout ((FILE)1L)
#define stderr ((FILE)2L)

#define O_RDONLY 0
#define O_RDWR   1
#define O_WRONLY 2
