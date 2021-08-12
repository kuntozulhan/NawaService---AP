Imports System.Threading

Public Class CheckSFTPStatusBLL
    Implements IDisposable

    Private mylog As New NawaConsoleLog

    Sub run()
        Try
            While True
                Thread.Sleep(My.Settings.IntThreadInterval * 60)

                CekUploadClaim()
            End While
        Catch ex As Exception
            mylog.LogError("An Error has been occurred on Check SFTP Status: ", ex)
        End Try
    End Sub

    Sub CekUploadClaim()
        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim objTrxDafnom_Print As List(Of TrxDafnom_Print) = objDb.TrxDafnom_Print.Where(Function(x) x.Fk_MsStatusUpload_Id = 4).Take(1).ToList
            For Each item As TrxDafnom_Print In objTrxDafnom_Print
                Dim IsSukses As Boolean = False
                Using ObjSFTPBLL As New SFTPBLL
                    If Not ObjSFTPBLL.IsSFTPFileExist(item.AttachmentFileName, EnumSFTPSetting.PathFolderFT) Then
                        item.Fk_MsStatusUpload_Id = 6
                        item.TanggalUpload = Date.Now
                        IsSukses = True
                    Else
                        item.RetryCount = item.RetryCount + 1
                        If item.RetryCount >= 10 Then
                            item.Fk_MsStatusUpload_Id = 5
                        End If
                    End If
                End Using

                objDb.Entry(item).State = Entity.EntityState.Modified
                objDb.SaveChanges()

            Next
        End Using
    End Sub

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
