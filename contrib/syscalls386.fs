\ syscalls386.fs
\
\ Selected system calls for gforth 0.6.x on 386 machines running Linux.
\   
\ Copyright (c) 2004 Krishna Myneni,
\ Provided under the GNU General Public License
\
\ Notes:
\
\ 1)   System calls under Linux may be performed using a software
\        interrupt, $80, and placing the parameters in appropriate
\        registers. The corresponding C wrapper functions are another
\        way to do this, but the $80 int method is direct. The Forth
\        stack parameters are chosen to correspond to the C wrapper 
\        function argument list.
\ 
\ 2)   There are about 221 system calls under Linux, but this file
\        provides only a select few. The provided syscalls allow 
\        communication with device drivers, e.g. serial port drivers. 
\        Add others as needed following the examples below and using 
\        the man pages for the C wrapper functions. System call numbers 
\        are listed in /usr/include/asm/unistd.h 
\
\ 3)   Compatibility with low-level kForth words is maintained to allow 
\        kForth code to be used under gforth, e.g. serial.fs, terminal.fs.
\        Other driver interface examples from kForth should also
\        work, e.g. the National Instruments GPIB interface nigpib.4th.
\
\ 4)   The code should be readily adaptable to other Forths running
\        on the same platform (386/Linux). It also demonstrates why
\        an assembler can be an important component of a Forth system.
\
\ Revisions:
\ 	2004-09-16  created  KM 

\ syscall0  ( syscall_num -- retval | system call with no args )
 
code syscall0
	.d  di )  ax   mov
	.d  $80 #      int
	.d  ax    di ) mov
	next
end-code

\ syscall1  ( arg syscall_num -- retval | system call with one arg )

code syscall1
	.d  di )  ax   mov
	.d   4 #  di   add
	.d  di )  bx   mov
	.d  $80 #      int
	.d  ax    di ) mov
	next
end-code

\ syscall2  ( arg1 arg2 syscall_num -- retval | system call with 2 args )

code syscall2
	.d  di )  ax   mov
	.d   4 #  di   add
	.d  di )  cx   mov
	.d   4 #  di   add
	.d  di )  bx   mov
	.d  $80 #      int
	.d  ax    di ) mov
	next
end-code

\ syscall3  ( arg1 arg2 arg3 syscall_num -- retval | system call with 3 args )

code syscall3
	.d  di )  ax   mov
	.d   4 #  di   add
	.d  di )  dx   mov
	.d   4 #  di   add
	.d  di )  cx   mov
	.d   4 #  di   add
	.d  di )  bx   mov
	.d  $80 #      int
	.d  ax    di ) mov
	next
end-code



\ sysexit ( code --  | exit to system with code )
\   sysexit is NOT the recommended way to exit back to the 
\   system from Forth. It is provided here as a demo of a very 
\   simple syscall.

: sysexit  1 syscall1 ;

: getpid ( -- u | get process id )
	20 syscall0 ;

: open ( ^zaddr  flags -- fd | file descriptor is returned)
\   Note zaddr points to a buffer containing the counted filename
\   string terminated with a null character.
        swap 1+ swap
	0 	\ set mode to zero  
	5 syscall3 ;

: close ( fd -- flag )  6 syscall1 ;


: read ( fd  buf  count --  n | read count byes into buf from file )
	3 syscall3 ;


: write ( fd  buf  count  --  n  | write count bytes from buf to file )
	4 syscall3 ;


: lseek ( fd  offset  type  --  offs  | reposition the file ptr )
	19 syscall3 ;

: ioctl ( fd  request argp -- error )
        54 syscall3 ;


