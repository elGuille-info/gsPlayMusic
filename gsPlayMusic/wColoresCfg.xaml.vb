'------------------------------------------------------------------------------
' Ventana para la configuración de los colores                      (19/Ago/07)
'
' También permite guardar valores de colores definidos
' estos se guardarán en "Mis documentos\gsPlay\Colores\
' Los definidos por el usuario tendrán la extensión: .colorDef.user.xml
' los de la aplicación tendrán la extensión: .colorDef.app.xml
' Usar este formato de nombre para que estén clasificados:
' (nombre es el nombre del fichero)
' colorDef.app.nombre.xml
' colorDef.user.nombre.xml
'
' Es un rollo para filtrar al abrir, usar: .colorDef.xml
' (aunque los del usuario se guardan como .user.colorDef.xml
'
' ©Guillermo 'guille' Som, 2007
'------------------------------------------------------------------------------
Option Strict On

Imports vb = Microsoft.VisualBasic

Imports System
Imports System.Windows
Imports System.Windows.Controls
'Imports System.Windows.Data
Imports System.Windows.Media
'Imports System.Windows.Media.Animation
'Imports System.Windows.Navigation
Imports System.Windows.Media.Imaging

Imports System.Windows.Input

Imports System.Collections.Generic
Imports System.ComponentModel

Imports System.IO
Imports System.Text

Imports System.Xml
Imports System.Xml.XPath

'System.Windows.Media.LinearGradientBrush
' System.Windows.Media.SolidColorBrush

