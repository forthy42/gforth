\ NCEX CPU dependent part 1

\ This file covers all CPU and system dependent parts that perform calls from
\ native to threaded code and back.
\
\ First of all we define the register mapping of the VM. If you change
\ machine.h you have to change code here too.
\
\ The 386 calling sequence in C uses esp as the frame pointer.
\ gforth 0.5.x-fast uses the following registers:
\ esi as SP
\ edi as RP
\ ebp as IP
\ ebx as TOS
\ there's no CFA, since it is direct theaded code
\ All other registers are stored in the stack frame. 
\
\ The native code uses ebp as stack pointer and esp as return pointer. All
\ other registers are free to be used by the code. The top of the return
\ stack contains the address of the C stack frame. This value is preserved
\ across calls.
\
\ A call to theaded code therefore has to normalize the stack. We then put
\ SP and RP into place, set up IP to our return word, and restore C's stack
\ pointer. It is then sufficient to jump into the threaded code, the rest is
\ handled by this code.
\
\ IP and C's stack are preserved when entering the native code domain from
\ threaded code, and are restored on exit. We use Gforth's code field to
\ store the appropriate code there. Native calls will skip the code field
\ and call directly into the data field.

Variable c-stack \ C's stack
Variable t-ip    \ threaded IP

3 Constant csfo-tos
5 Constant csfo-ip
6 Constant csfo-sp
7 Constant csfo-rp

\ Native code literal compiler.
: nc-literal ( x -- )
  regalloc-reset
  req-free 
  ## free0 mov, 
  0 free>tos
  ;

: native-xt? ( xt -- flag )
  dup @ swap >body > ;

\ Compile a call to the threaded code xt. As the threaded code expects to
\ return to a threaded, we have to create one. It just needs to be 1 cell
\ long. This cell must contain the address of a faked xt . This faked xt
\ contains the address of the native code in the first cell.
: nc-to-tc, ( xt -- )
  regalloc-reset
  regalloc-flush \ r: ... csf
  ( Restore registers of VM )
  0 [esp] esi mov, \ esi = csf
  ebp ebx mov, 
  ( Fake an execute )
  ( push the current ip )
  DWORD csfo-ip [esi] push,
  ( restore VM-rp and the C frame pointer)
  esp csfo-rp [esi] mov, 
  esi esp mov,
  ( set the current ip to a faked tread )
  0 ## ecx mov, asm-here 1 cells - \ xt fix
  ecx csfo-ip [esi] mov,
  ( cfa=XT; )
  over DWORD ## csfo-cfa [esp] mov, \ xt fix
  ( goto **cfa; )
  swap @ ## jmp, \ fix
  ( here is the faked thread )
  asm-here tuck \ here fix here
  swap ! \ here
  ( the thread )
  cell+ dup asm-, \ thread
  ( the faked xt )
  cell+ asm-, \ 
  ( load back the registers )
  esp esi mov, \ save C stack frame
  csfo-rp [esp] esp mov, 
  ebx ebp mov, 
  DWORD csfo-ip [esi] pop,
  ;

\ Compile a call to the native code xt.
: nc-to-nc, ( xt -- )      
  regalloc-reset
  regalloc-flush
  esi pop,
  >body ## call,
  esi push,
  ;

\ Compile the prefix code of a colon definition.
: (nc-:), ( -- ) ( rt: esi: csf -- )
  esi push,
  ;
  
\ Calling gateway. Generates native code to call threaded or native code
\ depending on xt.
: call-gateway, ( xt -- )
  dup native-xt? if \ xt
    nc-to-nc,
  else \ xt
    nc-to-tc,
  then
  ;

\ Compile code to load the return pointer (esp) from the interpreter rp.
\ Compile code to load the stack pointer (ebp) from the interpreter sp.
: load-rp&sp, ( -- ) ( rt: -- esi: csf )
  esp esi mov, \ save C stack frame
  csfo-rp [esp] esp mov, 
  ebx ebp mov, 
  ;

\ Compile code to load the interpreter rp from native rp. 
\ Compile code to load the interpreter sp from native sp. 
: save-rp&sp, ( -- ) ( rt: esi: csf -- )
  esp csfo-rp [esi] mov,
  ebp ebx mov,
  esi esp mov,
  ;

\ Compile code to call the given address.
: call-nc, ( addr -- )
  ## call,
  ;

\ Compile code to continue the interpretation.
: next, ( -- )
  ['] noop @ ## jmp,
  ;

