/* Peephole optimization routines and tables

  Copyright (C) 2001 Free Software Foundation, Inc.

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
  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.
*/

#include "config.h"
#include "forth.h"
#include <stdlib.h>
#include <string.h>
#include <assert.h>

/* the numbers in this struct are primitive indices */
typedef struct Combination {
  int prefix;
  int lastprim;
  int combination_prim;
} Combination;

Combination peephole_table[] = {
#include "peephole.i"
};

int use_super = 1;

typedef Xt Inst;

typedef struct Peeptable_entry {
  struct Peeptable_entry *next;
  Inst prefix;
  Inst lastprim;
  Inst combination_prim;
} Peeptable_entry;

#define HASH_SIZE 1024
#define hash(_i1,_i2) (((((Cell)(_i1))^((Cell)(_i2)))>>4)&(HASH_SIZE-1))

Cell peeptable;

Cell prepare_peephole_table(Inst insts[])
{
  Cell i;
  Peeptable_entry **pt = (Peeptable_entry **)calloc(HASH_SIZE,sizeof(Peeptable_entry *));

  for (i=0; i<sizeof(peephole_table)/sizeof(peephole_table[0]); i++) {
    Combination *c = &peephole_table[i];
    Peeptable_entry *p = (Peeptable_entry *)malloc(sizeof(Peeptable_entry));
    Cell h;
    p->prefix =           insts[c->prefix];
    p->lastprim =         insts[c->lastprim];
    p->combination_prim = insts[c->combination_prim];
    h = hash(p->prefix,p->lastprim);
    p->next = pt[h];
    pt[h] = p;
  }
  return (Cell)pt;
}

Inst peephole_opt(Inst inst1, Inst inst2, Cell peeptable)
{
  Peeptable_entry **pt = (Peeptable_entry **)peeptable;
  Peeptable_entry *p;

  if (use_super == 0)
      return 0;
  for (p = pt[hash(inst1,inst2)]; p != NULL; p = p->next)
    if (inst1 == p->prefix && inst2 == p->lastprim)
      return p->combination_prim;
  return NULL;
}


/* hashtable stuff (again) */

#undef hash

typedef int (*Pred1) (void* arg1);
typedef int (*Pred2) (void* arg1, void* arg2);
typedef void* (*Proc1) (void* arg1);
typedef void* (*Proc2) (void* arg1, void* arg2);
typedef void* (*Proc3) (void* arg1, void* arg2, void* arg3);
typedef int (*Key_Eq) (void* key1, void* key2);
typedef unsigned (*Key_Hash) (void* key, unsigned modulus);

typedef struct hash_table *Hash_Table;

Hash_Table make_hash_table (unsigned size, Key_Eq eq, Key_Hash hash);
void* hash_table_ref (Hash_Table table, void* key, void* deflt);
void hash_table_set (Hash_Table table, void *key, void* value);
void* hash_table_find_value (Hash_Table table, Pred1 p, void* deflt);
void* hash_table_with_env_find_value (Hash_Table table, 
				      Pred2 p, void* env,
				      void* deflt);
void* hash_table_fold (Hash_Table table, Proc3 proc, void *knil);
void hash_table_print (Hash_Table table);
int hash_table_addr_eq (void* key1, void* key2);
unsigned hash_table_addr_hash (void* key, unsigned modulus);
int hash_table_string_eq (char* k1, char* k2);
unsigned hash_table_string_hash (char* key, unsigned modulus);

#include <stdlib.h>
#include <assert.h>

typedef struct hash_bucket {
  void* key;
  void* value;
  struct hash_bucket* next;
} *Hash_Bucket;

struct hash_table {
  unsigned size;
  Hash_Bucket* buckets;
  Key_Eq eq;
  Key_Hash hash;
};

Hash_Table make_hash_table (unsigned size, Key_Eq eq, Key_Hash hash) {
	Hash_Table tab = malloc (sizeof *tab);
	Hash_Bucket* buckets = calloc (size, sizeof *buckets);
	assert (tab);
	assert (buckets);
	tab->size = size;
	tab->buckets = buckets;
	tab->eq = eq;
	tab->hash = hash;
	return tab;
}

static void* hash_bucket_search (Hash_Bucket* next, Pred1 p, 
				 Proc1 found, Proc1 not_found) {
	for (;;) {
		Hash_Bucket bucket = *next;
		if (bucket == 0) return not_found (next);
		else if (p (bucket->key)) return found (next);
		else { next = &(bucket->next); continue; }
	}
}

