Imports System.ServiceProcess
Imports System.Threading
Imports System.Configuration
Imports System.Text
Imports System.Net
Imports System.IO
Imports CookComputing.XmlRpc
Imports System.Xml


Public Class NawaDataConsole
    Inherits ServiceBase
    Private objThreadEODScheduler As Thread
    Private objThreadEmail As Thread
    Private objThreadSFTP As Thread
    Private objThreadCheckSFTP As Thread
    Private objThreadFile As Thread
    Private objThreadValidation As Thread
    Private objThreadOTPSFTP As Thread

    Private objThreadAngsuran As Thread

    Private tSchedulerManual As Thread

    Dim strAddress As String = My.Settings.IPServerDatabase
    Dim intPort As Integer = My.Settings.XMLRPCPort
    Dim listener As HttpListener

    Protected Sub TestStartupAndStop(args As String())
        Me.OnStart(args)
        Console.ReadLine()
        Me.OnStop()
    End Sub

    'Private Shared Sub Main(args As String())
    '    If Environment.UserInteractive Then
    '        Dim service1 As New NawaDataConsole(args)
    '        service1.TestStartupAndStop(args)
    '        ' Put the body of your old Main method here.
    '    Else

    '    End If
    'End Sub

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private mylog As New NawaConsoleLog
    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.
        mylog.LogInfo("Nawa Service Starting....")

        'db butuh ini
        'objThreadEODScheduler = New Thread(New ThreadStart(AddressOf RunEODScheduler))
        'objThreadEODScheduler.IsBackground = True
        'objThreadEODScheduler.Start()
        'end

        'ap butuh ini
        objThreadAngsuran = New Thread(New ThreadStart(AddressOf RunAngsuran))
        objThreadAngsuran.IsBackground = True
        objThreadAngsuran.Start()


        objThreadOTPSFTP = New Thread(New ThreadStart(AddressOf RunOTPSFTP))
        objThreadOTPSFTP.IsBackground = True
        objThreadOTPSFTP.Start()
        'end

        'objThreadSFTP = New Thread(New ThreadStart(AddressOf RunSFTP))
        'objThreadSFTP.IsBackground = True
        'objThreadSFTP.Start()

        'objThreadCheckSFTP = New Thread(New ThreadStart(AddressOf RunCheckSFTP))
        'objThreadCheckSFTP.IsBackground = True
        'objThreadCheckSFTP.Start()

        'objThreadValidation = New Thread(New ThreadStart(AddressOf RunValidation))
        'objThreadValidation.IsBackground = True
        'objThreadValidation.Start()

        'tSchedulerManual = New Thread(AddressOf StartXMLRPCProcess)
        'tSchedulerManual.Start()

    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
        mylog.LogInfo("Nawa Service Stoping....")
        StopService()
    End Sub

    'Sub RunCheckSFTP()
    '    Dim objCheckSFTPStatusBLL As New CheckSFTPStatusBLL
    '    objCheckSFTPStatusBLL.run()
    'End Sub

    Sub RunAngsuran()
        'Thread.Sleep(60000)

        Dim ObjAngsuranBLL As New AngsuranBLL
        ObjAngsuranBLL.run()
    End Sub

    Sub RunOTPSFTP()
        Dim objUploadOTPBLL As New UploadOTPBLL
        objUploadOTPBLL.run()
    End Sub
    'Sub RunSFTP()
    '    Dim objUploadJournalBLL As New UploadJournalBLL
    '    objUploadJournalBLL.run()
    'End Sub

    Sub RunEmail()
        Dim objEmailTemplateBLL As New EmailTemplateBLL
        objEmailTemplateBLL.run()
    End Sub

    Sub RunEODScheduler()
        Dim objEOdSchedulerBLL As New EODSchedulerBLL
        objEOdSchedulerBLL.run()
    End Sub

    Sub RunGenerateFile()
        Dim GenerateFileBLL As New ServicesForWeb
        GenerateFileBLL.run()
    End Sub

    Sub RunValidation()
        Dim ValidateRecordsBLL As New ValidateRecords
        ValidateRecordsBLL.run()
    End Sub

    Public Sub StartService()
        Try
            listener = New HttpListener()
            listener.Prefixes.Add("http://" & strAddress & ":" & intPort & "/")
            listener.Start()
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub StartListen()
        While Not listener Is Nothing
            Try
                Dim context As HttpListenerContext = listener.GetContext()
                Dim svc As ListenerService = New ServicesForWeb()
                svc.ProcessRequest(context)
            Catch ex As Exception
                'ignore
            End Try
        End While
    End Sub
    Public Sub StopService()
        Try
            If Not listener Is Nothing Then
                listener.Stop()
                listener = Nothing
            End If
        Catch ex As Exception
            'ignore
        End Try
    End Sub
    Private Sub StartXMLRPCProcess()
        StartService()
        StartListen()
    End Sub
End Class
