\ lauftext

rom

Create text
," Forth Tagung 2006 -- GNU Forth EC R8C -- "
Create ledtable 1 c, 2 c, 4 c, 8 c, 4 c, 2 c,
Variable /text

: lauftext  task
  BEGIN  text count /text @ over mod /string
         16 min dup >r lcdpage lcdtype
         r@ 16 < IF  text 1+ 16 r@ - lcdtype  THEN
         rdrop 1 /text +!
         /text @ 6 mod ledtable + c@ led!
         &200 0 DO pause 1 ms LOOP
  AGAIN ;

ram
