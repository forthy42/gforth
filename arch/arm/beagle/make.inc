BEAGLE = ../arch/arm/beagle

EXTRAS = serial.c ns16550.c

AOBJECTS += $(EXTRAS:%.c=../arch/arm/beagle/%.o)

LDLIBS += -L $(shell dirname `$(CC) $(CFLAGS) -print-libgcc-file-name`) -lgcc -L $(shell dirname `$(CC) $(CFLAGS) -print-file-name=libg.a`) -lc -lg

DEPS += init.o

init.o:	../arch/arm/beagle/init.o
	$(CP) $< $@

gforth-ec.srec:	 gforth-ll-reg-ec
	arm-none-eabi-objcopy -O srec $< $@

all:	gforth-ec.srec