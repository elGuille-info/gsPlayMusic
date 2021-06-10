Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    ''' <summary>
    ''' Directorio de la aplicación.
    ''' </summary>
    Public Shared DirApp As String
    ''' <summary>
    ''' Directorio de recursos.
    ''' </summary>
    Public Shared DirResources As String

    Public Sub New()
        DirApp = cIniArray.AppPath(False)
        DirResources = System.IO.Path.Combine(DirApp, "Resources")

    End Sub

End Class
