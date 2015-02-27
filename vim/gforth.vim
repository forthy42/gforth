" Vim syntax file
" Language:    Gforth
" Author:      Marcos Cruz (programandala.net)
" License:     Vim license (GPL compatible)
" URL:         http://programandala.net/en.program.gforth_vim_syntax_file.html
" Filenames:   *.fs
" Updated:     2015-02-26

" ----------------------------------------------
" Based on:

" Language:    FORTH
" Maintainer:  Christian V. J. Brüssow
" Last Change: So 27 Mai 2012 15:56:28 CEST
" Filenames:   *.fs,*.ft
" URL:         http://www.cvjb.de/wp-content/uploads/vim/forth.vim

" --------------------------------------------------------------
" Usage

" 1) Link this syntax file to its proper location

" If you are the only user that needs this Vim syntax file, copy, move
" or link it into <~/.vim/syntax/>, e.g.:

"     ln -s gforth.vim ~/.vim/syntax/

" If you want every user to use this Vim syntax file, link it into
" </usr/share/vim/vimcurrent/syntax/>, e.g.:

"     sudo ln -s gforth.vim /usr/share/vim/vimcurrent/syntax/

" 2) Associate the filename extension

" If you use the 'fs' filename extension for Gforth programs, Vim will use its
" own Forth syntax file and you will have to change it manually with the
" following command after the Gforth source file has been loaded:

"     :set filetype=gforth

" In order to make Vim to associate the 'fs' filename extension to the
" Gforth filetype, all you need is to add the following line at the end of
" the file <~/.vim/filetype.vim> (if it doesn't exist, just create it):

"     autocmd BufNewFile,BufRead *.fs setfiletype gforth

" 3) Set the filetype configuration

" Create a <~/.vim/ftplugin/gforth.vim> file with your prefered configuration
" for Gforth sources. I copy here the content of my own file, as an example:

" ..............................................................

"        " ~/.vim/ftplugin/gforth.vim
"        " Vim filetype plugin for Gforth
"        setlocal comments=b:\\
"        setlocal textwidth=70
"        setlocal formatoptions=cqorj
"        setlocal tabstop=2
"        setlocal softtabstop=0
"        setlocal shiftwidth=2
"        setlocal expandtab
"        setlocal ignorecase
"        setlocal smartcase
"        setlocal smartindent
"        " Used by the Vim-Commentary plugin:
"        setlocal commentstring=\\\ %s

" ..............................................................


" ----------------------------------------------
" To-do

" Fix: Words like bla" get the quote and the previous char highlighted (and
" the folowing text until a closing quote); the whole word should be
" highligted.

" Fix: Words like (bla") get the quote and the last char highlighted (and the
" following text until a closing quote); the word and the string should not be
" highligted.

" ----------------------------------------------
" History

" See at the end of this file.

" ----------------------------------------------

" For version 5.x: Clear all syntax items
" For version 6.x: Quit when a syntax file was already loaded
if version < 600
    syntax clear
elseif exists("b:current_syntax")
    finish
endif

let s:cpo_save = &cpo
set cpo&vim

" Synchronization method
syn sync ccomment
syn sync maxlines=200

syn case ignore

" Characters allowed in keywords

if version >= 600
    setlocal iskeyword=33-255
else
    set iskeyword=33-255
endif

" when wanted, highlight trailing white space
if exists("gforth_space_errors")
    if !exists("gforth_no_trail_space_error")
        syn match gforthSpaceError display excludenl "\s\+$"
    endif
    if !exists("gforth_no_tab_space_error")
        syn match gforthSpaceError display " \+\t"me=e-1
    endif
endif

" ----------------------------------------------
" Keywords

" Some special, non-FORTH keywords

syn keyword gforthTodo contained TODO
syn keyword gforthTodo contained FIXME
syn keyword gforthTodo contained XXX
syn match gforthTodo contained 'Copyright\(\s([Cc])\)\=\(\s[0-9]\{2,4}\)\='

" Define

syn keyword gforthEndOfColonDef ;
syn keyword gforthDefine constant
syn keyword gforthDefine 2constant
syn keyword gforthDefine fconstant
syn keyword gforthDefine variable
syn keyword gforthDefine 2variable
syn keyword gforthDefine fvariable
syn keyword gforthDefine create
syn keyword gforthDefine marker
syn keyword gforthDefine nextname
syn keyword gforthDefine user
syn keyword gforthDefine value
syn keyword gforthDefine alias
syn keyword gforthDefine does>
syn keyword gforthDefine immediate
syn keyword gforthDefine compile-only
syn keyword gforthDefine cvariable
syn keyword gforthDefine ,
syn keyword gforthDefine 2,
syn keyword gforthDefine f,
syn keyword gforthDefine c,
syn keyword gforthDefine s,
syn keyword gforthDefine literal
syn keyword gforthDefine 2literal
syn keyword gforthDefine sliteral
syn keyword gforthDefine restrict
syn keyword gforthDefine postpone

" '[' must be defined before any other word using the bracket:
syn match gforthDefine "\<\[\>"
syn keyword gforthDefine ]
" XXX FIXME why this does not work? ([[ is highlighted even as part of other
" words):
syn match gforthDefine "\<\[\[\>"
syn keyword gforthDefine ]]

