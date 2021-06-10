'------------------------------------------------------------------------------
' Para los colores del tipo LinearGradientBrus en la configuración  (20/Ago/07)
'
' ©Guillermo 'guille' Som, 2007
'------------------------------------------------------------------------------
Option Strict On

Imports Microsoft.VisualBasic

Imports System
'Imports System.Windows
'Imports System.Windows.Controls
'Imports System.Windows.Data
Imports System.Windows.Media
'Imports System.Windows.Media.Animation
'Imports System.Windows.Navigation
'Imports System.Windows.Media.Imaging
'Imports System.Windows.Input

Imports System.Collections.Generic
Imports System.Text

Public Class ColoresGradient

    ''' <summary>
    ''' Convierte un LinearGradientBrush en una cadena,
    ''' si no es de ese tipo, se devuelve backg.ToString
    ''' </summary>
    ''' <param name="backg"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' El formato usado es:
    ''' sc#N; R; G; B@Offset|
    ''' </remarks>
    Public Overloads Shared Function ToString(ByVal backg As Object) As String
        Dim lgb As LinearGradientBrush = TryCast(backg, LinearGradientBrush)
        If lgb Is Nothing Then Return backg.ToString

        ' Usar formato sin depender de la configuración local       (20/Ago/07)
        Dim cult As System.Globalization.CultureInfo = System.Globalization.CultureInfo.InvariantCulture

        ' Convertir en cadena
        Dim sb As New StringBuilder
        ' Guardar todos los GradientStops que haya                  (20/Ago/07)
        For i As Integer = 0 To lgb.GradientStops.Count - 1
            With lgb.GradientStops(i)
                sb.AppendFormat("sc#{0}; {1}; {2}; {3}@{4}|", _
                                i + 1, _
                                lgb.GradientStops(i).Color.ScR.ToString(cult.NumberFormat), _
                                .Color.ScG.ToString(cult.NumberFormat), _
                                .Color.ScB.ToString(cult.NumberFormat), _
                                .Offset.ToString(cult.NumberFormat))

            End With
        Next

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Convierte una cadena guardada con ColoresGradient.ToString
    ''' en un LinearGradientBrush
    ''' </summary>
    ''' <param name="color"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Parse(ByVal color As String) As LinearGradientBrush
        ' Convertir de cadena en color
        Dim lgb As New LinearGradientBrush
        Dim sCol() As String = color.Split("|".ToCharArray, StringSplitOptions.RemoveEmptyEntries)
        If sCol.Length < 1 Then Return Nothing

        ' Usar formato sin depender de la configuración local       (20/Ago/07)
        Dim cult As System.Globalization.CultureInfo = System.Globalization.CultureInfo.InvariantCulture

        ' Recuperar todos los valores que haya
        ' El formato interno también puede ser en la forma #AARRGGBB
        ' el problema es para el valor de Offset que debe estar entre 0 y 1,
        ' por tanto solo funcionará bien con dos valores...
        ' salvo que se use @offset 
        For i As Integer = 0 To sCol.Length - 1
            Dim gs As New GradientStop
            Dim sOffset As String = i.ToString
            Dim k As Integer = sCol(i).IndexOf("@")
            If k > -1 Then
                sOffset = sCol(i).Substring(k + 1)
                sCol(i) = sCol(i).Substring(0, k)
            End If
            sCol(i) = sCol(i).Replace(cult.NumberFormat.CurrencyDecimalSeparator, _
                                      Globalization.CultureInfo.CurrentCulture. _
                                      NumberFormat.CurrencyDecimalSeparator)
            gs.Color = CType(ColorConverter.ConvertFromString(sCol(i)), Color)
            'gs.Color = CType(ColorConverter.ConvertFromString(sCol(i).ToString(cult.NumberFormat)), Color)
            'gs.Color = CType(ColorConverter.ConvertFromString(sCol(i)), Color)
            ' Si el valor está en punto decimal y se usa la coma, reemplazarlo
            sOffset = sOffset.Replace(cult.NumberFormat.CurrencyDecimalSeparator, _
                                      Globalization.CultureInfo.CurrentCulture. _
                                      NumberFormat.CurrencyDecimalSeparator)
            gs.Offset = CDbl(sOffset)
            lgb.GradientStops.Add(gs)
        Next

        Return lgb
    End Function

End Class
