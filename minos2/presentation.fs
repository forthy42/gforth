\ Presentation on ΜΙΝΩΣ2 made in ΜΙΝΩΣ2

\ Copyright (C) 2018 Bernd Paysan


\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU Affero General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU Affero General Public License for more details.

\ You should have received a copy of the GNU Affero General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

require minos2/widgets.fs

[IFDEF] android
    hidekb also android >changed hidestatus >changed previous
[THEN]

also minos

ctx 0= [IF]  window-init  [THEN]

require minos2/font-style.fs

: update-size# ( -- )
    dpy-w @ s>f 42e f/ fround to font-size#
    font-size# 16e f/ m2c:curminwidth% f!
    dpy-h @ s>f dpy-w @ s>f f/ 45% f/ font-size# f* fround to baseline#
    dpy-w @ s>f 1280e f/ to pixelsize# ;

update-size#

require minos2/text-style.fs

Variable slides[]
Variable slide#

0 Value m2-img

: >slides ( o -- ) slides[] >stack ;

glue ' new static-a with-allocater Constant glue-left
glue ' new static-a with-allocater Constant glue-right

: glue0 ( -- ) 0e fdup
    [ glue-left  .hglue-c ]L df!
    [ glue-right .hglue-c ]L df! ;
: trans-frame ( o -- )
    >o $00000000 to frame-color o> ;
: solid-frame ( o -- )
    >o $FFFFFFFF to frame-color o> ;
: !slides ( nprev n -- )
    update-size# update-glue
    over slide# !
    slides[] $[] @ /flip drop
    slides[] $[] @ /flop drop glue0 ;
: fade-img ( r0..1 img1 img2 -- ) >r >r
    $FF fm* f>s $FFFFFF00 or dup
    r> >o to frame-color parent-w .parent-w /flop drop o> invert $FFFFFF00 or
    r> >o to frame-color parent-w .parent-w /flop drop o> ;
: anim!slides ( r0..1 n -- )
    slides[] $[] @ /flop drop
    fdup fnegate dpy-w @ fm* glue-left  .hglue-c df!
    -1e f+       dpy-w @ fm* glue-right .hglue-c df! ;

: prev-anim ( n r0..1 -- )
    dup 0<= IF  drop fdrop  EXIT  THEN
    fdup 1e f>= IF  fdrop
	dup 1- swap !slides +sync +config  EXIT
    THEN
    1e fswap f-
    1- sin-t anim!slides +sync +config ;

: next-anim ( n r0..1 -- )
    dup slides[] $[]# 1- u>= IF  drop fdrop  EXIT  THEN
    fdup 1e f>= IF  fdrop
	dup 1+ swap !slides +sync +config  EXIT
    THEN
    1+ sin-t anim!slides +sync +config ;

1e FValue slide-time%

: prev-slide ( -- )
    slide-time% anims[] $@len IF  anim-end .2e f*  THEN
    slide# @ ['] prev-anim >animate ;
: next-slide ( -- )
    slide-time% anims[] $@len IF  anim-end .2e f*  THEN
    slide# @ ['] next-anim >animate ;

: slide-frame ( glue color -- o )
    font-size# 70% f* }}frame ;

box-actor class
    \ sfvalue: s-x
    \ sfvalue: s-y
    \ sfvalue: last-x
    \ sfvalue: last-t
    \ sfvalue: speed
end-class slide-actor

:noname ( axis dir -- ) nip
    0< IF  prev-slide  ELSE  next-slide  THEN ; slide-actor is scrolled
:noname ( rx ry b n -- )  dup 1 and 0= IF
	over $8  and IF  prev-slide  2drop fdrop fdrop  EXIT  THEN
	over $10 and IF  next-slide  2drop fdrop fdrop  EXIT  THEN
	over -$2 and 0= IF
	    fover caller-w >o x f- w f/ o>
	    fdup 0.1e f< IF  fdrop  2drop fdrop fdrop  prev-slide  EXIT
	    ELSE  0.9e f> IF  2drop fdrop fdrop  next-slide  EXIT  THEN  THEN
	THEN  THEN
    [ box-actor :: clicked ] ; slide-actor to clicked
