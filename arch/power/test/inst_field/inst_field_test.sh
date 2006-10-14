#!/bin/sh

gforth='/home/complang/micrev/gforth-20050128-ppc64/bin/gforth-fast'
disasm='/home/complang/micrev/praktikum/ppc/disasm.fs'

# ranges
echo "testing single:";
echo "---------------";
for k in $(cat befehle_binaer);
do
  for l in $(cat to_test);
  do
    first=`echo $l | cut -d, -f1`
    second=`echo $l | cut -d, -f2`
    len=`echo $second - $first + 1 | bc`
    echo -n "$k disasm-$first,$second: ";
    $gforth $disasm -e "%$k disasm-$first,$second dup hex. %${k:$first:$len}
    dup hex. = . bye"
    echo ""
    #$gforth -e "%${k:$first:$len} hex. cr bye"
    #$gforth -e "%$(echo $k | awk '{print substr($0,$pos,$len)}') hex. cr bye"
    #echo "len: $len, pos: $first, ${k:$first:$len}"
  done
done

# single bits
echo "testing single bits:";
echo "--------------------";
for k in $(cat befehle_binaer);
do
  for l in $(cat to_test2);
  do
    bit=`echo $l`
    echo -n "$k disasm-$bit: ";
    $gforth $disasm -e "%$k disasm-$bit dup hex. %${k:$bit:1} dup hex. = . bye"
    echo ""
    #$gforth -e "%${k:$first:$len} hex. cr bye"
    #$gforth -e "%$(echo $k | awk '{print substr($0,$pos,$len)}') hex. cr bye"
    #echo "len: $len, pos: $first, ${k:$first:$len}"
  done
done
