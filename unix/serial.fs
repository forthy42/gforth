\ serial interface for Gforth under Unix

c-library serial
    \c #include <termios.h>
    \c #include <sys/ioctl.h>
    \c #include <stdio.h>
    c-function tcgetattr tcgetattr n a -- n ( fd termios -- r )
    c-function tcsetattr tcsetattr n n a -- n ( fd opt termios -- r )
    c-function tcflow tcflow n n -- n ( fd action -- n )
    c-function ioctl ioctl n n a -- n ( fd cmd ptr -- n )
    c-function fileno fileno a{(FILE*)} -- n ( file* -- fd )
end-c-library

: lfield: ( offset -- offset' )
    3 + -4 and 4 +field ;

begin-structure termios
lfield: c_iflag           \ input mode flags
lfield: c_oflag           \ output mode flags
lfield: c_cflag           \ control mode flags
lfield: c_lflag           \ local mode flags
cfield: c_line
32 +field c_cc           \ line discipline
lfield: c_ispeed          \ input speed
lfield: c_ospeed          \ output speed
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
base !

5 Constant VTIME
6 Constant VMIN

$5409 Constant TCSBRK
$540B Constant TCFLSH
$541B Constant FIONREAD

: set-baud ( baud fd -- )  >r
    t_old r@ tcgetattr drop
    t_old t_buf termios move
    \  t_buf sizeof termios erase
    [ IGNPAR                   ] Literal    t_buf  c_iflag l!
    0                                       t_buf  c_oflag l!
    [ 0 CS8 or CSTOPB or CREAD or CLOCAL or ] Literal or
    t_buf  c_cflag l!
    0                                       t_buf  c_lflag l!
    1 t_buf  c_cc VMIN + c!
    0 t_buf  c_cc VTIME + c!
    
    28800 t_buf  c_cflag @ $F and lshift

    dup t_buf  c_ispeed l! t_buf  c_ospeed l!
    t_buf 1 r> tcsetattr drop ;

: reset-baud ( fd -- )
    t_old 1 rot tcsetattr drop ;

: check-read ( fd -- n )  >r
    0 sp@ FIONREAD r> fileno ioctl drop ;
