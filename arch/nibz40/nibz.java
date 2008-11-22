/* This file is (C)2008 Simon Jackson, BEng.
 * It may be distributed along with Gforth, but is not GPL
 * This file implements a nibz40 virtual machine
 * and the ufm flash and 8 word port interface
 * The ufm 512 image and the midlet are passed as parameters
 */

package nibz40;

import javax.microedition.lcdui.*;

/**
 *
 * @author Jacko
 */
public class VM implements Runnable {
    //registers and trace address
    short p, q, r, s, c, ir, addr;
    //breakpoint address
    short brk;
    //accumulation temp
    long acc;
    //memory and ports
    /* the ports are located from address 0:
     * 0: Control register (direction in=1; out=0) port X
     * 1: Data register port X
     * 2: Control Y
     * 3: Data Y
     * 4: Control Z (half Register/Lower Half)
     * 5: Data Z
     * 6: Control Address (Special IO)
     * 7: Data Special
     */
    short[] m, flash, port;
    //carry flag and running
    boolean cf, running;
    //the emulator program class
    NibzEmu prog;
    //khz calculation
    long t,cnt,sdt;
    //frequency and trace register
    public int hz, reg;
    //hidden thread
    Thread th;

    static String[] regs = {
        "cx","dx","cy","dy","cz","dz","ad","da",
        "p=","q=","r=","s=","c=","cf","ir","hz"};
    static String[] ops = {
        "BA","FI","RI","SI",
        "GO","DI","BO","SU",
        "RO","FA","RA","SA",
        "SO","FE","RE","SE"
        };
    static String[] hex = {
        "0","1","2","3","4","5","6","7",
        "8","9","A","B","C","D","E","F"
    };

    public VM(short[] fl, NibzEmu ctl) {
        super();
        reset();
        m=new short[65536];
        flash=fl;
        port=new short[8];
        prog=ctl;
        brk=(short)0xFFFF;
        th = new Thread(this);
    }

    //reset the machine
    public void reset() {
        p=q=r=s=0;
        ir=0;
        cf=false;
        for(int i=0;i<4;i++) {
            poke(i*2,(short)-1,1);
            poke(i*2+1,(short)0,1);
        }
    }

    //register list display
    public void fillProc(List proc) {
        proc.deleteAll();
        for(int i=0;i<8;i++) {
            proc.append(regs[i]+" "+toHex(m[i]), null);
        }
        proc.append(regs[8]+" "+toHex(p), null);
        proc.append(regs[9]+" "+toHex(q), null);
        proc.append(regs[10]+" "+toHex(r), null);
        proc.append(regs[11]+" "+toHex(s), null);
        proc.append(regs[12]+" "+toHex(c), null);
        proc.append(regs[13]+" "+toHex(cf?0:-1), null);
        proc.append(regs[14]+" "+toHex(ir), null);
        proc.append(regs[15]+" "+toHex(hz), null);
        proc.setSelectedIndex(0, true);
    }

    //trace dissembly display
    public void fillTrace(List proc, List trace) {
        int i=proc.getSelectedIndex();
        addr=0;
        reg=0;
        if(i>0 && i<8) {
            addr=m[i];
            reg = 1;
        } else {
            switch(i) {
                case 8: addr=p; reg = 0; break;
                case 9: addr=q; reg = 1; break;
                case 10: addr=r; reg = 2; break;
                case 11: addr=s; reg = 3; break;
                //rest not really used, so use p reg
                case 12: addr=c; reg = 0; break;
                case 13: addr=0; reg = 0; break;
                case 14: addr=ir; reg = 0; break;
                case 15: addr=0; reg = 0;
            }
        }
        String tmp = "";
        trace.deleteAll();
        for(i=0+addr;i<32+addr;i++) {
            tmp = String.valueOf((peek(i,reg)>>8)&255)+String.valueOf
(peek(i,reg)&255);
            trace.append(toHex(i)+" "+toHex(peek(i,reg))+" "+tmp,
null);
        }
        trace.setSelectedIndex(0, true);
    }

