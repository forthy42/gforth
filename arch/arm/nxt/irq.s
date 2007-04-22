

@
@  IRQ wrappers.
@
@  These call a C function.
@  We switch to supervisor mode and reenable interrupts to allow nesting.
@
@

  .text
  .code 32
  .align   2

@
@  Macros
@

  .macro   irq_wrapper_nested, C_function

@ Save registers on stack
  sub r14,r14,#4 @ fix up for return
  stmfd r13!,{r14}
  mrs  r14,spsr
  stmfd r13!,{r14}

@ Acknowledge the IVR for debugging to support Protected Mode
  ldr   r14,=0xFFFFF100
  str   r14,[r14]

 @ swich to system mode and enable IRQ, but not FIQ
  msr cpsr_c,#0x5F

  @push stack
  stmfd r13!,{r0-r12,r14}


@ Call the function
  ldr r0,=\C_function
  mov lr,pc
  bx  r0

  @ pop stack
   ldmfd r13!,{r0-r12,r14}

 @ swich to interrupt mode and disable IRQs and FIQs
  msr cpsr_c,#0xD2

@End of interrupt by doing a write to AIC_EOICR
  ldr  r14,=0xFFFFF130
  str  r14,[r14]

  @ Unstack the saved spsr
  ldmfd r13!,{r14}
  msr  spsr_all,r14

  @ Return from interrupt (unstacking the modified r14)
  ldmfd r13!,{pc}^

  .endm

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
  .macro   irq_wrapper_not_nested, C_function

@ Save registers on stack
  sub r14,r14,#4 @ fix up for return
  stmfd r13!,{r0-r12,r14}

@ Acknowledge the IVR for debugging to support Protected Mode
  ldr   r14,=0xFFFFF100
  str   r14,[r14]

@ Call the function
  ldr r0,=\C_function
  mov lr,pc
  bx  r0

@End of interrupt by doing a write to AIC_EOICR
  ldr  r14,=0xFFFFF130
  str  r14,[r14]

  @ Return from interrupt (unstacking the modified r14)
  ldmfd r13!,{r0-r12,pc}^

  .endm

@
@	ISRs
@
@


	.global spurious_isr
	.global default_isr
	.global default_fiq
default_fiq:
spurious_isr:
default_isr:
	b default_isr

  .extern systick_isr_C
  .global systick_isr_entry
systick_isr_entry:
  irq_wrapper_nested systick_isr_C

  .extern systick_low_priority_C
  .global systick_low_priority_entry
systick_low_priority_entry:
  irq_wrapper_nested systick_low_priority_C

  .extern udp_isr_C
  .global udp_isr_entry
udp_isr_entry:
  irq_wrapper_nested udp_isr_C

  .extern spi_isr_C
  .global spi_isr_entry
spi_isr_entry:
  irq_wrapper_nested spi_isr_C

  .extern twi_isr_C
  .global twi_isr_entry
twi_isr_entry:
  irq_wrapper_nested twi_isr_C
  
  .extern sound_isr_C
  .global sound_isr_entry
sound_isr_entry:
  irq_wrapper_nested sound_isr_C

  .extern uart_isr_C_0
  .global uart_isr_entry_0
uart_isr_entry_0:
  irq_wrapper_nested uart_isr_C_0

  .extern uart_isr_C_1
  .global uart_isr_entry_1
uart_isr_entry_1:
  irq_wrapper_nested uart_isr_C_1

  .extern nxt_motor_isr_C
  .global nxt_motor_isr_entry
nxt_motor_isr_entry:
  irq_wrapper_nested nxt_motor_isr_C


  .extern i2c_timer_isr_C
  .global i2c_timer_isr_entry
i2c_timer_isr_entry:
  irq_wrapper_nested i2c_timer_isr_C