static void* hash_table_search (Hash_Table table, void* key, Key_Eq eq,
				Proc1 found, Proc1 not_found) {
	int pred (void* key2) { return eq (key, key2); }
 	unsigned idx= table->hash(key, table->size);
	assert (idx < table->size);
	return hash_bucket_search (table->buckets+idx, pred, found, not_found);
}

void* hash_table_ref (Hash_Table table, void* key, void* deflt) {
	void* found (Hash_Bucket* next) { return (*next)->value; }
	void* not_found (Hash_Bucket* next) { return deflt; }
	return hash_table_search (table, key, table->eq, 
				  (Proc1)found, (Proc1)not_found);
}

void hash_table_set (Hash_Table table, void *key, void* value) {
	void* found (Hash_Bucket* next) {
		Hash_Bucket bucket = *next;
		bucket->key = key;
		bucket->value = value;
		return value;
	}
	void* not_found (Hash_Bucket* next) {
		Hash_Bucket bucket = malloc (sizeof (struct hash_bucket));
		assert (bucket);
		bucket->key = key;
		bucket->value = value;
		bucket->next = *next;
		*next = bucket;
		return value;
	}
	hash_table_search (table, key, table->eq, 
			   (Proc1)found,(Proc1)not_found);
}

void* hash_table_find_value (Hash_Table table, Pred1 p, void* deflt) {
	unsigned i = 0;
	for (;;) 
	    if (i == table->size) return deflt;
	    else { 
		    Hash_Bucket bucket = table->buckets[i];
		    for (;;)
			if (bucket == 0) { i++; break; }
			else if (p (bucket->value)) return bucket->value;
			else { bucket = bucket->next; continue; }
	    }
}

void* hash_table_with_env_find_value (Hash_Table table, 
				      Pred2 p, void* env,
				      void* deflt) {
	int pred (void* val) { return p (env, val); }
	return hash_table_find_value (table, pred, deflt);
}


static void* hash_bucket_fold (Hash_Bucket bucket, Proc3 proc, void *init) {
	for (;;)
	    if (bucket == 0) return init;
	    else { 
		    init = proc (bucket->key, bucket->value, init);
		    bucket = bucket->next; continue;
	    }
}

void* hash_table_fold (Hash_Table table, Proc3 proc, void *init) {
	unsigned i = 0, size = table->size;
	Hash_Bucket* buckets = table->buckets;
	for (;;) 
	    if (i == size) return init;
	    else { 
		    init = hash_bucket_fold (buckets [i], proc, init);
		    i++; continue;
	    }
}

void hash_table_print (Hash_Table table) {
	void* print (void* key, void* value, void* init) {
		printf ("[%p, %p]\n", key, value);
		return init;
	}
	hash_table_fold (table, print, 0);
}

int hash_table_addr_eq (void* key1, void* key2) {
	return key1 == key2;
}

unsigned hash_table_addr_hash (void* key, unsigned modulus) {
	return (((unsigned long)key) >> 3) % modulus;
}

int hash_table_string_eq (char* k1, char* k2) {
	return strcmp (k1, k2) == 0;
}

unsigned hash_table_string_hash (char* key, unsigned modulus) {
	unsigned sum = 0;
	for (;;)
	    if (*key == 0) return (sum >> 3) % modulus;
	    else { sum += *key; key++; continue; }
}

/* 

Select super instructions with dynamic programming.

A buffer `dp_table' stores an instruction sequence, i.e. a basic
block.  Instructions can be added with `peephole_dp_append'.
`peephole_dp_append' takes the Prim_Descriptor of the operator and a
pointer to a gap where to place the super-instruction, i.e. the
current `here' pointer.

The actual instructions are copied into the gaps by calling
`peephole_dp_flush'.  This selects an optimal cover for the stored
sequence, copies the corresponding XTs into the gaps, moves inline
arguments (this may be required, since some gaps may not be filled
with instructions), and finally flushes the buffer `dp_table'.

The rest of the system must cooperate, i.e. has to call
`peephole_dp_flush' in the right places, e.g. before taking labels.
Non-cooperation will be punished with crashes.

*/

/*  #define XDP_DEBUG(stm) stm; */
#define XDP_DEBUG(stm) 

#define dprintf(format, args...) XDP_DEBUG(fprintf (stderr, format , ## args))

typedef struct dp_table_entry {
  Prim_Descriptor op;		/* NULL marks the end. */
  Cell* mark;			/* the gap. */ 
  struct burm_state* state;	/* iburg needs this. */
/*    unsigned state; */
}* Dp_Table_Entry;

