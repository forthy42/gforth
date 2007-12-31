// misc simulator environment

// Copyright (C) 1998,2000,2003,2004,2007 Free Software Foundation, Inc.

// This file is part of Gforth.

// Gforth is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation, either version 3
// of the License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, see http://www.gnu.org/licenses/.

 `define L [0:l-1]
 
module main;
   parameter l=16, d=10;
   reg clock;
   wire `L addr, data;
   wire csel, rw, read, write;

   reg `L mem[0:(1<<l)-1];
   reg [0:7] keys[0:15];
   integer inputq, fileno, start;
   
   initial
    begin
       clock = 0;
       for(start='h0000; start < 'h8000; start=start+1)
         mem[start] = 0;
       $readmemh("kernl-misc.hex", mem);
//       fileno=$fopen("misc.out");
       mem['hffff] = 1;
       mem['hfffe] = 32;
       keys[0]=99;
       keys[1]=114;
       keys[2]=32;
       keys[3]=49;
       keys[4]=32;
       keys[5]=50;
       keys[6]=32;
       keys[7]=43;
       keys[8]=32;
       keys[9]=46;
       keys[10]=13;
       keys[11]=32;
       inputq=0;
       #d clock = 0;
       #d clock = 0;
       forever
	begin
	   #d clock = ~clock;
	end
    end

   assign #d
    write = csel & ~rw,
    read  = csel &  rw;
   
   always @(posedge write)
    #d if(addr == 'hfffc) $write("%c", data);
    else
     mem[addr] = data;

   assign
    data = (read & ~write) ? mem[addr] : {l{1'bz}};

   always @(addr or read or write)
    if(read & ~write & (addr == 'hfffe))
     begin
	#(d*4)
	mem['hfffe] = { 8'b0, keys[inputq] };
	inputq = inputq + 1;
	if(inputq > 11) $finish;
     end

   misc #(l,d)
    misc0(clock, data, addr, csel, rw);
   
endmodule /* main */
