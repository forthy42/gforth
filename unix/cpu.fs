\ simple CPU feature check for Linux/Android

\ Copyright (C) 2016 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

e? os-type s" linux" string-prefix? [IF]
    Variable cpuflags
    
    : get-cpuflags ( -- )
	s" /proc/cpuinfo" r/o open-file throw
	[: BEGIN  refill  WHILE
		  ':' parse BEGIN dup WHILE 1- 2dup + c@ bl > UNTIL THEN
		  [ machine s" arm" string-prefix? ] [IF] s" Features" [ELSE]
		      [ machine s" amd64" str= machine s" 386" str= or ] [IF] s" flags" [ELSE]
			  [ machine s" mips" str= ] [IF] s" isa" [ELSE]
			      [ machine s" mipsel" str= ] [IF] s" cpu model" [ELSE]
				  s" cpu" [THEN] [THEN] [THEN] [THEN]
		  str= IF
		      source >in @ /string cpuflags $!  THEN
	  REPEAT ;] execute-parsing-file ;
    
    : string-cpu? ( addr u -- flag )
	\G check if the CPU has the feature @var{addr u}
	false -rot cpuflags bl
	[: 2over str= IF rot drop true -rot THEN ;] $iter 2drop ;
    
    : cpu? ( "name" -- flag )
	\G check if the CPU has the feature @var{name}
	parse-name string-cpu? ;
    
    get-cpuflags
    
    :noname defers 'cold cpuflags off get-cpuflags ; is 'cold
    
    \ fastlib check
    
    machine s" arm" str= [IF]
	: fast-lib s" neon" string-cpu? ;
    [ELSE]
	machine s" 386" str= [IF]
	    : fast-lib s" ssse3" string-cpu? ;
	[ELSE]
	    false Constant fast-lib
	[THEN]
    [THEN]
[ELSE]
    : string-cpu? ( addr u -- flag )
	\G check if the CPU has the feature @var{addr u}
	parse-name 2drop false ;
    : cpu? ( "name" -- flag )
	\G check if the CPU has the feature @var{name}
	parse-name string-cpu? ;
    : get-cpuflags ( -- ) ;
    
    false Constant fast-lib
[THEN]
