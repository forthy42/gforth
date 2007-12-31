/* VM profiling support stuff

  Copyright (C) 2001,2002,2003,2007 Free Software Foundation, Inc.

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
#include <stdlib.h>
#include <stdio.h>
#include <assert.h>


/* data structure: simple hash table with external chaining */

#define HASH_SIZE (1<<20)

typedef struct block_count {
  struct block_count *next; /* next in hash table */
  struct block_count *fallthrough; /* the block that this one falls
                                       through to without SUPER_END */
  Xt *ip;
  long long count;
  char **insts;
  size_t ninsts;
} block_count;

block_count *blocks[HASH_SIZE];

#define hash(p) ((((Cell)(p))/sizeof(Xt))&(HASH_SIZE-1))

block_count *block_lookup(Xt *ip)
{
  block_count *b = blocks[hash(ip)];

  while (b!=NULL && b->ip!=ip)
    b = b->next;
  return b;
}

/* looks up present elements, inserts absent elements */
block_count *block_insert(Xt *ip)
{ 
  block_count *b = block_lookup(ip);
  block_count *new;

  if (b != NULL)
    return b;
  new = (block_count *)malloc(sizeof(block_count));
  new->next = blocks[hash(ip)];
  new->fallthrough = NULL;
  new->ip = ip;
  new->count = 0LL;
  new->insts = malloc(0);
  assert(new->insts != NULL);
  new->ninsts = 0;
  blocks[hash(ip)] = new;
  return new;
}

void add_inst(block_count *b, char *inst)
{
  b->insts = realloc(b->insts, (b->ninsts+1) * sizeof(char *));
  b->insts[b->ninsts++] = inst;
}

void vm_count_block(Xt *ip)
{
  block_insert(ip)->count++;
}

#ifdef DIRECT_THREADED
#define VM_IS_INST(inst, n) ((inst) == vm_prims[n])
#else
#define VM_IS_INST(inst, n) ((inst) == &(vm_prims[n]))
#endif

void postprocess_block(block_count *b)
{
  Xt *ip = b->ip;
  block_count *next_block;

  do {
#include "profile.i"
    /* else */
    {
      add_inst(b,"unknown");
      ip++;
    }
  _endif_:
    next_block = block_lookup(ip);
  } while (next_block == NULL);
  /* we fell through, so set fallthrough and update the count */
  b->fallthrough = next_block;
  /* also update the counts of all following fallthrough blocks that
     have already been processed */
  while (next_block != NULL) {
    next_block->count += b->count;
    next_block = next_block->fallthrough;
  }
}

/* Deal with block entry by falling through from non-SUPER_END
   instructions.  And fill the insts and ninsts fields. */
void postprocess(void)
{
  size_t i;

  for (i=0; i<HASH_SIZE; i++) {
    block_count *b = blocks[i];
    for (; b!=0; b = b->next)
      postprocess_block(b);
   }
}

#if 1
/* full basic blocks only */
void print_block(FILE *file, block_count *b)
{
  size_t i;

  fprintf(file,"%14lld\t",b->count);
  for (i=0; i<b->ninsts; i++)
    fprintf(file, "%s ", b->insts[i]);
  putc('\n', file);
}
#elif 0
/* full basic blocks and all their prefixes */
void print_block(FILE *file, block_count *b)
{
  size_t i,j;

  for (j=1; j<=b->ninsts; j++) {
    fprintf(file,"%14lld\t",b->count);
    for (i=0; i<j; i++)
      fprintf(file, "%s ", b->insts[i]);
    putc('\n', file);
  }
}
#else
/* all subsequences up to length 12 */
void print_block(FILE *file, block_count *b)
{
  size_t i,j,k;

  for (i=1; i<2; i++)
    for (j=0; i+j<=b->ninsts; j++) {
      fprintf(file,"%14lld\t",b->count);
      for (k=j; k<i+j; k++)
	fprintf(file, "%s ", b->insts[k]);
      putc('\n', file);
    }
}
#endif

void vm_print_profile(FILE *file)
{
  size_t i;

  postprocess();
  for (i=0; i<HASH_SIZE; i++) {
    block_count *b = blocks[i];
    for (; b!=0; b = b->next)
      print_block(file, b);
   }
}