#define DP_TABLE_SIZE 25
static struct dp_table_entry dp_table[DP_TABLE_SIZE+1];
static unsigned dp_table_len = 0;

static int dp_table_end_p (Dp_Table_Entry entry) { return entry->op == 0; }
static void dp_table_clear (void) {
	dp_table_len = 0;
	dp_table[0].op = 0;
/*  	dprintf ("dp_table_clear\n"); */
}
static void dump_dp_table (void) {
	Dp_Table_Entry e = dp_table;
	for (;;)
	    if (dp_table_end_p (e)) return;
	    else { 
		    printf ("[%s, %p]\n", e->op->name, e->mark);
		    e++; continue;
	    }
}

Cell* peephole_dp_flush (Cell* mark);
 
Cell* peephole_dp_append (Prim_Descriptor desc, Cell* mark) {
	if (dp_table_len == DP_TABLE_SIZE) {
		dp_table[dp_table_len].op = 0;
		mark = peephole_dp_flush (mark);
	}
	dp_table[dp_table_len].op = desc;
	assert (dp_table_len == 0 ||
		dp_table[dp_table_len-1].mark < mark); 
	dp_table[dp_table_len].mark = mark;
	dp_table_len ++;
	dp_table[dp_table_len].op = 0;
	return mark+1;
}

/* burg stuff */

#define NIL_TERM_NUM 1
typedef struct dp_table_entry* NODEPTR_TYPE; 
#define OP_LABEL(p)	(((p)->op ? (p)->op->num+2 : NIL_TERM_NUM))
#define STATE_LABEL(p)	((p)->state)
#define LEFT_CHILD(p)	((p)+1)
#define RIGHT_CHILD(p)	((NODEPTR_TYPE)(assert (0), 0L))
#define PANIC		dprintf

#include "prim_burm.i"

static Label* peephole_symbols = 0;
static Xt desc_to_xt (Prim_Descriptor desc) {
	static Xt* primtab = 0;
	if (primtab == 0) {
	  Label* symbols = peephole_symbols;
	  unsigned symbols_size = DOESJUMP+1;
	  assert (peephole_symbols);
	  for (;;)
	      if (symbols[symbols_size] == 0) break;
	      else { symbols_size ++; continue; }
	  primtab = primtable(symbols+DOESJUMP+1,
			      symbols_size-DOESJUMP-1);
	}
	return primtab[desc->num];
}


static Prim_Descriptor peephole_descs = 0;
static Prim_Descriptor external_rule_number_to_desc (int eruleno) {
	assert (peephole_descs);
	return &peephole_descs[eruleno-NIL_TERM_NUM-1];
}

static void move_args_left (Dp_Table_Entry entry, unsigned n, unsigned offset){
	if (n == 0) return;
	else {
		Cell* from = entry->mark+1;
		size_t size = entry[1].mark - from;
		memmove (from-offset, from, size * sizeof *from);
		move_args_left (entry+1, n-1, offset+1);
	}
}

     
static unsigned reduce (Dp_Table_Entry entry, int goalnt, unsigned saved) {
	if (dp_table_end_p (entry)) 
	    return saved;
	else { 
		int eruleno = burm_rule (STATE_LABEL(entry), goalnt);
		short *nts = burm_nts[eruleno];      
		NODEPTR_TYPE kids[2];
		Prim_Descriptor desc = external_rule_number_to_desc (eruleno);
		Label* xt = (Label*)desc_to_xt (desc);;
		dprintf ("burm_string = %s, desc = %s, xt = %p\n",
			 burm_string[eruleno], desc->name, xt);
		*(entry->mark-saved) = (Cell)xt;
		move_args_left (entry, desc->len, saved);
		burm_kids (entry, eruleno, kids);
		return reduce (kids[0], nts[0], saved+desc->len-1);
	}
}


static void dump_match (NODEPTR_TYPE p, int goalnt, int indent) {
	int eruleno = burm_rule (STATE_LABEL(p), goalnt);
	short *nts = burm_nts[eruleno];      
	NODEPTR_TYPE kids[2];                     
	int i;
  	for (i = 0; i < indent; i++) printf (" ");
	printf ("%s [%d]\n", burm_string[eruleno], eruleno);
	burm_kids(p, eruleno, kids);
	for (i = 0; nts[i]; i++) 
	    dump_match (kids[i], nts[i], indent+1); 
}

#ifdef PIUMARTA

