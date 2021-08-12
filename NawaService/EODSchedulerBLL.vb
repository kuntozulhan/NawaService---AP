Imports System.Threading
Imports System.Configuration
Imports System.Data.SqlClient
Imports Microsoft.SqlServer.Dts.Runtime

Public Class EODSchedulerBLL
    Implements IDisposable

    Enum EODTaskDetailType
        SSIS = 1
        StoreProcedure
    End Enum

    Enum MsEODStatus
        OnQueue = 1
        Inprogress
        Sucess
        ErrorProcess
    End Enum
    Private mylog As New NawaConsoleLog

    Sub New()

    End Sub

    Sub run()
        Try
            While True
                Thread.Sleep(My.Settings.IntThreadInterval)
                ExecSPCurrentScheduler()

                Using objDb As NawaDataEntities = New NawaDataEntities
                    Dim objListEODSchedulerLog As List(Of EODSchedulerLog) = objDb.EODSchedulerLogs.Where(Function(x) x.FK_MsEODStatus_ID = 1).Take(1).ToList

                    For Each item As EODSchedulerLog In objListEODSchedulerLog
                        Dim lngPK_EODSchedulerLog_ID As Long = item.PK_EODSchedulerLog_ID
                        ExecUpdateParamModifiedDate(item.DataDate.GetValueOrDefault(DateTime.Now))

                        RunEODScheduler(lngPK_EODSchedulerLog_ID)
                    Next
                End Using
            End While
        Catch ex As Exception
            mylog.LogError("An Error has been occurred on RunEODScheduler: ", ex)
            Throw ex
        End Try
    End Sub

    Sub ExecUpdateParamModifiedDate(ByVal processdate As Date)
        Using objDb As NawaDataEntities = New NawaDataEntities
            ' Create a SqlParameter for each parameter in the stored procedure.

            Dim objparamdate As New Data.SqlClient.SqlParameter("@ProcessDate", processdate.ToString("yyyy-MM-dd"))
            objDb.Database.ExecuteSqlCommand("exec Usp_Console_UpdateParamModifiedDate @ProcessDate", objparamdate)
        End Using
    End Sub

    Sub RunEODScheduler(lngPK_EODSchedulerLog_ID As Long)
        Dim IsprocessValid As Boolean = True
        Dim strErrorMessage As String = ""

        Using objDb As NawaDataEntities = New NawaDataEntities
            Try

                Dim objEodSchedulerLog As EODSchedulerLog = objDb.EODSchedulerLogs.Where(Function(x) x.PK_EODSchedulerLog_ID = lngPK_EODSchedulerLog_ID).FirstOrDefault()
                Dim EODSchedulerID As Long = 0
                If Not objEodSchedulerLog Is Nothing Then
                    EODSchedulerID = objEodSchedulerLog.FK_EODSchedulerID
                End If

                'udpate status jadi inprogress (2)
                EODSchedulerStart(lngPK_EODSchedulerLog_ID)

                For Each itemTaskLog As EODTaskLog In objDb.EODTaskLogs.Where(Function(x) x.EODSchedulerLogID = lngPK_EODSchedulerLog_ID).ToList
                    'update status task log  jadi inprogress (2)
                    EODTaskStart(itemTaskLog.PK_EODTaskLog_ID)

                    For Each itemTaskDetailLog As EODTaskDetailLog In objDb.EODTaskDetailLogs.Where(Function(x) x.EODTaskLogID = itemTaskLog.PK_EODTaskLog_ID).ToList
                        If IsprocessValid Then
                            Try
                                'update status jadi inprocess (2)
                                EODTaskDetailStart(itemTaskDetailLog.PK_EODTaskDetailLog_ID)

                                Dim objEOdTaskDetail As EODTaskDetail = objDb.EODTaskDetails.Where(Function(x) x.PK_EODTaskDetail_ID = itemTaskDetailLog.FK_EODTAskDetail_ID).FirstOrDefault()

                                If Not objEOdTaskDetail Is Nothing Then
                                    If objEOdTaskDetail.FK_EODTaskDetailType_ID = EODTaskDetailType.SSIS Then
                                        objDb.Database.ExecuteSqlCommand("exec Usp_Console_UpdateParamEODTaskDetailLogID @PK_EODTaskDetailLog_ID",
                                            New Data.SqlClient.SqlParameter("@PK_EODTaskDetailLog_ID", itemTaskDetailLog.PK_EODTaskDetailLog_ID))

                                        If RunEodTaskDetailSSIS(objEOdTaskDetail, itemTaskDetailLog, objEodSchedulerLog.DataDate) Then
                                            EODTaskDetailEnd(itemTaskDetailLog.PK_EODTaskDetailLog_ID, Convert.ToInt32(MsEODStatus.Sucess), "")
                                        Else
                                            EODTaskDetailEnd(itemTaskDetailLog.PK_EODTaskDetailLog_ID, Convert.ToInt32(MsEODStatus.ErrorProcess), "Error Running SSIS " & objEOdTaskDetail.SSISName)
                                        End If
                                    ElseIf objEOdTaskDetail.FK_EODTaskDetailType_ID = EODTaskDetailType.StoreProcedure Then
                                        RunEodTaskDetailStoreProcedure(objEOdTaskDetail, itemTaskDetailLog, objEodSchedulerLog.DataDate)
                                        EODTaskDetailEnd(itemTaskDetailLog.PK_EODTaskDetailLog_ID, Convert.ToInt32(MsEODStatus.Sucess), "")
                                    End If
                                Else
                                    EODTaskDetailEnd(itemTaskDetailLog.PK_EODTaskDetailLog_ID, Convert.ToInt32(MsEODStatus.ErrorProcess), "Error Running Store Procedure " & objEOdTaskDetail.StoreProcedureName)
                                End If
                            Catch ex As Exception
                                strErrorMessage = ex.Message
                                EODTaskDetailEnd(itemTaskDetailLog.PK_EODTaskDetailLog_ID, Convert.ToInt32(MsEODStatus.ErrorProcess), "Task Detail is canceled")
                                IsprocessValid = False

                                Dim objEOdTaskDetail As EODTaskDetail = objDb.EODTaskDetails.Where(Function(x) x.PK_EODTaskDetail_ID = itemTaskDetailLog.FK_EODTAskDetail_ID).FirstOrDefault()
                                If objEOdTaskDetail.FK_EODTaskDetailType_ID = EODTaskDetailType.SSIS Then
                                    mylog.LogError("An error has been occurred on RunEODProcess, package " + objEOdTaskDetail.SSISName, ex)
                                Else
                                    mylog.LogError("An error has been occurred on RunEODProcess, Store Procedure " + objEOdTaskDetail.StoreProcedureName, ex)
                                End If
                            End Try
                        Else
                            '//Kalo sebelumnya ada process package yg gagal,
                            '//Maka gak usah dijalankan, sisanya lgsg dianggap gagal aja

                            '//Update Status Log Task Detail jadi In Progress
                            EODTaskDetailStart(itemTaskDetailLog.PK_EODTaskDetailLog_ID)

                            '//Update Status Log Package jadi Error
                            EODTaskDetailEnd(itemTaskDetailLog.PK_EODTaskDetailLog_ID, Convert.ToInt32(MsEODStatus.ErrorProcess), "Task Detail is canceled")
                        End If
                    Next

                    If IsprocessValid Then
                        EODTaskEnd(itemTaskLog.PK_EODTaskLog_ID, Convert.ToInt32(MsEODStatus.Sucess), "")
                    Else
                        EODTaskEnd(itemTaskLog.PK_EODTaskLog_ID, Convert.ToInt32(MsEODStatus.ErrorProcess), strErrorMessage)
                    End If
                Next

                If IsprocessValid Then
                    EodSchedulerEnd(lngPK_EODSchedulerLog_ID, Convert.ToInt32(MsEODStatus.Sucess), "")
                Else
                    EodSchedulerEnd(lngPK_EODSchedulerLog_ID, Convert.ToInt32(MsEODStatus.ErrorProcess), strErrorMessage)
                End If

            Catch ex As Exception
                mylog.LogError("An error has been occurred on RunEODProcess, Run EOD Scheduler", ex)
            End Try
        End Using
    End Sub

    Private Function ExecuteDTSX(strFullFileDirectory As String, strPackageName As String, datadate As Date?) As Boolean
        Dim strPkgLocation As String
        Dim pkg As New Package()
        Dim app As New Application()
        Dim pkgResults As New DTSExecResult()

        Try
            strPkgLocation = strFullFileDirectory
            'app.PackagePassword = "keymaker"
            pkg = app.LoadPackage(strPkgLocation, Nothing)

            Using objDb As NawaDataEntities = New NawaDataEntities

                'Dim objConnectionString As List(Of NawadataConnectionString) = objDb.NawadataConnectionStrings.Where(Function(x) x.Active = True).ToList

                'If Not pkg Is Nothing Then
                '    For i As Integer = 0 To pkg.Connections.Count - 1
                '        Dim objFind As NawadataConnectionString = objConnectionString.Find(Function(x) x.ConnecitonName.ToLower = pkg.Connections(i).Name.ToLower)
                '        If Not objFind Is Nothing Then
                '            Dim connectionstring As String = GetConnectionStringByPk(objFind.PK_NawaDataConnectionString_ID)
                '            pkg.Connections(i).ConnectionString = connectionstring
                '        End If
                '    Next
                'End If

                If pkg.Connections.Count > 0 Then
                    For i As Integer = 0 To pkg.Connections.Count - 1

                        If pkg.Connections(i).Name.ToLower = "hivesystem" Then
                            ''Ini Untuk Proses Big Data
                            ''Generate Ticket
                            Dim TalendFileLoc As String = getSystemParameter(30008)
                            Dim strkeyTabFileName As String = getSystemParameter(30016) '"talend.keytab"
                            Dim strprincipal As String = getSystemParameter(30017) '"talend@CORPUAT.DANAMON.CO.ID"
                            Dim strArguments As String = String.Format("-k -t ""{0}\{1}"" {2}", TalendFileLoc, strkeyTabFileName, strprincipal)
                            Dim psi As ProcessStartInfo = New ProcessStartInfo("kinit") With
                                {
                                    .UseShellExecute = True,
                                    .RedirectStandardOutput = False,
                                    .RedirectStandardInput = False,
                                    .RedirectStandardError = False,
                                    .CreateNoWindow = True,
                                    .WindowStyle = ProcessWindowStyle.Hidden,
                                    .Arguments = strArguments
                                 }
                            Dim process As Process = Process.Start(psi)
                            Thread.Sleep(6000)
                            If process Is Nothing Then
                                'mylog.Info("Unable to generate Kerberos Ticket.")
                            End If

                            Dim conn As Odbc.OdbcConnection = New Odbc.OdbcConnection()
                            'conn.ConnectionString = "Driver=" & getSystemParameter(30006) & ";applysspwithqueries=1;asyncexecpollinterval=100;authmech=1;delegatekrbcreds=0;enabletemptable=0;gettableswithquery=0;hiveservertype=2;host=" & getSystemParameter(30002) & ";invalidsessionautorecover=1;krbhostfqdn=" & getSystemParameter(30004) & ";krbrealm=" & getSystemParameter(30003) & ";krbservicename=" & getSystemParameter(30005) & ";port=" & getSystemParameter(30007) & ";schema=" & getSystemParameter(30001) & ";servicediscoverymode=0;serviceprincipalcanonicalization=0;ssl=0;"
                            conn.ConnectionString = "Driver=" & getSystemParameter(30006) & ";authmech=1;autoreconnect=1;binarycolumnlength=32767;decimalcolumnscale=10;defaultstringcolumnlength=255;delegatekrbcreds=1;fastsqlprepare=0;fixunquoteddefaultschemanameinquery=1;gettableswithquery=0;hiveservertype=2;host=" & getSystemParameter(30002) & ";invalidsessionautorecover=1;krbhostfqdn=" & getSystemParameter(30004) & ";krbrealm=" & getSystemParameter(30003) & ";krbservicename=" & getSystemParameter(30005) & ";port=" & getSystemParameter(30007) & ";rowsfetchedperblock=10000;schema=" & getSystemParameter(30001) & ";servicediscoverymode=0;serviceprincipalcanonicalization=0;showsystemtable=0;ssl=0;thrifttransport=1;usenativequery=0;useunicodesqlcharactertypes=0;"
                            conn.Open()
                            pkg.Connections(i).ConnectionString = conn.ConnectionString 'getSystemParameter(30015)
                            conn.Close()
                        End If

                        If pkg.Connections(i).Name.ToLower = "nawadataconnection" Then
                            Dim enc As String = getSystemParameter(30011)
                            Dim salt As String = getSystemParameterEnc(30011)
                            pkg.Connections(i).ConnectionString = "Data Source=" & getSystemParameter(30013) & ";Initial Catalog=" + getSystemParameter(30010) + ";Provider=SQLNCLI11.1;Persist Security Info=True;User ID=" & getSystemParameter(30014) & ";Password=" & NawaBLL.Common.DecryptRijndael(enc, salt) 'getSystemParameter(30011) 'Common.DecryptRijndael(getSystemParameter(30011), getSystemParameterEnc(30011)) 'NawaBLL.Common.DecryptRijndael(getSystemParameter(30011), getSystemParameterEnc(30011))
                        End If
                    Next
                End If

                'If pkg IsNot Nothing Then
                'Dim listVariables As List(Of SystemParameter) = objDb.SystemParameters.Where(Function(item) item.FK_SystemParameterGroup_ID = 9).ToList()
                Thread.Sleep(6000)
                For Each pkgVariable As Variable In pkg.Variables
                    If pkgVariable.Name = "ProcessDate" Then
                        pkgVariable.Value = Format(datadate, "yyyyMMdd")
                    End If
                    'Dim param As SystemParameter = listVariables.Where(Function(item) item.SettingName = pkgVariable.Name).FirstOrDefault()
                    'If param IsNot Nothing Then
                    '    If param.IsEncript.GetValueOrDefault(False) Then
                    '        pkgVariable.Value = NawaBLL.Common.DecryptRijndael(param.SettingValue, param.EncriptionKey)
                    '    Else
                    '        pkgVariable.Value = param.SettingValue
                    '    End If
                    'End If
                Next

                'End If

            End Using

            Thread.Sleep(10000)
            'Execute
            'pkgResults = pkg.Execute(Nothing, Nothing, Nothing, Nothing, Nothing)
            pkgResults = pkg.Execute()

            Select Case pkgResults
                Case DTSExecResult.Canceled
                    Throw New Exception((Convert.ToString("Dtsx Package ") & strPackageName) + " is canceled")
                Case DTSExecResult.Success
                    Return (True)
                Case DTSExecResult.Failure
                    Dim strDTSXErrorMessage As String = ""
                    If pkg.Errors.Count = 1 Then
                        If pkg.Errors(0).Description.StartsWith("To run a SSIS package outside of SQL Server Data Tools you must install SSIS log provider") Then
                            Return True
                        End If
                    End If
                    For i As Integer = 0 To pkg.Errors.Count - 1
                        strDTSXErrorMessage += pkg.Errors(i).Description + ", "
                    Next
                    Throw New Exception(Convert.ToString((Convert.ToString("Error running package ") & strPackageName) + ": ") & strDTSXErrorMessage)
                Case DTSExecResult.Completion
                    Return (True)
            End Select

            Return (False)
        Catch ex As Exception
            Throw
        Finally
            pkg = Nothing
            pkgResults = Nothing
            app = Nothing

        End Try

    End Function

    Function getSystemParameter(pk As Integer) As String
        Using objDb As NawaDataEntities = New NawaDataEntities

            Dim param As SystemParameter = objDb.SystemParameters.Where(Function(item) item.PK_SystemParameter_ID = pk).FirstOrDefault()
            Return param.SettingValue
        End Using

    End Function

    Function getSystemParameterEnc(pk As Integer) As String
        Using objDb As NawaDataEntities = New NawaDataEntities

            Dim param As SystemParameter = objDb.SystemParameters.Where(Function(item) item.PK_SystemParameter_ID = pk).FirstOrDefault()
            Return param.EncriptionKey

        End Using

    End Function
    Function GetConnectionStringByPk(pk As Integer) As String

        Dim objparam(0) As SqlParameter
        objparam(0) = New SqlParameter
        objparam(0).ParameterName = "@pkid"
        objparam(0).Value = pk

        Return Convert.ToString(SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.StoredProcedure, "Nawa_usp_GetConnectionStringByPK", objparam))
    End Function
    Function RunEodTaskDetailSSIS(ByVal objEODTaskDetail As EODTaskDetail, ByVal objEODTaskLogDetail As EODTaskDetailLog, datadate As Date?) As Boolean

        If objEODTaskDetail.SSISFIle.Length > 0 Then
            Dim strFullFileDirectory As String = ""
            strFullFileDirectory = My.Settings.StrTempFileDirectory & "\" & objEODTaskDetail.SSISName
            System.IO.File.WriteAllBytes(strFullFileDirectory, objEODTaskDetail.SSISFIle)

            Return (ExecuteDTSX(strFullFileDirectory, objEODTaskDetail.SSISName, datadate))
        Else
            Return False
        End If

    End Function
    Function RunEodTaskDetailStoreProcedure(ByVal objEODTaskDetail As EODTaskDetail, ByVal objEODTaskLogDetail As EODTaskDetailLog, datadate As Date) As Boolean
        If objEODTaskDetail.StoreProcedureName.Length > 0 Then


            Using objDb As NawaDataEntities = New NawaDataEntities

                Dim objparamaskID As New SqlParameter("@PK_EODTaskDetailLog_ID", objEODTaskLogDetail.PK_EODTaskDetailLog_ID)
                Console.WriteLine("SP Name:" + objEODTaskDetail.StoreProcedureName.ToString + ",EODLogID:" + objEODTaskLogDetail.PK_EODTaskDetailLog_ID.ToString)

                If objEODTaskDetail.IsUseParameterProcessDate Then
                    Dim objparamdate As New SqlParameter("@DataDate", datadate.ToString("yyyy-MM-dd"))
                    Console.WriteLine("Data Date:" + datadate.ToString("yyyy-MM-dd"))

                    'objDb.Database.ExecuteSqlCommand("exec " & objEODTaskDetail.StoreProcedureName & "  @PK_EODTaskDetailLog_ID, @DataDate", objparamaskID, objparamdate)
                    objDb.Database.ExecuteSqlCommand(System.Data.Entity.TransactionalBehavior.DoNotEnsureTransaction, "exec " & objEODTaskDetail.StoreProcedureName & "  @PK_EODTaskDetailLog_ID, @DataDate", objparamaskID, objparamdate)
                Else
                    objDb.Database.ExecuteSqlCommand(System.Data.Entity.TransactionalBehavior.DoNotEnsureTransaction, "exec " & objEODTaskDetail.StoreProcedureName & "  @PK_EODTaskDetailLog_ID", objparamaskID)
                    'objDb.Database.ExecuteSqlCommand("exec " & objEODTaskDetail.StoreProcedureName & "  @PK_EODTaskDetailLog_ID", objparamaskID)
                End If

                Return True
            End Using
        End If

    End Function


    Sub EODTaskDetailEnd(ByVal lngPK_EODTaskDetailLog_ID As Long, intEODStatus As Integer, strErrorMessage As String)

        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim objdata As EODTaskDetailLog = objDb.EODTaskDetailLogs.Where(Function(x) x.PK_EODTaskDetailLog_ID = lngPK_EODTaskDetailLog_ID).FirstOrDefault
            If Not objdata Is Nothing Then
                objdata.Enddate = DateTime.Now
                objdata.FK_MsEODStatus_ID = intEODStatus
                objdata.ErrorMessage = strErrorMessage
                objDb.SaveChanges()
            End If
        End Using
    End Sub

    Sub EODTaskDetailStart(ByVal lngPK_EODTaskDetailLog_ID As Long)
        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim objdata As EODTaskDetailLog = objDb.EODTaskDetailLogs.Where(Function(x) x.PK_EODTaskDetailLog_ID = lngPK_EODTaskDetailLog_ID).FirstOrDefault
            If Not objdata Is Nothing Then
                objdata.StartDate = DateTime.Now
                objdata.FK_MsEODStatus_ID = MsEODStatus.Inprogress
                objDb.SaveChanges()
            End If
        End Using
    End Sub


    Sub EodSchedulerEnd(ByVal lngPK_EODSchedulerLog_ID As Long, intEODStatus As Integer, strErrorMessage As String)
        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim objdata As EODSchedulerLog = objDb.EODSchedulerLogs.Where(Function(x) x.PK_EODSchedulerLog_ID = lngPK_EODSchedulerLog_ID).FirstOrDefault
            If Not objdata Is Nothing Then
                objdata.Enddate = DateTime.Now
                objdata.FK_MsEODStatus_ID = intEODStatus
                objdata.ErrorMessage = strErrorMessage
                objDb.SaveChanges()
            End If
        End Using
    End Sub
    Sub EODTaskEnd(ByVal lngPK_EODTaskLog_ID As Long, intEODStatus As Integer, strErrorMessage As String)
        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim objdata As EODTaskLog = objDb.EODTaskLogs.Where(Function(x) x.PK_EODTaskLog_ID = lngPK_EODTaskLog_ID).FirstOrDefault
            If Not objdata Is Nothing Then
                objdata.Enddate = DateTime.Now
                objdata.FK_MsEODStatus_ID = intEODStatus
                objdata.ErrorMessage = strErrorMessage
                objDb.SaveChanges()
            End If
        End Using
    End Sub

    Sub EODTaskStart(ByVal lngPK_EODTaskLog_ID As Long)
        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim objdata As EODTaskLog = objDb.EODTaskLogs.Where(Function(x) x.PK_EODTaskLog_ID = lngPK_EODTaskLog_ID).FirstOrDefault
            If Not objdata Is Nothing Then
                objdata.StartDate = DateTime.Now
                objdata.FK_MsEODStatus_ID = MsEODStatus.Inprogress
                objDb.SaveChanges()
            End If
        End Using
    End Sub


    Sub EODSchedulerStart(ByVal lngPK_EODSchedulerLog_ID As Long)
        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim objdata As EODSchedulerLog = objDb.EODSchedulerLogs.Where(Function(x) x.PK_EODSchedulerLog_ID = lngPK_EODSchedulerLog_ID).FirstOrDefault
            If Not objdata Is Nothing Then
                objdata.StartDate = DateTime.Now
                objdata.FK_MsEODStatus_ID = MsEODStatus.Inprogress
                objDb.SaveChanges()
            End If
        End Using

    End Sub

    Sub ExecSPCurrentScheduler()
        Try
            Using objDb As NawaDataEntities = New NawaDataEntities
                ' Create a SqlParameter for each parameter in the stored procedure.
                Dim intPeriodprocessdate As Integer = objDb.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = 3000).FirstOrDefault.SettingValue

                'Dim objparamdate As New Data.SqlClient.SqlParameter("@datProcessDate", DateAdd(DateInterval.Day, intPeriodprocessdate, DateTime.Now).ToString("yyyy-MM-dd"))
                Dim objparamdate As New Data.SqlClient.SqlParameter("@datProcessDate", DateTime.Now)
                Dim objparamuser As New Data.SqlClient.SqlParameter("@strUserId", "sysadmin")
                Dim objparamdatadate As New Data.SqlClient.SqlParameter("@datadate", DateAdd(DateInterval.Day, intPeriodprocessdate, DateTime.Now).ToString("yyyy-MM-dd"))

                objDb.Database.ExecuteSqlCommand("exec Usp_Console_InsertEODLog @datProcessDate,@strUserId,@datadate", objparamdate, objparamuser, objparamdatadate)
            End Using
        Catch ex As Exception
            mylog.LogError("An error has been occurred on RunEODProcess, ExecSPCurrentScheduler", ex)
        End Try
    End Sub


