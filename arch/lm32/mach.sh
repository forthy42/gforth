#! /bin/sh

output="gforth-lm32.bin"

cp $1- ${output}
cp $1- $1

echo "*** Gforth-EC image for LM32/Milkymist stored in gforth-lm32.bin ***"