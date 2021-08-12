Imports CookComputing.XmlRpc
Imports System.Net

Public Class ListenerRequest
    Implements CookComputing.XmlRpc.IHttpRequest
    Private request As HttpListenerRequest

    Public Sub New(ByVal request As HttpListenerRequest)
        Me.request = request
    End Sub
    Public ReadOnly Property HttpMethod() As String Implements CookComputing.XmlRpc.IHttpRequest.HttpMethod
        Get
            Return request.HttpMethod
        End Get
    End Property
    Public ReadOnly Property InputStream() As System.IO.Stream Implements CookComputing.XmlRpc.IHttpRequest.InputStream
        Get
            Return request.InputStream
        End Get
    End Property
End Class
