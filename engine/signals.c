/* signal handling

  Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

*/


#define _GNU_SOURCE

#include <stdio.h>
#include <signal.h>
#include <setjmp.h>
#include <string.h>
#include <stdlib.h>
#if !defined(apollo) && !defined(MSDOS)
#include <sys/ioctl.h>
#endif
#include "config.h"
#include "forth.h"
#include "io.h"


#define DEFAULTCOLS 80
#if defined(MSDOS) || defined (_WIN32)
#define DEFAULTROWS 25
#else
#define DEFAULTROWS 24
#endif

UCell cols=DEFAULTCOLS;
UCell rows=DEFAULTROWS;


static void
graceful_exit (int sig)
{
  deprep_terminal();
  fprintf (stderr, "\n\n%s.\n", strsignal (sig));
  exit (0x80|sig);
}

jmp_buf throw_jmp_buf;

static void 
signal_throw(int sig)
{
  int code;
  struct {
    int signal;
    int throwcode;
  } *p, throwtable[] = {
    { SIGINT, -28 },
    { SIGFPE, -55 },
#ifdef SIGBUS
    { SIGBUS, -23 },
#endif
    { SIGSEGV, -9 },
  };
  signal(sig,signal_throw);
  for (code=-256-sig, p=throwtable; p<throwtable+(sizeof(throwtable)/sizeof(*p)); p++)
    if (sig == p->signal) {
      code = p->throwcode;
      break;
    }
  longjmp(throw_jmp_buf,code); /* or use siglongjmp ? */
}

#ifdef SIGCONT
static void termprep (int sig)
{
  signal(sig,termprep);
  terminal_prepped=0;
}
#endif

void get_winsize()
{
#ifdef TIOCGWINSZ
  struct winsize size;
  
  if (ioctl (1, TIOCGWINSZ, (char *) &size) >= 0) {
    rows = size.ws_row;
    cols = size.ws_col;
  }
#else
  char *s;
  if ((s=getenv("LINES"))) {
    rows=atoi(s);
    if (rows==0)
      rows=DEFAULTROWS;
  }
  if ((s=getenv("COLUMNS"))) {
    rows=atoi(s);
    if (rows==0)
      cols=DEFAULTCOLS;
  }
#endif
}

#ifdef SIGWINCH
static void change_winsize(int sig)
{
  signal(sig,change_winsize);
#ifdef TIOCGWINSZ
  get_winsize();
#endif
}
#endif

void install_signal_handlers (void)
{

#if 0
/* these signals are handled right by default, no need to handle them;
   they are listed here just for fun */
  static short sigs_to_default [] = {
#ifdef SIGCHLD
    SIGCHLD,
#endif
#ifdef SIGINFO
    SIGINFO,
#endif
#ifdef SIGIO
    SIGIO,
#endif
#ifdef SIGLOST
    SIGLOST,
#endif
#ifdef SIGKILL
    SIGKILL,
#endif
#ifdef SIGSTOP
    SIGSTOP,
#endif
#ifdef SIGPWR
    SIGPWR,
#endif
#ifdef SIGMSG
    SIGMSG,
#endif
#ifdef SIGDANGER
    SIGDANGER,
#endif
#ifdef SIGMIGRATE
    SIGMIGRATE,
#endif
#ifdef SIGPRE
    SIGPRE,
#endif
#ifdef SIGVIRT
    SIGVIRT,
#endif
#ifdef SIGGRANT
    SIGGRANT,
#endif
#ifdef SIGRETRACT
    SIGRETRACT,
#endif
#ifdef SIGSOUND
    SIGSOUND,
#endif
#ifdef SIGSAK
    SIGSAK,
#endif
#ifdef SIGTSTP
    SIGTSTP,
#endif
#ifdef SIGTTIN
    SIGTTIN,
#endif
#ifdef SIGTTOU
    SIGTTOU,
#endif
#ifdef SIGSTKFLT
    SIGSTKFLT,
#endif
#ifdef SIGUNUSED
    SIGUNUSED,
#endif
  };
#endif

  static short sigs_to_throw [] = {
#ifdef SIGBREAK
    SIGBREAK,
#endif
#ifdef SIGINT
    SIGINT,
#endif
#ifdef SIGILL
    SIGILL,
#endif
#ifdef SIGEMT
    SIGEMT,
#endif
#ifdef SIGFPE
    SIGFPE,
#endif
#ifdef SIGIOT
    SIGIOT,
#endif
#ifdef SIGSEGV
    SIGSEGV,
#endif
#ifdef SIGALRM
    SIGALRM,
#endif
#ifdef SIGPIPE
    SIGPIPE,
#endif
#ifdef SIGPOLL
    SIGPOLL,
#endif
#ifdef SIGPROF
    SIGPROF,
#endif
#ifdef SIGBUS
    SIGBUS,
#endif
#ifdef SIGSYS
    SIGSYS,
#endif
#ifdef SIGTRAP
    SIGTRAP,
#endif
#ifdef SIGURG
    SIGURG,
#endif
#ifdef SIGUSR1
    SIGUSR1,
#endif
#ifdef SIGUSR2
    SIGUSR2,
#endif
#ifdef SIGVTALRM
    SIGVTALRM,
#endif
#ifdef SIGXFSZ
    SIGXFSZ,
#endif
  };
  static short sigs_to_quit [] = {
#ifdef SIGHUP
    SIGHUP,
#endif
#ifdef SIGQUIT
    SIGQUIT,
#endif
#ifdef SIGABRT
    SIGABRT,
#endif
#ifdef SIGTERM
    SIGTERM,
#endif
#ifdef SIGXCPU
    SIGXCPU,
#endif
  };
  int i;

#define DIM(X)		(sizeof (X) / sizeof *(X))
/*
  for (i = 0; i < DIM (sigs_to_ignore); i++)
    signal (sigs_to_ignore [i], SIG_IGN);
*/
  for (i = 0; i < DIM (sigs_to_throw); i++)
    signal (sigs_to_throw [i], die_on_signal ? graceful_exit : signal_throw);
  for (i = 0; i < DIM (sigs_to_quit); i++)
    signal (sigs_to_quit [i], graceful_exit);
#ifdef SIGCONT
    signal (SIGCONT, termprep);
#endif
#ifdef SIGWINCH
    signal (SIGWINCH, change_winsize);
#endif
}
