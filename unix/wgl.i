// this file is in the public domain
%module wgl
%insert("include")
%{
#include <w32api/GL/gl.h>
#include <w32api/GL/glext.h>
#include <w32api/GL/wglext.h>
%}
#define WINAPI
#define WGL_WGLEXT_PROTOTYPES

%apply float { GLfloat };
%apply long long { GLint64, GLint64EXT, GLuint64, GLuint64EXT, INT64 };
%apply long { GLsizeiptr, GLintptr, GLsizeiptrARB, GLintptrARB, HDC, HANDLE, HPVIDEODEV, HPBUFFERARB, HVIDEOINPUTDEVICENV, GLsizei };
%apply int { BOOL, GLint, GLenum };
%apply unsigned int { UINT, GLuint };
%apply void * { LPVOID };

// exec: sed -e 's/\(c-function .*\(NV\|SUN\|IBM\|ATI\|AMD\|EXT\|SUN\|SGI\|MESA\|INTEL\|HP\|GREMEDY\|APPLE\|OES\|3DFX\|ARB\|INGR\)\)/\\ \1/g'

%include <w32api/GL/wglext.h>
