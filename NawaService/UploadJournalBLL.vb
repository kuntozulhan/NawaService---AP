
Imports System.Threading
Imports System.Data.SqlClient
Imports System.Data

Public Class UploadJournalBLL
    Implements IDisposable

    Private mylog As New NawaConsoleLog

    Sub run()
        Dim lngFk_EODTaskDetailLog_ID As Long = 0
        Try
            While True
                Thread.Sleep(My.Settings.IntThreadInterval)

                Using objDb As NawaDataEntities = New NawaDataEntities
                    Dim objTrxDafnom_Print As List(Of TrxDafnom_Print) = objDb.TrxDafnom_Print.Where(Function(x) x.Fk_MsStatusUpload_Id = 2).Take(1).ToList
                    For Each item As TrxDafnom_Print In objTrxDafnom_Print
                        UploadClaim(item.Pk_TrxDafnom_Print_Id)
                    Next

                End Using

            End While
        Catch ex As Exception
            'Update Status
            Dim strQuery As String = "UPDATE LogSFTPService SET Fk_MsStatusUpload_Id = 2 WHERE Fk_EODTaskDetailLog_ID = " & lngFk_EODTaskDetailLog_ID
            SQLHelper.ExecuteNonQuery(SQLHelper.strConnectionString, CommandType.Text, strQuery, Nothing)

            mylog.LogError("An Error has been occurred on Upload Journal: ", ex)
        End Try
    End Sub

