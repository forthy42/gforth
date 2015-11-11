// this file is in the public domain
%module wingdi
%insert("include")
%{
#include <w32api/minwindef.h>
#include <w32api/windef.h>
#include <w32api/winuser.h>
%}
#define WINAPI
#define WINAPI_FAMILY_PARTITION(x) x
#define __MINGW_TYPEDEF_AW(x) typedef x ## W x;
#define __MSABI_LONG(x) x
#define WINAPI_PARTITION_DESKTOP 1
#define WINAPI_PARTITION_APP 1
#define UNALIGNED
#define CONST
#define CALLBACK
#define DECLSPEC_IMPORT
#define WINUSERAPI
#define __C89_NAMELESS
#define __LONG32 int
#define __cdecl
#define LONG long
#define SHORT short

#define tagRECT
#define _RECTL
#define tagPOINT
#define _POINTL
#define tagSIZE
#define tagPOINTS
#define tagCBT_CREATEWNDA
#define tagCBT_CREATEWNDW
#define tagCBTACTIVATESTRUCT
#define tagWTSSESSION_NOTIFICATION
#define tagCWPSTRUCT
#define tagCWPRETSTRUCT
#define tagKBDLLHOOKSTRUCT
#define tagMSLLHOOKSTRUCT
#define tagDEBUGHOOKINFO
#define tagMOUSEHOOKSTRUCT
#define tagMOUSEHOOKSTRUCTEX
#define tagHARDWAREHOOKSTRUCT
#define tagMOUSEMOVEPOINT
#define tagUSEROBJECTFLAGS
#define tagWNDCLASSEXA
#define tagWNDCLASSEXW
#define tagWNDCLASSA
#define tagWNDCLASSW
#define tagMSG
#define tagMINMAXINFO
#define tagCOPYDATASTRUCT
#define tagMDINEXTMENU
#define tagWINDOWPOS
#define tagNCCALCSIZE_PARAMS
#define tagTRACKMOUSEEVENT
#define tagACCEL
#define tagPAINTSTRUCT
#define tagWINDOWPLACEMENT
#define tagNMHDR
#define tagSTYLESTRUCT
#define tagMEASUREITEMSTRUCT
#define tagDRAWITEMSTRUCT
#define tagDELETEITEMSTRUCT
#define tagCOMPAREITEMSTRUCT
#define tagUPDATELAYEREDWINDOWINFO
#define tagMOUSEINPUT
#define tagKEYBDINPUT
#define tagHARDWAREINPUT
#define tagINPUT
#define tagTOUCHINPUT
#define tagPOINTER_INFO
#define tagPOINTER_TOUCH_INFO
#define tagPOINTER_PEN_INFO
#define tagTOUCH_HIT_TESTING_PROXIMITY_EVALUATION
#define tagTOUCH_HIT_TESTING_INPUT
#define tagLASTINPUTINFO
#define tagTPMPARAMS
#define tagMENUINFO
#define tagMENUGETOBJECTINFO
#define tagMENUITEMINFOA
#define tagMENUITEMINFOW
#define tagDROPSTRUCT
#define tagDRAWTEXTPARAMS
#define tagHELPINFO
#define tagMSGBOXPARAMSA
#define tagMSGBOXPARAMSW
#define tagCURSORSHAPE
#define tagSCROLLINFO
#define tagMDICREATESTRUCTA
#define tagMDICREATESTRUCTW
#define tagCLIENTCREATESTRUCT
#define tagMULTIKEYHELPA
#define tagMULTIKEYHELPW
#define tagHELPWININFOA
#define tagHELPWININFOW
#define tagTouchPredictionParameters
#define tagNONCLIENTMETRICSA
#define tagNONCLIENTMETRICSW
#define tagMINIMIZEDMETRICS
#define tagICONMETRICSA
#define tagICONMETRICSW
#define tagANIMATIONINFO
#define tagSERIALKEYSA
#define tagSERIALKEYSW
#define tagHIGHCONTRASTA
#define tagHIGHCONTRASTW
#define tagFILTERKEYS
#define tagSTICKYKEYS
#define tagMOUSEKEYS
#define tagACCESSTIMEOUT
#define tagSOUNDSENTRYA
#define tagSOUNDSENTRYW
#define tagTOGGLEKEYS
#define tagMONITORINFO
#define tagAUDIODESCRIPTION
#define tagMONITORINFOEXA
#define tagMONITORINFOEXW
#define tagGUITHREADINFO
#define tagCURSORINFO
#define tagWINDOWINFO
#define tagTITLEBARINFO
#define tagTITLEBARINFOEX
#define tagSCROLLBARINFO
#define tagCOMBOBOXINFO
#define tagALTTABINFO
#define tagRAWINPUTHEADER
#define tagRAWMOUSE
#define tagRAWKEYBOARD
#define tagRAWHID
#define tagRAWINPUT
#define tagRID_DEVICE_INFO_MOUSE
#define tagRID_DEVICE_INFO_KEYBOARD
#define tagRID_DEVICE_INFO_HID
#define tagRID_DEVICE_INFO
#define tagRAWINPUTDEVICE
#define tagRAWINPUTDEVICELIST
#define tagPOINTER_DEVICE_INFO
#define tagPOINTER_DEVICE_PROPERTY
#define tagPOINTER_DEVICE_CURSOR_INFO
#define tagCHANGEFILTERSTRUCT
#define tagGESTUREINFO
#define tagGESTURENOTIFYSTRUCT
#define tagGESTURECONFIG
#define tagINPUT_MESSAGE_SOURCE
#define tagINPUT_TRANSFORM
#define DUMMYUNIONNAME

