#!/bin/sh

FORMS="a b d ds i m md mds sc x xfl xfx xl xo xs"

echo "disassembler"
for k in  $FORMS; do
  ./test_disasm-inst.py -m -a $k | egrep 'form|Testcases' ;
  echo "==============="
done

echo "assembler"
for k in  $FORMS; do
  ./test_asm.py $k | egrep 'form|Testcases' ;
  echo "==============="
done
