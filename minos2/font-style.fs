\ MINOS2 font style example

\ Copyright (C) 2018 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

Variable font-path
: font-path+ ( "font" -- )
    parse-name 2dup open-dir 0= IF
	close-dir throw font-path also-path
    ELSE  drop 2drop  THEN ;
: ?font ( addr u -- addr' u' true / false )
    font-path open-path-file 0= IF
	rot close-file throw true
    ELSE
	false
    THEN ;
: fonts= ( "font1|font2|..." -- addr u )
    parse-name  BEGIN  dup  WHILE  '|' $split 2swap ?font  UNTIL  2nip
    ELSE  true abort" No suitable font found"  THEN  save-mem ;

[IFDEF] android
    font-path+ /system/fonts
[ELSE]
    font-path+ /usr/share/fonts/truetype/
    font-path+ /usr/share/fonts/truetype/noto
    font-path+ /usr/share/fonts/truetype/droid
    font-path+ /usr/share/fonts/truetype/liberation
    font-path+ /usr/share/fonts/truetype/arphic-gkai00m
    font-path+ /usr/share/fonts/truetype/emoji
    font-path+ /usr/share/fonts/opentype/
    font-path+ /usr/share/fonts/opentype/noto
[THEN]

Vocabulary fonts

\ !!FIXME!! create a font matrix: regular/bold/italic/bi, various sizes, various shapes

get-current also fonts definitions

fonts= NotoSans-Regular.ttf|DroidSans.ttf|Roboto-Medium.ttf|LiberationSans-Regular.ttf
2Value sans

fonts= NotoSans-Italic.ttf|LiberationSans-Italic.ttf|Roboto-Italic.ttf
2Value sans-i

fonts= NotoSans-Bold.ttf|LiberationSans-Bold.ttf|Roboto-Bold.ttf
2Value sans-b

fonts= NotoSans-BoldItalic.ttf|LiberationSans-BoldItalic.ttf|Roboto-BoldItalic.ttf
2Value sans-bi

fonts= NotoSerif-Regular.ttf|LiberationSerif-Regular.ttf
2Value serif

fonts= NotoSerif-Bold.ttf|LiberationSerif-Bold.ttf
2Value serif-b

fonts= NotoSerif-Italic.ttf|LiberationSerif-Italic.ttf
2Value serif-i

fonts= NotoSerif-BoldItalic.ttf|LiberationSerif-BoldItalic.ttf
2Value serif-bi

fonts= DroidSansMono.ttf|LiberationMono-Regular.ttf
2Value mono

fonts= DroidSansMono.ttf|LiberationMono-Bold.ttf
2Value mono-b

fonts= DroidSansMono.ttf|LiberationMono-Italic.ttf
2Value mono-i

fonts= DroidSansMono.ttf|LiberationMono-BoldItalic.ttf
2Value mono-bi

[IFDEF] android
    fonts= DroidSansFallback.ttf|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
[ELSE]
    fonts= gkai00mp.ttf|NotoSansSC-Regular.otf|NotoSansCJK-Regular.ttc
[THEN]
2Value chinese

fonts= SamsungColorEmoji.ttf|NotoColorEmoji.ttf|emojione-android.ttf|TwitterColorEmojiv2.ttf
2Value emoji

previous set-current