syn keyword gforthDefine :noname
syn keyword gforthDefine noname

" Structures

syn keyword gforthStructure %align
syn keyword gforthStructure %alignment
syn keyword gforthStructure %alloc
syn keyword gforthStructure %allocate
syn keyword gforthStructure %allot
syn keyword gforthStructure %size
syn keyword gforthStructure +field
syn keyword gforthStructure cell%
syn keyword gforthStructure char%
syn keyword gforthStructure dfloat%
syn keyword gforthStructure double%
syn keyword gforthStructure end-struct
syn keyword gforthStructure float%
syn keyword gforthStructure naligned
syn keyword gforthStructure sfloat%
syn keyword gforthStructure struct

" Operators

syn keyword gforthOperator *
syn keyword gforthOperator */
syn keyword gforthOperator */mod
syn keyword gforthOperator +
syn keyword gforthOperator -
syn keyword gforthOperator /
syn keyword gforthOperator /mod
syn keyword gforthOperator 0<
syn keyword gforthOperator 0<=
syn keyword gforthOperator 0<>
syn keyword gforthOperator 0=
syn keyword gforthOperator 0>
syn keyword gforthOperator 0>=
syn keyword gforthOperator 1+
syn keyword gforthOperator 1-
syn keyword gforthOperator 1/f
syn keyword gforthOperator 2*
syn keyword gforthOperator 2/
syn keyword gforthOperator <
syn keyword gforthOperator <=
syn keyword gforthOperator <>
syn keyword gforthOperator =
syn keyword gforthOperator >
syn keyword gforthOperator >=
syn keyword gforthOperator ?dnegate
syn keyword gforthOperator ?negate
syn keyword gforthOperator abs
syn keyword gforthOperator and
syn keyword gforthOperator d+
syn keyword gforthOperator d-
syn keyword gforthOperator d0<
syn keyword gforthOperator d0<=
syn keyword gforthOperator d0<>
syn keyword gforthOperator d0=
syn keyword gforthOperator d0>
syn keyword gforthOperator d0>=
syn keyword gforthOperator d2*
syn keyword gforthOperator d2/
syn keyword gforthOperator d<
syn keyword gforthOperator d<=
syn keyword gforthOperator d<>
syn keyword gforthOperator d=
syn keyword gforthOperator d>
syn keyword gforthOperator d>=
syn keyword gforthOperator dabs
syn keyword gforthOperator dmax
syn keyword gforthOperator dmin
syn keyword gforthOperator dnegate
syn keyword gforthOperator du<
syn keyword gforthOperator du<=
syn keyword gforthOperator du>
syn keyword gforthOperator du>=
syn keyword gforthOperator f*
syn keyword gforthOperator f**
syn keyword gforthOperator f+
syn keyword gforthOperator f-
syn keyword gforthOperator f/
syn keyword gforthOperator f2*
syn keyword gforthOperator f2/
syn keyword gforthOperator fabs
syn keyword gforthOperator facos
syn keyword gforthOperator facosh
syn keyword gforthOperator falog
syn keyword gforthOperator fasin
syn keyword gforthOperator fasinh
syn keyword gforthOperator fatan
syn keyword gforthOperator fatan2
syn keyword gforthOperator fatanh
syn keyword gforthOperator fcos
syn keyword gforthOperator fcosh
syn keyword gforthOperator fexp
syn keyword gforthOperator fexpm1
syn keyword gforthOperator fln
syn keyword gforthOperator flnp1
syn keyword gforthOperator flog
syn keyword gforthOperator floor
syn keyword gforthOperator fm/mod
syn keyword gforthOperator fmax
syn keyword gforthOperator fmin
syn keyword gforthOperator fnegate
syn keyword gforthOperator fround
syn keyword gforthOperator fsin
syn keyword gforthOperator fsincos
syn keyword gforthOperator fsinh
syn keyword gforthOperator fsqrt
syn keyword gforthOperator ftan
syn keyword gforthOperator ftanh
syn keyword gforthOperator f~
syn keyword gforthOperator f~abs
syn keyword gforthOperator f~rel
syn keyword gforthOperator invert
syn keyword gforthOperator lshift
syn keyword gforthOperator m*
syn keyword gforthOperator m*/
syn keyword gforthOperator m+
syn keyword gforthOperator max
syn keyword gforthOperator min
syn keyword gforthOperator mod
syn keyword gforthOperator negate
syn keyword gforthOperator not
syn keyword gforthOperator or
syn keyword gforthOperator rshift
syn keyword gforthOperator sm/rem
syn keyword gforthOperator u<
syn keyword gforthOperator u<=
syn keyword gforthOperator u>
syn keyword gforthOperator u>=
syn keyword gforthOperator um*
syn keyword gforthOperator um/mod
syn keyword gforthOperator under+
syn keyword gforthOperator within
syn keyword gforthOperator xor