%apply unsigned char { BYTE, CHAR };
%apply unsigned short { WPARAM, WCHAR, ATOM };
%apply int { WINBOOL, BOOLEAN, INT_PTR };
%apply unsigned int { UINT, COLORREF, DWORD, BLENDFUNCTION, ACCESS_MASK,
     UINT_PTR };
%apply long { LPARAM, LRESULT, __LONG32, ULONG_PTR };
%apply unsigned long { ULONG };
%apply float { FLOAT };
%apply void { VOID };
%apply const char * { LPCSTR };
%apply const wchar_t * { LPCWSTR };
%apply SWIGTYPE * { HDC, HGLRC, LPWSTR, LPSTR, HPALETTE,
     LPVOID, LPDWORD, HCOLORSPACE, LPLOGCOLORSPACEW, LPLOGCOLORSPACEA,
     HGDIOBJ, LPPOINT, HBITMAP, LPSIZE, HRGN, HANDLE, PFLOAT,
     LPXFORM, LPTEXTMETRICA, HPEN, PROC, LPTEXTMETRICW, HENHMETAFILE,
     LPBYTE, HENHMETAFILE, LPPALETTEENTRY, LPHANDLETABLE, LPENHMETAHEADER,
     HMETAFILE, LPMETARECORD, LPHANDLETABLE, PVOID, PTRIVERTEX,
     LPINT, LPWORD, HFONT, LPRECT, LPRGNDATA, LPBITMAPINFO,
     LPLOGFONTW, HBRUSH, LPLOGFONTA, LPDEVMODE, HWND, HMODULE, HGLOBAL,
     LPPIXELFORMATDESCRIPTOR, PUINT, HRAWINPUT, HWINEVENTHOOK, HMONITOR,
     LPMSG, HINSTANCE, LPCRECT, HICON, PBYTE, HCURSOR, HMENU,
     HHOOK, HOOKPROC, PROPENUMPROCA, WNDENUMPROC, PROPENUMPROCEXW,
     GRAYSTRINGPROC, HACCEL, TIMERPROC, HKL, PROPENUMPROCW,
     PROPENUMPROCEXA, LPCDLGTEMPLATEW, DLGPROC, LPCDLGTEMPLATEA,
     DRAWSTATEPROC, FARPROC, SENDASYNCPROC, PDWORD_PTR, PSECURITY_DESCRIPTOR,
     PSECURITY_INFORMATION, HWINSTA, LPSECURITY_ATTRIBUTES, HDESK, va_list };

// exec: sed -e 's/c-function \(.*Shutdown\|DisableProcessWindowsGhosting\|IsWow64Message\|GetWindowRgnBox\|RegisterShellHookWindow\)/\\ &/' -e 's/c-function .*A /\\ &/' -e 's/\(c-function [^ ]*\)W /\1 /g'

%include <w32api/minwindef.h>
%include <w32api/windef.h>
%include <w32api/winuser.h>

