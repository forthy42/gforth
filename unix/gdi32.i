// this file is in the public domain
%module gdi32
%insert("include")
%{
#include <w32api/wtypes.h>
#include <w32api/wingdi.h>
%}
#define WINAPI
#define WINAPI_FAMILY_PARTITION(x) x
#define __MINGW_TYPEDEF_AW(x)
#define WINAPI_PARTITION_DESKTOP 1
#define UNALIGNED
#define CONST
#define CALLBACK
#define DECLSPEC_IMPORT

%apply unsigned short { WORD };
%apply int { WINBOOL, INT };
%apply unsigned int { UINT, COLORREF, DWORD, BLENDFUNCTION };
%apply long { LONG, LPARAM };
%apply unsigned long { ULONG };
%apply float { FLOAT };
%apply void { VOID };
%apply SWIGTYPE * { HDC, HGLRC, LPCSTR, LPWSTR, LPSTR, HPALETTE,
     LPVOID, LPDWORD, HCOLORSPACE, LPLOGCOLORSPACEW, LPLOGCOLORSPACEA,
     HGDIOBJ, LPPOINT, HBITMAP, LPSIZE, LPCWSTR, HRGN, HANDLE, PFLOAT,
     LPXFORM, LPTEXTMETRICA, HPEN, PROC, LPTEXTMETRICW, HENHMETAFILE,
     LPBYTE, HENHMETAFILE, LPPALETTEENTRY, LPHANDLETABLE, LPENHMETAHEADER,
     HMETAFILE, LPMETARECORD, LPHANDLETABLE, PVOID, PTRIVERTEX,
     LPINT, LPWORD, HFONT, LPRECT, LPRGNDATA, LPBITMAPINFO,
     LPLOGFONTW, HBRUSH, LPLOGFONTA, LPDEVMODE, HWND, HMODULE, HGLOBAL,
     LPPIXELFORMATDESCRIPTOR };

// exec: sed -e 's/^c-library/cs-vocabulary gdi32\nget-current >r also gdi32 definitions\n\nc-library/g' -e 's/^end-c-library/end-c-library\nprevious r> set-current/g' -e 's/add-lib/add-lib\ns" opengl32" add-lib\ns" msimg32" add-lib\ns" winspool" add-lib\n/g' -e 's/AlphaBlend a n n n n a n n n n u/&{*(BLENDFUNCTION*)\&}/' -e 's/c-function .*A /\\ &/' -e 's/\(c-function [^ ]*\)W /\1 /g'

%include <w32api/wingdi.h>
