'------------------------------------------------------------------------------
' Ventana para pedir datos                                          (23/Ago/07)
' (al estilo de InputBox, pero simplificada)
'
' Basada en la clase InputBoxDialog, pero creada solo con código para WPF
'
' ©Guillermo 'guille' Som, 2007
'------------------------------------------------------------------------------
Option Strict On

Imports System
Imports System.Windows

Partial Public Class wInputBoxDialog

    ''' <summary>
    ''' El valor a modificar o indicar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Valor() As String
        Get
            Return Me.txtValor.Text
        End Get
        Set(ByVal value As String)
            Me.txtValor.Text = value
        End Set
    End Property

    ''' <summary>
    ''' El texto del mensaje a mostrar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Descripcion() As String
        Get
            Return Me.labelMensaje.Text
        End Get
        Set(ByVal value As String)
            Me.labelMensaje.Text = value
        End Set
    End Property

    Private m_OcultarCancelar As Boolean = False

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Icon = New BitmapImage(New Uri($"{Application.DirResources}/Opciones colores.png"))
    End Sub

    ''' <summary>
    ''' Si se debe ocultar el botón de Cancelar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>
    ''' 23/Ago/07
    ''' </remarks>
    Public Property OcultarCancelar() As Boolean
        Get
            Return m_OcultarCancelar
        End Get
        Set(ByVal value As Boolean)
            ' Usar Collapse para que no se deje el espacio
            ' que ocupaba
            If value Then
                Me.btnCancelar.Visibility = Windows.Visibility.Collapsed
            Else
                Me.btnCancelar.Visibility = Windows.Visibility.Visible
            End If
            m_OcultarCancelar = value
        End Set
    End Property

    Private Sub btnAceptar_Click(ByVal sender As Object, _
                                 ByVal e As RoutedEventArgs) Handles btnAceptar.Click
        Me.DialogResult = True
    End Sub

    Private Sub btnCancelar_Click(ByVal sender As Object, _
                                  ByVal e As RoutedEventArgs) Handles btnCancelar.Click
        Me.DialogResult = False
    End Sub
End Class
