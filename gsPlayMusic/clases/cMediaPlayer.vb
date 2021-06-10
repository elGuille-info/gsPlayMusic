'------------------------------------------------------------------------------
' cMediaPlayer                                                      (07/Jun/21)
' Clase para tocar música usando la clase MediaPlayer
'
' (c)Guillermo Som (elGuille), 2021
'------------------------------------------------------------------------------
Option Strict On
Option Infer On

Imports Microsoft.VisualBasic

Imports System
Imports System.Collections.Generic
Imports System.Linq

Imports System.Windows
Imports System.Windows.Media
Imports Microsoft.Win32
Imports System.Windows.Threading

Public Class cMediaPlayer

    Public Enum eMCINotify
        mciNotifySuccessful = 1 ' El comando se completó satisfactoriamente.
        mciNotifySuperseded = 2 ' El comando se sustituyó por otro comando.
        mciAborted = 4          ' El usuario anuló el comando.
        mciFailure = 8          ' El comando falló.
    End Enum
    '
    Public Enum eMCIMode
        mciModeIDLE = 0
        mciModeNotOpen = 524    ' El dispositivo no está abierto.
        mciModeStop = 525       ' Parar dispositivo.
        mciModePlay = 526       ' Reproducir dispositivo.
        mciModeRecord = 527     ' Grabar dispositivo.
        mciModeSeek = 528       ' Buscar dispositivo.
        mciModePause = 529      ' Pausa dispositivo.
        mciModeReady = 530      ' Dispositivo preparado.
    End Enum

    Private WithEvents MCICtl As New MediaPlayer()

    Private intervalo As Integer = 900

    ' La clase DispatcherTimer está definida en el espacio de nombres:
    ' Windows.UI.Xaml
    Private timer1 As DispatcherTimer

    ''' <summary>
    ''' Este evento es del temporizador y
    ''' se ejecutará en cada intervalo.
    ''' </summary>
    ''' <remarks>
    ''' Es conveniente no usar un valor muy pequeño para el intervalo
    ''' lo recomendable es un poco menos de 1 segundo (1000 milisegundos)
    ''' En el programa utilizo un valor de 900 milisegundos
    ''' </remarks>
    Private Sub timer1_Tick(sender As Object, e As Object) 'Handles timer1.Tick
        ' Aquí se puede comprobar la duración de la canción que está sonando, etc.

        If m_EstadoActual = eMCIModo.ePlay Then
            Dim p = ComprobarDuration() - MCICtl.Position.TotalSeconds
            RaiseEvent MilisegundosRestantes(CInt(p * 1000))
            RaiseEvent StatusUpdate()
        End If

    End Sub

    Public Sub New()
        AddHandler MCICtl.Changed, AddressOf OnChanged
        AddHandler MCICtl.MediaEnded, AddressOf OnEnded
        AddHandler MCICtl.MediaFailed, AddressOf OnFailed
        AddHandler MCICtl.MediaOpened, AddressOf OnOpened

        ' crear el temporizador
        timer1 = New DispatcherTimer
        AddHandler timer1.Tick, AddressOf timer1_Tick
        ' el intervalo: 900 milisegundos (algo menos de 1 segundo (1seg = 1000ms))
        timer1.Interval = New TimeSpan(0, 0, 0, 0, intervalo)
        timer1.Start()
    End Sub

    Public Event Changed(sender As Object, e As EventArgs)
    Public Event Done(NotifyCode As eMCINotify)
    Public Event MilisegundosRestantes(milisegundos As Integer)
    Public Event StatusUpdate()

    Private Sub OnChanged(sender As Object, e As EventArgs)
        'RaiseEvent MilisegundosRestantes()
        RaiseEvent Changed(Me, e)
    End Sub
    Private Sub OnEnded(sender As Object, e As EventArgs)
        RaiseEvent Done(eMCINotify.mciNotifySuccessful)
        timer1.Stop()
    End Sub
    Private Sub OnFailed(sender As Object, e As EventArgs)
        RaiseEvent Done(eMCINotify.mciFailure)
        timer1.Stop()
    End Sub
    Private Sub OnOpened(sender As Object, e As EventArgs)
        Debug.WriteLine("OnOpened")
        'timer1.Start()
    End Sub

    ''' <summary>
    ''' Asigna u obtiene el volumen actual.
    ''' </summary>
    Public Property Volumen() As Integer
        Get
            Return CInt(MCICtl.Volume * 10)
        End Get
        Set(value As Integer)
            MCICtl.Volume = value / 10
        End Set
    End Property


    ' El valor máximo y mínimo del volumen                          (24/Ago/02)
    ''' <summary>
    ''' El valor máximo del volumen(10).
    ''' </summary>
    Public ReadOnly Property VolMax As Integer
        Get
            Return 10 'CInt(MCICtl.Volume * 10) '. volCtrl.lMaximum
        End Get
    End Property
    ''' <summary>
    ''' El valor mínimo del volumen (0).
    ''' </summary>
    Public ReadOnly Property VolMin As Integer
        Get
            Return 0 'volCtrl.lMinimum
        End Get
    End Property
    ''' <summary>
    ''' Obtiene o asigna la posición actual.
    ''' </summary>
    ''' <remarks>Se indica en milisegundos.</remarks>
    Public Property CurrentPosition() As Integer
        Get
            Try
                Return CInt(MCICtl.Position.TotalMilliseconds) ' .CurrentPosition
            Catch ex As Exception
                Return 0
            End Try
        End Get
        Set(value As Integer)
            Try
                Dim ts As New TimeSpan(0, 0, 0, 0, value)
                MCICtl.Position = ts

            Catch ex As Exception
                Debug.WriteLine(ex.Message)
            End Try
        End Set
    End Property
    ''' <summary>
    ''' La duración en milisegundos.
    ''' </summary>
    Public ReadOnly Property Duration As Integer
        Get
            Return ComprobarDuration() * 1000
        End Get
    End Property
    ''' <summary>
    ''' Comprueba la duración y devuelve el total de segundos.
    ''' </summary>
    ''' <remarks>También lo asigna a m_durTotal.</remarks>
    Private Function ComprobarDuration() As Integer
        Dim ts = ComprobarDuration(MCICtl.NaturalDuration)
        m_durTotal = ts.TotalSeconds
        Return CInt(m_durTotal)

        'If MCICtl.NaturalDuration.HasTimeSpan Then
        '    m_durTotal = MCICtl.NaturalDuration.TimeSpan.TotalSeconds
        'Else
        '    System.Diagnostics.Debug.WriteLine(MCICtl.NaturalDuration.ToString)
        '    Dim sd = MCICtl.NaturalDuration.ToString
        '    If sd <> "" AndAlso sd.Contains(":") Then
        '        Dim ts = New TimeSpan
        '        TimeSpan.TryParse(sd, ts)
        '        m_durTotal = ts.TotalSeconds
        '    End If
        'End If

        'Return CInt(m_durTotal)
    End Function

    ''' <summary>
    ''' Comprueba el objeto de tipo <see cref="System.Windows.Duration" /> por si tiene TimeSpan.
    ''' </summary>
    ''' <param name="wDuration"></param>
    ''' <returns>
    ''' Si tiene TimeSpan el contenido de wDuration.TimeSpan, si no, el valor convertido de wDuration.ToString()
    ''' </returns>
    Private Shared Function ComprobarDuration(wDuration As System.Windows.Duration) As TimeSpan
        Dim ts As New TimeSpan
        If wDuration.HasTimeSpan Then
            ts = wDuration.TimeSpan
        Else
            System.Diagnostics.Debug.WriteLine(wDuration.ToString)
            Dim sd = wDuration.ToString
            If sd <> "" AndAlso sd.Contains(":") Then
                TimeSpan.TryParse(sd, ts)
            End If
        End If
        Return ts
    End Function

    Private m_modoTiempo As Boolean

    Public ReadOnly Property TiempoRestante As String
        Get
            ' Esta propiedad devuelve el tiempo restante o la duración total,
            ' según el valor de modoTiempo

            ' Cuando se cambia el modo, mostrar el tiempo total
            Static durTmp As TimeSpan 'Integer 'Currency

            'If m_modoTiempo Then
            '    ' TODO: Averiguar cómo se sabe lo que lleva tocando
            '    ' Lo que resta... por hacer
            '    durTmp = MCICtl.NaturalDuration.TimeSpan ' (MCICtl.Length - MCICtl.CurrentPosition) \ 1000
            'Else
            '    durTmp = MCICtl.NaturalDuration.TimeSpan ' MCICtl.Length \ 1000 ' Int(MCICtl.Length / 1000 + 0.5)
            'End If

            ''durMinutos = durTmp.Minutes 'Fix(durTmp \ 60)
            ''durSegundos = durTmp.Seconds ' durTmp - durMinutos * 60
            ''m_TiempoRestante = durMinutos.ToString("00") & "." & durSegundos.ToString("00")

            durTmp = ComprobarDuration(MCICtl.NaturalDuration)

            Return durTmp.ToString("mm.ss")
        End Get
    End Property

    ''' <summary>
    ''' El tiempo total en minutos y segundos.
    ''' </summary>
    Public ReadOnly Property TiempoTotal() As String 'Implements gsPlayAxI.IPlay.TiempoTotal
        Get
            ' Esta propiedad siempre devolverá el tiempo total
            'TiempoTotal = MCICtl.Length \ 1000 ' Int(MCICtl.Length / 1000 + 0.5)
            Dim durTmp As TimeSpan 'Integer

            'durTmp = MCICtl.NaturalDuration.TimeSpan ' MCICtl.Length \ 1000
            durTmp = ComprobarDuration(MCICtl.NaturalDuration)

            'durMinutos = Fix(durTmp \ 60)
            'durSegundos = durTmp - durMinutos * 60

            If durTmp.TotalSeconds = 0 Then
                Return "00.00"
            End If

            'Return durMinutos.ToString("00") & "." & durSegundos.ToString("00")
            Return $"{durTmp.TotalMinutes:00}.{durTmp.Seconds:00}" '  durTmp.ToString("hh:mm:ss") ' "mm\:ss").Replace(":", ".")
        End Get
    End Property

    ''' <summary>
    ''' Los segundos totales restantes.
    ''' </summary>
    Public ReadOnly Property SegundosRestantes() As Integer
        Get
            Return ComprobarDuration()
            ' (MCICtl.Length - MCICtl.CurrentPosition) \ 1000 ' m_durTotal '\ 1000
        End Get
    End Property

    Public Enum eMCIModo
        ' Valores anteriores:
        ''    eError = -1
        ''    eNinguna = 0
        '    ePlay = 1
        '    ePausa = 2
        '    eStop = 3
        ''    eSalir = 4
        ' Nuevos valores, para que sean iguales a los de csPlayAvM
        eStop '=0
        ePausa '=1
        ePlay '=2
        ' Estos son los valores de csPlayAvM,
        ' (que son los mismos que usa el ActiveMovie)
        '    ecsStopped
        '    ecsPaused
        '    ecsRunning
    End Enum

    Private m_EstadoActual As eMCIModo
    Public ReadOnly Property EstadoActual() As eMCIModo
        Get
            Return m_EstadoActual
        End Get
    End Property

    Public ReadOnly Property Mode() As eMCIModo
        Get
            Return m_EstadoActual
        End Get
    End Property

    ''' <summary>
    ''' La duración total en segundos.
    ''' </summary>
    Private m_durTotal As Double 'Decimal

    Public Property FileName As String
    Public Sub Play()
        Play("")
    End Sub

    ''' <summary>
    ''' Toca la canción indicada.
    ''' </summary>
    ''' <param name="file">El path al fichero a tocar.</param>
    Public Sub Play(file As String)
        timer1.Stop()

        MCICtl.Stop()
        If file <> "" Then
            FileName = file
        End If

        MCICtl.Open(New Uri(FileName))

        timer1.Start()

        ComprobarDuration()

        ' Si la duración para el MCI es 0... no tocarla             (12/Dic/99)
        If m_durTotal < 1 Then
            Parar()
            m_EstadoActual = eMCIModo.eStop
        Else
            MCICtl.Play()
            m_EstadoActual = eMCIModo.ePlay
            RaiseEvent MilisegundosRestantes(CInt(m_durTotal * 1000))
        End If

        'Dim d = MCICtl.NaturalDuration
        'm_durTotal = d.TimeSpan.TotalSeconds

        '' Si la duración para el MCI es 0... no tocarla             (12/Dic/99)
        'If m_durTotal < 1 Then
        '    Parar()
        '    m_EstadoActual = eMCIModo.eStop

        'Else
        '    MCICtl.Play()
        '    m_EstadoActual = eMCIModo.ePlay
        '    RaiseEvent MilisegundosRestantes(CInt(d.TimeSpan.TotalMilliseconds))
        'End If

        'System.Windows.Forms.Application.DoEvents()
    End Sub
    ''' <summary>
    ''' Parar lo que está sonando.
    ''' </summary>
    Public Sub Parar()
        timer1.Stop()

        MCICtl.Stop()
        m_EstadoActual = eMCIModo.eStop
        m_durTotal = 0
        RaiseEvent Done(eMCINotify.mciNotifySuccessful)
    End Sub
    ''' <summary>
    ''' Tocar el fichero indicado o el asignado a FileName si no se indica.
    ''' </summary>
    ''' <param name="sFichero">El fichero a reproducir o una cadena vacía.</param>
    Public Sub Tocar(Optional sFichero As String = "")
        Play(sFichero)
    End Sub
    ''' <summary>
    ''' Pausar la canción actual.
    ''' </summary>
    ''' <param name="ObligarPausa">Si es True se intenta pausar lo que está sonando.</param>
    Public Sub Pausa(Optional ObligarPausa As Boolean = False)
        ' Ponerlo en modo pausa o reanudarlo
        ' pero sólo si antes estaba en pausa o tocando
        If ObligarPausa Then
            m_EstadoActual = eMCIModo.ePlay
        End If

        If m_EstadoActual = eMCIModo.ePlay Then
            m_EstadoActual = eMCIModo.ePausa
            MCICtl.Pause()
        ElseIf m_EstadoActual = eMCIModo.ePausa Then
            m_EstadoActual = eMCIModo.ePlay
            MCICtl.Play()
        End If
    End Sub
    ''' <summary>
    ''' Hace una copia de la clase actual.
    ''' </summary>
    Public Function Clone() As cMediaPlayer
        Return TryCast(Me.MemberwiseClone, cMediaPlayer)
    End Function

    ''' <summary>
    ''' Duración en milisegundos del fichero indicado.
    ''' </summary>
    ''' <param name="sFic"></param>
    Public Shared Function Duracion(sFic As String) As Integer
        Dim tmpMP As New MediaPlayer
        tmpMP.Open(New Uri(sFic))
        Dim durMilisegundos As Double

        If tmpMP.NaturalDuration.HasTimeSpan Then
            durMilisegundos = tmpMP.NaturalDuration.TimeSpan.TotalMilliseconds
        Else
            System.Diagnostics.Debug.WriteLine(tmpMP.NaturalDuration.ToString)
            Dim sd = tmpMP.NaturalDuration.ToString
            If sd <> "" AndAlso sd.Contains(":") Then
                Dim ts = New TimeSpan
                TimeSpan.TryParse(sd, ts)
                durMilisegundos = ts.TotalMilliseconds
            End If
        End If

        Return CInt(durMilisegundos)
    End Function
    ''' <summary>
    ''' La duración del fichero indicado en formato cadena mm:ss
    ''' </summary>
    Public Shared Function DuracionStr(sFic As String) As String
        ' devuelve la duración del fichero indicado usando mm.ss    (22/Ene/05)
        '
        Dim durTmp As Integer = Duracion(sFic) \ 1000
        Dim durMin As Integer = Fix(durTmp \ 60)
        Dim durSeg As Integer = durTmp - durMin * 60
        '
        Return durMin.ToString("00") & "." & durSeg.ToString("00")
    End Function
    ''' <summary>
    ''' La duración del fichero en milisegundos.
    ''' </summary>
    ''' <param name="sFic">El fichero del que se quiere saber la duración.</param>
    ''' <param name="cuantosMilisegundos">Valor por referencia con los milisegundos.</param>
    ''' <returns>Una cadena con los minutos y segundos (mm.ss)</returns>
    Public Shared Function DuracionStr(sFic As String, ByRef cuantosMilisegundos As Integer) As String
        ' devuelve la duración del fichero indicado usando mm.ss    (22/Ene/05)
        ' además de devolver la duración en milisegundos            (28/Feb/05)
        '
        cuantosMilisegundos = Duracion(sFic)
        Dim durTmp As Integer = cuantosMilisegundos \ 1000
        Dim durMin As Integer = Fix(durTmp \ 60)
        Dim durSeg As Integer = durTmp - durMin * 60
        '
        Return durMin.ToString("00") & "." & durSeg.ToString("00")
    End Function

End Class