/* Return the size (in bytes) for the superinstrcution from first to
   (exluding) last. */
static unsigned 
super_code_size (Dp_Table_Entry first, Dp_Table_Entry last, unsigned length) {
	if (first == last)
	    return length;
	else if (first+1 == last)
	    return length + (first->op->after_next - first->op->after_trace);
	else
	    return super_code_size (first+1,
				    last,
				    length + (first->op->before_next
					      - first->op->after_trace));
}

static void
concat_code (Dp_Table_Entry first, Dp_Table_Entry last, char* code) {
	if (first == last)
	    return;
	else if (first+1 == last)
	    memcpy (code, first->op->after_trace,
		    first->op->after_next - first->op->after_trace);
	else {
		unsigned size = first->op->before_next-first->op->after_trace;
		memcpy (code, first->op->after_trace, size);
		concat_code (first+1, last, code+size);
	}
}

static void apply_icache_magic (char* addr, unsigned size) {
	FLUSH_ICACHE(addr, size);
}

static void print_from_to (Dp_Table_Entry first, Dp_Table_Entry last) {
	if (first == last) printf ("\n");
	else {
		printf ("[%s]", first->op->name);
		print_from_to (first+1, last);
	}
}

static Prim_Descriptor search_desc (char *name) {
	Prim_Descriptor d = peephole_descs;
	for (;;)
	    if (d->name == 0) assert (0);
	    else if (strcmp (d->name, name) == 0) return d;
	    else { d++; continue; }
}

static Xt get_trace_xt (void) {
	static Xt trace_xt = 0;
	if (trace_xt == 0) trace_xt = desc_to_xt (search_desc ("noop"));
	return trace_xt;
}

static unsigned 
piumarta_gen_simple_inst (Dp_Table_Entry first, unsigned saved) {
	Label* xt = (Label*)desc_to_xt (first->op);;
	*(first->mark-saved) = (Cell)xt;
	move_args_left (first, 1, saved);
	return saved;
}


#if 0 
static void alloc_xt (unsigned size, Xt *xt, char** code) {
#    if defined(DIRECT_THREADED)
	*xt = malloc (size);
	assert (*xt);
	*code = *xt;
	return ;
#    elsif DOUBLY_INDIRECT
	/* this is never reached.  blocked in comp.fs. */
	assert (0); return;
#    else
#	warning Assuming indirect threaded
	Xt* xtb = malloc (size + sizeof xt);
	assert (xtb);
	xtb[0] = (Xt)&xtb[1];
	*xt = (Xt)xtb;
	*code = (char*)&xtb[1];
	return;
#    endif
}
#else 

static Xt make_xt (char* code) {
#    if defined(DIRECT_THREADED)
	return code;
#    elif defined(DOUBLY_INDIRECT)
	/* this is never reached.  blocked in comp.fs. */
	assert (0); return 0;
#    else
#	warning Assuming indirect threaded
	Xt xt = malloc (sizeof xt);
	assert (xt);
	*xt = code;
	return xt;
#    endif
}
static void alloc_xt (unsigned size, Xt *xt, char** code) {
	*code = malloc (size);
	assert (code);
	*xt = make_xt (*code);
}

#endif



int peephole_enable_tracing = 0;
typedef struct sequence {
  unsigned length;
  Dp_Table_Entry start;
}* Sequence;

static int sequence_eq (Sequence s1, Sequence s2) {
	Dp_Table_Entry e1=s1->start, e2=s2->start, last=e1+s1->length;
	XDP_DEBUG ({
		print_from_to (e1, last);
		print_from_to (e2, e2+s2->length); });
	if (s1->length != s2->length) return 0;
	for (;;) 
	    if (e1 == last) return 1;
	    else if (e1->op == e2->op) { e1++; e2++; continue; }
	    else return 0;
}

static unsigned sequence_hash (Sequence s, unsigned modulus) {
	unsigned long h=0;
	Dp_Table_Entry e=s->start, last=e+s->length;
	for (;;)
	    if (e == last) return (h>>3) % modulus;
	    else { h+=(unsigned long)e->op; e++; continue; }
}

static Hash_Table generated_insts = 0;
typedef Dp_Table_Entry Entry;

static Xt lookup (Entry first, Entry last) {
	unsigned len = last-first;
	struct sequence seq = { len, first };
	return hash_table_ref (generated_insts, &seq, 0);
}