#Region "Claim"

    Private Function UploadClaim(ByVal lngPk_TrxGenerateTextfileClaim_Id As Long) As Boolean
        Dim BolReturn As Boolean = True

        Using ObjSFTPBLL As New SFTPBLL
            If ObjSFTPBLL.GetSFTPSettingValue(EnumSFTPSetting.CekSFTP) = "1" Then
                Using objdb As New NawaDataEntities
                    Dim objTrxDafnom_Print As TrxDafnom_Print = objdb.TrxDafnom_Print.Where(Function(x) x.Pk_TrxDafnom_Print_Id = lngPk_TrxGenerateTextfileClaim_Id).FirstOrDefault
                    If Not objTrxDafnom_Print Is Nothing Then
                        'InsertEODLog(lngFk_EODTaskDetailLog_ID, "Process Upload Journal SFTP", "Upload file CA to SFTP Started")

                        'Copy file to local folder path
                        Dim strFilePath As String = ""
                        strFilePath = ObjSFTPBLL.GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath) & "\" & objTrxDafnom_Print.AttachmentFileName
                        System.IO.File.WriteAllBytes(strFilePath, objTrxDafnom_Print.AttachmentFileContent)

                        Try
                            'Upload to SFTP
                            ''Cek, apakah file Journal nya utk Cash Advance atau Settlement
                            'Dim IsFolderDC As Boolean = False
                            'Dim ObjCADetail As TrxGenerateTextfileClaim_Detail = objdb.TrxGenerateTextfileClaim_Detail.Where(Function(x) x.Fk_TrxGenerateTextfileClaim_Id = lngPk_TrxGenerateTextfileClaim_Id).FirstOrDefault
                            'If Not ObjCADetail Is Nothing Then
                            '    If ObjCADetail.Fk_GeneratedTextfileType_Id = 2 Then
                            '        IsFolderDC = True
                            '    End If
                            'End If

                            'If IsFolderDC Then
                            '    ObjSFTPBLL.PutFileSFTP(EnumSFTPSetting.PathFolderDC)
                            'Else
                            ObjSFTPBLL.PutFileSFTP(EnumSFTPSetting.PathFolderDC)
                            'End If

                            'Update Status di TrxGenerateTextfileClaim
                            objTrxDafnom_Print.Fk_MsStatusUpload_Id = 4
                            objTrxDafnom_Print.TanggalUpload = Date.Now
                            objTrxDafnom_Print.UploadMessage = ""
                        Catch ex As Exception
                            'Update Status di TrxGenerateTextfileClaim
                            objTrxDafnom_Print.Fk_MsStatusUpload_Id = 5
                            objTrxDafnom_Print.TanggalUpload = Date.Now
                            objTrxDafnom_Print.UploadMessage = ex.Message.ToString
                            BolReturn = False
                        Finally
                            'Delete all file
                            Dim strLocalFolderPath As String = ObjSFTPBLL.GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath)
                            For Each deleteFile In System.IO.Directory.GetFiles(strLocalFolderPath)
                                System.IO.File.Delete(deleteFile)
                            Next
                        End Try

                        objdb.Entry(objTrxDafnom_Print).State = Entity.EntityState.Modified
                        objdb.SaveChanges()

                        'InsertEODLog(lngFk_EODTaskDetailLog_ID, "Process Upload Journal SFTP", "Upload file CA to SFTP Finish")
                    End If
                End Using
            End If
        End Using

        Return BolReturn
    End Function

    Private Function GenerateClaim(ByVal lngFk_EODTaskDetailLog_ID As Long) As Boolean
        'Dim BolReturn As Boolean = True

        ''Generate File utk Transaksi & Journal
        'If CountClaimToGenerate() > 0 Then
        '    Dim lngPk_TrxGenerateTextfileClaim_Id As Long = 0

        '    InsertEODLog(lngFk_EODTaskDetailLog_ID, "Process Upload Journal SFTP", "Generate Journal Claim Started")
        '    lngPk_TrxGenerateTextfileClaim_Id = GenerateTransaksiAndJournalClaim()
        '    InsertEODLog(lngFk_EODTaskDetailLog_ID, "Process Upload Journal SFTP", "Generate Journal Claim Finish")

        '    Using ObjSFTPBLL As New SFTPBLL
        '        If ObjSFTPBLL.GetSFTPSettingValue(EnumSFTPSetting.CekSFTP) = "1" Then
        '            Using objdb As New NawaDataEntities
        '                Dim objTrxGenerateTextfileClaim As TrxGenerateTextfileClaim = objdb.TrxGenerateTextfileClaims.Where(Function(x) x.Pk_TrxGenerateTextfileClaim_Id = lngPk_TrxGenerateTextfileClaim_Id).FirstOrDefault
        '                If Not objTrxGenerateTextfileClaim Is Nothing Then
        '                    InsertEODLog(lngFk_EODTaskDetailLog_ID, "Process Upload Journal SFTP", "Upload file Claim to SFTP Started")

        '                    'Copy file to local folder path
        '                    Dim strFilePath As String = ""
        '                    strFilePath = ObjSFTPBLL.GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath) & "\" & Replace(objTrxGenerateTextfileClaim.FileJournalName, ".csv", Date.Now.ToString("yyyyMMdd_HHmmss") & ".csv")
        '                    System.IO.File.WriteAllBytes(strFilePath, objTrxGenerateTextfileClaim.FileJournalContent)

        '                    Try
        '                        'Upload to SFTP
        '                        ObjSFTPBLL.PutFileSFTP(EnumSFTPSetting.PathFolderFT)

        '                        'Update Status di TrxGenerateTextfileClaim
        '                        objTrxGenerateTextfileClaim.IsUploadSukses = 1
        '                        objTrxGenerateTextfileClaim.TanggalUpload = Date.Now
        '                        objTrxGenerateTextfileClaim.UploadMessage = ""
        '                    Catch ex As Exception
        '                        'Update Status di TrxGenerateTextfileClaim
        '                        objTrxGenerateTextfileClaim.IsUploadSukses = 0
        '                        objTrxGenerateTextfileClaim.TanggalUpload = Date.Now
        '                        objTrxGenerateTextfileClaim.UploadMessage = ex.Message.ToString
        '                        BolReturn = False
        '                    Finally
        '                        'Delete all file
        '                        Dim strLocalFolderPath As String = ObjSFTPBLL.GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath)
        '                        For Each deleteFile In System.IO.Directory.GetFiles(strLocalFolderPath)
        '                            System.IO.File.Delete(deleteFile)
        '                        Next
        '                    End Try

        '                    objdb.Entry(objTrxGenerateTextfileClaim).State = Entity.EntityState.Modified
        '                    objdb.SaveChanges()

        '                    InsertEODLog(lngFk_EODTaskDetailLog_ID, "Process Upload Journal SFTP", "Upload file Claim to SFTP Finish")
        '                End If
        '            End Using
        '        End If
        '    End Using
        'End If

        'Return BolReturn
    End Function

    Private Function GenerateTransaksiAndJournalClaim() As Long
        'Dim lngReturn As Long = 0
        'Dim sbGenerateTransaksi As New System.Text.StringBuilder
        'Dim strTransaksiSeparator As String = ","

        'Using objdb As New NawaDataEntities
        '    'Transaksi
        '    For Each ObjClaim As Vw_Claim_GenerateTransaksi In objdb.Vw_Claim_GenerateTransaksi.ToList
        '        With ObjClaim
        '            Dim isGenerateTransaksi As Boolean = True
        '            Dim strNoRekening As String = ""
        '            Dim strNamaUser As String = ""
        '            Dim dblNominalTransfer As Double = 0

        '            strNoRekening = .AccountNo
        '            strNamaUser = .UserName

        '            Select Case .Fk_MsStatusApproval_Id
        '                Case 7  'Claimsh Advance
        '                    dblNominalTransfer = .TotalPengajuanKlaim
        '            End Select
        '            If isGenerateTransaksi Then
        '                sbGenerateTransaksi.Append(strNoRekening).Append(strTransaksiSeparator).Append(strNamaUser).Append(strTransaksiSeparator).Append(dblNominalTransfer.ToString).Append(vbCrLf)
        '            End If
        '        End With
        '    Next

        '    'Journal
        '    Dim sbGenerateJournal As New System.Text.StringBuilder
        '    Dim objdt As DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "EXEC Usp_GenerateJournalClaim '" & Date.Now.ToString("yyyyMMdd") & "'", Nothing)
        '    For Each ObjDtRow As DataRow In objdt.Rows
        '        sbGenerateJournal.Append(ObjDtRow.Item("Journal").ToString).Append(vbCrLf)
        '    Next

        '    'Update Table
        '    Dim ObjGenerate As New TrxGenerateTextfileClaim
        '    With ObjGenerate
        '        .GeneratedDate = Date.Now
        '        .GeneratedBy = "System"

        '        'Transaksi
        '        .FileTransferName = "TransaksiClaim.csv"
        '        Dim enc As New System.Text.UTF8Encoding
        '        Dim arrBytData() As Byte = enc.GetBytes(sbGenerateTransaksi.ToString)
        '        .FileTransferContent = arrBytData

        '        'Journal
        '        .FileJournalName = "JournalClaim.csv"
        '        Dim encJournal As New System.Text.UTF8Encoding
        '        Dim arrBytDataJournal() As Byte = encJournal.GetBytes(sbGenerateJournal.ToString)
        '        .FileJournalContent = arrBytDataJournal
        '    End With

        '    'Update Status Paid
        '    Using objtrans As System.Data.Entity.DbContextTransaction = objdb.Database.BeginTransaction()
        '        Try
        '            objdb.Entry(ObjGenerate).State = Entity.EntityState.Added
        '            objdb.SaveChanges()
        '            lngReturn = ObjGenerate.Pk_TrxGenerateTextfileClaim_Id

        '            'Update Status di Claim
        '            For Each objVwClaim As Vw_Claim_GenerateListId In objdb.Vw_Claim_GenerateListId.ToList
        '                Dim objTrxClaim As TrxClaim = objdb.TrxClaims.Where(Function(x) x.Pk_TrxClaim_Id = objVwClaim.Pk_TrxClaim_Id).FirstOrDefault
        '                If Not objTrxClaim Is Nothing Then
        '                    With objTrxClaim
        '                        Select Case .Fk_MsStatusApproval_Id
        '                            Case 7  'Claim
        '                                .Fk_MsStatusApproval_Id = 7 'Verified
        '                                .Fk_MsStatusDokumen_Id = 6  'Paid
        '                                .Fk_MsStatusGeneral_Id = 1  'Outstanding
        '                        End Select

        '                        .LastUpdateBy = "System"
        '                        .LastUpdateDate = Date.Now
        '                        .ApprovedBy = "System"
        '                        .ApprovedDate = Date.Now
        '                    End With
        '                    objdb.Entry(objTrxClaim).State = Entity.EntityState.Modified

        '                    UpdateApprovalLog("Generate Journal Claim", objTrxClaim.Pk_TrxClaim_Id, objdb)
        '                End If
        '            Next

        '            objdb.SaveChanges()
        '            objtrans.Commit()
        '        Catch ex As Exception
        '            objtrans.Rollback()
        '            Throw
        '        End Try
        '    End Using
        'End Using

        'Return lngReturn
    End Function

    Function CountClaimToGenerate() As Integer
        Dim intReturn As Integer = 0

        intReturn = CInt(SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, "SELECT COUNT(1) FROM TrxClaim tca WHERE Fk_MsStatusApproval_Id = 7 AND Fk_MsStatusDokumen_Id = 5", Nothing))

        Return intReturn
    End Function
