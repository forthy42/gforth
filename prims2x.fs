\ converts primitives to, e.g., C code 

\ Copyright (C) 1995,1996,1997,1998,2000 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.


\ This is not very nice (hard limits, no checking, assumes 1 chars = 1).
\ And it grew even worse when it aged.

\ Optimizations:
\ superfluous stores are removed. GCC removes the superfluous loads by itself
\ TOS and FTOS can be kept in register( variable)s.
\ 
\ Problems:
\ The TOS optimization is somewhat hairy. The problems by example:
\ 1) dup ( w -- w w ): w=TOS; sp-=1; sp[1]=w; TOS=w;
\    The store is not superfluous although the earlier opt. would think so
\    Alternatively:    sp[0]=TOS; w=TOS; sp-=1; TOS=w;
\ 2) ( -- .. ): sp[0] = TOS; ... /* This additional store is necessary */
\ 3) ( .. -- ): ... TOS = sp[0]; /* as well as this load */
\ 4) ( -- ): /* but here they are unnecessary */
\ 5) Words that call NEXT themselves have to be done very carefully.
\
\ To do:
\ add the store optimization for doubles
\ regarding problem 1 above: It would be better (for over) to implement
\ 	the alternative
\ store optimization for combined instructions.
\ eliminate stack-cast (no longer used)

\ Design Uglyness:

\ - global state (values, variables) in connection with combined instructions.

\ - index computation is different for instruction-stream and the
\ stacks; there are two mechanisms for dealing with that
\ (stack-in-index-xt and a test for stack==instruction-stream); there
\ should be only one.

warnings off

[IFUNDEF] vocabulary	\ we are executed just with kernel image
			\ load the rest that is needed
			\ (require fails because this file is needed from a
			\ different directory with the wordlibraries)
include ./search.fs			
include ./extend.fs
[THEN]
include ./stuff.fs

[IFUNDEF] environment?
include ./environ.fs
[THEN]

: struct% struct ; \ struct is redefined in gray

include ./gray.fs

32 constant max-effect \ number of things on one side of a stack effect
4 constant max-stacks  \ the max. number of stacks (including inst-stream).
255 constant maxchar
maxchar 1+ constant eof-char
#tab constant tab-char
#lf constant nl-char

variable rawinput \ pointer to next character to be scanned
variable endrawinput \ pointer to the end of the input (the char after the last)
variable cookedinput \ pointer to the next char to be parsed
variable line \ line number of char pointed to by input
variable line-start \ pointer to start of current line (for error messages)
0 line !
2variable filename \ filename of original input file
0 0 filename 2!
2variable f-comment
0 0 f-comment 2!
variable skipsynclines \ are sync lines ("#line ...") invisible to the parser?
skipsynclines on 

: th ( addr1 n -- addr2 )
    cells + ;

: holds ( addr u -- )
    \ like HOLD, but for a string
    tuck + swap 0 +do
	1- dup c@ hold
    loop
    drop ;

: start ( -- addr )
 cookedinput @ ;

: end ( addr -- addr u )
 cookedinput @ over - ;

: print-error-line ( -- )
    \ print the current line and position
    line-start @ endrawinput @ over - 2dup nl-char scan drop nip ( start end )
    over - type cr
    line-start @ rawinput @ over - typewhite ." ^" cr ;

: ?print-error { f addr u -- }
    f ?not? if
	outfile-id >r try
	    stderr to outfile-id
	    filename 2@ type ." :" line @ 0 .r ." : " addr u type cr
	    print-error-line
	    0
	recover endtry
	r> to outfile-id throw
	abort
    endif ;

: quote ( -- )
    [char] " emit ;

variable output          \ xt ( -- ) of output word for simple primitives
variable output-combined \ xt ( -- ) of output word for combined primitives

: printprim ( -- )
 output @ execute ;

struct%
    cell%    field stack-number \ the number of this stack
    cell% 2* field stack-pointer \ stackpointer name
    cell%    field stack-type \ name for default type of stack items
    cell% 2* field stack-cast \ cast string for assignments to stack elements
    cell%    field stack-in-index-xt \ ( in-size item -- in-index )
end-struct stack%

struct%
 cell% 2* field item-name   \ name, excluding stack prefixes
 cell%    field item-stack  \ descriptor for the stack used, 0 is default
 cell%    field item-type   \ descriptor for the item type
 cell%    field item-offset \ offset in stack items, 0 for the deepest element
 cell%	  field item-first  \ true if this is the first occurence of the item
end-struct item%

