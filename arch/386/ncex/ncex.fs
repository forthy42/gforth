\ Gforth native code extensions
\ Written by Lars Krueger 1999, based on the FLK compiler.

\ The gforth native code extensions (NCEX) were developed to speed up the
\ execution of gforth code. They are based on the FLK compiler. 
\ 
\ I intended FLK to be a gforth compatible native code compiler. Sadly I
\ failed to realize the weaknesses of this approach. It is a lot of effort to
\ maintain a whole compiler, especially if you want to be compatible to
\ a different system. 
\ 
\ Now, due to the revived request for FLK (half a year after I uploaded the
\ last version of FLK before giving up), Jens Wilke, one of the gforth
\ maintainers, encouraged me to sit down and stuff the FLK code in gforth.
\ 
\ This is the result.

\ Before the actual code follows I want to explain the basic principle of
\ NCEX.
\ 
\ There are a few sources of speed-ups in a threaded code system:
\ 1. The inner interpreter. Speeding up NEXT by only one clock cycle per call
\    results in an enormous gain in performance. gforth's NEXT is between 3
\    and 5 cycles on a Pentium I guess. A subroutine threading system has a
\    NEXT that is two cycles (again on the Pentium) long: call x takes two
\    cycles. 
\ 2. Call and return. A good forth program consists of many small words that
\    call other small words. A study of Ertl et al. show that NEST and UNNEST
\    are the words a threaded forth spends most of its time in. Many system
\    exists that reduce this overhead by removing tail calls. The next logical
\    step is to remove all calls. Of course this is not possible or not
\    practical. One could inline code but that would require a lot of
\    complicated compiler code to handle all different cases. Instead NCEX
\    uses an improved calling convention of native code words.
\ 3. Primitives. If you don't have any NEST's and UNNEST's anymore, the system
\    spends it's time in the primitives. If you speed up the ones that are
\    used most often, you can gain a lot of performance. Luckily the
\    primitives used most often are the most simple ones: DUP, SWAP, + etc. If
\    you compile them inline as native code instead of calling them you save
\    the call and the Pentium can pair (execute in parallel) them. Some FORTH
\    systems (e.g. bigForth) solve this by compiling macros: Instead of
\    calling DUP they copy the assembler code of DUP to the current position
\    in the code. The disadvantage of this method becomes clear in the next
\    point.
\ 4. Stack access. If you optimized everything else, the only thing that
\    wastes time is the stack access. Sure, you can't avoid this, but usually
\    it is not nessessary to reach out of the CPU and really read data from
\    the memory, where the stack is stored. Why? Because FORTH words usually
\    use less stack items than even the register starved Intel CPU's have.
\    Many people found that out long before I even heard of FORTH. :-) That's
\    the reason why almost all FORTH system keep their topmost stack item in a
\    CPU register. NCEX goes one step further: It tries to keep as many items
\    in the CPU as long as possible. This reduces the time to access the
\    memory a lot. Sadly this is the largest disadvantage of NCEX too: If you
\    have to flush the registers very often it costs almost the same time as
\    having no cache at all.
\ 5. Propability. In every FORTH program there are sequences that repeat very
\    often. One example is "DUP IF". Another one is "< IF". One solution of
\    the past is to provide special code primitives like "DUP-ZBRANCH" or 
\    "<-ZBRANCH" that perform the computation and "compiler front-ends" like
\    "DUP-IF" etc. This is not very portable and leads to ugly words like:
\    "OVER_+_@_OR_IF". Additionally the programmer has to known all the words
\    in order to exploit the full speed possible. NCEX reliefs you from these
\    tasks. It knows which of these words exist and uses them even if you
\    write "OVER + @ OR IF".
\
\ How all this is actually implemented can be seen in the next section.

\ NCEX architecture.
\
\ NCEX is a layered system. You can see the architecture in the following
\ graph. 
\
\  colon compiler | primitive optimizers
\              xt cache 
\ number compiler | optimizer tree
\         register allocator
\             assembler
\                CPU
\
\ What CPU and assembler are doing is obvious. The register allocator
\ maintains the top n stack items in CPU registers. The more there are the
\ faster the system gets. Who would have thought this: The number compiler
\ indeed compiles numbers, namely all literals in the colon definition. In the
\ xt cache we find the xt's that are stored there by compile, which are then
\ looked up in the optimizer tree to find the primitive optimizers. These
\ words are the actual native code compiler. Finally the colon compiler is the
\ builtin one except that some words have been revectored.

\ And so it begins...

\ At first we need some support words that may be recompiled as primitives.
: PLUCK 2 PICK ;
: TURN 3 ROLL ;
: -TURN TURN TURN TURN ;
: FLOCK 3 PICK ;

: ?throw ( flag x -- )
  swap if throw then drop ;


\ The first thing to load is the assembler. It is a simple postfix assembler
\ for 32 bit protected mode. That brings it down to 1000 lines including
\ floating point and a lot of comments.
include ./ncexasm.fs

\ Now we need the register allocator. It maintains a virtual stack in the CPU
\ registers.
include ./ncexregalloc.fs

\ Now we are ready to include the optimizer tree support. It contains the code
\ for the sequences to optimize away. E.g.:
\
\ opttree
\ |
\ +-- dup: ... inline code for dup
\ |   |
\ |   +-- literal: ... no code, can't optimize "dup literal"
\ |       |
\ |       +-- +: ... inline code for "dup literal +"
\ +-- over: ... inline code for over
\ |
\ ...
\ 
\ The variable opttree is the root of the tree. Whenever a sequence of words
\ should be compiled e.g. "dup 5 + over" the deepest matching node in the tree
\ is searched. In this case it is the node "dup literal +". Then the word
\ assigned to that node is executed. The word itself reads the parameter (5 in
\ this case) and compiles the assembler code. After that the first three words
\ are removed from the compiler cache. The remaining word "over" goes through
\ the same procedure and leaves the cache empty.
include ./ncextree.fs

\ Include the first CPU dependent part. It contains the nc literal compiler
\ and the threaded code gateway compiler.
include ./ncexcpu1.fs

\ The next important data structure of the NCEX is the xt-cache. Whenever the
\ cache needs to be flushed partially (because it is full) or completely
\ (because the words ends) real code gets compiled using the tree above.
\
\ This file does not contain the actual flushing code because this is at least
\ partially system dependent. 
\ The cache is stored in a ring buffer, a first in first out storage or queue.
\ This is loaded first.
include ./ring.fs
include ./ncexcache.fs

\ Include the second CPU dependent part. It contains the optimizers.
include ./ncexcpu2.fs

\ Include the control flow stack. It is almost system independent as it only
\ depends on the register allocator.
include ./ncexcfstack.fs

\ Include the control flow words.
include ./ncexcontrol.fs

\ The last missing part are the patches to the compiler which are loaded now.
\ The words switch between the threaded code compiler and the native code
\ compiler. Other words are provided to generate the code to run the optimized
\ native code from the treaded code.
include ./ncexcompiler.fs

\ Start the whole thing.
init-ncex
