Imports CookComputing.XmlRpc
Imports System.Threading

Public Class ServicesForWeb
    Inherits ListenerService
    Private mylog As New NawaConsoleLog

    Sub run()
        'Using objDb As NawaDataEntities = New NawaDataEntities
        '    objDb.Database.ExecuteSqlCommand("exec usp_GenerateFiles")
        'End Using
        'GenerateTextFileFromWeb()
    End Sub

    Public Shared Function GenerateTextFileFromWeb()
        GenerateTextFileFromWeb(Nothing, Nothing, Nothing, Nothing, Nothing)
    End Function

    <XmlRpcMethod("GenerateTextFileFromWeb")>
    Public Shared Function GenerateTextFileFromWeb(Bulan As String, Tahun As String, ModuleName() As String, KodeCabang As String, UserName As String) As Boolean
        'Insert D01
        Console.WriteLine("Enter Generate. Param:" & Bulan & "," & Tahun & "," & KodeCabang)

        Dim Modules As String = ""
        For i As Integer = 0 To ModuleName.Length - 1
            DeletePrevious(Bulan, Tahun, ModuleName(i), KodeCabang)
            InsertBase(Bulan, Tahun, ModuleName(i), KodeCabang, UserName)
            InsertRecordCount(Bulan, Tahun, ModuleName(i), KodeCabang)
            If Modules <> "" Then
                Modules = Modules + ","
            End If
            Modules = Modules + "'" + Left(ModuleName(i), 3) + "'"
        Next

        Dim ProcessedRecords As DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text,
        "SELECT * FROM GeneratedFileList WHERE Bulan = '" & Bulan & "' AND Tahun = '" & Tahun & "' AND (FK_KodeCabang_ID = '" & KodeCabang & "') AND FK_KodeSegmen_ID IN (" & Modules & ")", Nothing)

        For Each items As DataRow In ProcessedRecords.Rows
            Using objDb As NawaDataEntities = New NawaDataEntities
                Dim SQL As String = ""

                'Update Status
                SQL = "UPDATE GeneratedFileList SET GenerationStatus = 'PROCESSING', UpdatedBy='System',UpdatedDate=GETDATE()"
                SQL += "WHERE PK_GeneratedFileList_ID=" & items("PK_GeneratedFileList_ID")
                SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, SQL, Nothing)

                'Isi data temporary
                Dim paramFileName As New Data.SqlClient.SqlParameter("@recordID", items("PK_GeneratedFileList_ID"))
                Dim paramBulan As New Data.SqlClient.SqlParameter("@bulan", items("Bulan"))
                Dim paramTahun As New Data.SqlClient.SqlParameter("@tahun", items("Tahun"))
                Dim paramKodeSegmen As New Data.SqlClient.SqlParameter("@KodeSegmen", items("FK_KodeSegmen_ID"))
                Dim paramKodeCabang As New Data.SqlClient.SqlParameter("@KodeCabang", items("FK_KodeCabang_ID"))

                objDb.Database.ExecuteSqlCommand("exec usp_GenerateFileContent @recordID,@bulan,@tahun,@KodeSegmen,@KodeCabang", paramFileName, paramBulan, paramTahun, paramKodeSegmen, paramKodeCabang)

                'Update ke table actual
                Dim paramFileName3 As New Data.SqlClient.SqlParameter("@recordID", items("PK_GeneratedFileList_ID"))
                Dim paramBulan3 As New Data.SqlClient.SqlParameter("@bulan", items("Bulan"))
                Dim paramTahun3 As New Data.SqlClient.SqlParameter("@tahun", items("Tahun"))
                Dim paramKodeSegmen3 As New Data.SqlClient.SqlParameter("@KodeSegmen", items("FK_KodeSegmen_ID"))
                Dim paramKodeCabang3 As New Data.SqlClient.SqlParameter("@KodeCabang", items("FK_KodeCabang_ID"))
                objDb.Database.ExecuteSqlCommand("exec usp_FillFileContent @recordID,@bulan,@tahun,@KodeSegmen,@KodeCabang", paramFileName3, paramBulan3, paramTahun3, paramKodeSegmen3, paramKodeCabang3)
            End Using
        Next
    End Function

    Protected Shared Sub DeletePrevious(Bulan As String, Tahun As String, ModuleName As String, KodeCabang As String)
        'Hapus dulu data yang sudah ada supaya tidak duplikat
        Console.WriteLine("Enter delete. Param:" & Bulan & "," & Tahun & "," & ModuleName & "," & KodeCabang)
        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim paramBulan As New Data.SqlClient.SqlParameter("@bulan", Bulan)
            Dim paramTahun As New Data.SqlClient.SqlParameter("@tahun", Tahun)
            Dim paramModule As New Data.SqlClient.SqlParameter("@modulename", Left(ModuleName, 3))
            Dim paramCabang As New Data.SqlClient.SqlParameter("@kodecabang", KodeCabang)

            objDb.Database.ExecuteSqlCommand("exec usp_CleanFiles @bulan,@tahun,@modulename,@kodecabang", paramBulan, paramTahun, paramModule, paramCabang)
        End Using
    End Sub

    Protected Shared Sub InsertBase(Bulan As String, Tahun As String, ModuleName As String, KodeCabang As String, UserName As String)
        'Insert dulu default value ke table untuk menunjukkan bahwa dokumen sedang digenerate
        Console.WriteLine("Enter insert. Param:" & Bulan & "," & Tahun & "," & ModuleName & "," & KodeCabang)
        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim paramBulan As New Data.SqlClient.SqlParameter("@bulan", Bulan)
            Dim paramTahun As New Data.SqlClient.SqlParameter("@tahun", Tahun)
            Dim paramModule As New Data.SqlClient.SqlParameter("@modulename", ModuleName)
            Dim paramCabang As New Data.SqlClient.SqlParameter("@kodecabang", KodeCabang)
            Dim paramUser As New Data.SqlClient.SqlParameter("@username", UserName)

            objDb.Database.ExecuteSqlCommand("exec usp_GenerateFiles @bulan,@tahun,@modulename,@kodecabang,@username", paramBulan, paramTahun, paramModule, paramCabang, paramUser)
        End Using
    End Sub

    Protected Shared Sub InsertRecordCount(Bulan As String, Tahun As String, ModuleName As String, KodeCabang As String)
        'Insert dulu default value ke table untuk menunjukkan bahwa dokumen sedang digenerate
        Console.WriteLine("Enter insert count. Param:" & Bulan & "," & Tahun & "," & ModuleName & "," & KodeCabang)
        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim paramBulan As New Data.SqlClient.SqlParameter("@bulan", Bulan)
            Dim paramTahun As New Data.SqlClient.SqlParameter("@tahun", Tahun)
            Dim paramModule As New Data.SqlClient.SqlParameter("@modulename", ModuleName)
            Dim paramCabang As New Data.SqlClient.SqlParameter("@kodecabang", KodeCabang)

            objDb.Database.ExecuteSqlCommand("exec usp_InsertCounts @bulan,@tahun,@modulename,@kodecabang", paramBulan, paramTahun, paramModule, paramCabang)
        End Using
    End Sub

    <XmlRpcMethod("ValidateRecordsFromWeb")>
    Public Shared Function ValidateRecordsFromWeb(Bulan As String, ModuleName() As String, Tahun As String, kodecabang As String) As Boolean
        For i As Integer = 0 To ModuleName.Length - 1
            Console.WriteLine("Start. Param:" & Bulan & "," & Tahun & "," & ModuleName(i).ToString)
            'Dim paramBulan As New Data.SqlClient.SqlParameter("@bulan", Bulan)
            'Dim paramTahun As New Data.SqlClient.SqlParameter("@tahun", Tahun)
            'Dim paramkodecabang As New Data.SqlClient.SqlParameter("@kodekantorcabang", kodecabang)
            'Dim parammodule As New Data.SqlClient.SqlParameter("@ModuleTable", ModuleName(i))

            'Using objDb As NawaDataEntities = New NawaDataEntities
            '    objDb.Database.ExecuteSqlCommand("exec usp_ExecuteValidationBySegmentData @bulan,@tahun,@kodekantorcabang,@ModuleTable", paramBulan, paramTahun, paramkodecabang, parammodule)
            'End Using
        Next

        Dim objtest As New ServicesForWeb
        objtest.ThreadStartValidateRecords(Bulan, ModuleName, Tahun, kodecabang)
    End Function

    Private ObjValidate As ValidateFromWebBLL
    Private trdValidateRecord As Thread
    Private Sub ThreadStartValidateRecords(Bulan As String, ModuleName() As String, Tahun As String, kodecabang As String)
        ObjValidate = New ValidateFromWebBLL
        ObjValidate.Bulan = Bulan
        ObjValidate.ModuleName = ModuleName
        ObjValidate.Tahun = Tahun
        ObjValidate.kodecabang = kodecabang

        trdValidateRecord = New Thread(AddressOf ObjValidate.RunValidate)
        trdValidateRecord.IsBackground = True
        trdValidateRecord.Start()
    End Sub

    <XmlRpcMethod("CleanRecords")>
    Public Shared Function CleanRecords(RecordID() As String,
                          UserName As String) As Boolean
        Console.WriteLine("Clean records")
        Console.WriteLine("Record count:" & RecordID.Length)

        Dim paramUserName As New Data.SqlClient.SqlParameter("@UserName", UserName)
        Dim Records As String = ""
        For i As Integer = 0 To RecordID.Length - 1
            Console.WriteLine("Record processed:" & RecordID(i))

            Records = Records + RecordID(i) + ","
        Next

        Dim paramRecordID As New Data.SqlClient.SqlParameter("@RecordID", Records)
        Using objDb As NawaDataEntities = New NawaDataEntities
            objDb.Database.ExecuteSqlCommand("exec usp_ExecuteCleansing @RecordID,@UserName", paramRecordID, paramUserName)
        End Using
    End Function

    <XmlRpcMethod("CleanRecordsAll")>
    Public Shared Function CleanRecordsAll(UserName As String) As Boolean
        Console.WriteLine("Clean records all")

        Dim paramUserName As New Data.SqlClient.SqlParameter("@UserName", UserName)
        Using objDb As NawaDataEntities = New NawaDataEntities
            'objDb.Database.ExecuteSqlCommand("exec usp_ExecuteCleansing @RecordID,@UserName", paramRecordID, paramUserName)
            objDb.Database.ExecuteSqlCommand("exec usp_ExecuteCleansing NULL,@UserName", paramUserName)
        End Using
    End Function

    <XmlRpcMethod("InsertAsDictionary")>
    Public Shared Function InsertAsDictionary(RecordID() As String) As Boolean
        Console.WriteLine("Insert to Dictionary")
        Console.WriteLine("Record count:" & RecordID.Length)

        If RecordID.Length < 1 Then
            Using objDb As NawaDataEntities = New NawaDataEntities
                objDb.Database.ExecuteSqlCommand("exec usp_InsertDataDictionary NULL")
            End Using
        Else
            For i As Integer = 0 To RecordID.Length - 1
                Console.WriteLine("Record processed:" & RecordID(i))
                Dim paramRecordID As New Data.SqlClient.SqlParameter("@RecordID", RecordID(i))
                Using objDb As NawaDataEntities = New NawaDataEntities
                    objDb.Database.ExecuteSqlCommand("exec usp_InsertDataDictionary @RecordID", paramRecordID)
                End Using
            Next
        End If
    End Function
    '<XmlRpcMethod("CallProcessRPCFromWeb")> _
    'Public Shared Function CallProcessRPCFromWeb(ByVal ValSchedulerId As String, ByVal ValProcessDate As String, ByVal ValProcessDownloadFromMISICBS As String, ByVal ValProcessDownloadFromMISNCBS As String, ByVal ValProcessGenerateCTRReport As String, ByVal ValProcessPurgingData As String, ByVal ValProcessNegativeScreeningDaily As String, ByVal ValProcessNegativeScreeningMonthly As String, ByVal ValProcessRemittanceNewsScreening As String, ByVal ValProcessDataQualityMonitoring As String, ByVal ValPocessDataNasabah As String, ByVal ValDownloadFileWorldcheckDeletedDaily As String, ByVal ValDownloadFileWorldcheckDeletedWeekly As String, ByVal ValDownloadFileWorldcheckDeletedMonthly As String, ByVal ValDownloadFileWorldcheckNewUpdateDaily As String, ByVal ValDownloadFileWorldcheckNewUpdateWeekly As String, ByVal ValDownloadFileWorldcheckNewUpdateMonthly As String, ByVal ValExtractSaveFileWorldcheckDeletedDaily As String, ByVal ValExtractSaveFileWorldcheckDeletedWeekly As String, ByVal ValExtractSaveFileWorldcheckDeletedMonthly As String, ByVal ValExtractSaveFileWorldcheckNewUpdateDaily As String, ByVal ValExtractSaveFileWorldcheckNewUpdateWeekly As String, ByVal ValExtractSaveFileWorldcheckNewUpdateMonthly As String, ByVal ValProcessIFTIReport As String) As Boolean
    '    Dim LongSchedulerId As Long = Convert.ToInt64(ValSchedulerId)
    '    Dim DateProcessDate As DateTime = Convert.ToDateTime(ValProcessDate)
    '    Dim BolProcessDownloadDataFromMISICBS As Boolean = Convert.ToBoolean(ValProcessDownloadFromMISICBS)
    '    Dim BolProcessDownloadDataFromMISNCBS As Boolean = Convert.ToBoolean(ValProcessDownloadFromMISNCBS)
    '    Dim bolProcessCTRReport As Boolean = Convert.ToBoolean(ValProcessGenerateCTRReport)
    '    Dim bolProcessPurgingData As Boolean = Convert.ToBoolean(ValProcessPurgingData)

    '    Dim BolProcessNegativeScreeningDaily As Boolean = Convert.ToBoolean(ValProcessNegativeScreeningDaily)
    '    Dim BolProcessNegativeScreeningMonthly As Boolean = Convert.ToBoolean(ValProcessNegativeScreeningMonthly)
    '    Dim bolProcessRemittanceNewsScreening As Boolean = Convert.ToBoolean(ValProcessRemittanceNewsScreening)
    '    Dim bolProcessDataQualityMonitoring As Boolean = Convert.ToBoolean(ValProcessDataQualityMonitoring)
    '    Dim bolProcessDataNasabah As Boolean = Convert.ToBoolean(ValPocessDataNasabah)


    '    Dim bolDownloadFileWorldcheckDeletedDaily As Boolean = Convert.ToBoolean(ValDownloadFileWorldcheckDeletedDaily)
    '    Dim bolDownloadFileWorldcheckDeletedWeekly As Boolean = Convert.ToBoolean(ValDownloadFileWorldcheckDeletedWeekly)
    '    Dim bolDownloadFileWorldcheckDeletedMonthly As Boolean = Convert.ToBoolean(ValDownloadFileWorldcheckDeletedMonthly)
    '    Dim bolDownloadFileWorldcheckNewUpdateDaily As Boolean = Convert.ToBoolean(ValDownloadFileWorldcheckNewUpdateDaily)
    '    Dim bolDownloadFileWorldcheckNewUpdateWeekly As Boolean = Convert.ToBoolean(ValDownloadFileWorldcheckNewUpdateWeekly)
    '    Dim bolDownloadFileWorldcheckNewUpdateMonthly As Boolean = Convert.ToBoolean(ValDownloadFileWorldcheckNewUpdateMonthly)
    '    Dim bolExtractSaveFileWorldcheckDeletedDaily As Boolean = Convert.ToBoolean(ValExtractSaveFileWorldcheckDeletedDaily)
    '    Dim bolExtractSaveFileWorldcheckDeletedWeekly As Boolean = Convert.ToBoolean(ValExtractSaveFileWorldcheckDeletedWeekly)
    '    Dim bolExtractSaveFileWorldcheckDeletedMonthly As Boolean = Convert.ToBoolean(ValExtractSaveFileWorldcheckDeletedMonthly)
    '    Dim bolExtractSaveFileWorldcheckNewUpdateDaily As Boolean = Convert.ToBoolean(ValExtractSaveFileWorldcheckNewUpdateDaily)
    '    Dim bolExtractSaveFileWorldcheckNewUpdateWeekly As Boolean = Convert.ToBoolean(ValExtractSaveFileWorldcheckNewUpdateWeekly)
    '    Dim bolExtractSaveFileWorldcheckNewUpdateMonthly As Boolean = Convert.ToBoolean(ValExtractSaveFileWorldcheckNewUpdateMonthly)
    '    Dim bolprocessIFTIReport As Boolean = Convert.ToBoolean(ValProcessIFTIReport)

    '    Try
    '        InsertProgressLogETL("Start Process CallProcessManualFromWeb", DateProcessDate)
    '        Using ObjCTRConsole As New CTRService
    '            ObjCTRConsole.CallProcessManualFromWeb(LongSchedulerId, DateProcessDate, BolProcessDownloadDataFromMISICBS, BolProcessDownloadDataFromMISNCBS, bolProcessCTRReport, bolProcessPurgingData, BolProcessNegativeScreeningDaily, BolProcessNegativeScreeningMonthly, bolProcessRemittanceNewsScreening, bolProcessDataQualityMonitoring, bolProcessDataNasabah, False, False, False, False, False, False, False, False, False, False, bolDownloadFileWorldcheckDeletedDaily, bolDownloadFileWorldcheckDeletedWeekly, bolDownloadFileWorldcheckDeletedMonthly, bolDownloadFileWorldcheckNewUpdateDaily, bolDownloadFileWorldcheckNewUpdateWeekly, bolDownloadFileWorldcheckNewUpdateMonthly, bolExtractSaveFileWorldcheckDeletedDaily, bolExtractSaveFileWorldcheckDeletedWeekly, bolExtractSaveFileWorldcheckDeletedMonthly, bolExtractSaveFileWorldcheckNewUpdateDaily, bolExtractSaveFileWorldcheckNewUpdateWeekly, bolExtractSaveFileWorldcheckNewUpdateMonthly, bolprocessIFTIReport)
    '        End Using
    '        InsertProgressLogETL("End Process CallProcessManualFromWeb", DateProcessDate)
    '        Return True
    '    Catch ex As Exception
    '        myLog.Error(ex.StackTrace)
    '        Return False
    '    End Try
    'End Function
End Class
