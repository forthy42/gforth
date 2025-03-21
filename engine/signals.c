/* signal handling

  Authors: Anton Ertl, Bernd Paysan
  Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007,2011,2012,2013,2014,2015,2016,2018,2019,2020,2021,2023,2024 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation, either version 3
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, see http://www.gnu.org/licenses/.

*/


#include "config.h"
#include "forth.h"
#include <stdio.h>
#include <setjmp.h>
#include <string.h>
#include <stdlib.h>
#if !defined(apollo) && !defined(MSDOS)
#include <sys/ioctl.h>
#endif
#include <sys/types.h>
#include <signal.h>
#include <termios.h>
#include <stdarg.h>
#include "io.h"

#ifdef HAS_DEBUG
extern int debug;
# define debugp(x...) do { if (debug) fprintf(x); } while (0)
#else
# define perror(x...)
# define fprintf(x...)
# define debugp(x...)
#endif

#ifndef HAVE_STACK_T
/* Darwin uses "struct sigaltstack" instead of "stack_t" */
typedef struct sigaltstack stack_t;
#endif

#define DEFAULTCOLS 80
#if defined(MSDOS) || defined (_WIN32) || defined (__CYGWIN__)
#define DEFAULTROWS 25
#else
#define DEFAULTROWS 24
#endif

UCell cols=DEFAULTCOLS;
UCell rows=DEFAULTROWS;

#define SIGPP(sig) { switch(die_on_signal) {		\
    case 0: break;					\
    case 1: graceful_exit(sig); break;			\
    default: die_on_signal--;				\
    }							\
  }

#ifndef SA_NODEFER
#define SA_NODEFER 0
/* systems that don't have SA_NODEFER hopefully don't block anyway */
#endif

#ifndef SA_ONSTACK
#define SA_ONSTACK 0
#endif

#ifdef SA_SIGINFO
void install_signal_handler(int sig, void (*handler)(int, siginfo_t *, void *))
     /* installs three-argument signal handler for sig */
{
  struct sigaction action;

  action.sa_sigaction=handler;
  sigemptyset(&action.sa_mask);
  action.sa_flags=SA_RESTART|SA_NODEFER|SA_SIGINFO|SA_ONSTACK; /* pass siginfo */
  sigaction(sig, &action, NULL);
}
#endif

void gforth_sigset(sigset_t *set, ...)
{
  va_list ap;
  int sig;
  va_start(ap, set);
  sigemptyset(set);
  while((sig=va_arg(ap, int))) {
    sigaddset(set, sig);
  }
  va_end(ap);
}

Sigfunc *bsd_signal(int signo, Sigfunc *func)
{
  struct sigaction act, oact;

  act.sa_handler=func;
  sigemptyset(&act.sa_mask);
  act.sa_flags=SA_NODEFER; /* SA_ONSTACK does not work for graceful_exit */
  if (sigaction(signo,&act,&oact) < 0)
    return SIG_ERR;
  else
    return oact.sa_handler;
}

static void
graceful_exit (int sig)
{
  deprep_terminal();
  fprintf (stderr, "\n\n%s.\n", strsignal (sig));
  exit (0x80|sig);
}

void throw(int code)
{
  debugp(stderr,"\nthrow code %d to %p\n", code, *throw_jmp_handler);
  longjmp(*throw_jmp_handler,code); /* !! or use siglongjmp ? */
}

void gforth_fail()
{
  throw(-21);
}

static void 
signal_throw(int sig)
{
  int code;
  SIGPP(sig);
  debugp(stderr,"\ncaught signal %d\n", sig);

  switch (sig) {
  case SIGINT: code=-28; break;
  case SIGFPE: code=-55; break;
#ifdef SIGBUS
#ifdef __APPLE__
  case SIGBUS: code=-9; break; /* On MacOS, this is not an alignment exception */
#else
  case SIGBUS: code=-23; break;
#endif
#endif
  case SIGSEGV: code=-9; break;
#ifdef SIGPIPE
  case SIGPIPE: code=-2049; break;
#endif
  default: code=-256-sig; break;
  }
#ifdef __CYGWIN__
  /* the SA_NODEFER apparently does not work on Cygwin 1.3.18(0.69/3/2) */
  {
    sigset_t emptyset;
    sigemptyset(&emptyset);
    sigprocmask(SIG_SETMASK, &emptyset, NULL);
  }
#endif
  if(*throw_jmp_handler == NULL)
    graceful_exit(0x80|sig);
  throw(code);
}

