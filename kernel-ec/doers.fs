\ run-time code for defining words

\ Copyright (C) 1995,1996,1997,1998,1999,2000,2003,2007 Free Software Foundation, Inc.

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


\ If we leave out the compiler we need the runtime code for our defining
\ words. This file defines the defining words without the
\ interpretative/compiling part.

has? compiler 0= [IF]

\ fillers for interpreter only mode
\D compileddofillers .( Do-Fillers: )

: (does>) ;    

doer? :dofield 0= [IF] \D compileddofillers .( DOFIELD )
| : (Field)  DOES> @ + ;
[THEN]

doer? :dodefer 0= [IF] \D compileddofillers .( DODEFER )
| : Defer ( "name" -- ) DOES> @ execute ;
[THEN]

\D compileddofillers .( DO2CON )
| : 2Constant ( w1 w2 "name" -- ) 
    DOES> ( -- w1 w2 )
        2@ ;

doer? :docon 0= [IF] \D compileddofillers .( DOCON )
| : (Constant)  DOES> @ ;
[THEN]

doer? :douser 0= [IF] \D compileddofillers .( DOUSER )
| : User DOES> @ [IFDEF] up@ up@ [ELSE] up @ [THEN] + ;
[THEN]

doer? :dovar 0= [IF] \D compileddofillers .( DOVAR )
| : Create ( "name" -- ) \ core
    DOES> ;

\D compileddofillers .( .)
[THEN]