struct%
    cell% 2* field type-c-name
    cell%    field type-stack \ default stack
    cell%    field type-size  \ size of type in stack items
    cell%    field type-fetch \ xt of fetch code generator ( item -- )
    cell%    field type-store \ xt of store code generator ( item -- )
end-struct type%

variable next-stack-number 0 next-stack-number !
create stacks max-stacks cells allot \ array of stacks

: stack-in-index ( in-size item -- in-index )
    item-offset @ - 1- ;

: inst-in-index ( in-size item -- in-index )
    nip dup item-offset @ swap item-type @ type-size @ + 1- ;

: make-stack ( addr-ptr u1 type addr-cast u2 "stack-name" -- )
    create stack% %allot >r
    r@ stacks next-stack-number @ th !
    next-stack-number @ r@ stack-number !  1 next-stack-number +!
    save-mem r@ stack-cast 2!
    r@ stack-type !
    save-mem r@ stack-pointer 2! 
    ['] stack-in-index r> stack-in-index-xt ! ;

\ stack items

: init-item ( addr u addr1 -- )
    \ initialize item at addr1 with name addr u
    \ !! remove stack prefix
    dup item% %size erase
    item-name 2! ;

: map-items { addr end xt -- }
    \ perform xt for all items in array addr...end
    end addr ?do
	i xt execute
    item% %size +loop ;

\ types

: print-type-prefix ( type -- )
    body> >head name>string type ;

\ various variables for storing stuff of one primitive

struct%
    cell% 2* field prim-name
    cell% 2* field prim-wordset
    cell% 2* field prim-c-name
    cell% 2* field prim-doc
    cell% 2* field prim-c-code
    cell% 2* field prim-forth-code
    cell% 2* field prim-stack-string
    cell%    field prim-items-wordlist \ unique items
    item% max-effect * field prim-effect-in
    item% max-effect * field prim-effect-out
    cell%    field prim-effect-in-end
    cell%    field prim-effect-out-end
    cell% max-stacks * field prim-stacks-in  \ number of in items per stack
    cell% max-stacks * field prim-stacks-out \ number of out items per stack
end-struct prim%

: make-prim ( -- prim )
    prim% %alloc { p }
    s" " p prim-doc 2! s" " p prim-forth-code 2! s" " p prim-wordset 2!
    p ;

0 value prim     \ in combined prims either combined or a part
0 value combined \ in combined prims the combined prim
variable in-part \ true if processing a part
 in-part off

1000 constant max-combined
create combined-prims max-combined cells allot
variable num-combined

create current-depth max-stacks cells allot
create max-depth     max-stacks cells allot
create min-depth     max-stacks cells allot

wordlist constant primitives

: create-prim ( prim -- )
    get-current >r
    primitives set-current
    dup prim-name 2@ nextname constant
    r> set-current ;

: stack-in ( stack -- addr )
    \ address of number of stack items in effect in
    stack-number @ cells prim prim-stacks-in + ;

: stack-out ( stack -- addr )
    \ address of number of stack items in effect out
    stack-number @ cells prim prim-stacks-out + ;

\ global vars
variable c-line
2variable c-filename
variable name-line
2variable name-filename
2variable last-name-filename
Variable function-number 0 function-number !

\ a few more set ops

: bit-equivalent ( w1 w2 -- w3 )
 xor invert ;

: complement ( set1 -- set2 )
 empty ['] bit-equivalent binary-set-operation ;

\ stack access stuff

: normal-stack-access ( n stack -- )
    stack-pointer 2@ type
    dup
    if
	." [" 0 .r ." ]"
    else
	drop ." TOS"
    endif ;

\ forward declaration for inst-stream (breaks cycle in definitions)
defer inst-stream-f ( -- stack )

: part-stack-access { n stack -- }
    \ print _<stack><x>, x=inst-stream? n : maxdepth-currentdepth-n-1
    ." _" stack stack-pointer 2@ type
    stack stack-number @ { stack# }
    current-depth stack# th @ n + { access-depth }
    stack inst-stream-f = if
	access-depth
    else
	combined prim-stacks-in stack# th @
	assert( dup max-depth stack# th @ = )
	access-depth - 1-
    endif
    0 .r ;

: stack-access ( n stack -- )
    \ print a stack access at index n of stack
    in-part @ if
	part-stack-access
    else
	normal-stack-access
    endif ;

: item-in-index { item -- n }
    \ n is the index of item (in the in-effect)
    item item-stack @ dup >r stack-in @ ( in-size r:stack )
    item r> stack-in-index-xt @ execute ;

: item-stack-type-name ( item -- addr u )
    item-stack @ stack-type @ type-c-name 2@ ;

: fetch-single ( item -- )
 \ fetch a single stack item from its stack
 >r
 r@ item-name 2@ type
 ."  = vm_" r@ item-stack-type-name type
 ." 2" r@ item-type @ print-type-prefix ." ("
 r@ item-in-index r@ item-stack @ stack-access
 ." );" cr
 rdrop ; 

: fetch-double ( item -- )
 \ fetch a double stack item from its stack
 >r
 ." vm_two"
 r@ item-stack-type-name type ." 2"
 r@ item-type @ print-type-prefix ." ("
 r@ item-name 2@ type ." , "
 r@ item-in-index r@ item-stack @ 2dup ." (Cell)" stack-access
 ." , "                      -1 under+ ." (Cell)" stack-access
 ." );" cr
 rdrop ;

: same-as-in? ( item -- f )
 \ f is true iff the offset and stack of item is the same as on input
 >r
 r@ item-first @ if
     rdrop false exit
 endif
 r@ item-name 2@ prim prim-items-wordlist @ search-wordlist 0= abort" bug"
 execute @
 dup r@ =
 if \ item first appeared in output
   drop false
 else
   dup  item-stack  @ r@ item-stack  @ = 
   swap item-offset @ r@ item-offset @ = and
 endif
 rdrop ;

: item-out-index ( item -- n )
    \ n is the index of item (in the in-effect)
    >r r@ item-stack @ stack-out @ r> item-offset @ - 1- ;

: really-store-single ( item -- )
 >r
 r@ item-out-index r@ item-stack @ stack-access ."  = vm_"
 r@ item-type @ print-type-prefix ." 2"
 r@ item-stack-type-name type ." ("
 r@ item-name 2@ type ." );"
 rdrop ;

: store-single ( item -- )
 >r
 r@ same-as-in?
 if
   r@ item-in-index 0= r@ item-out-index 0= xor
   if
       ." IF_" r@ item-stack @ stack-pointer 2@ type
       ." TOS(" r@ really-store-single ." );" cr
   endif
 else
   r@ really-store-single cr
 endif
 rdrop ;

: store-double ( item -- )
\ !! store optimization is not performed, because it is not yet needed
 >r
 ." vm_"
 r@ item-type @ print-type-prefix ." 2two"
 r@ item-stack-type-name type ." ("
 r@ item-name 2@ type ." , "
 r@ item-out-index r@ item-stack @ 2dup stack-access
 ." , "                       -1 under+ stack-access
 ." );" cr
 rdrop ;

: single ( -- xt1 xt2 n )
    ['] fetch-single ['] store-single 1 ;

: double ( -- xt1 xt2 n )
    ['] fetch-double ['] store-double 2 ;

: s, ( addr u -- )
\ allocate a string
 here swap dup allot move ;

wordlist constant prefixes

: declare ( addr "name" -- )
\ remember that there is a stack item at addr called name
 create , ;

: !default ( w addr -- )
    dup @ if
	2drop \ leave nonzero alone
    else
	!
    endif ;

: create-type { addr u xt1 xt2 n stack -- } ( "prefix" -- )
    \ describes a type
    \ addr u specifies the C type name
    \ stack effect entries of the type start with prefix
    create type% %allot >r
    addr u save-mem r@ type-c-name 2!
    xt1   r@ type-fetch !
    xt2   r@ type-store !
    n     r@ type-size !
    stack r@ type-stack !
    rdrop ;

: type-prefix ( xt1 xt2 n stack "prefix" -- )
    create-type
does> ( item -- )
    \ initialize item
    { item typ }
    typ item item-type !
    typ type-stack @ item item-stack !default
    item item-name 2@ prim prim-items-wordlist @ search-wordlist 0= if
	item item-name 2@ nextname item declare
	item item-first on
	\ typ type-c-name 2@ type space type  ." ;" cr
    else
	drop
	item item-first off
    endif ;

: execute-prefix ( item addr1 u1 -- )
    \ execute the word ( item -- ) associated with the longest prefix
    \ of addr1 u1
    0 swap ?do
	dup i prefixes search-wordlist
	if \ ok, we have the type ( item addr1 xt )
	    nip execute
	    UNLOOP EXIT
	endif
	-1 s+loop
    \ we did not find a type, abort
    true abort" unknown prefix" ;

: declaration ( item -- )
    dup item-name 2@ execute-prefix ;

: declaration-list ( addr1 addr2 -- )
    ['] declaration map-items ;

: declarations ( -- )
 wordlist dup prim prim-items-wordlist ! set-current
 prim prim-effect-in prim prim-effect-in-end @ declaration-list
 prim prim-effect-out prim prim-effect-out-end @ declaration-list ;

: print-declaration { item -- }
    item item-first @ if
	item item-type @ type-c-name 2@ type space
	item item-name 2@ type ." ;" cr
    endif ;

: print-declarations ( -- )
    prim prim-effect-in  prim prim-effect-in-end  @ ['] print-declaration map-items
    prim prim-effect-out prim prim-effect-out-end @ ['] print-declaration map-items ;
    
: stack-prefix ( stack "prefix" -- )
    name tuck nextname create ( stack length ) 2,
does> ( item -- )
    2@ { item stack prefix-length }
    item item-name 2@ prefix-length /string item item-name 2!
    stack item item-stack !
    item declaration ;

\ types pointed to by stacks for use in combined prims
s" Cell"  single 0 create-type cell-type
s" Float" single 0 create-type float-type

s" sp" save-mem cell-type  s" (Cell)" make-stack data-stack 
s" fp" save-mem float-type s" "       make-stack fp-stack
s" rp" save-mem cell-type  s" (Cell)" make-stack return-stack
s" IP" save-mem cell-type  s" error don't use # on results" make-stack inst-stream
' inst-in-index inst-stream stack-in-index-xt !
' inst-stream <is> inst-stream-f
\ !! initialize stack-in and stack-out

\ offset computation
\ the leftmost (i.e. deepest) item has offset 0
\ the rightmost item has the highest offset

: compute-offset { item xt -- }
    \ xt specifies in/out; update stack-in/out and set item-offset
    item item-type @ type-size @
    item item-stack @ xt execute dup @ >r +!
    r> item item-offset ! ;

: compute-offset-in ( addr1 addr2 -- )
    ['] stack-in compute-offset ;

: compute-offset-out ( addr1 addr2 -- )
    ['] stack-out compute-offset ;

: clear-stack { -- }
    dup stack-in off stack-out off ;

: compute-offsets ( -- )
    data-stack clear-stack  fp-stack clear-stack return-stack clear-stack
    inst-stream clear-stack
    prim prim-effect-in  prim prim-effect-in-end  @ ['] compute-offset-in  map-items
    prim prim-effect-out prim prim-effect-out-end @ ['] compute-offset-out map-items
    inst-stream stack-out @ 0<> abort" # can only be on the input side" ;

: flush-a-tos { stack -- }
    stack stack-out @ 0<> stack stack-in @ 0= and
    if
	." IF_" stack stack-pointer 2@ 2dup type ." TOS("
	2dup type ." [0] = " type ." TOS);" cr
    endif ;

: flush-tos ( -- )
    data-stack   flush-a-tos
    fp-stack     flush-a-tos
    return-stack flush-a-tos ;

: fill-a-tos { stack -- }
    stack stack-out @ 0= stack stack-in @ 0<> and
    if
	." IF_" stack stack-pointer 2@ 2dup type ." TOS("
	2dup type ." TOS = " type ." [0]);" cr
    endif ;

: fill-tos ( -- )
    \ !! inst-stream for prefetching?
    fp-stack     fill-a-tos
    data-stack   fill-a-tos
    return-stack fill-a-tos ;

: fetch ( addr -- )
    dup item-type @ type-fetch @ execute ;

: fetches ( -- )
    prim prim-effect-in prim prim-effect-in-end @ ['] fetch map-items ;

: stack-pointer-update { stack -- }
    \ stack grow downwards
    stack stack-in @ stack stack-out @ -
    ?dup-if \ this check is not necessary, gcc would do this for us
	stack stack-pointer 2@ type ."  += " 0 .r ." ;" cr
    endif ;

: inst-pointer-update ( -- )
    inst-stream stack-in @ ?dup-if
	." INC_IP(" 0 .r ." );" cr
    endif ;

: stack-pointer-updates ( -- )
    inst-pointer-update
    data-stack   stack-pointer-update
    fp-stack     stack-pointer-update
    return-stack stack-pointer-update ;

: store ( item -- )
\ f is true if the item should be stored
\ f is false if the store is probably not necessary
 dup item-type @ type-store @ execute ;

: stores ( -- )
    prim prim-effect-out prim prim-effect-out-end @ ['] store map-items ;

: output-c-tail ( -- )
    \ the final part of the generated C code
    ." NEXT_P1;" cr
    stores
    fill-tos
    ." NEXT_P2;" cr ;

: type-c ( c-addr u -- )
    \ like TYPE, but replaces "TAIL;" with tail code
    begin ( c-addr1 u1 )
	2dup s" TAIL;" search
    while ( c-addr1 u1 c-addr3 u3 )
	2dup 2>r drop nip over - type
	output-c-tail
	2r> 5 /string
	\ !! resync #line missing
    repeat
    2drop type ;

: print-debug-arg { item -- }
    ." fputs(" quote space item item-name 2@ type ." =" quote ." , vm_out); "
    ." printarg_" item item-type @ print-type-prefix
    ." (" item item-name 2@ type ." );" cr ;
    
: print-debug-args ( -- )
    ." #ifdef VM_DEBUG" cr
    ." if (vm_debug) {" cr
    prim prim-effect-in prim prim-effect-in-end @ ['] print-debug-arg map-items
    ." fputc('\n', vm_out);" cr
    ." }" cr
    ." #endif" cr ;

: print-entry ( -- )
    ." I_" prim prim-c-name 2@ type ." :" ;
    
: output-c ( -- ) 
 print-entry ."  /* " prim prim-name 2@ type ."  ( " prim prim-stack-string 2@ type ." ) */" cr
 ." /* " prim prim-doc 2@ type ."  */" cr
 ." NAME(" quote prim prim-name 2@ type quote ." )" cr \ debugging
 ." {" cr
 ." DEF_CA" cr
 print-declarations
 ." NEXT_P0;" cr
 flush-tos
 fetches
 print-debug-args
 stack-pointer-updates
 ." {" cr
 ." #line " c-line @ . quote c-filename 2@ type quote cr
 prim prim-c-code 2@ type-c
 ." }" cr
 output-c-tail
 ." }" cr
 cr
;

: disasm-arg { item -- }
    item item-stack @ inst-stream = if
	."   fputc(' ', vm_out); "
	." printarg_" item item-type @ print-type-prefix
	." ((" item item-type @ type-c-name 2@ type ." )"
	." ip[" item item-offset @ 1+ 0 .r ." ]);" cr
    endif ;

: disasm-args ( -- )
    prim prim-effect-in prim prim-effect-in-end @ ['] disasm-arg map-items ;

: output-disasm ( -- )
    \ generate code for disassembling VM instructions
    ." if (ip[0] == prim[" function-number @ 0 .r ." ]) {" cr
    ."   fputs(" quote prim prim-name 2@ type quote ." , vm_out);" cr
    disasm-args
    ."   ip += " inst-stream stack-in @ 1+ 0 .r ." ;" cr
    ." } else " ;

: gen-arg-parm { item -- }
    item item-stack @ inst-stream = if
	." , " item item-type @ type-c-name 2@ type space
	item item-name 2@ type
    endif ;

: gen-args-parm ( -- )
    prim prim-effect-in prim prim-effect-in-end @ ['] gen-arg-parm map-items ;

: gen-arg-gen { item -- }
    item item-stack @ inst-stream = if
	."   genarg_" item item-type @ print-type-prefix
        ." (ctp, " item item-name 2@ type ." );" cr
    endif ;

: gen-args-gen ( -- )
    prim prim-effect-in prim prim-effect-in-end @ ['] gen-arg-gen map-items ;

: output-gen ( -- )
    \ generate C code for generating VM instructions
    ." void gen_" prim prim-c-name 2@ type ." (Inst **ctp" gen-args-parm ." )" cr
    ." {" cr
    ."   gen_inst(ctp, vm_prim[" function-number @ 0 .r ." ]);" cr
    gen-args-gen
    ." }" cr ;

: stack-used? { stack -- f }
    stack stack-in @ stack stack-out @ or 0<> ;

: output-funclabel ( -- )
  ." &I_" prim prim-c-name 2@ type ." ," cr ;

: output-forthname ( -- )
  '" emit prim prim-name 2@ type '" emit ." ," cr ;

: output-c-func ( -- )
\ used for word libraries
    ." Cell * I_" prim prim-c-name 2@ type ." (Cell *SP, Cell **FP)      /* " prim prim-name 2@ type
    ."  ( " prim prim-stack-string 2@ type ."  ) */" cr
    ." /* " prim prim-doc 2@ type ."  */" cr
    ." NAME(" quote prim prim-name 2@ type quote ." )" cr
    \ debugging
    ." {" cr
    print-declarations
    inst-stream  stack-used? IF ." Cell *ip=IP;" cr THEN
    data-stack   stack-used? IF ." Cell *sp=SP;" cr THEN
    fp-stack     stack-used? IF ." Cell *fp=*FP;" cr THEN
    return-stack stack-used? IF ." Cell *rp=*RP;" cr THEN
    flush-tos
    fetches
    stack-pointer-updates
    fp-stack   stack-used? IF ." *FP=fp;" cr THEN
    ." {" cr
    ." #line " c-line @ . quote c-filename 2@ type quote cr
    prim prim-c-code 2@ type
    ." }" cr
    stores
    fill-tos
    ." return (sp);" cr
    ." }" cr
    cr ;

: output-label ( -- )  
    ." (Label)&&I_" prim prim-c-name 2@ type ." ," cr ;

: output-alias ( -- ) 
    ( primitive-number @ . ." alias " ) ." Primitive " prim prim-name 2@ type cr ;

: output-forth ( -- )  
    prim prim-forth-code @ 0=
    IF    	\ output-alias
	\ this is bad for ec: an alias is compiled if tho word does not exist!
	\ JAW
    ELSE  ." : " prim prim-name 2@ type ."   ( "
	prim prim-stack-string 2@ type ." )" cr
	prim prim-forth-code 2@ type cr
    THEN ;

: output-tag-file ( -- )
    name-filename 2@ last-name-filename 2@ compare if
	name-filename 2@ last-name-filename 2!
	#ff emit cr
	name-filename 2@ type
	." ,0" cr
    endif ;

: output-tag ( -- )
    output-tag-file
    prim prim-name 2@ 1+ type
    127 emit
    space prim prim-name 2@ type space
    1 emit
    name-line @ 0 .r
    ." ,0" cr ;

[IFDEF] documentation
: register-doc ( -- )
    get-current documentation set-current
    prim prim-name 2@ nextname create
    prim prim-name 2@ 2,
    prim prim-stack-string 2@ condition-stack-effect 2,
    prim prim-wordset 2@ 2,
    prim prim-c-name 2@ condition-pronounciation 2,
    prim prim-doc 2@ 2,
    set-current ;
[THEN]


\ combining instructions

\ The input should look like this:

\ lit_+ = lit +

\ The output should look like this:

\  I_lit_+:
\  {
\  DEF_CA
\  Cell _x_ip0;
\  Cell _x_sp0;
\  Cell _x_sp1;
\  NEXT_P0;
\  _x_ip0 = (Cell) IPTOS;
\  _x_sp0 = (Cell) spTOS;
\  INC_IP(1);
\  /* sp += 0; */
\  /* lit ( #w -- w ) */
\  /*  */
\  NAME("lit")
\  {
\  Cell w;
\  w = (Cell) _x_ip0;
\  #ifdef VM_DEBUG
\  if (vm_debug) {
\  fputs(" w=", vm_out); printarg_w (w);
\  fputc('\n', vm_out);
\  }
\  #endif
\  {
\  #line 136 "./prim"
\  }
\  _x_sp1 = (Cell)w;
\  }
\  I_plus:	/* + ( n1 n2 -- n ) */
\  /*  */
\  NAME("+")
\  {
\  DEF_CA
\  Cell n1;
\  Cell n2;
\  Cell n;
\  NEXT_P0;
\  n1 = (Cell) _x_sp0;
\  n2 = (Cell) _x_sp1;
\  #ifdef VM_DEBUG
\  if (vm_debug) {
\  fputs(" n1=", vm_out); printarg_n (n1);
\  fputs(" n2=", vm_out); printarg_n (n2);
\  fputc('\n', vm_out);
\  }
\  #endif
\  {
\  #line 516 "./prim"
\  n = n1+n2;
\  }
\  NEXT_P1;
\  _x_sp0 = (Cell)n;
\  NEXT_P2;
\  }
\  NEXT_P1;
\  spTOS = (Cell)_x_sp0;
\  NEXT_P2;

: init-combined ( -- )
    prim to combined
    0 num-combined !
    current-depth max-stacks cells erase
    max-depth     max-stacks cells erase
    min-depth     max-stacks cells erase
    prim prim-effect-in  prim prim-effect-in-end  !
    prim prim-effect-out prim prim-effect-out-end ! ;

: max! ( n addr -- )
    tuck @ max swap ! ;

: min! ( n addr -- )
    tuck @ min swap ! ;

: add-depths { p -- }
    \ combine stack effect of p with *-depths
    max-stacks 0 ?do
	current-depth i th @
	p prim-stacks-in  i th @ +
	dup max-depth i th max!
	p prim-stacks-out i th @ -
	dup min-depth i th min!
	current-depth i th !
    loop ;

: add-prim ( addr u -- )
    \ add primitive given by "addr u" to combined-prims
    primitives search-wordlist s" unknown primitive" ?print-error
    execute { p }
    p combined-prims num-combined @ th !
    1 num-combined +!
    p add-depths ;

: compute-effects { q -- }
    \ compute the stack effects of q from the depths
    max-stacks 0 ?do
	max-depth i th @ dup
	q prim-stacks-in i th !
	current-depth i th @ -
	q prim-stacks-out i th !
    loop ;

: make-effect-items { stack# items effect-endp -- }
    \ effect-endp points to a pointer to the end of the current item-array
    \ and has to be updated
    stacks stack# th @ { stack }
    items 0 +do
	effect-endp @ { item }
	i 0 <# #s stack stack-pointer 2@ holds [char] _ hold #> save-mem
	item item-name 2!
	stack item item-stack !
	stack stack-type @ item item-type !
	i item item-offset !
	item item-first on
	item% %size effect-endp +!
    loop ;

: init-effects { q -- }
    \ initialize effects field for FETCHES and STORES
    max-stacks 0 ?do
	i q prim-stacks-in  i th @ q prim-effect-in-end  make-effect-items
	i q prim-stacks-out i th @ q prim-effect-out-end make-effect-items
    loop ;

: process-combined ( -- )
    prim compute-effects
    prim init-effects
    output-combined perform ;

\ C output

: print-item { n stack -- }
    \ print nth stack item name
    stack stack-type @ type-c-name 2@ type space
    ." _" stack stack-pointer 2@ type n 0 .r ;

: print-declarations-combined ( -- )
    max-stacks 0 ?do
	max-depth i th @ min-depth i th @ - 0 +do
	    i stacks j th @ print-item ." ;" cr
	loop
    loop ;

: part-fetches ( -- )
    fetches ;

: part-output-c-tail ( -- )
    output-c-tail ;

: output-part ( p -- )
    to prim
    ." /* " prim prim-name 2@ type ."  ( " prim prim-stack-string 2@ type ." ) */" cr
    ." NAME(" quote prim prim-name 2@ type quote ." )" cr \ debugging
    ." {" cr
    print-declarations
    part-fetches
    print-debug-args
    prim add-depths \ !! right place?
    ." {" cr
    ." #line " c-line @ . quote c-filename 2@ type quote cr
    prim prim-c-code 2@ type-c \ !! deal with TAIL
    ." }" cr
    part-output-c-tail
    ." }" cr ;

: output-parts ( -- )
    prim >r in-part on
    current-depth max-stacks cells erase
    num-combined @ 0 +do
	combined-prims i th @ output-part
    loop
    in-part off
    r> to prim ;

: output-c-combined ( -- )
    print-entry cr
    \ debugging messages just in parts
    ." {" cr
    ." DEF_CA" cr
    print-declarations-combined
    ." NEXT_P0;" cr
    flush-tos
    fetches
    \ print-debug-args
    stack-pointer-updates
    output-parts
    output-c-tail
    ." }" cr
    cr ;

: output-forth-combined ( -- )
    ;

\ the parser

eof-char max-member \ the whole character set + EOF

: getinput ( -- n )
 rawinput @ endrawinput @ =
 if
   eof-char
 else
   cookedinput @ c@
 endif ;

:noname ( n -- )
 dup bl > if
  emit space
 else
  .
 endif ;
print-token !

: testchar? ( set -- f )
 getinput member? ;
' testchar? test-vector !

: checksyncline ( -- )
    \ when input points to a newline, check if the next line is a
    \ sync line.  If it is, perform the appropriate actions.
    rawinput @ >r
    s" #line " r@ over compare 0<> if
	rdrop 1 line +! EXIT
    endif
    0. r> 6 chars + 20 >number drop >r drop line ! r> ( c-addr )
    dup c@ bl = if
	char+ dup c@ [char] " <> abort" sync line syntax"
	char+ dup 100 [char] " scan drop swap 2dup - save-mem filename 2!
	char+
    endif
    dup c@ nl-char <> abort" sync line syntax"
    skipsynclines @ if
	dup char+ rawinput !
	rawinput @ c@ cookedinput @ c!
    endif
    drop ;

: ?nextchar ( f -- )
    s" syntax error, wrong char" ?print-error
    rawinput @ endrawinput @ <> if
	rawinput @ c@
	1 chars rawinput +!
	1 chars cookedinput +!
	nl-char = if
	    checksyncline
	    rawinput @ line-start !
	endif
	rawinput @ c@ cookedinput @ c!
    endif ;

: charclass ( set "name" -- )
 ['] ?nextchar terminal ;

: .. ( c1 c2 -- set )
 ( creates a set that includes the characters c, c1<=c<=c2 )
 empty copy-set
 swap 1+ rot do
  i over add-member
 loop ;

: ` ( -- terminal ) ( use: ` c )
 ( creates anonymous terminal for the character c )
 char singleton ['] ?nextchar make-terminal ;

char a char z ..  char A char Z ..  union char _ singleton union  charclass letter
char 0 char 9 ..					charclass digit
bl singleton tab-char over add-member			charclass white
nl-char singleton eof-char over add-member complement	charclass nonl
nl-char singleton eof-char over add-member
    char : over add-member complement                   charclass nocolonnl
bl 1+ maxchar .. char \ singleton complement intersection
                                                        charclass nowhitebq
bl 1+ maxchar ..                                        charclass nowhite
char " singleton eof-char over add-member complement	charclass noquote
nl-char singleton					charclass nl
eof-char singleton					charclass eof
nl-char singleton eof-char over add-member		charclass nleof

(( letter (( letter || digit )) **
)) <- c-ident ( -- )

(( ` # ?? (( letter || digit || ` : )) **
)) <- stack-ident ( -- )

(( nowhitebq nowhite ** ))
<- forth-ident ( -- )

Variable forth-flag
Variable c-flag

(( (( ` e || ` E )) {{ start }} nonl ** 
   {{ end evaluate }}
)) <- eval-comment ( ... -- ... )

(( (( ` f || ` F )) {{ start }} nonl ** 
   {{ end forth-flag @ IF type cr ELSE 2drop THEN }}
)) <- forth-comment ( -- )

(( (( ` c || ` C )) {{ start }} nonl ** 
   {{ end c-flag @ IF type cr ELSE 2drop THEN }}
)) <- c-comment ( -- )

(( ` - nonl ** {{ 
	forth-flag @ IF ." [ELSE]" cr THEN
	c-flag @ IF ." #else" cr THEN }}
)) <- else-comment

(( ` + {{ start }} nonl ** {{ end
	dup
	IF	c-flag @
		IF    ." #ifdef HAS_" bounds ?DO  I c@ toupper emit  LOOP cr
		THEN
		forth-flag @
		IF  ." has? " type ."  [IF]"  cr THEN
	ELSE	2drop
	    c-flag @      IF  ." #endif"  cr THEN
	    forth-flag @  IF  ." [THEN]"  cr THEN
	THEN }}
)) <- if-comment

(( (( eval-comment || forth-comment || c-comment || else-comment || if-comment )) ?? nonl ** )) <- comment-body

(( ` \ comment-body nleof )) <- comment ( -- )

(( {{ start }} stack-ident {{ end 2 pick init-item item% %size + }} white ** )) **
<- stack-items

(( {{ prim prim-effect-in }}  stack-items {{ prim prim-effect-in-end ! }}
   ` - ` - white **
   {{ prim prim-effect-out }} stack-items {{ prim prim-effect-out-end ! }}
)) <- stack-effect ( -- )

(( {{ prim create-prim }}
   ` ( white ** {{ start }} stack-effect {{ end prim prim-stack-string 2! }} ` ) white **
   (( {{ start }} forth-ident {{ end prim prim-wordset 2! }} white **
      (( {{ start }}  c-ident {{ end prim prim-c-name 2! }} )) ??
   )) ??  nleof
   (( ` " ` "  {{ start }} (( noquote ++ ` " )) ++ {{ end 1- prim prim-doc 2! }} ` " white ** nleof )) ??
   {{ skipsynclines off line @ c-line ! filename 2@ c-filename 2! start }} (( nocolonnl nonl **  nleof white ** )) ** {{ end prim prim-c-code 2! skipsynclines on }}
   (( ` :  white ** nleof
      {{ start }} (( nonl ++  nleof white ** )) ++ {{ end prim prim-forth-code 2! }}
   )) ?? {{  declarations compute-offsets printprim 1 function-number +! }}
   nleof
)) <- simple-primitive ( -- )

(( {{ init-combined }}
   ` = (( white ++ {{ start }} forth-ident {{ end add-prim }} )) ++
   nleof {{ process-combined }}
)) <- combined-primitive

(( {{ make-prim to prim 0 to combined
      line @ name-line ! filename 2@ name-filename 2!
      start }} forth-ident {{ end 2dup prim prim-name 2! prim prim-c-name 2! }}  white ++
   (( simple-primitive || combined-primitive ))
)) <- primitive ( -- )

(( (( comment || primitive || nl white ** )) ** eof ))
parser primitives2something
warnings @ [IF]
.( parser generated ok ) cr
[THEN]

: primfilter ( addr u -- )
    \ process the string at addr u
    over dup rawinput ! dup line-start ! cookedinput !
    + endrawinput !
    checksyncline
    primitives2something ;    

: process-file ( addr u xt-simple x-combined -- )
    output-combined ! output !
    save-mem 2dup filename 2!
    slurp-file
    warnings @ if
	." ------------ CUT HERE -------------" cr  endif
    primfilter ;

\  : process      ( xt -- )
\      bl word count rot
\      process-file ;