#ifdef SA_SIGINFO
static void
sigaction_throw(int sig, siginfo_t *info, void *_)
{
  debugp(stderr,"\nsigaction_throw %d %p %p @%p\n", sig, info, _, info->si_addr);
  signal_throw(sig);
}

static void fpe_handler(int sig, siginfo_t *info, void *_)
     /* handler for SIGFPE */
{
  int code;

  SIGPP(sig);
  debugp(stderr,"\nfpe_handler %d %p %p\n", sig, info, _);

  switch(info->si_code) {
#ifdef FPE_INTDIV
  case FPE_INTDIV:
    if (gforth_debugging && ((Cell)gforth_SP)!=0)
      code = BALL_RESULTRANGE;
    else
      code= BALL_DIVZERO;
    break;
#endif
#ifdef FPE_INTOVF
  case FPE_INTOVF: code=BALL_RESULTRANGE; break; /* integer overflow */
#endif
#ifdef FPE_FLTDIV
  case FPE_FLTDIV: code=-42; break; /* floating point divide by zero */
#endif
#ifdef FPE_FLTOVF
  case FPE_FLTOVF: code=-43; break; /* floating point overflow  */
#endif
#ifdef FPE_FLTUND
  case FPE_FLTUND: code=-54; break; /* floating point underflow  */
#endif
#ifdef FPE_FLTRES
  case FPE_FLTRES: code=-41; break; /* floating point inexact result  */
#endif
#if 0 /* defined by Unix95, but unnecessary */
  case FPE_FLTINV: /* invalid floating point operation  */
  case FPE_FLTSUB: /* subscript out of range  */
#endif
  default: code=-55; break;
  }
  throw(code);
}


#define SPILLAGE 128
/* if there's a SIGSEGV within SPILLAGE bytes of some stack, we assume
   that this stack has over/underflowed */

#define JUSTUNDER(addr1,addr2) (((UCell)((addr2)-1-(addr1)))<SPILLAGE)
/* true is addr1 is just under addr2 */

#define JUSTOVER(addr1,addr2) (((UCell)((addr1)-(addr2)))<SPILLAGE)

#define NEXTPAGE(addr) ((Address)((((UCell)(addr)-1)&-pagesize)+pagesize))
#define NEXTPAGE2(addr) ((Address)((((UCell)(addr)-1)&-pagesize)+2*pagesize))
#define NEXTPAGE3(addr) ((Address)((((UCell)(addr)-1)&-pagesize)+3*pagesize))

static void segv_handler(int sig, siginfo_t *info, void *_)
{
  int code=-9;
  Address addr=info->si_addr;
  ImageHeader *section=gforth_UP->current_section;
  UCell section_end=(UCell)(section->base + section->dict_size + (pagesize-1)) & -pagesize;

  SIGPP(sig);
  debugp(stderr,"\nsegv_handler %d %p %p @%p\n", sig, info, _, addr);

  if ((UCell)(addr - section_end) < pagesize)
    code=-8;
  else if (JUSTUNDER(addr, NEXTPAGE3(gforth_UP)))
    code=-3;
  else if (JUSTOVER(addr, NEXTPAGE(gforth_UP->sp0)))
    code=-4;
  else if (JUSTUNDER(addr, NEXTPAGE2(gforth_UP->sp0)))
    code=-5;
  else if (JUSTOVER(addr, NEXTPAGE(gforth_UP->rp0)))
    code=-6;
  else if (JUSTUNDER(addr, NEXTPAGE2(gforth_UP->rp0)))
    code=-44;
  else if (JUSTOVER(addr, NEXTPAGE(gforth_UP->fp0)))
    code=-45;
  else if (JUSTUNDER(addr, NEXTPAGE2(gforth_UP->fp0)))
    code=-2058;
  else if (JUSTOVER(addr, NEXTPAGE(gforth_UP->lp0)))
    code=-2059;
  throw(code);
}

#endif /* defined(SA_SIGINFO) */

#ifdef SIGCONT
static void termprep(int sig)
{
  bsd_signal(sig,termprep);
  terminal_prepped=0;
}
#endif

