'------------------------------------------------------------------------------
' Clase para leer de un fichero y convertirlo en un array           (03/Abr/01)
'
' Esta clase permite leer de un fichero y devolver:
' -Devuelve una cadena con el contenido de un fichero,
'   cada linea separada con CrLf
' -Devuelve un array con el contenido de un fichero
' -Guarda el contenido de un array en un fichero
' -Guarda el contenido de una cadena en un fichero
' -Insertar una cadena en un array, tanto al principio como al final
' -Comprueba si existe un fichero
'
' Nuevos métodos:                                                   (25/Mar/02)
'   InsertAt
'   InsertArrayAt
'   FileExtension
'
' ©Guillermo 'guille' Som, 2001-2004, 2007
'------------------------------------------------------------------------------
Option Strict On
Option Explicit On 

Imports System
Imports Microsoft.VisualBasic
'Imports System.Windows.Forms

Public Class cFileToArray
    '
    Public Enum eFACase
        LowerCaseFA
        UpperCaseFA
        SameCaseFA
    End Enum
    Public Enum eFAInsertArray
        DesdeElUltimo = -2
        DespuesDelUltimo = -1
        OtrosValoresIndicarElIndice = 0
    End Enum
    '
    Public Function InsertAt(ByRef strArray() As String, ByVal sLine As String, ByVal nIndex As Integer) As String()
        ' Insertar la cadena indicada en la posición indicada           (25/Mar/02)
        Dim i As Integer
        Dim j As Integer
        Dim tmpArray() As String ' Array temporal
        '
        j = UBound(strArray)
        ReDim tmpArray(j + 1)
        For i = 0 To nIndex - 1
            tmpArray(i) = strArray(i)
        Next
        tmpArray(nIndex) = sLine
        For i = nIndex To j
            tmpArray(i + 1) = strArray(i)
        Next
        ' Copiar el array en el indicado, por si quiere usar como Sub
        strArray = tmpArray
        ' Devolver el array generado
        InsertAt = tmpArray
    End Function
    '
    Public Function InsertAtStart(ByRef strArray() As String, ByVal sLine As String) As String()
        ' Insertar la cadena indicada al principio del array
        Dim j As Integer
        Dim tmpArray() As String    ' Array temporal
        '
        j = strArray.Length
        ReDim tmpArray(j + 1)
        strArray.CopyTo(tmpArray, 1)
        tmpArray(0) = sLine
        ' Copiar el array en el indicado, por si quiere usar como Sub
        'ReDim Preserve StrArray(tmpArray.Length)
        strArray = tmpArray
        ' Devolver el array generado
        Return tmpArray
        '
        '' Insertar la cadena indicada al principio del array
        'Dim i As Integer
        'Dim j As Integer
        'Dim tmpArray() As String ' Array temporal
        ''
        'j = UBound(strArray)
        'ReDim tmpArray(j + 1)
        'For i = 0 To j
        '    tmpArray(i + 1) = strArray(i)
        'Next
        'tmpArray(0) = sLine
        '' Copiar el array en el indicado, por si quiere usar como Sub
        'strArray = tmpArray
        '' Devolver el array generado
        'InsertAtStart = tmpArray
    End Function
    '
    Public Function InsertAtEnd(ByRef strArray() As String, ByVal sLine As String) As String()
        ' Insertar la cadena indicada al final del array
        Dim j As Integer
        Dim tmpArray() As String
        '
        j = strArray.Length
        ReDim tmpArray(j + 1)
        strArray.CopyTo(tmpArray, 0)
        tmpArray(j) = sLine
        ' Copiar el array en el indicado, por si quiere usar como Sub
        'ReDim Preserve StrArray(tmpArray.Length)
        strArray = tmpArray
        ' Devolver el array generado
        Return tmpArray
        '
        '' Insertar la cadena indicada al final del array
        'Dim j As Integer
        'Dim tmpArray() As String
        ''
        'j = UBound(strArray)
        'tmpArray = strArray
        'ReDim Preserve tmpArray(j + 1)
        'tmpArray(j + 1) = sLine
        '' Copiar el array en el indicado, por si quiere usar como Sub
        'strArray = tmpArray
        '' Devolver el array generado
        'InsertAtEnd = tmpArray
    End Function
    '
    Public Function StringFromFile(ByVal FileName As String) As String
        ' Leer el fichero indicado y devolver una cadena con el contenido
        Dim s As String = ""
        '
        ' Si no se especifica el fichero, salir
        If FileName.Trim = "" Then
            Return ""
        End If
        '
        If FileExists(FileName) Then
            ' intentar leerlo con el formato que tenía              (25/Ene/04)
            'Dim sr As New IO.StreamReader(FileName, System.Text.Encoding.Default)
            Dim sr As New System.IO.StreamReader(FileName, FormatoFichero(FileName))
            s = sr.ReadToEnd
            sr.Close()
        End If
        ' Devolver la cadena
        Return s
    End Function
    '
    Public Function StringArrayFromFile(ByVal FileName As String) As String()
        ' Leer el fichero indicado y devolver un array con cada línea
        ' Leer el contenido del fichero y guardarlo en el array indicado
        Dim s As String
        '
        ' Si no se especifica el fichero, salir
        If FileName = Nothing Then
            Dim af(0) As String
            'ReDim af(0)
            Return af
            'Exit Function
        End If
        '
        ' Leemos el contenido del fichero
        s = StringFromFile(FileName)
        '
        ' Devolver el array
        'Return Split(s, vbCrLf)
        ' Eliminar las entradas vacias                  v2.0.0.0289 (24/Ago/07)
        Return s.Split(vbCrLf.ToCharArray, StringSplitOptions.RemoveEmptyEntries)
    End Function
    '
    Public Sub WriteStringToFile(ByVal fileName As String, ByVal sString As String)
        ' Guarda en FileName el contenido de la cadena
        '
        WriteStringToFile(fileName, sString, System.Text.Encoding.Default)
    End Sub
    '
    Public Sub WriteStringToFile(ByVal fileName As String, _
                                 ByVal sString As String, _
                                 ByVal formato As System.Text.Encoding)
        ' Guarda en FileName el contenido de la cadena              (25/Ene/04)
        ' usando el formato indicado
        '
        Try
            Dim sw As New System.IO.StreamWriter(fileName, False, formato)
            sw.WriteLine(sString)
            sw.Close()
        Catch
        End Try
    End Sub
    '
    Public Sub WriteArrayToFile(ByVal fileName As String, _
                                ByVal sFileArray() As String)
        WriteArrayToFile(fileName, sFileArray, System.Text.Encoding.Default)
    End Sub
    Public Sub WriteArrayToFile(ByVal fileName As String, _
                                ByVal sFileArray() As String, _
                                ByVal formato As System.Text.Encoding)
        ' Guarda en FileName el contenido del array indicado        (25/Ene/04)
        ' usando el formato indicado
        Dim s As String
        '
        Try
            ' Convertimos el array en una cadena, usando CrLf como separador
            's = Join(sFileArray, vbCrLf)
            s = String.Join(vbCrLf, sFileArray)
            '
            Dim sw As New System.IO.StreamWriter(fileName, False, formato)
            sw.WriteLine(s)
            sw.Close()
        Catch
        End Try
        '
    End Sub
    '
    Public Function FileExists(ByVal FileName As String) As Boolean
        ' Devuelve True si el fichero existe
        ' False si no existe o da error el acceso
        Dim b As Boolean
        '
        Try
            b = System.IO.File.Exists(FileName)
        Catch
            b = False
        End Try
        Return b
        '
    End Function
    '
    Public Function InsertArrayAt(ByRef aSource() As String, _
                                  ByRef aDest() As String, _
                                  Optional ByVal nIndex As cFileToArray.eFAInsertArray = eFAInsertArray.DespuesDelUltimo _
                                  ) As String()
        ' Insertar en el array aDest los elementos de aSource
        ' a partir de la posición nIndex del array de destino,
        ' el valor por defecto (-1) indicará que se añadan al final,
        ' el problema es cuando el array está vacío y el índice superior
        ' es cero, el primer elemento seguirá vacio, por tanto se indicará -2.
        ' Los arrays deben tener el índice 0, al menos el de destino.
        '
        Dim k, i, j, n As Integer
        ' Redimensionar el array de destino con los nuevos elementos del origen
        n = UBound(aSource)
        k = LBound(aSource)
        j = UBound(aDest)
        ReDim Preserve aDest((n - k + 1) + j)
        ' Si nIndex <> -1, hacer el hueco para los nuevos elementos
        If nIndex > eFAInsertArray.DespuesDelUltimo Then
            For i = nIndex To j
                aDest(i + (n - k + 1)) = aDest(i)
            Next
        ElseIf nIndex = eFAInsertArray.DespuesDelUltimo Then
            nIndex = CType(j + 1, eFAInsertArray)
        Else
            nIndex = CType(j, eFAInsertArray)
        End If
        For i = k To n
            aDest(nIndex) = aSource(i)
            nIndex = CType(nIndex + 1, eFAInsertArray)
        Next
        Return aDest
    End Function
    '
    Public Function FileExtension(ByVal sFileName As String, _
                                  Optional ByVal retLowerCase As cFileToArray.eFACase = eFACase.SameCaseFA) As String
        ' Devuelve la extensión del fichero indicado                    (25/Mar/02)
        ' (incluyendo el punto)
        '
        Dim s As String = System.IO.Path.GetExtension(sFileName)
        '
        Select Case retLowerCase
            Case eFACase.LowerCaseFA
                's = LCase(s)
                s = s.ToLower
            Case eFACase.UpperCaseFA
                's = UCase(s)
                s = s.ToUpper
        End Select
        '
        Return s
    End Function
    '
    '<summary>
    ' Averigua el formato del fichero indicado
    ' Sólo se comprueban los formatos Unicode, UTF-8 y ANSI (predeterminado de Windows)
    '</summary>
    Public Function FormatoFichero(ByVal fichero As String) As System.Text.Encoding
        ' por defecto devolver ANSI
        ' Los ficheros Unicode tienen estos dos bytes: FF FE
        ' Los ficheros UTF-8 tienen estos tres bytes: EF BB BF
        Dim f As System.Text.Encoding
        'Dim fs As System.IO.FileStream = Nothing
        f = System.Text.Encoding.Default
        ' En Windows Vista me dio error al acceder a una unidad     (23/Nov/06)
        ' de red compartida desde otro Windows Vista.
        ' En realidad el error era al acceder a una carpeta inexistente.
        Try
            Using fs As New System.IO.FileStream(fichero, System.IO.FileMode.Open)
                ' Abrir el fichero y averiguar el formato
                Dim c1 As Integer = fs.ReadByte
                Dim c2 As Integer = fs.ReadByte
                Dim c3 As Integer = fs.ReadByte
                '
                If c1 = 255 AndAlso c2 = 254 Then
                    f = System.Text.Encoding.Unicode
                ElseIf c1 = 239 AndAlso c2 = 187 AndAlso c3 = 191 Then
                    f = System.Text.Encoding.UTF8
                End If
            End Using
        Catch 'ex As Exception
            f = System.Text.Encoding.Default
            'Finally
            '    fs.Close()
        End Try
        '
        Return f
    End Function
End Class
