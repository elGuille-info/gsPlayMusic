'------------------------------------------------------------------------------
' Acerca de gsPlayWPF                                               (22/Ago/07)
' Ventana creada completamente con WPF
'
' ©Guillermo 'guille' Som, 2007
'------------------------------------------------------------------------------
Option Strict On

Imports vb = Microsoft.VisualBasic

Imports System
Imports System.Windows
Imports System.Windows.Input


Partial Public Class wAbout

    Private ratonPulsado As Boolean
    Private pX, pY As Double

    Private Sub btnAceptar_Click(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles btnAceptar.Click
        Me.Close()
    End Sub

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        iniciarApp()
    End Sub

    Private Sub iniciarApp()
        Dim ensamblado As System.Reflection.Assembly
        Dim fvi As System.Diagnostics.FileVersionInfo
        Dim bugInfo As String

        ensamblado = System.Reflection.Assembly.GetExecutingAssembly
        fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(ensamblado.Location)


        'With My.Application.Info
        '    Me.labelCopyR.Content = .Copyright
        '    Me.labelDescripcion.Content = .Description.Replace("  ", vb.vbCrLf) & vb.vbCrLf & vb.vbCrLf & _
        '                                  .Trademark.Replace("  ", vb.vbCrLf) & vb.vbCrLf
        '    Me.labelTitulo.Content = .Title
        '    Me.labelVersion.Content = .ProductName & " v" & .Version.ToString & " (" & fvi.FileVersion & ")"
        '    bugInfo = "Bug o mejora en " & .Title & " v" & fvi.FileVersion

        'End With
        Me.labelCopyR.Content = fvi.LegalCopyright ' .Copyright
        Me.labelDescripcion.Content = fvi.FileDescription.Replace("  ", vb.vbCrLf) & vb.vbCrLf & vb.vbCrLf &
                                          fvi.LegalTrademarks.Replace("  ", vb.vbCrLf) & vb.vbCrLf
        Me.labelTitulo.Content = fvi.ProductName ' .Title
        Me.labelVersion.Content = fvi.ProductName & " v" & fvi.ProductVersion.ToString & " (" & fvi.FileVersion & ")"
        bugInfo = "Bug o mejora en " & fvi.ProductName & " v" & fvi.FileVersion

        ' El hyperlink lo añado en un TextBlock                     (23/Ago/07)
        Me.linkBug.ToolTip = "Pulsa aquí para reportar un bug o mejora de " & bugInfo
        Me.linkBug.NavigateUri = New Uri("https://www.elguille.info/elguille_bugsmejoras.asp?subj=" & bugInfo)

        'If My.Computer.Network.IsAvailable Then
        '    Me.labelWeb.Content = "Actualmente tienes conexión a la red." & vb.vbCrLf
        'Else
        '    Me.labelWeb.Content = "Parece que no tienes conexión a la red." & vb.vbCrLf
        '    Me.linkBug.IsEnabled = False
        'End If

    End Sub

    Private Sub wAbout_PreviewMouseDown(ByVal sender As Object, _
                                        ByVal e As MouseButtonEventArgs) Handles Me.PreviewMouseDown
        ' Mover el formulario mientras se mantenga el ratón pulsado
        ratonPulsado = True
        pX = e.GetPosition(Me).X
        pY = e.GetPosition(Me).Y

    End Sub

    Private Sub wAbout_PreviewMouseMove(ByVal sender As Object, _
                                        ByVal e As MouseEventArgs) Handles Me.PreviewMouseMove
        If ratonPulsado Then
            Me.Left += e.GetPosition(Me).X - pX
            Me.Top += e.GetPosition(Me).Y - pY
        End If
    End Sub

    Private Sub wAbout_PreviewMouseUp(ByVal sender As Object, _
                                      ByVal e As MouseButtonEventArgs) Handles Me.PreviewMouseUp
        ratonPulsado = False
    End Sub

    Private Sub linkBug_Click(ByVal sender As Object, _
                              ByVal e As RoutedEventArgs) Handles linkBug.Click
        'If My.Computer.Network.IsAvailable Then
        '    System.Diagnostics.Process.Start(linkBug.NavigateUri.ToString)
        'End If
        System.Diagnostics.Process.Start(linkBug.NavigateUri.ToString)
    End Sub
End Class
