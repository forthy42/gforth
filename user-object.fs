\ Mini-OOF extension: current object in user variable  08jan13py

[IFUNDEF] coverage? 0 Value coverage? [THEN]

Variable class-o

: user-o ( "name" -- )
    0 uallot class-o !  User ;

: umethod ( m v -- m' v )
    coverage? >r  false to coverage?
    over >r : postpone u#exec class-o @ , r> cell/ , postpone ;
    swap cell+ swap
    ['] umethod, set-optimizer ['] umethod! set-to ['] umethod@ set-defer@
    r> to coverage? ;

: uvar ( m v size -- m v' )
    coverage? >r  false to coverage?
    over >r : postpone u#+ class-o @ , r> , postpone ; +
    ['] uvar, set-optimizer
    r> to coverage? ;

: uclass ( c "name" -- c m v )
    ' execute next-task - class-o ! dup cell- cell- 2@ ;
