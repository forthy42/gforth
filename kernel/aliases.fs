\ run-time routine headers

\ Copyright (C) 1997 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

-2 Alias: :docol
-3 Alias: :docon
-4 Alias: :dovar
-5 Alias: :douser
-6 Alias: :dodefer
-7 Alias: :dofield
-8 Alias: :dodoes
-9 Alias: :doesjump
-10 alias noop
-11 alias lit
-12 alias execute
-13 alias perform

has? glocals [IF]
-14 alias branch-lp+!#

[THEN]
-15 alias branch
-16 alias ?branch

has? glocals [IF]
-17 alias ?branch-lp+!#

[THEN]

has? xconds [IF]
-18 alias ?dup-?branch
-19 alias ?dup-0=-?branch

[THEN]
-20 alias (next)

has? glocals [IF]
-21 alias (next)-lp+!#

[THEN]
-22 alias (loop)

has? glocals [IF]
-23 alias (loop)-lp+!#

[THEN]
-24 alias (+loop)

has? glocals [IF]
-25 alias (+loop)-lp+!#

[THEN]

has? xconds [IF]
-26 alias (-loop)

has? glocals [IF]
-27 alias (-loop)-lp+!#

[THEN]
-28 alias (s+loop)

has? glocals [IF]
-29 alias (s+loop)-lp+!#

[THEN]

[THEN]
-30 alias unloop
-31 alias (for)
-32 alias (do)
-33 alias (?do)

has? xconds [IF]
-34 alias (+do)
-35 alias (u+do)
-36 alias (-do)
-37 alias (u-do)

[THEN]
-38 alias i
-39 alias i'
-40 alias j
-41 alias k
-42 alias move
-43 alias cmove
-44 alias cmove>
-45 alias fill
-46 alias compare
-47 alias -text
-48 alias toupper
-49 alias capscomp
-50 alias -trailing
-51 alias /string
-52 alias +
-53 alias under+
-54 alias -
-55 alias negate
-56 alias 1+
-57 alias 1-
-58 alias max
-59 alias min
-60 alias abs
-61 alias *
-62 alias /
-63 alias mod
-64 alias /mod
-65 alias 2*
-66 alias 2/
-67 alias fm/mod
-68 alias sm/rem
-69 alias m*
-70 alias um*
-71 alias um/mod
-72 alias m+
-73 alias d+
-74 alias d-
-75 alias dnegate
-76 alias d2*
-77 alias d2/
-78 alias and
-79 alias or
-80 alias xor
-81 alias invert
-82 alias rshift
-83 alias lshift
-84 alias 0=
-85 alias 0<>
-86 alias 0<
-87 alias 0>
-88 alias 0<=
-89 alias 0>=
-90 alias =
-91 alias <>
-92 alias <
-93 alias >
-94 alias <=
-95 alias >=
-96 alias u=
-97 alias u<>
-98 alias u<
-99 alias u>
-100 alias u<=
-101 alias u>=

has? dcomps [IF]
-102 alias d=
-103 alias d<>
-104 alias d<
-105 alias d>
-106 alias d<=
-107 alias d>=
-108 alias d0=
-109 alias d0<>
-110 alias d0<
-111 alias d0>
-112 alias d0<=
-113 alias d0>=
-114 alias du=
-115 alias du<>
-116 alias du<
-117 alias du>
-118 alias du<=
-119 alias du>=

[THEN]
-120 alias within
-121 alias sp@
-122 alias sp!
-123 alias rp@
-124 alias rp!

has? floating [IF]
-125 alias fp@
-126 alias fp!

[THEN]
-127 alias ;s
-128 alias >r
-129 alias r>
-130 alias rdrop
-131 alias 2>r
-132 alias 2r>
-133 alias 2r@
-134 alias 2rdrop
-135 alias over
-136 alias drop
-137 alias swap
-138 alias dup
-139 alias rot
-140 alias -rot
-141 alias nip
-142 alias tuck
-143 alias ?dup
-144 alias pick
-145 alias 2drop
-146 alias 2dup
-147 alias 2over
-148 alias 2swap
-149 alias 2rot
-150 alias 2nip
-151 alias 2tuck
-152 alias @
-153 alias !
-154 alias +!
-155 alias c@
-156 alias c!
-157 alias 2!
-158 alias 2@
-159 alias cell+
-160 alias cells
-161 alias char+
-162 alias (chars)
-163 alias count
-164 alias (f83find)

