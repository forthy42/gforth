\ enhance TAGS file with install directory

\ Copyright (C) 2008 Free Software Foundation, Inc.

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


\ usage: gforth -e 's" dir"' install-tags.fs -e bye <TAGS >install/TAGS

\ We used to do this with

\ awk 'BEGIN {RS="\014\n"; ORS=RS} {if (NR==1) print $0; else print "$(datadir)/gforth/$(VERSION)/"$$0;}'

\ but the awk of HP/UX B.11.23 was not up to the task

2constant dir
s\" \f\l" 2constant separator

: install-tags ( c-addr u -- )
    begin { c-addr u }
        c-addr u separator search while
            separator nip /string { c-addr2 u2 }
            c-addr c-addr2 over - type
            dir type
            c-addr2 u2 repeat
    type ;

infile-id slurp-fid install-tags


            
            
            
            