\ add links to NEWS-nolinks.html

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke
\ Copyright (C) 1995,1996,1997,2000,2003,2007,2011,2012,2013,2014,2015,2017,2019,2021,2023,2024,2025 Free Software Foundation, Inc.

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

\ Look checks first if the word is a primitive. If yes then the
\ vocabulary in the primitive area is beeing searched, meaning
\ creating for each word a xt and comparing it...

\ If a word is no primitive look searches backwards to find the nfa.
\ Problems: A compiled xt via compile, might be created with noname:
\           a noname: leaves now a empty name field

"doc/gforth_html/" 2constant url-prefix

"doc/gforth_html/Word-Index.html" slurp-file 2constant linkdata

wordlist constant links

: parse-linkdata ( -- )
    case
        "<td class=\"printindex-index-entry\"><a href=\""
        string-parse nip 0= ?of endof
        "\"><code>" string-parse dup 0= ?of 2drop endof
        parse-name dup 0= ?of 2drop 2drop endof
        [: links set-current nextname 2constant ;] current-execute
    next-case ;

linkdata `parse-linkdata execute-parsing

"NEWS-nolinks.html" slurp-file 2constant news

: parse-code ( -- )
    source drop >r begin ( R: c-addr )
        parse-name r@ third r> - type 2dup + >r
        dup while
            2dup links find-name-in dup if
                .\" <a href=\"" url-prefix type
                name>interpret execute type .\" \">"
                type ." </a>"
            else
                drop type
            then
    repeat
    rdrop 2drop ;

: parse-news ( -- )
    case
        "<code>" string-parse dup 0= ?of 2drop endof
        type "<code>" type
        "</code>" string-parse dup 0= ?of 2drop endof
        `parse-code execute-parsing "</code>" type
    next-case ;
    
news `parse-news execute-parsing