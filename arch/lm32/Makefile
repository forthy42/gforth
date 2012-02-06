# test-case for lm32 assembler

lm32-objdump = /opt/rtems-4.11/bin/lm32-rtems4.11-objdump  

all: check

check: testasm.disas testasm.disas-ok
	diff $^

testasm.bin: testasm.fs asm.fs FORCE
	gforth ./$< -e bye

testasm.disas: testasm.bin
	$(lm32-objdump) -EB -b binary -m lm32 -D $< > $@

testasm.bin: FORCE

FORCE:

.PHONY: FORCE


