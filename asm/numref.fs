\ refs.fs

0 [IF]

This is a generic solution for doing labels (forward and backward
references) in an assembler program.

- Who to use local labels:

Example:

Label 10pause
		10 # ldy,
	1 $:	dey,
		1 $ bne,
		rts,
End-Label

"n $:" defines an address reference. "n $" returns the address of the
reference defined with "n $:".

- How to embed local labels in your assembler:

At the moment all references are forward references, meaning,
all references are resolved at the end of the definition.

The Simple Resolver

The only special thing is how a label is resolved. Numref executes
therefor a resolver-word, example for a two byte opcode with the second
byte as branch-offset:

: doresovle ( iaddr -- )
  dup ref-addr @ - swap 1+ X c! ;

iaddr is the address of the instruction with the reference that must
be resolved. The destination address of the reference is stored at ref-addr.

The resolver must be registered bye "' doresolve TO std-resolve". This is 
not a defered word!

Complex Resolving

To support different cpu-instruction with different operand formats it is
possible to find out the type of opcode bye accessing the targets' memory
in doresolve. This works for very simple processors, e.g. for 6502 it is
very easy to find out whether we have a 2-byte absolute address or a 1-byte
relative address.

If this method is to difficult, it is possible to store additional
information in the resolve structure.

When assembling an opcode you should find out whether the address is a
reference and then  store the xt of a special
resolver word in the resolve structure by "ref-resolver !", or store some
additional data in the resolve structure by "ref-data !", if one data field
is not enough allocate memory and use ref-data as pointer to it.

- Internal strucutre:

There is a heap buffer to store the references.
The structure of one entry is:

 1 cell		ref-link
 1 cell		ref-flag	\ mixture of tag-number
				\ and tag type
 1 cell		ref-resolver	\ xt of resolver
 1 cell		ref-addr	\ pointer to destination or on reference
				\ instruction
				\ (start of the instruction)
 1 cell		ref-data	\ additional information for resolver


[THEN]

require ./basic.fs

also assembler definitions

hex

0 value ref-marker \ tells us that address is an reference

0 value ref-now	\ points to the reference we are working on

: ref-link ref-now ;
: ref-flag ref-now cell+ ;
: ref-resolver ref-now 2 cells + ;
: ref-adr ref-now 3 cells + ;
: ref-addr ref-now 3 cells + ;
: ref-data ref-now 4 cells + ;
: ref-tag-len 5 cells ;

: ref-resolve ref-resolver @ execute ;

: ref? 	( -- )
	ref-marker
	false TO ref-marker ;

: forward? ( target-addr -- target-addr false | true )
	dup there = ref? and dup
	IF nip THEN ;

:noname false TO ref-marker ; propper8 chained

variable ref-heap 0 ref-heap !

' drop value std-resolver

: ref! ( flags/nr -- )
\G stores a reference tag
  \ get mem for tag
  ref-tag-len allocate throw to ref-now 
  \ build link
  ref-heap @ ref-link !  ref-link ref-heap !
  there ref-adr !
  std-resolver ref-resolver ! 
  ref-flag ! ;

: $ ( num -- address )
\G makes a reference source with the next instruction
  01ff and 0200 or ref! there  ;

: $: ( num -- )
\G makes a reference target
  01ff and 0a00 or ref! ;

: g$: ( num -- )
\G makes a reference target for a global label
  01ff and 0e00 or ref! ;
  
: g$ ( num -- addr )
\G searches a global label and gets its address
  01ff and 0e00 or
  ref-heap BEGIN dup >r @ dup WHILE 2dup cell+ @ = 
  IF nip to ref-now
     ref-link @ r> !
     ref-adr @ 
     ref-now free throw EXIT THEN
     r> drop
  REPEAT 2drop -1 ABORT" could not resolve G label!" ;

: kill$: ( -- )
\G deallocs the complete reference heap
  ref-heap @ BEGIN dup WHILE dup @ swap free throw REPEAT drop 
  0 ref-heap ! ;

: find$: ( adr nr -- )
  0800 or
  ref-heap 
  BEGIN dup >r @ dup WHILE 2dup cell+ @ =
  	IF nip to ref-now
     	   r> drop
           ref-resolve EXIT
	THEN
     r> drop
  REPEAT 2drop -1 ABORT" could not resolve label!" ;

: solve$
  ref-heap dup >r @ 
  BEGIN dup WHILE dup cell+ @ 0E00 and 0200 =
   IF to ref-now
      ref-link @ r@ !
      ref-now >r
      ref-adr @ ref-flag @ ( 01ff and ) find$:
      r> to ref-now
      ref-link ( dup >r ) @
      ref-now free throw
   ELSE 
      r> drop
      dup >r @
   THEN
  REPEAT r> drop drop kill$: ;

' solve$ end-code8 chained

previous definitions
