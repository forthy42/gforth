
#include "mytypes.h"
#include "udp.h"
#include "interrupts.h"
#include "at91sam7s256.h"

#include "aic.h"
#include "systick.h"
#include "display.h"

#define EP_OUT	1
#define EP_IN	2

#define AT91C_PERIPHERAL_ID_UDP		11

#define AT91C_UDP_CSR0  ((AT91_REG *)   0xFFFB0030) 
#define AT91C_UDP_CSR1  ((AT91_REG *)   0xFFFB0034) 
#define AT91C_UDP_CSR2  ((AT91_REG *)   0xFFFB0038) 
#define AT91C_UDP_CSR3  ((AT91_REG *)   0xFFFB003C)

#define AT91C_UDP_FDR0  ((AT91_REG *)   0xFFFB0050) 
#define AT91C_UDP_FDR1  ((AT91_REG *)   0xFFFB0054) 
#define AT91C_UDP_FDR2  ((AT91_REG *)   0xFFFB0058) 
#define AT91C_UDP_FDR3  ((AT91_REG *)   0xFFFB005C) 

static U8 currentConfig;
static unsigned currentConnection;
static unsigned currentRxBank;
static unsigned usbTimeOut;

// Device descriptor
static const U8 dd[] = {
  0x12, 
  0x01,
  0x00,
  0x02,
  0x00,
  0x00,
  0x00, 
  0x08,
  0x94,
  0x06,
  0x02,
  0x00,
  0x00,
  0x00,
  0x00, 
  0x00, 
  0x01,
  0x01  
};

// Configuration descriptor
static const U8 cfd[] = {
  0x09,
  0x02,
  0x20,
  0x00, 
  0x01,
  0x01, 
  0x00,
  0xC0,
  0x00,
  0x09,
  0x04,
  0x00,
  0x00,
  0x02,
  0xFF, 
  0xFF,
  0xFF,
  0x00,
  0x07, 
  0x05,
  0x01,
  0x02,
  64,
  0x00,
  0x00, 
  0x07,
  0x05,
  0x82,
  0x02,
  64,
  0x00,
  0x00};

// Serial Number Descriptor
static U8 snd[] =
{
      0x1A,
      0x03, 
      0x31, 0x00,     // MSD of Lap (Lap[2,3]) in UNICode
      0x32, 0x00,     // Lap[4,5]
      0x33, 0x00,     // Lap[6,7]
      0x34, 0x00,     // Lap[8,9]
      0x35, 0x00,     // Lap[10,11]
      0x36, 0x00,     // Lap[12,13]
      0x37, 0x00,     // Lap[14,15]
      0x38, 0x00,     // LSD of Lap (Lap[16,17]) in UNICode
      0x30, 0x00,     // MSD of Nap (Nap[18,19]) in UNICode
      0x30, 0x00,     // LSD of Nap (Nap[20,21]) in UNICode
      0x39, 0x00,     // MSD of Uap in UNICode
      0x30, 0x00      // LSD of Uap in UNICode
};

static const U8 ld[] = {0x04,0x03,0x09,0x04}; // Language descriptor
      
extern void udp_isr_entry(void);

static int configured = 0;

static char x4[5];
static char* hexchars = "0123456789abcdef";
  
static char *
hex4(int i)
{
  x4[0] = hexchars[(i >> 12) & 0xF];
  x4[1] = hexchars[(i >> 8) & 0xF];
  x4[2] = hexchars[(i >> 4) & 0xF];
  x4[3] = hexchars[i & 0xF];
  x4[4] = 0;
  return x4;
}
 
void
udp_isr_C(void)
{
}

