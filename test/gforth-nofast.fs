\ test stuff that is not guaranteed in gforth-fast, but elsewhere

\ Author: Anton Ertl
\ Copyright (C) 2006,2007,2019 Free Software Foundation, Inc.

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

require ./tester.fs
decimal

\ division by zero
{ 1 0 ' /    catch 0= -> 1 0 false }
{ 1 0 ' mod  catch 0= -> 1 0 false }
{ 1 0 ' /mod catch 0= -> 1 0 false }
{ 1 1 0 ' */mod catch 0= -> 1 1 0 false }
{ 1 1 0 ' */    catch 0= -> 1 1 0 false }

{ 1 0 ' /s    catch 0= -> 1 0 false }
{ 1 0 ' mods  catch 0= -> 1 0 false }
{ 1 0 ' /mods catch 0= -> 1 0 false }
{ 1 1 0 ' */mods catch 0= -> 1 1 0 false }
{ 1 1 0 ' */s    catch 0= -> 1 1 0 false }

{ 1 0 ' /s    catch 0= -> 1 0 false }
{ 1 0 ' mods  catch 0= -> 1 0 false }
{ 1 0 ' /mods catch 0= -> 1 0 false }
{ 1 1 0 ' */mods catch 0= -> 1 1 0 false }
{ 1 1 0 ' */s    catch 0= -> 1 1 0 false }

{ 1 0 ' u/    catch 0= -> 1 0 false }
{ 1 0 ' umod  catch 0= -> 1 0 false }
{ 1 0 ' u/mod catch 0= -> 1 0 false }
{ 1 1 0 ' u*/mod catch 0= -> 1 1 0 false }
{ 1 1 0 ' u*/    catch 0= -> 1 1 0 false }

{ 1. 0 ' fm/mod catch 0= -> 1. 0 false }
{ 1. 0 ' sm/rem catch 0= -> 1. 0 false }
{ 1. 0 ' um/mod catch 0= -> 1. 0 false }

\ division overflow
environment-wordlist >order
{ max-n invert -1 ' /    catch 0= -> max-n invert -1 false }
{ max-n invert -1 ' mod  catch 0= -> max-n invert -1 false }
{ max-n invert -1 ' /mod catch 0= -> max-n invert -1 false }
{ 1 max-n invert -1 ' */     catch 0= -> 1 max-n invert -1 false }
{ 1 max-n invert -1 ' */mod  catch 0= -> 1 max-n invert -1 false }

{ max-n invert -1 ' /s    catch 0= -> max-n invert -1 false }
{ max-n invert -1 ' mods  catch 0= -> max-n invert -1 false }
{ max-n invert -1 ' /mods catch 0= -> max-n invert -1 false }
{ 1 max-n invert -1 ' */s     catch 0= -> 1 max-n invert -1 false }
{ 1 max-n invert -1 ' */mods  catch 0= -> 1 max-n invert -1 false }

{ max-n invert -1 ' /f    catch 0= -> max-n invert -1 false }
{ max-n invert -1 ' modf  catch 0= -> max-n invert -1 false }
{ max-n invert -1 ' /modf catch 0= -> max-n invert -1 false }
{ 1 max-n invert -1 ' */f     catch 0= -> 1 max-n invert -1 false }
{ 1 max-n invert -1 ' */modf  catch 0= -> 1 max-n invert -1 false }

{ max-n invert s>d -1 ' fm/mod catch 0= -> max-n invert s>d -1 false }
{ max-n invert s>d -1 ' sm/rem catch 0= -> max-n invert s>d -1 false }

{ 2 max-n 2/ 1+ 1 ' */    catch 0= -> 2 max-n 2/ 1+ 1 false }
{ 2 max-n 2/ 1+ 1 ' */mod catch 0= -> 2 max-n 2/ 1+ 1 false }
{ 2 max-n 2/ 1+ 1 ' */s    catch 0= -> 2 max-n 2/ 1+ 1 false }
{ 2 max-n 2/ 1+ 1 ' */mods catch 0= -> 2 max-n 2/ 1+ 1 false }
{ 2 max-n 2/ 1+ 1 ' */f    catch 0= -> 2 max-n 2/ 1+ 1 false }
{ 2 max-n 2/ 1+ 1 ' */modf catch 0= -> 2 max-n 2/ 1+ 1 false }
{ 2 max-u 1 rshift 1+ 1 ' u*/    catch 0= -> 2 max-u 1 rshift 1+ 1 false }
{ 2 max-u 1 rshift 1+ 1 ' u*/mod catch 0= -> 2 max-u 1 rshift 1+ 1 false }
{ max-n 0 1. d+ 1 ' fm/mod catch 0= -> max-n 0 1. d+ 1 false }
{ max-n 0 1. d+ 1 ' sm/rem catch 0= -> max-n 0 1. d+ 1 false }
{ max-u 0 1. d+ 1 ' um/mod catch 0= -> max-u 0 1. d+ 1 false }

{ 1 1 dnegate 2 ' fm/mod catch 0= -> max-u 0 2. d+ dnegate 2 false }
{ 1 1 dnegate 2 ' sm/rem catch 0= -> -1 max-n invert true }

{ 1 1 -2 ' fm/mod catch 0= -> 1 1 -2 false }
{ 1 1 -2 ' sm/rem catch 0= -> 1 max-n invert true }

{ max-u max-n 2/ max-n invert ' fm/mod catch -> -1 max-n invert 0 }
{ max-u max-n 2/ max-n invert ' sm/rem catch -> max-n max-n negate 0 }

{ 0 max-n 2/ 1+ max-n invert ' fm/mod catch -> 0 max-n invert 0 }
{ 0 max-n 2/ 1+ max-n invert ' sm/rem catch -> 0 max-n invert 0 }

{ 1 max-n 2/ 1+ max-n invert ' fm/mod catch 0= -> 1 max-n 2/ 1+ max-n invert false }
{ 1 max-n 2/ 1+ max-n invert ' sm/rem catch 0= -> 1 max-n invert true }

{ 0 max-u -1. d+ max-u ' um/mod catch 0= -> max-u 1- max-u true }
{ 0 max-u max-u ' um/mod catch 0= -> 0 max-u max-u false }


\ underflow of various stacks; if one of these tests fails, probably
\ the C compiler has not compiled ALIVE_DEBUGGING as intended.

\ in some cases we THROW at the end to restore the stack depths.

{ :noname rp@ 1000 begin rdrop 1- dup 0= until drop rp! ;          catch dup    -6 = swap -9 = or -> true }
{ :noname rp@ 1000 begin 2rdrop 1- dup 0= until drop rp! ;         catch dup    -6 = swap -9 = or -> true }
\ in Cygwin, these drops will cause silent exit of the system
s" os-type" environment? [IF] s" cygwin" str= [IF] \\\ [THEN] [THEN]
{ :noname drop drop drop fdrop fdrop fdrop ;                       catch dup    -4 = swap -9 = or -> true }
{ :noname 2drop 2drop 2drop fdrop fdrop fdrop ;                    catch dup    -4 = swap -9 = or -> true }
{ :noname fdrop fdrop fdrop 1 throw ;                              catch dup   -45 = swap -9 = or -> true }
{ :noname 1000 begin lp+!# [ 16 , ] 1- dup 0= until drop 1 throw ; catch dup -2059 = swap -9 = or -> true }
{ :noname 1000 begin lp+            1- dup 0= until drop 1 throw ; catch dup -2059 = swap -9 = or -> true }
{ :noname 1000 begin lp+2           1- dup 0= until drop 1 throw ; catch dup -2059 = swap -9 = or -> true }
