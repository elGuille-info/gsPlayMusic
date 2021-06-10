'------------------------------------------------------------------------------
' ItemFichero                                                       (16/Ago/07)
' Clase para mantener los ficheros a mostrar en el ListView
'
' ©Guillermo 'guille' Som, 2007
'------------------------------------------------------------------------------
Option Strict On

Imports Microsoft.VisualBasic
Imports System

Imports System.ComponentModel
Imports System.Collections.Generic


''' <summary>
''' Clase para el ListView de gsPlayWPF
''' Usar objetos de esta clase para añadir al ListView
''' </summary>
''' <remarks>
''' Autor: ©Guillermo 'guille' Som, 2008
''' Fecha: 16/Ago/2007
''' Revisado: 17/Ago/2007
''' Revisado: 19/Ago/2007 con la enumeración CamposClasificar, etc.
''' Revisado: 22/Ago/2007 añado FullName a las opciones de clasificación
''' </remarks>
Public Class ItemFichero
    Implements IComparable


    ''' <summary>
    ''' Si se debe mostrar la información completa en ToString
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared InfoCompleta As Boolean = True

    ''' <summary>
    ''' Si la info completa se muestra en una línea
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared InfoUnaLinea As Boolean = False ' True

    ''' <summary>
    ''' Enumeración para el campo por el que se clasificará
    ''' </summary>
    ''' <remarks>
    ''' 19/Ago/07
    ''' 22/Ago/07: Añado el FullName
    ''' </remarks>
    Public Enum CamposClasificar
        Nombre
        Directorio
        Fecha
        Tamaño
        Duración
        FullName
    End Enum

    ''' <summary>
    ''' El campo que se usará en la clasificación.
    ''' </summary>
    ''' <remarks>
    ''' 18/Ago/07
    ''' 19/Ago: Lo cambio para usar la enumeración CamposClasificar
    ''' </remarks>
    Public Shared CampoClasificar As CamposClasificar = CamposClasificar.Nombre

    Private m_FullName As String

    ''' <summary>
    ''' Nombre completo del fichero
    ''' </summary>
    ''' <remarks></remarks>
    Public Property FullName() As String
        Get
            Return m_FullName
        End Get
        Set(ByVal value As String)
            m_FullName = value
        End Set
    End Property

    Private m_Nombre As String

    ''' <summary>
    ''' Nombre del fichero, sin directorio
    ''' </summary>
    ''' <remarks>
    ''' Como propiedad para usar con el Listview
    ''' </remarks>
    Public Property Nombre() As String
        Get
            Return m_Nombre
        End Get
        Set(ByVal value As String)
            m_Nombre = value
        End Set
    End Property

    Private m_Directorio As String

    ''' <summary>
    ''' El directorio del fichero
    ''' </summary>
    ''' <remarks></remarks>
    Public Property Directorio() As String
        Get
            Return m_Directorio
        End Get
        Set(ByVal value As String)
            m_Directorio = value
        End Set
    End Property

    Private m_Fecha As DateTime

    ''' <summary>
    ''' Fecha de última escritura (en UTC/GMT)
    ''' </summary>
    ''' <remarks></remarks>
    Public Property Fecha() As DateTime
        Get
            Return m_Fecha
        End Get
        Set(ByVal value As DateTime)
            m_Fecha = value
        End Set
    End Property

    ''' <summary>
    ''' Para mostrar la fecha en el Listview
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>
    ''' 22/Ago/07
    ''' </remarks>
    Public ReadOnly Property FechaList() As String
        Get
            Return Fecha.ToString("dd/MM/yy HH:mm:ss")
        End Get
    End Property

    Private m_Tamaño As Long

    ''' <summary>
    ''' Tamaño del fichero
    ''' </summary>
    ''' <remarks></remarks>
    Public Property Tamaño() As Long
        Get
            Return m_Tamaño
        End Get
        Set(ByVal value As Long)
            m_Tamaño = value
        End Set
    End Property

    Public ReadOnly Property TamañoList() As String
        Get
            Return (Me.Tamaño / (1024 * 1024)).ToString("#,##0.00 MB").PadLeft(11)
        End Get
    End Property

    Private m_Duracion As String

    ''' <summary>
    ''' Duración del fichero (con el formato de mm:ss)
    ''' </summary>
    ''' <remarks></remarks>
    Public Property Duración() As String
        Get
            Return m_Duracion
        End Get
        Set(ByVal value As String)
            m_Duracion = value
        End Set
    End Property

    Public ReadOnly Property DuraciónList() As String
        Get
            ' Por si la cadena está vacía                       (01/Nov/15 14.02)
            Return If(String.IsNullOrWhiteSpace(Duración), "", Duración.PadLeft(10))
        End Get
    End Property

    ''' <summary>
    ''' Devuelve el nombre (sin path) del fichero.
    ''' Será la cadena que se muestre en las listas.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function ToString() As String
        Return ToString(InfoCompleta)
    End Function

    ''' <summary>
    ''' Sobrecarga para devolver la info completa o sencilla
    ''' </summary>
    ''' <param name="infoCompleta"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Function ToString(ByVal infoCompleta As Boolean) As String
        If infoCompleta Then
            Dim sb As New System.Text.StringBuilder(Nombre)

            If InfoUnaLinea Then
                sb.Append(" - ")
            Else
                sb.AppendLine()
            End If

            sb.AppendFormat("{0}, {1} MB, {2}", _
                            Me.Fecha.ToString("dd/MM/yy HH:mm:ss"), _
                            (Me.Tamaño / (1024 * 1024)).ToString("#,###.##"), _
                            Me.Duración)

            If InfoUnaLinea Then
                sb.Append(" - ")
            Else
                sb.AppendLine()
            End If

            sb.Append(Me.Directorio)

            Return sb.ToString
        Else
            Return Nombre
        End If
    End Function

    ''' <summary>
    ''' Constructor indicando el nombre completo del fichero
    ''' </summary>
    ''' <param name="fullName"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal fullName As String)
        Me.FullName = fullName
        Dim fi As New System.IO.FileInfo(fullName)
        With fi
            Nombre = .Name
            Directorio = .DirectoryName
            Fecha = .LastWriteTimeUtc
            Tamaño = .Length
        End With
    End Sub

    ''' <summary>
    ''' Constructor indicando un objeto FileInfo que apunta al fichero
    ''' </summary>
    ''' <param name="fi"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal fi As System.IO.FileInfo)
        With fi
            Me.FullName = .FullName
            Nombre = .Name
            Directorio = .DirectoryName
            Fecha = .LastWriteTimeUtc
            Tamaño = .Length
        End With
    End Sub

    ''' <summary>
    ''' Para clasificar los elementos de este tipo
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo
        If TypeOf obj Is ItemFichero Then
            Dim ifi As ItemFichero = TryCast(obj, ItemFichero)
            Select Case CampoClasificar
                Case CamposClasificar.Directorio
                    Return String.Compare(Me.Directorio, ifi.Directorio)
                    ' El directorio y el nombre                     (22/Ago/07)
                    ' (¿sería mejor usar full name?)
                    'Return String.Compare(Me.Directorio & Me.Nombre, ifi.Directorio & ifi.Nombre)
                    'Return String.Compare(Me.FullName, ifi.FullName)
                    ' Dejarlo solo por el directorio, como ya está la opción de FullName, pues...
                Case CamposClasificar.Duración
                    Return String.Compare(Me.Duración, ifi.Duración)
                Case CamposClasificar.Fecha
                    Return String.Compare(Me.Fecha.ToString("yyyymmddHHmmss"), ifi.Fecha.ToString("yyyymmddHHmmss"))
                Case CamposClasificar.Tamaño
                    Return String.Compare(Me.Tamaño.ToString("00000000"), ifi.Tamaño.ToString("00000000"))
                Case CamposClasificar.FullName
                    Return String.Compare(Me.FullName, ifi.FullName)
                Case Else
                    Return String.Compare(Me.Nombre, ifi.Nombre)
            End Select
        Else
            Return String.Compare(Me.ToString, obj.ToString)
        End If
    End Function
End Class
