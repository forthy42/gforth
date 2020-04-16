\ Pulse audio driver

\ Authors: Bernd Paysan
\ Copyright (C) 2020 Free Software Foundation, Inc.

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

require unix/pulse.fs
require unix/pthread.fs

pulse also

debug: pulse( \ )

0 Value pa-ml
0 Value pa-api
0 Value pa-ctx
0 Value pa-task
$Variable app-name
"Gforth" app-name $!

' execute pa_context_notify_cb_t: Constant pa-context-notify-cb
' execute pa_server_info_cb_t: Constant pa-server-info-cb
' execute pa_source_info_cb_t: Constant pa-source-info-cb
' execute pa_sink_info_cb_t: Constant pa-sink-info-cb
' execute pa_context_subscribe_cb_t: Constant pa-context-subscribe-cb
' execute pa_context_success_cb_t: Constant pa-context-success-cb

0 Value pa-ready
Variable inputs[]
Variable def-input$
Variable outputs[]
Variable def-output$

: c$@ ( addr -- addr u )
    @ cstring>sstring ;

: +input-list { ctx inputinfo eol -- }
    inputinfo IF
	inputinfo pa_source_info-name c$@ 
	inputinfo pa_source_info-index l@ inputs[] $[]!
	pulse(
	." Name: " inputinfo pa_source_info-name c$@ type cr
	." Desc: " inputinfo pa_source_info-description c$@ type cr
	." Index: " inputinfo pa_source_info-index l@ . cr )
    THEN ;
: +output-list { ctx outputinfo eol -- }
    outputinfo IF
	outputinfo pa_sink_info-name c$@
	outputinfo pa_sink_info-index l@ outputs[] $[]!
	pulse(
	." Name: " outputinfo pa_sink_info-name c$@ type cr
	." Desc: " outputinfo pa_sink_info-description c$@ type cr
	." Index: " outputinfo pa_sink_info-index l@ . cr )
    THEN ;
: +server-info { ctx serverinfo -- }
    serverinfo IF
	serverinfo pa_server_info-default_source_name c$@ def-input$ $!
	serverinfo pa_server_info-default_sink_name c$@   def-output$ $!
	pulse(
	." Host: " serverinfo pa_server_info-host_name c$@ type cr
	." User: " serverinfo pa_server_info-user_name c$@ type cr
	." Def.source: " serverinfo pa_server_info-default_source_name c$@ type cr
	." Def.sink: " serverinfo pa_server_info-default_sink_name c$@ type cr )
    THEN ;

: >input-list ( -- )
    pa-ctx pa-source-info-cb ['] +input-list
    pa_context_get_source_info_list drop ;
: >output-list ( -- )
    pa-ctx pa-sink-info-cb ['] +output-list
    pa_context_get_sink_info_list drop ;
: >server-info ( -- )
    pa-ctx pa-server-info-cb ['] +server-info
    pa_context_get_server_info drop ;

: pa-success { ctx success -- }
    pulse( ." success: " success . cr ) ;

: >subscribe ( -- )
    pa-ctx PA_SUBSCRIPTION_MASK_ALL pa-context-success-cb ['] pa-success
    pa_context_subscribe drop ;

: pa-notify-state { ctx -- }
    case  ctx pa_context_get_state
	PA_CONTEXT_FAILED       of  2 to pa-ready  endof
	PA_CONTEXT_TERMINATED   of  3 to pa-ready  endof
	PA_CONTEXT_READY        of  1 to pa-ready
	    >input-list >output-list >server-info >subscribe
	endof
    endcase ;

: pa-subscribe { ctx ev-t idx -- }
    case ev-t ." event type: " dup . cr PA_SUBSCRIPTION_EVENT_FACILITY_MASK and
	PA_SUBSCRIPTION_EVENT_SINK   of  >output-list  endof
	PA_SUBSCRIPTION_EVENT_SOURCE of  >input-list   endof
	PA_SUBSCRIPTION_EVENT_SERVER of  >server-info  endof
    endcase ;

: pulse-init ( -- )
    stacksize4 NewTask4 to pa-task
    pa-task activate   debug-out debug-vector !
    [:  pa_mainloop_new to pa-ml
	pa-ml pa_mainloop_get_api to pa-api
	pa-api pa_signal_init drop
	pa-api app-name $@ pa_context_new to pa-ctx
	pa-ctx 0 0 PA_CONTEXT_NOAUTOSPAWN 0 pa_context_connect drop
	pa-ctx pa-context-notify-cb ['] pa-notify-state pa_context_set_state_callback
	pa-ctx pa-context-subscribe-cb ['] pa-subscribe pa_context_set_subscribe_callback
	BEGIN
	    ?events { | w^ retval }
	    pa-ml 1 retval pa_mainloop_iterate drop
	AGAIN ;] catch DoError ;

event: :>exec ( xt -- ) execute ;
event: :>execq ( xt -- ) dup >r execute r> >addr free throw ;
: pulse-exec ( xt -- ) { xt }
    <event xt elit, :>exec  pa-task event>
    pa-ml pa_mainloop_wakeup ;
: pulse-execq ( xt -- ) { xt }
    <event xt elit, :>execq pa-task event>
    pa-ml pa_mainloop_wakeup ;

previous
