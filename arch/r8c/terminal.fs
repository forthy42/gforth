\ Terminal for R8C

require lib.fs

s" os-type" environment? [IF]
    2dup s" linux-gnu" str= [IF] 2drop
	[IFUNDEF] libc  library libc libc.so.6  [THEN]
	
	libc tcgetattr int ptr (int) tcgetattr ( fd termios -- r )
	libc tcsetattr int int ptr (int) tcsetattr ( fd opt termios -- r )
	libc tcflow int int (int) tcflow ( fd action -- r )
	libc ioctl<p> int int ptr (int) ioctl ( d request ptr -- r )
	libc fileno ptr (int) fileno ( file* -- fd )
	
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
	
	0 Value term
	0 Value term-fd
	: open-port ( addr u -- )
	    r/w open-file throw dup to term dup fileno to term-fd ;
	: term-read ( -- addr u )
	    pad term-fd check-read term read-file throw pad swap ;
	: term-emit ( char -- )
	    term emit-file throw ;
	: (term-type) ( addr u -- )
	    term write-file throw ;
	: term-flush ( -- )
	    term flush-file throw ;
    [ELSE] s" cygwin" str= [IF]
	    \ Cygwin terminal adoption
	    library kernel32 kernel32
	    
	    kernel32 GetCommState int ptr (int) GetCommState ( handle addr -- r )
	    kernel32 SetCommState int ptr (int) SetCommState ( handle addr -- r )
	    kernel32 CreateFile ptr int int ptr int int ptr (int) CreateFileA ( name access share security disp attr temp -- handle )
	    kernel32 WriteFile int ptr int ptr ptr (int) WriteFile ( handle data size &len &data -- flag )
	    kernel32 ReadFile int ptr int ptr ptr (int) ReadFile ( handle data size &len &data -- flag )
	    kernel32 SetCommTimeouts int ptr (int) SetCommTimeouts ( handle addr -- flag )
	    kernel32 GetCommTimeouts int ptr (int) GetCommTimeouts ( handle addr -- flag )
	    
	    $80000000 Constant GENERIC_READ
	    $40000000 Constant GENERIC_WRITE
	    3 Constant OPEN_EXISTING
	    
	    50 Constant B50
	    75 Constant B75
	    110 Constant B110
	    134 Constant B134
	    150 Constant B150
	    200 Constant B200
	    300 Constant B300
	    600 Constant B600
	    1200 Constant B1200
	    1800 Constant B1800
	    2400 Constant B2400
	    4800 Constant B4800
	    9600 Constant B9600
	    19200 Constant B19200
	    38400 Constant B38400
	    
	    4 4 2Constant int%
	    2 2 2Constant word%
	    
	    struct
		int% field DCBlength
		int% field BaudRate
		int% field flags
		word% field wReserved
		word% field XonLim
		word% field XoffLim
		char% field ByteSize
		char% field Parity
		char% field StopBits
		char% field XonChar
		char% field XoffChar
		char% field ErrorChar
		char% field EofChar
		char% field EvtChar
		word% field wReserved1
	    end-struct DCB
	    struct
		int% field ReadIntervalTimeout
		int% field ReadTotalTimeoutMultiplier
		int% field ReadTotalTimeoutConstant
		int% field WriteTotalTimeoutMultiplier
		int% field WriteTotalTimeoutConstant
	    end-struct COMMTIMEOUTS
	    
	    Create t_old  DCB %allot drop
	    Create t_buf  DCB %allot drop
	    Create tout_buf  COMMTIMEOUTS %allot drop
	    
	    0 Value term-fd
	    0 Value term
	    : open-port ( addr u -- )
		tuck pad swap move 0 swap pad + c!
		pad GENERIC_READ GENERIC_WRITE or 0 0 OPEN_EXISTING 0 0 CreateFile
		to term-fd ;
	    : set-baud ( baud fd -- )  >r
		r@ t_old GetCommState drop
		1 t_old flags !
		r@ tout_buf GetCommTimeouts drop
		3 tout_buf ReadIntervalTimeout !
		3 tout_buf ReadTotalTimeoutMultiplier !
		2 tout_buf ReadTotalTimeoutConstant !
		3 tout_buf WriteTotalTimeoutMultiplier !
		2 tout_buf WriteTotalTimeoutConstant !
		r@ tout_buf SetCommTimeouts drop
		t_old t_buf DCB %size move
		t_buf BaudRate !
                8 t_buf ByteSize c!
		r> t_buf SetCommState drop ;
	    : reset-baud ( fd -- )
		t_old SetCommState drop ;
	    Create emit-buf  0 c,
            Variable term-len
	    : term-read ( -- addr u )
		term-fd pad &64 term-len 0 ReadFile drop
		pad term-len @ ;
	    : (term-type) ( addr u -- )
	        term-fd -rot term-len 0 WriteFile drop ;
	    : term-emit ( char -- )
		emit-buf c!  emit-buf 1 (term-type) ;
	    : term-flush ( -- ) ;
    [THEN]
[THEN]

Create file-buf $40 allot
Variable file-len
Variable term-stack $10 cells allot

: 'term ( -- addr ) term-stack @ cells term-stack + ;
: termfile ( -- file ) 'term @ ;
: >term ( o -- )  1 term-stack +! 'term ! ;
: term> ( -- )  -1 term-stack +! ;
Variable term-state
Variable progress-state

: term-end ( -- )
    4   term-emit
    #cr term-emit
    term-flush ;
: open-include ( -- )
    file-buf file-len @ r/o open-file
    IF    ." File '" file-buf file-len @ type ." ' not found" term-end drop
    ELSE  >term  THEN ;
: end-include ( -- )  termfile 0= IF  EXIT  THEN
    termfile close-file throw  term> ;

Create progress s" /-\|" here over allot swap move

: term-type ( addr u -- )
    bounds ?DO
	I c@ CASE
	    2 OF  1 term-state !  ENDOF
	    3 OF
		BEGIN
		    termfile IF
			file-buf $40 termfile read-line throw
			progress progress-state @ + c@ emit #bs emit
			progress-state @ 1+ 3 and progress-state !
		    ELSE
			0 0
		    THEN
		    0= termfile and  WHILE
			drop end-include
		REPEAT
		term-stack @ 0= IF
		    drop term-end
		ELSE
		    file-buf swap (term-type)
		    #cr  term-emit
		THEN
		term-flush
	    ENDOF
	    4 OF end-include  ENDOF
	    5 OF  abort  ENDOF
	    term-state @ CASE
		0 OF  emit  ENDOF
		1 OF  $20 - $3F min file-len !  2 term-state !  ENDOF
		2 - file-buf + c!  1 term-state +!
		term-state @ file-len @ 2 + = IF
		    open-include term-state off  THEN
		0 ENDCASE
	0 ENDCASE
    LOOP ;

: term-loop ( -- )
    BEGIN
	term-read term-type
	key? IF  key term-emit term-flush
	ELSE  &10 ms  THEN
    AGAIN ;
: say-hallo
    ." Gforth terminal"
    cr ." Press ENTER to get ok from connected device."
    cr ." Leave with BYE" 
    cr ;
: terminal ( "name" -- )
    parse-name open-port
    B38400 term-fd set-baud say-hallo ['] term-loop catch
    dup -1 = IF  drop cr EXIT  THEN  throw ;

s" os-type" environment? [IF]
    2dup s" linux-gnu" str= [IF] 2drop
        script? [IF]  terminal /dev/ttyUSB0 bye [THEN]
    [ELSE] s" cygwin" str= [IF]
        script? [IF]  terminal COM2 bye [THEN]
    [THEN]
[THEN]
