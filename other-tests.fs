\ various tests, especially for bugs that have been fixed

\ combination of marker and locals
marker foo1
marker foo2
foo2

: bar { xxx yyy } ;

foo1

\ comments across several lines

( fjklfjlas;d
abort" ( does not work across lines"
)