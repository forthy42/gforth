/* This file defines a number of threading schemes.

  Copyright (C) 1995, 1996,1997 Free Software Foundation, Inc.

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


  This files defines macros for threading. Many sets of macros are
  defined. Functionally they have only one difference: Some implement
  direct threading, some indirect threading. The other differences are
  just variations to help GCC generate faster code for various
  machines.

  (Well, to tell the truth, there actually is another functional
  difference in some pathological cases: e.g., a '!' stores into the
  cell where the next executed word comes from; or, the next word
  executed comes from the top-of-stack. These differences are one of
  the reasons why GCC cannot produce the right variation by itself. We
  chose disallowing such practices and using the added implementation
  freedom to achieve a significant speedup, because these practices
  are not common in Forth (I have never heard of or seen anyone using
  them), and it is easy to circumvent problems: A control flow change
  will flush any prefetched words; you may want to do a "0
  drop" before that to write back the top-of-stack cache.)

  These macro sets are used in the following ways: After translation
  to C a typical primitive looks like

  ...
  {
  DEF_CA
  other declarations
  NEXT_P0;
  main part of the primitive
  NEXT_P1;
  store results to stack
  NEXT_P2;
  }

  DEF_CA and all the NEXT_P* together must implement NEXT; In the main
  part the instruction pointer can be read with IP, changed with
  INC_IP(const_inc), and the cell right behind the presently executing
  word (i.e. the value of *IP) is accessed with NEXT_INST.

  If a primitive does not fall through the main part, it has to do the
  rest by itself. If it changes ip, it has to redo NEXT_P0 (perhaps we
  should define a macro SET_IP).

  Some primitives (execute, dodefer) do not end with NEXT, but with
  EXEC(.). If NEXT_P0 has been called earlier, it has to perform
  "ip=IP;" to ensure that ip has the right value (NEXT_P0 may change
  it).

  Finally, there is NEXT1_P1 and NEXT1_P2, which are parts of EXEC
  (EXEC(XT) could be defined as "cfa=XT; NEXT1_P1; NEXT1_P2;" (is this
  true?)) and are used for making docol faster.

  We can define the ways in which these macros are used with a regular
  expression:

  For a primitive

  DEF_CA NEXT_P0 ( IP | INC_IP | NEXT_INST | ip=...; NEXT_P0 ) * ( NEXT_P1 NEXT_P2 | EXEC(...) )

  For a run-time routine, e.g., docol:
  PFA1(cfa) ( NEXT_P0 NEXT | cfa=...; NEXT1_P1; NEXT1_P2 | EXEC(...) )

  This comment does not yet describe all the dependences that the
  macros have to satisfy.

  To organize the former ifdef chaos, each path is separated
  This gives a quite impressive number of paths, but you clearly
  find things that go together.

  It should be possible to organize the whole thing in a way that
  contains less redundancy and allows a simpler description.

*/

/* CFA_NEXT: if NEXT uses cfa, you have to #define CFA_NEXT, to get
 * cfa declared in engine.
 */

#ifdef DOUBLY_INDIRECT
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ip++; ca=**cfa;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({DEF_CA cfa=(XT); ca=**cfa; goto *ca;})
#  define NEXT1_P1 ({ca = **cfa;})
#  define NEXT1_P2 ({goto *ca;})

#else /* !defined(DOUBLY_INDIRECT) */

#if defined(DIRECT_THREADED)

/* note that the "cfa dead" versions only work if GETCFA exists and works */

#if THREADING_SCHEME==1
#warning direct threading scheme 1: autoinc, long latency, cfa live
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip++;})
#  define IP		(ip-1)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({goto *cfa;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#endif

#if THREADING_SCHEME==2
#warning direct threading scheme 2: autoinc, long latency, cfa dead
#ifndef GETCFA
#error GETCFA must be defined for cfa dead threading
#endif
#  define NEXT_P0	(ip++)
#  define IP		(ip-1)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*(ip-1))
#  define INC_IP(const_inc)	({ ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({goto **(ip-1);})
#  define EXEC(XT)	({goto *(XT);})
#endif


#if THREADING_SCHEME==3
#warning direct threading scheme 3: autoinc, low latency, cfa live
#  define CFA_NEXT
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	({cfa=*ip++;})
#  define NEXT_P2	({goto *cfa;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#endif

