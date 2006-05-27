\ lauftext

rom

Create text
," GNU Forth EC R8C -- Microprocessor -- "
Create ledtable 1 c, 2 c, 4 c, 8 c, 4 c, 2 c,
Variable /text

: lauftext  task
  BEGIN  text count /text @ over mod /string
         16 min dup >r lcdpage lcdtype
         r@ 16 < IF  text 1+ 16 r@ - lcdtype  THEN
         rdrop 1 /text +!
         /text @ 6 mod ledtable + c@ led!
         6 adc@ 2/ ms
  AGAIN ;

ram