" Stack manipulations

syn keyword gforthFloatStack fdrop
syn keyword gforthFloatStack fdup
syn keyword gforthFloatStack fnip
syn keyword gforthFloatStack fover
syn keyword gforthFloatStack frot
syn keyword gforthFloatStack fswap
syn keyword gforthFloatStack ftuck
syn keyword gforthReturnStack 2>r
syn keyword gforthReturnStack 2r>
syn keyword gforthReturnStack 2r@
syn keyword gforthReturnStack 2rdrop
syn keyword gforthReturnStack >r
syn keyword gforthReturnStack r>
syn keyword gforthReturnStack r@
syn keyword gforthReturnStack rdrop
syn keyword gforthStack -rot
syn keyword gforthStack 2drop
syn keyword gforthStack 2dup
syn keyword gforthStack 2nip
syn keyword gforthStack 2over
syn keyword gforthStack 2rot
syn keyword gforthStack 2swap
syn keyword gforthStack 2tuck
syn keyword gforthStack ?dup
syn keyword gforthStack drop
syn keyword gforthStack dup
syn keyword gforthStack nip
syn keyword gforthStack over
syn keyword gforthStack pick
syn keyword gforthStack roll
syn keyword gforthStack rot
syn keyword gforthStack swap
syn keyword gforthStack tuck

" Stack pointers

syn keyword gforthStackPointers !csp
syn keyword gforthStackPointers ?csp
syn keyword gforthStackPointers fp!
syn keyword gforthStackPointers fp@
syn keyword gforthStackPointers lp!
syn keyword gforthStackPointers lp@
syn keyword gforthStackPointers rp!
syn keyword gforthStackPointers rp@
syn keyword gforthStackPointers sp!
syn keyword gforthStackPointers sp@

" Address operations

syn keyword gforthMemory !
syn keyword gforthMemory +!
syn keyword gforthMemory 2!
syn keyword gforthMemory 2@
syn keyword gforthMemory ?
syn keyword gforthMemory @
syn keyword gforthMemory c!
syn keyword gforthMemory c@
syn keyword gforthMemory df!
syn keyword gforthMemory df@
syn keyword gforthMemory f!
syn keyword gforthMemory f@
syn keyword gforthMemory sf!
syn keyword gforthMemory sf@
syn keyword gforthMemory to

syn keyword gforthAdrArith address-unit-bits
syn keyword gforthAdrArith align
syn keyword gforthAdrArith aligned
syn keyword gforthAdrArith allocate
syn keyword gforthAdrArith allot
syn keyword gforthAdrArith cell
syn keyword gforthAdrArith cell+
syn keyword gforthAdrArith cells
syn keyword gforthAdrArith cfalign
syn keyword gforthAdrArith cfaligned
syn keyword gforthAdrArith char+
syn keyword gforthAdrArith char-
syn keyword gforthAdrArith chars
syn keyword gforthAdrArith dfalign
syn keyword gforthAdrArith dfaligned
syn keyword gforthAdrArith dfloat+
syn keyword gforthAdrArith dfloats
syn keyword gforthAdrArith falign
syn keyword gforthAdrArith faligned
syn keyword gforthAdrArith float
syn keyword gforthAdrArith float+
syn keyword gforthAdrArith floats
syn keyword gforthAdrArith free
syn keyword gforthAdrArith here
syn keyword gforthAdrArith maxalign
syn keyword gforthAdrArith maxaligned
syn keyword gforthAdrArith resize
syn keyword gforthAdrArith sfalign
syn keyword gforthAdrArith sfaligned
syn keyword gforthAdrArith sfloat+
syn keyword gforthAdrArith sfloats

" Memory blocks
" xxx completed after the Gforth manual, plus 'append'

syn keyword gforthMemBlks +place
syn keyword gforthMemBlks -trailing
syn keyword gforthMemBlks /string
syn keyword gforthMemBlks append
syn keyword gforthMemBlks blank
syn keyword gforthMemBlks bounds
syn keyword gforthMemBlks cmove
syn keyword gforthMemBlks cmove>
syn keyword gforthMemBlks compare
syn keyword gforthMemBlks erase
syn keyword gforthMemBlks fill
syn keyword gforthMemBlks move
syn keyword gforthMemBlks place
syn keyword gforthMemBlks s+
syn keyword gforthMemBlks save-mem
syn keyword gforthMemBlks search
syn keyword gforthMemBlks skip
syn keyword gforthMemBlks str<
syn keyword gforthMemBlks str=
syn keyword gforthMemBlks string-prefix?

