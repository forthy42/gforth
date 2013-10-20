\ callable object

require mini-oof2.fs

object class
    >body cell var call-xt
    nip vtsize swap
end-class callable

' spaces cell- @ callable vtsize move

: do-callable ( body -- )
    body> >o call-xt perform o> ;

: callable! ( xt callable -- )
   ['] do-callable >body over does-code! >o call-xt ! o> ;