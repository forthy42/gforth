\ image dump                                           15nov94py


: dump-fi ( addr u -- )  w/o bin create-file throw >r
  s" gforth00" r@ write-file throw
  forthstart here over - dup forthstart cell+ !
                         r@ write-file throw
  relinfo here forthstart - 1- 8 cells / 1+ r@ write-file throw
  r> close-file throw ;

: savesystem ( "name" -- )  name dump-fi ;
