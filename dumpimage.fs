\ image dump                                           15nov94py

Create magic  s" gforth00" here over allot swap move

'1 1 cells + 0 pad ! -1 pad c! pad @ 0< +  magic 7 chars + c!

: dump-fi ( addr u -- )  w/o bin create-file throw >r
  magic 8 r@ write-file throw
  forthstart here over - dup forthstart cell+ !
                         r@ write-file throw
\  relinfo here forthstart - 1- 8 cells / 1+ r@ write-file throw
  r> close-file throw ;

: savesystem ( "name" -- )  name dump-fi ;
