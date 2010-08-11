#
# A minimal driver for running the assembler tests
#

FS = ../../gforth -i ../../kernl64l.fi ../../exboot.fs ../../startup.fs

PWD := $(shell pwd)
SRC = $(PWD)/asm.fs $(PWD)/testasm.fs

.PHONY: all
all: check

.INTERMEDIATE: testdisasm.res
testdisasm.res: $(SRC) FORCE
	$(FS) $(SRC) -e 'bye' | sed -f testdisasm.sed > $@

.PHONY: check
check: testdisasm.res
	diff -U1 testdisasm.out testdisasm.res && echo OK


.PHONY: check-gen
check-gen:
	$(FS) $(SRC) -e 'bye' | sed -f testdisasm.sed \
		| tee testdisasm.out

.PHONY: FORCE