void
udp_check_interrupt()
{
  if (*AT91C_UDP_ISR & END_OF_BUS_RESET) 
  { 
  	display_goto_xy(0,0);
  	display_string("Bus Reset");
  	display_update();
	*AT91C_UDP_ICR = END_OF_BUS_RESET;          
	*AT91C_UDP_ICR = SUSPEND_RESUME;      
	*AT91C_UDP_ICR = WAKEUP;              
	configured = 0;
	currentConfig = 0;
	*AT91C_UDP_RSTEP = 0xFFFFFFFF;
	*AT91C_UDP_RSTEP = 0x0; 
	currentRxBank = AT91C_UDP_RX_DATA_BK0;
	*AT91C_UDP_FADDR = AT91C_UDP_FEN;    
	*AT91C_UDP_CSR0 = (AT91C_UDP_EPEDS | AT91C_UDP_EPTYPE_CTRL); 
  }
  else if (*AT91C_UDP_ISR & SUSPEND_INT)
  {
  	display_goto_xy(0,0);
  	display_string("Suspend");
  	display_update();
    if (configured == 1) configured = 2;
    else configured = 0;
	*AT91C_UDP_ICR = SUSPEND_INT;
	currentRxBank = AT91C_UDP_RX_DATA_BK0;
  }
  else if (*AT91C_UDP_ISR & SUSPEND_RESUME)
  {
  	display_goto_xy(0,0);
  	display_string("Resume");
  	display_update();
    if (configured == 2) configured = 1;
    else configured = 0;
    *AT91C_UDP_ICR = WAKEUP;
    *AT91C_UDP_ICR = SUSPEND_RESUME;
  }
  else if (*AT91C_UDP_ISR & AT91C_UDP_EPINT0)
  {
  	display_goto_xy(0,0);
  	display_string("Enumerate");
  	display_update();
    *AT91C_UDP_ICR = AT91C_UDP_EPINT0; 
	udp_enumerate();					
  } 
}

int
udp_init(void)
{
  //int i_state;

  /* Make sure the USB PLL and clock are set up */
  *AT91C_CKGR_PLLR |= AT91C_CKGR_USBDIV_1;
  *AT91C_PMC_SCER = AT91C_PMC_UDP;
  *AT91C_PMC_PCER = (1 << AT91C_ID_UDP);

  /* Enable the UDP pull up by outputting a zero on PA.16 */
  *AT91C_PIOA_PER = (1 << 16);
  *AT91C_PIOA_OER = (1 << 16);
  *AT91C_PIOA_CODR = (1 << 16);

  /* Set up default state */

  currentConfig = 0;
  currentConnection = 0;
  currentRxBank = AT91C_UDP_RX_DATA_BK0;

  /*i_state = interrupts_get_and_disable();

  aic_mask_off(AT91C_PERIPHERAL_ID_UDP);
  aic_set_vector(AT91C_PERIPHERAL_ID_UDP, AIC_INT_LEVEL_NORMAL,
		 (U32) udp_isr_entry);
  aic_mask_on(AT91C_PERIPHERAL_ID_UDP);

  if (i_state)
    interrupts_enable(); */

  return 1; 
}

void
udp_close(U32 u)
{
  /* Nothing */
}

void
udp_disable()
{
  *AT91C_PIOA_PER = (1 << 16);
  *AT91C_PIOA_OER = (1 << 16);
  *AT91C_PIOA_SODR = (1 << 16);
}

void 
udp_reset()
{
  udp_disable();  
  systick_wait_ms(1);
  udp_init();
}

int
udp_short_timed_out()
{
  return (USB_TIMEOUT < 
     ((((*AT91C_PITC_PIIR) & AT91C_PITC_CPIV) 
         - usbTimeOut) & AT91C_PITC_CPIV));
}

static int timeout_counter = 0;

int
udp_timed_out()
{
   if(udp_short_timed_out())
   {
      timeout_counter++;
      udp_short_reset_timeout();
   }
   return (timeout_counter > 500);
}

void
udp_reset_timeout()
{
  timeout_counter = 0;
  udp_short_reset_timeout();  
}

void
udp_short_reset_timeout()
{
  usbTimeOut = ((*AT91C_PITC_PIIR) & AT91C_PITC_CPIV);  
}