static void memoize (Entry first, Entry last, Xt xt) {
	Sequence seq = malloc (sizeof *seq);
	unsigned size = (last-first) * sizeof *first;
	Dp_Table_Entry start = malloc (size);
	assert (seq); assert (start);
	memcpy (start, first, size);
	seq->length = last-first;
	seq->start = start;
	XDP_DEBUG(print_from_to (start, start+(last-first)));
	hash_table_set (generated_insts, seq, xt);
	assert (lookup (first, last) == xt);
}

static Xt combine_insts (Entry first, Entry last) {
	Xt xt = lookup (first, last);
	if (xt == 0) { 
		unsigned size = super_code_size (first, last, 0);
		char* code = 0;
		alloc_xt (size, &xt, &code);
		concat_code (first, last, code);
		apply_icache_magic (code, size);
		memoize (first, last, xt);
		return xt;
	} 
	else { dprintf ("reusing combination.\n"); return xt; }
}

static unsigned
piumarta_gen_combined_inst (Entry first, Entry last, unsigned saved) {
	Xt xt = combine_insts (first, last);
	unsigned len = last-first;
	unsigned offset = saved-(len-1);
	dprintf ("len = %d, saved = %d\n", len, saved);
	if (!peephole_enable_tracing) {
		move_args_left (first, len, offset);
		*(first->mark-offset) = (Cell)xt;
		return saved;
	} else {
		/** hairy: Move operands for the first op 1 slot to
                    right (possible because len>=2).  Move operands of
                    the remaining ops offset-1 slots to left (and
                    eliminate empty slots). */
		Cell* from = first->mark+1;
		Cell* to = from-offset+1;
		size_t size = first[1].mark-from;
		memmove (to, from, size * sizeof *from);
		move_args_left (first+1, len-1, offset);
		*(to-1) = (Cell)xt;
		*(to-2) = (Cell)get_trace_xt (); 
		return saved-1;
	}
}

static unsigned piumarta_gen_inst (Entry first, Entry last, unsigned saved) {
	XDP_DEBUG (print_from_to (first, last));
	switch (last-first) {
	case 0: return saved;
	case 1: return piumarta_gen_simple_inst (first, saved);
	default: return piumarta_gen_combined_inst (first, last, saved);
	}
}

static unsigned 
piumartaize (Dp_Table_Entry lag, Dp_Table_Entry tail, unsigned saved) {
	assert (tail >= lag);
	if (dp_table_end_p (tail) && tail == lag) return saved;
	else if (dp_table_end_p (tail)) {
		return piumarta_gen_inst (lag, tail, saved);
	}
	else if (tail->op->relocatable)
	    return piumartaize (lag, tail+1, saved+((tail==lag)?0:1));
	else {
		saved = piumarta_gen_inst (lag, tail, saved);
		saved = piumarta_gen_inst (tail, tail+1, saved);
		return piumartaize (tail+1, tail+1, saved);
	}
}

#endif /* PIUMARTA */

Cell* peephole_dp_flush (Cell* mark) {
	assert (dp_table[dp_table_len].op == 0);
	assert (dp_table_len == 0 ||
		dp_table[dp_table_len-1].mark < mark ); 
	dp_table[dp_table_len].mark = mark;
  	XDP_DEBUG(dump_dp_table ());
#ifndef PIUMARTA
	burm_label (dp_table);
	XDP_DEBUG(dump_match (dp_table, 1, 0));
	{
		unsigned saved = reduce (dp_table, 1, 0);
		dp_table_clear ();
		return mark-saved;
	}
#else
	{
		unsigned saved = piumartaize (dp_table, dp_table, 0);
		dp_table_clear ();
		return mark-saved;
	}
#endif 
}


static Hash_Table ca_table = 0;
Prim_Descriptor peephole_code_address_to_desc (void* ca) {
	assert (ca_table);
	return hash_table_ref (ca_table, ca, 0);
}

static void init_ca_table (Prim_Descriptor descs) {
	unsigned len = 0;
	Prim_Descriptor d = descs;
	for (;;)
	    if (d->name == 0) break;
	    else { len++; d++; continue; }
	ca_table = make_hash_table (len, hash_table_addr_eq,
				    hash_table_addr_hash);
	d=descs;
	for (;;)
	    if (d->name == 0) break;
	    else { hash_table_set (ca_table, d->start, d); d++; continue; }
}

void peephole_init (Label* symbols, Prim_Descriptor descs) {
	peephole_symbols = symbols;
	peephole_descs = descs;
	init_ca_table (descs);
	generated_insts = make_hash_table (2000,
					   (Key_Eq)sequence_eq,
					   (Key_Hash)sequence_hash);
}

