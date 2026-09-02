/*
   win2lin.h - declare the Windows typedef and constant in Linux
*/
#ifndef WIN2LIN_H_
#define WIN2LIN_H_

#include <stdlib.h>
#include <limits.h>

#if defined(_WINDOWS) || defined(_WIN32_WCE)
#include <windows.h>
#endif

#if defined __cplusplus
namespace Tech5_core {
#endif

#ifndef maxAB
#define maxAB(a,b)            (((a) > (b)) ? (a) : (b))
#endif

#ifndef minAB
#define minAB(a,b)            (((a) < (b)) ? (a) : (b))
#endif

#if defined(__linux__) || defined (__ANDROID__) || defined(ANDROID) || defined (__APPLE__) || defined(__IOS__)
   #define P_PACKED_1  __attribute__((aligned(1),packed))
   #define P_PACKED_4  __attribute__((aligned(4),packed))
   #define P_PACKED_8  __attribute__((aligned(8),packed))
   #define P_PACKED_16 __attribute__((aligned(16),packed))
#else
   #define P_PACKED_1
   #define P_PACKED_4
   #define P_PACKED_8
   #define P_PACKED_16
#endif

#if defined(__linux__) || defined(__IOS__) || defined(__APPLE__)
   #define _stricmp strcasecmp
   #define _strnicmp strncasecmp
#endif

#if !defined(_WINDOWS) && !defined(_WIN32_WCE)
	typedef unsigned char	uint8_t;
	typedef unsigned char	BYTE;
	typedef unsigned short	WORD;
	typedef unsigned int    DWORD;
	typedef unsigned int    UINT;
   typedef int             LONG;
   #ifndef NULL
      #define NULL 0
   #endif
   #define __cdecl   

   #define MAX_PATH            512
   #define _MAX_PATH           512   
//   #define INT_MIN     (-2147483647 - 1) /* minimum (signed) int value */
//   #define INT_MAX       2147483647    /* maximum (signed) int value */



   #define CONST           const

   typedef void*           HANDLE;
   typedef char            CHAR;
   typedef CHAR            *NPSTR, *LPSTR, *PSTR;
   typedef CONST           CHAR *LPCSTR, *PCSTR;
   typedef void           *LPVOID;
   


#ifdef __cplusplus
//template <class T>
//static inline T	abs	( T val )		
//{
//	return (val > 0) ? val : -val;
//}
#endif // __cplusplus

#ifdef  UNICODE         

#define __T(x)      L ## x

#ifndef _TCHAR_DEFINED
typedef wchar_t WCHAR;    
typedef wchar_t  _TCHAR;
typedef WCHAR TCHAR, *PTCHAR;
typedef WCHAR TBYTE , *PTBYTE ;
#define _TCHAR_DEFINED
#endif /* !_TCHAR_DEFINED */

typedef WCHAR *PWCHAR, *LPWCH, *PWCH;
typedef __nullterminated WCHAR *NWPSTR, *LPWSTR, *PWSTR;
typedef __nullterminated CONST WCHAR *LPCWSTR, *PCWSTR;
typedef __nullterminated WCHAR UNALIGNED *LPUWSTR, *PUWSTR;
typedef __nullterminated CONST WCHAR UNALIGNED *LPCUWSTR, *PCUWSTR;
typedef LPWCH LPTCH, PTCH;
typedef LPWSTR PTSTR, LPTSTR;
typedef LPCWSTR PCTSTR, LPCTSTR;
typedef LPUWSTR PUTSTR, LPUTSTR;
typedef LPCUWSTR PCUTSTR, LPCUTSTR;
typedef LPWSTR LP;

#else   /* UNICODE */               

#define __T(x)      x

#ifndef _TCHAR_DEFINED
typedef char _TCHAR;
typedef char TCHAR, *PTCHAR;
typedef unsigned char TBYTE , *PTBYTE ;
#define _TCHAR_DEFINED
#endif /* !_TCHAR_DEFINED */

typedef CHAR *PCHAR, *LPCH, *PCH;
typedef CONST CHAR *LPCCH, *PCCH;

typedef LPCH LPTCH, PTCH;
typedef LPSTR PTSTR, LPTSTR, PUTSTR, LPUTSTR;
typedef LPCSTR PCTSTR, LPCTSTR, PCUTSTR, LPCUTSTR;
#define __TEXT(quote) quote         

#endif /* UNICODE */                



#ifdef UNICODE     
   #define _tcslen         wcslen  
   #define _tcscpy         wcscpy  
   #define _tcscpy_s       wcscpy_s  
   #define _tcsncpy        wcsncpy  
   #define _tcsncpy_s      wcsncpy_s  
   #define _tcscat         wcscat  
   #define _tcscat_s       wcscat_s  
   #define _tcsupr         wcsupr  
   #define _tcsupr_s       wcsupr_s  
   #define _tcslwr         wcslwr  
   #define _tcslwr_s       wcslwr_s    
   #define _stprintf_s     swprintf_s  
   #define _stprintf       swprintf  
   #define _tprintf        wprintf    
   #define _vstprintf_s    vswprintf_s  
   #define _vstprintf      vswprintf    
   #define _tscanf         wscanf      
   #define _tfopen         _wfopen
#else    
   #define _tcslen         strlen  
   #define _tcscpy         strcpy  
   #define _tcscpy_s       strcpy_s  
   #define _tcsncpy        strncpy  
   #define _tcsncpy_s      strncpy_s  
   #define _tcscat         strcat  
   #define _tcscat_s       strcat_s  
   #define _tcsupr         strupr  
   #define _tcsupr_s       strupr_s  
   #define _tcslwr         strlwr  
   #define _tcslwr_s       strlwr_s    
   #define _stprintf_s     sprintf_s  
   #define _stprintf       sprintf  
   #define _tprintf        printf    
   #define _vstprintf_s    vsprintf_s  
   #define _vstprintf      vsprintf    
   #define _tscanf         scanf    
   #define _tfopen         fopen
#endif  


#define _T(x)       __T(x)
#define _TEXT(x)    __T(x)


#endif // #if defined(_WINDOWS) || defined(_WIN32_WCE)
#if defined __cplusplus
} // namespace Tech5_core {
#endif // __cplusplus
#endif   // WIN2LIN_H_