void get_winsize()
{
#ifdef TIOCGWINSZ
  struct winsize size;
  size.ws_row = size.ws_col = 0;
  
  if (ioctl (1, TIOCGWINSZ, (char *) &size) >= 0) {
    rows = size.ws_row;
    cols = size.ws_col;
  }
#else
  char *s;
  if ((s=getenv("LINES"))) {
    rows=atoi(s);
  }
  if ((s=getenv("COLUMNS"))) {
    rows=atoi(s);
  }
#endif
  if (rows==0)
    rows=DEFAULTROWS;
  if (cols==0)
    cols=DEFAULTCOLS;
}

#ifdef SIGWINCH
extern Cell winch_addr;

static void change_winsize(int sig)
{
  /* signal(sig,change_winsize); should not be necessary with bsd_signal */
#ifdef TIOCGWINSZ
  get_winsize();
  winch_addr=-1;
#endif
}
#endif

void install_signal_handlers(void)
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

  static short async_sigs_to_throw [] = {
#ifdef SIGINT
    SIGINT,
#endif
#ifdef SIGALRM
    SIGALRM,
#endif
#ifdef SIGPOLL
    SIGPOLL,
#endif
#ifdef SIGPROF
    SIGPROF,
#endif
#ifdef SIGURG
    SIGURG,
#endif
#ifdef SIGPIPE
    SIGPIPE,
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

  static short sigs_to_throw [] = {
#ifdef SIGBREAK
    SIGBREAK,
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
#ifdef SIGBUS
    SIGBUS,
#endif
#ifdef SIGSYS
    SIGSYS,
#endif
#ifdef SIGTRAP
    SIGTRAP,
#endif
#ifdef SIGABRT
    SIGABRT,
#endif
  };

  static short sigs_to_quit [] = {
#ifdef SIGQUIT
    SIGQUIT,
#endif
#ifdef SIGHUP
    SIGHUP,
#endif
#ifdef SIGTERM
    SIGTERM,
#endif
#ifdef SIGXCPU
    SIGXCPU,
#endif
  };
  int i;
#if 0
  /* sigaltstack is now called by gforth_stacks() */
#if defined(SIGSTKSZ)
  stack_t sigstack;
  int sas_retval=-1;

  sigstack.ss_size=SIGSTKSZ;
  /* Actually the stack should only be ss_size large, and according to
     SUSv2 ss_sp should point to the start of the stack, but
     unfortunately Irix 6.5 (at least) expects ss_sp to point to the
     end, so we work around this issue by accomodating everyone. */
  if ((sigstack.ss_sp = gforth_alloc(sigstack.ss_size*2)) != NULL) {
    sigstack.ss_sp += sigstack.ss_size;
    sigstack.ss_flags=0;
    sas_retval=sigaltstack(&sigstack,(stack_t *)0);
  }
#if defined(HAS_FILE) || !defined(STANDALONE)
  debugp(stderr,"sigaltstack: %s\n",strerror(sas_retval));
#endif
#endif
#endif

#define DIM(X)		(sizeof (X) / sizeof *(X))
/*
  for (i = 0; i < DIM (sigs_to_ignore); i++)
    signal (sigs_to_ignore [i], SIG_IGN);
*/
  for (i = 0; i < DIM (sigs_to_throw); i++)
    bsd_signal(sigs_to_throw[i], signal_throw);
  for (i = 0; i < DIM (async_sigs_to_throw); i++)
    bsd_signal(async_sigs_to_throw[i], 
               ignore_async_signals ? SIG_IGN : signal_throw);
  for (i = 0; i < DIM (sigs_to_quit); i++)
    bsd_signal(sigs_to_quit [i], graceful_exit);
#ifdef SA_SIGINFO
#ifdef SIGFPE
  install_signal_handler(SIGFPE, fpe_handler);
#endif
#ifdef SIGSEGV
  install_signal_handler(SIGSEGV, segv_handler);
#endif
  /* use SA_ONSTACK for all signals that could come from executing
     wrong code */
#ifdef SIGILL
  install_signal_handler(SIGILL, sigaction_throw);
#endif
#ifdef SIGBUS
  install_signal_handler(SIGBUS, sigaction_throw);
#endif
#ifdef SIGTRAP
  install_signal_handler(SIGTRAP, sigaction_throw);
#endif
#ifdef SIGABRT
  install_signal_handler(SIGABRT, sigaction_throw);
#endif
#endif /* defined(SA_SIGINFO) */
#ifdef SIGCONT
    bsd_signal(SIGCONT, termprep);
#endif
#ifdef SIGWINCH
    bsd_signal(SIGWINCH, change_winsize);
#endif
}