:noname ( ekey -- )
    case
	k-up      of  prev-slide  endof
	k-down    of  next-slide  endof
	k-prior   of  prev-slide  endof
	k-next    of  next-slide  endof
	k-volup   of  prev-slide  endof
	k-voldown of  next-slide  endof
	s-k3      of  1e ambient% sf!
	    Ambient 1 ambient% opengl:glUniform1fv  +sync endof
	k-f3      of  ambient% sf@ 0.1e f+ 1e fmin  ambient% sf!
	    Ambient 1 ambient% opengl:glUniform1fv  +sync endof
	k-f4      of  ambient% sf@ 0.1e f- 0e fmax  ambient% sf!
	    Ambient 1 ambient% opengl:glUniform1fv  +sync endof
	s-k5      of  1e saturate% sf!
	    Saturate 1 saturate% opengl:glUniform1fv  +sync endof
	k-f5      of  saturate% sf@ 0.1e f+ 3e fmin saturate% sf!
	    Saturate 1 saturate% opengl:glUniform1fv  +sync endof
	k-f6      of  saturate% sf@ 0.1e f- 0e fmax saturate% sf!
	    Saturate 1 saturate% opengl:glUniform1fv  +sync endof
	[ box-actor :: ekeyed ]  EXIT
    endcase ; slide-actor to ekeyed
\ :noname ( $xy b -- )  dup 1 > IF
\ 	[ box-actor :: touchdown ] EXIT
\     THEN  drop
\     xy@ to s-y to s-x ftime to last-t
\     true to grab-move? ; slide-actor is touchdown
\ :noname ( $xy b -- ) dup 1 > IF
\ 	[ box-actor :: touchmove ] EXIT
\     THEN  drop xy@ fdrop
\     ftime last-t fover to last-t f- \ delta-t
\     last-x fover to last-x f-       \ delta-x
\     fswap f/ caller-w .w f/ to speed
\     last-x s-x f- caller-w .w f/ fdup f0< IF \ to the right
\ 	1e f+ slide# @ prev-anim
\     ELSE \ to the left
\ 	slide# @ next-anim
\     THEN ; slide-actor is touchmove
:noname ( $xy b -- ) 2dup [ box-actor :: touchmove ] drop
    xy@ dpy-h @ s>f fswap f- dpy-h @ 2/ fm/ lightpos-xyz sfloat+ sf!
    dpy-w @ s>f f- dpy-w @ 2/ fm/ lightpos-xyz sf!
    3.0e lightpos-xyz 2 sfloats + sf!
    LightPos 1 lightpos-xyz opengl:glUniform3fv  +sync ; slide-actor is touchmove
\ :noname ( $xy b -- )  dup 1 > IF
\ 	[ box-actor :: touchup ] EXIT
\     THEN  2drop
\     slide# @ 1e next-anim
\     false to grab-move? ; slide-actor is touchup

: slide[] ( o -- o )
    >o slide-actor new to act o act >o to caller-w o> o o> ;

glue-left  >o 1glue vglue-c glue! 1glue dglue-c glue! o>
glue-right >o 1glue vglue-c glue! 1glue dglue-c glue! o>

tex: minos2
' minos2 "net2o-minos2.png" 0.666e }}image-file Constant minos2-glue

: logo-img ( xt xt -- o o-img ) 2>r
    baseline# 0e to baseline#
    {{ 2r> }}image-tex dup >r /right
    glue*l }}glue
    }}v outside[] >o font-size# f2/ to border o o>
    to baseline# r> ;

: pres-frame ( color -- o1 o2 ) \ drop $FFFFFFFF
    color, glue*wh swap slide-frame dup .button1 simple[] ;

{{
{{ glue-left }}glue

\ page 0
{{
$FFFFFFFF pres-frame
{{
glue*l }}glue \ ) $CCDDDD3F 4e }}frame dup .button1
"ΜΙΝΩΣ2 GUI" /title
"Ein leichtgewichtiges GUI für Gforth und net2o" /subtitle
glue*2 }}glue  \ ) $CCDDDD3F 4e }}frame dup .button1
"Bernd Paysan" /author
"Forth–Tagung 2018, Essen" /location
glue*l }}glue \ ) $CCDDDD3F 4e }}frame dup .button1
}}v box[] >o font-size# to border o Value title-page o o>
}}z box[] dup >slides

