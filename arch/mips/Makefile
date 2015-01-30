#
# A minimal driver for running the dis/assembler tests
#

FS = gforth

SRC = ./asm.fs ./disasm.fs 

.PHONY: all
all: check

.INTERMEDIATE: testasm.res
testasm.res: $(SRC) ./testasm.fs FORCE
	$(FS) $(SRC) ./testasm.fs -e bye 
	objdump -D -b binary -EL -m mips:isa64r2 testasm.bin > $@

testdisasm.res: $(SRC) ./testdisasm.fs FORCE
	$(FS) $(SRC) ./testdisasm.fs -e bye | sed -f testdisasm.sed > $@

.PHONY: check
check: testasm.res testdisasm.res
	diff -U1 testasm.out testasm.res && echo Assembler OK
	diff -U1 testdisasm.out testdisasm.res && echo Disassembler OK

.PHONY: check-gen
check-gen:
	$(FS) $(SRC) ./testasm.fs -e bye 
	objdump -D -b binary -EL -m mips:isa64r2 testasm.bin | tee testasm.out
	$(FS) $(SRC) ./testdisasm.fs -e bye | sed -f testdisasm.sed | tee testdisasm.out

.PHONY: FORCE
