\ image dump                                           15nov94py


: dump-fi ( addr u -- )  w/o open-file throw >r
  forthstart here over - dup forthstart cell+ !
                         r@ write-file throw
  relinfo here forthstart - 1- 8 cells / 1+ r@ write-file throw
  r> close-file throw ;
