'------------------------------------------------------------------------------
' Quitar espacios y caracteres de los nombres de los ficheros       (27/Feb/02)
' Se pueden cambiar los nombres de los ficheros
' El fichero seleccionado se puede escuchar (se usa csPlayAvM)
'
' Versión para Visual Basic.NET                                     (27/Abr/02)
' Revisión para usar estilos de Windows XP                          (11/Oct/02)
' (también compilado con la beta 1 de Visual Studio .NET 2003 (Everett)
' Cambio el nombre del control chkCalcularTiempoLista,              (15/Sep/05)
' ya que era chkNocalcularTiempoLista y no concordaba con lo que hacía
'
' Añado el botón de añadir ficheros y cambio algunos iconos         (26/Jun/06)
'
' Versión convertida a Visual Basic 2005                            (20/Oct/06)
' Permite mezclar canciones                                         (21/Oct/06)
'
' Mejoras para el acceso remoto
' Última revisión:  07/Abr/07
' Guarda la lista actual para poder usarla remotamente              (18/Abr/07)
'
' Revisión (en serio) para WinFX                                    (15/Ago/07)
'   Primer intento del diseño del interfaz gráfico fue: 07/Feb/07
'   Con la misma funcionalidad de la aplicación .NET 2.0 y algo más (18/Ago/07)
' Revisión 2.0.0.101                                                (19/Ago/07)
'   Incluyo los formularios que estaban en gsPlayUtil
' Revisiones hasta la 2.0.0.171                                     (21/Ago/07)
'   Toda la funcionalidad de cambiar colores, etc.
'
' 21/Ago/07 v2.0.0.172
'   Cambio el nombre de la aplicación a gsPlayWPF
' 24/Ago/07 v2.0.0.273
'   Todas las ventanas están creadas con WPF
'
' 01/Dic/07 v2.0.1.0
'   Compilado con la versión final de Visual Studio 2008
' 01/Dic/07 v2.0.1.1    Compilado con Strong Name
'
' 01/Abr/10 v2.0.5.0    Mejoras menores
' 25/Feb/14 v2.0.6.0    Compilado con VS2013 y .NET 4.0
' 01/Nov/15 v2.0.7.0    Arreglo de un par de bugs y compilado con VS2015 y .NET 4.0
' 01/Nov/15 v2.1.0.0    Quito la dll de cPlayMCI y compilo con VS2015 y .NET 4.6
'
' 19/Ene/16 v2.1.0.3 
'
' 07/Jun/21 v3.0.0.0    Uso la clase MediaPlayer (no uso cPlayMCI ni fMCICtl)
'                       Compilado con .NET Framework 4.7.2
'                       El MediaPlayer no tiene todas las cosillas del cPlayMCI.
' 08/Jun/21 v3.0.0.2
'
' ©Guillermo 'guille' Som, 2002-2007, 2008, 2010, 2014-2016, 2021
'------------------------------------------------------------------------------
Option Strict On

Imports vb = Microsoft.VisualBasic

Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Imports System.Reflection.Assembly
' Usando un alias                                                   (08/Mar/07)
Imports sysIO = System.IO
' Para la clase Process
Imports System.Diagnostics

'Imports elGuille.gsPlayLib

Imports System.Windows.Input

Imports System.Windows.Threading
Imports System.Timers
Imports System.ComponentModel
Imports System.Collections.Generic

'Imports gsPlayWPF.elGuille.gsPlayLib

