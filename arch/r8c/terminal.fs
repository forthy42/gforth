\ Terminal for R8C

require lib.fs
[IFUNDEF] libc  library libc libc.so.6  [THEN]

libc tcgetattr int ptr (int) tcgetattr ( fd termios -- r )
libc tcsetattr int int ptr (int) tcsetattr ( fd opt termios -- r )
libc tcflow int int (int) tcflow ( fd action -- r )
libc ioctl<p> int int ptr (int) ioctl ( d request ptr -- r )

4 4 2Constant int%

struct
    int% field c_iflag
    int% field c_oflag
    int% field c_cflag
    int% field c_lflag
    32 chars 0 field c_line
    int% field c_ispeed
    int% field c_ospeed
end-struct termios

Create t_old  termios %allot drop
Create t_buf  termios %allot drop

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
000000010001 Constant B57600
000000010002 Constant B115200
020000000000 Constant CRTSCTS
000000000060 Constant CS8
000000000200 Constant CREAD
000000004000 Constant CLOCAL
000000010017 Constant CBAUD
000000000001 Constant IGNBRK
000000000004 Constant IGNPAR
base !

6 Constant VTIME
7 Constant VMIN

: set-baud ( baud fd -- )  >r
  r@ t_old tcgetattr drop
  t_old t_buf termios %size move
  [ IGNBRK IGNPAR or         ] Literal    t_buf c_iflag l!
  0                                       t_buf c_oflag l!
  [ CS8 CREAD or CLOCAL or ] Literal or
                                          t_buf c_cflag l!
  0                                       t_buf c_lflag l!
  1 t_buf c_line VMIN + c!
  0 t_buf c_line VTIME + c!
    28800 t_buf c_cflag @ $F and lshift
    dup t_buf c_ispeed l! t_buf c_ospeed l!
  r> 1 t_buf tcsetattr drop ;

: reset-baud ( fd -- )
    1 t_old tcsetattr drop ;

$541B Constant FIONREAD

: check-read ( fd -- n )  >r
  0 sp@ r> FIONREAD rot ioctl<p> drop ;

: >fd ( wfileid -- fd )  &14 cells + @ ;

Create file-buf $40 allot
Variable file-len

0 Value term
0 Value termfile
Variable term-state

: term-end ( -- )
    4   term emit-file throw
    #cr term emit-file throw
    term flush-file throw ;
: open-include ( -- )
    file-buf file-len @ r/o open-file
    IF    ." File '" file-buf file-len @ type ." ' not found" term-end drop
    ELSE  to termfile  THEN ;
: end-include ( -- )  termfile 0= IF  EXIT  THEN
    termfile close-file throw  0 to termfile ;

: term-type ( addr u -- )
    bounds ?DO
	I c@ CASE
	    2 OF  1 term-state !  ENDOF
	    3 OF
		termfile IF
		    file-buf $40 termfile read-line throw
		ELSE
		    0 0
		THEN
		0= IF
		    term-end
		ELSE  file-buf swap term write-file throw
		    #cr  term emit-file throw  THEN
		term flush-file throw
	    ENDOF
	    4 OF end-include  ENDOF
	    5 OF  abort  ENDOF
	    term-state @ CASE
		0 OF  emit  ENDOF
		1 OF  $3F min file-len !  2 term-state !  ENDOF
		2 - file-buf + c!  1 term-state +!
		term-state @ file-len @ 2 + = IF
		    open-include term-state off  THEN
		0 ENDCASE
	0 ENDCASE
    LOOP ;

: terminal ( "name" -- ) cr
    parse-name r/w open-file throw dup to term dup >fd { term-fd }
    B38400 term-fd set-baud
    BEGIN
	pad term-fd check-read term read-file throw pad swap term-type
	key? IF  key term emit-file throw term flush-file throw
	ELSE  &10 ms  THEN
    AGAIN ;

    