" Conditionals

syn keyword gforthCond ?dup-0=-if
syn keyword gforthCond ?dup-if
syn keyword gforthCond ahead
syn keyword gforthCond case
syn keyword gforthCond catch
syn keyword gforthCond cs-pick
syn keyword gforthCond cs-roll
syn keyword gforthCond else
syn keyword gforthCond endcase
syn keyword gforthCond endif
syn keyword gforthCond endof
syn keyword gforthCond endtry
syn keyword gforthCond endtry-iferror
syn keyword gforthCond exception
syn keyword gforthCond if
syn keyword gforthCond iferror
syn keyword gforthCond of
syn keyword gforthCond recover
syn keyword gforthCond restore
syn keyword gforthCond then
syn keyword gforthCond throw
syn keyword gforthCond try

syn match gforthCond "\<\[defined]\>"
syn match gforthCond "\<\[else]\>"
syn match gforthCond "\<\[endif]\>"
syn match gforthCond "\<\[if]\>"
syn match gforthCond "\<\[ifdef]\>"
syn match gforthCond "\<\[ifundef]\>"
syn match gforthCond "\<\[then]\>"
syn match gforthCond "\<\[undefined]\>"

" Loops

syn keyword gforthLoop +do
syn keyword gforthLoop +loop
syn keyword gforthLoop -do
syn keyword gforthLoop -loop
syn keyword gforthLoop ?do
syn keyword gforthLoop ?exit
syn keyword gforthLoop ?leave
syn keyword gforthLoop again
syn keyword gforthLoop begin
syn keyword gforthLoop do
syn keyword gforthLoop done
syn keyword gforthLoop for
syn keyword gforthLoop i
syn keyword gforthLoop j
syn keyword gforthLoop k
syn keyword gforthLoop leave
syn keyword gforthLoop loop
syn keyword gforthLoop next
syn keyword gforthLoop repeat
syn keyword gforthLoop u+do
syn keyword gforthLoop u-do
syn keyword gforthLoop unloop
syn keyword gforthLoop until
syn keyword gforthLoop while

syn match gforthLoop "\<\[+loop]\>"
syn match gforthLoop "\<\[?do]\>"
syn match gforthLoop "\<\[again]\>"
syn match gforthLoop "\<\[begin]\>"
syn match gforthLoop "\<\[do]\>"
syn match gforthLoop "\<\[loop]\>"
syn match gforthLoop "\<\[next]\>"
syn match gforthLoop "\<\[repeat]\>"
syn match gforthLoop "\<\[until]\>"

" OOP

syn match gforthClassDef '\<:class\s\+[^ \t]\+\>'
syn match gforthObjectDef '\<:object\s\+[^ \t]\+\>'
syn match gforthColonDef '\<:m\?\s\+[^ \t]\+\>'
syn keyword gforthEndOfColonDef ;m
syn keyword gforthEndOfClassDef ;class
syn keyword gforthEndOfObjectDef ;object

" Forth-2012 structures
" Done after Gforth's <struct0x.fs>

syn keyword gforthDefine +field
syn keyword gforthDefine 2field:
syn keyword gforthDefine begin-structure
syn keyword gforthDefine cfield:
syn keyword gforthDefine dffield:
syn keyword gforthDefine end-structure
syn keyword gforthDefine ffield:
syn keyword gforthDefine field:
syn keyword gforthDefine sffield:

" Calls and returns

syn keyword gforthLoop exit
syn keyword gforthLoop recurse
syn keyword gforthDefine recursive
syn keyword gforthEndOfColonDef ;s

" Defered words

syn keyword gforthDefine <is>
syn keyword gforthDefine action-of
syn keyword gforthDefine defer
syn keyword gforthDefine defer!
syn keyword gforthDefine defer@
syn keyword gforthDefine defers
syn keyword gforthDefine is
syn keyword gforthDefine what's

syn match gforthDefine "\<\[is]\>"

" Input stream
" xxx todo

syn keyword gforthDefine evaluate
syn keyword gforthDefine execute-parsing
syn keyword gforthDefine execute-parsing-file
syn keyword gforthDefine interpret
syn keyword gforthDefine name
syn keyword gforthDefine parse
syn keyword gforthDefine parse-name
syn keyword gforthDefine parse-word
syn keyword gforthDefine refill
syn keyword gforthDefine restore-input
syn keyword gforthDefine save-input
syn keyword gforthDefine source
syn keyword gforthDefine source-id
syn keyword gforthDefine word

" Debugging

