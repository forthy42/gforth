
GFORTHHOME = ..

include $(GFORTHHOME)/engine/Makefile

%.c: %.pri
	cat $*.h > $@
	$(GFORTHHOME)/gforth -e "include $(GFORTHHOME)/prims2cl.fs file $< main bye" >> $@

%.so: %.c
	$(GCC) -shared -I$(GFORTHHOME) $(CFLAGS) $< -o $@

unixlib.c: unixlib.pri unixlib.h

