\ asm.fs	assembler file (for PPC32/64)
\
\ Copyright (C) 2006,2007 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

get-current
also assembler definitions

: out-bin dup 2 base ! . decimal ;

\ helpers
1 cells 4 =
[if]
: h, ( h -- )
, ;
[endif]

1 cells 8 = 
[if]
: h, ( h -- )     \ 32 bit store + allot
\ dup hex. \ uncoment this for testing with the test_asm.py script
here here aligned = if
  32 lshift
  here !
else
  here 4 - dup
  @ rot or
  swap !
endif
4 allot ;
[endif]

: check-range ( u1 u2 u3 -- )
  within 0= -24 and throw ;

: copy>here ( a1 u1 -- )
  chars here over allot swap cmove ;

: concat ( a1 u1 a2 u2 -- a u )
  here >r 2swap copy>here copy>here r>
  here over - 1 chars / ;

\ words used by {a,b,ds,i,m,md,mds,x,xo,xl,xs,xfl,xfx}-form
\ convention: asm-1-<range>
\ 1 is just an ID, range says which bits it affects in the code of this inst.
: asm-1-0,5 ( addr code -- code ) \ asm-a-{1,2,3,4}, asm-mds
  swap 2 cells + @ 26 lshift or ;

: asm-1-6,10 ( ... addr code -- ... addr code ) \ asm-a-{1,2,3,4}
  swap >r over 0 $20 check-range swap 21 lshift or r> swap ;

: asm-1-11,15 ( ... addr code -- ... addr code ) \ asm-a-{1,3}
  swap >r over 0 $20 check-range swap 16 lshift or r> swap ;

: asm-1-16,20 ( ... addr code -- ... addr code ) \ asm-a-{1,2,4}
  swap >r over 0 $20 check-range swap 11 lshift or r> swap ;

\ end, common to {a,b,ds,i,m,md,mds,x,xo,xl,xs,xfl,xfx}-form

\ common to {a,m}-form

: asm-2-21,25 ( ... addr code -- ... addr code ) \ asm-a-{2,3}
  swap >r over 0 $20 check-range swap 6 lshift or r> swap ;

\ end, common to {a,m-form}

\ a-form
: asm-a-26,31-0 ( ... n -- ... code ) \ asm-a-1
  \ sets bit 31 to zero
  1 lshift $3F and ;

: asm-a-26,31-1 ( ... n -- ... code ) \ asm-a-1
  \ sets bit 31 to one
  1 lshift $3F and 1 or ;

