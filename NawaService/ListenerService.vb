Imports CookComputing.XmlRpc
Imports System.Net

Public Class ListenerService
    Inherits XmlRpcHttpServerProtocol

    Public Overridable Sub ProcessRequest(ByVal RequestContext As HttpListenerContext)
        Try
            Dim req As IHttpRequest = New ListenerRequest(RequestContext.Request)
            Dim resp As IHttpResponse = New ListenerResponse(RequestContext.Response)
            HandleHttpRequest(req, resp)
            RequestContext.Response.OutputStream.Close()
        Catch ex As Exception
            ' "Internal server error"
            RequestContext.Response.StatusCode = 500
            RequestContext.Response.StatusDescription = ex.Message
        End Try
    End Sub
End Class
