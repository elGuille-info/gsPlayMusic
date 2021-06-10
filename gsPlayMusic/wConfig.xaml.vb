'------------------------------------------------------------------------------
' Configuración para gsPlayWPF                                      (23/Ago/07)
' Creado completamente con código WPF
'
' ©Guillermo 'guille' Som, 2007
'------------------------------------------------------------------------------
Option Strict On

Imports vb = Microsoft.VisualBasic

Imports System
Imports System.Windows
Imports System.Windows.Input

Imports System.Windows.Controls
Imports Microsoft.Win32

Imports System.Collections.Generic
Imports System.IO
Imports System.ComponentModel

Partial Public Class wConfig

    Private colFicColores As New List(Of FileInfo)

    Private dirDefColores As String
    Private inicializando As Boolean = True

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.


        iniciarApp()

    End Sub

    'Private DirResources As String

    Private Sub iniciarApp()
        Me.btnRestablecer.IsEnabled = False

        'With My.Computer.FileSystem
        '    dirDefColores = .SpecialDirectories.MyDocuments & "\gsPlay\Colores"

        '    Dim di As New DirectoryInfo(dirDefColores)
        '    If di.Exists Then
        '        Dim fis() As FileInfo = di.GetFiles(
        '                                            "*.user.colorDef.xml",
        '                                            SearchOption.TopDirectoryOnly)
        '        colFicColores.AddRange(fis)
        '    End If
        'End With

        'Dim dirApp = cIniArray.AppPath(False)
        'DirResources = System.IO.Path.Combine(dirApp, "Resources")

        Icon = New BitmapImage(New Uri($"{Application.DirResources}/_AudioPropertiesHS.png"))

        dirDefColores = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\gsPlay\Colores"
        Dim di As New DirectoryInfo(dirDefColores)
        If di.Exists Then
            Dim fis() As FileInfo = di.GetFiles("*.user.colorDef.xml", SearchOption.TopDirectoryOnly)
            colFicColores.AddRange(fis)
        End If

        lvListas.SelectionMode = SelectionMode.Extended

        With lvFicColores
            .DisplayMemberPath = "Name"
            .SelectionMode = SelectionMode.Extended
        End With
        asignarValores()

        inicializando = False

        ' Actualizar el tamaño de los ListView
        'wConfig_SizeChanged(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' El título de la ventana de configuración
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Titulo() As String
        Get
            Return Me.Title
        End Get
        Set(ByVal value As String)
            Me.Title = value
        End Set
    End Property


    ''' <summary>
    ''' Asignar los valores de la configuración
    ''' </summary>
    ''' <remarks>
    ''' 08/Mar/2007
    ''' </remarks>
    Private Sub asignarValores()
        lvFicColores.Items.Clear()
        For Each fi As FileInfo In colFicColores
            lvFicColores.Items.Add(fi)
        Next

        lvListas.Items.Clear()
        For Each s As String In My.Settings.Listas
            lvListas.Items.Add(s)
        Next
        Me.chkCalcularTiempoLista.IsChecked = My.Settings.CalcularTiempo
        Me.chkMezclar.IsChecked = My.Settings.MezclarCanciones
        Me.chkMinimizarEnTaskBar.IsChecked = My.Settings.MinimizarTaskBar
        Me.chkRepeat.IsChecked = My.Settings.Repeat
        Me.chkShuffle.IsChecked = My.Settings.Shuffle
        Me.txtIntervaloRemoto.Text = My.Settings.IntervaloRemoto.ToString
        Me.txtMezclar.Text = My.Settings.MilisegundosMezcla.ToString
        Me.chkMostrarInfo.IsChecked = My.Settings.InfoCompleta
        Me.chkInfoUnaLinea.IsChecked = My.Settings.InfoUnaLinea

        Me.btnRestablecer.IsEnabled = False
    End Sub

    ''' <summary>
    ''' Comprobar si se han cambiado los datos
    ''' para habilitar adecuadamente el botón restablecer
    ''' </summary>
    ''' <remarks>
    ''' 08/Mar/2007
    ''' </remarks>
    Private Sub datosCambiados()
        If inicializando Then Exit Sub

        Dim b As Boolean = False

        If Me.chkInfoUnaLinea.IsChecked <> My.Settings.InfoUnaLinea Then
            b = True
        End If
        If Me.chkMostrarInfo.IsChecked <> My.Settings.InfoCompleta Then
            b = True
        End If
        If Me.chkCalcularTiempoLista.IsChecked <> My.Settings.CalcularTiempo Then
            b = True
        End If
        If Me.chkMezclar.IsChecked <> My.Settings.MezclarCanciones Then
            b = True
        End If
        If Me.chkMinimizarEnTaskBar.IsChecked <> My.Settings.MinimizarTaskBar Then
            b = True
        End If
        If Me.chkRepeat.IsChecked <> My.Settings.Repeat Then
            b = True
        End If
        If Me.chkShuffle.IsChecked <> My.Settings.Shuffle Then
            b = True
        End If
        If Me.txtIntervaloRemoto.Text <> My.Settings.IntervaloRemoto.ToString Then
            b = True
        End If
        If Me.txtMezclar.Text <> My.Settings.MilisegundosMezcla.ToString Then
            b = True
        End If

        ' La comprobación de la lista,
        ' solo hacerla si no se han cambiado los datos,
        ' con idea de que no se tarde más de la cuenta
        If b = False Then
            ' Primero comprobar el número de elementos
            If Me.lvListas.Items.Count <> My.Settings.Listas.Count Then
                b = True
            Else
                ' Si es el mismo número comprobar individualmente
                For Each lvi As String In Me.lvListas.Items
                    If My.Settings.Listas.Contains(lvi) = False Then
                        b = True
                        Exit For
                    End If
                Next
            End If
        End If
        If b = False Then
            If lvFicColores.Items.Count <> colFicColores.Count Then
                b = True
            Else
                For Each lvi As FileInfo In Me.lvFicColores.Items
                    If colFicColores.Contains(lvi) = False Then
                        b = True
                        Exit For
                    End If
                Next
            End If
        End If

        Me.btnRestablecer.IsEnabled = b
    End Sub

    Private Sub btnAceptar_Click(ByVal sender As Object, _
                                 ByVal e As RoutedEventArgs) Handles btnAceptar.Click

        ' Aceptar los cambios (asignar solo si han cambiado)
        If Me.btnRestablecer.IsEnabled Then
            My.Settings.Listas.Clear()
            For Each s As String In Me.lvListas.Items
                My.Settings.Listas.Add(s)
            Next
            My.Settings.CalcularTiempo = Me.chkCalcularTiempoLista.IsChecked.Value
            My.Settings.MezclarCanciones = Me.chkMezclar.IsChecked.Value
            My.Settings.MinimizarTaskBar = Me.chkMinimizarEnTaskBar.IsChecked.Value
            My.Settings.Repeat = Me.chkRepeat.IsChecked.Value
            My.Settings.Shuffle = Me.chkShuffle.IsChecked.Value
            My.Settings.InfoCompleta = Me.chkMostrarInfo.IsChecked.Value
            My.Settings.InfoUnaLinea = Me.chkInfoUnaLinea.IsChecked.Value

            Dim i As Integer
            If vb.IsNumeric(Me.txtIntervaloRemoto.Text) Then
                i = CInt(Me.txtIntervaloRemoto.Text)
                If i < 1500 OrElse i > 20000 Then
                    i = 4000
                End If
                My.Settings.IntervaloRemoto = i
            End If
            If vb.IsNumeric(Me.txtMezclar.Text) Then
                i = CInt(Me.txtMezclar.Text)
                ' De 100 a 3000 milisegundos
                If i < 100 OrElse i > 3000 Then
                    i = 500
                End If
                My.Settings.MilisegundosMezcla = i
            End If
            ' Eliminar los ficheros que no estén en la lista
            For Each fi As FileInfo In colFicColores
                If Me.lvFicColores.Items.Contains(fi) = False Then
                    ' Eliminarlo
                    fi.Delete()
                End If
            Next
        End If

        Me.DialogResult = True ' System.Windows.Forms.DialogResult.OK

    End Sub

    Private Sub btnCancelar_Click(ByVal sender As Object, _
                                  ByVal e As RoutedEventArgs) Handles btnCancelar.Click
        Me.DialogResult = False
    End Sub

    Private Sub btnRestablecer_Click(ByVal sender As Object, _
                                     ByVal e As RoutedEventArgs) Handles btnRestablecer.Click
        inicializando = True
        asignarValores()
        inicializando = False
    End Sub

    Private Sub lvListas_ColumHeaderClick(ByVal sender As Object, _
                                          ByVal e As RoutedEventArgs)

        ' Clasificar el contenido del ListView                      (24/Ago/07)
        Dim cabecera As GridViewColumnHeader = TryCast(e.OriginalSource, GridViewColumnHeader)
        If cabecera Is Nothing Then Exit Sub

        ' Si es una columna "de relleno", salir
        If cabecera.Role = GridViewColumnHeaderRole.Padding Then
            Exit Sub
        End If

        Static sortDir As ListSortDirection = ListSortDirection.Ascending
        If sortDir = ListSortDirection.Ascending Then
            sortDir = ListSortDirection.Descending
        Else
            sortDir = ListSortDirection.Ascending
        End If

        ' Como es una cadena, no usar nombre de propiedad
        'Window1.VentanaPrincipal.ClasificarLista(lvListas.Items, "", sortDir)
        clasificarListView(lvListas.Items, "", sortDir, cabecera)
    End Sub

    Private Sub lvFicColores_ColumHeaderClick(ByVal sender As Object, _
                                              ByVal e As RoutedEventArgs)

        ' Clasificar el contenido del ListView                      (24/Ago/07)
        Dim cabecera As GridViewColumnHeader = TryCast(e.OriginalSource, GridViewColumnHeader)
        If cabecera Is Nothing Then Exit Sub

        ' Si es una columna "de relleno", salir
        If cabecera.Role = GridViewColumnHeaderRole.Padding Then
            Exit Sub
        End If

        Static sortDir As ListSortDirection = ListSortDirection.Ascending
        If sortDir = ListSortDirection.Ascending Then
            sortDir = ListSortDirection.Descending
        Else
            sortDir = ListSortDirection.Ascending
        End If

        'Window1.VentanaPrincipal.ClasificarLista(lvFicColores.Items, "Name", sortDir)
        clasificarListView(lvFicColores.Items, "Name", sortDir, cabecera)
    End Sub

    ''' <summary>
    ''' Clasificar los elementos del listView y ajustar la imagen de la cabecera
    ''' </summary>
    ''' <param name="items"></param>
    ''' <param name="nombreCab"></param>
    ''' <param name="sortDir"></param>
    ''' <param name="cabecera"></param>
    ''' <remarks>
    ''' 24/Ago/07 v2.0.0.280
    ''' </remarks>
    Private Sub clasificarListView( _
                                   ByVal items As ItemCollection, _
                                   ByVal nombreCab As String, _
                                   ByVal sortDir As ListSortDirection, _
                                   ByVal cabecera As GridViewColumnHeader)

        Window.VentanaPrincipal.ClasificarLista(items, nombreCab, sortDir)

        ' Actualizar el icono de la cabecera
        Dim nombreTemplate As String = ""
        If sortDir = ListSortDirection.Ascending Then
            nombreTemplate = "templateHeaderUp"
        Else
            nombreTemplate = "templateHeaderDown"
        End If

        cabecera.Column.HeaderTemplate = CType(Resources(nombreTemplate), DataTemplate)
    End Sub

    Private Sub btnAddLista_Click(ByVal sender As Object, _
                                  ByVal e As RoutedEventArgs) Handles btnAddLista.Click

        ' Añadir un nuevo fichero de lista
        Dim oFD As New OpenFileDialog
        With oFD
            .Filter = "Lista de canciones (*.m3u)|*.m3u|Todos los archivos (*.*)|*.*"
            .FileName = My.Settings.UltimaLista
            .Multiselect = False
            If .ShowDialog Then
                If lvListas.Items.Contains(.FileName) = False Then
                    lvListas.Items.Add(.FileName)
                    datosCambiados()
                End If
            End If
        End With

    End Sub

    Private Sub btnEliminarLista_Click(ByVal sender As Object, _
                                       ByVal e As RoutedEventArgs) Handles btnEliminarLista.Click
        ' Eliminar los elementos seleccionados
        eliminarSeleccionados(lvListas)
    End Sub

    Private Sub btnEliminarListaColores_Click(ByVal sender As Object, _
                                              ByVal e As RoutedEventArgs) Handles btnEliminarListaColores.Click
        eliminarSeleccionados(lvFicColores)
    End Sub

    Private Sub lvListas_KeyDown(ByVal sender As Object, _
                                 ByVal e As KeyEventArgs) Handles lvListas.KeyDown
        ' Si se pulsa la tecla DEL, borrar el seleccionado
        If e.Key = Key.Delete Then
            eliminarSeleccionados(lvListas)
        End If
    End Sub

    Private Sub lvFicColores_KeyDown(ByVal sender As Object, _
                                     ByVal e As KeyEventArgs) Handles lvFicColores.KeyDown
        ' Si se pulsa la tecla DEL, borrar el seleccionado
        If e.Key = Key.Delete Then
            eliminarSeleccionados(lvFicColores)
        End If
    End Sub

    Private Sub eliminarSeleccionados(ByVal elListView As ListView)
        With elListView
            If .SelectedItems.Count = 0 Then Exit Sub

            Dim i As Integer = .SelectedIndex
            ' Borrar todos los seleccionados
            For j As Integer = .SelectedItems.Count - 1 To 0 Step -1
                .Items.Remove(.SelectedItems(j))
            Next
            ' seleccionar el que estaba seleccionado
            If i >= .Items.Count Then
                i = .Items.Count - 1
            End If
            .SelectedIndex = i
        End With
        datosCambiados()
    End Sub

    Private Sub txtIntervaloRemoto_TextChanged(ByVal sender As Object, _
                                               ByVal e As TextChangedEventArgs) _
                                               Handles txtIntervaloRemoto.TextChanged, _
                                                       txtMezclar.TextChanged
        datosCambiados()
    End Sub

    Private Sub chkCalcularTiempoLista_Click(ByVal sender As Object, _
                                             ByVal e As RoutedEventArgs) _
                                             Handles chkCalcularTiempoLista.Click, _
                                                     chkInfoUnaLinea.Click, _
                                                     chkMezclar.Click, _
                                                     chkMinimizarEnTaskBar.Click, _
                                                     chkMostrarInfo.Click, _
                                                     chkRepeat.Click, chkShuffle.Click
        datosCambiados()
    End Sub

    Private Sub wConfig_SizeChanged(ByVal sender As Object, _
                                    ByVal e As SizeChangedEventArgs) _
                                    Handles Me.SizeChanged
        If inicializando Then Exit Sub

        If Me.WindowState <> System.Windows.WindowState.Minimized Then
            'Me.lvFicColores.Height = 0 ' Me.gridListas.RowDefinitions(0).ActualHeight '+ 40
            'Me.lvListas.Height = 0 'Me.gridListas.RowDefinitions(1).ActualHeight '+ 20

            ' La cantidad que se ponga es igual...
            ' se adapta al tamaño del grid
            Me.lvFicColores.Height = Me.gridColores.Height '- 40
            Me.lvListas.Height = Me.lvFicColores.Height
        End If
    End Sub
End Class
