/* This file defines a number of threading schemes.
   To organize the former ifdef chaos, each path is separated
   This gives a quite impressive number of paths, but you clearly
   find things that go together. */

#ifndef GETCFA
#  define CFA_NEXT
#endif

#if defined(DIRECT_THREADED) && defined(AUTO_INCREMENT)\
    && defined(LONG_LATENCY) && defined(CFA_NEXT)
#warning scheme 1
#  define NEXT_P0	({cfa=*ip++;})
#  define IP		(ip-1)
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({goto *cfa;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#endif

#if defined(DIRECT_THREADED) && defined(AUTO_INCREMENT)\
    && defined(LONG_LATENCY) && !defined(CFA_NEXT)
#warning scheme 2
#  define NEXT_P0	(ip++)
#  define IP		(ip-1)
#  define NEXT_INST	(*(ip-1))
#  define INC_IP(const_inc)	({ ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({goto **(ip-1);})
#  define EXEC(XT)	({goto *(XT);})
#endif


#if defined(DIRECT_THREADED) && defined(AUTO_INCREMENT)\
    && !defined(LONG_LATENCY) && defined(CFA_NEXT)
#warning scheme 3
#  define NEXT_P0
#  define IP		(ip)
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	({cfa=*ip++;})
#  define NEXT_P2	({goto *cfa;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#endif

#if defined(DIRECT_THREADED) && defined(AUTO_INCREMENT)\
    && !defined(LONG_LATENCY) && !defined(CFA_NEXT)
#warning scheme 4
#  define NEXT_P0
#  define IP		(ip)
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({goto **(ip++);})
#  define EXEC(XT)	({goto *(XT);})
#endif

/* without autoincrement */

#if defined(DIRECT_THREADED) && !defined(AUTO_INCREMENT)\
    && defined(LONG_LATENCY) && defined(CFA_NEXT)
#warning scheme 5
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto *cfa;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#endif

#if defined(DIRECT_THREADED) && !defined(AUTO_INCREMENT)\
    && defined(LONG_LATENCY) && !defined(CFA_NEXT)
#warning scheme 6
#  define NEXT_P0
#  define IP		(ip)
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	(ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto **(ip-1);})
#  define EXEC(XT)	({goto *(XT);})
#endif


#if defined(DIRECT_THREADED) && !defined(AUTO_INCREMENT)\
    && !defined(LONG_LATENCY) && defined(CFA_NEXT)
#warning scheme 7
#  define NEXT_P0
#  define IP		(ip)
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	({cfa=*ip++;})
#  define NEXT_P2	({goto *cfa;})
#  define EXEC(XT)	({cfa=(XT); goto *cfa;})
#endif

#if defined(DIRECT_THREADED) && !defined(AUTO_INCREMENT)\
    && !defined(LONG_LATENCY) && !defined(CFA_NEXT)
#warning scheme 8
#  define NEXT_P0
#  define IP		(ip)
#  define NEXT_INST	(*IP)
#  define INC_IP(const_inc)	({ ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto **(ip-1);})
#  define EXEC(XT)	({goto *(XT);})
#endif

/* common settings for direct THREADED */


/* indirect THREADED  */

#if !defined(DIRECT_THREADED) && defined(AUTO_INCREMENT)\
    && defined(LONG_LATENCY) && defined(CISC_NEXT)
#  define NEXT_P0	({cfa=*ip++;})
#  define IP		(ip-1)
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1
#  define NEXT_P2	({goto **cfa;})
#  define EXEC(XT)	({cfa=(XT); goto **cfa;})
#endif

#if !defined(DIRECT_THREADED) && defined(AUTO_INCREMENT)\
    && defined(LONG_LATENCY) && !defined(CISC_NEXT)
#  define NEXT_P0	({cfa=*ip++;})
#  define IP		(ip-1)
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ca=*cfa;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({DEF_CA cfa=(XT); ca=*cfa; goto *ca;})
#endif


#if !defined(DIRECT_THREADED) && defined(AUTO_INCREMENT)\
    && !defined(LONG_LATENCY) && defined(CISC_NEXT)
#  define NEXT_P0
#  define IP		(ip)
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	({cfa=*ip++;})
#  define NEXT_P2	({goto **cfa;})
#  define EXEC(XT)	({cfa=(XT); goto **cfa;})
#endif

#if !defined(DIRECT_THREADED) && defined(AUTO_INCREMENT)\
    && !defined(LONG_LATENCY) && !defined(CISC_NEXT)
#  define NEXT_P0	({cfa=*ip++;})
#  define IP		(ip-1)
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ca=*cfa;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({DEF_CA cfa=(XT); ca=*cfa; goto *ca;})
#endif


/* without autoincrement */

#if !defined(DIRECT_THREADED) && !defined(AUTO_INCREMENT)\
    && defined(LONG_LATENCY) && defined(CISC_NEXT)
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto **cfa;})
#  define EXEC(XT)	({cfa=(XT); goto **cfa;})
#endif

#if !defined(DIRECT_THREADED) && !defined(AUTO_INCREMENT)\
    && defined(LONG_LATENCY) && !defined(CISC_NEXT)
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ip++; ca=*cfa;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({DEF_CA cfa=(XT); ca=*cfa; goto *ca;})
#endif


#if !defined(DIRECT_THREADED) && !defined(AUTO_INCREMENT)\
    && !defined(LONG_LATENCY) && defined(CISC_NEXT)
#  define NEXT_P0
#  define IP		(ip)
#  define NEXT_INST	(*ip)
#  define INC_IP(const_inc)	({ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	({cfa=*ip;})
#  define NEXT_P2	({ip++; goto **cfa;})
#  define EXEC(XT)	({cfa=(XT); goto **cfa;})
#endif

#if !defined(DIRECT_THREADED) && !defined(AUTO_INCREMENT)\
    && !defined(LONG_LATENCY) && !defined(CISC_NEXT)
#  define NEXT_P0	({cfa=*ip;})
#  define IP		(ip)
#  define NEXT_INST	(cfa)
#  define INC_IP(const_inc)	({cfa=IP[const_inc]; ip+=(const_inc);})
#  define DEF_CA	Label ca;
#  define NEXT_P1	({ip++; ca=*cfa;})
#  define NEXT_P2	({goto *ca;})
#  define EXEC(XT)	({DEF_CA cfa=(XT); ca=*cfa; goto *ca;})
#endif

#define NEXT ({DEF_CA NEXT_P1; NEXT_P2;})

#if defined(CISC_NEXT) && !defined(LONG_LATENCY)
# define NEXT1_P1
# ifdef DIRECT_THREADED
#  define NEXT1_P2 ({goto *cfa;})
# else
#  define NEXT1_P2 ({goto **cfa;})
# endif /* DIRECT_THREADED */
#else /* defined(CISC_NEXT) && !defined(LONG_LATENCY) */
# ifdef DIRECT_THREADED
#  define NEXT1_P1
#  define NEXT1_P2 ({goto *cfa;})
# else /* DIRECT_THREADED */
#  define NEXT1_P1 ({ca = *cfa;})
#  define NEXT1_P2 ({goto *ca;})
# endif /* DIRECT_THREADED */
#endif
