alarm	u --	unixlib
alarm(u);

timeusec	-- u_usec u_sec	unixlib
struct timeval tv;
struct timezone zone1;
 gettimeofday(&tv,&zone1);
u_usec=tv.tv_usec;
u_sec=tv.tv_sec;

time	-- u1	unixlib
u1=(long) time(NULL);

\ Serial Interface

setttyspeed	u u2 -- wior	unixlib
struct termios tm;
int BT[]={
  0,B0,50,B50,75,B75,110,B110,134,B134,150,B150,200,B200,300,B300,
  600,B600,1200,B1200,1800,B1800,2400,B2400,4800,B4800,9600,B9600,19200,B19200,38400,B38400,
#ifdef B57600
  57600,B57600,
#endif
#ifdef B115200
  115200,B115200,
#endif
#ifdef B230400
  230400,B230400,
#endif
  1};
speed_t br;
int i;
i=0; br=0;
while (BT[i]!=1)
{ if (BT[i]==u)
  { br=BT[i+1];
    break;
  }
  i=i+2;
}
if (BT[i]!=1)
{ tcgetattr(u2,&tm);
  cfsetispeed(&tm, br);
  cfsetospeed(&tm, br);
  tcsetattr(u2,TCSANOW,&tm);
  wior=0;
} else
{ wior=-1;
}

setttyraw	u -- wior	unixlib
struct termios tm;
tcgetattr(u,&tm);
/* cfmakeraw(&tm); !!!!???? worked with linux, but nut on solaris */
tcsetattr(u,TCSANOW,&tm);
wior=0;

ttytostd	c_addr1 u1 -- wior	unixlib
int i;
wior=0;
close(0);
close(1);
close(2);
i=open(cstr(c_addr1,u1,0),O_RDWR|O_NOCTTY);
if ((i==-1) || (i!=0))
{	wior=-1;
} else
{	i= dup(0);
	i= dup(0);
	i=open("/dev/tty",O_RDWR);
	if (i>=0) {
	  ioctl(i,TIOCNOTTY,0);
	  (void) close(i);
	}
}

uopen	c_addr u uflags umode -- w2 wior	file
w2 = open(tilde_cstr(c_addr, u, 1), uflags , (mode_t) umode);
if (w2 == -1) {
  wior = -37;
} else {
  wior = 0;
}

uread	c_addr u u1 -- u3 wior	new
wior=0;
if ((u3 = read(u1, c_addr, u))==-1) 
{	if (errno==EWOULDBLOCK) u3=0;
	else wior=-37;
} else
{ 	if (u3==0) wior=-39;
}

uwrite	c_addr u u1 -- u3 wior	new
wior=0;
if ((u3 = write(u1, c_addr, u))==-1)
{	if (errno==EAGAIN) u3=0;
	else wior=-37;
}

uclose	u -- wior	new
wior=0;
if (close(u)) wior=-37;

nonblock	u1 -- wior	new
fcntl(u1,F_SETFL,O_NONBLOCK);
wior=0;

get_cconst	c_addr u -- u1 wior	new
static char CONST_NAMES[][32]={
"O_RDONLY",
"O_WRONLY",
"O_RDWR",
""};
static unsigned int CONST_VALUES[]={
O_RDONLY,
O_WRONLY,
O_RDWR}; 
int i=0;
int contd=1;
u1=0;
wior=-1;
while (CONST_NAMES[i] && contd) {
	if (strcmp(CONST_NAMES[i],cstr(c_addr,u,1))==0) {
		contd=0; u1=CONST_VALUES[i]; wior=0;} ;
	i++;
}

fork	-- u	new
u=fork();

wait	a_addr -- u	new	I_wait
u=wait((int *) a_addr);

waitpid	u a_addr u2 -- u3	new	I_waitpid
u3=waitpid((pid_t) u,(int *) a_addr,u2);

execv	c_addr1 u1 c_addr2 --	new
char *s1=cstr(c_addr1, u1, 0);
execv(s1, (void *) c_addr2); 

errno	-- u	new
u=errno;