#if THREADING_SCHEME==4
#warning direct threading scheme 4: autoinc, low latency, cfa dead
#ifndef GETCFA
#error GETCFA must be defined for cfa dead threading
#endif
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({goto **(ip++);})
#  define EXEC(XT)	({goto *(XT);})
#endif

#if THREADING_SCHEME==5
#warning direct threading scheme 5: long latency, cfa live
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto *cfa;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#endif

#if THREADING_SCHEME==6
#warning direct threading scheme 6: long latency, cfa dead
#ifndef GETCFA
#error GETCFA must be defined for cfa dead threading
#endif
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto **(ip-1);})
#  define EXEC(XT)	({goto *(XT);})
#endif


#if THREADING_SCHEME==7
#warning direct threading scheme 7: low latency, cfa live
#  define CFA_NEXT
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	({cfa=*ip++;})
#  define NEXT_P2	({goto *cfa;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#endif

#if THREADING_SCHEME==8
#warning direct threading scheme 8: cfa dead, i386 hack
#ifndef GETCFA
#error GETCFA must be defined for cfa dead threading
#endif
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*IP)
#  define INC_IP(const_inc)	({ ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto **(ip-1);})
#  define EXEC(XT)	({goto *(XT);})
#endif

#if THREADING_SCHEME==9
#warning direct threading scheme 9: Power/PPC hack, long latency
/* Power uses a prepare-to-branch instruction, and the latency between
   this inst and the branch is 5 cycles on a PPC604; so we utilize this
   to do some prefetching in between */
#  define CFA_NEXT
#  define NEXT_P0
#  define IP		ip
#  define SET_IP(p)	({ip=(p); next_cfa=*ip; NEXT_P0;})
#  define NEXT_INST	(next_cfa)
#  define INC_IP(const_inc)	({next_cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ca=next_cfa; cfa=next_cfa; ip++; next_cfa=*ip;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#  define MORE_VARS	Xt next_cfa;
#endif

#if THREADING_SCHEME==10
#warning direct threading scheme 10: plain (no attempt at scheduling)
#  define CFA_NEXT
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({cfa=*ip++; goto *cfa;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#endif

/* direct threaded */
#else
/* indirect THREADED  */

#if THREADING_SCHEME==1
#warning indirect threading scheme 1: autoinc, long latency, cisc
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip++;})
#  define IP		(ip-1)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({goto **cfa;})
#  define EXEC(XT)	({cfa=(XT); goto **cfa;})
#endif

#if THREADING_SCHEME==2
#warning indirect threading scheme 2: autoinc, long latency
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip++;})
#  define IP		(ip-1)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ca=*cfa;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({DEF_CA cfa=(XT); ca=*cfa; goto *ca;})
#endif


#if THREADING_SCHEME==3
#warning indirect threading scheme 3: autoinc, low latency, cisc
#  define CFA_NEXT
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({cfa=*ip++; goto **cfa;})
#  define EXEC(XT)	({cfa=(XT); goto **cfa;})
#endif

#if THREADING_SCHEME==4
#warning indirect threading scheme 4: autoinc, low latency
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip++;})
#  define IP		(ip-1)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ca=*cfa;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({DEF_CA cfa=(XT); ca=*cfa; goto *ca;})
#endif


#if THREADING_SCHEME==5
#warning indirect threading scheme 5: long latency, cisc
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto **cfa;})
#  define EXEC(XT)	({cfa=(XT); goto **cfa;})
#endif

#if THREADING_SCHEME==6
#warning indirect threading scheme 6: long latency
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ip++; ca=*cfa;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({DEF_CA cfa=(XT); ca=*cfa; goto *ca;})
#endif

#if THREADING_SCHEME==7
#warning indirect threading scheme 7: low latency
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ip++; ca=*cfa;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({DEF_CA cfa=(XT); ca=*cfa; goto *ca;})
#endif

#if THREADING_SCHEME==8
#warning indirect threading scheme 8: low latency,cisc
#  define CFA_NEXT
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({cfa=*ip++; goto **cfa;})
#  define EXEC(XT)	({cfa=(XT); goto **cfa;})
#endif

/* indirect threaded */
#endif

#endif /* !defined(DOUBLY_INDIRECT) */

#define NEXT ({DEF_CA NEXT_P1; NEXT_P2;})

