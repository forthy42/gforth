\ NCEX CPU dependent part 1

\ This file covers all CPU and system dependent parts that perform calls from
\ native to threaded code and back.
\
\ First of all we define the register mapping of the VM. If you change
\ machine.h you have to change code here too.
\
\ The 386 calling sequence in C uses esp as the frame pointer.
\ gforth 0.5.x-fast with gcc 2.95.2 uses the following registers:
\ esi as SP
\ edi as RP
\ ebp as IP
\ ebx as TOS
\ there's no CFA, since it is direct theaded code
\ All other registers are stored in the stack frame.
\ next is pre-increment and looks like -4 [ebx] jmp
\ This allows us to directly jump to the threaded function
\ and just setting IP to the return-to-nc code
\ 
\ The native code uses ebp as stack pointer and esp as return pointer. All
\ other registers are free to be used by the code.
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

Create return-to-nc
    esp DWORD c-stack #[] mov,
    edi esp mov,
    ebx 0 [esi] mov,
    esi ebp mov,
    ret,

Create call-tc return-to-nc ,

Create nc-to-tc ( xt -- )
  eax pop,  4 ## eax add,  eax push,
  esp edi mov,
  0 [ebp] ebx mov, \ restore TOS
  ebp esi mov,
  ( Fake an execute )
  DWORD c-stack #[] esp mov,
  call-tc ## ebp mov,
  DWORD -4 [eax] jmp,

Create wrapper
  eax pop, \ that's were the native code will start
  esp DWORD c-stack #[] mov,
  edi esp mov,
  ebp push, \ save IP
  ebx 0 [esi] mov,
  esi ebp mov,
  3 ## eax add,
  eax call, \ call native code word
  0 [ebp] ebx mov, \ that's where we return
  ebp esi mov,
  ebp pop,
  esp edi mov,
  4 ## ebp add,
  c-stack #[] esp mov,
  DWORD -4 [ebp] jmp,

\ Compile a call to the native code xt.
: nc-to-nc, ( xt -- )      
  regalloc-reset
  regalloc-flush
  >body ## call,
  ;

: nc-to-tc, ( xt -- )
    ['] nc-to-tc nc-to-nc, , ;
    
\ Compile the prefix code of a colon definition.
: (nc-:), ( -- ) ( rt: esi: csf -- )
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

\ Compile code to call the given address.
: call-nc, ( addr -- )
  ## call,
  ;

\ Compile code to continue the interpretation.
: next, ( -- )
  ['] noop ## jmp,
  ;

