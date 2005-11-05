\ regexp test

charclass [bl-]   blanks +class '- +char
charclass [0-9(]  '( +char '0 '9 ..char

: telnum ( addr u -- flag )
    (( {{ ` (  \( \d \d \d \) ` ) || \( \d \d \d \) }}  blanks c?
    \( \d \d \d \) [bl-] c?
    \( \d \d \d \d \) {{ \$ || -\d }} )) ;

: ?tel ( addr u -- ) telnum
    IF  '( emit \1 type ." ) " \2 type '- emit \3 type ."  succeeded"
    ELSE \0 type ."  failed " THEN ;

: ?tel-s ( addr u -- ) ?tel ."  should succeed" space depth . cr ;
: ?tel-f ( addr u -- ) ?tel ."  should fail" space depth . cr ;

." --- Telephone number match ---" cr
s" (123) 456-7890" ?tel-s
s" (123) 456-7890 " ?tel-s
s" (123)-456 7890" ?tel-f
s" (123) 456 789" ?tel-f
s" 123 456-7890" ?tel-s
s" 123 456-78909" ?tel-f

: telnum2 ( addr u -- flag )
    (( // {{ [0-9(] -c? || \^ }}
    {{ ` (  \( \d \d \d \) ` ) || \( \d \d \d \) }}  blanks c?
    \( \d \d \d \) [bl-] c?
    \( \d \d \d \d \) {{ \$ || -\d }} )) ;

: ?tel2 ( addr u -- ) telnum2
    IF   '( emit \1 type ." ) " \2 type '- emit \3 type ."  succeeded"
    ELSE \0 type ."  failed " THEN  cr ;
." --- Telephone number search ---" cr
s" blabla (123) 456-7890" ?tel2
s" blabla (123) 456-7890 " ?tel2
s" blabla (123)-456 7890" ?tel2
s" blabla (123) 456 789" ?tel2
s" blabla 123 456-7890" ?tel2
s" blabla 123 456-78909" ?tel2
s" (123) 456-7890" ?tel2
s"  (123) 456-7890 " ?tel2
s" a (123)-456 7890" ?tel2
s" la (123) 456 789" ?tel2
s" bla 123 456-7890" ?tel2
s" abla 123 456-78909" ?tel2

." --- Number extraction test ---" cr

charclass [0-9,./:]  '0 '9 ..char ', +char '. +char '/ +char ': +char

: ?num
    (( // \( {++ [0-9,./:] c? ++} \) ))
    IF  \1 type  ELSE  \0 type ."  failed"  THEN   cr ;

s" 1234" ?num
s" 12,345abc" ?num
s" foobar12/345:678.9abc" ?num
s" blafasel" ?num

." --- String test --- " cr

: ?string
    (( // \( {{ =" foo" || =" bar" || =" test" }} \) ))
    IF  \1 type  cr THEN ;
s" dies ist ein test" ?string
s" foobar" ?string
s" baz bar foo" ?string
s" Hier kommt nichts vor" ?string

." --- longer matches test --- " cr

: ?foos
    (( \( {** =" foo" **} \) ))
    IF  \1 type  ELSE  \0 type ."  failed"  THEN  cr ;

: ?foobars
    (( // \( {** =" foo" **} \) \( {++ =" bar" ++} \) ))
    IF  \1 type ', emit \2 type  ELSE  \0 type ."  failed"  THEN  cr ;

: ?foos1
    (( // \( {+ =" foo" +} \) \( {++ =" bar" ++} \) ))
    IF  \1 type ', emit \2 type  ELSE  \0 type ."  failed"  THEN  cr ;

s" foobar" ?foos
s" foofoofoobar" ?foos
s" fofoofoofofooofoobarbar" ?foos
s" bla baz bar" ?foos
s" foofoofoo" ?foos

s" foobar" ?foobars
s" foofoofoobar" ?foobars
s" fofoofoofofooofoobarbar" ?foobars
s" bla baz bar" ?foobars
s" foofoofoo" ?foobars

s" foobar" ?foos1
s" foofoofoobar" ?foos1
s" fofoofoofofooofoobarbar" ?foos1
s" bla baz bar" ?foos1
s" foofoofoo" ?foos1

script? [IF] bye [THEN]
