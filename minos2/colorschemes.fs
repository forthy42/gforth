\ colorschemes by Base24 (actual colors in termcolors.png)

\ This file contains the indices into the termcolors.png, based on the
\ filenames fo the actual themes.  Gforth uses a colormap texture, so the
\ actual colors are not in a text file.  Nonetheless, the actual artistic work
\ is in choosing the colors.  Using the texture compresses 1,3MB of text files
\ into 24kB of image data.

\ Copyright (c) 2020 Base24

\ MIT License

\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to
\ deal in the Software without restriction, including without limitation the
\ rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
\ sell copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:

\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.

\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
\ FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
\ IN THE SOFTWARE.

: enums... ( -- )  0 { n }
    BEGIN  refill  source nip 0<> and  WHILE  n Constant  1 +to n  REPEAT ;

get-current >r
Vocabulary base16
Vocabulary base24
also base16 definitions

enums...
3024
apathy
ashes
atelier-cave
atelier-cave-light
atelier-dune
atelier-dune-light
atelier-estuary
atelier-estuary-light
atelier-forest
atelier-forest-light
atelier-heath
atelier-heath-light
atelier-lakeside
atelier-lakeside-light
atelier-plateau
atelier-plateau-light
atelier-savanna
atelier-savanna-light
atelier-seaside
atelier-seaside-light
atelier-sulphurpool
atelier-sulphurpool-light
atlas
bespin
black-metal-bathory
black-metal-burzum
black-metal
black-metal-dark-funeral
black-metal-gorgoroth
black-metal-immortal
black-metal-khold
black-metal-marduk
black-metal-mayhem
black-metal-nile
black-metal-venom
brewer
bright
brogrammer
brushtrees
brushtrees-dark
chalk
circus
classic-dark
classic-light
codeschool
cupcake
cupertino
darktooth
decaf
default-dark
default-light
dracula
edge-dark
edge-light
eighties
embers
espresso
flat
framer
fruit-soda
gigavolt
github
google-dark
google-light
grayscale-dark
grayscale-light
greenscreen
gruvbox-dark-hard
gruvbox-dark-medium
gruvbox-dark-pale
gruvbox-dark-soft
gruvbox-light-hard
gruvbox-light-medium
gruvbox-light-soft
hardcore
harmonic-dark
harmonic-light
heetch
heetch-light
helios
hopscotch
horizon-dark
horizon-light
horizon-terminal-dark
horizon-terminal-light
ia-dark
ia-light
icy
irblack
isotope
macintosh
marrakesh
materia
material
material-darker
material-lighter
material-palenight
material-vivid
mellow-purple
mexico-light
mocha
monokai
nord
nova
ocean
oceanicnext
outrun-dark
papercolor-dark
papercolor-light
paraiso
phd
pico
pop
porple
railscasts
rebecca
sandcastle
seti
shapeshifter
snazzy
solarflare
solarized-dark
solarized-light
spacemacs
summerfruit-dark
summerfruit-light
synth-midnight-dark
tomorrow
tomorrow-night
tomorrow-night-eighties
tube
twilight
unikitty-dark
unikitty-light
woodland
xcode-dusk
zenburn

base24 definitions
enums...
3024-day
3024-night
adventuretime
alienblood
argonaut
arthur
ateliersulphurpool
ayu
ayu_light
banana-blueberry
batman
birdsofparadise
blazer
blueberrypie
blue-matrix
blulocodark
blulocolight
borland
breeze
broadcast
brogrammer
builtin-dark
builtin-light
builtin-pastel-dark
builtin-solarized-dark
builtin-solarized-light
builtin-tango-dark
builtin-tango-light
chalkboard
chalk
challengerdeep
ciapre
clrs
cobalt2
cobalt-neon
coffee_theme
crayonponyfish
cyberdyne
dark+
deep
desert
dimmedmonokai
dracula24
earthsong
elemental
elementary
encom
espresso
espresso-libre
fideloper
firefoxdev
fishtank
flat
flatland
floraverse
forestblue
framer
frontenddelight
funforrest
galaxy
github
grape
gruvbox-dark
hacktober
hardcore
highway
hipster-green
hivacruz
homebrew
hopscotch
hurtado
hybrid
ic_green_ppl
ic_orange_ppl
idea
idletoes
ir_black
jackie-brown
japanesque
jellybeans
jetbrains-darcula
kibble
lab-fox
laser
later-this-evening
lavandula
lovelace
man-page
material
materialdark
mathias
medallion
misterioso
molokai
monalisa
monokai-vivid
n0tch2k
nightlion-v1
nightlion-v2
night-owlish-light
nocturnal-winter
obsidian
ocean
oceanicmaterial
ollie
one-dark
onehalflight
one-light
operator-mono-dark
pandora
paulmillr
pencildark
pencillight
piatto-light
pnevma
pro
pro-light
purplepeter
purple-rain
rebecca
red-alert
red-planet
red-sands
rippedcasts
royal
scarlet-protocol
seafoam-pastel
seashells
shades-of-purple
shaman
slate
sleepyhollow
smyck
softserver
solarized-dark-higher-contrast
solarized-dark---patched
spacedust
spacegray-eighties
spacegray-eighties-dull
spiderman
square
sundried
synthwave-everything
tango-adapted
tango-half-adapted
terminal-basic
thayer-bright
the-hulk
toychest
treehouse
twilight
ubuntu
ultraviolent
underthesea
unikitty
urple
vaughn
vibrantink
violet-dark
violet-light
warmneon
wez
wildcherry
wombat
wryan
zenburn

previous r> set-current
