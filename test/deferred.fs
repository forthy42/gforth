\ test DEFER and friends

\ This file is in the public domain. NO WARRANTY.

require tester.fs

{ defer defer1 -> }
{ : is-defer1 is defer1 ; -> }
{ : action-defer1 action-of defer1 ; -> }
{ ' * ' defer1 defer! -> }
{ 2 3 defer1 -> 6 }
{ ' defer1 defer@ -> ' * }
{ action-of defer1 -> ' * }
{ action-defer1 -> ' * }
{ ' + is defer1 -> }
{ 1 2 defer1 -> 3 }
{ ' defer1 defer@ -> ' + }
{ action-of defer1 -> ' + }
{ action-defer1 -> ' + }
{ ' - is-defer1 -> }
{ 1 2 defer1 -> -1 }
{ ' defer1 defer@ -> ' - }
{ action-of defer1 -> ' - }
{ action-defer1 -> ' - }