#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls


    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class


'Sub Main()
'    Dim dateProcess As String = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")
'    Dim ProcessDate As DateTime = DateTime.Now
'    Dim strSequencesProcesss As Integer = 1
'    Dim pkgLocation As String
'    Dim pkg As New Microsoft.SqlServer.Dts.Runtime.Package
'    Dim app As New Microsoft.SqlServer.Dts.Runtime.Application
'    Dim pkgResults As Microsoft.SqlServer.Dts.Runtime.DTSExecResult
'    Dim processnow As Boolean = False
'    Dim processStatus As Boolean = False

'    Try
'        processStatus = GetStatusEOD(EnumSystemParameter.EODStatusBigData)
'        If processStatus Then
'            If ProcessDate.Day = GetSystemParameterValue(EnumSystemParameter.ProcessDate) Then
'                processnow = True
'            End If
'            If processnow Then
'                pkgLocation = GetSystemParameterValue(EnumSystemParameter.PackagePath)
'                pkg = app.LoadPackage(pkgLocation, Nothing, Nothing)

'                If pkg.Connections.Count > 0 Then
'                    Dim objEncrypt As New EncryptionMIS
'                    For Each con As Microsoft.SqlServer.Dts.Runtime.ConnectionManager In pkg.Connections
'                        If con.Name = "sourceMISBigData" Then
'                            con.ConnectionString = GetSystemParameterValue(EnumSystemParameter.SourceDataSource)
'                        End If
'                        If con.Name = "destination" Then
'                            con.ConnectionString = "Data Source=" & GetSystemParameterValue(EnumSystemParameter.DestinationDataSource) & ";Initial Catalog=" + GetSystemParameterValue(EnumSystemParameter.DestinationDBName) + ";Provider=SQLNCLI11.1;Persist Security Info=True;User ID=" & GetSystemParameterValue(EnumSystemParameter.DestinationUsername) & ";Password=" & objEncrypt.Decrypt(GetSystemParameterValue(EnumSystemParameter.DestinationPassword))
'                        End If
'                    Next
'                End If
'                '
'                'pkgResults = pkg.Execute(Nothing, Nothing, Me, Nothing, Nothing)
'                pkgResults = pkg.Execute()
'                Select Case pkgResults
'                    Case Microsoft.SqlServer.Dts.Runtime.Wrapper.DTSExecResult.DTSER_CANCELED

'                    Case Microsoft.SqlServer.Dts.Runtime.Wrapper.DTSExecResult.DTSER_SUCCESS
'                        ConsoleLogInsert(dateProcess, "Process Import Data GL Oploss Big Data has been done in  " & dateProcess, strSequencesProcesss)
'                    Case Microsoft.SqlServer.Dts.Runtime.Wrapper.DTSExecResult.DTSER_FAILURE
'                        For Each lstError As Microsoft.SqlServer.Dts.Runtime.DtsError In pkg.Errors
'                            ConsoleLogInsert(dateProcess, "An error has been occurred Download MIS Big Data : " & lstError.Description, strSequencesProcesss)
'                        Next
'                    Case Microsoft.SqlServer.Dts.Runtime.Wrapper.DTSExecResult.DTSER_COMPLETION
'                End Select
'            End If
'        End If
'    Catch ex As Exception
'        ConsoleLogInsert(dateProcess, "An error has been occurred Process GL Data Oploss Big Data : " & ex.Message, 1)
'        Throw ex
'    End Try
'End Sub