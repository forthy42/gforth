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

/* Minimal Instruction Set Computer
 \ sources                 destinations

$0 Constant PC		$0 Constant JMP
$1 Constant PC+2	$1 Constant JS
$2 Constant PC+4	$2 Constant JZ
$3 Constant PC+6	
			$4 Constant JC

 
 
$8 Constant ACCU	$8 Constant ACCU
$9 Constant SF		$9 Constant SUB
$A Constant ZF		$A Constant SUBR
			$B Constant ADD
$C Constant CF		$C Constant XOR
			$D Constant OR
			$E Constant AND
			$F Constant SHR
*/

`define L [0:l-1]
 
module misc(clock, data, addr, csel, rw);
   parameter l=16, d=10;
   input clock;
   inout `L data;
   output `L addr;
   output csel;
   output rw;

   reg `L inst, dtr, accu, pc;
   reg [0:1] state;
   reg carry, zero;
   wire `L regs;
   wire alusel, cout, zout, pccond;
   wire `L alu1, alu2, aluout;
   wire [0:2] aluop;
   
   initial
    begin
       state = 0;
       pc = 'h10;
       inst = 0;
       dtr = 0;
       accu = 0;
       carry = 0;
       zero = 1;
    end
   
   assign
    rw=~&state,
    csel=~state[1] | |inst[0:l-5],
    addr = state[1] ? inst : pc,
    data = rw ? {l{1'bz}} : dtr;

   assign
    alusel= inst[l-4],
    pccond = ~|(inst[l-3:l-1] & ~{ carry, zero, accu[0] }),
    regs = inst[l-4] ? (|inst[l-3:l-1] ? { {(l-1){1'b0}}, pccond } : accu)
                     : aluout;
   
   always @(posedge clock)
    begin
       casez(state)
	 2'bz0 : begin
	    inst = data;
	    pc = aluout;
	 end
	 2'b01 : begin
	    dtr = csel ? data : regs;
//	    $fwrite(2, "PC: %x : %x -( %x )->", pc-1'b1, inst, dtr);
	 end
	 2'b11 :
	  begin
	     if(~|inst[0:l-5])
	      if(alusel) { carry, zero, accu } = { cout, zout, aluout };
	      else
	       if (pccond) pc=dtr;
//	     $fwrite(2, " %x ACCU: %x\n", inst, accu);
	  end
       endcase /* 2 */
       state = state + 1;
    end

   assign
    alu1 = &state ? accu : pc,
    alu2 = ~state[1] ? {{l{1'b0}}, 1'b1 } :
                       state[0] ? dtr : { inst[1:l-1], 1'b0 } - 1,
    aluop = &state ? inst[l-3:l-1] : 3'b011;
   
   alu #(l,d) alu0(aluop, alu1, alu2, carry, aluout, cout, zout);
   
endmodule /* misc */

module alu(op, in1, in2, cin, out, cout, zout);
   parameter l=16, d=10;
   input [0:2] op;
   input `L in1, in2;
   input cin;
   output `L out;
   output cout, zout;
   
   reg `L out;
   reg cout;

   initial
    cout = 0;
   
   always @(in1 or in2 or op)
    #d case(op)
      3'b000 : { cout, out } = { cin, in2 };
      3'b001 : { cout, out } = in1 - in2;
      3'b010 : { cout, out } = in2 - in1;
      3'b011 : { cout, out } = in1 + in2;
      3'b100 : { cout, out } = { cin, in1 ^ in2 };
      3'b101 : { cout, out } = { cin, in1 | in2 };
      3'b110 : { cout, out } = { cin, in1 & in2 };
      3'b111 : { cout, out } = { in2[l-1], cin, in2[0:l-2] };
    endcase /* 3 */

   assign
    zout = ~|out;
   
endmodule /* alu */
