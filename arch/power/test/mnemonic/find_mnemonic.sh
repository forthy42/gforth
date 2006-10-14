#!/bin/sh

# Autor Michal Revucky (c)
# Purpose: find a certain mnemonic specified by $1 and write to a file
#          it writes only a specified number of lines into to result file
#          which is determined by $COUNT, the file name contains the mnemonic
#          too. it checks all binaries from DIRS, it places the result file
#          into ./<form>/$HOSTNAME.$1
# Usage: ./find_mnemonic <mnemonic> <form>

DIRS="/home/complang/micrev/gforth-20050128-ppc64/bin /bin /usr/bin
/usr/local/bin /usr/powerpc64-unknown-linux-gnu/gcc-bin/3.4.3
/usr/X11R6/bin /opt/Ice-2.0.0/bin"

COUNT=100

if [ $# -ne 2 ]; then 
  echo "usage: $0 <mnemonic> <form>"
  exit
fi

if [ ! -d $2 ]; then 
  mkdir $2
fi

for k in $DIRS; do
  for l in `ls $k`; do
    objdump -d $k/$l | grep $1 | head -n1 >> $2/$HOSTNAME.$1
#    if [ `wc -l $2/$HOSTNAME.$1 | grep -o [0-9]*` -ge $COUNT ] ; then
#      exit
#    fi
  done
done