syn keyword gforthDebug .debugline
syn keyword gforthDebug assert(
syn keyword gforthDebug assert-level
syn keyword gforthDebug assert0(
syn keyword gforthDebug assert1(
syn keyword gforthDebug assert2(
syn keyword gforthDebug assert3(
syn keyword gforthDebug printdebugdata
syn match gforthDebug "\<\~\~\>"

"syn keyword gforthDebug )

" Assembler

syn keyword gforthAssembler ;code
syn keyword gforthAssembler assembler
syn keyword gforthAssembler code
syn keyword gforthAssembler end-code
syn keyword gforthAssembler flush-icache

" Character operations

syn keyword gforthCharOps (.)
syn keyword gforthCharOps (emit)
syn keyword gforthCharOps (key)
syn keyword gforthCharOps (key?)
syn keyword gforthCharOps (type)
syn keyword gforthCharOps .
syn keyword gforthCharOps .r
syn keyword gforthCharOps accept
syn keyword gforthCharOps at-xy
syn keyword gforthCharOps char
syn keyword gforthCharOps count
syn keyword gforthCharOps cr
syn keyword gforthCharOps d.
syn keyword gforthCharOps ekey
syn keyword gforthCharOps ekey>char
syn keyword gforthCharOps ekey?
syn keyword gforthCharOps emit
syn keyword gforthCharOps expect
syn keyword gforthCharOps hex.
syn keyword gforthCharOps key
syn keyword gforthCharOps key?
syn keyword gforthCharOps page
syn keyword gforthCharOps space
syn keyword gforthCharOps spaces
syn keyword gforthCharOps tib
syn keyword gforthCharOps toupper
syn keyword gforthCharOps type
syn keyword gforthCharOps typewhite
syn keyword gforthCharOps u.
syn keyword gforthCharOps ud.
syn keyword gforthCharOps xkey

" recognize 'char (' or '[char] (' correctly, so it doesn't
" highlight everything after the paren as a comment till a closing ')'
syn match gforthCharOps '\<char\s\+\S\+\>'
syn match gforthCharOps '\<\[char\]\s\+\S\+\>'

" 2012-09-14: programandala.net XXX FIXME:
syn region gforthCharOps start=+."\s+ skip=+\\"+ end=+"+

" Char-number conversion

syn keyword gforthConversion #
syn keyword gforthConversion #>
syn keyword gforthConversion #>>
syn keyword gforthConversion #s
syn keyword gforthConversion <#
syn keyword gforthConversion <<#
syn keyword gforthConversion >number
syn keyword gforthConversion convert
syn keyword gforthConversion d>f
syn keyword gforthConversion d>s
syn keyword gforthConversion digit
syn keyword gforthConversion dpl
syn keyword gforthConversion f>d
syn keyword gforthConversion f>s
syn keyword gforthConversion hld
syn keyword gforthConversion hold
syn keyword gforthConversion number
syn keyword gforthConversion number?
syn keyword gforthConversion s>d
syn keyword gforthConversion s>f
syn keyword gforthConversion sign

" xchar-ext
" XXX TODO distribute

syn keyword gforthForth +string
syn keyword gforthForth +x/string
syn keyword gforthForth -trailing-garbage
syn keyword gforthForth c!+?
syn keyword gforthForth set-enconding-fixed-width
syn keyword gforthForth set-enconding-utf-8
syn keyword gforthForth string-
syn keyword gforthForth x-size
syn keyword gforthForth x-width
syn keyword gforthForth x@+/string
syn keyword gforthForth x\string-
syn keyword gforthForth xc!+?
syn keyword gforthForth xc-size
syn keyword gforthForth xc@
syn keyword gforthForth xc@+
syn keyword gforthForth xchar+
syn keyword gforthForth xchar-
syn keyword gforthForth xemit
syn keyword gforthForth xhold

" Interpreter, wordbook, compiler

syn keyword gforthForth (local)
syn keyword gforthForth .id
syn keyword gforthForth <compilation
syn keyword gforthForth <interpretation
syn keyword gforthForth >body
syn keyword gforthForth >link
syn keyword gforthForth >name
syn keyword gforthForth >next
syn keyword gforthForth >view
syn keyword gforthForth abort
syn keyword gforthForth body>
syn keyword gforthForth bye
syn keyword gforthForth cfa
syn keyword gforthForth cold
syn keyword gforthForth comp'
syn keyword gforthForth compilation>
syn keyword gforthForth create-interpret/compile
syn keyword gforthForth execute
syn keyword gforthForth forget
syn keyword gforthForth here
syn keyword gforthForth id.
syn keyword gforthForth interpretation>
syn keyword gforthForth l>name
syn keyword gforthForth lastxt
syn keyword gforthForth latest
syn keyword gforthForth latestxt
syn keyword gforthForth link>
syn keyword gforthForth n>link
syn keyword gforthForth name>
syn keyword gforthForth name>comp
syn keyword gforthForth name>int
syn keyword gforthForth name>string
syn keyword gforthForth name>string
syn keyword gforthForth name?int
syn keyword gforthForth noop
syn keyword gforthForth pad
syn keyword gforthForth perform
syn keyword gforthForth postpone,
syn keyword gforthForth quit
syn keyword gforthForth state
syn keyword gforthForth view
syn keyword gforthForth view>
syn keyword gforthForth warnings
syn match gforthForth "'"
syn match gforthForth "\<\[']\>"
syn match gforthForth "\<\[comp']\>"
syn match gforthForth "\<\[compile]\>"
syn region gforthForth start=+\<abort"\s+ end=+"\>+

" Vocabularies

syn keyword gforthVocs #vocs
syn keyword gforthVocs >order
syn keyword gforthVocs also
syn keyword gforthVocs context
syn keyword gforthVocs current
syn keyword gforthVocs definitions
syn keyword gforthVocs find
syn keyword gforthVocs find-name
syn keyword gforthVocs forth
syn keyword gforthVocs forth-wordlist
syn keyword gforthVocs get-current
syn keyword gforthVocs get-order
syn keyword gforthVocs only
syn keyword gforthVocs order
syn keyword gforthVocs previous
syn keyword gforthVocs root
syn keyword gforthVocs seal
syn keyword gforthVocs search-wordlist
syn keyword gforthVocs set-current
syn keyword gforthVocs set-order
syn keyword gforthVocs table
syn keyword gforthVocs vlist
syn keyword gforthVocs vocabulary
syn keyword gforthVocs vocs
syn keyword gforthVocs wordlist
syn keyword gforthVocs words

" Files

syn keyword gforthFileMode bin
syn keyword gforthFileMode r/o
syn keyword gforthFileMode r/w
syn keyword gforthFileMode w/o
syn keyword gforthFiles close-dir
syn keyword gforthFiles close-file
syn keyword gforthFiles create-file
syn keyword gforthFiles delete-file
syn keyword gforthFiles emit-file
syn keyword gforthFiles file-position
syn keyword gforthFiles file-size
syn keyword gforthFiles file-status
syn keyword gforthFiles flush-file
syn keyword gforthFiles infile-execute
syn keyword gforthFiles key-file
syn keyword gforthFiles key?-file
syn keyword gforthFiles open-dir
syn keyword gforthFiles open-file
syn keyword gforthFiles outfile-execute
syn keyword gforthFiles read-dir
syn keyword gforthFiles read-file
syn keyword gforthFiles read-line
syn keyword gforthFiles rename-file
syn keyword gforthFiles reposition-file
syn keyword gforthFiles resize-file
syn keyword gforthFiles slurp-fid
syn keyword gforthFiles slurp-file
syn keyword gforthFiles sourcefilename
syn keyword gforthFiles stderr
syn keyword gforthFiles stdin
syn keyword gforthFiles stdout
syn keyword gforthFiles write-file
syn keyword gforthFiles write-line

" Paths

syn keyword gforthFiles .path
syn keyword gforthFiles also-path
syn keyword gforthFiles clear-path
syn keyword gforthFiles fpath
syn keyword gforthFiles open-path-file
syn keyword gforthFiles path+
syn keyword gforthFiles path-allot
syn keyword gforthFiles path=

" Blocks

syn keyword gforthBlocks +load
syn keyword gforthBlocks +thru
syn keyword gforthBlocks -->
syn keyword gforthBlocks block
syn keyword gforthBlocks block-included
syn keyword gforthBlocks block-offset
syn keyword gforthBlocks block-position
syn keyword gforthBlocks buffer
syn keyword gforthBlocks empty-buffer
syn keyword gforthBlocks empty-buffers
syn keyword gforthBlocks flush
syn keyword gforthBlocks get-block-fid
syn keyword gforthBlocks list
syn keyword gforthBlocks load
syn keyword gforthBlocks open-blocks
syn keyword gforthBlocks save-buffer
syn keyword gforthBlocks save-buffers
syn keyword gforthBlocks scr
syn keyword gforthBlocks thru
syn keyword gforthBlocks update
syn keyword gforthBlocks updated?
syn keyword gforthBlocks use

" Time

syn keyword gforthCommand ms
syn keyword gforthFunction time&date
syn keyword gforthFunction utime

" Misc
" xxx todo distribute

syn keyword gforthFunction bl
syn keyword gforthFunction cols
syn keyword gforthFunction depth
syn keyword gforthFunction false
syn keyword gforthFunction form
syn keyword gforthFunction rows
syn keyword gforthFunction true
syn keyword gforthMemory off
syn keyword gforthMemory on

" OS shell

syn keyword gforthForth $?
syn keyword gforthForth arg
syn keyword gforthForth argc
syn keyword gforthForth argv
syn keyword gforthForth getenv
syn keyword gforthForth next-arg
syn keyword gforthForth sh
syn keyword gforthForth shift-args
syn keyword gforthForth system

" Environmental queries

syn keyword gforthEnvironment environment-wordlist
syn keyword gforthEnvironment environment?
syn keyword gforthEnvironment gforth
syn keyword gforthEnvironment os-class

" Common use

syn match gforthFunction "\<\[false]\>"
syn match gforthFunction "\<\[true]\>"

" Numbers

syn keyword gforthMath decimal
syn keyword gforthMath hex
syn keyword gforthMath base
syn match gforthInteger '\<-\=[0-9.]*[0-9.]\+\>'
syn match gforthInteger '\<&-\=[0-9.]*[0-9.]\+\>'
" recognize hex and binary numbers, the '$' and '%' notation is for gforth
"syn match gforthInteger '\<\$\x*\x\+\>' " *1* --- dont't mess
syn match gforthInteger '\<\$\x\+\>' " *1* --- dont't mess
" xxx removed 2013-06-06, it highlights other words with [a-f0-9]:
"syn match gforthInteger '\<\x*\d\x*\>'  " *2* --- this order!
syn match gforthInteger '\<0x\x\+\>'  " *2* --- this order!
syn match gforthInteger '\<%[0-1]*[0-1]\+\>'
"syn match gforthFloat '\<-\=\d*[.]\=\d\+[DdEe]\d\+\>'
"syn match gforthFloat '\<-\=\d*[.]\=\d\+[DdEe][-+]\d\+\>'

"" XXX TODO 2015-01-17: this does not work (0.7.3) the way the manual says:
"syn match gforthInteger /\<'.\+\>/  " *2* --- this order!

" XXX If you find this overkill you can remove it. This has to come after the
" highlighting for numbers otherwise it has no effect.
syn region gforthComment start='\<0 \[if\]\>' end='\<\[endif\]\>' end='\<\[then\]\>' contains=forthTodo
" (* *) block comments (not in Gforth):
syn region gforthComment start='\<(\*\>' end='\<\*)\>' contains=forthTodo


" Strings

syn match gforthString /\<'.'\>/
syn region gforthString start=+\<\S*"\>+ end=+"+ end=+$+ contains=@Spell
syn region gforthString start=+\<\S*\\"\>+ skip=+\\"+ end=+"+ end=+$+ contains=@Spell

" Comments

syn match gforthComment '\<\\\>.*$' contains=@Spell,gforthTodo,gforthSpaceError
syn match gforthComment '\<#!\>.*$' 

" The first version does not work:
"syn match gforthComment '\<\.(\>' end=')' contains=@Spell,gforthTodo,gforthSpaceError
syn match gforthComment '\<\.(\s[^)]*)' contains=@Spell,gforthTodo,gforthSpaceError

syn region gforthComment start='\<(\>' end=')' contains=@Spell,gforthTodo,gforthSpaceError
"syn region gforthComment start='\</\*' end='\*/' contains=@Spell,gforthTodo,gforthSpaceError

syn match gforthComment '\<\\G\s.*$' contains=@Spell,gforthTodo,gforthSpaceError " Gforth comment for documentation

" Include files

syn keyword gforthInclude included
syn keyword gforthInclude required
syn match gforthInclude '\<include\s\+\k\+'
syn match gforthInclude '\<needs\s\+\k\+'
syn match gforthInclude '\<require\s\+\k\+'

" Locals

syn region gforthLocals start='\<{\>' end='\<}\>'
syn region gforthDeprecated start='locals|' end='|'

" Define the highlighting.

hi def link gforthTodo Todo
hi def link gforthOperator Operator
hi def link gforthMath Number
hi def link gforthInteger Number
hi def link gforthFloat Float
hi def link gforthStack Special
hi def link gforthReturnStack Special
hi def link gforthFloatStack Special
hi def link gforthStackPointers Special
hi def link gforthMemory Function
hi def link gforthAdrArith Function
hi def link gforthMemBlks Function
hi def link gforthCond Conditional
hi def link gforthLoop Repeat
hi def link gforthColonDef Define
hi def link gforthEndOfColonDef Define
hi def link gforthDefine Define
hi def link gforthStructure Define
hi def link gforthDebug Debug
hi def link gforthAssembler Include
hi def link gforthCharOps Character
hi def link gforthConversion String
hi def link gforthForth Statement
hi def link gforthEnvironment Statement
hi def link gforthVocs Statement
hi def link gforthString String
hi def link gforthComment Comment
hi def link gforthClassDef Define
hi def link gforthEndOfClassDef Define
hi def link gforthObjectDef Define
hi def link gforthEndOfObjectDef Define
hi def link gforthInclude Include
hi def link gforthLocals Type " nothing else uses type and locals must stand out
hi def link gforthDeprecated Error " if you must, change to Type
hi def link gforthFileMode Function
hi def link gforthFiles Statement
hi def link gforthBlocks Statement
hi def link gforthSpaceError Error
hi def link gforthFunction Function
hi def link gforthCommand Statement

let b:current_syntax = "gforth"

"Show tab and trailing characters
set listchars=tab:»·,trail:·
set list

let &cpo = s:cpo_save
unlet s:cpo_save

" ----------------------------------------------
" History
"
" 2012-12-28: Start.
" 2013-06-04: New words. Some non-Gforth words removed.
" 2013-06-06: Many new words. Removed hexadecimal numbers without the '$' prefix.
" 2013-06-07: New word: 'sourcefilename'.
" 2013-06-08: New: completed the section "Memory Blocks"
" 2013-06-08: New: 'outfile-execute' and 'infile-execute'.
" 2013-06-09: New: defered words completed after the manual.
" 2013-06-17: Fix: Todo marks were not highlighted: 'forthTodo' has not been
" completely renamed to 'gforthTodo'; similar case with 'forthSpaceError'.
" 2013-06-19: New: 'latest' and 'latestxt'; 'accept'; 'compile' is deleted
" 2013-06-19: Change: Many words moved from "gforthDefine" to "gforthForth" and
" other sections.
" 2013-06-20: New: 'marker', 'warnings'.
" 2013-06-26: New: 'at-xy'.
" 2013-06-27: New: some low level words, e.g. '(emit)', '(type)', '(key)'...
" 2013-07-11: New: 'char-'.
" 2013-07-12: New: 'source' and 'source-id'.
" 2013-07-20: New: '!csp' and '?csp'.
" 2013-07-23: New: 'resize' and 'free'.
" 2013-08-08: New: 'append'.
" 2013-08-16: New: Galope's select structure (not in Gforth): 'select', 'endselect', 'cond', 'range', 'equal', 'when'.
" 2013-08-23: New: '2literal'.
" 2013-08-30: Fix: '[' has to be defined before any other word with brackets.
" 2013-08-30: New: '[defined]' and '[undefined]'.
" 2013-09-02: New: Words of xchar-ext.
" 2013-09-13: New: 'save-mem'.
" 2013-10-25: New: 'save-input', 'restore-input', 'sliteral'.
" 2013-10-30: New: 'system', 'sh', 'getenv', '$?'; all eight path words.
" 2013-11-07: New: 'try', 'endtry', 'iferror', 'endtry-iferror', 'restore',
" 'recover', 'exception'.
" 2013-11-08: New: '.', 'd.', 'u.', 'ud.', 'hex.'.
" 2013-11-09: New: '?exit', '+place'.
" 2013-11-11: Fix: 'str-prefix?' changed to 'string-prefix?'.
" 2013-11-11: New: 'arg', 'argc', 'argv', 'shift-args', next-arg', 'skip'.
" 2013-11-25: New: environmental queries.
" 2013-11-26: New: '(*' and '*)' (not in Gforth).
" 2013-11-27: Fix: '0 [if] ... [then]' block comments lacked word delimiters in
" the zone regex.
" 2013-11-28: Fix: regex of 's\"' was wrong; 'c\"' removed.
" 2013-11-30: New: Forth-2012 structures; 'noname' and ':noname'.
" 2013-12-10: New: 'noop'.
" 2013-12-12: New: '.r', 's,'.
" 2013-12-31: Removed: '/*'and '*/' block comments, not in Forth.
" 2013-12-31: New: '#!' as line comment.
" 2014-02-14: New: 'form', 'rows' and 'cols'.
" 2014-05-25: New: 'quit'.
" 2014-05-25: Change: Galope's select structure (not in Gforth) removed.
" 2014-07-25: New: 'recurse', 'recursive', ';s'.
" 2014-10-15: New: '[[' and ']]'.
" 2014-10-19: Fix: space in OOP definitions.
" 2014-10-19: New: 'space' and 'spaces'.
" 2014-10-21: New: 'page'.
" 2014-10-26: Fix: All words in brackets were highlighted also as part of a longer word.
" 2014-10-27: Fix: typo in 'buffer'; comments; strings.
" 2015-01-06: New: "0x" notation for hex numbers.
" 2015-01-13: New: 'set listchars' and 'set list', from AsciiDoc syntax file.
" 2015-01-17: New: 'open-dir', 'read-dir', 'close-dir'.
" 2015-01-17: New: highlight single quoted chars as strings, with or without
" the closing quote, e.g.: 'G' emit 'F .
" 2015-01-26: New: 'typewhite', 'toupper'.
" 2015-01-29: New: 'id.' and '.id'.
" 2015-02-01: New: All structure words.
" 2015-02-15: Change: UTF-8 encoding.
" 2015-02-26: Vim license.

" vim:et:ts=4:sts=4:sw=4:nocindent:smartindent:
