\ freetype GL example

require gl-helper.fs
ctx 0= [IF] window-init [THEN]

require ftgl-helper.fs

\ Demo Toplevel

also freetype-gl
also opengl
also [IFDEF] android android [THEN]

48e FConstant fontsize#
atlas [IFDEF] android  "/system/fonts/DroidSans.ttf\0" drop
[ELSE] "/usr/share/fonts/truetype/LiberationSans-Regular.ttf\0" drop 
[THEN]
fontsize# texture_font_new_from_file Value font1

atlas [IFDEF] android  "/system/fonts/DroidSansFallback.ttf\0" drop
[ELSE] "/usr/share/fonts/truetype/gkai00mp.ttf\0" drop
[THEN]
fontsize# texture_font_new_from_file Value font2

Variable text1$ "Dös isch a Tägscht." text1$ $!
Variable text2$ "这是一个文本：在德语说" text2$ $!

: glyph-draw ( -- )
    0.01e 0.02e 0.15e 1.0e glClearColor
    clear
    Ambient 1 ambient% glUniform1fv
    <render
    0e penxy sf!  -20e penxy sfloat+ sf!
    font1 to font  text1$ $@ render-string
    -100e penxy sf! -80e penxy sfloat+ sf!
    font2 to font  text2$ $@ render-string
    all-glyphs
    render> sync ;

: glyph-demo ( -- )  program init [IFDEF] hidekb  hidekb [THEN]
    1 level# +!  BEGIN  glyph-draw  >looper  level# @ 0= UNTIL ;

previous previous previous