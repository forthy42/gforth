#include "nxt_avr.h"


#include "twi.h"
#include "nxt_motors.h"

#include "systick.h"
#include <string.h>




#define NXT_AVR_ADDRESS 1
#define NXT_AVR_N_OUTPUTS 4
#define NXT_AVR_N_INPUTS  4


const char avr_brainwash_string[] =
  "\xCC" "Let's samba nxt arm in arm, (c)LEGO System A/S";

static U32 nxt_avr_initialised;

static struct {
  U8 power;
  U8 pwm_frequency;
  S8 output_percent[NXT_AVR_N_OUTPUTS];
  U8 output_mode;
  U8 input_power;
} io_to_avr;


static struct {
  U16 adc_value[NXT_AVR_N_INPUTS];
  U16 buttons;
  U16 battery_is_AA;
  U16 battery_mV;
  U8 avr_fw_version_major;
  U8 avr_fw_version_minor;
} io_from_avr;

static U8 data_from_avr[(2 * NXT_AVR_N_INPUTS) + 5];

static U8 data_to_avr[5 + NXT_AVR_N_OUTPUTS];



/* We're assuming that we get good packing */

static void
nxt_avr_start_read(void)
{
  memset(data_from_avr, 0, sizeof(data_from_avr));
  twi_start_read(NXT_AVR_ADDRESS, 0, 0, data_from_avr, sizeof(data_from_avr));
}

static void
nxt_avr_start_send(void)
{
  int i;
  U8 checkByte = 0;
  U8 *a = data_to_avr;
  U8 *b = (U8 *) (&io_to_avr);

  i = sizeof(io_to_avr);
  while (i) {
    *a = *b;
    checkByte += *b;
    a++;
    b++;
    i--;
  }

  *a = ~checkByte;

  twi_start_write(NXT_AVR_ADDRESS, 0, 0, data_to_avr, sizeof(data_to_avr));

}

void
nxt_avr_power_down(void)
{
  io_to_avr.power = 0x5a;
  io_to_avr.pwm_frequency = 0x00;
}


void
nxt_avr_firmware_update_mode(void)
{
  io_to_avr.power = 0xA5;
  io_to_avr.pwm_frequency = 0x5A;
}

void
nxt_avr_link_init(void)
{
  twi_start_write(NXT_AVR_ADDRESS, 0, 0, (const U8 *) avr_brainwash_string,
		  strlen(avr_brainwash_string));
}


static U16
Unpack16(const U8 *x)
{
  U16 retval;

  retval = (((U16) (x[0])) & 0xff) | ((((U16) (x[1])) << 8) & 0xff00);
  return retval;
}


static struct {
  U32 good_rx;
  U32 bad_rx;
  U32 resets;
  U32 still_busy;
  U32 not_ok;
} nxt_avr_stats;

static void
nxt_avr_unpack(void)
{
  U8 check_sum;
  U8 *p;
  U16 buttonsVal;
  U32 voltageVal;
  int i;

  p = data_from_avr;

  for (check_sum = i = 0; i < sizeof(data_from_avr); i++) {
    check_sum += *p;
    p++;
  }

  if (check_sum != 0xff) {
    nxt_avr_stats.bad_rx++;
    return;
  }

  nxt_avr_stats.good_rx++;

  p = data_from_avr;

  // Marshall
  for (i = 0; i < NXT_AVR_N_INPUTS; i++) {
    io_from_avr.adc_value[i] = Unpack16(p);
    p += 2;
  }

  buttonsVal = Unpack16(p);
  p += 2;


  io_from_avr.buttons = 0;

  if (buttonsVal > 1023) {
    io_from_avr.buttons |= 1;
    buttonsVal -= 0x7ff;
  }

  if (buttonsVal > 720)
    io_from_avr.buttons |= 0x08;
  else if (buttonsVal > 270)
    io_from_avr.buttons |= 0x04;
  else if (buttonsVal > 60)
    io_from_avr.buttons |= 0x02;

  voltageVal = Unpack16(p);

  io_from_avr.battery_is_AA = (voltageVal & 0x8000) ? 1 : 0;
  io_from_avr.avr_fw_version_major = (voltageVal >> 13) & 3;
  io_from_avr.avr_fw_version_minor = (voltageVal >> 10) & 7;


  // Figure out voltage
  // The units are 13.848 mV per bit.
  // To prevent fp, we substitute 13.848 with 14180/1024

  voltageVal &= 0x3ff;		// Toss unwanted bits.
  voltageVal *= 14180;
  voltageVal >>= 10;
  io_from_avr.battery_mV = voltageVal;

}


void
nxt_avr_init(void)
{
  twi_init();

  memset(&io_to_avr, 0, sizeof(io_to_avr));
  io_to_avr.power = 0;
  io_to_avr.pwm_frequency = 8;

  nxt_avr_initialised = 1;
}

static U32 update_count;
static U32 link_init_wait;
static U32 link_running;

void
nxt_avr_1kHz_update(void)
{

  if (!nxt_avr_initialised)
    return;

  if (link_init_wait) {
    link_init_wait--;
    return;
  }

  if (!twi_ok()) {
    nxt_avr_stats.not_ok++;
    link_running = 0;
  }

  if (twi_busy()) {
    nxt_avr_stats.still_busy++;
    link_running = 0;
  }


  if (!twi_ok() || twi_busy() || !link_running) {
    memset(data_from_avr, 0, sizeof(data_from_avr));
    link_running = 1;
    nxt_avr_link_init();
    link_init_wait = 2;
    update_count = 0;
    nxt_avr_stats.resets++;
    return;
  }

  if (update_count & 1) {
    nxt_avr_start_read();
  } else {
    nxt_avr_unpack();
    nxt_avr_start_send();
  }
  update_count++;
}

U32
buttons_get(void)
{
  return io_from_avr.buttons;
}

U32
battery_voltage(void)
{
  return io_from_avr.battery_mV;
}

U32
sensor_adc(U32 n)
{
  if (n < 4)
    return io_from_avr.adc_value[n];
  else
    return 0;
}


void
nxt_avr_set_motor(U32 n, int power_percent, int brake)
{
  if (n < NXT_N_MOTORS) {
    io_to_avr.output_percent[n] = power_percent;
    if (brake)
      io_to_avr.output_mode |= (1 << n);
    else
      io_to_avr.output_mode &= ~(1 << n);
  }
}

void
nxt_avr_set_input_power(U32 n, U32 power_type)
{
  // The power to the sensor is controlled by a bit in
  // each of the two nibbles of the byte. There is one
  // bit for each of the four sensors. if the low nibble
  // bit is set then the sensor is "ACTIVE" and 9v is
  // supplied to it but it will be pulsed off to allow
  // the sensor to be be read. A 1 in the high nibble
  // indicates that it is a 9v always on sensor and
  // 9v will be supplied constantly. If both bits are
  // clear then 9v is not supplied to the sensor. 
  // Having both bits set is currently not supported.
  if (n < NXT_AVR_N_INPUTS && power_type <= 2) {
    U8 val = (power_type & 0x2 ? 0x10 << n : 0) | ((power_type & 1) << n);
    io_to_avr.input_power &= ~(0x11 << n);
    io_to_avr.input_power |= val;
  }
}