\ page 1
{{
$FFFFFFFF pres-frame
{{
"Motivation" /title
glue*l }}glue \ ) $CCDDDD3F 4e }}frame dup .button1
tex: bad-gateway
' bad-gateway "bad-gateway.png" 0.666e }}image-file
Constant bgw-glue /center
glue*l }}glue \ ) $CCDDDD3F 4e }}frame dup .button1
}}v box[] >bdr
}}z box[] /flip dup >slides

\ page 2
{{
$FF7F7FFF pres-frame
{{
"5 Jahre nach Snowden" /title
"Was hat sich verändert?" \\
\skip
"Politik" /subsection
blackish
"  Fake News/Hate Speech sind jetzt Ausreden für Zensur #NetzDG" "🤦" e\\
"  Die Crypto Wars heißen jetzt “reasonable encryption”" "🤦🤦" e\\
"  Legalize it (Schleppnetzüberwachung)" "🤦🤦🤦" e\\
"  Der Link ist immer noch nicht ganz tot! (EuGH und LG Humbug)" "🤦🤦🤦🤦" e\\
"  Privacy: Niemand muss das Interwebs benutzen (Jim Sensenbrenner)" "🤦🤦🤦🤦🤦" e\\
"  “Crypto” bedeutet nun BitCoin" "🤦🤦🤦🤦🤦🤦" e\\
\skip
"Mitbewerber" /subsection
"  Stasi–artige Zersetzung (Tor project)" \\
"  Cambridge Analytica–Skandal (Facebook)" \\
\skip
"Lösungen" /subsection
"  net2o fängt an, benutztbar zu werden" \\
glue*l }}glue \ ) $CCDDDD3F 4e }}frame dup .button1
}}v box[] >o o Value snowden-page font-size# to border o o>
}}z box[] /flip dup >slides

\ page 5
{{
$BFBFFFFF pres-frame
{{
"ΜΙΝΩΣ2–Technologie" /title
"ΜΙΝΩΣ2 ist unterhalb des DOM–Layers" \\
\skip
vt{{
"Rendering: " "OpenGL (ES), Vulkan backend möglich" b\\
"Font nach Textur: " "Freetype–GL (mit eigenen Verbesserungen)" b\\
"Image nach Textur: " "SOIL2 (TBD: AV1 photo?)" b\\
"Video nach Textur: " "OpenMAX AL (Android), gstreamer für Linux (geplant)" b\\
"Koordinaten: " "Single float, Ursprung links unten" b\\
{{ "Typesetting: " b0 blackish
"Boxes & Glues ähnlich wie " }}text
\LaTeX
" — mit Ober– & Unterlängen" }}text glue*l }}glue }}h box[] >bl
"" "Glues können schrumpfen, nicht nur wachsen" b\\
"Object System: " "extrem leichtgewichtiges Mini–OOF2" b\\
"Klassenzahl: " "Weniger Klassen, viele mögliche Kombinationen" b\\
}}vt
glue*l }}glue \ ) $CCDDDD3F 4e }}frame dup .button1
}}v box[] >bdr
}}z box[] /flip dup >slides

\ page 6
{{
$FFBFFFFF pres-frame
{{
"ΜΙΝΩΣ2 Widgets" /title
"Design-Prinzip ist eine Lego–artige Kombination aus vielen sehr einfachen Objekten" \\
{{ {{ vt{{
"actor " "Basis–Klasse, die auf alle Aktionen reagiert (Klicks, Touch, Tasten)" b\\
"widget " "Basis–Klasse für alle sichtbaren Objekte" b\\
{{ "edit " b0 blackish "Editierbarer Text: " }}text
"复活节快乐！" }}edit dup Value edit-field glue*l }}glue }}h edit-field ' true edit[] >bl
\sans \latin \normal
"glue " "Basis–Klasse für flexible Objekte" b\\
"tile " "Farbiges Rechteck" b\\
"frame " "Farbiges Rechteck mit Rand" b\\
"text " "Text–Element+Emoji 😀🤭😁😂😇😈🙈🙉🙊💓💔💕💖💗💘🍺🍻🎉🎻🎺🎷" b\\
"icon " "Bild aus der Icon–Textur" b\\
"image " "Größeres Bild" b\\
"animation " "Klasse für Animationen" b\\
"canvas " "Vektor–Grafik (TBD)" b\\
"video " "Video–Player (TBD)" b\\
}}vt
glue*l }}glue
tex: vp0 glue*l ' vp0 }}vp vp[]
$FFBFFFFF color, dup to slider-color to slider-fgcolor
font-size# f2/ f2/ to slider-border
dup font-size# f2/ fdup vslider
}}h box[]
}}v box[] >bdr
}}z box[]
/flip dup >slides