#End Region

    Private Function InsertEODLog(ByVal lngFk_EODTaskDetailLog_ID As Long, ByVal strProcess As String, ByVal strKeterangan As String) As Boolean
        Dim strQuery As String = ""
        strQuery = "INSERT INTO EODLogSP(executionID, ProcessDate, Process, Keterangan) "
        strQuery &= "VALUES('" & lngFk_EODTaskDetailLog_ID.ToString & "', '" & Date.Now.ToString("yyyyMMdd") & "', '" & strProcess & "', '" & strKeterangan & "')"

        SQLHelper.ExecuteNonQuery(SQLHelper.strConnectionString, CommandType.Text, strQuery, Nothing)

        Return True
    End Function

    'Private Function UpdateApprovalLog(ByVal strAction As String, ByVal lngPk_TrxCashAdvance_Id As Long, ByRef objdb As NawaDataEntities) As Boolean
    '    Dim ObjTrxCashAdvance_Log As New TrxCashAdvance_Log
    '    With ObjTrxCashAdvance_Log
    '        .Fk_TrxCashAdvance_Id = lngPk_TrxCashAdvance_Id
    '        .UserId = "System"
    '        .UpdatedDate = Date.Now
    '        .UserAction = strAction
    '    End With
    '    objdb.Entry(ObjTrxCashAdvance_Log).State = Entity.EntityState.Added
    '    objdb.SaveChanges()

    '    Return True
    'End Function

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
