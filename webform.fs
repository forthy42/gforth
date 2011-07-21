\ Webform and CGI handling
\
\ Copyright (C) 2011 Free Software Foundation, Inc.

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

wordlist constant form-fields

: cut-string ( c-addr1 u1 c -- c-addr2 u2 c-addr3 u3 )
    \ cut c-addr1 u1 using separator c; c-addr3 u3 is the part before
    \ the first separator, c-addr2 u2 the rest.
    >r 2dup r> scan over >r dup if
        1 /string \ skip c
    endif
    2swap drop r> over - ;

: hex>u ( c-addr u -- u2 f )
    \ convert hex string c-addr u into u2; f is true if the conversion
    \ worked, otherwise it is false and u2 is anything
    0. 2swap ['] >number $10 base-execute nip nip 0= ;


\ basic CGI handling

: type-cgi ( c-addr u -- )
    begin
        dup while
            over c@ case
                '% of
                    dup 3 >= if
                        over 1+ 2 hex>u if
                            emit 3
                        else
                            drop 1
                        then
                    else
                        1
                    then
                endof
                '+ of
                    space 1 endof
                dup emit 1 swap
            endcase
            /string
    repeat
    2drop ;

\ s" q=bla%26foo%25%23&test=field2%3Dxy&review=line1%0D%0Aline2%0D%0A" type-cgi

: cgi-field ( c-addr u -- )
    \ process a cgi field
    get-current >r form-fields set-current
    '= cut-string 2>r ['] type-cgi >string-execute 2r> nextname 2constant
    r> set-current ;

: cgi-input ( c-addr u -- )
    \ process gci input: split into fields, and make the fields words
    begin
        '& cut-string dup while
            cgi-field
    repeat
    2drop 2drop ;
