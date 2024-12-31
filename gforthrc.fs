\ load a ~/.gforthrc on startup

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2015,2016,2018,2019,2022,2023,2024 Free Software Foundation, Inc.

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

Variable load-rc?
Variable load-rc0? load-rc0? on

get-current also options definitions
: --no-rc ( -- )  load-rc? off ;
previous set-current

:noname ( -- ) defers image-options
    ."   --no-rc			    don't load ~/.config/gforthrc" cr
; is image-options

: load-rc ( -- ) \ gforth-internal
    \G if available, load @file{~/.config/gforthrc} after processing args
    \G disable by setting @var{load-rc?} to 0 (see option @code{--no-rc}).
    load-rc? @ IF
	s" ~/.config/gforthrc" open-fpath-file
	0= IF  included1  ELSE  drop  THEN
    THEN ;
: load-rc0 ( -- ) \ gforth-internal
    \G if available, load @file{~/.config/gforthrc0} or whatever is in the
    \G environment variable @code{GFORTH_ENV} before processing args.
    \G disable loading by setting @code{GFORTH_ENV} to @file{off}.
    s" GFORTH_ENV" getenv 2dup d0= IF  2drop s" ~/.config/gforthrc0"  THEN
    2dup s" off" str= IF  2drop EXIT  THEN
    open-fpath-file 0= IF  included1  ELSE  drop  THEN ;

:noname  load-rc  defers bootmessage  ; is bootmessage

:noname
    \ one-shot recognizer that loads gforthrc0 and removes itself from
    \ the stack without moving that stack around.  By loading load-rc0
    \ and then failing, the rest of the process-option recognizer stack
    \ is tried as well.
    load-rc0? @ IF  [: 2drop load-rc0
	  cell negate action-of process-option @ +!
	  false ;] action-of process-option >stack  THEN
    load-rc? on
    defers 'image ; is 'image
