\ terminal.fs
\
\ Simple terminal emulator for gforth (ported from kForth)
\
\ Written by David P. Wallace and Krishna Myneni
\ Provided under the terms of the GNU General Public License
\
\ Requires:
\
\	strings.fs
\	ansi.fs
\	syscalls386.fs
\	serial.fs
\
\ Revisions:
\	2004-03-13  Avoid response lag to input due to key? in terminal;
\	              added Send File function  KM
\       2004-09-17  Ported to gforth from kForth; use WRITE-FILE instead
\                     of "write" to store data in capture file  KM
\       2005-09-28  Fixed problem associated with read-line  KM
\
include strings.fs
include ansi.fs
include syscalls386.fs
include serial.fs

\ ============= defs from kForth files.4th 
base @
hex
 A  constant EOL
40  constant O_CREAT
80  constant O_EXCL
200 constant O_TRUNC
400 constant O_APPEND
 0  constant SEEK_SET
 1  constant SEEK_CUR
 2  constant SEEK_END
base !
create EOL_BUF 4 allot
EOL EOL_BUF c!
0 EOL_BUF 1+ c!

: file-exists ( ^filename  -- flag | return true if file exists )
        count R/O open-file
        if drop false else close-file drop true then ;
\ =============

: ms@ ( -- u )  utime 1 1000 m*/ d>s ; 


: >UPC 95 AND ;
: EKEY ( -- u | return extended key as concatenated byte sequence )
       BEGIN key? UNTIL
       0 BEGIN  key?  WHILE  8 LSHIFT key or  REPEAT ;


variable com			
create buf 64 allot

\ examples of using terminal:
\
\   COM2 B9600  c" 8N1" terminal 	( terminal on com2 at 9600 baud, 8N1 )
\   COM1 B57600 c" E71" terminal 	( terminal on com1 at 57.6 Kbaud, 7E1 )

HEX
0D     CONSTANT  <CR>
1B     CONSTANT  ESC
1B4F50 CONSTANT  F1
1B4F51 CONSTANT  F2
1B4F52 CONSTANT  F3
DECIMAL

0      CONSTANT  HELP_ROW
BLUE   CONSTANT  HELP_EKEY_COLOR
BLACK  CONSTANT  HELP_TEXT_COLOR
WHITE  CONSTANT  HELP_BACK_COLOR
BLACK  CONSTANT  TERM_BACK_COLOR
WHITE  CONSTANT  TERM_TEXT_COLOR

: clear-line ( row background -- ) background dup 0 SWAP AT-XY 
       80 spaces 0 SWAP AT-XY ;

: set-terminal-colors ( -- )
	TERM_TEXT_COLOR foreground
	TERM_BACK_COLOR background ;
  
: terminal-help ( -- | show the help line )
        save_cursor
	HELP_ROW HELP_BACK_COLOR clear-line
	HELP_EKEY_COLOR foreground   ." Esc "
	HELP_TEXT_COLOR foreground   ." Exit  "
	HELP_EKEY_COLOR foreground   ." F1 "
	HELP_TEXT_COLOR foreground   ." Show Key Help   "
	HELP_EKEY_COLOR foreground   ." F2 "
	HELP_TEXT_COLOR foreground   ." Capture On/Off  "
	HELP_EKEY_COLOR foreground   ." F3 "
	HELP_TEXT_COLOR foreground   ." Send Text File  "
	restore_cursor
;


variable fid
FALSE VALUE ?capture
create filename 256 allot
create capture-filename 256 allot

: close-capture-file ( -- )  fid @ close drop FALSE to ?capture ;

: capture-file ( -- )
     ?capture IF close-capture-file
                 HELP_ROW HELP_BACK_COLOR clear-line
		 HELP_TEXT_COLOR foreground
		 ." Capture file closed!"
              ELSE
		HELP_ROW HELP_BACK_COLOR clear-line
		HELP_TEXT_COLOR foreground
		." Capture to file named: "
		filename 254 accept
		filename swap strpck capture-filename strcpy
		capture-filename file-exists IF
		  HELP_ROW HELP_BACK_COLOR clear-line
		  ." File " capture-filename count type 
		  ."  already exists! Overwrite (Y/N)? "
		  key >upc [char] Y = IF
		    capture-filename count W/O O_TRUNC or open-file
		    0= IF fid ! TRUE to ?capture
		       ELSE HELP_ROW HELP_BACK_COLOR clear-line
		         ." Unable to open output file!"
		         EXIT
		       THEN
		  ELSE
		    HELP_ROW HELP_BACK_COLOR clear-line
		    ." Capture cancelled!" EXIT
		  THEN
		ELSE
		  capture-filename count W/O create-file
		  0= IF fid ! TRUE to ?capture
		     ELSE HELP_ROW HELP_BACK_COLOR clear-line
		       ." Unable to open output file!"
		       EXIT
		     THEN
		THEN
	      THEN ;


