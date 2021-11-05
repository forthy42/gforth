Run gforth inside a container.

Get your container:

```shell
$ docker pull forthy42/gforth
```

Example session 

```shell
$ docker run -ti --rm forthy42/gforth
Gforth 0.7.9_20180905, Copyright (C) 1995-2017 Free Software Foundation, Inc.
Gforth comes with ABSOLUTELY NO WARRANTY; for details type `license'
Type `help' for basic help
words 
disasm disline disass .... (lots of words)...
perform execute call noop  ok
bye 
$
```

Use a directory mount to access files from a host directory
(current work directory in the example) inside the container

```shell
$ cat $(pwd)/test.fs
.( huhu )
$ docker run -ti --rm -v$(pwd):/work forthy42/gforth
s" /work/test.fs" included huhu ok
bye
$
```

Use the current user id (or any other numeric id) inside the container

```shell
$ id -u
1001
$ docker run -ti --rm --user $(id -u) forthy42/gforth
s" id -u" system uid=1001
 ok
bye
$
```

See https://neu.forth-ev.de/wiki/res/lib/exe/fetch.php/vd-archiv:4d2018-02.pdf
for more (in German)