Partial Public Class Window

    ' Por si se indica desde la línea de comandos                   (18/Ago/07)
    ' empezar automáticamente a tocar
    ' Lo pongo a nivel de la clase para usarlo en el timer          (24/Ago/07)
    Private esLineaComandos As Boolean = False


    ' Para evitar la reentrada desde el menú contextual             (24/Ago/07)
    ' y habilitar adecuadamente las opciones en el menú contextual

    ''' <summary>
    ''' Para saber si se muestra la ventana de configuración
    ''' (la normal o la de colores)
    ''' </summary>
    ''' <remarks></remarks>
    Private estaConfigurando As Boolean

    ''' <summary>
    ''' Para saber si se está mostrando la ventana de acercaDe
    ''' </summary>
    ''' <remarks></remarks>
    Private estaAcercaDe As Boolean


    '''' <summary>
    '''' Para usar el DoEvents
    '''' </summary>
    '''' <remarks>
    '''' En C#, añadir una referencia a System.Windows.Forms
    '''' y usar DoEvents definido en esa DLL
    '''' </remarks>
    'Private vbApp As New Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()

    ''' <summary>
    ''' Array para los campos de clasificación
    ''' </summary>
    ''' <remarks></remarks>
    Private nombresCampos() As String = {"Nombre", "Directorio", "Tamaño", "Fecha", "Duración", "Nombre completo"}

    ''' <summary>
    ''' Para el orden de clasificación de las columnas del ListView
    ''' </summary>
    ''' <remarks>
    ''' 22/Ago/07
    ''' </remarks>
    Private cabeceraSort As New Dictionary(Of String, ListSortDirection)

    ''' <summary>
    ''' Para las cabeceras del ListView
    ''' </summary>
    ''' <remarks></remarks>
    Private cabecerasGridView As New Dictionary(Of String, GridViewColumnHeader)


    ' Para el valor de la posición predeterminada                   (19/Ago/07)
    Private posNormal As Point
    Private acoplandoVentana As Boolean = False

    ' para ocultar/mostrar los paneles                              (18/Ago/07)
    Private paneles As New System.Collections.Generic.List(Of Expander)

    ' Para saber si los paneles están expandidos o contraidos       (18/Ago/07)
    ' (desde el menú)
    'Private panelesExpandidos As Boolean = False

    ' OJO con estos cuadros de diálogo,
    ' ya que ShowDialog devuelve Boolean
    Private oFD As New Microsoft.Win32.OpenFileDialog
    Private sFD As New Microsoft.Win32.SaveFileDialog

    'Private WithEvents timerHilos As New System.Timers.Timer 'System.Windows.Forms.Timer()
    Private WithEvents timerFeb As New System.Timers.Timer
    ' Para abrir la lista al iniciar
    Private WithEvents timerAbrirLista As New System.Timers.Timer


    Private Const tituloApp As String = "gsPlayMusic"
    Private inicializando As Boolean = True


    ' Para las últimas listas (en los datos de configuración)       (08/Mar/07)
    Private listasUser As System.Collections.Specialized.StringCollection
    ' Para saber si se mezclan las canciones o no                   (21/Oct/06)
    Private mezclarCanciones As Boolean = True
    Private milisegundosMezcla As Integer = 500
    ' Para saber si se usa mPlayMCI o mPlayMCI2                     (21/Oct/06)
    Private esMCI2 As Boolean = False
    ' El objeto cPlayMCI actual                                     (21/Oct/06)
    Private WithEvents mPlayMCIactual As cMediaPlayer ' cPlayMCI

    '' Según se quiera minimizar o restaurar se asignará             (20/Oct/06)
    '' False o True respectivamente.
    'Private esRestaurar As Boolean = False
    ' Tiempo de comprobación de control remoto
    Private intervaloFeb As Integer = 4000

    ' para que al minimizar, si está tocando,                       (13/Feb/05)
    ' se ponga/quite la pausa
    Private _tocandoMinimizado As Boolean
    ' lo pongo fuera para quitarle el WithEvents                    (13/Feb/05)
    'Private notifyIcon1 As New System.Windows.Forms.NotifyIcon

    Private mINI As cIniArray
    Private callado As Boolean = False
    ' Para el control del nivel del volumen
    Private volMax As Integer
    Private volMin As Integer
    Private volAct As Integer
    Private Const cMaxTrack As Integer = 10

    '' Clase para clasificar el ListView                             (29/Jun/02)
    'Private oSorter As New ListViewColumnSort
    '' El último orden de las columnas
    'Private colSorting() As System.Windows.Forms.SortOrder

    Private minimizarEnTaskBar As Integer
    Private sFicIni As String
    Private sNombreLista As String

    Private mTocandoLista As cMediaPlayer.eMCIModo = cMediaPlayer.eMCIModo.eStop
    Private nCancionActual As Integer
    Private pasoScroll As Double
    Private cambiandoScroll As Boolean
    Private cancelarMostrar As Boolean
    Private WithEvents mPlayMCI As cMediaPlayer ' cPlayMCI
    Private WithEvents mPlayMCI2 As cMediaPlayer ' cPlayMCI
    Private mFileName As String

    ' Es que en realidad la primera versión la tuve que hacer en 1998 ó 1999 no el 2002
    ' En realidad este programa está basado en uno que hice en el 2002 para VB6.
    ' Aunque el "precedente" seguramente es de antes del 2002.
    Private Const esElGuilleConst As String = " ©Guillermo Som (elGuille), 2002"
    Private Const esteAño As Integer = 2021 '2007
    Private esElGuille As String = " ©Guillermo 'guille' Som, 2002-" & esteAño.ToString
    Private elGuille2 As String
    Private ultimaCancion As Integer

    ''' <summary>
    ''' Para las columnas del ListView
    ''' (no se usa en la versión para WinFX)
    ''' </summary>
    ''' <remarks></remarks>
    Private Enum columnasLV
        Nombre
        Duración
        FechaHora
        NombreCompleto
        Directorio
    End Enum

    ' Menú contextual para el icono de notificación y la ventana principal
    ' Cambio el menú contextual del icono al tipo ContextMenuStrip  (21/Ago/07)
    'Private contextNotify As New System.Windows.Forms.ContextMenuStrip ' System.Windows.Forms.ContextMenu
    Private contextMenuForm As New ContextMenu

    ''' <summary>
    ''' Opciones del menú contextual de icono de notificación
    ''' </summary>
    ''' <remarks></remarks>
    Private Enum OpcionesMenuNotify
        Restaurar
        Sep1
        'Tocar
        Pausa
        'Parar
        Sep2
        ReiniciarLista
        Sep3
        'TocarLista
        'PararLista
        'Sep3
        'Primero
        'Anterior
        'Siguiente
        'Último
        'Sep4
        BajarVolumen
        SubirVolumen
        SilencioTotal
        Sep5
        AcercaDe
        Sep6
        Config
        Sep7
        OcultarMinimizar
        Sep8
        Cerrar
    End Enum

    ''' <summary>
    ''' Para acceder a los métodos desde otro hilo 
    ''' </summary>
    ''' <remarks></remarks>
    Public Delegate Sub InvocarCallback()

    'Private dirApp As String

    '
    ' El constructor para llamar al método de iniciar la aplicación
    '

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insert code required on object creation below this point.
        'Me.notifyIcon1.Icon = My.Resources.gsPlayWPF
        'Me.notifyIcon1.Text = tituloApp

        ' Es más rápido y no se nota el cambio de tamaño, etc. si se llama desde aquí

        'dirApp = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.Location)
        'dirApp = cIniArray.AppPath(False)

        ' Controlar los posibles errores no controlados             (24/Ago/07)
        '                                                           v2.0.0.283
        ' Da error de overflow... a ver dónde lo da                 v2.0.0.284
        ' Es en el valor de Left/Top, a ver si ahora...             v2.0.0.285
        Try
            iniciarlizarVentanaPrincipal()
        Catch ex As Exception
            MessageBox.Show("ERROR no controlado" & vb.vbCrLf &
                            "Mensaje: " & ex.Message & vb.vbCrLf &
                            "TargetSite: " & ex.TargetSite.Name & vb.vbCrLf &
                            "StackTrace: " & ex.StackTrace,
                            "gsPlayMusic",
                            MessageBoxButton.OK, MessageBoxImage.Error)
            ' Salir
            Application.Current.Shutdown(-1)
        End Try

    End Sub


    '--------------------------------------------------------------------------
    ' Los métodos privados (no de eventos)
    '--------------------------------------------------------------------------

    Private Property AppTitle As String
    'Private DirResources As String

    ''' <summary>
    ''' Iniciarlizar la ventana principal
    ''' Todo en un método para llamarlo desde el constructor
    ''' en lugar desde el evento Load
    ''' </summary>
    ''' <remarks>
    ''' 20/Ago/07 (v2.0.0.161)
    ''' </remarks>
    Private Sub iniciarlizarVentanaPrincipal()
        ' Las asignaciones que no se han hecho en el diseño         (18/Ago/07)
        expMenu.Background = CType(New BrushConverter().ConvertFromString(SystemColors.MenuBarColor.ToString), Brush)
        'DirResources = System.IO.Path.Combine(dirApp, "Resources")

        'picVol.Source = New BitmapImage(New Uri($"{DirResources}/volumenOn.png"))
        'picVol.Source = New BitmapImage(New Uri("gsPlayMusic;component/Resources/volumenOn.png"))
        picVol.Source = New BitmapImage(New Uri($"{Application.DirResources}/volumenOn.png"))
        Icon = New BitmapImage(New Uri($"{Application.DirResources}/gsPlayMusic.png"))

        Dim i, j As Integer
        Dim s As String

        ' Asignar a la propiedad FlatStyle el valor System          (11/Oct/02)
        ' para que sean compatibles con los temas de XP
        'CambiarEstiloXP.CambiarEstilo(Me)

        mINI = New cIniArray
        sFicIni = cIniArray.AppPath(True) & GetExecutingAssembly.GetName.Name & ".ini"

        ' Si no existe el fichero INI                               (21/Ago/07)
        ' no acceder a los valores de la posición del formulario
        'Dim bExisteIni As Boolean = System.IO.File.Exists(sFicIni)


        ' Usar "single instance" de la configuración                (27/Mar/07)
        ' y usar los eventos de la aplicación
        ' para que se restaure al iniciar otra instancia.
        '
        '' Comprobar si ya hay otra instancia funcionando            (02/Ene/07)
        ''
        '' Con esto otro solo funcionará si no se ha cerrado brúscamente
        '' i = CInt(mINI.IniGet(sFicIni, "General", "Esta funcionando", "0"))
        'If Process.GetProcessesByName(Process.GetCurrentProcess.ProcessName).Length > 1 Then
        '    Application.Exit()
        'End If

        tocandoLista = cMediaPlayer.eMCIModo.eStop

        ' guardar los valores a usar mientras está funcionando      (28/Feb/05)
        ' por si se quiere controlar remotamente
        mINI.IniWrite(sFicIni, "Control remoto", "Pausa", "0 ; como si hiciéramos click en pausa (pausa o reanuda)")
        mINI.IniWrite(sFicIni, "Control remoto", "TerminarActual", "0 ; Para la actual, pero no la lista")
        mINI.IniWrite(sFicIni, "Control remoto", "TerminarLista", "0 ; Para la lista (o la actual, es decir, deja de tocar)")
        mINI.IniWrite(sFicIni, "Control remoto", "Restaurate", "0 ; Para restaurar el programa")
        mINI.IniWrite(sFicIni, "Control remoto", "TocarLista", "0 ; Inicia la lista")
        mINI.IniWrite(sFicIni, "Control remoto", "Subir volumen", "0 ; Subir el volumen")
        mINI.IniWrite(sFicIni, "Control remoto", "Bajar volumen", "0 ; Bajar el volumen")
        mINI.IniWrite(sFicIni, "Control remoto", "Tocando lista", tocandoLista.ToString)
        mINI.IniWrite(sFicIni, "Control remoto", "ReiniciarLista", "0 ; Reinicia la lista")
        mINI.IniWrite(sFicIni, "Control remoto", "CambiarLista", "0 ; Cambiar la lista")
        ' Por si se cambia la canción de la lista                   (18/Abr/07)
        mINI.IniWrite(sFicIni, "Control remoto", "Cambiar cancion", "0")
        mINI.IniWrite(sFicIni, "Control remoto", "Numero cancion", "-1")

        mINI.IniWrite(sFicIni, "General", "Tocando", "")
        ' El estado de los botones
        btnTocar.IsEnabled = True
        btnPausa.IsEnabled = False
        btnParar.IsEnabled = False
        Me.btnPararLista.IsEnabled = False

        ' La última posición de la ventana                          (21/Oct/06)
        ' La posición de la ventana                                 (25/Nov/06)
        ' obtenerla de los datos del usuario local.
        ' Si el valor de FormLeft es -1, usar el fichero de configuración.
        j = My.Settings.FormLeft
        If j >= 0 Then
            Me.Left = My.Settings.FormLeft
            Me.Top = My.Settings.FormTop
        End If
        ' Comprobaciones extras
        If j < 0 Then j = 0
        'If j > System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width Then j = 0
        Me.Left = j
        j = My.Settings.FormTop
        If j < 0 Then j = 0
        'If j > System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height Then j = 0
        Me.Top = j

        ' Asignar los valores, ya que se usan más abajo
        My.Settings.FormLeft = CInt(Me.Left)
        My.Settings.FormTop = CInt(Me.Top)

        mPlayMCI = New cMediaPlayer ' cPlayMCI
        mPlayMCI2 = New cMediaPlayer ' cPlayMCI
        mPlayMCIactual = mPlayMCI

        ' Cuando sea superior a 2002, ponerlo como está comentado   (11/Oct/02)
        esElGuille = esElGuilleConst ' " ©Guillermo 'guille' Som, 2002"
        i = DateTime.Now.Year
        If i < esteAño Then
            esElGuille &= "-" & esteAño.ToString()
        Else
            esElGuille &= "-" & i.ToString
        End If

        ' Para mostrar el valor de FileVersion                      (19/Ago/07)
        Dim fvi As System.Diagnostics.FileVersionInfo
        fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(GetExecutingAssembly.Location)

        'With My.Application.Info
        '    esElGuille &= " (" & .Title & " v" & .Version.ToString
        '    'elGuille2 = esElGuille & " - " & .ProductName & ")"
        '    elGuille2 = esElGuille & " - rev." & fvi.FileVersion & ")"

        '    ' La etiqueta de abajo a la izquierda                   (21/Ago/07)
        '    lblStatusVersion.Content = .Title & " v" & fvi.FileVersion
        '    lblStatusVersion.ToolTip = elGuille2
        'End With

        AppTitle = fvi.ProductName

        esElGuille &= "(" & fvi.ProductName & " v" & fvi.ProductVersion
        elGuille2 = esElGuille & " - rev." & fvi.FileVersion & ")"

        lblStatusVersion.Content = fvi.ProductName & " v" & fvi.FileVersion
        lblStatusVersion.ToolTip = elGuille2

        Me.Title = esElGuille & ")"

        lblStatus.Content = elGuille2

        With lvCanciones
            .AllowDrop = True
            .SelectionMode = SelectionMode.Extended
        End With


        ' La configuración del usuario tiene preferencia            (08/Mar/07)
        ' (los nuevos datos se leerán si MilisegundoMezcla no es -1)
        If My.Settings.MilisegundosMezcla = -1 Then
            ' Leer los datos del fichero INI
            ' para saber si se debe calcular el tiempo total...         (05/May/05)
            chkCalcularTiempoLista.IsChecked = mINI.IniGet(sFicIni, "General", "Calcular Tiempo Total", False)
            Me.chkMezclar.IsChecked = mINI.IniGet(sFicIni, "General", "Mezclar canciones", True)
            milisegundosMezcla = mINI.IniGet(sFicIni, "General", "milisegundosMezcla", 500)
            ' De 100 a 3000 milisegundos
            If milisegundosMezcla < 100 OrElse milisegundosMezcla > 3000 Then
                milisegundosMezcla = 500
            End If

            listasUser = New System.Collections.Specialized.StringCollection
            ' Leer las listas del fichero INI
            j = mINI.IniGet(sFicIni, "Listas", "Total", 0)
            For i = 0 To j - 1
                s = mINI.IniGet(sFicIni, "Listas", "Lista" & i.ToString("00"), "")
                If String.IsNullOrEmpty(s) = False Then
                    listasUser.Add(s)
                End If
            Next

            ' Asignar los valores que se usaron la última vez
            chkRepeat.IsChecked = mINI.IniGet(sFicIni, "Lista", "Repeat", False)
            chkShuffle.IsChecked = mINI.IniGet(sFicIni, "Lista", "Shuffle", False)
            minimizarEnTaskBar = Int32.Parse(mINI.IniGet(sFicIni, "General", "Minimizar en TaskBar", "1"))
            mnuOcultarMini.IsChecked = Convert.ToBoolean(minimizarEnTaskBar)

            ' Activar el temporizador para el control remoto            (28/Feb/05)
            ' Leer el intervalo de comprobación remota                  (25/Nov/06)
            j = mINI.IniGet(sFicIni, "Control remoto", "Intervalo remoto", 4000)
            ' Los valores serán de 1,5 a 20 segundos, predeterminado 4
            If j < 1500 OrElse j > 20000 Then
                j = 4000
            End If
            intervaloFeb = j

            ' Asignar los datos a la configuración
            My.Settings.Listas = listasUser
            My.Settings.Repeat = Me.chkRepeat.IsChecked.Value
            My.Settings.Shuffle = Me.chkShuffle.IsChecked.Value
            My.Settings.CalcularTiempo = chkCalcularTiempoLista.IsChecked.Value
            My.Settings.MinimizarTaskBar = mnuOcultarMini.IsChecked
            My.Settings.MezclarCanciones = chkMezclar.IsChecked.Value
            My.Settings.MilisegundosMezcla = milisegundosMezcla
            My.Settings.IntervaloRemoto = Me.intervaloFeb

            My.Settings.silencioTotal = callado
        Else
            chkCalcularTiempoLista.IsChecked = My.Settings.CalcularTiempo
            chkMezclar.IsChecked = My.Settings.MezclarCanciones
            milisegundosMezcla = My.Settings.MilisegundosMezcla
            ' De 100 a 3000 milisegundos
            If milisegundosMezcla < 100 OrElse milisegundosMezcla > 3000 Then
                milisegundosMezcla = 500
            End If
            listasUser = My.Settings.Listas
            chkRepeat.IsChecked = My.Settings.Repeat
            chkShuffle.IsChecked = My.Settings.Shuffle
            mnuOcultarMini.IsChecked = My.Settings.MinimizarTaskBar
            If mnuOcultarMini.IsChecked Then
                minimizarEnTaskBar = 1
            Else
                minimizarEnTaskBar = 0
            End If
            intervaloFeb = My.Settings.IntervaloRemoto

            callado = My.Settings.silencioTotal
        End If

        ' El estado de los Expander                                 (17/Ago/07)
        ' El Expander del menú hay que asignarlo después de inicializando = False
        'Me.expMenu.IsExpanded = My.Settings.expMenu
        Me.expActual.IsExpanded = My.Settings.expActual
        Me.expListas.IsExpanded = My.Settings.expListas
        Me.expListaActual.IsExpanded = My.Settings.expListaActual
        Me.expOpcionesLista.IsExpanded = My.Settings.expOpcionesLista
        Me.expBotonera.IsExpanded = My.Settings.expBotonera

        ' El tamaño del formulario                                  (18/Ago/07)
        Me.Width = My.Settings.FormWidth
        Me.Height = My.Settings.FormHeight

        ' Los colores                                               (20/Ago/07)
        With My.Settings
            If String.IsNullOrEmpty(My.Settings.fondoVentana) = False Then
                Me.gridMain.Background = ColoresGradient.Parse(.fondoVentana)
                Me.expActual.Background = ColoresGradient.Parse(.fondoActual)
                Me.expActual.Foreground = .letraActual
                Me.expListas.Background = ColoresGradient.Parse(.fondoListas)
                Me.expListas.Foreground = .letraListas
                Me.expListaActual.Background = ColoresGradient.Parse(.fondoListaActual)
                Me.expListaActual.Foreground = .letraListaActual
                Me.expOpcionesLista.Background = ColoresGradient.Parse(.fondoOpcionesListaActual)
                Me.expOpcionesLista.Foreground = .letraOpcionesListaActual
                Me.expBotonera.Background = ColoresGradient.Parse(.fondoBotonera)
                Me.expBotonera.Foreground = .letraBotonera
            End If
        End With

        ' La última lista obtenerla de los datos del usuario        (25/Nov/06)
        ' si no hay ninguna, probar con el fichero de configuración.
        sNombreLista = My.Settings.UltimaLista
        If String.IsNullOrEmpty(sNombreLista) Then
            sNombreLista = mINI.IniGet(sFicIni, "Lista", "Ultima lista", "")
            ' Porque se puede usar más abajo                        (18/Ago/07)
            My.Settings.UltimaLista = sNombreLista
        End If

        ' Asignar las listas desde listasUser                       (08/Mar/07)
        ' Esto hay que asignarlo antes de llamar a guardarINI()
        cboListas.Items.Clear()
        For Each s1 As String In listasUser
            cboListas.Items.Add(s1)
        Next

        guardarINI()

        tocandoLista = cMediaPlayer.eMCIModo.eStop
        btnTocarLista.IsEnabled = False

        With trackVol
            volMax = mPlayMCIactual.VolMax
            ' Si no hay dispositivo de audio, volMax valdrá cero    (18/Nov/06)
            If volMax = 0 Then
                'MessageBox.Show("ERROR: Según parece no hay dispositivo de audio." & vb.vbCrLf &
                '                "Este programa no funcionará hasta que corrijas el error.",
                '                "Error no hay dispositivo de audio", MessageBoxButton.OK,
                '                  MessageBoxImage.Error)
                'System.Windows.Forms.Application.Exit()

                ' Avisar, pero permitir continuar               (10.44 19/ene/16)
                If MessageBox.Show("ERROR: Según parece no hay dispositivo de audio." & vb.vbCrLf &
                                   "El programa puede continuar e intentarlo, " &
                                   "si continúas y no funciona, tendrás que corregir el error." & vb.vbCrLf &
                                   "¿Quieres continuar? (pulsa en NO para detener la aplicación)",
                                   "Error no hay dispositivo de audio", MessageBoxButton.YesNo,
                                   MessageBoxImage.Error) = MessageBoxResult.No Then
                    'System.Windows.Forms.Application.Exit()
                    End
                End If
            End If
            volMin = mPlayMCIactual.VolMin
            .Maximum = cMaxTrack
            .Minimum = 0
            .Value = mPlayMCIactual.Volumen * cMaxTrack \ volMax
        End With

        ' Añadir las opciones al menú contextual                    (17/May/02)
        crearMenusContextuales()

        'notifyIcon1.Visible = True

        Me.lblDuracion.Content = My.Settings.durActual

        ' la última canción que se tocó                             (22/Ene/05)
        ultimaCancion = mINI.IniGet(sFicIni, "Lista", "Ultima cancion", 0)

        '' Por si se indica desde la línea de comandos               (18/Ago/07)
        '' empezar automáticamente a tocar
        'Dim esLineaComandos As Boolean = False

        s = "" 'sNombreLista
        ' Si se ha especificado un fichero en la línea de comandos  (11/Abr/02)
        Try
            If Environment.GetCommandLineArgs.Length > 1 Then
                s = Environment.GetCommandLineArgs(1)
                ultimaCancion = 0
                esLineaComandos = True
            End If
        Catch
            s = "" ' sNombreLista
        End Try
        If String.IsNullOrEmpty(s) = False Then
            'If s.StartsWith(Chr(34)) Then s = s.Substring(1)
            'If s.EndsWith(Chr(34)) Then s = s.Substring(0, s.Length - 1)

            ' quita los espacios y comillas dobles del principio    (21/Dic/02)
            s = s.Trim((" " & vb.ChrW(34)).ToCharArray)

            ' Si no es un acceso directo...
            If vb.InStrRev(s, ".lnk", , vb.CompareMethod.Text) = 0 Then
                sNombreLista = s
            Else
                'C:\Documents and Settings\Guillermo\Desktop\Lista6.m3u.lnk
                'G:\Documents and Settings\Guillermo\Desktop\Lista6.m3u.lnk
                'MessageBox.Show(s & vbCrLf & cPlayMCI.Lnk2Path(s), "Fichero con extensión .lnk", MessageBoxButtons.OK)
                'sNombreLista = cPlayMCI.Lnk2Path(s).Trim
                'sNombreLista = ""
                ' Usar la que estaba guardada
                sNombreLista = My.Settings.UltimaLista
                esLineaComandos = False
            End If
        End If

        If String.IsNullOrEmpty(sNombreLista) = False Then
            ' Abrir esta lista
            ' Abrir la lista desde otro hilo                        (24/Ago/07)
            ' para acelerar la carga sobre todo si se debe leer la duración
            timerAbrirLista.Interval = 100
            timerAbrirLista.Enabled = True

            'abrirLista()
            'If ultimaCancion > 0 AndAlso Me.lvCanciones.Items.Count > ultimaCancion Then
            '    lvCanciones.SelectedIndex = ultimaCancion
            'End If
            'If esLineaComandos Then
            '    Me.btnTocarLista_Click(Nothing, Nothing)
            'End If
        End If

        'TODO: Asignar algunos valores a partir de aquí             (18/Ago/07)
        trackVol.ToolTip = " " & mPlayMCIactual.Volumen.ToString & " (" & trackVol.Value * cMaxTrack & "%)"
        ' Los botones de siguiente, etc. solo si hay canciones en la lista
        actualizarBotones()

        ' Asignar el valor del campo por el que se clasificará
        ItemFichero.CampoClasificar = CType(My.Settings.campoClasificar, ItemFichero.CamposClasificar)
        Me.mnuClasificarFullName.IsChecked = False
        Select Case ItemFichero.CampoClasificar
            Case ItemFichero.CamposClasificar.Directorio
                Me.mnuClasificarDirectorio.IsChecked = True
            Case ItemFichero.CamposClasificar.Duración
                Me.mnuClasificarDuración.IsChecked = True
            Case ItemFichero.CamposClasificar.Fecha
                Me.mnuClasificarFecha.IsChecked = True
            Case ItemFichero.CamposClasificar.Tamaño
                Me.mnuClasificarTamaño.IsChecked = True
            Case ItemFichero.CamposClasificar.Nombre
                Me.mnuClasificarNombre.IsChecked = True
            Case Else
                Me.mnuClasificarFullName.IsChecked = True
        End Select
        If cabeceraSort.ContainsKey(ItemFichero.CampoClasificar.ToString) = False Then
            cabeceraSort.Add(ItemFichero.CampoClasificar.ToString, ListSortDirection.Descending)
        End If
        Dim sortDir As ListSortDirection = cabeceraSort(ItemFichero.CampoClasificar.ToString)
        Dim sOrden As String
        If sortDir = ListSortDirection.Ascending Then
            sOrden = " (descendente)"
        Else
            sOrden = " (ascendente)"
        End If

        Me.btnClasificar.ToolTip = "Clasificar la lista por " &
                                    ItemFichero.CampoClasificar.ToString &
                                    sOrden

        posNormal.X = My.Settings.FormLeft
        posNormal.Y = My.Settings.FormTop

        ' Añadir los expander a la colección                        (18/Ago/07)
        ' para expandirlos o cerrarlos
        ' todos execepto el de la canción actual y el menú
        paneles.Add(Me.expBotonera)
        paneles.Add(Me.expListaActual)
        paneles.Add(Me.expOpcionesLista)
        paneles.Add(Me.expListas)
        'paneles.Add(Me.expMenu)


        Me.timerFeb.Interval = intervaloFeb
        Me.timerFeb.Enabled = True

        ' Indicar que está funcionando                              (25/Oct/06)
        mINI.IniWrite(sFicIni, "General", "Esta funcionando", "1")

        inicializando = False

        ' Para que se muestre u oculte el menú                      (18/Ago/07)
        Me.expMenu.IsExpanded = My.Settings.expMenu

    End Sub

    ''' <summary>
    ''' Método compartido para acceder a esta ventana desde otras ventanas
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared ReadOnly Property VentanaPrincipal() As Window
        Get
            Return CType(Application.Current.MainWindow, Window)
        End Get
    End Property

    ''' <summary>
    ''' Abrir la lista y asignarla al ListView
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub abrirLista()
        ' Abrir la lista indicada en sNombreLista
        Dim oFA As cFileToArray
        Dim aFiles() As String

        If String.IsNullOrEmpty(sNombreLista) Then Exit Sub

        oFA = New cFileToArray

        ' Abrir lista
        If sysIO.File.Exists(sNombreLista) = False Then
            MessageBox.Show("No se puede leer la lista de canciones:" & vb.vbCrLf &
                    sNombreLista, "Abrir lista",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation)
            Exit Sub
        End If
        aFiles = oFA.StringArrayFromFile(sNombreLista)
        ' Aquí se guarda la lista actual                            (18/Abr/07)
        array2Lista(aFiles)

        If Me.lvCanciones.Items.Count > 0 Then
            ' Seleccionar el primero al abrir la lista              (17/Ago/07)
            lvCanciones.SelectedIndex = 0
            ' comprobar si esta lista está en el combo              (28/Nov/04)
            comprobarListasRecientes()
            My.Settings.UltimaLista = sNombreLista
            ' Guardarlo también en el fichero de configuración      (06/Feb/07)
            ' por si se inicia el programa remoto.
            mINI.IniWrite(sFicIni, "Lista", "Ultima lista", sNombreLista)

            ' Se asigna en array2Lista                  v2.0.0.0290 (24/Ago/07)
            '    ' El número de ficheros en esta lista       v2.0.0.0288 (24/Ago/07)
            '    Me.labelListaActual.ToolTip = "Lista con " & Me.lvCanciones.Items.Count & " ficheros"
            'Else
            '    Me.labelListaActual.ToolTip = "Parece que no se ha podido leer la lista"
        End If

        ' Este método se llama desde array2Lista
        'actualizarBotones()

    End Sub

    ''' <summary>
    ''' Actualizar los datos del volumen
    ''' para las opciones de los menús creados con MenuItem de WinFX
    ''' </summary>
    ''' <param name="items"></param>
    ''' <param name="indexBajar"></param>
    ''' <remarks>
    ''' El orden es: bajar, subir y silencio
    ''' </remarks>
    Private Sub actualizarDatosVolumen(ByVal items As ItemCollection, ByVal indexBajar As Integer)
        Dim oBajarVol As MenuItem = TryCast(items(indexBajar), MenuItem)
        Dim oSubirVol As MenuItem = TryCast(items(indexBajar + 1), MenuItem)
        Dim oSilencio As MenuItem = TryCast(items(indexBajar + 2), MenuItem)

        With items
            Dim bV As Integer = CInt(trackVol.Value - trackVol.SmallChange)
            Dim sV As Integer = CInt(trackVol.Value + trackVol.SmallChange)
            If bV < trackVol.Minimum Then
                bV = CInt(trackVol.Minimum)
                oBajarVol.IsEnabled = False
            Else
                oBajarVol.IsEnabled = trackVol.IsEnabled
            End If
            If sV > trackVol.Maximum Then
                sV = CInt(trackVol.Maximum)
                oSubirVol.IsEnabled = False
            Else
                oSubirVol.IsEnabled = trackVol.IsEnabled
            End If
            oBajarVol.Header = "Bajar volumen al " & bV.ToString & "0 %"
            oSubirVol.Header = "Subir volumen al " & sV.ToString & "0 %"

            ' Cambiar la imagen según esté en silencio total o no   (17/Ago/07)
            Dim img As Image = TryCast(oSilencio.Icon, Image)
            If callado Then
                img.Source = New BitmapImage(New Uri($"{Application.DirResources}/volumenOff.ico"))
            Else
                img.Source = New BitmapImage(New Uri($"{Application.DirResources}/volumenOn.ico"))
            End If
        End With

    End Sub

    '''' <summary>
    '''' Actualizar los datos del volumen
    '''' para las opciones de los menús del tipo ToolStripMenuItem
    '''' </summary>
    '''' <param name="items"></param>
    '''' <param name="indexBajar"></param>
    '''' <remarks></remarks>
    'Private Sub actualizarDatosVolumen(
    '                                   ByVal items As System.Windows.Forms.ToolStripItemCollection,
    '                                   ByVal indexBajar As Integer)

    '    Dim oBajarVol As System.Windows.Forms.ToolStripItem = items(indexBajar)
    '    Dim oSubirVol As System.Windows.Forms.ToolStripItem = items(indexBajar + 1)
    '    Dim oSilencio As System.Windows.Forms.ToolStripMenuItem =
    '            TryCast(items(indexBajar + 2), System.Windows.Forms.ToolStripMenuItem)

    '    With items
    '        Dim bV As Integer = CInt(trackVol.Value - trackVol.SmallChange)
    '        Dim sV As Integer = CInt(trackVol.Value + trackVol.SmallChange)
    '        If bV < trackVol.Minimum Then
    '            bV = CInt(trackVol.Minimum)
    '            oBajarVol.Enabled = False
    '        Else
    '            oBajarVol.Enabled = trackVol.IsEnabled
    '        End If
    '        If sV > trackVol.Maximum Then
    '            sV = CInt(trackVol.Maximum)
    '            oSubirVol.Enabled = False
    '        Else
    '            oSubirVol.Enabled = trackVol.IsEnabled
    '        End If
    '        oBajarVol.Text = "Bajar volumen al " & bV.ToString & "0 %"
    '        oSubirVol.Text = "Subir volumen al " & sV.ToString & "0 %"

    '        oSilencio.Checked = callado

    '    End With

    'End Sub


    ''' <summary>
    ''' Actualizar los botones relacionados con la lista
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub actualizarBotones()
        Dim b As Boolean = Me.lvCanciones.Items.Count > 0

        ' Estos dependen de si hay canciones
        Me.btnReiniciarLista.IsEnabled = b
        Me.btnPrimera.IsEnabled = b
        Me.btnUltima.IsEnabled = b
        Me.btnSiguiente.IsEnabled = b
        Me.btnAnterior.IsEnabled = b

        If tocandoLista <> cMediaPlayer.eMCIModo.eStop Then
            Me.btnPararLista.IsEnabled = True
            ' Debe ser False, independientemente del valor de b     (07/Sep/07)
            btnAñadirCanciones.IsEnabled = False ' b
            b = False
        Else
            ' Debe ser True, independientemente del valor de b      (07/Sep/07)
            btnAñadirCanciones.IsEnabled = True ' b
            Me.btnPararLista.IsEnabled = False
        End If

        btnTocarLista.IsEnabled = b
        btnBorrarLista.IsEnabled = b
        ' Para no calcular el tiempo cuando está tocando
        ' pero tampoco cuando la lista esté vacia                   (07/Sep/07)
        If b Then
            btnCalcularTiempo.IsEnabled = btnTocar.IsEnabled
        Else
            btnCalcularTiempo.IsEnabled = False
        End If

        btnSubir.IsEnabled = b
        btnBajar.IsEnabled = b

        btnClasificar.IsEnabled = b

    End Sub

    ''' <summary>
    ''' Actualizar los datos del fichero que se está tocando
    ''' </summary>
    ''' <param name="unPlayMCI"></param>
    ''' <remarks></remarks>
    Private Sub actualizarDatosActual(ByVal unPlayMCI As cMediaPlayer) ' cPlayMCI)
        If WindowState <> WindowState.Minimized Then
            cambiandoScroll = True
            Try
                scrollPosicion.Value = Convert.ToInt32(unPlayMCI.CurrentPosition / pasoScroll)
                With trackVol
                    ' Con la divisón / va bien...
                    .Value = CInt(unPlayMCI.Volumen * cMaxTrack / volMax)
                    .ToolTip = " " & unPlayMCI.Volumen & " (" & trackVol.Value * cMaxTrack & "%)"
                End With
            Catch
                '
            End Try
            cambiandoScroll = False
        End If
    End Sub


    ''' <summary>
    ''' Adaptar el tamaño del ListView al alto de la ventana
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub adaptarTamañoListaCanciones()
        If inicializando Then Exit Sub

        ' El al tamaño máximo de la lista, para que sea visible     (22/Ago/07)
        If Me.WindowState <> System.Windows.WindowState.Minimized Then
            Me.lvCanciones.Height = Me.ActualHeight -
                                      (Me.expActual.ActualHeight + Me.expListas.ActualHeight +
                                       Me.expMenu.ActualHeight + Me.expOpcionesLista.ActualHeight +
                                       Me.gridStatus.ActualHeight + 8 * 12)

        End If
    End Sub


    ''' <summary>
    ''' Asignar los ficheros soltados a la lista de canciones
    ''' </summary>
    ''' <param name="Data"></param>
    ''' <remarks></remarks>
    Private Sub addDrop2List(ByVal Data As IDataObject)
        Dim i As Integer
        Dim aFiles() As String
        Dim aFiles2() As String
        Dim aDataFiles() As String
        Dim s As String
        Dim n As Integer
        Dim oFA As New cFileToArray
        Dim tPlayMCI As New cMediaPlayer
        '
        '--------------------------------------------------------------------------
        ' Si se suelta una lista mp3 (.m3u) o un fichero de texto (.txt),
        ' leer el contenido como si fuese una lista con canciones.
        ' Se tendrán en cuenta todos los ficheros "lista"               (25/Mar/02)
        ' que se hayan soltado, así como los ficheros individuales
        '--------------------------------------------------------------------------
        n = 0
        ReDim aFiles(0)
        ' el primer índice no se usa...
        aFiles(0) = ""

        If Data.GetDataPresent(DataFormats.FileDrop) Then
            aDataFiles = DirectCast(Data.GetData(DataFormats.FileDrop), String())

            For i = 0 To aDataFiles.Length - 1
                s = aDataFiles(i)
                Select Case oFA.FileExtension(s, cFileToArray.eFACase.LowerCaseFA)
                    Case ".m3u", ".txt", ".csl"
                        aFiles2 = oFA.StringArrayFromFile(s)
                        ' Insertar el array en el otro
                        Call oFA.InsertArrayAt(aFiles2, aFiles, (cFileToArray.eFAInsertArray.DespuesDelUltimo))
                    ' Añado dos tipos creados por iTunes        (01/Nov/15 13.25)
                    Case ".wav", ".mp3", ".wma", ".m4a", ".m4r"
                        n = aFiles.GetUpperBound(0) + 1
                        ReDim Preserve aFiles(n)
                        aFiles(n) = s
                    Case Else
                        ' En teoría no hace falta hacer comprobación de error (01/Nov/15)
                        tPlayMCI.FileName = s
                        If cMediaPlayer.Duracion(s) > 0 Then
                            n = aFiles.GetUpperBound(0) + 1
                            ReDim Preserve aFiles(n)
                            aFiles(n) = s
                        End If
                End Select
            Next
            ' Aquí se guarda la lista actual
            ' hacer que se clasifiquen                              (18/Dic/07)
            array2Lista(aFiles, True)
        End If
    End Sub

    ''' <summary>
    ''' Asigna el array de nombres de ficheros al ListView
    ''' </summary>
    ''' <param name="aFiles"></param>
    ''' <param name="clasificar">
    ''' Opcional, si se debe clasificar el contenido de la lista antes de añadirla.
    ''' Por defecto es False.
    ''' </param>
    ''' <remarks></remarks>
    Private Sub array2Lista(ByVal aFiles() As String, Optional ByVal clasificar As Boolean = False)
        Dim i, j As Integer
        Dim oFile As sysIO.FileInfo
        ' para los totales de tiempo
        Dim t1 As Integer
        Dim tot As Integer

        ' Al usarse con While, no restar uno                        (18/Dic/07)
        Try
            j = aFiles.Length '- 1
        Catch
            j = 0 ' -1
        End Try

        ' Si se debe clasificar                                     (18/Dic/07)
        ' ya que al seleccionar múltiples ficheros no los deja en orden
        If clasificar Then
            lblStatus.Content = "Clasificando la lista..."
            Array.Sort(aFiles)
        End If

        ' Usando un bucle While en vez de For                       (18/Dic/07)
        i = 0

        ' Recorrer todos los elementos y saltarse los vacíos        (22/Ago/08)
        ' mejor con un bucle Do/Loop While
        Do
            If String.IsNullOrEmpty(aFiles(i)) Then
                ' Incrementar antes de seguir con el bucle          (01/Nov/15)
                ' ¡¡¡NO INCREMENTABA!!!
                ' y se quedaba colgado... ¡joorrrr!
                i += 1
                Continue Do
            End If

            ' Por si se produce algún error...                      (25/Ene/04)
            Try
                ' Comprobar si existe el fichero                    (11/Abr/02)
                oFile = New sysIO.FileInfo(aFiles(i))
                If oFile.Exists Then
                    Dim itf As New ItemFichero(oFile)

                    lvCanciones.Items.Add(itf)
                    If chkCalcularTiempoLista.IsChecked Then
                        itf.Duración = cMediaPlayer.DuracionStr(oFile.FullName, t1)
                        tot += t1
                    End If
                End If
            Catch 'ex As Exception
            End Try

            i += 1
        Loop While i < j

        '' Es posible que el primer índice no se use                 (16/Ago/08)
        'If String.IsNullOrEmpty(aFiles(0)) Then i = 1
        ''
        'While i < j AndAlso String.IsNullOrEmpty(aFiles(i)) = False
        '    ' Por si se produce algún error...                      (25/Ene/04)
        '    Try
        '        ' Comprobar si existe el fichero                    (11/Abr/02)
        '        oFile = New sysIO.FileInfo(aFiles(i))
        '        If oFile.Exists Then
        '            Dim itf As New ItemFichero(oFile)

        '            lvCanciones.Items.Add(itf)
        '            If chkCalcularTiempoLista.IsChecked Then
        '                itf.Duración = cPlayMCI.DuracionStr(oFile.FullName, t1)
        '                tot += t1
        '            End If
        '        End If
        '    Catch 'ex As Exception
        '    End Try
        '    '
        '    i += 1
        'End While

        ' Si hay elementos en la lista, habilitar el botón de tocar
        If lvCanciones.Items.Count > 0 Then
            btnTocarLista.IsEnabled = True
            ' Esto es por si se debe deshabilitar
            tocandoLista = tocandoLista
            guardarListaActual()
        End If

        ' Mostrar el número de ficheros
        If chkCalcularTiempoLista.IsChecked Then
            lblStatus.Content = String.Format(" Hay {0} ficheros, duración total: {1}.",
                                              lvCanciones.Items.Count.ToString,
                                              milisegundos2Display(tot))
        Else
            lblStatus.Content = String.Format(" Hay {0} ficheros, duración total: (no calculada).",
                                              lvCanciones.Items.Count.ToString)
        End If

        ' Asignar la info al tooltip de la etiqueta     v2.0.0.0290 (24/Ago/07)
        Me.labelListaActual.ToolTip = lblStatus.Content.ToString '"Lista con " & Me.lvCanciones.Items.Count & " ficheros"

        actualizarBotones()
    End Sub

    ''' <summary>
    ''' Clasifica los elementos de la lista indicada
    ''' </summary>
    ''' <param name="items">
    ''' Elementos de la lista que se va a clasificar 
    ''' </param>
    ''' <param name="propiedad">
    ''' Nombre la propiedad por la que se va a clasificar
    ''' </param>
    ''' <param name="sortDir">
    ''' El orden de clasificación a usar
    ''' </param>
    ''' <remarks></remarks>
    Public Sub ClasificarLista(ByVal items As ItemCollection,
                               ByVal propiedad As String,
                               ByVal sortDir As ListSortDirection)

        ' Clasificar los elementos según los parámetros

        ' Eliminar las definiciones de clasificación que hubiera
        items.SortDescriptions.Clear()

        items.SortDescriptions.Add(New SortDescription(propiedad, sortDir))

    End Sub


    ''' <summary>
    ''' Clasificar el listView por la cabecera indicada
    ''' </summary>
    ''' <param name="nombreCab"></param>
    ''' <remarks>
    ''' Además se ajusta el orden de clasificación, etc.
    ''' </remarks>
    Private Sub clasificarListView(ByVal nombreCab As String, ByVal cabecera As GridViewColumnHeader)
        If Me.cabeceraSort.ContainsKey(nombreCab) = False Then
            cabeceraSort.Add(nombreCab, ListSortDirection.Descending)
        End If
        Dim sortDir As ListSortDirection = cabeceraSort(nombreCab)

        ' Invertir el orden de la última clasificación
        If sortDir = ListSortDirection.Ascending Then
            sortDir = ListSortDirection.Descending
        Else
            sortDir = ListSortDirection.Ascending
        End If

        Dim sOrden As String
        If sortDir = ListSortDirection.Ascending Then
            sOrden = "(descendente)"
        Else
            sOrden = "(ascendente)"
        End If
        Me.btnClasificar.ToolTip = "Clasificar la lista por " &
                            ItemFichero.CampoClasificar.ToString &
                            " " & sOrden


        ' Guardarlo para la próxima
        cabeceraSort(nombreCab) = sortDir

        ' Actualizar el menú
        For Each mnu As MenuItem In btnClasificar.ContextMenu.Items
            mnu.IsChecked = False
        Next
        Select Case nombreCab
            Case ItemFichero.CamposClasificar.Nombre.ToString
                Me.mnuClasificarNombre.IsChecked = True
            Case ItemFichero.CamposClasificar.Directorio.ToString
                Me.mnuClasificarDirectorio.IsChecked = True
            Case ItemFichero.CamposClasificar.Duración.ToString
                Me.mnuClasificarDuración.IsChecked = True
            Case ItemFichero.CamposClasificar.Fecha.ToString
                Me.mnuClasificarFecha.IsChecked = True
            Case ItemFichero.CamposClasificar.FullName.ToString
                Me.mnuClasificarFullName.IsChecked = True
            Case ItemFichero.CamposClasificar.Tamaño.ToString
                Me.mnuClasificarTamaño.IsChecked = True
        End Select

        Dim img As Object = btnClasificar.Content
        btnClasificar.Content = "..."
        Me.lblStatus.Content = "Clasificando " & lvCanciones.Items.Count &
                               " canciones por " & nombreCab & "..."
        DoEvents()

        ' Clasificar los elementos
        ClasificarLista(lvCanciones.Items, nombreCab, sortDir)

        ' Asignar la imagen correspondiente al valor de clasificación
        ' tener en cuenta que hay columnas que se muestran a la derecha
        Dim nombreTemplate As String = ""
        Dim sRight As String = ""
        Select Case nombreCab
            Case "Fecha", "Tamaño"
                sRight = "Right"
            Case Else
                sRight = ""
        End Select
        If sortDir = ListSortDirection.Ascending Then
            nombreTemplate = "templateHeaderUp"
        Else
            nombreTemplate = "templateHeaderDown"
        End If
        If cabecera Is Nothing Then
            ' Ha entrado por el botón de clasificar
            ' asignar la cabecera adecuada a la columna a clasificar
            Dim sCab As String = nombreCab
            If sCab = "FullName" Then sCab = "Nombre completo"
            Dim gridV As GridView = TryCast(lvCanciones.View, GridView)
            For Each c As GridViewColumn In gridV.Columns
                If c.Header.ToString = sCab Then
                    c.HeaderTemplate = CType(Resources(nombreTemplate & sRight), DataTemplate)
                    Exit For
                End If
            Next
        Else
            cabecera.Column.HeaderTemplate = CType(Resources(nombreTemplate & sRight), DataTemplate)
        End If

        btnClasificar.Content = img
        Me.lblStatus.Content = Me.lblStatus.Tag
    End Sub


    ''' <summary>
    ''' Pasa al siguiente fichero que esté seleccionado
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub comprobarLista()
        Static yaEstoy As Boolean
        '
        If yaEstoy = False Then
            yaEstoy = True
            ' Si se está tocando la lista, pasar al seleccionado
            If tocandoLista <> cMediaPlayer.eMCIModo.eStop Then
                btnTocar_Click(btnTocar, Nothing)
            End If
            yaEstoy = False
        End If
    End Sub

    ''' <summary>
    ''' Comprobar si la lista actual está en el combo.
    ''' </summary>
    ''' <remarks>
    ''' Debido a que en WinFX el combo no tiene FindStringExact,
    ''' lo que hago es asignar el texto al combo y si ese fichero está
    ''' el valor devuelto por SelectedIndex será la posición o -1 si no está.
    ''' </remarks>
    Private Sub comprobarListasRecientes()
        If String.IsNullOrEmpty(sNombreLista) Then
            Exit Sub
        End If

        ' Si la lista está vacía, no buscar, asignarlo directamente (17/Ago/07)
        If cboListas.Items.Count = 0 Then
            cboListas.Items.Add(sNombreLista)
            cboListas.Text = sNombreLista
            Exit Sub
        End If

        ' Buscar en el combo por el nombre de la lista              (16/Ago/07)
        ' Como en WinFX no hay FindString, asignar el texto para que se busque solo
        cboListas.Text = sNombreLista
        Dim i As Integer = cboListas.SelectedIndex ' .FindStringExact(sNombreLista)
        If i = -1 Then
            ' añadirla
            cboListas.Items.Add(sNombreLista)
            cboListas.Text = sNombreLista
            ' Faltaba esta asignación                               (18/Ago/07)
            cboListas.SelectedIndex = cboListas.Items.Count - 1
        Else
            cboListas.SelectedIndex = i
        End If
    End Sub

    ''' <summary>
    ''' Crear los menús contextuales
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub crearMenusContextuales()
        ' Creo también para el formulario (ventana),                (16/Ago/07)
        ' ya que son distintos tipos
        With contextMenuForm
            Dim mni As MenuItem
            Dim img As Image

            mni = New MenuItem
            mni.Header = "Mostrar"
            AddHandler mni.Click, New RoutedEventHandler(AddressOf mostrarRestaurar)
            .Items.Add(mni)
            .Items.Add(New Separator)

            mni = New MenuItem
            mni.Header = "Pausa"
            img = New Image
            img.Style = CType(Me.Resources("imgMenu"), Style)
            img.Source = New BitmapImage(New Uri($"{Application.DirResources}/PauseHS.png"))
            mni.Icon = img
            AddHandler mni.Click, New RoutedEventHandler(AddressOf btnPausa_Click)
            .Items.Add(mni)
            .Items.Add(New Separator)

            mni = New MenuItem
            mni.Header = "Reiniciar lista"
            img = New Image
            img.Style = CType(Me.Resources("imgMenu"), Style)
            img.Source = New BitmapImage(New Uri($"{Application.DirResources}/PlayHS.png"))
            mni.Icon = img
            AddHandler mni.Click, New RoutedEventHandler(AddressOf reiniciarLista)
            .Items.Add(mni)
            .Items.Add(New Separator)

            mni = New MenuItem
            mni.Header = "Bajar volumen"
            img = New Image
            img.Style = CType(Me.Resources("imgMenu"), Style)
            img.Source = New BitmapImage(New Uri($"{Application.DirResources}/_AudioBajarHS.png"))
            mni.Icon = img
            AddHandler mni.Click, New RoutedEventHandler(AddressOf bajarVolumen)
            .Items.Add(mni)

            mni = New MenuItem
            img = New Image
            img.Style = CType(Me.Resources("imgMenu"), Style)
            img.Source = New BitmapImage(New Uri($"{Application.DirResources}/_AudioSubirHS.png"))
            mni.Icon = img
            mni.Header = "Subir volumen"
            AddHandler mni.Click, New RoutedEventHandler(AddressOf subirVolumen)
            .Items.Add(mni)

            mni = New MenuItem
            mni.Header = "Silencion total"
            img = New Image
            img.Style = CType(Me.Resources("imgMenu"), Style)
            ' En realidad da igual el que se asigne aquí,
            ' ya que al mostrar el menú se actualiza
            If Me.callado Then
                img.Source = New BitmapImage(New Uri($"{Application.DirResources}/volumenOff.ico"))
            Else
                img.Source = New BitmapImage(New Uri($"{Application.DirResources}/volumenOn.ico"))
            End If
            mni.Icon = img
            AddHandler mni.Click, New RoutedEventHandler(AddressOf btnSilencio_Click)
            .Items.Add(mni)
            .Items.Add(New Separator)

            mni = New MenuItem
            mni.Header = "Acerca de..."
            img = New Image
            img.Style = CType(Me.Resources("imgMenu"), Style)
            img.Source = New BitmapImage(New Uri($"{Application.DirResources}/acerca de.png"))
            mni.Icon = img
            AddHandler mni.Click, New RoutedEventHandler(AddressOf mnuAcercaDe_Click)
            .Items.Add(mni)
            .Items.Add(New Separator)

            mni = New MenuItem
            mni.Header = "Configurar..."
            img = New Image
            img.Style = CType(Me.Resources("imgMenu"), Style)
            img.Source = New BitmapImage(New Uri($"{Application.DirResources}/_AudioPropertiesHS.png"))
            mni.Icon = img
            AddHandler mni.Click, New RoutedEventHandler(AddressOf mnuConfigurar_Click)
            .Items.Add(mni)
            .Items.Add(New Separator)

            mni = New MenuItem
            mni.Header = "Ocultar al minimizar"
            AddHandler mni.Click, New RoutedEventHandler(AddressOf mnuOcultarMini_Click)
            .Items.Add(mni)
            .Items.Add(New Separator)

            mni = New MenuItem
            mni.Header = "Cerrar"
            img = New Image
            img.Style = CType(Me.Resources("imgMenu"), Style)
            img.Source = New BitmapImage(New Uri($"{Application.DirResources}/StandBy.png"))
            mni.Icon = img
            AddHandler mni.Click, New RoutedEventHandler(AddressOf btnCerrar_Click)
            .Items.Add(mni)

            mni = TryCast(.Items(OpcionesMenuNotify.OcultarMinimizar), MenuItem)
            mni.IsChecked = mnuOcultarMini.IsChecked
        End With
        ' Este evento se comprueba en la ventana 
        'AddHandler contextMenuForm.ContextMenuOpening, AddressOf contextMenuForm_ContextMenuOpening

        'With contextNotify
        '    .Items.Add("Mostrar " & tituloApp, Nothing, AddressOf mostrarRestaurar)
        '    ' DefaultItem no está en ToolStripMenu                  (21/Ago/07)
        '    ' Cambiar el tipo de la fuente...
        '    '.MenuItems(0).DefaultItem = True
        '    With .Items(0)
        '        .Font = New System.Drawing.Font(.Font, System.Drawing.FontStyle.Bold)
        '    End With
        '    .Items.Add("-")
        '    .Items.Add("Pausa", My.Resources.PauseHS, AddressOf btnPausa_Click)
        '    .Items.Add("-")
        '    .Items.Add("Reiniciar lista", My.Resources.PlayHS, AddressOf reiniciarLista)
        '    .Items.Add("-")
        '    ' opciones para cambiar el volumen                      (21/Feb/04)
        '    .Items.Add("Bajar volumen", My.Resources._AudioBajarHS, AddressOf bajarVolumen)
        '    .Items.Add("Subir volumen", My.Resources._AudioSubirHS, AddressOf subirVolumen)
        '    .Items.Add("Silencion total", Nothing, AddressOf btnSilencio_Click)
        '    .Items.Add("-")
        '    .Items.Add("Acerca de...", My.Resources.acerca_de, AddressOf mnuAcercaDe_Click)
        '    .Items.Add("-")
        '    .Items.Add("Configurar...", My.Resources._AudioPropertiesHS, AddressOf mnuConfigurar_Click)
        '    .Items.Add("-")
        '    .Items.Add("Ocultar al minimizar", Nothing, AddressOf mnuOcultarMini_Click)
        '    .Items.Add("-")
        '    .Items.Add("Cerrar", My.Resources.StandBy, AddressOf btnCerrar_Click)

        '    CType(.Items(OpcionesMenuNotify.OcultarMinimizar),
        '            System.Windows.Forms.ToolStripMenuItem).Checked = mnuOcultarMini.IsChecked
        'End With
        'AddHandler contextNotify.Opening, AddressOf contextNotify_Popup
        'notifyIcon1.ContextMenuStrip = contextNotify

        Me.ContextMenu = contextMenuForm

        'AddHandler notifyIcon1.DoubleClick, AddressOf notifyIcon1_DoubleClick
    End Sub

    ''' <summary>
    ''' Para dar tiempo a que se actualice el formulario
    ''' </summary>
    ''' <remarks>
    ''' TODO: Buscar una forma de hacerlo usando las clases de WinFX
    ''' La alternativa es usar el objeto de Visual Basic:
    ''' Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase
    ''' </remarks>
    Private Sub DoEvents()
        'System.Windows.Forms.Application.DoEvents()
        'vbApp.DoEvents()
    End Sub

    ''' <summary>
    ''' Pasar el foco al listView de canciones
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub focolvCanciones()
        With lvCanciones
            If .Visibility = Visibility.Visible Then
                .Focus()
            End If
        End With
    End Sub

    ''' <summary>
    ''' Guardar los datos de configuración.
    ''' Se guardan tanto en los valores del INI 
    ''' como en los datos de usuario (My.Settings)
    ''' </summary>
    ''' <remarks>
    ''' 24/Ago/07 v2.0.0.276
    ''' Solo se leen los datos del INI desde el timer,
    ''' pero hay que guardarlos para el programa de control remoto.
    ''' </remarks>
    Private Sub guardarINI()
        Dim i, j As Integer, s As String

        mINI.IniWrite(sFicIni, "Lista", "Ultima lista", sNombreLista)
        My.Settings.UltimaLista = sNombreLista

        ' Guardar las listas en la configuración del usuario        (08/Mar/07)
        listasUser = New System.Collections.Specialized.StringCollection
        j = cboListas.Items.Count
        mINI.IniDeleteSection(sFicIni, "Listas")
        mINI.IniWrite(sFicIni, "Listas", "Total", j.ToString)
        For i = 0 To j - 1
            s = cboListas.Items(i).ToString
            If String.IsNullOrEmpty(s) = False Then
                mINI.IniWrite(sFicIni, "Listas", "Lista" & i.ToString("00"), s)
                listasUser.Add(s)
            End If
        Next
        My.Settings.Listas = listasUser
        ' Los otros valores de la configuración del usuario         (08/Mar/07)
        My.Settings.Repeat = chkRepeat.IsChecked.Value
        My.Settings.Shuffle = chkShuffle.IsChecked.Value
        My.Settings.CalcularTiempo = chkCalcularTiempoLista.IsChecked.Value
        My.Settings.MinimizarTaskBar = mnuOcultarMini.IsChecked
        My.Settings.MezclarCanciones = chkMezclar.IsChecked.Value
        My.Settings.MilisegundosMezcla = Me.milisegundosMezcla
        My.Settings.IntervaloRemoto = Me.intervaloFeb

        mINI.IniWrite(sFicIni, "Control remoto", "Intervalo remoto", intervaloFeb)

        mINI.IniWrite(sFicIni, "Lista", "Repeat", chkRepeat.IsChecked)
        mINI.IniWrite(sFicIni, "Lista", "Shuffle", chkShuffle.IsChecked)

        ' para saber si se debe calcular el tiempo total...         (05/May/05)
        mINI.IniWrite(sFicIni, "General", "Calcular Tiempo Total", chkCalcularTiempoLista.IsChecked)
        mINI.IniWrite(sFicIni, "General", "Minimizar en TaskBar", mnuOcultarMini.IsChecked)

        ' Para la mezcla de las canciones                           (21/Oct/06)
        mINI.IniWrite(sFicIni, "General", "Mezclar canciones", Me.chkMezclar.IsChecked)
        mINI.IniWrite(sFicIni, "General", "milisegundosMezcla", milisegundosMezcla)

        'My.Settings.Save()
    End Sub

    ''' <summary>
    ''' Guardar la lista en el fichero indicado en sNombreLista
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub guardarLista()
        Dim oFA As New cFileToArray
        Dim aFiles() As String

        ' comprobar si esta lista está en el combo                  (28/Nov/04)
        comprobarListasRecientes()

        Dim n As Integer = lvCanciones.Items.Count - 1

        ReDim aFiles(n)
        For i As Integer = 0 To n
            aFiles(i) = TryCast(lvCanciones.Items(i), ItemFichero).FullName
        Next
        oFA.WriteArrayToFile(sNombreLista, aFiles)
        My.Settings.UltimaLista = sNombreLista
    End Sub

    ''' <summary>
    ''' Guardar la lista actual en el directorio del programa.
    ''' Esta lista la usa el programa de control remoto para saber 
    ''' la lista de canciones.
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    Private Sub guardarListaActual()
        ' Guardar la lista actual en el directorio del programa     (18/Abr/07)
        If lvCanciones.Items.Count = 0 Then Exit Sub

        Dim sFic As String = cIniArray.AppPath(True) & "ListaActual.txt"
        Dim sw As New System.IO.StreamWriter(sFic, False, System.Text.Encoding.Default)
        For i As Integer = 0 To lvCanciones.Items.Count - 1
            sw.WriteLine(TryCast(lvCanciones.Items(i), ItemFichero).FullName)
        Next
        sw.Close()
        ' Indicar que se ha cambiado el contenido de la lista actual
        mINI.IniWrite(sFicIni, "Lista", "ListaActual cambiada", "1")
    End Sub

    ''' <summary>
    ''' Mezclar aleatoriamente el contenido de la lista
    ''' para usar con la opción Shuffle.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub mezclarLista()
        ' Mezclar el contenido de la lista, para hacer Shuffle          (11/Ene/00)
        Dim i As Integer
        Dim j As Integer
        Dim n As Integer
        Dim nItems() As Integer
        Dim sItems() As String
        Dim sItems2() As String
        Dim nRnd As New System.Random

        With lvCanciones
            j = .Items.Count - 1
            ReDim nItems(j)
            ReDim sItems(j)

            ' Guardar el contenido actual
            For i = 0 To j
                ' el elemento a clasificar es el nombre completo    (28/Feb/05)
                sItems(i) = TryCast(.Items(i), ItemFichero).FullName
                nItems(i) = i
            Next

            ' Mezclar
            '
            ' Este método para mezclar los elementos,
            ' está "inspirado" en un código de Bruce McKinney.
            ' La verdad es que es más simple y rápido que el método de
            ' sacar un número aleatorio y comprobar que no haya salido antes.
            For i = j To 0 Step -1
                DoEvents()
                n = nRnd.Next(0, i)
                swap(Of Integer)(nItems(i), nItems(n))
            Next

            ' Poner el nuevo contenido
            ReDim sItems2(j)
            For i = 0 To j
                sItems2(i) = sItems(nItems(i))
            Next
            .Items.Clear()
        End With
        array2Lista(sItems2)
    End Sub

    ''' <summary>
    ''' Devuelve una cadena con formato del tiempo en milisegundos indicado.
    ''' Se usa para mostrar en las etiquetas del tiempo total y restante.
    ''' </summary>
    ''' <param name="cuantosMilisegundos"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function milisegundos2Display(ByVal cuantosMilisegundos As Integer) As String
        Dim durTmp As Integer = cuantosMilisegundos \ 1000
        Dim durHor As Integer = vb.Fix(durTmp \ 3600)
        Dim durMin As Integer = vb.Fix((durTmp - durHor * 3600) \ 60)
        Dim durSeg As Integer = durTmp - durMin * 60 - durHor * 3600

        Return durHor.ToString("00") & "." & durMin.ToString("00") & "." & durSeg.ToString("00")
    End Function

    '''' <summary>
    '''' Minimizar la aplicación usando un temporizador,
    '''' con idea de que de tiempo a que se salga del evento Load, etc.
    '''' </summary>
    '''' <remarks></remarks>
    'Private Sub minimizar()
    '    ' Llamar directamente a minimizarThread                     (21/Ago/07)
    '    minimizarThread()
    '    'esRestaurar = False
    '    'timerHilos.Interval = 300
    '    'timerHilos.Enabled = True
    'End Sub

    ''' <summary>
    ''' Método para minimizar el formulario,
    ''' se llamará desde otro hilo.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub minimizarThread()
        'DoEvents()
        'Me.WindowState = System.Windows.WindowState.Minimized
        'DoEvents()

        ' Hacer todo el trabajo aquí,                               (21/Ago/07)
        ' ya que se minimiza, pero no se oculta...
        ' (aunque se ejecuta el método de StateChanged)
        ' ¡Pasa de todo!

        ' Cuando se minimice, ocultar el formulario                 (27/Abr/02)

        ' Para que funcione bien en WinFX simplemente ocultarlo     (21/Ago/07)
        ' ¡pero hay que tener en cuenta que el WindowState no es minimizado!
        If minimizarEnTaskBar <> 0 Then

            ' No hacer esta asignación a WindowState
            ' porque se queda en la barra de la tarea
            ' aunque se indique que se oculte...

            'Me.WindowState = System.Windows.WindowState.Minimized

            Me.Hide()

            ' Para cambiar el estado del menú contextual            (25/Oct/06)
            tocandoMinimizado = tocandoMinimizado
        Else
            Me.WindowState = System.Windows.WindowState.Minimized
        End If
    End Sub

    ''' <summary>
    ''' Mover un el elemento seleccionado en la lista.
    ''' El segundo parámetro se usará para indicar
    ''' si se sube o se baja.
    ''' </summary>
    ''' <param name="lv">
    ''' El control ListView en el que está el elemento a mover.
    ''' El elemento que se moverá será el que esté seleccionado.
    ''' </param>
    ''' <param name="arriba">
    ''' True para subir el elemento una posición,
    ''' False para bajarlo una posición.
    ''' </param>
    ''' <remarks></remarks>
    Private Sub mover(ByVal lv As ListView, ByVal arriba As Boolean)
        Dim j As Integer

        With lv
            ' Salir si no hay elmentos                              (17/Ago/07)
            If .Items.Count = 0 Then
                Exit Sub
            End If
            Try
                j = .SelectedIndex
            Catch
                j = -1
            End Try
            ' Elimanr la selección actual
            .SelectedItems.Clear()

            If arriba Then   ' Arriba
                If j > 0 Then
                    swap(.Items(j), .Items(j - 1))
                    .SelectedIndex = j - 1
                End If
            Else                ' Abajo
                If j < .Items.Count - 1 AndAlso j > -1 Then
                    swap(.Items(j), .Items(j + 1))
                    .SelectedIndex = j + 1
                End If
            End If
        End With
    End Sub

    ''' <summary>
    ''' Reiniciar la lista actual.
    ''' Empezar de nuevo por el primero.
    ''' </summary>
    ''' <remarks>
    ''' Al tocar la lista, si se tiene que mezclar, se mezclará.
    ''' 28/Sep/07: Se usa la otra sobrecarga
    ''' </remarks>
    Private Sub reiniciarLista()
        reiniciarLista(0)
    End Sub

    ''' <summary>
    ''' Reiniciar la lista a partir del índice indicado
    ''' </summary>
    ''' <param name="index">
    ''' El índice a asignar en el listView
    ''' para que empiece por esa canción
    ''' </param>
    ''' <remarks>
    ''' v2.0.0.300
    ''' 28/Sep/07
    ''' </remarks>
    Private Sub reiniciarLista(ByVal index As Integer)
        ' Parar la lista
        Me.btnPararLista_Click(Nothing, Nothing)
        ' Un respiro                                                (28/Sep/07)
        DoEvents()
        ' poner como seleccionado el índice del parámetro
        lvCanciones.SelectedIndex = index
        ' tocar la lista
        Me.btnTocarLista_Click(Nothing, Nothing)
    End Sub

    '''' <summary>
    '''' Restaurar la ventana. 
    '''' Se inicia el temporizador que se encarga de restaurarla.
    '''' </summary>
    '''' <remarks></remarks>
    'Private Sub restaurar()
    '    ' Llamar directamente a restaurarThread                     (21/Ago/07)
    '    restaurarThread()
    '    'esRestaurar = True
    '    'timerHilos.Interval = 300
    '    'timerHilos.Enabled = True
    'End Sub

    ''' <summary>
    ''' Para restaurar la ventana.
    ''' Este método se llamará desde otro hilo, 
    ''' concretamente desde un timer.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub restaurarThread()
        ' Ponerla para que se muestre                               (21/Ago/07)
        ' ya que desde el menú contextual se oculta directamente
        ' en vez de minimizar

        '**********************************************************************
        ' Es que como se ocultaba en vez de minimizar...
        ' ¡NUNCA ENTRABA EN ESTE MÉTODO!
        '**********************************************************************

        ' Seguir este orden:
        '   Show, WindowState y TopMost

        Me.Show()
        Me.WindowState = WindowState.Normal
        Me.Activate()
        'Me.Topmost = True

    End Sub

    ''' <summary>
    ''' Intercambiar dos valores de cualquier tipo.
    ''' (los dos valores a intercambiar deben ser del mismo tipo)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="uno"></param>
    ''' <param name="dos"></param>
    ''' <remarks></remarks>
    Private Sub swap(Of T)(ByRef uno As T, ByRef dos As T)
        Dim tres As T = uno
        uno = dos
        dos = tres
    End Sub

    ''' <summary>
    ''' Actualizar los datos cuando se produce el evento del temporizador
    ''' Lo pongo en método aparte para llamarlo desde el timer.
    ''' Aquí se procesan también los datos que se hayan actualizado remotamente.
    ''' </summary>
    ''' <remarks>
    ''' Utilizo la función Val de Visual Basic para tomar solo los valores
    ''' de las cadenas leídas del fichero INI, ya que pueden tener comentarios
    ''' (separados con punto y coma) después del valor.
    ''' </remarks>
    Private Sub timerRemotoActualizar()
        Dim i As Integer

        ' Comprobar si hay datos modificados                        (25/Oct/06)
        ' Por si no hay, para solo hacer una lectura
        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "Hay cambios", "0")))

        ' El tiempo restante de la canción actual                   (18/Abr/07)
        ' Se actualiza siempre aunque no haya cambios
        mINI.IniWrite(sFicIni, "Control remoto", "Tiempo restante", lblInfo2.Content.ToString)

        If i = 0 Then Exit Sub

        mINI.IniWrite(sFicIni, "Control remoto", "Hay cambios", "0 ; indica si se ha modificado remotamente")

        ' ¿Por qué estaba esta línea?                               (09/Ago/07)
        'mINI.IniWrite(sFicIni, "Control remoto", "Cambiar cancion", "0")

        ' Si se indica otra canción a tocar (de la lista actual)    (18/Abr/07)
        i = CInt(mINI.IniGet(sFicIni, "Control remoto", "Cambiar cancion", "0"))
        If i <> 0 Then
            i = CInt(mINI.IniGet(sFicIni, "Control remoto", "Numero cancion", "-1"))
            If i > -1 AndAlso i < lvCanciones.Items.Count Then
                ' Lo que se debe detener y reiniciar es la lista    (18/Abr/07)
                ' (no llamar al método reiniciarLista, ya que se debe parar,
                ' asignar el índice y después empezar a tocar,
                ' con idea de que se empiece por ese índice
                ' en lugar de por el primero, que es lo que se hace allí)

                ' Añado una sobrecarga a reiniciarLista             (28/Sep/07)
                ' en la que se indica la canción a tocar.
                reiniciarLista(i)

                'Me.btnPararLista_Click(Nothing, Nothing)
                '' Algunas veces no funciona (creo)                  (28/Sep/07)
                '' así que darle un respiro
                'DoEvents()
                'lvCanciones.SelectedIndex = i
                'Me.btnTocarLista_Click(Nothing, Nothing)
            End If
        End If
        mINI.IniWrite(sFicIni, "Control remoto", "Cambiar cancion", "0")

        ' Si se cambia el intervalo de comprobación                 (07/Feb/07)
        i = CInt(mINI.IniGet(sFicIni, "Control remoto", "Intervalo remoto", "5000"))
        If i <> Me.timerFeb.Interval Then
            ' Los valores serán de 1,5 a 20 segundos, predeterminado 4
            If i < 1500 OrElse i > 20000 Then
                i = 5000
            End If
            intervaloFeb = i
            Me.timerFeb.Stop()
            Me.timerFeb.Interval = i
            Me.timerFeb.Start()
            My.Settings.IntervaloRemoto = intervaloFeb
        End If

        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "Pausa", "0")))
        If i <> 0 Then
            mINI.IniWrite(sFicIni, "Control remoto", "Pausa", "0 ; como si hiciéramos click en pausa (pausa o reanuda)")
            btnPausa_Click(Nothing, Nothing)
        End If

        ' terminar actual permite parar la actual
        ' si está tocando la lista, pasa a la siguiente
        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "TerminarActual", "0")))
        If i <> 0 Then
            mINI.IniWrite(sFicIni, "Control remoto", "TerminarActual", "0 ; Para la actual, pero no la lista")
            btnParar_Click(btnParar, Nothing)
        End If

        ' Parar lo que se esté tocando
        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "TerminarLista", "0")))
        If i <> 0 Then
            mINI.IniWrite(sFicIni, "Control remoto", "TerminarLista", "0 ; Para la lista (o la actual, es decir, deja de tocar)")
            btnPararLista_Click(Nothing, Nothing)
        End If

        ' restaurar la aplicación
        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "Restaurate", "0")))
        If i <> 0 Then
            mINI.IniWrite(sFicIni, "Control remoto", "Restaurate", "0 ; Para restaurar el programa")
            'restaurar()
            restaurarThread()
        End If

        ' Tocar la lista                                            (25/Oct/06)
        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "TocarLista", "0")))
        If i <> 0 Then
            mINI.IniWrite(sFicIni, "Control remoto", "TocarLista", "0 ; Inicia la lista")
            Me.btnTocarLista_Click(Nothing, Nothing)
        End If

        ' Reiniciar la lista                                        (06/Feb/07)
        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "ReiniciarLista", "0")))
        If i <> 0 Then
            mINI.IniWrite(sFicIni, "Control remoto", "ReiniciarLista", "0 ; Reinicia la lista")
            ' Reiniciar la lista desde un método                    (13/Jul/07)
            reiniciarLista()
        End If

        ' Subir y bajar el volumen                                  (25/Oct/06)
        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "Subir volumen", "0")))
        If i <> 0 Then
            mINI.IniWrite(sFicIni, "Control remoto", "Subir volumen", "0 ; Subir el volumen")
            Me.subirVolumen(Nothing, Nothing)
        End If
        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "Bajar volumen", "0")))
        If i <> 0 Then
            mINI.IniWrite(sFicIni, "Control remoto", "Bajar volumen", "0 ; Bajar el volumen")
            Me.bajarVolumen(Nothing, Nothing)
        End If

        i = CInt(mINI.IniGet(sFicIni, "General", "Mezclar canciones", "1"))
        If i <> 0 Then
            Me.chkMezclar.IsChecked = True
        Else
            Me.chkMezclar.IsChecked = False
        End If
        My.Settings.MezclarCanciones = Me.chkMezclar.IsChecked.Value
        milisegundosMezcla = CInt(mINI.IniGet(sFicIni, "General", "milisegundosMezcla", "500"))
        ' De 100 a 3000 milisegundos
        If milisegundosMezcla < 100 OrElse milisegundosMezcla > 3000 Then
            milisegundosMezcla = 500
        End If
        mINI.IniWrite(sFicIni, "General", "milisegundosMezcla", milisegundosMezcla.ToString)
        My.Settings.MilisegundosMezcla = milisegundosMezcla

        i = Int32.Parse(mINI.IniGet(sFicIni, "Lista", "Repeat", "0"))
        chkRepeat.IsChecked = Convert.ToBoolean(i)
        My.Settings.Repeat = chkRepeat.IsChecked.Value

        i = Int32.Parse(mINI.IniGet(sFicIni, "Lista", "Shuffle", "0"))
        chkShuffle.IsChecked = Convert.ToBoolean(i)
        My.Settings.Shuffle = chkShuffle.IsChecked.Value

        ' Si se cambia de lista                                     (06/Feb/07)
        i = CInt(vb.Val(mINI.IniGet(sFicIni, "Control remoto", "CambiarLista", "0")))
        If i <> 0 Then
            mINI.IniWrite(sFicIni, "Control remoto", "CambiarLista", "0 ; Cambiar la lista")
            Dim sLista As String
            sLista = mINI.IniGet(sFicIni, "Control remoto", "Ultima lista", "")
            If String.IsNullOrEmpty(sLista) = False Then
                sNombreLista = sLista

                ' si no existe la lista, añadirla               (01/Nov/15 16.54)
                comprobarListasRecientes()

                ' Parar la lista
                Me.btnPararLista_Click(Nothing, Nothing)
                ' Eliminar los elementos que haya
                Me.lvCanciones.Items.Clear()

                ' Para que empiece por la primera
                ultimaCancion = 0

                ' Iniciar la lista desde el timer                   (24/Ago/07)
                '                                                   v2.0.0.282
                ' Se hacen estos tres pasos:
                '   abrirlista, seleccionar el fichero y empezar a tocar
                ' aunque empezar a tocar solo lo hará si esLineaComandos = True

                ' Esto es lo que hará que se toque...
                ' ya que se comprueba este valor para llamar a btnTocarLosta_Click
                esLineaComandos = True

                timerAbrirLista.Interval = 100
                timerAbrirLista.Enabled = True

                '' Abrir esta lista
                'abrirLista()
                '' poner como seleccionado el primero
                'lvCanciones.SelectedIndex = ultimaCancion
                '' tocar la lista
                'Me.btnTocarLista_Click(Nothing, Nothing)

                My.Settings.UltimaLista = sNombreLista
            End If
        End If

        My.Settings.Save()

        mINI.IniWrite(sFicIni, "Control remoto", "Tocando lista", tocandoLista.ToString)
    End Sub

    ''' <summary>
    ''' Para saber si se está tocando la lista
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Property tocandoLista() As cMediaPlayer.eMCIModo
        Get
            Return mTocandoLista
        End Get
        Set(ByVal value As cMediaPlayer.eMCIModo)
            mTocandoLista = value
            btnPararLista.IsEnabled = (mTocandoLista <> cMediaPlayer.eMCIModo.eStop)
            btnTocarLista.IsEnabled = (btnPararLista.IsEnabled = False)
        End Set
    End Property

    ''' <summary>
    ''' Para cuando esté minimizado y tocando se pueda hacer doble clic
    ''' y cambiar el estado de la pausa
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>
    ''' No actualizar el menú contextual del formulario,
    ''' ya que en el del formulario, se muestra de forma normal,
    ''' solo en el icono es donde se puede hacer doble clic.
    ''' </remarks>
    Private Property tocandoMinimizado() As Boolean
        Get
            Return _tocandoMinimizado
        End Get
        Set(ByVal value As Boolean)
            _tocandoMinimizado = value
            ' Ajustar el menú contextual del notifyicon
            ' y el evento a interceptar

            ' El evento es el mismo,                                (21/Ago/07)
            ' pero si está tocando se usa Pausa
            ' y si no está tocando es como mostrar/minimizar
            If value Then
                ' DefaultItem no existe en los ToolMenuStrip,       (21/Ago/07)
                ' cambiar la fuente a Bold ...
                'With Me.notifyIcon1.ContextMenuStrip.Items(OpcionesMenuNotify.Restaurar)
                '    .Font = New System.Drawing.Font(.Font, System.Drawing.FontStyle.Regular)
                'End With
                'With Me.notifyIcon1.ContextMenuStrip.Items(OpcionesMenuNotify.Pausa)
                '    .Font = New System.Drawing.Font(.Font, System.Drawing.FontStyle.Bold)
                'End With
            Else
                'With Me.notifyIcon1.ContextMenuStrip.Items(OpcionesMenuNotify.Pausa)
                '    .Font = New System.Drawing.Font(.Font, System.Drawing.FontStyle.Regular)
                'End With
                'With Me.notifyIcon1.ContextMenuStrip.Items(OpcionesMenuNotify.Restaurar)
                '    .Font = New System.Drawing.Font(.Font, System.Drawing.FontStyle.Bold)
                'End With
            End If
        End Set
    End Property

    ''' <summary>
    ''' Toca la canción indicada en el parámetro.
    ''' Se actualizan los datos y habilitan botones, etc.
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns>
    ''' Devuelve la duración en formato mm:ss
    ''' </returns>
    ''' <remarks></remarks>
    Private Function tocarCancion(ByVal s As String) As String
        Try
            Me.lblFullName.Content = s

            If esMCI2 Then
                mPlayMCIactual = mPlayMCI2
            Else
                mPlayMCIactual = mPlayMCI
            End If

            With mPlayMCIactual
                .FileName = s
                .Play()
                With Me.scrollPosicion
                    .Maximum = 100
                    .Minimum = 0
                    .Value = 0
                    .SmallChange = 1
                    .LargeChange = 5
                End With
                pasoScroll = .Duration / 100

                lblInfo.Content = .TiempoTotal
                lblInfo2.Content = .TiempoRestante

                ' Actualizar la info de la canción actual           (18/Ago/07)
                ' (al menos la de la duración)
                s = .TiempoTotal
                Me.lblDuracion.Content = s ' .TiempoTotal
                ' Guardar la duración de esta canción
                My.Settings.durActual = s ' Me.lblDuracion.Content.ToString

                actualizarBotones()

                btnTocar.IsEnabled = False
                btnPausa.IsEnabled = True
                btnParar.IsEnabled = True
            End With

        Catch e As Exception
            MessageBox.Show("ERROR: " & e.Message,
                            "Error al tocar",
                            MessageBoxButton.OK, MessageBoxImage.Exclamation)
            s = "00:00"
        End Try
        '' marcar que no está en pausa                               (10/Dic/04)
        'CType(contextNotify.Items(OpcionesMenuNotify.Pausa), System.Windows.Forms.ToolStripMenuItem).Checked = False

        Return s
    End Function


    '--------------------------------------------------------------------------
    ' Los métodos de eventos asignados manualmente
    ' aunque algunos los he asignado a los menús de la aplicación
    '--------------------------------------------------------------------------

    '''' <summary>
    '''' Se produce al mostrar el menú contextual del icono del área de notificación.
    '''' Y se aprovecha para habilitar adecuadamente las opciones del menú.
    '''' </summary>
    '''' <param name="sender"></param>
    '''' <param name="e"></param>
    '''' <remarks></remarks>
    'Private Sub contextNotify_Popup(ByVal sender As Object, ByVal e As EventArgs)
    '    ' Habilitar las opciones adecuadas
    '    With contextNotify
    '        ' Mostrar el texto adecuado según esté minimizado o no  (17/Ago/07)
    '        If Me.WindowState = WindowState.Minimized OrElse Me.IsVisible = False Then
    '            .Items(OpcionesMenuNotify.Restaurar).Text = "Restaurar"
    '        Else
    '            .Items(OpcionesMenuNotify.Restaurar).Text = "Minimizar"
    '        End If

    '        .Items(OpcionesMenuNotify.Pausa).Enabled = btnPausa.IsEnabled

    '        ' Habilitar adecuadamente los elementos del menú        (24/Ago/07)
    '        .Items(OpcionesMenuNotify.AcercaDe).Enabled = (estaAcercaDe = False)
    '        .Items(OpcionesMenuNotify.Config).Enabled = (estaConfigurando = False)

    '        ' No cerrar si está configurando o mostrando la configuración
    '        If estaAcercaDe OrElse estaConfigurando Then
    '            .Items(OpcionesMenuNotify.Cerrar).Enabled = False
    '        Else
    '            .Items(OpcionesMenuNotify.Cerrar).Enabled = True
    '        End If

    '        actualizarDatosVolumen(.Items, OpcionesMenuNotify.BajarVolumen)

    '    End With
    'End Sub

    ''' <summary>
    ''' Mostrar la ventana de Acerca De
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuAcercaDe_Click(ByVal sender As Object,
                                  ByVal e As EventArgs) _
                                  Handles mnuAcercaDe.Click

        ' Evitar la reentrada desde el menú contextual              (24/Ago/07)
        If estaAcercaDe Then Exit Sub

        estaAcercaDe = True

        'If acercaDe Is Nothing OrElse acercaDe.IsVisible = False Then
        '    acercaDe = New wAbout
        'End If

        With New wAbout 'acercaDe
            .ShowDialog()
            'If .IsVisible Then
            '    .Left = Me.Left
            '    .Top = Me.Top
            '    .Activate()
            'Else
            '    .ShowDialog()
            'End If
        End With

        estaAcercaDe = False
    End Sub

    ''' <summary>
    ''' Mostrar la ventana de configuración
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>
    ''' Debido a que la ventana de configuración está en una DLL,
    ''' hay que asignar y recuperar los datos de configuración en 
    ''' la propiedad gsPlaySettings.
    ''' Si se añaden nuevos valores a la configuración, 
    ''' hay que actualizar también las configuraciones usadas en la DLL.
    ''' </remarks>
    Private Sub mnuConfigurar_Click(ByVal sender As Object,
                                    ByVal e As EventArgs) _
                                    Handles mnuConfigurar.Click

        ' Evitar la reentrada desde el menú contextual              (24/Ago/07)
        If estaConfigurando Then Exit Sub

        estaConfigurando = True

        guardarINI()

        Dim fConf As New wConfig ' fConfig

        fConf.Titulo = "Configuración de " & AppTitle ' My.Application.Info.Title

        If fConf.ShowDialog Then

            ' En el fichero de configuración se habrán asignado los datos
            Dim i As Integer = cboListas.SelectedIndex
            listasUser = My.Settings.Listas
            cboListas.Items.Clear()
            For Each s1 As String In listasUser
                cboListas.Items.Add(s1)
            Next
            If i > -1 AndAlso i < cboListas.Items.Count Then
                cboListas.SelectedIndex = i
            End If

            ' Los otros valores de la configuración del usuario         (08/Mar/07)
            chkRepeat.IsChecked = My.Settings.Repeat
            chkShuffle.IsChecked = My.Settings.Shuffle
            chkCalcularTiempoLista.IsChecked = My.Settings.CalcularTiempo
            mnuOcultarMini.IsChecked = My.Settings.MinimizarTaskBar
            chkMezclar.IsChecked = My.Settings.MezclarCanciones
            Me.milisegundosMezcla = My.Settings.MilisegundosMezcla
            Me.intervaloFeb = My.Settings.IntervaloRemoto

            ItemFichero.InfoCompleta = My.Settings.InfoCompleta
            ItemFichero.InfoUnaLinea = My.Settings.InfoUnaLinea

            ' guardarlos para que también se guarden en el INI
            guardarINI()

            ' Habilitar adecuadamente el temporizador
            If Me.timerFeb.Interval <> Me.intervaloFeb Then
                Me.timerFeb.Enabled = False
                Me.timerFeb.Interval = Me.intervaloFeb
                Me.timerFeb.Enabled = True
            End If
        End If

        estaConfigurando = False
    End Sub

    ''' <summary>
    ''' Para mostrar o restaurar desde el menú contextual
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>
    ''' 17/Ago/07
    ''' </remarks>
    Private Sub mostrarRestaurar(ByVal sender As Object, ByVal e As EventArgs)
        If Me.WindowState = WindowState.Minimized OrElse Me.IsVisible = False Then
            ' restaurar
            'restaurar()
            restaurarThread()
        Else
            ' minimizar
            'minimizar()
            minimizarThread()
        End If
    End Sub

    ''' <summary>
    ''' Restaurar desde el notifyIcon
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub notifyIcon1_DoubleClick(ByVal sender As Object, ByVal e As EventArgs)
        If tocandoMinimizado Then
            Me.btnPausa_Click(Nothing, Nothing)
        Else
            ' Usar el método adecuado según esté oculto o no        (21/Ago/07)
            mostrarRestaurar(Nothing, Nothing)
            'restaurar()
        End If
    End Sub

    '''' <summary>
    '''' Al hacer doble clic se ejecutará el comando de pausa.
    '''' Este evento solo estará disponible cuando se esté tocando una canción
    '''' y se llamará desde el menú contextual del icono del área de notificación.
    '''' </summary>
    '''' <param name="sender"></param>
    '''' <param name="e"></param>
    '''' <remarks></remarks>
    'Private Sub notifyTocandoMinimizado_DoubleClick(ByVal sender As Object, ByVal e As EventArgs)
    '    Me.btnPausa_Click(Nothing, Nothing)
    'End Sub



    '--------------------------------------------------------------------------
    ' Los métodos de eventos de las clases cPlayMCI
    '--------------------------------------------------------------------------

    Private Sub mPlayMCIactual_Done(
                                    ByVal NotifyCode As cMediaPlayer.eMCINotify) _
                                    Handles mPlayMCIactual.Done

        ' Aqui llega cuando ha terminado de tocar o hay un error        (17/Mar/02)
        ' pero no es necesario, ya que ahora cuando termina de tocar
        ' se lanza el evento StatusUpdate
        '
        ' Si se está tocando la lista...
        If tocandoLista <> cMediaPlayer.eMCIModo.eStop Then
            ' Pasar a la siguiente canción
            ' Solo si no se mezcla                                  (21/Oct/06)
            'If mezclarCanciones = False Then
            '    btnSiguiente_Click(btnSiguiente, Nothing)
            'End If

            ' Pasar SIEMPRE a la siguiente canción              (01/Nov/15 14.58)
            btnSiguiente_Click(btnSiguiente, Nothing)
        End If
    End Sub

    Private Sub mPlayMCIactual_MilisegundoRestantes(
                                                    ByVal milisegundos As Integer) _
                                                    Handles mPlayMCIactual.MilisegundosRestantes

        ' Con este valor podemos calcular los segundos restantes    (20/Oct/06)
        ' segundos = milisegundos / 1000
        ' 500 sería medio segundo
        ' Se puede mezclar al quedar medio segundo
        ' o el tiempo indicado en cada canción, si queremos quitar los finales
        If mezclarCanciones Then
            If milisegundos <= milisegundosMezcla Then
                If Me.esMCI2 Then
                    mPlayMCIactual = mPlayMCI '.Clone()
                    esMCI2 = False
                Else
                    mPlayMCIactual = mPlayMCI2 '.Clone()
                    esMCI2 = True
                End If
                btnSiguiente_Click(btnSiguiente, Nothing)
            End If
        End If
    End Sub

    Private Sub mPlayMCIactual_StatusUpdate() Handles mPlayMCIactual.StatusUpdate
        Select Case mPlayMCIactual.Mode  'mPlayMCIactual.EstadoActual
            Case cMediaPlayer.eMCIModo.eStop
                btnTocar.IsEnabled = True
                btnPausa.IsEnabled = False
                btnParar.IsEnabled = False
                ' Indicar que la lista está parada                  (07/Abr/07)
                mINI.IniWrite(sFicIni, "Control remoto", "Tocando lista", tocandoLista.ToString)
            Case cMediaPlayer.eMCIModo.ePausa
                btnTocar.IsEnabled = True
                btnPausa.IsEnabled = True
                btnParar.IsEnabled = True
            Case cMediaPlayer.eMCIModo.ePlay
                btnTocar.IsEnabled = False
                btnPausa.IsEnabled = True
                btnParar.IsEnabled = True
        End Select

        lblInfo2.Content = mPlayMCIactual.TiempoRestante

        ' En realidad, si está minimizado...                        (21/Ago/07)
        ' ¿para qué hacer esta asignación?
        ' salvo que sea para que al restaurarse se muestre correctamente...
        ' pero.... ¿por qué no hacerlo siempre?
        If Me.WindowState = WindowState.Minimized OrElse Me.IsVisible = False Then
            cambiandoScroll = True
            Try
                scrollPosicion.Value = Convert.ToInt32(mPlayMCI.CurrentPosition / pasoScroll)
                With trackVol
                    ' Con la divisón / va bien...
                    .Value = CInt(mPlayMCIactual.Volumen * cMaxTrack / volMax)
                    trackVol.ToolTip = " " & mPlayMCI.Volumen & " (" & trackVol.Value * cMaxTrack & "%)"
                End With
            Catch
                '
            End Try
            cambiandoScroll = False
        End If
        '
        '
        ' Mostrar siempre en el notifyIcon el título de la canción  (21/Oct/06)
        Dim s As String = "", st As String = ""

        If mPlayMCIactual.Mode <> cMediaPlayer.eMCIModo.eStop Then
            s = sysIO.Path.GetFileNameWithoutExtension(mPlayMCIactual.FileName)
            If s.Length > 63 Then
                s = s.Substring(0, 63)
            End If
            st = tituloApp & " - " & s
            s &= " [" & mPlayMCIactual.TiempoRestante & "]"
        Else
            s = tituloApp
            st = esElGuille & ")"
        End If
        If s.Length > 61 Then
            s = " " & s.Substring(s.Length - 61, 61) & " "
        Else
            s = " " & s & " "
        End If
        If Me.WindowState = WindowState.Minimized Then
            st &= " - " & s
            If st.Length > 63 Then
                st = st.Substring(0, 63)
            End If
        End If
        Me.Title = st
        'If notifyIcon1 IsNot Nothing Then
        '    notifyIcon1.Text = s
        'End If
    End Sub

    Private Sub mPlayMCI_StatusUpdate() Handles mPlayMCI.StatusUpdate
        If mPlayMCI.Mode = cMediaPlayer.eMCIModo.ePlay Then
            actualizarDatosActual(mPlayMCI)
        End If
    End Sub

    Private Sub mPlayMCI2_StatusUpdate() Handles mPlayMCI2.StatusUpdate
        If mPlayMCI2.Mode = cMediaPlayer.eMCIModo.ePlay Then
            actualizarDatosActual(mPlayMCI2)
        End If
    End Sub


    '--------------------------------------------------------------------------
    ' Los métodos de eventos de los controles
    '--------------------------------------------------------------------------

    Private Sub chkMezclar_Checked(ByVal sender As Object,
                                   ByVal e As RoutedEventArgs) _
                                   Handles chkMezclar.Checked
        If Me.inicializando Then Exit Sub

        Me.mezclarCanciones = True ' Me.chkMezclar.IsChecked.Value
        mINI.IniWrite(sFicIni, "General", "Mezclar canciones", "1")
        focolvCanciones()
    End Sub

    Private Sub chkMezclar_Unchecked(ByVal sender As Object,
                                     ByVal e As RoutedEventArgs) _
                                     Handles chkMezclar.Unchecked
        If Me.inicializando Then Exit Sub

        Me.mezclarCanciones = False ' Me.chkMezclar.IsChecked.Value
        mINI.IniWrite(sFicIni, "General", "Mezclar canciones", "0")
        focolvCanciones()
    End Sub

    Private Sub chkRepeat_Checked(ByVal sender As Object,
                                  ByVal e As RoutedEventArgs) Handles chkRepeat.Checked
        If Me.inicializando Then Exit Sub

        mINI.IniWrite(sFicIni, "Lista", "Repeat", "1")

        focolvCanciones()
    End Sub

    Private Sub chkRepeat_Unchecked(ByVal sender As Object,
                                    ByVal e As RoutedEventArgs) Handles chkRepeat.Unchecked
        If Me.inicializando Then Exit Sub

        mINI.IniWrite(sFicIni, "Lista", "Repeat", "0")

        focolvCanciones()
    End Sub

    Private Sub chkShuffle_Checked(ByVal sender As Object,
                                   ByVal e As RoutedEventArgs) Handles chkShuffle.Checked
        If Me.inicializando Then Exit Sub

        mINI.IniWrite(sFicIni, "Lista", "Shuffle", "1")

        focolvCanciones()
    End Sub

    Private Sub chkShuffle_Unchecked(ByVal sender As Object,
                                     ByVal e As RoutedEventArgs) Handles chkShuffle.Unchecked
        If Me.inicializando Then Exit Sub

        mINI.IniWrite(sFicIni, "Lista", "Shuffle", "0")

        focolvCanciones()
    End Sub

    ''' <summary>
    ''' Abrir una lista, 
    ''' se asigna el contenido y se añade al combo de listas recientes
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnAbrirLista_Click(ByVal sender As Object,
                                    ByVal e As RoutedEventArgs) _
                                    Handles btnAbrirLista.Click, mnuAbrirLista.Click
        ' Abrir una lista del tipo .m3u (o .txt)
        Try
            If sysIO.File.Exists(sNombreLista) = False Then
                sNombreLista = ""
            End If
        Catch
            sNombreLista = ""
        End Try
        '
        With oFD
            .Title = "Seleccionar la lista de canciones a añadir a la lista actual"
            .Filter = "Lista de canciones (*.m3u)|*.m3u|Todos los archivos (*.*)|*.*"
            .FileName = sNombreLista
            If .ShowDialog() Then
                sNombreLista = .FileName
                ' Abrir lista
                abrirLista()

                ' Guardar los datos de configuración                (24/Ago/07)
                ' para que el programa remoto se entere.            v2.0.0.287
                guardarINI()
            End If
        End With

        focolvCanciones()
    End Sub

    Private Sub btnBorrarLista_Click(ByVal sender As Object,
                                     ByVal e As RoutedEventArgs) _
                                     Handles btnBorrarLista.Click
        ' Borrar la lista
        'lvCanciones.ListViewItemSorter = Nothing
        lvCanciones.Items.Clear()
        lblStatus.Content = elGuille2

        actualizarBotones()

    End Sub
    '
    Private Sub btnCerrar_Click(ByVal sender As Object,
                                ByVal e As EventArgs) _
                                Handles btnCerrar.Click, mnuCerrar.Click
        Me.Close()
    End Sub
    '
    Private Sub btnGuardarLista_Click(ByVal sender As Object,
                                      ByVal e As RoutedEventArgs) _
                                      Handles btnGuardarLista.Click, mnuGuardarLista.Click
        ' Guardar el contenido de la lista
        ' Abrir una lista del tipo .m3u (o .txt)
        With sFD
            .Filter = "Lista de canciones (*.m3u)|*.m3u|Todos los archivos (*.*)|*.*"
            .FileName = sNombreLista
            If .ShowDialog() Then
                sNombreLista = .FileName
                ' Guardar la lista
                guardarLista()
            End If
        End With

        focolvCanciones()
    End Sub

    Private Sub btnTocarLista_Click(ByVal sender As Object,
                                   ByVal e As RoutedEventArgs) _
                                   Handles btnTocarLista.Click, mnuTocarLista.Click
        ' Tocar la lista
        '
        ' Cuando se toca la lista, no clasificar el contenido
        'lvCanciones.ListViewItemSorter = Nothing

        If chkShuffle.IsChecked Then
            mezclarLista()
        End If
        nCancionActual = 0

        ' actualizar el menú contextual                             (13/Feb/05)
        If Me.minimizarEnTaskBar <> 0 Then
            tocandoMinimizado = True
        End If

        ' Empezar a tocar
        Me.esMCI2 = False
        Me.mPlayMCIactual = Me.mPlayMCI

        tocandoLista = cMediaPlayer.eMCIModo.ePlay
        mINI.IniWrite(sFicIni, "Control remoto", "Tocando lista", tocandoLista.ToString)
        btnTocar_Click(btnTocar, Nothing)

        'btnBorrarLista.IsEnabled = False
        'btnAbrirLista.IsEnabled = False
        'btnGuardarLista.IsEnabled = False
        'btnCalcularTiempo.IsEnabled = False
        'btnAñadirCanciones.IsEnabled = False

        cboListas.IsEnabled = False
        btnAbrirReciente.IsEnabled = False

    End Sub

    Private Sub btnPararLista_Click(ByVal sender As Object,
                                   ByVal e As RoutedEventArgs) _
                                   Handles btnPararLista.Click, mnuPararLista.Click

        ' Parar la lista
        tocandoLista = cMediaPlayer.eMCIModo.eStop
        mINI.IniWrite(sFicIni, "Control remoto", "Tocando lista", tocandoLista.ToString)
        ' actualizar el menú contextual                             (13/Feb/05)
        tocandoMinimizado = False

        btnParar_Click(btnParar, Nothing)
        'ListView1.SetFocus
        btnBorrarLista.IsEnabled = True
        btnAbrirLista.IsEnabled = True
        btnGuardarLista.IsEnabled = True
        btnCalcularTiempo.IsEnabled = btnBorrarLista.IsEnabled
        btnAñadirCanciones.IsEnabled = btnGuardarLista.IsEnabled

        cboListas.IsEnabled = True
        btnAbrirReciente.IsEnabled = True

        Me.esMCI2 = False
        Me.mPlayMCIactual = Me.mPlayMCI

    End Sub

    Private Sub btnPrimera_Click(ByVal sender As Object,
                                 ByVal e As RoutedEventArgs) _
                                 Handles btnPrimera.Click, mnuPrimera.Click
        Try
            ' Ir a la primera canción
            ' Parar el actual,                          (21/Oct/06)
            ' ya que se puede usar otro objeto
            If Me.mezclarCanciones Then
                Me.mPlayMCI.Parar()
                Me.mPlayMCI2.Parar()
                Me.esMCI2 = False
            End If

            nCancionActual = 0
            With lvCanciones
                If .Items.Count > 0 Then
                    .SelectedIndex = 0
                    'CType(.Items(nCancionActual), ListViewItem).IsSelected = True
                End If
            End With
            '
            comprobarLista()
            '
            focolvCanciones()
        Catch 'ex As Exception
        End Try
    End Sub

    Private Sub btnUltima_Click(ByVal sender As Object,
                                ByVal e As RoutedEventArgs) _
                                Handles btnUltima.Click, mnuUltima.Click
        Try
            ' Ir a la última canción
            ' Parar el actual,                          (21/Oct/06)
            ' ya que se puede usar otro objeto
            If Me.mezclarCanciones Then
                Me.mPlayMCI.Parar()
                Me.mPlayMCI2.Parar()
                Me.esMCI2 = False
            End If

            With lvCanciones
                nCancionActual = .Items.Count - 1
                If .Items.Count > 0 Then
                    .SelectedItem = .Items(nCancionActual)
                End If
            End With

            comprobarLista()

            focolvCanciones()
        Catch 'ex As Exception
        End Try
    End Sub

    Private Sub btnSiguiente_Click(ByVal sender As Object,
                                 ByVal e As RoutedEventArgs) _
                                 Handles btnSiguiente.Click, mnuSiguiente.Click
        Try
            ' Tocar la canción siguiente

            ' No parar las canciones aquí,
            ' ya que al mezclar se llama a este método

            nCancionActual = nCancionActual + 1
            With lvCanciones
                If nCancionActual >= .Items.Count Then
                    nCancionActual = .Items.Count - 1
                    ' si se está tocando la lista
                    If tocandoLista <> cMediaPlayer.eMCIModo.eStop Then
                        ' Parar el actual,                          (21/Oct/06)
                        ' ya que se puede usar otro objeto
                        If Me.mezclarCanciones Then
                            Me.mPlayMCI.Parar()
                            Me.mPlayMCI2.Parar()
                            Me.esMCI2 = False
                        End If

                        ' indicar que se ha terminado
                        tocandoLista = cMediaPlayer.eMCIModo.eStop
                        ' actualizar el menú contextual             (13/Feb/05)
                        Me.tocandoMinimizado = False

                        If chkRepeat.IsChecked Then
                            ' Si está en Repeat, se pasa al primero
                            .SelectedIndex = 0
                            'CType(.Items(0), ListViewItem).IsSelected = True
                            '.EnsureVisible(0)
                            lvCanciones.ScrollIntoView(lvCanciones.Items(0))
                            btnTocarLista_Click(sender, e)
                            Exit Sub
                        Else
                            ' Se termina la lista                   (07/Abr/07)
                            mINI.IniWrite(sFicIni, "Control remoto", "Tocando lista", tocandoLista.ToString)
                            ' Guardar una cadena vacía en el nombre de la canción
                            mINI.IniWrite(sFicIni, "General", "Tocando", "")
                        End If
                        ' habilitar la lista y el botón recientes   (12/Feb/05)
                        cboListas.IsEnabled = True
                        btnAbrirReciente.IsEnabled = True
                        btnBorrarLista.IsEnabled = True
                        btnAbrirLista.IsEnabled = True
                        btnGuardarLista.IsEnabled = True
                        btnCalcularTiempo.IsEnabled = btnBorrarLista.IsEnabled
                        btnAñadirCanciones.IsEnabled = btnGuardarLista.IsEnabled
                    End If
                End If
                If .Items.Count > nCancionActual Then
                    .SelectedItem = .Items(nCancionActual)
                End If
            End With
            comprobarLista()

            focolvCanciones()
        Catch 'ex As Exception
        End Try
    End Sub
    '
    Private Sub btnAnterior_Click(ByVal sender As Object,
                                  ByVal e As RoutedEventArgs) _
                                  Handles btnAnterior.Click, mnuAnterior.Click
        Try
            ' Tocar la canción anterior
            ' Parar el actual,                          (21/Oct/06)
            ' ya que se puede usar otro objeto
            If Me.mezclarCanciones Then
                Me.mPlayMCI.Parar()
                Me.mPlayMCI2.Parar()
                Me.esMCI2 = False
            End If

            nCancionActual = nCancionActual - 1
            If nCancionActual < 0 Then nCancionActual = 0
            lvCanciones.SelectedItem = lvCanciones.Items(nCancionActual)

            comprobarLista()
            focolvCanciones()
        Catch 'ex As Exception
        End Try
    End Sub
    '
    Private Sub btnPausa_Click(ByVal sender As Object,
                               ByVal e As EventArgs) _
                               Handles btnPausa.Click, mnuPausa.Click
        mPlayMCIactual.Pausa()

        ' marcar que está en pausa (o no)                           (10/Dic/04)
        Dim b As Boolean
        b = (mPlayMCIactual.Mode = cMediaPlayer.eMCIModo.ePausa)

        'CType(contextNotify.Items.Item(OpcionesMenuNotify.Pausa), System.Windows.Forms.ToolStripMenuItem).Checked = b

        ' Esto no se debe asignar tan directamente                  (18/Ago/07)
        ' ya que si se hace pausa cuando solo se toca una,
        ' al pararla desde la pausa, sigue con la lista.
        ' Asignarlo solo si no está parada                          (18/Ago/07)
        If tocandoLista <> cMediaPlayer.eMCIModo.eStop Then
            tocandoLista = mPlayMCIactual.Mode
            mINI.IniWrite(sFicIni, "Control remoto", "Tocando lista", tocandoLista.ToString)
        End If

        focolvCanciones()
    End Sub

    ''' <summary>
    ''' Este método se llama desde el clic del botón,
    ''' también lo usan otros métodos para tocar la siguiente canción
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnTocar_Click(ByVal sender As Object,
                              ByVal e As RoutedEventArgs) _
                              Handles btnTocar.Click
        Dim s As String

        Try
            If lvCanciones.SelectedItems.Count = 0 Then
                lvCanciones.SelectedIndex = 0
            End If
            Dim ifi As ItemFichero = TryCast(lvCanciones.SelectedItem, ItemFichero)
            s = ifi.FullName

            ultimaCancion = lvCanciones.SelectedIndex
            mINI.IniWrite(sFicIni, "Lista", "Ultima cancion", ultimaCancion.ToString)

            nCancionActual = ultimaCancion ' lvCanciones.SelectedIndex

            ' guardar la que se va a tocar (el nombre)              (28/Feb/05)
            mINI.IniWrite(sFicIni, "General", "Tocando", ifi.Nombre)

            ' Guardar el número de canción                          (02/Abr/10)
            mINI.IniWrite(sFicIni, "Control remoto", "Numero cancion", nCancionActual.ToString)

            ' Actualizar la duración                                (19/Ago/07)
            ifi.Duración = tocarCancion(s)
            lvCanciones.ToolTip = ifi.ToString(True)
            lvCanciones.Items.Refresh()

            actualizarBotones()

            ' Guardar los datos cada vez que se toca una canción    (01/Abr/10)
            guardarINI()

        Catch ex As Exception
            MessageBox.Show("ERROR: " & ex.Message,
                            "Error al tocar",
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation)
        End Try
        lblStatus.Content = elGuille2

        focolvCanciones()
    End Sub
    '
    Private Sub btnParar_Click(ByVal sender As Object,
                              ByVal e As RoutedEventArgs) _
                              Handles btnParar.Click, mnuParar.Click
        mPlayMCIactual.Parar()
        btnTocar.IsEnabled = True
        btnPausa.IsEnabled = False
        btnParar.IsEnabled = False
        lvCanciones.Focus()

        ' marcar que no está en pausa                               (10/Dic/04)
        'CType(contextNotify.Items.Item(OpcionesMenuNotify.Pausa), System.Windows.Forms.ToolStripMenuItem).Checked = False
        ' actualizar el menú contextual                             (13/Feb/05)
        tocandoMinimizado = False

        ' borrar la canción del fichero ini                         (28/Feb/05)
        ' aunque si sigue tocando... se guardará la canción
        mINI.IniWrite(sFicIni, "General", "Tocando", "")

        actualizarBotones()

        ' Si se está tocando la lista...
        If tocandoLista <> cMediaPlayer.eMCIModo.eStop Then
            ' Pasar a la siguiente canción
            btnSiguiente_Click(btnSiguiente, Nothing)
            btnTocar_Click(btnTocar, Nothing)
        End If
    End Sub

    Private Sub fgsPlayMCIAPI_FormClosing(ByVal sender As Object,
                ByVal e As System.ComponentModel.CancelEventArgs) _
                Handles Me.Closing ' .FormClosing
        ' El estado de los Expander                                 (17/Ago/07)
        My.Settings.expMenu = Me.expMenu.IsExpanded
        My.Settings.expActual = Me.expActual.IsExpanded
        My.Settings.expListas = Me.expListas.IsExpanded
        My.Settings.expListaActual = Me.expListaActual.IsExpanded
        My.Settings.expOpcionesLista = Me.expOpcionesLista.IsExpanded
        My.Settings.expBotonera = Me.expBotonera.IsExpanded

        Try
            ' Si está tocando, parar la lista
            btnPararLista_Click(btnPararLista, Nothing)
        Catch ex As Exception
#If DEBUG Then
            System.Diagnostics.Debug.WriteLine(ex.Message)
            Stop
#End If
        End Try
        '
        ' Guardar los datos de configuración
        ' tanto en el INI como en los datos del usuario
        guardarINI()
        '
        '
        ' Indicar que está parado                                   (25/Oct/06)
        mINI.IniWrite(sFicIni, "General", "Esta funcionando", "0")
        '
        ' Quitar el icono de la barra de tareas
        'notifyIcon1.Visible = False
        ''
    End Sub

    'Private Sub fgsPlayMCIAPI_Load(ByVal sender As Object, _
    '                               ByVal e As RoutedEventArgs) _
    '                               Handles MyBase.Loaded ' .Load

    '    'Me.Visibility = Windows.Visibility.Hidden

    '    ' Para probar en el Load y en New
    '    'iniciarlizarVentanaPrincipal()

    '    'Me.Visibility = Windows.Visibility.Visible
    'End Sub



    'Private Sub fgsPlayMCIAPI_Move(ByVal sender As Object, _
    '            ByVal e As RoutedEventArgs) Handles Me.Move
    '    If inicializando Then Exit Sub

    '    If mINI IsNot Nothing AndAlso Me.WindowState = FormWindowState.Normal Then
    '        mINI.IniWrite(sFicIni, "General", "Form.Left", Me.Left.ToString)
    '        mINI.IniWrite(sFicIni, "General", "Form.Top", Me.Top.ToString)
    '        My.Settings.FormLeft = Me.Left
    '        My.Settings.FormTop = Me.Top
    '    End If
    'End Sub

    Private Sub Window1_LocationChanged(ByVal sender As Object,
                                        ByVal e As EventArgs) Handles Me.LocationChanged
        If inicializando Then Exit Sub

        If mINI Is Nothing Then
            Exit Sub
        End If

        'If Me.WindowState = WindowState.Normal Then
        '    mINI.IniWrite(sFicIni, "General", "Form.Left", Me.Left.ToString)
        '    mINI.IniWrite(sFicIni, "General", "Form.Top", Me.Top.ToString)
        '    My.Settings.FormLeft = CInt(Me.Left)
        '    My.Settings.FormTop = CInt(Me.Top)
        'Else
        '    mINI.IniWrite(sFicIni, "General", "Form.Left", Me.RestoreBounds.Left.ToString)
        '    mINI.IniWrite(sFicIni, "General", "Form.Top", Me.RestoreBounds.Top.ToString)
        '    My.Settings.FormLeft = CInt(Me.RestoreBounds.Left)
        '    My.Settings.FormTop = CInt(Me.RestoreBounds.Top)
        'End If

        If Me.WindowState <> System.Windows.WindowState.Normal _
        OrElse Me.Visibility <> System.Windows.Visibility.Visible Then
            mINI.IniWrite(sFicIni, "General", "Form.Left", Me.RestoreBounds.Left.ToString)
            mINI.IniWrite(sFicIni, "General", "Form.Top", Me.RestoreBounds.Top.ToString)
            My.Settings.FormLeft = CInt(Me.RestoreBounds.Left)
            My.Settings.FormTop = CInt(Me.RestoreBounds.Top)
        Else
            mINI.IniWrite(sFicIni, "General", "Form.Left", Me.Left.ToString)
            mINI.IniWrite(sFicIni, "General", "Form.Top", Me.Top.ToString)
            My.Settings.FormLeft = CInt(Me.Left)
            My.Settings.FormTop = CInt(Me.Top)
        End If

        ' Solo si no se está acoplando la ventana
        If acoplandoVentana = False Then
            posNormal.X = My.Settings.FormLeft
            posNormal.Y = My.Settings.FormTop
        End If
    End Sub

    ''' <summary>
    ''' Este evento se produce cuando se cambia el tamaño (lógico),
    ''' pero no cuando se minimiza!!!
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub fgsPlayMCIAPI_Resize(ByVal sender As Object,
                ByVal e As RoutedEventArgs) Handles MyBase.SizeChanged ' .Resize

        If inicializando Then Exit Sub

        If Me.WindowState = WindowState.Normal Then
            'If mINI IsNot Nothing Then
            '    mINI.IniWrite(sFicIni, "General", "Form.Left", Me.Left.ToString)
            '    mINI.IniWrite(sFicIni, "General", "Form.Top", Me.Top.ToString)
            'End If
            'My.Settings.FormLeft = CInt(Me.Left)
            'My.Settings.FormTop = CInt(Me.Top)
            My.Settings.FormWidth = CInt(Me.Width)
            My.Settings.FormHeight = CInt(Me.Height)
        Else
            My.Settings.FormWidth = CInt(Me.RestoreBounds.Width)
            My.Settings.FormHeight = CInt(Me.RestoreBounds.Height)
        End If

        adaptarTamañoListaCanciones()
    End Sub

    ''' <summary>
    ''' Este evento se produce al cambiar el valor de WindowState
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Window1_StateChanged(ByVal sender As Object,
                                     ByVal e As System.EventArgs) Handles Me.StateChanged
        If inicializando Then Exit Sub

        ' Mostrar la canción que está tocando cuando se minimiza    (25/Mar/02)
        If WindowState = WindowState.Minimized Then
            ' Cuando se minimice, ocultar el formulario             (27/Abr/02)
            If minimizarEnTaskBar <> 0 Then
                Hide()
            End If
            ' Para cambiar el estado del menú contextual            (25/Oct/06)
            tocandoMinimizado = tocandoMinimizado
        End If

        Dim s As String = "", st As String = ""

        If mPlayMCIactual IsNot Nothing _
        AndAlso mPlayMCIactual.Mode <> cMediaPlayer.eMCIModo.eStop Then
            s = sysIO.Path.GetFileNameWithoutExtension(mPlayMCIactual.FileName)
            st = tituloApp
        Else
            st = esElGuille & ")"
            s = tituloApp
        End If

        If s.Length > 61 Then
            s = " " & s.Substring(s.Length - 61, 61) & " "
        Else
            s = " " & s & " "
        End If
        'If notifyIcon1 IsNot Nothing Then
        '    notifyIcon1.Text = s
        'End If
        If Me.WindowState = WindowState.Minimized Then
            st &= " - " & s
            If st.Length > 63 Then
                st = st.Substring(0, 63)
            End If
        End If
        Me.Title = st
    End Sub


    Private Sub fgsPlayMCIAPI_DragDrop(ByVal sender As Object,
                                       ByVal e As DragEventArgs) _
                                       Handles MyBase.Drop ' .DragDrop
        ' Si está este evento y el del ListView se ejecutan los dos (16/Ago/08)
        addDrop2List(e.Data)
    End Sub

    Private Sub fgsPlayMCIAPI_DragOver(ByVal sender As Object,
                                       ByVal e As DragEventArgs) _
                                       Handles MyBase.DragOver
        e.Effects = DragDropEffects.Copy ' .Effect = DragDropEffects.Copy
    End Sub


    Private Sub mnuOcultarMini_CheckedChanged(ByVal sender As Object,
                                                     ByVal e As RoutedEventArgs) _
                                                     Handles mnuOcultarMini.Checked,
                                                             mnuOcultarMini.Unchecked
        If Me.inicializando Then Exit Sub
        '
        Static yaEstoy As Boolean
        If yaEstoy Then Exit Sub
        yaEstoy = True
        If mnuOcultarMini.IsChecked Then
            minimizarEnTaskBar = 1
        Else
            minimizarEnTaskBar = 0
        End If
        'CType(contextNotify.Items.Item(OpcionesMenuNotify.OcultarMinimizar),
        '        System.Windows.Forms.ToolStripMenuItem).Checked = Me.mnuOcultarMini.IsChecked
        'CType(contextMenuForm.Items(OpcionesMenuNotify.OcultarMinimizar),
        '        MenuItem).IsChecked = Me.mnuOcultarMini.IsChecked

        yaEstoy = False
    End Sub

    ''' <summary>
    ''' La opción de ocultar la aplicación cuando está minimizada
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuOcultarMini_Click(ByVal sender As Object,
                                     ByVal e As EventArgs) _
                                     Handles mnuOcultarMini.Click
        If Me.inicializando Then Exit Sub

        Static yaEstoy As Boolean

        If yaEstoy Then Exit Sub

        yaEstoy = True

        'With CType(contextNotify.Items(OpcionesMenuNotify.OcultarMinimizar), System.Windows.Forms.ToolStripMenuItem)
        '    .Checked = Not .Checked
        '    mnuOcultarMini.IsChecked = .Checked
        '    My.Settings.MinimizarTaskBar = .Checked ' Me.mnuOcultarMini.IsChecked
        '    If Me.WindowState = WindowState.Minimized OrElse Me.IsVisible = False Then
        '        If .Checked Then
        '            Me.Hide()
        '        Else
        '            Me.Show()
        '            ' Sin el bringToFront no se muestra en la barra de tareas
        '            'Me.Topmost = True ' .BringToFront()
        '            Me.Activate()
        '        End If
        '    End If
        'End With
        ' Ajustar también el menú contextual del formulario
        CType(contextMenuForm.Items(OpcionesMenuNotify.OcultarMinimizar),
                MenuItem).IsChecked = Me.mnuOcultarMini.IsChecked

        yaEstoy = False
    End Sub

    Private Sub contextMenuForm_ContextMenuOpening(ByVal sender As Object,
                                                   ByVal e As ContextMenuEventArgs) _
                                                   Handles Me.ContextMenuOpening
        With contextMenuForm.Items
            ' Cambiar el texto de la opción de mostrar/ocultar
            If Me.WindowState = WindowState.Minimized Then
                CType(.Item(OpcionesMenuNotify.Restaurar), MenuItem).Header = "Restaurar"
            Else
                CType(.Item(OpcionesMenuNotify.Restaurar), MenuItem).Header = "Minimizar"
            End If
            CType(.Item(OpcionesMenuNotify.Pausa), MenuItem).IsEnabled = btnPausa.IsEnabled

            ' Habilitar adecuadamente los elementos del menú        (24/Ago/07)
            CType(.Item(OpcionesMenuNotify.AcercaDe), MenuItem).IsEnabled = (estaAcercaDe = False)
            CType(.Item(OpcionesMenuNotify.Config), MenuItem).IsEnabled = (estaConfigurando = False)


            actualizarDatosVolumen(contextMenuForm.Items, OpcionesMenuNotify.BajarVolumen)
        End With
    End Sub
    '
    ' Los eventos del ListView
    '
    'Private Sub ListView1_ColumnClick(ByVal sender As Object, _
    '            ByVal e As System.Windows.Forms.ColumnClickEventArgs) _
    '            Handles lvCanciones.ColumnClick
    '    '
    '    oSorter.ColumnIndex = e.Column
    '    '
    '    If e.Column = 1 OrElse e.Column = 2 Then
    '        oSorter.CompararPor = ListViewColumnSort.TipoCompare.Fecha
    '    Else
    '        oSorter.CompararPor = ListViewColumnSort.TipoCompare.Cadena
    '    End If
    '    If colSorting(e.Column) = SortOrder.Ascending Then
    '        colSorting(e.Column) = SortOrder.Descending
    '    Else
    '        colSorting(e.Column) = SortOrder.Ascending
    '    End If
    '    oSorter.Sorting = colSorting(e.Column)
    '    lvCanciones.Sorting = oSorter.Sorting
    '    lvCanciones.ListViewItemSorter = oSorter
    '    lvCanciones.Sort()
    '    '
    '    lvCanciones.Sorting = SortOrder.None
    '    lvCanciones.ListViewItemSorter = Nothing
    '    '
    '    nCancionActual = 0
    'End Sub

    Private Sub ListView1_KeyDown(ByVal sender As Object,
                                  ByVal e As KeyEventArgs) _
                                  Handles lvCanciones.KeyDown
        ' Si se pulsa la tecla DEL, borrar el seleccionado
        If e.Key = Key.Delete Then
            With lvCanciones
                If .SelectedItems.Count = 0 Then Return

                Dim i As Integer = .SelectedIndex
                ' Borrar todos los seleccionados                    (19/Ago/07)
                For j As Integer = .SelectedItems.Count - 1 To 0 Step -1
                    .Items.Remove(.SelectedItems(j))
                Next
                ' seleccionar el que estaba seleccionado
                If i >= .Items.Count Then
                    i = .Items.Count - 1
                End If
                .SelectedIndex = i
            End With

            guardarListaActual()
        End If
    End Sub

    Private Sub ListView1_DragDrop(ByVal sender As Object,
                                   ByVal e As DragEventArgs) _
                                   Handles lvCanciones.Drop
        ' Añadir los ficheros soltados al Listview
        If e.Data.GetDataPresent("FileDrop") Then
            ' Si está este evento y el del formulario,              (16/Ago/08)
            ' se ejecutan los dos
            'addDrop2List(e.Data)
        End If
    End Sub

    Private Sub ListView1_DragEnter(ByVal sender As Object,
                                    ByVal e As DragEventArgs) _
                                    Handles lvCanciones.DragEnter
        If e.Data.GetDataPresent("FileDrop") Then
            e.Effects = DragDropEffects.Copy
        End If
    End Sub

    ' Para saber si ha pulsado en la lista con el ratón             (15/Sep/07)
    Private pulsaEnListaRaton As DateTime = DateTime.Now

    'Private Sub lvCanciones_MouseDown(ByVal sender As Object, _
    '                                  ByVal e As MouseButtonEventArgs) _
    '                                  Handles lvCanciones.MouseDown
    '    If mPlayMCIactual.EstadoActual <> cMediaPlayer.eMCIModo.eStop Then
    '        pulsaEnListaRaton = DateTime.Now
    '    End If
    'End Sub

    ''' <summary>
    ''' Para saber si se ha seleccionado la canción con el ratón
    ''' o es porque ha cambiado mientras está tocando.
    ''' Hacer la comprobación en Preview porque en el otro no se produce.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>15/Sep/07</remarks>
    Private Sub lvCanciones_PreviewMouseDown(ByVal sender As Object,
                                             ByVal e As MouseButtonEventArgs) _
                                             Handles lvCanciones.PreviewMouseDown
        If mPlayMCIactual.EstadoActual <> cMediaPlayer.eMCIModo.eStop Then
            pulsaEnListaRaton = DateTime.Now
        End If
    End Sub

    ''' <summary>
    ''' Este evento se produce cuando cambia la selección
    ''' y la selección cambia cada vez que se toca una canción o se pulsa en la lista
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ListView1_SelectedIndexChanged(ByVal sender As Object,
                                               ByVal e As RoutedEventArgs) _
                                               Handles lvCanciones.SelectionChanged ' .SelectedIndexChanged
        If lvCanciones.SelectedItems.Count > 0 Then
            ' Comprobar si está tocando y se pulsa con el ratón     (15/Sep/07)
            ' No tenerlo en cuenta si ha pasado menos de un segundo
            If mPlayMCIactual.EstadoActual <> cMediaPlayer.eMCIModo.eStop Then
                If DateTime.Now.Subtract(pulsaEnListaRaton).Seconds < 1 Then
                    Exit Sub
                End If
            End If
            ' hacer que el elemento seleccionado esté visible
            lvCanciones.ScrollIntoView(lvCanciones.SelectedItem)

            ' Mostrar los datos de la canción seleccionada
            Dim ifi As ItemFichero = TryCast(lvCanciones.SelectedItem, ItemFichero)
            Me.lblNombre.Content = ifi.Nombre
            Me.lblFecha.Content = ifi.FechaList '& " - "
            Me.lblTamaño.Content = ifi.TamañoList '.TrimStart() & " - "
            Me.lblFullName.Content = ifi.FullName

            lvCanciones.ToolTip = ifi.ToString(True)
        End If
    End Sub
    '
    '
    Private Sub scrollPosicion_ValueChanged(ByVal sender As Object,
                                            ByVal e As RoutedEventArgs) _
                                            Handles scrollPosicion.ValueChanged
        If cambiandoScroll Then Exit Sub

        ' Comprobar que esté instanciado                            (07/Jun/21)
        If mPlayMCIactual Is Nothing Then Return

        cambiandoScroll = True
        Try
            mPlayMCIactual.CurrentPosition = Convert.ToInt32(scrollPosicion.Value * pasoScroll)
            lblInfo2.Content = mPlayMCIactual.TiempoRestante
        Catch
            '
        End Try
        cambiandoScroll = False
    End Sub

    Private Sub btnSilencio_Click(ByVal sender As Object,
                                  ByVal e As EventArgs) _
                                  Handles btnSilencio.Click, mnuSilencio.Click
        callado = Not callado
        If callado Then
            picVol.Source = New BitmapImage(New Uri($"{Application.DirResources}/volumenOff.ico"))

            volAct = mPlayMCIactual.Volumen
            trackVol.IsEnabled = False
            mPlayMCIactual.Volumen = volMin
        Else
            picVol.Source = New BitmapImage(New Uri($"{Application.DirResources}/volumenOn.ico"))

            mPlayMCIactual.Volumen = volAct
            trackVol.IsEnabled = True
        End If

        My.Settings.silencioTotal = callado
    End Sub

    Private Sub trackVol_ValueChanged(ByVal sender As Object,
                                      ByVal e As RoutedEventArgs) _
                                      Handles trackVol.ValueChanged
        If Me.inicializando Then Exit Sub

        mPlayMCIactual.Volumen = CInt(trackVol.Value * volMax / cMaxTrack)
        trackVol.ToolTip = " " & mPlayMCIactual.Volumen.ToString & " (" & trackVol.Value * cMaxTrack & "%)"
        mINI.IniWrite(sFicIni, "General", "Volumen", trackVol.Value * cMaxTrack & "%")
    End Sub

    ' para las opciones del menú de subir y bajar el volumen        (21/Feb/04)
    Private Sub bajarVolumen(ByVal sender As Object,
                             ByVal e As EventArgs) _
                             Handles btnBajarVolumen.Click, mnuBajarVol.Click
        Try
            trackVol.Value -= trackVol.SmallChange
        Catch 'ex As Exception
        End Try
    End Sub

    Private Sub subirVolumen(ByVal sender As Object,
                             ByVal e As EventArgs) _
                             Handles btnSubirVolumen.Click, mnuSubirVol.Click
        Try
            trackVol.Value += trackVol.SmallChange
        Catch 'ex As Exception
        End Try
    End Sub
    '
    Private Sub btnBajar_Click(ByVal sender As Object,
                               ByVal e As RoutedEventArgs) Handles btnBajar.Click
        mover(Me.lvCanciones, False)
    End Sub

    Private Sub btnSubir_Click(ByVal sender As Object,
                               ByVal e As RoutedEventArgs) Handles btnSubir.Click
        mover(Me.lvCanciones, True)
    End Sub

    Private Sub btnAbrirReciente_Click(ByVal sender As Object,
                                       ByVal e As RoutedEventArgs) Handles btnAbrirReciente.Click
        sNombreLista = cboListas.Text
        lvCanciones.Items.Clear()
        abrirLista()
    End Sub


    ' Para el menú contextual
    Private Sub reiniciarLista(ByVal sender As Object,
                               ByVal e As EventArgs) _
                               Handles btnReiniciarLista.Click, mnuReiniciarLista.Click
        reiniciarLista()
    End Sub

    Private Sub btnCalcularTiempo_Click(ByVal sender As Object,
                                        ByVal e As RoutedEventArgs) Handles btnCalcularTiempo.Click
        ' Mostrar el tiempo total de la lista actual                (05/May/05)
        Static yaEstoy As Boolean
        If yaEstoy Then Exit Sub

        Dim s As String = btnCalcularTiempo.ToolTip.ToString
        Dim img As Object = btnCalcularTiempo.Content

        btnCalcularTiempo.Content = "..."
        btnCalcularTiempo.ToolTip = "Calculando..."
        lblStatus.Content = "Calculando el tiempo total..."
        DoEvents()

        ' para los totales de tiempo
        Dim t1, tot As Integer
        '
        For i As Integer = 0 To lvCanciones.Items.Count - 1
            Dim ifi As ItemFichero = TryCast(lvCanciones.Items(i), ItemFichero)
            ifi.Duración = cMediaPlayer.DuracionStr(ifi.FullName, t1)
            tot += t1
        Next

        btnCalcularTiempo.Content = img
        btnCalcularTiempo.ToolTip = s

        lvCanciones.Items.Refresh()

        yaEstoy = False

        ' Mostrar el número de ficheros
        lblStatus.Content = String.Format(" Hay {0} ficheros, duración total: {1}.",
                                          lvCanciones.Items.Count.ToString, milisegundos2Display(tot))

        ' Actualizar la info de la lista actual         v2.0.0.0291 (24/Ago/07)
        Me.labelListaActual.ToolTip = lblStatus.Content.ToString
    End Sub

    Private Sub btnAñadirCanciones_Click(ByVal sender As Object,
                                         ByVal e As RoutedEventArgs) _
                                         Handles btnAñadirCanciones.Click, mnuSelFicheros.Click
        ' Añadir canciones a la lista actual                        (26/Jun/06)
        With oFD
            .Title = "Seleccionar canciones a añadir a la lista"
            .Filter = "Música (*.mp3;*.wav;*.wma)|*.mp3;*.wav;*.wma|Todos los archivos (*.*)|*.*"
            .Multiselect = True
            If .ShowDialog() Then
                ' Clasificar los ficheros seleccionados             (18/Dic/07)
                array2Lista(.FileNames, True)
            End If
        End With

        focolvCanciones()
    End Sub

    ''' <summary>
    ''' Comprobar si se cambia remotamente
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub timerFeb_Elapsed(ByVal sender As Object, ByVal e As ElapsedEventArgs) Handles timerFeb.Elapsed
        Me.Dispatcher.Invoke(
                             DispatcherPriority.Normal,
                             New InvocarCallback(AddressOf timerRemotoActualizar))
    End Sub

    '''' <summary>
    '''' Este evento será para controlar cuando restaurar o minimizar la ventana
    '''' con idea de no usar los hilos
    '''' porque en VB2005 da error de acceso a controles, etc.
    '''' </summary>
    '''' <param name="sender"></param>
    '''' <param name="e"></param>
    '''' <remarks></remarks>
    'Private Sub timerHilos_Elapsed(ByVal sender As Object, ByVal e As ElapsedEventArgs) Handles timerHilos.Elapsed
    '    timerHilos.Enabled = False
    '    If esRestaurar = True Then
    '        'restaurarThread()
    '        Me.Dispatcher.Invoke(DispatcherPriority.Normal, New InvocarCallback(AddressOf restaurarThread))
    '    Else
    '        'minimizarThread()
    '        Me.Dispatcher.Invoke(DispatcherPriority.Normal, New InvocarCallback(AddressOf minimizarThread))
    '    End If
    'End Sub

    Private Sub expMenu_Collapsed(ByVal sender As Object,
                                  ByVal e As RoutedEventArgs) _
                                  Handles expMenu.Collapsed
        If inicializando Then Exit Sub
        mnuPrincipal.Visibility = System.Windows.Visibility.Collapsed
    End Sub

    Private Sub expMenu_Expanded(ByVal sender As Object,
                                 ByVal e As RoutedEventArgs) _
                                 Handles expMenu.Expanded
        If inicializando Then Exit Sub
        mnuPrincipal.Visibility = System.Windows.Visibility.Visible
    End Sub

    Private Sub mnuFichero_SubmenuOpened(ByVal sender As Object,
                                         ByVal e As RoutedEventArgs) _
                                         Handles mnuFichero.SubmenuOpened
        Me.mnuGuardarLista.IsEnabled = Me.lvCanciones.Items.Count > 0
    End Sub

    Private Sub mnuVer_SubmenuOpened(ByVal sender As Object,
                                     ByVal e As RoutedEventArgs) _
                                     Handles mnuVer.SubmenuOpened
        ' Mostrar la info del estado actual de los paneles
        Dim sb As New System.Text.StringBuilder
        sb.Append("Estado actual de los paneles:")
        For Each exp As Expander In paneles
            sb.AppendLine()
            sb.AppendFormat("   {0} está {1}", exp.Header, If(exp.IsExpanded, "expandido", "contraído"))
        Next
        Me.mnuCerrarPaneles.ToolTip = sb.ToString
    End Sub

    Private Sub mnuReproduccion_SubmenuOpened(ByVal sender As Object,
                                              ByVal e As RoutedEventArgs) _
                                              Handles mnuReproduccion.SubmenuOpened
        ' Los botones de siguiente, etc. solo si hay canciones en la lista
        actualizarBotones()

        ' Ajustar la disponibiliad de los botones
        Me.mnuTocarLista.IsEnabled = Me.btnTocarLista.IsEnabled
        Me.mnuPararLista.IsEnabled = Me.btnPararLista.IsEnabled
        Me.mnuPausa.IsEnabled = Me.btnPausa.IsEnabled
        Me.mnuParar.IsEnabled = Me.btnParar.IsEnabled

        Me.mnuReiniciarLista.IsEnabled = Me.btnReiniciarLista.IsEnabled

    End Sub

    Private Sub mnuVolumen_SubmenuOpened(ByVal sender As Object,
                                         ByVal e As RoutedEventArgs) _
                                         Handles mnuVolumen.SubmenuOpened
        ' Ajustar el texto del volumen, etc.
        actualizarDatosVolumen(mnuVolumen.Items, 0)
    End Sub

    ''' <summary>
    ''' Ocultar/mostrar los paneles
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuCerrarPaneles_Click(ByVal sender As Object,
                                       ByVal e As RoutedEventArgs) _
                                       Handles mnuCerrarPaneles.Click
        'panelesExpandidos = Not panelesExpandidos
        For Each exp As Expander In paneles
            exp.IsExpanded = Not exp.IsExpanded
        Next
    End Sub

    Private Sub btnClasificar_Click(ByVal sender As Object,
                                    ByVal e As RoutedEventArgs) _
                                    Handles btnClasificar.Click

        ' Clasificar el contenido del ListView

        ' Usar las clases de WPF para clasificar por la columna

        Dim nombreCab As String = ""

        Me.lblStatus.Tag = Me.lblStatus.Content
        Select Case True
            Case mnuClasificarDirectorio.IsChecked
                ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Directorio
            Case mnuClasificarDuración.IsChecked
                ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Duración
            Case mnuClasificarFecha.IsChecked
                ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Fecha
            Case mnuClasificarTamaño.IsChecked
                ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Tamaño
            Case mnuClasificarFullName.IsChecked
                ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.FullName
                ' Hay que usar el mismo nombre de la cabecera del ListView
                'nombreCab = "Nombre completo"
            Case Else
                ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Nombre
        End Select
        My.Settings.campoClasificar = ItemFichero.CampoClasificar

        If String.IsNullOrEmpty(nombreCab) Then
            nombreCab = ItemFichero.CampoClasificar.ToString
        End If

        clasificarListView(nombreCab, Nothing)

    End Sub

    Private Sub mnuClasificar_Click(ByVal sender As Object,
                                    ByVal e As RoutedEventArgs) _
                                    Handles mnuClasificarNombre.Click, mnuClasificarDirectorio.Click,
                                            mnuClasificarDuración.Click, mnuClasificarFecha.Click,
                                            mnuClasificarTamaño.Click, mnuClasificarFullName.Click
        If inicializando Then Exit Sub

        Dim mnui As MenuItem = TryCast(sender, MenuItem)
        If mnui Is Nothing Then Exit Sub

        ' Si ya está marcado, no hacer nada
        If mnui.IsChecked Then Exit Sub

        For Each mnu As MenuItem In btnClasificar.ContextMenu.Items
            mnu.IsChecked = False
        Next

        mnui.IsChecked = True

        If mnui Is mnuClasificarDirectorio Then
            ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Directorio
        ElseIf mnui Is mnuClasificarDuración Then
            ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Duración
        ElseIf mnui Is mnuClasificarFecha Then
            ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Fecha
        ElseIf mnui Is mnuClasificarTamaño Then
            ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Tamaño
        ElseIf mnui Is mnuClasificarNombre Then
            ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.Nombre
        Else
            ItemFichero.CampoClasificar = ItemFichero.CamposClasificar.FullName
        End If

        My.Settings.campoClasificar = ItemFichero.CampoClasificar

        Me.btnClasificar.ToolTip = mnui.Header.ToString ' "Clasificar la lista por " & ItemFichero.CampoClasificar.ToString
    End Sub

    Private Sub btnClasificar_ContextMenuOpening(ByVal sender As Object,
                                                 ByVal e As ContextMenuEventArgs) _
                                                 Handles btnClasificar.ContextMenuOpening

        ' Mostrar si es ascendente o descendente
        Dim nombreCab As String = ItemFichero.CampoClasificar.ToString
        If Me.cabeceraSort.ContainsKey(nombreCab) = False Then
            cabeceraSort.Add(nombreCab, ListSortDirection.Descending)
        End If
        Dim sortDir As ListSortDirection = cabeceraSort(nombreCab)
        Dim sOrden As String
        If sortDir = ListSortDirection.Ascending Then
            sOrden = "(descendente)"
        Else
            sOrden = "(ascendente)"
        End If

        Me.btnClasificar.ToolTip = "Clasificar la lista por " &
                                    ItemFichero.CampoClasificar.ToString &
                                    " " & sOrden

        With btnClasificar.ContextMenu
            For i As Integer = 0 To .Items.Count - 1
                Dim mnu As MenuItem = TryCast(.Items(i), MenuItem)
                nombreCab = nombresCampos(i)
                If i = nombresCampos.Length - 1 Then
                    nombreCab = "FullName"
                End If
                If Me.cabeceraSort.ContainsKey(nombreCab) = False Then
                    cabeceraSort.Add(nombreCab, ListSortDirection.Descending)
                End If
                sortDir = cabeceraSort(nombreCab)
                If sortDir = ListSortDirection.Ascending Then
                    sOrden = " (descendente)"
                Else
                    sOrden = " (ascendente)"
                End If
                mnu.Header = "Clasificar por " & nombresCampos(i) & sOrden
            Next
        End With
    End Sub

    'Private Sub mnuInfoCompleta_Click(ByVal sender As Object, _
    '                                  ByVal e As RoutedEventArgs) _
    '                                  Handles mnuInfoCompleta.Click
    '    mnuInfoCompleta.IsChecked = Not mnuInfoCompleta.IsChecked
    '    ItemFichero.InfoCompleta = mnuInfoCompleta.IsChecked
    '    lvCanciones.Items.Refresh()

    '    My.Settings.InfoCompleta = ItemFichero.InfoCompleta
    'End Sub

    'Private Sub mnuInfoUnaLinea_Click(ByVal sender As Object, _
    '                                  ByVal e As RoutedEventArgs) _
    '                                  Handles mnuInfoUnaLinea.Click
    '    Me.mnuInfoUnaLinea.IsChecked = Not Me.mnuInfoUnaLinea.IsChecked
    '    ItemFichero.InfoUnaLinea = Me.mnuInfoUnaLinea.IsChecked
    '    lvCanciones.Items.Refresh()

    '    My.Settings.InfoUnaLinea = Me.mnuInfoUnaLinea.IsChecked
    'End Sub

    ' Los eventos para acoplar la ventana a los lados
    ' En realidad no se cómo usarlo, ya que la idea es que si se acopla abajo,
    ' que se muestre con el tamaño mínimo (por hacer)
    '
    ' En principio acomplarlo a los costados...
    Private Sub mnuAcoplarIzquierda_Click(ByVal sender As Object,
                                          ByVal e As RoutedEventArgs) _
                                          Handles mnuAcoplarIzquierda.Click
        acoplandoVentana = True
        Me.Left = 0
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarDerecha_Click(ByVal sender As Object,
                                        ByVal e As RoutedEventArgs) _
                                        Handles mnuAcoplarDerecha.Click
        acoplandoVentana = True
        Dim l As Double = 0 ' My.Computer.Screen.Bounds.Width - Me.Width
        Me.Left = l
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarArriba_Click(ByVal sender As Object,
                                       ByVal e As RoutedEventArgs) _
                                       Handles mnuAcoplarArriba.Click
        acoplandoVentana = True
        Me.Top = 0
        'Me.Left = 0
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarAbajo_Click(ByVal sender As Object,
                                      ByVal e As RoutedEventArgs) _
                                      Handles mnuAcoplarAbajo.Click
        acoplandoVentana = True
        Dim t As Double = 0 ' My.Computer.Screen.WorkingArea.Height - Me.Height
        Me.Top = t
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarNormal_Click(ByVal sender As Object,
                                       ByVal e As RoutedEventArgs) _
                                       Handles mnuAcoplarNormal.Click
        acoplandoVentana = True
        Me.Top = posNormal.Y
        Me.Left = posNormal.X
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarAbajoCen_Click(ByVal sender As Object,
                                         ByVal e As RoutedEventArgs) _
                                         Handles mnuAcoplarAbajoCen.Click
        acoplandoVentana = True
        Dim l As Double = 0 ' (My.Computer.Screen.Bounds.Width - Me.Width) / 2
        Dim t As Double = 0 ' My.Computer.Screen.WorkingArea.Height - Me.Height
        Me.Left = l
        Me.Top = t
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarAbajoDer_Click(ByVal sender As Object,
                                         ByVal e As RoutedEventArgs) _
                                         Handles mnuAcoplarAbajoDer.Click
        acoplandoVentana = True
        Dim l As Double = 0 ' My.Computer.Screen.Bounds.Width - Me.Width
        Dim t As Double = 0 ' My.Computer.Screen.WorkingArea.Height - Me.Height
        Me.Left = l
        Me.Top = t
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarAbajoIzq_Click(ByVal sender As Object,
                                         ByVal e As RoutedEventArgs) _
                                         Handles mnuAcoplarAbajoIzq.Click
        acoplandoVentana = True
        Dim t As Double = 0 ' My.Computer.Screen.WorkingArea.Height - Me.Height
        Me.Left = 0
        Me.Top = t
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarArribaCen_Click(ByVal sender As Object,
                                          ByVal e As RoutedEventArgs) _
                                          Handles mnuAcoplarArribaCen.Click
        acoplandoVentana = True
        Dim l As Double = 0 ' (My.Computer.Screen.Bounds.Width - Me.Width) / 2
        Me.Left = l
        Me.Top = 0
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarArribaDer_Click(ByVal sender As Object,
                                          ByVal e As RoutedEventArgs) _
                                          Handles mnuAcoplarArribaDer.Click
        acoplandoVentana = True
        Dim l As Double = 0 ' My.Computer.Screen.Bounds.Width - Me.Width
        Me.Left = l
        Me.Top = 0
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarArribaIzq_Click(ByVal sender As Object,
                                          ByVal e As RoutedEventArgs) _
                                          Handles mnuAcoplarArribaIzq.Click
        acoplandoVentana = True
        Me.Left = 0
        Me.Top = 0
        acoplandoVentana = False
    End Sub

    Private Sub mnuAcoplarCentro_Click(ByVal sender As Object,
                                       ByVal e As RoutedEventArgs) _
                                       Handles mnuAcoplarCentro.Click
        acoplandoVentana = True
        Dim l As Double = 0 ' (My.Computer.Screen.Bounds.Width - Me.Width) / 2
        Dim t As Double = 0 ' (My.Computer.Screen.WorkingArea.Height - Me.Height) / 2
        Me.Left = l
        Me.Top = t
        acoplandoVentana = False
    End Sub

    Private Sub mnuConfigColores_Click(ByVal sender As Object,
                                       ByVal e As RoutedEventArgs) _
                                       Handles mnuConfigColores.Click

        ' Evitar la reentrada desde el menú contextual              (24/Ago/07)
        If estaConfigurando Then Exit Sub

        estaConfigurando = True

        Dim wCfg As New wColoresCfg
        If wCfg.ShowDialog Then
            ' Asignar los colores
            With My.Settings
                .fondoVentana = ColoresGradient.ToString(Me.gridMain.Background)
                .fondoActual = ColoresGradient.ToString(Me.expActual.Background)
                .letraActual = CType(Me.expActual.Foreground, SolidColorBrush)
                .fondoListas = ColoresGradient.ToString(Me.expListas.Background)
                .letraListas = CType(Me.expListas.Foreground, SolidColorBrush)
                .fondoListaActual = ColoresGradient.ToString(Me.expListaActual.Background)
                .letraListaActual = CType(Me.expListaActual.Foreground, SolidColorBrush)
                .fondoOpcionesListaActual = ColoresGradient.ToString(Me.expOpcionesLista.Background)
                .letraOpcionesListaActual = CType(Me.expOpcionesLista.Foreground, SolidColorBrush)
                .fondoBotonera = ColoresGradient.ToString(Me.expBotonera.Background)
                .letraBotonera = CType(Me.expBotonera.Foreground, SolidColorBrush)
                .Save()
            End With
        Else
            With My.Settings
                If String.IsNullOrEmpty(My.Settings.fondoVentana) = False Then
                    Me.gridMain.Background = ColoresGradient.Parse(.fondoVentana)
                    Me.expActual.Background = ColoresGradient.Parse(.fondoActual)
                    Me.expActual.Foreground = .letraActual
                    Me.expListas.Background = ColoresGradient.Parse(.fondoListas)
                    Me.expListas.Foreground = .letraListas
                    Me.expListaActual.Background = ColoresGradient.Parse(.fondoListaActual)
                    Me.expListaActual.Foreground = .letraListaActual
                    Me.expOpcionesLista.Background = ColoresGradient.Parse(.fondoOpcionesListaActual)
                    Me.expOpcionesLista.Foreground = .letraOpcionesListaActual
                    Me.expBotonera.Background = ColoresGradient.Parse(.fondoBotonera)
                    Me.expBotonera.Foreground = .letraBotonera
                End If
            End With
        End If

        estaConfigurando = False
    End Sub

    Private Sub expActual_SizeChanged(ByVal sender As Object,
                                      ByVal e As SizeChangedEventArgs) _
                                      Handles expActual.SizeChanged,
                                              expListas.SizeChanged,
                                              expOpcionesLista.SizeChanged
        If inicializando Then Exit Sub

        If e.HeightChanged Then
            adaptarTamañoListaCanciones()
        End If
    End Sub

    Private Sub lvCanciones_ColumnHeaderClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim cabecera As GridViewColumnHeader = TryCast(e.OriginalSource, GridViewColumnHeader)
        If cabecera Is Nothing Then Exit Sub

        ' Si es una columna "de relleno", salir
        If cabecera.Role = GridViewColumnHeaderRole.Padding Then
            Exit Sub
        End If

        Dim nombreCab As String = cabecera.Content.ToString
        ' Siempre hay que usar el nombre del campo al que está ligado
        If nombreCab = "Nombre completo" Then
            nombreCab = "FullName"
        End If

        clasificarListView(nombreCab, cabecera)

    End Sub

    Private Sub timerAbrirLista_Elapsed(ByVal sender As Object,
                                        ByVal e As ElapsedEventArgs) _
                                        Handles timerAbrirLista.Elapsed

        ' Solo lo necesitamos una vez
        timerAbrirLista.Enabled = False

        Me.Dispatcher.Invoke(DispatcherPriority.Normal, _
                             New InvocarCallback(AddressOf abrirLista))
        'abrirLista()
        Me.Dispatcher.Invoke(DispatcherPriority.Normal, _
                             New InvocarCallback(AddressOf seleccionarUltimaCancion))
        'If ultimaCancion > 0 AndAlso Me.lvCanciones.Items.Count > ultimaCancion Then
        '    lvCanciones.SelectedIndex = ultimaCancion
        'End If
        If esLineaComandos Then
            Me.Dispatcher.Invoke(DispatcherPriority.Normal, _
                                 New RoutedEventHandler( _
                                                        AddressOf btnTocarLista_Click), _
                                                        Nothing, New Object() {Nothing})
            'Me.btnTocarLista_Click(Nothing, Nothing)
        End If

    End Sub

    ''' <summary>
    ''' En un método para llamarlo desde un delegado
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub seleccionarUltimaCancion()
        If ultimaCancion > 0 AndAlso Me.lvCanciones.Items.Count > ultimaCancion Then
            lvCanciones.SelectedIndex = ultimaCancion
        End If
    End Sub
End Class