create send-filename 256 allot
create send-line-buffer 256 allot
variable txfid
variable last-send-time
10    VALUE LINE-DELAY        \ delay in ms between sending each line of text
 1    VALUE CHAR-DELAY        \ to send data to *slow* terminals
FALSE VALUE ?sending
		
: send-file ( -- )
	    HELP_ROW HELP_BACK_COLOR clear-line
	    HELP_TEXT_COLOR foreground
	    ." Text File to Send: "
	    filename 254 accept
	    filename swap strpck send-filename strcpy
	    send-filename file-exists 0= IF
	      HELP_ROW HELP_BACK_COLOR clear-line
	      ." Input file does not exist!"
	      EXIT
	    THEN
	    send-filename count R/O open-file 0= IF
	      txfid !
	      HELP_ROW HELP_BACK_COLOR clear-line
	      ." Sending file " send-filename count type ."  ..."
	      TRUE to ?sending
	    ELSE
	      HELP_ROW HELP_BACK_COLOR clear-line
	      ." Unable to open input file!"
	      EXIT
	    THEN 
	    ms@ last-send-time ! ;


: terminal-status? ( -- flag | TRUE equals ok to exit terminal )
        ?sending IF
	  HELP_ROW HELP_BACK_COLOR clear-line
	  HELP_TEXT_COLOR foreground
	  ." File Send in Progress! Halt Sending and Exit (Y/N)? "
	  KEY >UPC [CHAR] Y = IF
	    txfid @ close-file drop
	    FALSE TO ?sending
	  ELSE
	    0 EXIT
	  THEN
	THEN
	?capture IF close-capture-file THEN
	TRUE ;
	  
: terminal ( port baud ^str_param -- | terminal emulator )
	TERM_BACK_COLOR background
	page
	terminal-help
	set-terminal-colors
	0 HELP_ROW 1+ AT-XY

	rot
	serial_open com !
	com @ swap serial_setparams
	com @ swap serial_setbaud
   
	BEGIN

	  ?sending ms@ last-send-time @ - LINE-DELAY >= AND IF
	    ms@ last-send-time !
	    send-line-buffer 256 txfid @ read-line IF
	      \ error reading file
	      2drop txfid @ close-file drop FALSE to ?sending
	      save_cursor
	      HELP_ROW HELP_BACK_COLOR clear-line
	      HELP_TEXT_COLOR foreground
	      ." Error reading file!"
	        restore_cursor set-terminal-colors
	    ELSE
	      FALSE = IF
	        \ reached EOF
		drop txfid @ close-file drop
	        FALSE to ?sending
	        save_cursor
	        HELP_ROW HELP_BACK_COLOR clear-line
	        HELP_TEXT_COLOR foreground
	        ." <<Terminal: Send Completed!>>"
	        restore_cursor set-terminal-colors
	      ELSE
	        com @ swap send-line-buffer swap serial_write drop
	      THEN
	    THEN
	  THEN

	  BEGIN
	    com @ serial_lenrx
	  WHILE
	    com @ buf 1 serial_read drop
	    buf c@ dup <CR> = IF CR ELSE emit THEN
	    ?capture IF
	      buf c@ <CR> = IF EOL_BUF dup strlen ELSE buf 1 THEN   
	      fid @ write-file drop 
	    THEN
	  REPEAT

	  key?

	  IF
	    EKEY CASE
	      ESC  OF terminal-status? IF 
	                com @ serial_close drop
	                text_normal \ restore normal colors and attributes
	                PAGE EXIT   \ clear the screen and exit
		      THEN ENDOF
	      F1   OF terminal-help set-terminal-colors ENDOF 
	      F2   OF save_cursor capture-file restore_cursor  
	              set-terminal-colors ENDOF
	      F3   OF save_cursor send-file restore_cursor     
	              set-terminal-colors ENDOF
	      dup  dup emit buf c! com @ buf 1 serial_write drop
	    ENDCASE
	  THEN
	AGAIN ;
		
: term ( -- | start the default terminal )
     COM1 B9600  c" 8N1" terminal 	( terminal on com1 at 9600 baud, 8N1 )
;

CR CR
.( Type 'term' to start a 9600 baud terminal on COM1 configured with 8N1.)
CR CR