Partial Public Class wColoresCfg

    ' Los nombres de las definiciones de colores                    (20/Ago/07)
    Private nombresFondos() As String = {"fondoVentana", "fondoActual", _
                                         "fondoListas", "fondoListaActual", _
                                         "fondoOpcionesListaActual", _
                                         "fondoBotonera"}
    ' letraVentana no se usa pero debe estar...
    Private nombresTextos() As String = {"letraVentana", "letraActual", _
                                         "letraListas", "letraListaActual", _
                                         "letraOpcionesListaActual", _
                                         "letraBotonera"}

    ' El directorio de las definiciones de colores                  (20/Ago/07)
    Private dirDefColores As String

    ' La posición original de la ventana principal                  (20/Ago/07)
    Private principalLeft As Double

    Private iniciando As Boolean = True

    Private fondos As New List(Of LinearGradientBrush)
    Private fondosCopia As New List(Of LinearGradientBrush)
    Private colorTextos As New List(Of SolidColorBrush)
    Private colorTextosCopia As New List(Of SolidColorBrush)

    Private etiquetas As New List(Of Label)
    Private slR() As List(Of Slider) = {New List(Of Slider), New List(Of Slider)}
    Private slG() As List(Of Slider) = {New List(Of Slider), New List(Of Slider)}
    Private slB() As List(Of Slider) = {New List(Of Slider), New List(Of Slider)}
    Private slFore As New List(Of Slider)


    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        iniciarContenidoVentana()
    End Sub

    ''' <summary>
    ''' Ponerlo en método aparte para llamarlo desde el constructor
    ''' </summary>
    ''' <remarks>
    ''' 20/Ago/07 (v2.0.0.161)
    ''' </remarks>
    Private Sub iniciarContenidoVentana()
        ' El directorio de las definiciones de colores
        'With My.Computer.FileSystem
        '    dirDefColores = .SpecialDirectories.MyDocuments &
        '                    "\gsPlay\Colores"
        '    If .DirectoryExists(dirDefColores) = False Then
        '        .CreateDirectory(dirDefColores)
        '    End If
        'End With

        Icon = New BitmapImage(New Uri($"{Application.DirResources}/Opciones colores.png"))

        dirDefColores = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\gsPlay\Colores"
        If System.IO.Directory.Exists(dirDefColores) = False Then
            System.IO.Directory.CreateDirectory(dirDefColores)
        End If
        '
        ' Leer los valores de la ventana principal
        ' y asignarlo a los controles, etc.

        ' Posicionar la ventana al lado de la principal
        With Window.VentanaPrincipal
            Me.Top = .Top
            ' Para restaurar la posición al cerrar                  (20/Ago/07)
            principalLeft = .Left
            ' Si la ventana se sale de la pantalla, desplazar la principal
            Dim p As Double = .Left + .Width + 6 + Me.Width
            'If p > My.Computer.Screen.WorkingArea.Width Then
            '    p = My.Computer.Screen.WorkingArea.Width - Me.Width - .Width - 6
            '    .Left = p
            'End If
            Me.Left = .Left + .Width + 6
        End With

        crearSliders()

        'fondos.Add(CType(Window1.VentanaPrincipal.gridMain.Background, LinearGradientBrush))
        'fondos.Add(CType(Window1.VentanaPrincipal.expActual.Background, LinearGradientBrush))
        'fondos.Add(CType(Window1.VentanaPrincipal.expListas.Background, LinearGradientBrush))
        'fondos.Add(CType(Window1.VentanaPrincipal.expListaActual.Background, LinearGradientBrush))
        'fondos.Add(CType(Window1.VentanaPrincipal.expOpcionesLista.Background, LinearGradientBrush))
        'fondos.Add(CType(Window1.VentanaPrincipal.expBotonera.Background, LinearGradientBrush))

        fondosCopia.Add(CType(Window.VentanaPrincipal.gridMain.Background.Clone, LinearGradientBrush))
        fondosCopia.Add(CType(Window.VentanaPrincipal.expActual.Background.Clone, LinearGradientBrush))
        fondosCopia.Add(CType(Window.VentanaPrincipal.expListas.Background.Clone, LinearGradientBrush))
        fondosCopia.Add(CType(Window.VentanaPrincipal.expListaActual.Background.Clone, LinearGradientBrush))
        fondosCopia.Add(CType(Window.VentanaPrincipal.expOpcionesLista.Background.Clone, LinearGradientBrush))
        fondosCopia.Add(CType(Window.VentanaPrincipal.expBotonera.Background.Clone, LinearGradientBrush))

        'letraCopia.Add(Nothing)
        colorTextosCopia.Add(New SolidColorBrush(Colors.Black))
        colorTextosCopia.Add(CType(Window.VentanaPrincipal.expActual.Foreground.Clone, SolidColorBrush))
        colorTextosCopia.Add(CType(Window.VentanaPrincipal.expListas.Foreground.Clone, SolidColorBrush))
        colorTextosCopia.Add(CType(Window.VentanaPrincipal.expListaActual.Foreground.Clone, SolidColorBrush))
        colorTextosCopia.Add(CType(Window.VentanaPrincipal.expOpcionesLista.Foreground.Clone, SolidColorBrush))
        colorTextosCopia.Add(CType(Window.VentanaPrincipal.expBotonera.Foreground.Clone, SolidColorBrush))

        etiquetas.Add(Me.lblPrincipal)
        etiquetas.Add(Me.lblActual)
        etiquetas.Add(Me.lblListas)
        etiquetas.Add(Me.lblListaActual)
        etiquetas.Add(Me.lblOpcionesLista)
        etiquetas.Add(Me.lblBotonera)

        Me.btnRestablecer_Click(Nothing, Nothing)

        ' Leer los ficheros de definición de colores                (20/Ago/07)
        leerFicColoresApp()
        leerFicherosColores()

        iniciando = False
    End Sub


    ''' <summary>
    ''' Crear los slider que se usarán
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub crearSliders()
        Dim stack1 As StackPanel
        Dim stack2 As StackPanel
        Dim slid As Slider
        Const colores As String = "RGB"

        For r As Integer = 0 To 5
            ' Para los slider del color de la letra
            ' (solo se usa uno)
            slid = New Slider
            slid.Style = CType(Resources("sliderColores"), Style)
            slid.VerticalAlignment = VerticalAlignment.Center
            ' La primera fila ahora es la 1 porque en la 0 está el menú
            Grid.SetRow(slid, r + 1)
            Grid.SetColumn(slid, 0)
            Me.gridColores.Children.Add(slid)
            slFore.Add(slid)
            If r = 0 Then
                slid.Visibility = System.Windows.Visibility.Hidden
            Else
                AddHandler slid.ValueChanged, AddressOf sliderTexto_ValueChanged
            End If

            ' Para los slider del color de fondo
            ' (se usan seis, tres para cada color
            ' en dos grupos para los gradientStop)
            stack1 = New StackPanel
            Me.gridColores.Children.Add(stack1)
            Grid.SetRow(stack1, r + 1)
            Grid.SetColumn(stack1, 2)

            For Each c As Char In colores
                stack2 = New StackPanel
                stack2.Style = CType(Resources("stackColores"), Style)
                stack1.Children.Add(stack2)

                For n As Integer = 0 To 1

                    slid = New Slider
                    slid.Style = CType(Resources("sliderColores"), Style)
                    If n = 0 Then
                        slid.Margin = New Thickness(0, 0, 8, 0)
                    End If
                    stack2.Children.Add(slid)
                    AddHandler slid.ValueChanged, AddressOf slider_ValueChanged
                    Select Case c
                        Case "R"c
                            slR(n).Add(slid)
                        Case "G"c
                            slG(n).Add(slid)
                        Case "B"c
                            slB(n).Add(slid)
                    End Select
                    slid.Tag = c & r & n

                Next
            Next
        Next

    End Sub

    Private Sub winCfg_Closing(ByVal sender As Object, _
                               ByVal e As CancelEventArgs) Handles Me.Closing
        ' Restaurar la posición de la ventana principal
        Window.VentanaPrincipal.Left = principalLeft
    End Sub

    Private Sub winCfg_Loaded(ByVal sender As Object, _
                              ByVal e As RoutedEventArgs) Handles Me.Loaded
    End Sub

    Private Sub btnCancelar_Click(ByVal sender As Object, _
                                  ByVal e As RoutedEventArgs) _
                                  Handles btnCancelar.Click, mnuCancelar.Click

        ' Restaurar los colores

        Window.VentanaPrincipal.gridMain.Background = fondosCopia(0).Clone
        Window.VentanaPrincipal.expActual.Background = fondosCopia(1).Clone
        Window.VentanaPrincipal.expListas.Background = fondosCopia(2).Clone
        Window.VentanaPrincipal.expListaActual.Background = fondosCopia(3).Clone
        Window.VentanaPrincipal.expOpcionesLista.Background = fondosCopia(4).Clone
        Window.VentanaPrincipal.expBotonera.Background = fondosCopia(5).Clone

        Window.VentanaPrincipal.expActual.Foreground = colorTextosCopia(1).Clone
        Window.VentanaPrincipal.expListas.Foreground = colorTextosCopia(2).Clone
        Window.VentanaPrincipal.expListaActual.Foreground = colorTextosCopia(3).Clone
        Window.VentanaPrincipal.expOpcionesLista.Foreground = colorTextosCopia(4).Clone
        Window.VentanaPrincipal.expBotonera.Foreground = colorTextosCopia(5).Clone

        Me.DialogResult = False
    End Sub

    Private Sub btnAceptar_Click(ByVal sender As Object, _
                                 ByVal e As RoutedEventArgs) Handles btnAceptar.Click
        Me.DialogResult = True
    End Sub

    Private Sub sliderTexto_ValueChanged(ByVal sender As Object, _
                                         ByVal e As RoutedPropertyChangedEventArgs(Of Double))
        If iniciando Then Exit Sub

        Dim sl As Slider = TryCast(sender, Slider)
        Dim i As Integer = Me.slFore.IndexOf(sl)
        If i = -1 Then Exit Sub

        Dim c As Color
        c = colorTextos(i).Color
        Dim col As Single = CSng(sl.Value / 10)
        c.ScR = col
        c.ScG = col
        c.ScB = col
        colorTextos(i).Color = c
        etiquetas(i).Foreground = colorTextos(i)

        sl.ToolTip = "RGB " & col
    End Sub

    ''' <summary>
    ''' Cuando se cambiar el valor de un slider de los colores.
    ''' Si chkBloquear está seleccionado, mover todos al mismo tiempo
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub slider_ValueChanged(ByVal sender As Object, _
                                    ByVal e As RoutedPropertyChangedEventArgs(Of Double))
        If iniciando Then Exit Sub

        Dim sl As Slider = TryCast(sender, Slider)
        Dim s As String = sl.Tag.ToString
        '"Rrn"
        Dim i As Integer = CInt(s.Substring(1, 1))
        Dim n As Integer = CInt(s.Substring(2, 1))

        Dim c As Color
        c = fondos(i).GradientStops(n).Color

        ' Si está marcada la opción de bloquear                     (20/Ago/07)
        If chkBloquear.IsChecked Then
            iniciando = True
            ' Bloquar todos los de ese mismo panel
            ' En realidad en este panel están los dos gradientStop
            ' hay que asignar los valores del mísmo índice (n)(?)
            'Dim panel As StackPanel = TryCast(sl.Parent, StackPanel)
            'For Each slc As Slider In panel.Children
            '    slc.Value = sl.Value
            'Next
            slR(n)(i).Value = sl.Value
            slG(n)(i).Value = sl.Value
            slB(n)(i).Value = sl.Value
            ' Asignar a los colores para que 
            ' estén unificados
            c.ScR = CSng(slR(n)(i).Value / 10)
            c.ScG = CSng(slG(n)(i).Value / 10)
            c.ScB = CSng(slB(n)(i).Value / 10)
            iniciando = False
        End If

        Select Case s.Substring(0, 1)
            Case "R"
                c.ScR = CSng(slR(n)(i).Value / 10)
            Case "G"
                c.ScG = CSng(slG(n)(i).Value / 10)
            Case "B"
                c.ScB = CSng(slB(n)(i).Value / 10)
        End Select

        fondos(i).GradientStops(n).Color = c
        etiquetas(i).Background = fondos(i)

        sl.ToolTip = s.Substring(0, 1) & " " & sl.Value / 10
    End Sub

    Private Sub btnRestablecer_Click(ByVal sender As Object, _
                                     ByVal e As RoutedEventArgs) Handles btnRestablecer.Click
        iniciando = True

        ' Restaurar los colores
        Window.VentanaPrincipal.gridMain.Background = fondosCopia(0).Clone
        Window.VentanaPrincipal.expActual.Background = fondosCopia(1).Clone
        Window.VentanaPrincipal.expListas.Background = fondosCopia(2).Clone
        Window.VentanaPrincipal.expListaActual.Background = fondosCopia(3).Clone
        Window.VentanaPrincipal.expOpcionesLista.Background = fondosCopia(4).Clone
        Window.VentanaPrincipal.expBotonera.Background = fondosCopia(5).Clone

        Window.VentanaPrincipal.expActual.Foreground = colorTextosCopia(1).Clone
        Window.VentanaPrincipal.expListas.Foreground = colorTextosCopia(2).Clone
        Window.VentanaPrincipal.expListaActual.Foreground = colorTextosCopia(3).Clone
        Window.VentanaPrincipal.expOpcionesLista.Foreground = colorTextosCopia(4).Clone
        Window.VentanaPrincipal.expBotonera.Foreground = colorTextosCopia(5).Clone

        fondos.Clear()
        fondos.Add(CType(Window.VentanaPrincipal.gridMain.Background, LinearGradientBrush))
        fondos.Add(CType(Window.VentanaPrincipal.expActual.Background, LinearGradientBrush))
        fondos.Add(CType(Window.VentanaPrincipal.expListas.Background, LinearGradientBrush))
        fondos.Add(CType(Window.VentanaPrincipal.expListaActual.Background, LinearGradientBrush))
        fondos.Add(CType(Window.VentanaPrincipal.expOpcionesLista.Background, LinearGradientBrush))
        fondos.Add(CType(Window.VentanaPrincipal.expBotonera.Background, LinearGradientBrush))

        colorTextos.Clear()
        colorTextos.Add(New SolidColorBrush(Colors.Black))
        colorTextos.Add(CType(Window.VentanaPrincipal.expActual.Foreground, SolidColorBrush))
        colorTextos.Add(CType(Window.VentanaPrincipal.expListas.Foreground, SolidColorBrush))
        colorTextos.Add(CType(Window.VentanaPrincipal.expListaActual.Foreground, SolidColorBrush))
        colorTextos.Add(CType(Window.VentanaPrincipal.expOpcionesLista.Foreground, SolidColorBrush))
        colorTextos.Add(CType(Window.VentanaPrincipal.expBotonera.Foreground, SolidColorBrush))

        asignarValoresControles()

        iniciando = False
    End Sub

    ''' <summary>
    ''' Asignar los valores de los colores a los controles
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub asignarValoresControles()
        For i As Integer = 0 To fondos.Count - 1
            With slFore(i)
                .Value = colorTextos(i).Color.ScR * 10
                .ToolTip = "RGB " & .Value / 10
            End With
            etiquetas(i).Foreground = colorTextos(i)
            etiquetas(i).Background = fondos(i)
            For n As Integer = 0 To 1
                With slR(n)(i)
                    .Value = fondos(i).GradientStops(n).Color.ScR * 10
                    .ToolTip = "R " & .Value / 10
                    .Background = Brushes.Red
                End With
                With slG(n)(i)
                    .Value = fondos(i).GradientStops(n).Color.ScG * 10
                    .ToolTip = "G " & .Value / 10
                    .Background = Brushes.Green
                End With
                With slB(n)(i)
                    .Value = fondos(i).GradientStops(n).Color.ScB * 10
                    .ToolTip = "B " & .Value / 10
                    .Background = Brushes.Blue
                End With
            Next
        Next
    End Sub

    ''' <summary>
    ''' Asignar los colores a la ventana principal
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub asignarColoresVentanaPrincipal()
        Window.VentanaPrincipal.gridMain.Background = fondos(0) '.Clone
        Window.VentanaPrincipal.expActual.Background = fondos(1) '.Clone
        Window.VentanaPrincipal.expListas.Background = fondos(2) '.Clone
        Window.VentanaPrincipal.expListaActual.Background = fondos(3) '.Clone
        Window.VentanaPrincipal.expOpcionesLista.Background = fondos(4) '.Clone
        Window.VentanaPrincipal.expBotonera.Background = fondos(5) '.Clone

        Window.VentanaPrincipal.expActual.Foreground = colorTextos(1) '.Clone
        Window.VentanaPrincipal.expListas.Foreground = colorTextos(2) '.Clone
        Window.VentanaPrincipal.expListaActual.Foreground = colorTextos(3) '.Clone
        Window.VentanaPrincipal.expOpcionesLista.Foreground = colorTextos(4) '.Clone
        Window.VentanaPrincipal.expBotonera.Foreground = colorTextos(5) '.Clone

    End Sub

    Private Sub btnRestablecerOri_Click(ByVal sender As Object, _
                                        ByVal e As RoutedEventArgs) _
                                        Handles btnRestablecerOri.Click, mnuColorPredeterminado.Click
        ' Restablecer los colores originales de la aplicación
        fondosCopia.Clear()
        fondosCopia.Add(CType(Resources("fondoVentana"), LinearGradientBrush).Clone)
        fondosCopia.Add(CType(Resources("fondoActual"), LinearGradientBrush).Clone)
        fondosCopia.Add(CType(Resources("fondoListas"), LinearGradientBrush).Clone)
        fondosCopia.Add(CType(Resources("fondoListaActual"), LinearGradientBrush).Clone)
        fondosCopia.Add(CType(Resources("fondoOpcionesLista"), LinearGradientBrush).Clone)
        fondosCopia.Add(CType(Resources("fondoBotonera"), LinearGradientBrush).Clone)

        colorTextosCopia.Clear()
        colorTextosCopia.Add(New SolidColorBrush(Colors.Black))
        colorTextosCopia.Add(CType(Resources("textoActual"), SolidColorBrush).Clone)
        colorTextosCopia.Add(CType(Resources("textoListas"), SolidColorBrush).Clone)
        colorTextosCopia.Add(CType(Resources("textoListaActual"), SolidColorBrush).Clone)
        colorTextosCopia.Add(CType(Resources("textoOpcionesLista"), SolidColorBrush).Clone)
        colorTextosCopia.Add(CType(Resources("textoBotonera"), SolidColorBrush).Clone)

        btnRestablecer_Click(Nothing, Nothing)
    End Sub

    Private Sub chkUsarActual_Checked(ByVal sender As Object, _
                                      ByVal e As RoutedEventArgs) _
                                      Handles chkUsarActual.Checked
        iniciando = True

        ' Asignar a todos los paneles los valores de Actual
        For i As Integer = 0 To fondos.Count - 1
            ' El actual está en el índice 1
            If i = 1 Then Continue For

            fondos(i) = fondos(1).Clone
            colorTextos(i) = colorTextos(1).Clone
        Next
        asignarValoresControles()
        asignarColoresVentanaPrincipal()

        ' Quitar la marca para poder usarlo después
        chkUsarActual.IsChecked = False

        iniciando = False
    End Sub

    Private Sub mnuAbrir_Click(ByVal sender As Object, _
                               ByVal e As RoutedEventArgs) _
                               Handles mnuAbrir.Click
        ' Abrir una definición de colores
        ' La extensión será .colorDef.xml
        ' (los de usuario y los de la aplicación)
        Dim oFD As New Microsoft.Win32.OpenFileDialog
        With oFD
            .Title = "Abrir fichero de configuración de colores"
            .Filter = "Definición de colores (*.colorDef.xml)|*.colorDef.xml" '|Todos (*.*)|*.*"
            .FileName = dirDefColores & "\*.colorDef.xml"
            ' Que solo acepte ficheros que existan
            .CheckFileExists = True
            .AddExtension = True
            If .ShowDialog Then
                ' Leer el fichero de definición de colores
                ' (es posible que no sea correcto)
                leerColores(.FileName)
            End If
        End With
    End Sub

    Private Sub mnuGuardar_Click(ByVal sender As Object, _
                                 ByVal e As RoutedEventArgs) _
                                 Handles mnuGuardar.Click
        ' Guardar los colores actuales como una definición de usuario
        ' .user.colorDef.xml
        Dim sFD As New Microsoft.Win32.SaveFileDialog
        With sFD
            .Title = "Guardar los colores en un fichero de configuración"
            .Filter = "Definición de colores del usuario (*.user.colorDef.xml)|*.user.colorDef.xml" '|Todos (*.*)|*.*"
            .FileName = dirDefColores & "\misColores"
            .AddExtension = False
            If .ShowDialog Then
                ' Guardar los colores en el fichero indicado
                ' Comprobar que tenga la extensión adecuada
                Dim fic As String = .FileName
                If fic.ToLower().EndsWith(".user.colorDef.xml") = False Then
                    fic = Path.GetFileNameWithoutExtension(.FileName) & ".user.colorDef.xml"
                End If
                ' Algunas veces se empeña en añadir la extensión...
                ' particularmente cuando se guarda como uno existente
                If fic.Contains(".user.colorDef.user.colorDef.xml") Then
                    fic = fic.Replace(".user.colorDef.user.colorDef.xml", ".user.colorDef.xml")
                End If
                Dim i As Integer = fic.IndexOf(".user.colorDef.xml")
                ' Pedir el nombre y la descripción
                Dim nombre As String = fic.Substring(0, i)
                Dim descrip As String = "Definición de colores de " & fic.Substring(0, i)
                Dim inputB As New wInputBoxDialog 'InputBoxDialog
                With inputB
                    .Title = "El nombre de la definición"
                    .Descripcion = "Escribe el nombre de esta configuración de colores"
                    .Valor = nombre
                    .OcultarCancelar = True
                    If .ShowDialog Then '= Forms.DialogResult.OK Then
                        nombre = .Valor
                    End If
                End With
                inputB = New wInputBoxDialog 'InputBoxDialog
                With inputB
                    .Title = "La descripción de la definición"
                    .Descripcion = "Escribe la descripción de esta configuración de colores" & _
                                    vb.vbCrLf & vb.vbCrLf & _
                                    "Pulsa en Cancelar para no guardar esta configuración de colores."
                    .Valor = descrip
                    .OcultarCancelar = False
                    If .ShowDialog Then '= Forms.DialogResult.OK Then
                        descrip = .Valor
                        guardarColores(fic, nombre, descrip)
                    End If
                End With
                ' Si se cancela no se guarda
                'guardarColores(fic, nombre, descrip)

                ' releer los ficheros del menú de usuario
                leerFicherosColores()
            End If
        End With

    End Sub

    ''' <summary>
    ''' Guardar los colores actuales en el fichero indicado
    ''' Se usará el nombre y la descripción como valor interno de la definición,
    ''' esos valores se usarán para mostrar en los menús.
    ''' </summary>
    ''' <param name="fic"></param>
    ''' <param name="nombre"></param>
    ''' <param name="descrip"></param>
    ''' <remarks></remarks>
    Private Sub guardarColores(ByVal fic As String, ByVal nombre As String, ByVal descrip As String)
        ' Guardar los colores en el fichero indicado
        Using sw As New StreamWriter(fic, False, Encoding.UTF8)
            Dim sb As New StringBuilder
            sb.AppendFormat("<?xml version=""1.0"" encoding=""utf-8""?>{0}", vb.vbCrLf)
            ' Comentario de la fecha de creación
            sb.AppendFormat("<!-- Definición de colores para gsPlay (gsPlayMusic) - ©Guillermo Som (elGuille){0}",
                            vb.vbCrLf)
            sb.AppendFormat("     Definición creada el {0} UTC/GMT {1}", _
                            DateTime.Now.ToUniversalTime.ToString("dd/MMM/yyyy HH:mm:ss"), _
                            vb.vbCrLf)
            sb.AppendFormat("-->{0}", vb.vbCrLf)
            sb.AppendFormat("<colores>{0}", vb.vbCrLf)
            sb.AppendFormat("   <color name=""{0}"" description=""{1}"">{2}", _
                            nombre, descrip, vb.vbCrLf)
            For i As Integer = 0 To fondos.Count - 1
                sb.AppendFormat("      <setting name=""{0}"" serializeAs=""String"">{1}", _
                                nombresFondos(i), vb.vbCrLf)
                sb.AppendFormat("         <value>{0}</value>{1}", _
                                ColoresGradient.ToString(fondos(i)), vb.vbCrLf)
                sb.AppendFormat("      </setting>{0}", vb.vbCrLf)
            Next
            ' El cero no se usa (texto de la ventana principal)
            For i As Integer = 1 To colorTextos.Count - 1
                sb.AppendFormat("      <setting name=""{0}"" serializeAs=""String"">{1}", _
                                nombresTextos(i), vb.vbCrLf)
                sb.AppendFormat("         <value>{0}</value>{1}", colorTextos(i).ToString, vb.vbCrLf)
                sb.AppendFormat("      </setting>{0}", vb.vbCrLf)
            Next
            sb.AppendFormat("   </color>{0}", vb.vbCrLf)
            sb.AppendFormat("</colores>{0}", vb.vbCrLf)
            sw.Write(sb.ToString)
            sw.Close()
        End Using
    End Sub

    ''' <summary>
    ''' Leer la definición indicada en el fichero y asignarla a los colores
    ''' </summary>
    ''' <param name="fic"></param>
    ''' <remarks></remarks>
    Private Sub leerColores(ByVal fic As String)
        Try
            If System.IO.File.Exists(fic) = False Then
                MessageBox.Show("El fichero indicado no existe:" & vb.vbCrLf &
                                fic, "Leer fichero de colores",
                                 MessageBoxButton.OK, MessageBoxImage.Exclamation)
                Exit Sub
            End If

            Dim docXml As New System.Xml.XmlDocument

            docXml.Load(fic)

            ' Buscar los valores de cada configuración

            Dim navigator As XPathNavigator = docXml.CreateNavigator()

            'Si hay más de una definicion de colores, solo se lee la primera

            Dim nodes As XPathNodeIterator = navigator.Select("/colores/color/setting")
            While nodes.MoveNext()
                ' El nombre de la definición (si se ha navegado con /colores/color)
                'Dim nombre As String = nodes.Current.GetAttribute("name", "")
                'Dim descrip As String = nodes.Current.GetAttribute("description", "")

                'nodes.Current.MoveToFirstChild()
                If nodes.Current.Name.ToLower = "setting" Then
                    Dim setName As String = nodes.Current.GetAttribute("name", "")
                    Dim setVal As String = nodes.Current.Value
                    Dim esta As Boolean
                    ' Asignarlo...
                    esta = False
                    For i As Integer = 0 To nombresFondos.Length - 1
                        If setName = nombresFondos(i) Then
                            fondos(i) = ColoresGradient.Parse(setVal)
                            esta = True
                            Exit For
                        End If
                    Next
                    If esta = False Then
                        For i As Integer = 1 To nombresTextos.Length - 1
                            If setName = nombresTextos(i) Then
                                colorTextos(i) = New SolidColorBrush( _
                                                CType(ColorConverter.ConvertFromString(setVal), Color))
                                Exit For
                            End If
                        Next
                    End If
                End If
            End While
            ' asignar los colores
            asignarValoresControles()
            asignarColoresVentanaPrincipal()

        Catch ex As Exception
            MessageBox.Show("Error al leer el fichero de colores" & _
                            vb.vbCrLf & _
                            "Es posible que el formato no sea XML correcto.", _
                            "Leer fichero de colores", _
                             MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End Try
    End Sub

    Private Sub leerInfoFicColores(ByVal fic As String, ByRef nombre As Object, ByRef descrip As Object)
        Try
            Dim docXml As New System.Xml.XmlDocument

            docXml.Load(fic)

            ' Buscar los valores de cada configuración

            Dim navigator As XPathNavigator = docXml.CreateNavigator()

            'Si hay más de una definicion de colores, solo se lee la primera

            Dim nodes As XPathNodeIterator = navigator.Select("/colores/color")
            While nodes.MoveNext()
                ' El nombre de la definición (si se ha navegado con /colores/color)
                nombre = nodes.Current.GetAttribute("name", "")
                descrip = nodes.Current.GetAttribute("description", "")
                Exit While
            End While

        Catch ex As Exception
            MessageBox.Show("Error al leer el fichero de colores" & _
                            vb.vbCrLf & _
                            "Es posible que el formato no sea XML correcto.", _
                            "Leer fichero de colores", _
                             MessageBoxButton.OK, MessageBoxImage.Exclamation)
            'Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' Leer los ficheros de colores del usuario y crear los menús
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub leerFicherosColores()
        'With My.Computer.FileSystem
        ' Los del usuario
        mnuColoresUser.Items.Clear()

            Dim fics() As String = Directory.GetFiles(dirDefColores, _
                                                      "*.user.colorDef.xml", _
                                                      SearchOption.TopDirectoryOnly)

        For Each fic As String In fics
            Dim fi As New FileInfo(fic)
            Dim mnui As New MenuItem
            leerInfoFicColores(fic, mnui.Header, mnui.ToolTip)
            mnui.Tag = fi.FullName
            AddHandler mnui.Click, AddressOf mnuAbrirFicColores_Click
            mnuColoresUser.Items.Add(mnui)
        Next
        If mnuColoresUser.Items.Count = 0 Then
                ' No hay datos, añadir uno en blanco
                Dim mnui As New MenuItem
                mnui.Header = "(vacío)"
                mnui.IsEnabled = False
                mnuColoresUser.Items.Add(mnui)
            Else
            ' Clasificar los elementos del menú por el Header
            mnuColoresUser.Items.SortDescriptions.Add(New SortDescription("Header",
                                                                              ListSortDirection.Ascending))
        End If
        'End With
    End Sub

    ''' <summary>
    ''' Leer los ficheros de colores de la aplicación
    ''' (estarán en el directorio Colores dentro del directorio de la aplicación
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub leerFicColoresApp()
        Dim dirAppColores As String

        'With My.Application.Info
        '    dirAppColores = .DirectoryPath & "\Colores"
        'End With
        'dirAppColores = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.Location) & "\Colores"
        dirAppColores = cIniArray.AppPath(False) & "\Colores"

        'With My.Computer.FileSystem

        ' Los ficheros de la aplicación
        ' No borrarlos porque ya hay algunos definidos
        'mnuColoresApp.Items.Clear()
        Dim fics() As String = Directory.GetFiles(dirAppColores, _
                                                      "*.app.colorDef.xml", _
                                                      SearchOption.TopDirectoryOnly)

            '.GetFiles(dirAppColores, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.app.colorDef.xml")

            Dim mnuTmp As New MenuItem

            For Each fic As String In fics

                Dim fi As New FileInfo(fic)
                Dim mnui As New MenuItem
                leerInfoFicColores(fic, mnui.Header, mnui.ToolTip)
                mnui.Tag = fi.FullName
                'AddHandler mnui.Click, AddressOf mnuAbrirFicColores_Click
                'mnuColoresApp.Items.Add(mnui)
                mnuTmp.Items.Add(mnui)
            Next
            If mnuTmp.Items.Count = 0 Then
                ' No hay datos, añadir uno en blanco
                Dim mnui As New MenuItem
                mnui.Header = "(vacío)"
                mnui.IsEnabled = False
                mnuColoresApp.Items.Add(mnui)
            Else
            ' Clasificar los elementos del menú por el Header
            ' (falla si hay separator)

            ' Y no se puede añadir directamente,                (21/Ago/07)
            ' porque dice que ya tiene padre... o casi...
            ' así que... crearlos nuevamente
            mnuTmp.Items.SortDescriptions.Add(New SortDescription("Header",
                                                                      ListSortDirection.Ascending))
            For i = 0 To mnuTmp.Items.Count - 1
                    Dim mnui As MenuItem = TryCast(mnuTmp.Items(i), MenuItem)
                    Dim mnui2 As New MenuItem
                    mnui2.Header = mnui.Header
                    mnui2.Tag = mnui.Tag
                    mnui2.ToolTip = mnui.ToolTip
                    AddHandler mnui2.Click, AddressOf mnuAbrirFicColores_Click
                    mnuColoresApp.Items.Add(mnui2)
                Next
            End If

        'End With

    End Sub

    Private Sub mnuAbrirFicColores_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ' Abrir el fichero indicado en el Tag del menú
        Dim mnui As MenuItem = TryCast(sender, MenuItem)
        If mnui Is Nothing Then Exit Sub

        leerColores(mnui.Tag.ToString)
    End Sub
End Class

