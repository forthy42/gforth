\ This contains the same Forth source as mini-oof.fs, but
\ also contains glossary information for the manual.

: method ( m v "name" -- m' v ) \ mini-oof
    \G Define a selector.
    Create  over , swap cell+ swap
  DOES> ( ... o -- ... ) @ over @ + @ execute ;

: var ( m v size "name" -- m v' ) \ mini-oof
    \G Define a variable with @var{size} bytes.
    Create  over , +
  DOES> ( o -- addr ) @ + ;

: class ( class -- class selectors vars ) \ mini-oof
    \G Start the definition of a class.
    dup 2@ ;

: end-class  ( class selectors vars "name" -- ) \ mini-oof
    \G End the definition of a class.
    Create  here >r , dup , 2 cells ?DO ['] noop , 1 cells +LOOP
    cell+ dup cell+ r> rot @ 2 cells /string move ;

: defines ( xt class "name" -- ) \ mini-oof
    \G Bind @var{xt} to the selector @var{name} in class @var{class}.
    ' >body @ + ! ;

: new ( class -- o ) \ mini-oof
    \G Create a new incarnation of the class @var{class}.
    here over @ allot swap over ! ;

: :: ( class "name" -- ) \ mini-oof colon-colon
    \G Compile the method for the selector @var{name} of the class
    \G @var{class} (not immediate!).
    ' >body @ + @ compile, ;

Create object ( -- a-addr ) \ mini-oof
1 cells , 2 cells ,
\G @var{object} is the base class of all objects.
