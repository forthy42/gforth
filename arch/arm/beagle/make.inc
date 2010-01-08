BEAGLE = ../arch/arm/beagle

EXTRAS = serial.c ns16550.c usbtty.c core.c ep0.c musb_core.c musb_udc.c \
       omap3.c twl4030.c circbuf.c

AOBJECTS += $(EXTRAS:%.c=../arch/arm/beagle/%.o)

LDLIBS += -L $(shell dirname `$(CC) $(CFLAGS) -print-libgcc-file-name`) -lgcc -L $(shell dirname `$(CC) $(CFLAGS) -print-file-name=libg.a`) -lc -lg

DEPS += init.o

init.o:	../arch/arm/beagle/init.o
	$(CP) $< $@

gforth-ec.srec:	 gforth-ll-reg-ec
	arm-none-eabi-objcopy -O srec $< $@

all:	gforth-ec.srec