\ various tests, especially for bugs that have been fixed

\ combination of marker and locals
marker foo1
marker foo2
foo2

: bar { xxx yyy } ;

foo1

\ locals in an if structure
: locals-test1
    lp@ swap
    if
	{ a } a
    else
    endif
    lp@ <> abort" locals in if error 1" ;

0 locals-test1
1 locals-test1

\ comments across several lines

( fjklfjlas;d
abort" ( does not work across lines"
)

s" ( testing ( without delimited by newline in non-files" evaluate

\ last test!
\ testing '(' without ')' at end-of-file
." expect ``warning: ')' missing''" cr
(
