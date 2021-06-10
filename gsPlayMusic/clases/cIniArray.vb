'------------------------------------------------------------------------------
' Clase para manejar ficheros INIs
' Permite leer secciones enteras y todas las secciones de un fichero INI
'
' Últimas revisiones:   04/Abr/01, 23/Abr/02, 20/Jun/02
' Revisado para usar vb a las funciones propias de Visual Basic     (18/Abr/07)
'   y algunos cambios menores.
'
' ©Guillermo 'guille' Som, 1997-2001, 2007
'------------------------------------------------------------------------------
Option Strict On
'Option Explicit On 

Imports System
'Imports Microsoft.VisualBasic
Imports vb = Microsoft.VisualBasic
'Imports System.Windows.Forms

'<System.Runtime.InteropServices.ProgId("cIniArray_NET.cIniArray")>
Public Class cIniArray

    Private sBuffer As String ' Para usarla en las funciones GetSection(s)

    '--- Declaraciones para leer ficheros INI ---
    ' Leer todas las secciones de un fichero INI, esto seguramente no funciona en Win95
    ' Esta función no estaba en las declaraciones del API que se incluye con el VB
    Private Declare Function GetPrivateProfileSectionNames Lib "kernel32" Alias "GetPrivateProfileSectionNamesA" _
                (ByVal lpszReturnBuffer As String, ByVal nSize As Integer, _
                ByVal lpFileName As String) As Integer

    ' Leer una sección completa
    Private Declare Function GetPrivateProfileSection Lib "kernel32" Alias "GetPrivateProfileSectionA" _
                (ByVal lpAppName As String, ByVal lpReturnedString As String, _
                ByVal nSize As Integer, ByVal lpFileName As String) As Integer

    ' Leer una clave de un fichero INI
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" _
                (ByVal lpApplicationName As String, ByVal lpKeyName As String, _
                ByVal lpDefault As String, ByVal lpReturnedString As String, _
                ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" _
                (ByVal lpApplicationName As String, ByVal lpKeyName As Integer, _
                ByVal lpDefault As String, ByVal lpReturnedString As String, _
                ByVal nSize As Integer, ByVal lpFileName As String) As Integer

    ' Escribir una clave de un fichero INI (también para borrar claves y secciones)
    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" _
                (ByVal lpApplicationName As String, ByVal lpKeyName As String, _
                ByVal lpString As String, ByVal lpFileName As String) As Integer
    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" _
                (ByVal lpApplicationName As String, ByVal lpKeyName As String, _
                ByVal lpString As Integer, ByVal lpFileName As String) As Integer
    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" _
                (ByVal lpApplicationName As String, ByVal lpKeyName As Integer, _
                ByVal lpString As Integer, ByVal lpFileName As String) As Integer

    Public Sub IniDeleteKey(ByVal sIniFile As String, ByVal sSection As String, _
                Optional ByVal sKey As String = "")
        '--------------------------------------------------------------------------
        ' Borrar una clave o entrada de un fichero INI                  (16/Feb/99)
        ' Si no se indica sKey, se borrará la sección indicada en sSection
        ' En otro caso, se supone que es la entrada (clave) lo que se quiere borrar
        '
        ' Para borrar una sección se debería usar IniDeleteSection
        '
        If vb.Len(sKey) = 0 Then
            ' Borrar una sección
            Call WritePrivateProfileString(sSection, 0, 0, sIniFile)
        Else
            ' Borrar una entrada
            Call WritePrivateProfileString(sSection, sKey, 0, sIniFile)
        End If
    End Sub

    Public Sub IniDeleteSection(ByVal sIniFile As String, ByVal sSection As String)
        '--------------------------------------------------------------------------
        ' Borrar una sección de un fichero INI                          (04/Abr/01)
        ' Borrar una sección
        Call WritePrivateProfileString(sSection, 0, 0, sIniFile)
    End Sub

    ''' <summary>
    ''' Devuelve el valor de la clave de un fichero INI.
    ''' Si la clave no existe, se devuelve una cadena vacía.
    ''' </summary>
    ''' <param name="sFileName">
    ''' El fichero INI
    ''' </param>
    ''' <param name="sSection">
    ''' La sección de la que se quiere leer
    ''' </param>
    ''' <param name="sKeyName">
    ''' La clave de la que se quiere el valor
    ''' </param>
    ''' <returns>
    ''' El valor de la clave o una cadena vacía
    ''' </returns>
    ''' <remarks></remarks>
    Public Function IniGet( _
                       ByVal sFileName As String, _
                       ByVal sSection As String, _
                       ByVal sKeyName As String) As String

        Return IniGet(sFileName, sSection, sKeyName, "")
    End Function

    ''' <summary>
    ''' Devuelve el valor de la clave de un fichero INI.
    ''' </summary>
    ''' <param name="sFileName">
    ''' El fichero INI
    ''' </param>
    ''' <param name="sSection">
    ''' La sección de la que se quiere leer
    ''' </param>
    ''' <param name="sKeyName">
    ''' La clave de la que se quiere el valor
    ''' </param>
    ''' <param name="sDefault">
    ''' Valor por defecto en caso de que no exista la clave
    ''' </param>
    ''' <returns>
    ''' El valor de la clave o el indicado en sDefault
    ''' </returns>
    ''' <remarks></remarks>
    Public Function IniGet( _
                           ByVal sFileName As String, _
                           ByVal sSection As String, _
                           ByVal sKeyName As String, _
                           ByVal sDefault As String) As String

        Dim ret As Integer
        Dim sRetVal As String
        '
        sRetVal = New String(vb.Chr(0), 255)
        '
        ret = GetPrivateProfileString( _
                sSection, sKeyName, sDefault, sRetVal, vb.Len(sRetVal), sFileName)
        If ret = 0 Then
            Return sDefault
        Else
            Return vb.Left(sRetVal, ret)
        End If
    End Function

    ''' <summary>
    ''' Devuelve un valor Boolean de la clave de un fichero INI.
    ''' Si el valor es 0 se devuelve False, en otro caso se devuelve True
    ''' </summary>
    ''' <param name="sFileName">
    ''' El fichero INI
    ''' </param>
    ''' <param name="sSection">
    ''' La sección de la que se quiere leer
    ''' </param>
    ''' <param name="sKeyName">
    ''' La clave de la que se quiere el valor
    ''' </param>
    ''' <param name="sDefault">
    ''' Valor por defecto en caso de que no exista la clave
    ''' </param>
    ''' <returns>
    ''' El valor de la clave o el indicado en sDefault
    ''' </returns>
    ''' <remarks></remarks>
    Public Function IniGet( _
                           ByVal sFileName As String, _
                           ByVal sSection As String, _
                           ByVal sKeyName As String, _
                           ByVal sDefault As Boolean) As Boolean

        Dim sRet As String
        If sDefault Then
            sRet = IniGet(sFileName, sSection, sKeyName, "1")
        Else
            sRet = IniGet(sFileName, sSection, sKeyName, "0")
        End If

        If sRet = "0" Then
            Return False
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' Devuelve un valor Boolean? de la clave de un fichero INI.
    ''' Si el valor es 0 se devuelve False, en otro caso se devuelve True
    ''' </summary>
    ''' <param name="sFileName">
    ''' El fichero INI
    ''' </param>
    ''' <param name="sSection">
    ''' La sección de la que se quiere leer
    ''' </param>
    ''' <param name="sKeyName">
    ''' La clave de la que se quiere el valor
    ''' </param>
    ''' <param name="sDefault">
    ''' Valor por defecto en caso de que no exista la clave
    ''' </param>
    ''' <returns>
    ''' El valor de la clave o el indicado en sDefault
    ''' </returns>
    ''' <remarks></remarks>
    Public Function IniGet( _
                           ByVal sFileName As String, _
                           ByVal sSection As String, _
                           ByVal sKeyName As String, _
                           ByVal sDefault As Boolean?) As Boolean?

        Dim sRet As String
        If sDefault Then
            sRet = IniGet(sFileName, sSection, sKeyName, "1")
        Else
            sRet = IniGet(sFileName, sSection, sKeyName, "0")
        End If

        If sRet = "0" Then
            Return False
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' Devuelve un valor entero de la clave de un fichero INI.
    ''' </summary>
    ''' <param name="sFileName">
    ''' El fichero INI
    ''' </param>
    ''' <param name="sSection">
    ''' La sección de la que se quiere leer
    ''' </param>
    ''' <param name="sKeyName">
    ''' La clave de la que se quiere el valor
    ''' </param>
    ''' <param name="sDefault">
    ''' Valor por defecto en caso de que no exista la clave
    ''' </param>
    ''' <returns>
    ''' El valor de la clave o el indicado en sDefault
    ''' </returns>
    ''' <remarks></remarks>
    Public Function IniGet( _
                           ByVal sFileName As String, _
                           ByVal sSection As String, _
                           ByVal sKeyName As String, _
                           ByVal sDefault As Integer) As Integer

        Return CInt(vb.Val(IniGet(sFileName, sSection, sKeyName, sDefault.ToString)))
    End Function


    ''' <summary>
    ''' Guarda los datos de configuración
    ''' </summary>
    ''' <param name="sFileName">
    ''' El fichero INI
    ''' </param>
    ''' <param name="sSection">
    ''' La sección de la que se quiere leer
    ''' </param>
    ''' <param name="sKeyName">
    ''' La clave de la que se quiere el valor
    ''' </param>
    ''' <param name="sValue">
    ''' Valor de tipo String a guardar
    ''' </param>
    ''' <remarks></remarks>
    Public Sub IniWrite( _
                        ByVal sFileName As String, _
                        ByVal sSection As String, _
                        ByVal sKeyName As String, _
                        ByVal sValue As String)

        Call WritePrivateProfileString(sSection, sKeyName, sValue, sFileName)
    End Sub

    ''' <summary>
    ''' Guarda los datos de configuración
    ''' </summary>
    ''' <param name="sFileName">
    ''' El fichero INI
    ''' </param>
    ''' <param name="sSection">
    ''' La sección de la que se quiere leer
    ''' </param>
    ''' <param name="sKeyName">
    ''' La clave de la que se quiere el valor
    ''' </param>
    ''' <param name="value">
    ''' El valor Boolean a guardar (se guarda 1 para True y 0 para False)
    ''' </param>
    ''' <remarks></remarks>
    Public Sub IniWrite( _
                        ByVal sFileName As String, _
                        ByVal sSection As String, _
                        ByVal sKeyName As String, _
                        ByVal value As Boolean)

        If value Then
            Call WritePrivateProfileString(sSection, sKeyName, "1", sFileName)
        Else
            Call WritePrivateProfileString(sSection, sKeyName, "0", sFileName)
        End If
    End Sub

    ''' <summary>
    ''' Guarda los datos de configuración
    ''' </summary>
    ''' <param name="sFileName">
    ''' El fichero INI
    ''' </param>
    ''' <param name="sSection">
    ''' La sección de la que se quiere leer
    ''' </param>
    ''' <param name="sKeyName">
    ''' La clave de la que se quiere el valor
    ''' </param>
    ''' <param name="value">
    ''' El valor Boolean? a guardar (se guarda 1 para True y 0 para False)
    ''' </param>
    ''' <remarks></remarks>
    Public Sub IniWrite( _
                        ByVal sFileName As String, _
                        ByVal sSection As String, _
                        ByVal sKeyName As String, _
                        ByVal value As Boolean?)

        IniWrite(sFileName, sSection, sKeyName, value.Value)
    End Sub

    ''' <summary>
    ''' Guarda los datos de configuración
    ''' </summary>
    ''' <param name="sFileName">
    ''' El fichero INI
    ''' </param>
    ''' <param name="sSection">
    ''' La sección de la que se quiere leer
    ''' </param>
    ''' <param name="sKeyName">
    ''' La clave de la que se quiere el valor
    ''' </param>
    ''' <param name="value">
    ''' Valor de tipo Integer a guardar
    ''' </param>
    ''' <remarks></remarks>
    Public Sub IniWrite( _
                        ByVal sFileName As String, _
                        ByVal sSection As String, _
                        ByVal sKeyName As String, _
                        ByVal value As Integer)

        Call WritePrivateProfileString(sSection, sKeyName, value.ToString, sFileName)
    End Sub


    Public Function IniGetSection(ByVal sFileName As String, _
                ByVal sSection As String) As String()
        '--------------------------------------------------------------------------
        ' Lee una sección entera de un fichero INI                      (27/Feb/99)
        ' Adaptada para devolver un array de string                     (04/Abr/01)
        '
        ' Esta función devolverá un array de índice cero
        ' con las claves y valores de la sección
        '
        ' Parámetros de entrada:
        '   sFileName   Nombre del fichero INI
        '   sSection    Nombre de la sección a leer
        ' Devuelve:
        '   Un array con el nombre de la clave y el valor
        '   Para leer los datos:
        '       For i = 0 To UBound(elArray) -1 Step 2
        '           sClave = elArray(i)
        '           sValor = elArray(i+1)
        '       Next
        '
        Dim i As Integer
        Dim j As Integer
        Dim sTmp As String
        Dim sClave As String
        Dim sValor As String
        '
        Dim aSeccion() As String
        Dim n As Integer
        '
        ReDim aSeccion(0)
        '
        ' El tamaño máximo para Windows 95
        sBuffer = New String(vb.Chr(0), 32767)
        '
        n = GetPrivateProfileSection(sSection, sBuffer, vb.Len(sBuffer), sFileName)
        '
        If n > 0 Then
            '
            ' Cortar la cadena al número de caracteres devueltos
            sBuffer = vb.Left(sBuffer, n)
            ' Quitar los vbNullChar extras del final
            i = vb.InStr(sBuffer, vb.vbNullChar & vb.vbNullChar)
            If i > 0 Then
                sBuffer = vb.Left(sBuffer, i - 1)
            End If
            '
            n = -1
            ' Cada una de las entradas estará separada por un Chr$(0)
            Do
                i = vb.InStr(sBuffer, vb.Chr(0))
                If i > 0 Then
                    sTmp = vb.LTrim(vb.Left(sBuffer, i - 1))
                    If vb.Len(sTmp) > 0 Then
                        ' Comprobar si tiene el signo igual
                        j = vb.InStr(sTmp, "=")
                        If j > 0 Then
                            sClave = vb.Left(sTmp, j - 1)
                            sValor = vb.LTrim(vb.Mid(sTmp, j + 1))
                            '
                            n = n + 2
                            ReDim Preserve aSeccion(n)
                            aSeccion(n - 1) = sClave
                            aSeccion(n) = sValor
                        End If
                    End If
                    sBuffer = vb.Mid(sBuffer, i + 1)
                End If
            Loop While i > 0
            If vb.Len(sBuffer) > 0 Then
                j = vb.InStr(sBuffer, "=")
                If j > 0 Then
                    sClave = vb.Left(sBuffer, j - 1)
                    sValor = vb.LTrim(vb.Mid(sBuffer, j + 1))
                    n = n + 2
                    ReDim Preserve aSeccion(n)
                    aSeccion(n - 1) = sClave
                    aSeccion(n) = sValor
                End If
            End If
        End If
        ' Devolver el array
        Return aSeccion
    End Function

    Public Function IniGetSections(ByVal sFileName As String) As String()
        '--------------------------------------------------------------------------
        ' Devuelve todas las secciones de un fichero INI                (27/Feb/99)
        ' Adaptada para devolver un array de string                     (04/Abr/01)
        '
        ' Esta función devolverá un array con todas las secciones del fichero
        '
        ' Parámetros de entrada:
        '   sFileName   Nombre del fichero INI
        ' Devuelve:
        '   Un array con todos los nombres de las secciones
        '   La primera sección estará en el elemento 1,
        '   por tanto, si el array contiene cero elementos es que no hay secciones
        '
        Dim i As Integer
        Dim sTmp As String
        Dim n As Integer
        Dim aSections() As String
        '
        ReDim aSections(0)
        '
        ' El tamaño máximo para Windows 95
        sBuffer = New String(vb.Chr(0), 32767)
        '
        ' Esta función del API no está definida en el fichero TXT
        n = GetPrivateProfileSectionNames(sBuffer, vb.Len(sBuffer), sFileName)
        '
        If n > 0 Then
            ' Cortar la cadena al número de caracteres devueltos
            sBuffer = vb.Left(sBuffer, n)
            ' Quitar los vbNullChar extras del final
            i = vb.InStr(sBuffer, vb.vbNullChar & vb.vbNullChar)
            If i > 0 Then
                sBuffer = vb.Left(sBuffer, i - 1)
            End If
            '
            n = 0
            ' Cada una de las entradas estará separada por un Chr$(0)
            Do
                i = vb.InStr(sBuffer, vb.Chr(0))
                If i > 0 Then
                    sTmp = vb.LTrim(vb.Left(sBuffer, i - 1))
                    If vb.Len(sTmp) > 0 Then
                        n = n + 1
                        ReDim Preserve aSections(n)
                        aSections(n) = sTmp
                    End If
                    sBuffer = vb.Mid(sBuffer, i + 1)
                End If
            Loop While i > 0
            If vb.Len(sBuffer) > 0 Then
                n = n + 1
                ReDim Preserve aSections(n)
                aSections(n) = sBuffer
            End If
        End If
        ' Devolver el array
        Return aSections
    End Function
    '
    ' Función shared (compartida) para usar sin crear un objeto     (20/Jun/02)
    ' Pongo las dos sobrecargas en vez de usar Optional             (18/Abr/07)
    Public Shared Function AppPath() As String
        Return AppPath(True)
    End Function

    Public Shared Function AppPath(conBackSlash As Boolean) As String
        ' Devuelve el path del ejecutable                               (23/Abr/02)
        ' con o sin la barra de directorios

        Dim s As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.Location) ' System.Windows.Forms.Application.StartupPath
        If conBackSlash Then
            If vb.Right(s, 1) <> "\" Then
                s = s & "\"
            End If
        Else
            If vb.Right(s, 1) = "\" Then
                s = vb.Left(s, vb.Len(s) - 1)
            End If
        End If

        Return s
    End Function
End Class
