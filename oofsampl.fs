\ oof.fs	Object Oriented FORTH
\ 		This file is (c) 1996 by Bernd Paysan
\			e-mail: paysan@informatik.tu-muenchen.de
\
\		Please copy and share this program, modify it for your system
\		and improve it as you like. But don't remove this notice.
\
\		Thank you.
\

\ Data structures: data                                28nov93py

: place ( addr1 n addr2 -- )
    over >r rot over 1+ r> move c! ;

: i!  postpone ! ; immediate
: i@  postpone @ ; immediate

object class data       \ abstract data class
        cell var ref  \ reference counter
        method !        method @        method .
        method null     method atom?    method #
how:    : atom? ( -- flag )  true ;
        : #     ( -- n )     0 ;
        : null  ( -- addr )  new ;
class;

\ Data structures: int                                 30apr93py

data class int
        cell var value
how:    : !     value i! ;
        : @     value i@ ;
        : .     @ 0 .r ;
        : init  ( data -- )  ! ;
        : dispose  -1 ref +!
          ref i@ 0> 0= IF  super dispose  THEN ;
        : null  0 new ;
class;



\ Data structures: list                                17nov93py

0 Value nil

data class lists
        data ptr first  data ptr next
        method empty?   method ?
how:    : null  nil ;
        : atom? false ;
class;

lists class nil-class

how:    : empty? true ;
        : dispose ;
        : .  ." ()" ;
class;

nil-class : (nil                (nil self TO nil
nil (nil bind first             nil (nil bind next

    
\ Data structures: list                                12mar94py

lists class linked
how:    : empty? false ;
        : #      next # 1+ ;
        : ?      first . ;
        : @      first @ ;
        : !      first ! ;
        : init ( first next -- )
                 dup >o 1 ref +! o> bind next
		 dup >o 1 ref +! o> bind first ;
        : .      self >o  [char] (
                 BEGIN  emit ? next atom? next self o> >o
                        IF ."  . " data . o> ." )" EXIT THEN bl
                        empty?  UNTIL  o> drop ." )" ;
        : dispose  -1 ref +!  ref i@ 0> 0=
          IF   first dispose  next dispose  super dispose  THEN ;
class;
      
\ Data structures: string                              04dec93py

int class string
how:    : !     ( addr count -- )
                value i@ over 1+ resize throw value i!
                value i@ place ;
        : @     ( -- addr count )  value i@ count ;
        : .     @ type ;
        : init  ( addr count -- )
          dup 1+ allocate throw value i! value i@ place ;
        : null  S" " new ;
        : dispose ref i@ 1- 0> 0=
          IF  value i@ free throw  THEN  super dispose ;
class;

\ Data sturctures: pointer                             17nov93py

data class pointer
        data ptr container
        method ptr!
how:    : !     container ! ;
        : @     container @ ;
        : .     container . ;
        : #     container # ;
        : init  ( data -- )  dup >o 1 ref +! o> bind container ;
        : ptr!  ( data -- )  container dispose  init ;
        : dispose  -1 ref +!  ref i@ 0> 0=
          IF  container dispose super dispose  THEN ;
        : null  nil new ;
class;

\ Data sturctures: array                               30apr93py

data class array
        data [] container
        cell var range
how:    : !     container ! ;
        : @     container @ ;
        : .     [char] [
                # 0 ?DO emit I container . [char] , LOOP drop ." ]" ;
        : init  ( data n -- )  range i!  bind container ;
        : dispose  -1 ref +!  ref i@ 0> 0=
          IF  # 0 ?DO  I container dispose  LOOP
              super dispose  THEN ;
        : null  nil 0 new ;
        : #     range i@ ;
        : atom? false ;
class;

\ Data structure utilities                             17nov93py

: cons  linked new ;
: list  nil cons ;
: car   >o lists first self o> ;
: cdr   >o lists next  self o> ;
: print >o data . o> ;
: ddrop >o data dispose o> ;
: make-string string new ;
: $"    state @ IF  postpone S" postpone make-string exit  THEN
  [char] " parse make-string ;       immediate

\ Examples

$" This" $" is" $" a" list cons $" example" $" list" list cons list cons cons
cr dup print
cr dup car print
cr dup cdr cdr car print
pointer : list1
cr list1 .

1 2 3  3 int new[] 3 array : lotus
cr lotus .
cr 2 lotus @ .
cr 0 lotus @ .
cr 5 1 lotus ! lotus .

\ Interface test

interface bla
method fasel
method blubber
method Hu
how:
: fasel ." Bla Fasel" Hu ;
: blubber ." urps urps" Hu fasel ;
interface;

object class test
bla
method .
how:
: Hu ." ! " ;
: . fasel ;
class;

test : test1
cr test1 fasel
cr test1 blubber
cr test1 .
cr test1 self >o bla blubber o>

\ This should output the following lines:
\
\ (This (is a) (example list))
\ This
\ (example list)
\ (This (is a) (example list))
\ [1,2,3]
\ 3 
\ 1 
\ [1,5,3]
\ Bla Fasel! 
\ urps urps! Bla Fasel!
\ Bla Fasel! 
\ urps urps! Bla Fasel!

