#
# A minimal driver for running the dis/assembler tests
#

FS = gforth

SRC = asm.fs disasm.fs testdisasm.fs

.PHONY: all
all: check

.INTERMEDIATE: testdisasm.res
testdisasm.res: $(SRC) FORCE
	$(FS) $(SRC) -e bye | sed -f testdisasm.sed > testdisasm.res

.PHONY: check
check: testdisasm.res
	diff -U1 testdisasm.out testdisasm.res && echo OK

.PHONY: check-gen
check-gen:
	$(FS) $(SRC) -e bye | sed -f testdisasm.sed | tee testdisasm.out

.PHONY: FORCE