: asm-a-1-define ( {59, 63} xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, ,
does> dup dup @ swap 1 cells + @ execute asm-1-16,20 asm-1-11,15 ( D A B -- )
  asm-1-6,10 asm-1-0,5 h, ;

: asm-a-1-59 ( n "name" -- )
  name { n addr len }
  59 ['] asm-a-26,31-0 n addr len s" " asm-a-1-define
  59 ['] asm-a-26,31-1 n addr len s" ." asm-a-1-define ;

: asm-a-1-63 ( n "name" -- )
  name { n addr len }
  63 ['] asm-a-26,31-0 n addr len s" " asm-a-1-define
  63 ['] asm-a-26,31-1 n addr len s" ." asm-a-1-define ;

: asm-a-2-define ( {59,63} xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, ,
does> dup dup @ swap 1 cells + @ execute asm-1-16,20 asm-2-21,25 ( D A C B -- ) 
  asm-1-11,15 asm-1-6,10 asm-1-0,5 h, ;

: asm-a-2-59 ( n "name" -- )
  name { n addr len }
  59 ['] asm-a-26,31-0 n addr len s" " asm-a-2-define
  59 ['] asm-a-26,31-1 n addr len s" ." asm-a-2-define ;

: asm-a-2-63 ( n "name" -- )
  name { n addr len }
  63 ['] asm-a-26,31-0 n addr len s" " asm-a-2-define
  63 ['] asm-a-26,31-1 n addr len s" ." asm-a-2-define ;

: asm-a-3-define ( {59,63} xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, ,
does> dup dup @ swap 1 cells + @ execute asm-2-21,25 asm-1-11,15 ( D A C -- ) 
  asm-1-6,10 asm-1-0,5 h, ;

: asm-a-3-59 ( n "name" -- )
  name { n addr len }
  59 ['] asm-a-26,31-0 n addr len s" " asm-a-3-define
  59 ['] asm-a-26,31-1 n addr len s" ." asm-a-3-define ;

: asm-a-3-63 ( n "name" -- )
  name { n addr len }
  63 ['] asm-a-26,31-0 n addr len s" " asm-a-3-define
  63 ['] asm-a-26,31-1 n addr len s" ." asm-a-3-define ;

: asm-a-4-define ( {59,63} xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, ,
does> dup dup @ swap 1 cells + @ execute asm-1-16,20 ( D B -- ) 
  asm-1-6,10 asm-1-0,5 h, ;

: asm-a-4-59 ( n "name" -- )
  name { n addr len }
  59 ['] asm-a-26,31-0 n addr len s" " asm-a-4-define
  59 ['] asm-a-26,31-1 n addr len s" ." asm-a-4-define ;

: asm-a-4-63 ( n "name" -- )
  name { n addr len }
  63 ['] asm-a-26,31-0 n addr len s" " asm-a-4-define
  63 ['] asm-a-26,31-1 n addr len s" ." asm-a-4-define ;

\ /a-form

\ common to {md,mds}-form
: calc-MB-ME ( ... n -- ... u )
  \ dup 32 < if 2 * else 2 * $20 or $3F and endif ;
  dup 32 < if 2 * else dup $1F and 1 lshift swap 5 rshift or endif ;

: asm-md,mds-21,26 ( ... addr code -- ... addr code )
  swap >r over 0 $40 check-range swap calc-MB-ME 5 lshift 
  or r> swap ;

\ end, common to {md,mds}-form

\ common to {mds,xs}-form
: asm-md,xs-16,20,30 ( ... n addr code -- ... addr code ) 
  swap >r over dup 0 $40 check-range $1F and 11 lshift swap >r swap 
  $20 and 4 rshift or r> or r> swap ; 

\ end, common to {mds,xs}-form

\ mds-form
: asm-mds-27,31-0 ( ... n -- ... code )
  1 lshift $1F and ;

: asm-mds-27,31-1 ( ... n -- ... code )
  1 lshift $1F and 1 or ;

: asm-mds-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 30 , 
does> dup dup @ swap 1 cells + @ execute ( A S B M{E,B} -- ) 
  asm-md,mds-21,26 asm-1-16,20 asm-1-6,10 asm-1-11,15 asm-1-0,5 h, ;

: asm-mds ( n "name" -- )
  name { n addr len }
  ['] asm-mds-27,31-0 n addr len s" " asm-mds-define
  ['] asm-mds-27,31-1 n addr len s" ." asm-mds-define ;

\ /mds-form

\ md-form
: asm-md-27,31-0 ( ... n -- ... code )
  2 lshift $1D and ;

: asm-md-27,31-1 ( ... n -- ... code )
  2 lshift $1D and 1 or ;

: asm-md-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 30 ,
does> dup dup @ swap 1 cells + @ execute ( A S SH M{E,B} -- )
  asm-md,mds-21,26 asm-md,xs-16,20,30  
  asm-1-6,10 asm-1-11,15 asm-1-0,5 h, ;

: asm-md ( n "name" -- )
  name { n addr len }
  ['] asm-md-27,31-0 n addr len s" " asm-md-define
  ['] asm-md-27,31-1 n addr len s" ." asm-md-define ;

\ /md-form

\ m-form
: asm-m-31-0 ( ... -- ... code )
  0 ;

: asm-m-31-1 ( ... -- ... code )
  1 ;

: asm-m-26,30 ( ... n addr code -- ... addr code )
  swap >r over 0 $20 check-range swap 1 lshift or r> swap ;

: asm-m-0,5 ( addr code -- code )
  swap @ 26 lshift or ;

: asm-m-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2,
does> dup 1 cells + @ execute ( A S SH MB ME -- )
  asm-m-26,30 asm-2-21,25 asm-1-16,20 asm-1-6,10 asm-1-11,15 asm-m-0,5 h, ;

: asm-m ( n "name" -- )
  name { n addr len }
  ['] asm-m-31-0 n addr len s" " asm-m-define
  ['] asm-m-31-1 n addr len s" ." asm-m-define ;

\ /m-form

\ xo-form
: asm-xo-21,31-00 ( ... n -- ... code )
  \ sets bit 21 to 0 and 31 to 0
  1 lshift ;

: asm-xo-21,31-01 ( ... n -- ... code )
  \ sets bit 21 to 0 and 31 to 1
  1 lshift 1 or ;

: asm-xo-21,31-10 ( ... n -- ... code )
  \ sets bit 21 to 1 and 31 to 0
  1 lshift $400 or ;

: asm-xo-21,31-11 ( ... n -- ... code )
  \ sets bit 21 to 1 and 31 to 1
  1 lshift $401 or ;

: asm-xo-1-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 31 ,
does> dup dup @ swap 1 cells + @ execute ( D A B -- )
  asm-1-16,20 asm-1-11,15 asm-1-6,10 asm-1-0,5 h, ;

: asm-xo-1 ( n "name" -- )
  name { n addr len }
  ['] asm-xo-21,31-00 n addr len s" " asm-xo-1-define 
  ['] asm-xo-21,31-01 n addr len s" ." asm-xo-1-define 
  ['] asm-xo-21,31-10 n addr len s" o" asm-xo-1-define
  ['] asm-xo-21,31-11 n addr len s" o." asm-xo-1-define  ;

: asm-xo-2-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 31 ,
does> dup dup @ swap 1 cells + @ execute ( D A -- )
  asm-1-11,15 asm-1-6,10 asm-1-0,5 h, ;

: asm-xo-2 ( n "name" -- )
  name { n addr len }
  ['] asm-xo-21,31-00 n addr len s" " asm-xo-2-define
  ['] asm-xo-21,31-01 n addr len s" ." asm-xo-2-define 
  ['] asm-xo-21,31-10 n addr len s" o" asm-xo-2-define
  ['] asm-xo-21,31-11 n addr len s" o." asm-xo-2-define ;

\ /xo-form

\ common to {x,xl,xfl,xfx}-form

: asm-4-21,31-0 ( ... n -- ... code )
  \ sets bit 31 to 0
  1 lshift ;

: asm-4-21,31-1 ( ... n -- ... code )
  \ set bit 31 to 1
  1 lshift 1 or ;

\ /end common to {x,xl,xfl,xfx}-form

\ common to {ds,x,xl,xfx}-form
: asm-3-0,5 ( addr code -- code )
  swap 1 cells + @ 26 lshift or ;

\ /end common to {ds,x,xl,xfx}-form

\ xl-form
: asm-xl-11,13 ( ... n -- ... code )
  swap >r over 0 $8 check-range swap 18 lshift or r> swap ;

: asm-xl-6,8 ( ... n -- ... code )
  swap >r over 0 $8 check-range swap 23 lshift or r> swap ;

: asm-xl-1-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 19 ,
does> dup dup @ swap 1 cells + @ execute ( BO BI -- )
  asm-1-11,15 asm-1-6,10 asm-1-0,5 h, ;

: asm-xl-1 ( n "name" -- )
  name { n addr len }
  ['] asm-4-21,31-0 n addr len s" " asm-xl-1-define
  ['] asm-4-21,31-1 n addr len s" l" asm-xl-1-define ;

: asm-xl-2 ( n "name" -- )
  create , 19 ,
does>   ( crbD crbA crbB -- ) 
  dup @ asm-4-21,31-0 asm-1-16,20 asm-1-11,15 asm-1-6,10 
  asm-3-0,5 h, ;

: asm-xl-3 ( n "name" -- )
  create , 19 ,
does> ( -- ) dup @ asm-4-21,31-0 asm-3-0,5 h, ;

: asm-xl-4 ( n "name" -- )
  create , 19 ,
does>  ( crfD crfS -- ) 
  dup @ asm-4-21,31-0 asm-xl-11,13 asm-xl-6,8 asm-3-0,5 h, ;

\ /xl-form

\ xs-form
: asm-xs-21,29-31-0 ( ... n -- ... code )
  \ sets bit 31 to 0
  2 lshift ;

: asm-xs-21,29-31-1 ( ... n -- ... code )
  2 lshift 1 or ;

: asm-xs-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 31 ,
does> dup dup @ swap 1 cells + @ execute ( A S SH -- )
  asm-md,xs-16,20,30 asm-1-6,10 asm-1-11,15 asm-1-0,5 h, ;

: asm-xs ( n "name" -- )
  name { n addr len }
  ['] asm-xs-21,29-31-0 n addr len s" " asm-xs-define
  ['] asm-xs-21,29-31-1 n addr len s" ." asm-xs-define ;

\ /xs-form

\ xfl-form
: asm-xfl-7,14 ( ... addr code -- ... addr code )
  swap >r over 0 $100 check-range swap 17 lshift or r> swap ;

: asm-xfl-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 63 ,
does> dup dup @ swap 1 cells + @ execute ( FM B -- )
  asm-1-16,20 asm-xfl-7,14 asm-1-0,5 h, ;

: asm-xfl ( n "name" -- )
  name { n addr len }
  ['] asm-4-21,31-0 n addr len s" " asm-xfl-define
  ['] asm-4-21,31-1 n addr len s" ." asm-xfl-define ;

\ /xfl-form

\ sc-form
: asm-sc ( n "name" -- )
  create ,
does> @ 26 lshift 2 or h, ;

\ /sc-form

\ xfx-form
: calc-SPR ( ... o -- ... spr )
  dup $1F and 5 lshift swap 5 rshift or ;

: asm-xfx-11,20 ( ...n addr code -- ... addr code )
  swap >r over 0 $400 check-range swap calc-SPR 11 lshift or r> swap ;

: asm-xfx-12,19 ( ...n addr code -- ... addr code )
  swap >r over 0 $100 check-range swap 12 lshift or r> swap ;

: asm-xfx-1 ( n "name" -- )
  create , 31 ,
does> ( D SPR -- )
  dup @ asm-4-21,31-0 asm-xfx-11,20 asm-1-6,10 asm-3-0,5 h, ;

: asm-xfx-2 ( n "name" -- )
  create , 31 ,
does> ( CRM S -- ) 
  dup @ asm-4-21,31-0 asm-1-6,10 asm-xfx-12,19 asm-3-0,5 h, ;

: asm-xfx-3 ( n "name" -- )
  create , 31 ,
does> ( SPR S -- )  
  dup @ asm-4-21,31-0 asm-1-6,10 asm-xfx-11,20 asm-3-0,5 h, ;

\ /xfx-form

\ x-form
: asm-x-10 ( ... n addr code -- ... addr code )
  swap >r over 0 $2 check-range swap 21 lshift or r> swap ;

: asm-x-6,8 ( ... n addr code -- ... addr code )
  swap >r over 0 $8 check-range swap 23 lshift or r> swap ;

: asm-x-11,13 ( ... n addr code -- ... addr code )
  swap >r over 0 $8 check-range swap 18 lshift or r> swap ;

: asm-x-12,15 ( ... n addr code -- ... addr code )
  swap >r over 0 $10 check-range swap 16 lshift or r> swap ;

: asm-x-16,19 ( ... n addr code -- ... addr code )
  swap >r over 0 $10 check-range swap 12 lshift or r> swap ;

: asm-x-1-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 31 ,
does> dup dup @ swap 1 cells + @ execute ( A S B -- )
  asm-1-16,20 asm-1-6,10 asm-1-11,15 asm-1-0,5 h, ;  

: asm-x-1 ( n "name" -- )
  name { n addr len }
  ['] asm-4-21,31-0 n addr len s" " asm-x-1-define
  ['] asm-4-21,31-1 n addr len s" ." asm-x-1-define ;

: asm-x-2 ( n "name" -- )
  \ bit 31 is 0
  create , 31 ,
does> ( S A B -- )
  dup @ asm-4-21,31-0 asm-1-16,20 asm-1-11,15 asm-1-6,10 asm-3-0,5 h, ;

: asm-x-2-1-define ( n "name" -- )
  \ bit 31 is 1
  concat nextname
  create , 31 ,
does> ( S A B -- )
  dup @ asm-4-21,31-1 asm-1-16,20 asm-1-11,15 asm-1-6,10 asm-3-0,5 h, ;

: asm-x-2-1 ( n "nane" "name" -- )
  name { addr len }
  addr len s" ." asm-x-2-1-define ;

: asm-x-3 ( n "name" -- )
  create , 31 ,
does> ( cdfD L A B -- )
  dup @ asm-4-21,31-0 asm-1-16,20 asm-1-11,15 asm-x-10 asm-x-6,8 asm-3-0,5 h, ;

: asm-x-4 ( n "name" -- )
  create , 31 , 
does> ( -- ) dup @ asm-4-21,31-0 asm-3-0,5 h, ;

: asm-x-5-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 31 ,
does> dup dup @ swap 1 cells + @ execute ( A S -- )
  asm-1-6,10 asm-1-11,15 asm-1-0,5 h, ;  

: asm-x-5 ( n "name" -- )
  name { n addr len }
  ['] asm-4-21,31-0 n addr len s" " asm-x-5-define
  ['] asm-4-21,31-1 n addr len s" ." asm-x-5-define ;

: asm-x-6 ( n "name" -- )
  create , 31 ,
does> ( A B -- ) dup @ asm-4-21,31-0 asm-1-16,20 asm-1-11,15 asm-3-0,5 h, ;

: asm-x-7 ( n "name" -- )
  create , 31 ,
does> ( crfD -- ) dup @ asm-4-21,31-0 asm-x-6,8 asm-3-0,5 h, ;

: asm-x-8-31 ( n "name" -- )
  create , 31 ,
does> ( D -- ) dup @ asm-4-21,31-0 asm-1-6,10 asm-3-0,5 h, ;

: asm-x-8-define-63 ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 63 ,
does> dup dup @ swap 1 cells + @ execute ( D -- )
  asm-1-6,10 asm-1-0,5 h, ;  

: asm-x-8-63 ( n "name" -- )
  name { n addr len }
  ['] asm-4-21,31-0 n addr len s" " asm-x-8-define-63
  ['] asm-4-21,31-1 n addr len s" ." asm-x-8-define-63 ;

: asm-x-9 ( n "name" -- )
  create , 31 ,
does> ( D SR -- ) dup @ asm-4-21,31-0 asm-x-12,15 asm-1-6,10 asm-3-0,5 h, ;

: asm-x-10-31 ( n "name" -- )
  create , 31 ,
does> ( D B -- ) dup @ asm-4-21,31-0 asm-1-16,20 asm-1-6,10 asm-3-0,5 h, ;

: asm-x-10-define-63 ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 63 ,
does> dup dup @ swap 1 cells + @ execute ( D B -- )
  asm-1-16,20 asm-1-6,10 asm-1-0,5 h, ;  

: asm-x-10-63 ( n "name" -- )
  name { n addr len }
  ['] asm-4-21,31-0 n addr len s" " asm-x-10-define-63
  ['] asm-4-21,31-1 n addr len s" ." asm-x-10-define-63 ;

: asm-x-11 ( n "name" -- )
  create , 31 ,
does> ( SR S -- ) dup @ asm-4-21,31-0 asm-1-6,10 asm-x-12,15 asm-3-0,5 h, ;

: asm-x-12 ( n "name" -- )
  create , 31 ,
does> ( B -- ) dup @ asm-4-21,31-0 asm-1-16,20 asm-3-0,5 h, ;

: asm-x-13 ( n "name" -- )
  create , 63 ,
does> ( crfD A B -- ) 
  dup @ asm-4-21,31-0 asm-1-16,20 asm-1-11,15 asm-x-6,8 asm-3-0,5 h, ;

: asm-x-14 ( n "name" -- )
  create , 63 ,
does> ( crfD crfS -- ) 
  dup @ asm-4-21,31-0 asm-x-11,13 asm-x-6,8 asm-3-0,5 h, ;

: asm-x-15-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 63 ,
does> dup dup @ swap 1 cells + @ execute ( crbD IMM -- )
  asm-x-16,19 asm-x-6,8 asm-1-0,5 h, ;  

: asm-x-15 ( n "name" -- )
  name { n addr len }
  ['] asm-4-21,31-0 n addr len s" " asm-x-15-define
  ['] asm-4-21,31-1 n addr len s" ." asm-x-15-define ;

: asm-x-string-16,20 ( ... n addr code -- ... addr code )
  swap >r over 0 $21 check-range swap dup 32 = if 0 swap drop then
  11 lshift or r> swap ; 

: asm-x-16 ( n "name" -- )
  create , 31 ,
does> ( S A NB -- )
  dup @ asm-4-21,31-0 asm-x-string-16,20 asm-1-11,15 asm-1-6,10 asm-3-0,5 h, ;

\ /x-form

\ ds-form
: asm-ds-11,15-30,31 ( ... A addr n -- ... addr code )
   swap >r over 0 $20 check-range swap 16 lshift or r> swap ;

: asm-ds-16,29 ( ... ds addr code -- ... addr code )
  swap >r over -$FFFF $10000 check-range swap dup 3 and 0<> -24 and throw 
  $FFFC and or r> swap ; 

: asm-ds-1 ( n "name" -- )
  create , 58 ,
does> ( D ds A -- ) 
  dup @ asm-ds-11,15-30,31 asm-ds-16,29 asm-1-6,10 asm-3-0,5 h, ;

: asm-ds-2 ( n "name" -- )
  create , 62 ,
does> ( S ds A -- ) 
  dup @ asm-ds-11,15-30,31 asm-ds-16,29 asm-1-6,10 asm-3-0,5 h, ;

\ /ds-form

\ d-form
: asm-d-0,5 ( ... n -- .... code )
  26 lshift ;

: asm-d-16,31-S ( ... n code -- ... code )
  \ for signed operands
  swap dup -$FFFF $10000 check-range $FFFF and or ;

: asm-d-16,31-U ( ... n code -- ... code )
  \ for unsigned operands
  swap dup 0 $10000 check-range or ;

: asm-d-11,15 ( ... n code -- ... code )
  swap dup 0 $20 check-range 16 lshift or ;

: asm-d-6,10 ( n code -- code )
  swap dup 0 $20 check-range 21 lshift or ;

: asm-d-10 ( n code -- code )
  swap dup 0 $2 check-range 21 lshift or ;

: asm-d-6,8 ( n code -- code )
  swap dup 0 $8 check-range 23 lshift or ;

: asm-d-oper-1 ( n "name" -- )
  create ,
does> ( D A SIMM -- )
  @ asm-d-0,5 asm-d-16,31-S asm-d-11,15 asm-d-6,10 h, ;

: asm-d-oper-2 ( n "name" -- )
  create ,
does> ( A S UIMM -- )
  @ asm-d-0,5 asm-d-16,31-U asm-d-6,10 asm-d-11,15 h, ;

: asm-d-load-store ( n "name" -- )
  create ,
does> ( S d A -- )
  @ asm-d-0,5 asm-d-11,15 asm-d-16,31-S asm-d-6,10 h, ;

: asm-d-compare-1 ( n "name" -- )
  create ,
does> ( crfD L A SIMM -- )
  @ asm-d-0,5 asm-d-16,31-S asm-d-11,15 asm-d-10 asm-d-6,8 h, ;

: asm-d-compare-2 ( n "name" -- )
  create ,
does> ( crfD L A UIMM -- )
  @ asm-d-0,5 asm-d-16,31-U asm-d-11,15 asm-d-10 asm-d-6,8 h, ;

\ /d-form

\ i-form
: calc-rel-i-offset ( n LI -- n LI' ) \ offset
  here - $3FFFFFC and ;

: asm-i-6,31-rel-0 ( LI addr n -- addr code )
  \ set LK=0 AA is set by definition in inst.fs
  swap >r swap calc-rel-i-offset swap 1 lshift or r> swap ;

: asm-i-6,31-rel-1 ( LI addr n -- addr code )
  \ set LK=1 AA is set by definitions in inst.fs
  swap >r swap calc-rel-i-offset swap 1 lshift or 1 or r> swap ;

: asm-i-6,31-abs-0 ( LI addr n -- addr code )
  \ set LK=0 AA is set by definition in inst.fs
  swap >r swap $3FFFFFC and swap 1 lshift or r> swap ;

: asm-i-6,31-abs-1 ( LI addr n -- addr code )
  \ set LK=1 AA is set by definitions in inst.fs
  swap >r swap $3FFFFFC and swap 1 lshift or 1 or r> swap ;

: asm-i-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 18 ,
does> dup dup @ swap 1 cells + @ execute ( LI -- ) asm-1-0,5 h, ;

: asm-i-reladdr ( n "name" -- )
  name { n addr len }
  ['] asm-i-6,31-rel-0 n addr len s" " asm-i-define 
  ['] asm-i-6,31-rel-1 n addr len s" l" asm-i-define  ;

: asm-i-absaddr ( n "name" -- )
  name { n addr len }
  ['] asm-i-6,31-abs-0 n addr len s" a" asm-i-define 
  ['] asm-i-6,31-abs-1 n addr len s" la" asm-i-define  ;

\ /i-form

\ b-form
: calc-rel-b-offset ( n BD -- n BD' ) \ offset
  here - $FFFC and ;

: asm-b-16,31-rel-0 ( BD addr n -- addr code )
  \ set LK=0 AA is set by definition in inst.fs
  swap >r swap calc-rel-b-offset swap 1 lshift or r> swap ;

: asm-b-16,31-rel-1 ( BD addr n -- addr code )
  \ set LK=1 AA is set by definitions in inst.fs
  swap >r swap calc-rel-b-offset swap 1 lshift or 1 or r> swap ;

: asm-b-16,31-abs-0 ( BD addr n -- addr code )
  \ set LK=0 AA is set by definition in inst.fs
  swap >r swap $FFFC and swap 1 lshift or r> swap ;

: asm-b-16,31-abs-1 ( BD addr n -- addr code )
  \ set LK=1 AA is set by definitions in inst.fs
  swap >r swap $FFFC and swap 1 lshift or 1 or r> swap ;

: asm-b-define ( xt n "name" "name" -- ) \ name as addr length
  concat nextname
  create 2, 16 ,
does> dup dup @ swap 1 cells + @ execute ( BO BI LI -- ) 
  asm-1-11,15 asm-1-6,10 asm-1-0,5 h, ;

: asm-b-reladdr ( n "name" -- )
  name { n addr len }
  ['] asm-b-16,31-rel-0 n addr len s" " asm-b-define 
  ['] asm-b-16,31-rel-1 n addr len s" l" asm-b-define  ;

: asm-b-absaddr ( n "name" -- )
  name { n addr len }
  ['] asm-b-16,31-abs-0 n addr len s" a" asm-b-define 
  ['] asm-b-16,31-abs-1 n addr len s" la" asm-b-define  ;

\ /b-form

include ./inst.fs

' rlwimi. alias .rlwimi
' rlwinm. alias .rlwinm
' rlwnm.  alias .rlwnm

previous
set-current
