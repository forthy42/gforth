\ chains.fs execution chains for gforth			21jun97jaw

0 [IF]
This defines execution chains.
The first application for this is building initialization chains:
Think of many modules or program parts, each of it with some specific
initialization code. If we hardcode the initialization routines into a
"master-init" we will get unflexible and are not able only to load some
specific modules...

The chain is basicaly a linked-list. Define a Variable for the head of
linked-list. Name it "foo8" or "foo-chain" to indicate it is a execution
chain.

You can add a word to the list with "' my-init foo8 chained". You can
execute all the code with "foo8 chainperform".
[THEN]

has? cross 
[IF]   e? compiler
[ELSE] true
[THEN]

[IF] \ only needed with compiler

[IFUNDEF] linked
: linked        here over @ a, swap ! ;
[THEN]

\ generic chains

: chained 	( xt list -- ) \ gforth
  linked , ;

[THEN]

: chainperform	( list -- ) \ gforth
  BEGIN @ dup WHILE dup cell+ perform REPEAT drop ;

