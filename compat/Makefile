FILES=README package.4th anslocal.fs assert.fs caseext.fs control.fs defer.fs \
	exception.fs execute-parsing.fs loops.fs macros.fs required.fs \
	strcomp.fs struct.fs vocabulary.fs xlocals.fs

all: $(FILES)
	rm -f ../compat.zip
	cd .. && zip -9r compat.zip $(foreach file, $(FILES), compat/$(file))

