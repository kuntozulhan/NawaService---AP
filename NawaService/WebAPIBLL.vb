Imports System.Net
Imports System.IO
Imports System.Text

Public Class WebAPIBLL
    Implements IDisposable


#Region "Cek No Rekening"
    Public Function IsRekeningValid(ByVal strNoRekening As String, ByVal lngNominal As Long) As Boolean
        Dim BolReturn As Boolean = False

        Dim strPOSTResponse As String = ""
        strPOSTResponse = ReadPOSTMethod(strNoRekening, lngNominal)

        'Validasi Response
        If strPOSTResponse.Contains("|") Then
            If strPOSTResponse.Split("|").Length < 12 Then
                Throw New Exception("Pengecekan nomor rekening gagal: format response tidak sesuai")
            End If
        Else
            Throw New Exception("Pengecekan nomor rekening gagal: format response tidak sesuai")
        End If

        'Baca Response dr server,
        'Baca Response Code, kalau 00 berarti SUKSES/APPROVED
        Dim strResult As String = strPOSTResponse.Split("|")(11)
        Dim strResultDescription As String = strPOSTResponse.Split("|")(12)
        If strResult = "00" Then
            BolReturn = True
        Else
            Throw New Exception(strResult & "|" & strResultDescription)
        End If

        Return BolReturn
    End Function

    Private Function ReadPOSTMethod(ByVal strNoRekening As String, ByVal lngNominal As Long) As String
        Dim strReturn As String = ""

        'Ambil setting webservice dr SystemParameter
        Dim strWSIP As String = GetParameterValue(4000)
        Dim strWSPort As String = GetParameterValue(4001)
        Dim strWSParameter As String = GetParameterValue(4002)
        Dim strWSMerchantNo As String = GetParameterValue(4003)
        Dim strWSPassword As String = GetParameterValue(4004)
        Dim strWSIsUseProxy As String = GetParameterValue(4005)

        Dim strWebRequest As String = "http://" & strWSIP & ":" & strWSPort & "/" & strWSParameter & "&cardNumber=" & strNoRekening & "&password=" & strWSPassword & "&merchantNumber=" & strWSMerchantNo & "&amountTransaction=" & lngNominal.ToString
        Dim request As WebRequest = WebRequest.Create(strWebRequest)
        ' Set the Method property of the request to POST.
        request.Method = "POST"

        If strWSIsUseProxy = "1" Then
            Dim strWSProxyIP As String = GetParameterValue(4006)
            Dim strWSProxyPort As String = GetParameterValue(4007)
            Dim strWSProxyUserName As String = GetParameterValue(4008)
            Dim strWSProxyPassword As String = GetParameterValue(4009)
            Dim strWSProxyDomain As String = GetParameterValue(4010)

            Dim objCredential As New System.Net.NetworkCredential(strWSProxyUserName, strWSProxyPassword, strWSProxyDomain)
            Dim objProxy As New System.Net.WebProxy(strWSProxyIP, CInt(strWSProxyPort))

            objProxy.Credentials = objCredential
            request.Proxy = objProxy
        End If

        Dim postData As String = "This is a test that posts this string to a Web server."
        Dim byteArray() As Byte = Encoding.UTF8.GetBytes(postData)
        ' Set the ContentType property of the WebRequest.
        request.ContentType = "application/x-www-form-urlencoded"
        ' Set the ContentLength property of the WebRequest.
        request.ContentLength = byteArray.Length
        Dim dataStream As Stream = request.GetRequestStream
        ' Write the data to the request stream.
        dataStream.Write(byteArray, 0, byteArray.Length)
        ' Close the Stream object.
        dataStream.Close()
        Dim response As WebResponse = request.GetResponse
        ' Display the status.
        Console.WriteLine(CType(response, HttpWebResponse).StatusDescription)
        ' Get the stream containing content returned by the server.
        dataStream = response.GetResponseStream
        Dim reader As StreamReader = New StreamReader(dataStream)
        Dim responseFromServer As String = reader.ReadToEnd
        '' Display the content.
        'Console.WriteLine(responseFromServer)
        ' Clean up the streams.
        reader.Close()
        dataStream.Close()
        response.Close()

        strReturn = responseFromServer
        Return strReturn
    End Function

#End Region

    Public Function GetParameterValue(ByVal intPK_SystemParameter_ID As Integer) As String
        Dim StrReturn As String = ""
        Try
            Dim StrQuery As String = "SELECT sp.SettingValue FROM SystemParameter sp WHERE PK_SystemParameter_ID = " & intPK_SystemParameter_ID.ToString
            StrReturn = CStr(SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, StrQuery, Nothing))
        Catch ex As Exception

        End Try

        Return StrReturn
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
