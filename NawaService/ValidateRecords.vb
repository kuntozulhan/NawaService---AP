Imports CookComputing.XmlRpc

Public Class ValidateRecords
    Inherits ListenerService
    Private mylog As New NawaConsoleLog

    Sub run()
        'Using objDb As NawaDataEntities = New NawaDataEntities
        '    objDb.Database.ExecuteSqlCommand("exec usp_GenerateFiles")
        'End Using
        '     ValidateRecordsFromWeb("03", "2016", ModuleName)
    End Sub

    Public Shared Function ValidateRecordsFromWeb()
        ValidateRecordsFromWeb(Nothing, Nothing, Nothing)
    End Function

    <XmlRpcMethod("ValidateRecordsFromWeb")> _
    Public Shared Function ValidateRecordsFromWeb(Bulan As String, ModuleName() As String, Tahun As String) As Boolean
        For i As Integer = 0 To ModuleName.Length - 1
            Console.WriteLine("Start. Param:" & Bulan & "," & Tahun & "," & ModuleName(i).ToString)
            Dim paramBulan As New Data.SqlClient.SqlParameter("@bulan", Bulan)
            Dim paramTahun As New Data.SqlClient.SqlParameter("@tahun", Tahun)

            Using objDb As NawaDataEntities = New NawaDataEntities
                objDb.Database.ExecuteSqlCommand("exec usp_VALIDATE_" & Left(ModuleName(i).ToString, 3) & " @bulan,@tahun", paramBulan, paramTahun)
            End Using
        Next
    End Function
End Class
