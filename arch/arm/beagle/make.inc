BEAGLE = ../arch/arm/beagle

EXTRAS = serial.c ns16550.c
USBSER = usbtty.c core.c ep0.c musb_core.c musb_udc.c \
       omap3.c twl4030.c circbuf.c

AOBJECTS += $(EXTRAS:%.c=../arch/arm/beagle/%.o)

LDLIBS += -L $(shell dirname `$(CC) $(CFLAGS) -print-libgcc-file-name`) -lgcc -L $(shell dirname `$(CC) $(CFLAGS) -print-file-name=libg.a`) -lc -lg

DEPS += init.o

init.o:	../arch/arm/beagle/init.o
	$(CP) $< $@

%.srec:	 %
	arm-none-eabi-objcopy -O srec $< $@

%.exe:	%
	$(MAKE) -f arch/arm/beagle/make.inc $<.srec