    //possible poke values display
    public void fillPoke(List poke) {
        poke.deleteAll();
        for(int i=0;i<16;i++) {
            poke.append(toHex(i), null);
        }
        poke.append("x<<4", null);
        poke.setSelectedIndex(0, true);
    }

    //perform a poke
    public void doPoke(List trace, List poke) {
        int i=poke.getSelectedIndex();
        if(i>0 && i<15) {
            poke(addr+trace.getSelectedIndex(),(short)i,reg);
        } else {
            if(i==16)
                poke(addr+trace.getSelectedIndex(),
                        (short)(peek(addr+trace.getSelectedIndex(),reg)
<<4),reg);
        }
    }

    //run method (not called by you) use go() method
    public void run() {
        running=true;
        while(running) step();
    }

    //thread starter
    public void go() {
        th.start();
    }

    //stop method
    public void stop() {
        running=false;
    }

    //single step
    public void step() {
        //check breakpoint
        if(p==brk) {
            running=false;
        }
        //fetch
        ir=peek(p++,0);
        //execute
        if(ir>15) {
            poke(--r,p,2);
            p=ir;
            //advanced branch
        } else {
            switch(ir) {
                case 0:
                    p=peek(r++,2);
                    break;
                case 1:
                    q=peek(q,1);
                    break;
                case 2:
                    q=peek(r++,2);
                    break;
                case 3:
                    q=peek(s++,3);
                    break;

                case 4:
                    if(cf) p=peek(s++,3);
                    else peek(s++,3);
                    break;
                case 5:
                    q^=peek(s++,3);
                    break;
                case 6:
                    acc=-1;
                    acc&=peek(s++,3);
                    acc<<=1;
                    if(cf) acc+=1;
                    q=(short)(acc&0xFFFF);
                    c=(short)((acc>>16)&0xFFFF);
                    cf=((acc>>32)&1)==1;
                    break;
                case 7:
                    acc=q+peek(s++,3);
                    acc+=(c<<16);
                    if(cf) acc+=1;
                    q=(short)(acc&0xFFFF);
                    c=(short)((acc>>16)&0xFFFF);
                    cf=((acc>>32)&1)==1;
                    break;

                case 8:
                    poke(--r,q,2);
                    break;
                case 9:
                    c=peek(q++,1);
                    break;
                case 10:
                    c=peek(r++,2);
                    break;
                case 11:
                    c=peek(s++,3);
                    break;

                case 12:
                    poke(--s,q,3);
                    break;
                case 13:
                    poke(--q,c,1);
                    break;
                case 14:
                    poke(--r,c,2);
                    break;
                case 15:
                    poke(--s,c,3);
            }
        }
        khz();
    }

    public void setBreak(short b) {
        brk=b;
    }

    void khz() {
        long time=System.currentTimeMillis();
        long dt=time-t;
        cnt++;
        if(dt>1024 || !running) {
            sdt+=dt; // millis for hz calc.
            t=time;
            hz=(int)(cnt/sdt); //kHz!!
        }
    }

    short peek(int a, int reg) { //reg pqrs=0123
        if(reg==0&&a<512) {
            return flash[a];
        } else if(reg==1&&a<8) {
            prog.syncPorts();
            return port[a|1];
        } else {
            return m[a];
        }
    }

    void poke(int a, short d, int reg) {
        if(reg==0&&a<512) {
            flash[a]=d;
        } else if(reg==1&&a<8) {
            port[a]=d;
            prog.syncPorts();
        } else {
            m[a]=d;
        }
    }

    String toHex(int val) {
        if(val<15) {
            return ops[val]+"-"+hex[val];
        } else {
            String tmp = "";
            int i;
            for(i=3;i>-1;i--) {
                tmp+=hex[(val>>(4*i))&15];
            }
            return tmp;
        }
    }
}