\ page 7
{{
$BFFFFFFF pres-frame
{{
"ΜΙΝΩΣ2 Boxen" /title
{{
"Wie bei " }}text \LaTeX " werden Texte/Widgets in Boxen angeordnet" }}text glue*l }}h box[]
>bl
\skip
vt{{
"hbox " "Horizontale Box, gemeinsame Baseline" b\\
"vbox " "Verticale Box, Mindestdistanz eine baselineskip (der eingebetteten Boxen)" b\\
"zbox " "Mehrere Boxen überlappt" b\\
"grid " "Frei plazierbare Widgets (TBD)" b\\
"slider " "Horizontale und vertikale Slider (zusammengesetztes Objekt)" b\\
\skip
"Für Tabellen gibt es einen Hilfs–Glue, und formatierte Absätze sind auch geplant" \\
}}vt
{{ glue*l }}glue
{{ $0000007F to x-color \tiny l"  Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. " }}i18n-text \bold "Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit," }}text \regular " sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui " }}text \italic "dolorem ipsum quia dolor sit amet," }}text \regular " consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum " }}text \bold-italic "qui dolorem eum fugiat" }}text \regular " quo voluptas nulla pariatur?" }}text glue*l }}glue }}p cbl dpy-w @ 72% fm* dup .par-split /center
glue*l }}glue }}v
}}v box[] >bdr
}}z box[] /flip dup >slides

\ page 8
{{
$FFFFBFFF pres-frame
{{
"ΜΙΝΩΣ2 Displays" /title
"Rendern in verschiedene Arten von Displays" \\
\skip
vt{{
"viewport " "In eine Textur, genutzt als Viewport" b\\
"display " "Zum tatsächlichen Display" b\\
}}vt
glue*l }}glue
}}v box[] >bdr
}}z box[] /flip dup >slides

\ page 9
{{
$BFDFFFFF pres-frame
{{
"Draw–Calls minimieren" /title
"OpenGL möchte so wenig wie mögliche Draw–Calls pro Frame, also werden ver­schie­dene Contexte mit einem Draw–Call pro Stack gezeichnet" p\\
\skip
vt{{
"init " "Initialisierungs–Runde" b\\
"bg " "Hintergrund–Runde" b\\
"icon " "Zeichne Elemente der Icon–Textur" b\\
"thumbnail " "Zeichne Elemente der Thumbnail–Textur" b\\
"image " "Zeichne Bilder mit einem Draw–Call pro Image" b\\
"marking " "Cursor/Auswahl–Runde" b\\
"text " "Text–Runde" b\\
}}vt
glue*l }}glue
}}v box[] >bdr
}}z box[] /flip dup >slides

\ page 11
{{
$FFFFFFFF pres-frame
{{
"Literatur & Links" /title
vt{{
"Bernd Paysan " "net2o fossil repository" bi\\
"" "https://fossil.net2o.de/net2o/" bm\\
"Bernd Paysan " "$quid cryptocurrency & SwapDragonChain" bi\\
"" "https://squid.cash/" bm\\
}}vt
glue*l }}glue
\ tex: qr-code
\ ' qr-code "qr-code.png" 13e }}image-file drop /center
\ qr-code nearest
\ glue*l }}glue
}}v box[] >bdr
}}z box[] /flip dup >slides

\ end
glue-right }}glue
}}h box[]
{{
' minos2 minos2-glue  logo-img to m2-img
}}z
}}z slide[]
to top-widget

also opengl

: !widgets ( -- ) top-widget .htop-resize
    1e ambient% sf! set-uniforms ;

previous

also [IFDEF] android android [THEN]

: presentation ( -- )
    1config
    [IFDEF] hidestatus hidekb hidestatus [THEN]
    !widgets widgets-loop ;

previous

script? [IF]
    next-arg s" time" str= [IF]  +db time( \ ) [THEN]
    presentation bye
[ELSE]
    presentation
[THEN]
