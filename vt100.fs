\ VT100.STR     VT100 excape sequences                  20may93jaw

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook
\ Copyright (C) 1995,1999,2000,2003,2007,2012,2013,2014,2016,2018,2019,2023,2025 Free Software Foundation, Inc.

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

decimal

[IFUNDEF] #esc  27 Constant #esc  [THEN]

: #n ( n -- )  [: 0 #s 2drop ;] #10 base-execute ;
: #n; ( n -- )  #n ';' hold ;
\ : #esc[ ( -- ) '[' hold #esc hold ;
: #esc[ ( -- )
    s\" \e[" holds ;

: vt100-at-xy ( u1 u2 -- )
  1+ swap 1+ <<# 'H' hold #n; #n #esc[ #0. #> type #>> ;

[IFUNDEF] at-deltaxy  Defer at-deltaxy [THEN]
: vt100-at-deltaxy ( x y -- )
    \ over 0< over 0= and IF  drop abs backspaces  EXIT  THEN
    <<#
    ?dup-IF
	dup 0< 'A' 'B' rot select  hold abs #n #esc[
    THEN
    ?dup-IF
	dup 0< 'D' 'C' rot select  hold abs #n #esc[
    THEN #0. #> type #>> ;

: vt100-page ( -- )
  <<# s" [2J" holds #esc hold #0. #> type #>> 0 0 at-xy ;

' vt100-at-xy IS at-xy
' vt100-at-deltaxy IS at-deltaxy
' vt100-page IS page

[IFDEF] debug-out
    debug-out op-vector !
    
    ' vt100-at-xy IS at-xy
    ' vt100-at-deltaxy IS at-deltaxy
    ' vt100-page IS page
    
    default-out op-vector !
[THEN]