int
udp_read(U8* buf, int len)
{
  int packetSize = 0, i;
  
  if (udp_configured() != 1) return 0;
  
  if ((*AT91C_UDP_CSR1) & currentRxBank) // data to read
  {
  	packetSize = (*AT91C_UDP_CSR1) >> 16;
  	if (packetSize > len) packetSize = len;
  	
  	for(i=0;i<packetSize;i++) buf[i] = *AT91C_UDP_FDR1;
  	
  	*AT91C_UDP_CSR1 &= ~(currentRxBank);	

    if (currentRxBank == AT91C_UDP_RX_DATA_BK0) {	
      currentRxBank = AT91C_UDP_RX_DATA_BK1;
    } else {
      currentRxBank = AT91C_UDP_RX_DATA_BK0;
    }
  }
  return packetSize;
}

void
udp_write(U8* buf, int len)
{
  int i;
  
  if (configured != 1) return;
  
  for(i=0;i<len;i++) *AT91C_UDP_FDR2 = buf[i];
  
  *AT91C_UDP_CSR2 |= AT91C_UDP_TXPKTRDY;
  
  udp_reset_timeout();
  
  while ( !((*AT91C_UDP_CSR2) & AT91C_UDP_TXCOMP) )	
     if (udp_configured() != 1 || udp_timed_out()) return;
            
 (*AT91C_UDP_CSR2) &= ~(AT91C_UDP_TXCOMP);

  while ((*AT91C_UDP_CSR2) & AT91C_UDP_TXCOMP);
}

void 
udp_send_null()
{
   (*AT91C_UDP_CSR0) |= AT91C_UDP_TXPKTRDY;

   udp_reset_timeout();

   while ( !((*AT91C_UDP_CSR0) & AT91C_UDP_TXCOMP) && !udp_timed_out());

   (*AT91C_UDP_CSR0) &= ~(AT91C_UDP_TXCOMP);
   while ((*AT91C_UDP_CSR0) & AT91C_UDP_TXCOMP);
}

void udp_send_stall()
{
  (*AT91C_UDP_CSR0) |= AT91C_UDP_FORCESTALL;                           
  while ( !((*AT91C_UDP_CSR0) & AT91C_UDP_ISOERROR) );                    

  (*AT91C_UDP_CSR0) &= ~(AT91C_UDP_FORCESTALL | AT91C_UDP_ISOERROR);
  while ((*AT91C_UDP_CSR0) & (AT91C_UDP_FORCESTALL | AT91C_UDP_ISOERROR));
}

void udp_send_control(U8* p, int len, int send_null)
{
  int i = 0, j, tmp;
  
  do
  {
  	// send 8 bytes or less 

  	for(j=0;j<8 && i<len;j++)
  	{
  	  *AT91C_UDP_FDR0 = p[i++];
  	}

  	// Packet ready to send 
  	
  	(*AT91C_UDP_CSR0) |= AT91C_UDP_TXPKTRDY;
	udp_reset_timeout(); 	
    
  	do 
  	{
  	  tmp = (*AT91C_UDP_CSR0);

  	  if (tmp & AT91C_UDP_RX_DATA_BK0)
	  {

	    (*AT91C_UDP_CSR0) &= ~(AT91C_UDP_TXPKTRDY);

		(*AT91C_UDP_CSR0) &= ~(AT91C_UDP_RX_DATA_BK0);
        return;
	  }
  	}
  	while (!(tmp & AT91C_UDP_TXCOMP) && !udp_timed_out());
	
	(*AT91C_UDP_CSR0) &= ~(AT91C_UDP_TXCOMP);
    
  	while ((*AT91C_UDP_CSR0) & AT91C_UDP_TXCOMP);

  }
  while (i < len);
  // If needed send the null terminating data 
  if (send_null) udp_send_null();
  udp_reset_timeout();

  while(!((*AT91C_UDP_CSR0) & AT91C_UDP_RX_DATA_BK0) && !udp_timed_out());

  (*AT91C_UDP_CSR0) &= ~(AT91C_UDP_RX_DATA_BK0);

}

