#
# A minimal driver for running the dis/assembler tests
#
# This assumes a gforth with the ARM assembler already built-in on your path.
# If that is not the case, you can simply prepend asm.fs to SRC (after
# commenting out the code.fs require in asm.fs).
#

FS = gforth

SRC = disasm.fs testdisasm.fs

.PHONY: all
all: check

.PHONY: check
check:
	@trap "rm -f testdisasm.res" 0; \
	    $(FS) $(SRC) -e bye > testdisasm.res && \
	    diff -U1 testdisasm.out testdisasm.res && echo OK

.PHONY: check-gen
check-gen:
	$(FS) $(SRC) -e bye | tee testdisasm.out
