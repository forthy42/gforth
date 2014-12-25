\ sensor example

require jni-location.fs

also jni also android

: .sensor-at ( -- ) 0 sensor >o se-sensor >o getType ref> o> at-xy
    .sensor ;

: .location-at ( -- )  0 15 at-xy .location ;

:noname  to sensor .sensor-at ; is android-sensor
:noname  to location .location-at ; is android-location

: +sensors ( -- )  clazz >o sensorManager >o TYPE_ALL getSensorList >o
    [: getType 1000000 start-sensor 10 ms ;] o l-map ref> ref> o> ;

page start-gps +sensors

previous previous