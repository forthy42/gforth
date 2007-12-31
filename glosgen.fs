\ Glossary generator.
\ Written in ANS Forth, requires FILES wordset.

\ This file is part of Gforth.

\ Copyright (C) 1995,1997,2000,2003,2007 Free Software Foundation, Inc.
\ Copyright (c)1993 L.C. Benschop Eindhoven.

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

: \G postpone \ ; immediate
\G \G is an alias for \, so it is a comment till end-of-line, but
\G it has a special meaning for the Glossary Generator.

\G \G comments should appear immediately above or below the definition of
\G the word it belongs to. The definition line should contain no more
\G than the definition, a stack comment and a \ comment after which
\G the wordset and pronounciation.
\G An isolated block of \G comments is placed at the beginning of the
\G glossary file.

VARIABLE GLOSLIST 
VARIABLE CURRENT-COMMENT
VARIABLE FIXLINE

\ The Glossary entries in memory have the following form.
\ 1 cell: address of next entry, 1 cell: address of comment field
\ counted string: name counted string: stack picture 
\ counted string: extra field counted string: pronunciation.


\G This command starts a fresh glossary.
: NEWGLOS 
\  S" FORGET GSTART CREATE GSTART" EVALUATE
  0 GLOSLIST ! ;   

CREATE OLDLINE 256 CHARS ALLOT

VARIABLE CHARPTR


\G Insert the header into the list at the alphabetically correct place.
: INSERT-HEADER ( addr ---) 
  CHARPTR !
  GLOSLIST 
  BEGIN
   DUP @ 
   IF
    DUP @ 2 CELLS + COUNT CHARPTR @ 2 CELLS + COUNT COMPARE 0<=
   ELSE
    0
   THEN
  WHILE
   @
  REPEAT
  DUP @ CHARPTR @ ! CHARPTR @ SWAP ! 
;

\G Scan a word on oldline through pointer charptr
: SCAN-WORD ( ---- addr len)
  BEGIN
   CHARPTR @ OLDLINE - OLDLINE C@ <= CHARPTR @ C@ BL = AND
  WHILE
   1 CHARS CHARPTR +! 
  REPEAT
  CHARPTR @ 0
  BEGIN
   CHARPTR @ OLDLINE - OLDLINE C@ <= CHARPTR @ C@ BL <> AND
  WHILE
   1 CHARS CHARPTR +! 1+
  REPEAT
;

: SEARCH-NAME 
  SCAN-WORD 2DROP
  SCAN-WORD 2DUP BOUNDS ?DO 
   I C@ [CHAR] a [CHAR] { WITHIN IF I C@ 32 - I C! THEN
  LOOP \ translate to caps.
  DUP HERE C! HERE CHAR+ SWAP DUP 1+ CHARS ALLOT CMOVE
;

: SEARCH-STACK
  0 C,
  SCAN-WORD S" (" COMPARE 0= IF
   HERE 1 CHARS -
   BEGIN
    CHARPTR @ OLDLINE - OLDLINE C@ <= CHARPTR @ C@ [CHAR] ) <> AND
   WHILE
    CHARPTR @ C@ C, 
    DUP  C@ 1+ OVER C!
    1 CHARS CHARPTR +!
   REPEAT
   DROP
  THEN
;

: SEARCH-SETS
  0 C,
;

: SEARCH-PRON
  0 C,
;

\G Process the header information stored in OLDLINE
: PROCESS-HEADER
  HERE 0 , CURRENT-COMMENT @ ,
  OLDLINE CHARPTR ! 
  SEARCH-NAME
  SEARCH-STACK
  SEARCH-SETS
  SEARCH-PRON 
  INSERT-HEADER 
;

\G Determine if line at HERE is glossary comment, if so.
\G allot it, else store into oldline.
: GLOS-COMMENT? ( --- flag)
   HERE C@ 1 > HERE CHAR+ 2 S" \G" COMPARE 0= AND
   IF
    HERE C@ 1+ CHARS ALLOT 1 \G incorporate current line.
   ELSE
    FIXLINE @ 0=
    IF
     HERE OLDLINE HERE C@ 1+ CHARS CMOVE 
    THEN 0
   THEN
;

\G Read lines from the file fid until \G line encountered.
\G Collect all adjacent \G lines and find header line.
\G then insert entry into list flag=0 if no entry found.
: MAKE-GLOSENTRY ( fid --- fid flag)
  >R
  HERE CURRENT-COMMENT !
  0 FIXLINE ! 0 OLDLINE C!
  BEGIN
   HERE CHAR+ 255 R@ READ-LINE THROW 0= IF
    DROP R> 0 EXIT \ end of file.
   THEN
   HERE C! \ Store length at here.   
   GLOS-COMMENT?   
  UNTIL
  OLDLINE COUNT -TRAILING NIP IF 1 FIXLINE ! THEN
  BEGIN
   HERE CHAR+ 255 R@ READ-LINE THROW
   IF
    HERE C! 
    GLOS-COMMENT?
   ELSE
    DROP 0
   THEN
  0= UNTIL
  R> 1
  0 C, ALIGN  \ allocate end flag after included comment lines.
  PROCESS-HEADER
;  

\G This command reads a source file and builds glossary info
\G for it in memory.
: MAKEGLOS ( "name") 
  BL WORD COUNT R/O OPEN-FILE THROW
  BEGIN
   MAKE-GLOSENTRY
  0= UNTIL
  CLOSE-FILE THROW 
;

\G Build header line for glossary entry.
: BUILD-HLINE ( addr ---)
  79 OLDLINE C! \ Line will be 79 chars long. 
  OLDLINE CHAR+ 79 BL FILL
  2 CELLS + 
  COUNT 2DUP OLDLINE CHAR+ SWAP CMOVE \ place name
  DUP >R \ save name length.
  CHARS + 
  COUNT 2DUP OLDLINE R> 3 + CHARS + SWAP CMOVE \ move stack diagram.
  CHARS +
  COUNT 2DUP OLDLINE 45 CHARS + SWAP CMOVE \ move wordsets field.
  CHARS +
  COUNT OLDLINE 63 CHARS + SWAP CMOVE \ move pronunciation field.
;  
 
\G write the glossary entry at address addr to file fid.
: WRITE-GLOSENTRY ( addr fid --- )
  >R
  DUP 2 CELLS + C@ 
  IF
   DUP BUILD-HLINE
   OLDLINE CHAR+ OLDLINE C@ R@ WRITE-LINE THROW \ write header line.
  THEN
  CELL+ @
  BEGIN
   DUP C@ 1 >
  WHILE \ write all comment lines without prefixing \G.
   DUP 4 CHARS + OVER C@ 3 - 0 MAX R@ WRITE-LINE THROW 
   COUNT CHARS + 
  REPEAT DROP
  HERE 0 R> WRITE-LINE THROW \ Write final empty line.
;


\G This command writes the glossary info from memory to a file.
\G The glossary info may be collected from more source files.
: WRITEGLOS ( "name")
  BL WORD COUNT W/O CREATE-FILE THROW
  GLOSLIST  
  BEGIN
   @ DUP
  WHILE
   2DUP SWAP WRITE-GLOSENTRY
  REPEAT DROP
  CLOSE-FILE THROW
;

\G A typical glossary session may look like:
\G NEWGLOS MAKEGLOS SOURCE1.FS MAKEGLOS SOURCE2.FS WRITEGLOS GLOS.GLO


CREATE GSTART