int
udp_configured()
{
  udp_check_interrupt();
  /*display_goto_xy(0,7);
  display_int(configured,1);
  display_update();*/
  return configured;
}

void 
udp_enumerate()
{
  U8 bt, br;
  int req, len, ind, val; 
  short status;
  
  if (!((*AT91C_UDP_CSR0) & AT91C_UDP_RXSETUP)) return;
  
  bt = *AT91C_UDP_FDR0;
  br = *AT91C_UDP_FDR0;
  
  val = ((*AT91C_UDP_FDR0 & 0xFF) | (*AT91C_UDP_FDR0 << 8));
  ind = ((*AT91C_UDP_FDR0 & 0xFF) | (*AT91C_UDP_FDR0 << 8));
  len = ((*AT91C_UDP_FDR0 & 0xFF) | (*AT91C_UDP_FDR0 << 8));
  
  if (bt & 0x80)
  {
    *AT91C_UDP_CSR0 |= AT91C_UDP_DIR; 
    while ( !((*AT91C_UDP_CSR0) & AT91C_UDP_DIR) );
  }
  
  *AT91C_UDP_CSR0 &= ~AT91C_UDP_RXSETUP;
  while ( ((*AT91C_UDP_CSR0)  & AT91C_UDP_RXSETUP)  );

  req = br << 8 | bt;
  
  /*
  if (1) {
  	display_goto_xy(0,0);
    display_string(hex4(req));
    display_goto_xy(4,0);
    display_string(hex4(val));
    display_goto_xy(8,0);
    display_string(hex4(ind));
    display_goto_xy(12,0);
    display_string(hex4(len));
    display_goto_xy(0,1);
    display_string("   ");
    display_update();
  }
  */
    
  switch(req)
  {
    case STD_GET_DESCRIPTOR: 
      if (val == 0x100) // Get device descriptor
      {
        udp_send_control((U8 *)dd, sizeof(dd), 0);
      }
      else if (val == 0x200) // Configuration descriptor
      {     
        udp_send_control((U8 *)cfd, (len < sizeof(cfd) ? len : sizeof(cfd)), (len > sizeof(cfd) ? 1 : 0));
        //if (len > sizeof(cfd)) udp_send_null();
      }	
      else if ((val & 0xF00) == 0x300)
      {
        switch(val & 0xFF)
        {
          case 0x00:
	        udp_send_control((U8 *)ld, sizeof(ld), 0);
            break;
          case 0x01:
		    udp_send_control(snd, sizeof(snd), 0);
            break;
          default:
			udp_send_stall();
        }
      }  
      else
      {
        udp_send_stall();
      }
      break;
        
    case STD_SET_ADDRESS:
      
      (*AT91C_UDP_CSR0) |= AT91C_UDP_TXPKTRDY;

      udp_reset_timeout();

      while(((*AT91C_UDP_CSR0) & AT91C_UDP_TXPKTRDY) && !udp_timed_out());
        
      *AT91C_UDP_FADDR = (AT91C_UDP_FEN | val);            
                                                                   
      *AT91C_UDP_GLBSTATE  = (val) ? AT91C_UDP_FADDEN : 0;
      
      break;
        
    case STD_SET_CONFIGURATION:

      configured = 1;
      currentConfig = val;
      udp_send_null(); 
      *AT91C_UDP_GLBSTATE  = (val) ? AT91C_UDP_CONFG : AT91C_UDP_FADDEN;

      *AT91C_UDP_CSR1 = (val) ? (AT91C_UDP_EPEDS | AT91C_UDP_EPTYPE_BULK_OUT) : 0; 
      *AT91C_UDP_CSR2 = (val) ? (AT91C_UDP_EPEDS | AT91C_UDP_EPTYPE_BULK_IN)  : 0;
      *AT91C_UDP_CSR3 = (val) ? (AT91C_UDP_EPTYPE_INT_IN)   : 0;      
      
      break;
      
    case STD_SET_FEATURE_ENDPOINT:

      ind &= 0x0F;

      if ((val == 0) && ind && (ind <= 3))
      {
        switch (ind)
        {
          case 1:   
            (*AT91C_UDP_CSR1) = 0;
            break;
          case 2:   
            (*AT91C_UDP_CSR2) = 0;
            break;
          case 3:   
            (*AT91C_UDP_CSR3) = 0;
            break;
        }
        udp_send_null();
      }
      else udp_send_stall();
      break;

    case STD_CLEAR_FEATURE_ENDPOINT:
      ind &= 0x0F;

      if ((val == 0) && ind && (ind <= 3))
      {                                             
        if (ind == 1) {
          (*AT91C_UDP_CSR1) = (AT91C_UDP_EPEDS | AT91C_UDP_EPTYPE_BULK_OUT); 
          (*AT91C_UDP_RSTEP) |= AT91C_UDP_EP1;
          (*AT91C_UDP_RSTEP) &= ~AT91C_UDP_EP1;
        } else if (ind == 2) {
          (*AT91C_UDP_CSR2) = (AT91C_UDP_EPEDS | AT91C_UDP_EPTYPE_BULK_IN);
          (*AT91C_UDP_RSTEP) |= AT91C_UDP_EP2;
          (*AT91C_UDP_RSTEP) &= ~AT91C_UDP_EP2;
        } else if (ind == 3) {
          (*AT91C_UDP_CSR3) = (AT91C_UDP_EPEDS | AT91C_UDP_EPTYPE_INT_IN);
          (*AT91C_UDP_RSTEP) |= AT91C_UDP_EP3;
          (*AT91C_UDP_RSTEP) &= ~AT91C_UDP_EP3; 
        }
        udp_send_null();
      }
      else udp_send_stall();

      break;
      
    case STD_GET_CONFIGURATION:                                   

      udp_send_control((U8 *) &(currentConfig), sizeof(currentConfig), 0);
      break;

    case STD_GET_STATUS_ZERO:
    
      status = 0x01; 
      udp_send_control((U8 *) &status, sizeof(status), 0);
      break;
      
    case STD_GET_STATUS_INTERFACE:

      status = 0;
      udp_send_control((U8 *) &status, sizeof(status), 0);
      break;

    case STD_GET_STATUS_ENDPOINT:

      status = 0;
      ind &= 0x0F;

      if (((*AT91C_UDP_GLBSTATE) & AT91C_UDP_CONFG) && (ind <= 3)) 
      {
        switch (ind)
        {
          case 1: 
            status = ((*AT91C_UDP_CSR1) & AT91C_UDP_EPEDS) ? 0 : 1; 
            break;
          case 2: 
            status = ((*AT91C_UDP_CSR2) & AT91C_UDP_EPEDS) ? 0 : 1;
            break;
          case 3: 
            status = ((*AT91C_UDP_CSR3) & AT91C_UDP_EPEDS) ? 0 : 1;
            break;
        }
        udp_send_control((U8 *) &status, sizeof(status), 0);
      }
      else if (((*AT91C_UDP_GLBSTATE) & AT91C_UDP_FADDEN) && (ind == 0))
      {
        status = ((*AT91C_UDP_CSR0) & AT91C_UDP_EPEDS) ? 0 : 1;
        udp_send_control((U8 *) &status, sizeof(status), 0);
      }
      else udp_send_stall();                                // Illegal request :-(

      break;
      
    case STD_SET_FEATURE_INTERFACE:
    case STD_CLEAR_FEATURE_INTERFACE:
      udp_send_null();
      break;
 
    case STD_SET_INTERFACE:     
    case STD_SET_FEATURE_ZERO:
    case STD_CLEAR_FEATURE_ZERO:
    default:
      udp_send_stall();
  } 
}






