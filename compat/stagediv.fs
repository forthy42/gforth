\ staged division and division by constants

\ This file is in the public domain. NO WARRANTY.

\ Uses division by multiplying with the reciprocal.  This is faster if
\ many dividends are divided by the same divisor.

\ Staged division usage example:
\ <udivisor> uprepare/ 2>r <udividend1> 2r@ up/ . <udividend2> 2r@ up/ . ...

\ The divisor must not be 0 or 1.


\ Implementation: First, the unsigned case:

\ The reciprocal is represented as ceil(2^(2w)/divisor), where w is
\ the cell size.  This means that we use w*2w->3w multiplication for
\ implementing UP/, resulting in ~dividend*2^(2w)/divisor.  We then
\ take only the top cell of the result, getting rid of the factor
\ 2^(2w), and truncating the result; intuitively, the earlier ceil
\ operation makes sure that the truncation does not round down too
\ much, which would happen if we had used floor earlier; in the
\ following, I explain why it does not result in too-big results:

\ !!

!! under construction

: up/ ( u ud -- u2 )
    >r over um* nip 0 rot r> um* d+ nip ;

: uprepare/ ( u -- ud )
    \ compute ceil(2^(2w)/u) as floor((2^(2w)+u-1)/u)
    >r 0 1 r@ um/mod ( m qh )
    r@ 1- rot r> um/mod nip swap ;

    
