#
# A minimal driver for running the dis/assembler tests
#
# This assumes a gforth with the ARM assembler already built-in on your path.
# If that is not the case, you can simply prepend asm.fs to SRC (after
# commenting out the code.fs require in asm.fs).
#

FS = gforth

SRC = asm.fs disasm.fs testdisasm.fs

.PHONY: all
all: check

.PHONY: check

testdisasm.res : $(SRC) FORCE
	$(FS) $(SRC) -e bye > testdisasm.res
.INTERMEDIATE: testdisasm.res

check: testdisasm.res
	sed -i testdisasm.res -e 's/( \$$[0-9A-F]\+ )//g'
	sed -e 's/( \$$[0-9A-F]\+ )//g' < testdisasm.out \
		| diff -U1 - testdisasm.res && echo OK

.PHONY: check-gen
.PHONY: FORCE
check-gen:
	$(FS) $(SRC) -e bye | tee testdisasm.out
