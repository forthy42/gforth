\ syscalls386.fs
\
\ Selected Unix system calls for gforth >=0.7.x

\ Replacement for the original non-portable syscalls386 implementation
\ this one ports to other Gforth installations, other architectures,
\ and other Unix implementations.

\ The following original note is probably still applicable.

\ 3)   Compatibility with low-level kForth words is maintained to allow 
\        kForth code to be used under gforth, e.g. serial.fs, terminal.fs.
\        Other driver interface examples from kForth should also
\        work, e.g. the National Instruments GPIB interface nigpib.4th.

c-library syscalls386

\c #include <sys/types.h>
\c #include <sys/stat.h>
\c #include <fcntl.h>
\c #include <unistd.h>
\c #include <sys/ioctl.h>

\ sysexit ( code --  | exit to system with code )
\   sysexit is NOT the recommended way to exit back to the 
\   system from Forth. It is provided here as a demo of a very 
\   simple syscall.
c-function sysexit _exit n -- void
c-function getpid getpid -- n  ( -- u | get process id )
c-function open open a n -- n  ( ^zaddr  flags -- fd )
\   file descriptor is returned)
\   Note zaddr points to a buffer containing the counted filename
\   string terminated with a null character.
c-function close close n -- n ( fd -- flag )
c-function read read n a n -- n ( fd  buf  count --  n )
\ read count byes into buf from file
c-function write write n a n -- n ( fd  buf  count  --  n )
\ write count bytes from buf to file
c-function lseek lseek n n n -- n ( fd  offset  type  --  offs )
\ reposition the file ptr
c-function ioctl ioctl n n a -- n ( fd  request argp -- error )

end-c-library

