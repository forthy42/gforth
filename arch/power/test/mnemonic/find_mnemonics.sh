#!/bin/sh

# Autor Michal Revucky (c)
# Purpose: find certain mnemonics of form specified by $1 and write to a file
#          it writes only a specified number of lines into to result file
#          which is determined by $COUNT, the file name contains the mnemonic
#          too. it checks all binaries from DIRS, it places the result file
#          into ./<form>/$HOSTNAME.$1 it takes every mnemonic from $1/mnemonic
# Usage: ./find_mnemonic <form>

DIRS="/home/complang/micrev/gforth-20050128-ppc64/bin /bin /usr/bin
/usr/local/bin /usr/powerpc64-unknown-linux-gnu/gcc-bin/3.4.3
/usr/X11R6/bin /opt/Ice-2.0.0/bin"

COUNT=100

if [ $# -ne 1 ]; then 
  echo "usage: $0 <form>"
  exit
fi

if [ ! -d $1 ]; then 
  mkdir $1
fi

for j in `cat $1/mnemonics`; do
  echo "checking $j"
  touch $1/$HOSTNAME.$j;
  for k in $DIRS; do
    for l in `ls $k`; do
      if [ `wc -l $1/$HOSTNAME.$j | grep -o [0-9]*` -lt $COUNT ] ; then
        objdump -d $k/$l | grep $j | head -n1 >> $1/$HOSTNAME.$j ;
      fi
    done
  done
done
