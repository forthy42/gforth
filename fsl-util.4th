\ -*-forth-*-

\ Gforth used to ship two copies of slightly different versions of fsl-util,
\ under the names fsl-util.4th and fsl-util.fs.  We Keep backwards
\ compatability but remove code duplication by just making this file include
\ the (newer) fsl-util.fs

\ we don't use REQUIRE so that semantics of 'include fsl-util.4th' don't
\ change (maybe some bloated piece of Forth software has multiple copies of
\ fsl-util loaded into distinct vocabularies)
include fsl-util.fs
