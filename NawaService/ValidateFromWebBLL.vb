Imports System.Threading

Public Class ValidateFromWebBLL
    Implements IDisposable

    Public Bulan As String
    Public Tahun As String
    Public kodecabang As String
    Public ModuleName() As String

    Dim thisLock As New Object

    Public Sub RunValidate()
        SyncLock (thisLock)
            Try
                'Update dulu ke Validation Summary, set statusnya jadi in progress
                For i As Integer = 0 To ModuleName.Length - 1
                    Console.WriteLine("Start. Param:" & Bulan & "," & Tahun & "," & ModuleName(i).ToString)
                    Dim strQuery As String = "exec Usp_ValidationSummary_InsertInitial '" & Bulan & "', '" & Tahun & "', '" & kodecabang & "', '" & ModuleName(i) & "'"
                    SQLHelper.ExecuteDataSet(SQLHelper.strConnectionString, CommandType.Text, strQuery)
                Next

                For i As Integer = 0 To ModuleName.Length - 1
                    Console.WriteLine("Start. Param:" & Bulan & "," & Tahun & "," & ModuleName(i).ToString)
                    Dim strQuery As String = "exec usp_ExecuteValidationBySegmentData '" & Bulan & "', '" & Tahun & "', '" & kodecabang & "', '" & ModuleName(i) & "'"
                    SQLHelper.ExecuteDataSet(SQLHelper.strConnectionString, CommandType.Text, strQuery)
                Next
            Catch ex As Exception
                Dim strLogError As String = ex.Message.ToString
            End Try
        End SyncLock
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