has? hash [IF]
-165 alias (hashfind)
-166 alias (tablefind)
-167 alias (hashkey)
-168 alias (hashkey1)

[THEN]
-169 alias (parse-white)
-170 alias aligned
-171 alias faligned
-172 alias >body

has? standard-threading [IF]
-173 alias >code-address
-174 alias >does-code
-175 alias code-address!
-176 alias does-code!
-177 alias does-handler!
-178 alias /does-handler
-179 alias threading-method

[THEN]

has? os [IF]
-180 alias (key)
-181 alias key?
-182 alias stdout
-183 alias stderr
-184 alias form
-185 alias flush-icache
-186 alias (bye)
-187 alias (system)
-188 alias getenv
-189 alias open-pipe
-190 alias close-pipe
-191 alias time&date
-192 alias ms
-193 alias allocate
-194 alias free
-195 alias resize
-196 alias strerror
-197 alias strsignal
-198 alias call-c

[THEN] ( has? os ) has? file [IF]
-199 alias close-file
-200 alias open-file
-201 alias create-file
-202 alias delete-file
-203 alias rename-file
-204 alias file-position
-205 alias reposition-file
-206 alias file-size
-207 alias resize-file
-208 alias read-file
-209 alias read-line

[THEN]  has? file [IF] -1 [ELSE] has? os [THEN] [IF]
-210 alias write-file
-211 alias emit-file

[THEN]  has? file [IF]
-212 alias flush-file
-213 alias file-status

[THEN] ( has? file ) has? floating [IF]
-214 alias f=
-215 alias f<>
-216 alias f<
-217 alias f>
-218 alias f<=
-219 alias f>=
-220 alias f0=
-221 alias f0<>
-222 alias f0<
-223 alias f0>
-224 alias f0<=
-225 alias f0>=
-226 alias d>f
-227 alias f>d
-228 alias f!
-229 alias f@
-230 alias df@
-231 alias df!
-232 alias sf@
-233 alias sf!
-234 alias f+
-235 alias f-
-236 alias f*
-237 alias f/
-238 alias f**
-239 alias fnegate
-240 alias fdrop
-241 alias fdup
-242 alias fswap
-243 alias fover
-244 alias frot
-245 alias fnip
-246 alias ftuck
-247 alias float+
-248 alias floats
-249 alias floor
-250 alias fround
-251 alias fmax
-252 alias fmin
-253 alias represent
-254 alias >float
-255 alias fabs
-256 alias facos
-257 alias fasin
-258 alias fatan
-259 alias fatan2
-260 alias fcos
-261 alias fexp
-262 alias fexpm1
-263 alias fln
-264 alias flnp1
-265 alias flog
-266 alias falog
-267 alias fsin
-268 alias fsincos
-269 alias fsqrt
-270 alias ftan
-271 alias fsinh
-272 alias fcosh
-273 alias ftanh
-274 alias fasinh
-275 alias facosh
-276 alias fatanh
-277 alias sfloats
-278 alias dfloats
-279 alias sfaligned
-280 alias dfaligned

[THEN] ( has? floats ) has? glocals [IF]
-281 alias @local#
-282 alias @local0
-283 alias @local1
-284 alias @local2
-285 alias @local3

has? floating [IF]
-286 alias f@local#
-287 alias f@local0
-288 alias f@local1

[THEN]
-289 alias laddr#
-290 alias lp+!#
-291 alias lp-
-292 alias lp+
-293 alias lp+2
-294 alias lp!
-295 alias >l

has? floating [IF]
-296 alias f>l

[THEN]  [THEN] \ has? glocals

has? OS [IF]
-297 alias open-lib
-298 alias lib-sym
-299 alias icall0
-300 alias icall1
-301 alias icall2
-302 alias icall3
-303 alias icall4
-304 alias icall5
-305 alias icall6
-306 alias icall20
-307 alias fcall0
-308 alias fcall1
-309 alias fcall2
-310 alias fcall3
-311 alias fcall4
-312 alias fcall5
-313 alias fcall6
-314 alias fcall20

[THEN] \ has? OS
-315 alias up!
