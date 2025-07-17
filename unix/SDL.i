// this file is in the public domain
%module SDL
%insert("include")
%{
#include <SDL2/SDL.h>
#ifdef __gnu_linux__
#undef stderr
extern struct _IO_FILE *stderr;
#endif
%}

// exec: sed -e 's/^c-library\( .*\)/cs-vocabulary SDL2\nget-current >r also SDL2 definitions\n\nc-library\1\ns" a a 0" vararg$ $!/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g' -e 's/s" SDL" add-lib/s" SDL2" add-lib/g'
// prep: sed -e 's/swigFunctionPointer.*{((\([^*]*\)\*)ptr)->\([^}]*\)}.*/if(offsetof(\1, \2) >= 0) \0/g'

%apply unsigned int { Uint32, size_t, Uint16, Uint8, SDL_BlendMode };
%apply unsigned long long { Uint64 };
%apply long long { Sint64 };
%apply int { SDL_SensorID, Sint32, Sint16 };
%apply SWIGTYPE * { SDL_JoystickGUID, va_list };

#define DECLSPEC
#define SDLCALL
#define SDL_INLINE inline
#define SDL_FORCE_INLINE static inline
#define SDL_DEPRECATED

%include <SDL2/SDL_stdinc.h>
#undef SDL_COMPILE_TIME_ASSERT
#define SDL_COMPILE_TIME_ASSERT(x, y)
%include <SDL2/SDL_assert.h>
%include <SDL2/SDL_atomic.h>
%include <SDL2/SDL_audio.h>
%include <SDL2/SDL_clipboard.h>
%include <SDL2/SDL_cpuinfo.h>
%include <SDL2/SDL_endian.h>
%include <SDL2/SDL_error.h>
%include <SDL2/SDL_events.h>
%include <SDL2/SDL_filesystem.h>
 // %include <SDL2/SDL_gamecontroller.h>
%include <SDL2/SDL_haptic.h>
 // %include <SDL2/SDL_hidapi.h>
%include <SDL2/SDL_hints.h>
 // %include <SDL2/SDL_joystick.h>
%include <SDL2/SDL_loadso.h>
%include <SDL2/SDL_log.h>
%include <SDL2/SDL_messagebox.h>
%include <SDL2/SDL_metal.h>
%include <SDL2/SDL_mutex.h>
%include <SDL2/SDL_power.h>
%include <SDL2/SDL_render.h>
%include <SDL2/SDL_rwops.h>
%include <SDL2/SDL_sensor.h>
%include <SDL2/SDL_shape.h>
%include <SDL2/SDL_system.h>
%include <SDL2/SDL_thread.h>
%include <SDL2/SDL_timer.h>
%include <SDL2/SDL_version.h>
%include <SDL2/SDL_video.h>
%include <SDL2/SDL_locale.h>
%include <SDL2/SDL_misc.h>
