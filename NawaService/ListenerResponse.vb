Imports CookComputing.XmlRpc
Imports System.Net
Imports System.io

Public Class ListenerResponse
    Implements CookComputing.XmlRpc.IHttpResponse
    Private response As HttpListenerResponse

    Public Sub New(ByVal response As HttpListenerResponse)
        Me.response = response
    End Sub
    Public Property ContentType() As String Implements CookComputing.XmlRpc.IHttpResponse.ContentType
        Get
            Return response.ContentType
        End Get
        Set(ByVal value As String)
            response.ContentType = value
        End Set
    End Property
    Public ReadOnly Property Output() As System.IO.TextWriter Implements CookComputing.XmlRpc.IHttpResponse.Output
        Get
            Return New StreamWriter(response.OutputStream)
        End Get
    End Property
    Public ReadOnly Property OutputStream() As System.IO.Stream Implements CookComputing.XmlRpc.IHttpResponse.OutputStream
        Get
            Return response.OutputStream
        End Get
    End Property
    Public Property StatusCode() As Integer Implements CookComputing.XmlRpc.IHttpResponse.StatusCode
        Get
            Return response.StatusCode
        End Get
        Set(ByVal value As Integer)
            response.StatusCode = value
        End Set
    End Property
    Public Property StatusDescription() As String Implements CookComputing.XmlRpc.IHttpResponse.StatusDescription
        Get
            Return response.StatusDescription
        End Get
        Set(ByVal value As String)
            response.StatusDescription = value
        End Set
    End Property

End Class
