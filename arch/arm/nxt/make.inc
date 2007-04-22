
AOBJECTS += ../arch/arm/nxt/aic.o ../arch/arm/nxt/bt.o ../arch/arm/nxt/byte_fifo.o ../arch/arm/nxt/display.o ../arch/arm/nxt/i2c.o ../arch/arm/nxt/nxt_lcd.o ../arch/arm/nxt/nxt_spi.o ../arch/arm/nxt/systick.o ../arch/arm/nxt/uart.o ../arch/arm/nxt/interrupts.o ../arch/arm/nxt/irq.o ../arch/arm/nxt/sound.o ../arch/arm/nxt/nxt_avr.o ../arch/arm/nxt/nxt_motors.o ../arch/arm/nxt/twi.o ../arch/arm/nxt/udp.o

DEPS += init.o vectors.o

init.o:	../arch/arm/nxt/init.o
	$(CP) $< $@

vectors.o:	../arch/arm/nxt/vectors.o
	$(CP) $< $@
