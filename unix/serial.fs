\ serial interface for Gforth under Unix

c-library serial
    \c #include <termios.h>
    \c #include <sys/ioctl.h>
    \c #include <sys/types.h>
    \c #include <sys/stat.h>
    \c #include <stdio.h>
    \c #include <unistd.h>
    \c #include <fcntl.h>

    c-function tcgetattr tcgetattr n a -- n ( fd termios -- r )
    c-function tcsetattr tcsetattr n n a -- n ( fd opt termios -- r )
    c-function cfmakeraw cfmakeraw a -- void ( termios -- )
    c-function cfsetispeed cfsetispeed a n -- n ( termios speed -- r )
    c-function cfsetospeed cfsetospeed a n -- n ( termios speed -- r )
    c-function tcflow tcflow n n -- n ( fd action -- n )
    c-function ioctl ioctl n n a -- n ( fd cmd ptr -- n )
    c-function fileno fileno a{(FILE*)} -- n ( file* -- fd )
    c-function open open a n n -- n ( path flags mode -- fd )
    c-function fdopen fdopen n a -- a ( fd mode -- file )
    c-function read read n a n -- n ( fd addr u -- u' )
    c-function write write n a n -- n ( fd addr u -- u' )
    c-function close close n -- n ( fd -- r )
end-c-library

[IFDEF] android
    : flagfield: ( offset -- offset' )
	1+ -2 and 2 +field ;
    ' w@ alias flag@
    ' w! alias flag!
[ELSE]
    : flagfield: ( offset -- offset' )
	3 + -4 and 4 +field ;
    ' l@ alias flag@
    ' l! alias flag!
[THEN]

begin-structure termios
flagfield: c_iflag           \ input mode flags
flagfield: c_oflag           \ output mode flags
flagfield: c_cflag           \ control mode flags
flagfield: c_lflag           \ local mode flags
cfield: c_line
32 +field c_cc           \ line discipline
flagfield: c_ispeed          \ input speed
flagfield: c_ospeed          \ output speed
end-structure

Create t_old  termios allot
Create t_buf  termios allot

base @ 8 base !
0000001 Constant B50   
0000002 Constant B75   
0000003 Constant B110  
0000004 Constant B134  
0000005 Constant B150  
0000006 Constant B200  
0000007 Constant B300  
0000010 Constant B600  
0000011 Constant B1200 
0000012 Constant B1800 
0000013 Constant B2400 
0000014 Constant B4800 
0000015 Constant B9600 
0000016 Constant B19200
0000017 Constant B38400
0010001 Constant B57600
0010002 Constant B115200
0010003 Constant B230400
0010004 Constant B460800
0010005 Constant B500000
0010006 Constant B576000
0010007 Constant B921600
0010010 Constant B1000000
0010011 Constant B1152000
0010012 Constant B1500000
0010013 Constant B2000000
0010014 Constant B2500000
0010015 Constant B3000000
0010016 Constant B3500000
0010017 Constant B4000000
020000000000 Constant CRTSCTS
000000000060 Constant CS8
000000000100 Constant CSTOPB
000000000200 Constant CREAD
000000004000 Constant CLOCAL
000000004000 Constant IXANY
000000010017 Constant CBAUD
000000000001 Constant IGNBRK
000000000004 Constant IGNPAR
000000000400 Constant NOCTTY
000000004000 Constant NODELAY
base !

5 Constant VTIME
6 Constant VMIN

$5409 Constant TCSBRK
$540B Constant TCFLSH
$541B Constant FIONREAD

: set-baud ( baud fd -- )  >r
    r@ t_old tcgetattr drop
    t_old t_buf termios move
    t_buf cfmakeraw
    t_buf over cfsetispeed drop
    t_buf swap cfsetospeed drop
    r> 0 t_buf tcsetattr drop ;

: reset-baud ( fd -- )
    0 t_old tcsetattr drop ;

: check-read ( forth-fd -- n )  0 { w^ io-result }
    fileno FIONREAD io-result ioctl drop io-result l@ ;

\ get and set control lines

$5415 CONSTANT TIOCMGET
$5418 CONSTANT TIOCMSET
$002  CONSTANT TIOCM_DTR
$004  CONSTANT TIOCM_RTS
$020  CONSTANT TIOCM_CTS
$100  CONSTANT TIOCM_DSR

: get-ioctl  ( fd -- n ) 0 { w^ io-result }
     TIOCMGET io-result ioctl 0< abort" IOCTL GET Failed." io-result l@ ;

: set-ioctl  ( fd n -- ) 0 { w^ io-result } io-result l!
    TIOCMSET io-result ioctl 0< abort" IOCTL SET Failed." ;

: set-dtr  ( fd -- )
     dup get-ioctl TIOCM_DTR or set-ioctl ;

: clr-dtr  ( fd -- )
     dup get-ioctl TIOCM_DTR invert and set-ioctl ;

: set-rts  ( fd -- )
     dup get-ioctl TIOCM_RTS or set-ioctl ;

: clr-rts  ( fd -- )
     dup get-ioctl TIOCM_RTS invert and set-ioctl ;

: get-cts  ( fd -- n )
     get-ioctl TIOCM_CTS and ;

: get-dsr  ( fd -- n )
     get-ioctl TIOCM_DSR and ;
