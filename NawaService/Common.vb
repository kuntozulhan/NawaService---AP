Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Security.Cryptography

Module Common

    ''' <summary>
    ''' _ Decrypt Data _
    ''' </summary>
    ''' <param name="cleanString"></param>
    ''' <param name="cleansalt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DecryptRijndael(ByVal cleanString As String, ByVal cleansalt As String) As String
        Dim sym As New Encryption.Symmetric(Encryption.Symmetric.Provider.Rijndael)
        Dim key As New Encryption.Data(cleansalt)
        Dim encryptedData As New Encryption.Data
        encryptedData.Base64 = cleanString

        Dim decryptedData As Encryption.Data
        decryptedData = sym.Decrypt(encryptedData, key)
        Return decryptedData.ToString
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="ds"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function isValidDataset(ByVal ds As DataSet) As Boolean
        Try
            If Not (ds Is Nothing) Then
                If ds.Tables.Count > 0 Then
                    isValidDataset = True
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function isValidDatasetRow(ByVal ds As DataSet) As Boolean
        Try
            If Not (ds Is Nothing) Then
                If ds.Tables.Count > 0 Then
                    If ds.Tables(0).Rows.Count > 0 Then
                        isValidDatasetRow = True
                    End If
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function isValidDataTableRow(ByVal dt As DataTable) As Boolean
        Try
            If Not (dt Is Nothing) Then
                If dt.Rows.Count > 0 Then
                    isValidDataTableRow = True
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function isValidDataRow(ByVal dr As DataRow()) As Boolean
        Try
            If Not (dr Is Nothing) Then
                If dr.Length > 0 Then
                    isValidDataRow = True
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function ConvertToDate(ByVal strFormat As String, ByVal strDate As String) As DateTime
        Dim formats(0) As String
        formats(0) = strFormat
        Try
            Return DateTime.ParseExact(strDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Public Function IsCompleteValue(ByVal objValue As Object) As Boolean
        Dim strValue As String
        Dim blnComplete As Boolean
        Try
            If Not IsDBNull(objValue) Then
                strValue = CStr(objValue)
                If strValue.Trim.Length > 0 Then
                    blnComplete = True
                End If
            End If
            IsCompleteValue = blnComplete
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function isNotEmptyStringField(ByVal objValue As Object) As Boolean
        Dim strValue As String = ""
        Try
            If Not IsDBNull(objValue) Then
                strValue = CStr(objValue)
                If strValue.Length > 0 Then
                    isNotEmptyStringField = True
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function getStringFieldValue(ByVal objValue As Object) As String
        Dim strValue As String = ""
        Try
            If Not IsDBNull(objValue) Then
                strValue = CStr(objValue)
            End If
            Return strValue
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function getDateFieldValue(ByVal objValue As Object) As Date
        Try
            If Not IsDBNull(objValue) Then
                Return CDate(objValue)
            Else
                Return DateSerial(1900, 1, 1)
            End If
        Catch ex As Exception
            'Return DateSerial(1900, 1, 1)
            Throw ex
        End Try
    End Function

    Public Function getIntegerFieldValue(ByVal objValue As Object) As Integer
        Dim intValue As Integer
        intValue = 0
        Try
            If Not IsDBNull(objValue) Then
                intValue = CInt(objValue)
            End If
            Return intValue
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function getByteFieldValue(ByVal objValue As Object) As Byte
        Dim intValue As Byte
        Try
            If Not objValue Is Nothing Then
                If Not IsDBNull(objValue) Then
                    intValue = CByte(objValue)
                End If
            End If
            Return intValue
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function getLongFieldValue(ByVal objValue As Object) As Long
        Dim intValue As Long
        intValue = 0
        Try
            If Not IsDBNull(objValue) Then
                intValue = CLng(objValue)
            End If
            Return intValue
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function getSingleFieldValue(ByVal objValue As Object) As Single
        Dim intValue As Single
        intValue = -1
        Try
            If Not objValue Is Nothing Then
                If Not IsDBNull(objValue) Then
                    intValue = CSng(objValue)
                End If
            End If
            Return intValue
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function getDoubleFieldValue(ByVal objValue As Object) As Double
        Dim intValue As Double
        intValue = -1
        Try
            If Not objValue Is Nothing Then
                If Not IsDBNull(objValue) Then
                    intValue = CDbl(objValue)
                End If
            End If
            Return intValue
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Function getDecimalFieldValue(ByVal objValue As Object) As Decimal
        Dim intValue As Decimal
        intValue = -1
        Try
            If Not objValue Is Nothing Then
                If Not IsDBNull(objValue) Then
                    intValue = CDec(objValue)
                End If
            End If
            Return intValue
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function ColumnEqual(ByVal A As Object, ByVal B As Object) As Boolean
        '
        ' Compares two values to determine if they are equal. Also compares DBNULL.Value.
        '
        ' NOTE: If your DataTable contains object fields, you must extend this
        ' function to handle the fields in a meaningful way if you intend to group on them.
        '
        If A Is DBNull.Value And B Is DBNull.Value Then Return True ' Both are DBNull.Value.
        If A Is DBNull.Value Or B Is DBNull.Value Then Return False ' Only one is DBNull.Value.
        Return A Is B                                                ' Value type standard comparison
    End Function

    Public Function getSelectDistinct(ByVal TableName As String, ByVal ds As DataSet, ByVal FieldName As String, ByVal FieldType As Type) As DataTable
        Dim LastValue As Object
        Dim dr As DataRow
        Dim dt As New DataTable(TableName)
        dt.Columns.Add(FieldName, FieldType)
        LastValue = Nothing
        If isValidDatasetRow(ds) Then
            For Each dr In ds.Tables(0).Rows
                If LastValue Is Nothing OrElse Not ColumnEqual(LastValue, dr(FieldName)) Then
                    LastValue = dr(FieldName)
                    dt.Rows.Add(New Object() {LastValue})
                End If
            Next
        End If
        Return dt
    End Function

    Public Function getSelectDistinct(ByVal TableName As String, ByVal dt As DataTable, ByVal FieldName As String, ByVal FieldType As Type) As DataTable
        Dim LastValue As Object
        Dim dr As DataRow
        Dim ReturnDT As New DataTable(TableName)
        ReturnDT.Columns.Add(FieldName, FieldType)
        LastValue = Nothing
        If isValidDataTableRow(dt) Then
            For Each dr In dt.Rows
                If LastValue Is Nothing OrElse Not ColumnEqual(LastValue, dr(FieldName)) Then
                    LastValue = dr(FieldName)
                    ReturnDT.Rows.Add(New Object() {LastValue})
                End If
            Next
        End If
        Return ReturnDT
    End Function

    Public Function Date2IndonesiaDate(ByVal datValue As Date) As String
        Dim strResult As String
        strResult = datValue.Day.ToString
        Select Case datValue.Month
            Case 1
                strResult = strResult & " Januari "
            Case 2
                strResult = strResult & " Februari "
            Case 3
                strResult = strResult & " Maret "
            Case 4
                strResult = strResult & " April "
            Case 5
                strResult = strResult & " Mei "
            Case 6
                strResult = strResult & " Juni "
            Case 7
                strResult = strResult & " Juli "
            Case 8
                strResult = strResult & " Agustus "
            Case 9
                strResult = strResult & " September "
            Case 10
                strResult = strResult & " Oktober "
            Case 11
                strResult = strResult & " November "
            Case 12
                strResult = strResult & " Desember "
        End Select
        strResult = strResult & datValue.Year.ToString
        Date2IndonesiaDate = strResult
    End Function

    Public Function Month2IndonesiaMonth(ByVal intMonth As Integer) As String
        Dim strResult As String
        strResult = ""
        Select Case intMonth
            Case 1
                strResult = "Januari"
            Case 2
                strResult = "Februari"
            Case 3
                strResult = "Maret"
            Case 4
                strResult = "April"
            Case 5
                strResult = "Mei"
            Case 6
                strResult = "Juni"
            Case 7
                strResult = "Juli"
            Case 8
                strResult = "Agustus"
            Case 9
                strResult = "September"
            Case 10
                strResult = "Oktober"
            Case 11
                strResult = "November"
            Case 12
                strResult = "Desember"
        End Select
        Month2IndonesiaMonth = strResult
    End Function

    Function Julian2Date(ByVal JulDate As Integer) As Date
        Dim NormalDate As Date

        NormalDate = DateSerial(CType(Int(JulDate / 1000), Integer), 1, JulDate Mod 1000)
        Return NormalDate
    End Function

    Function Date2Julian(ByVal NormalDate As Date) As Integer
        Dim datJulian As Integer
        datJulian = (Year(NormalDate) * 1000) + CType(Format(DatePart(DateInterval.DayOfYear, NormalDate), "000"), Integer)
        Return datJulian
    End Function

    Public Function FormatTimeSpan(ByVal ts As TimeSpan) As String
        Return ts.Days & " day(s) " & Format(ts.Hours, "00") & ":" & Format(ts.Minutes, "00") & ":" & Format(ts.Seconds, "00")
    End Function

    Public Function GetJustAlphaNumeric(ByVal StringData As String) As String
        Dim Counter As Integer
        Dim StringNumber As String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Dim StringReturn As String = ""
        Dim S1 As Char
        StringData = StringData.ToUpper
        For Counter = 0 To StringData.Length - 1
            S1 = StringData.Chars(Counter)
            If StringNumber.IndexOf(S1) > -1 Then
                StringReturn += S1
            End If
        Next
        Return StringReturn
    End Function

    'Public Function GetDataset(ByVal SQLQuery As String) As DataSet
    '    Dim SqlConn As SqlConnection = Nothing
    '    Dim SqlDa As SqlDataAdapter = Nothing
    '    Dim Ds As DataSet = Nothing
    '    Try
    '        Ds = New DataSet
    '        SqlConn = New SqlConnection(My.Settings.CTRConnectionString)
    '        SqlConn.Open()
    '        SqlDa = New SqlDataAdapter(SQLQuery, SqlConn)
    '        SqlDa.SelectCommand.CommandTimeout = My.Settings.SQLCommandTimeout
    '        SqlDa.Fill(Ds)
    '        Return Ds
    '    Catch tex As Threading.ThreadAbortException
    '        Throw tex
    '    Catch ex As Exception
    '        Throw ex
    '    Finally
    '        If Not SqlConn Is Nothing Then
    '            SqlConn.Close()
    '            SqlConn.Dispose()
    '            SqlConn = Nothing
    '        End If
    '        If Not SqlDa Is Nothing Then
    '            SqlDa.Dispose()
    '            SqlDa = Nothing
    '        End If
    '        If Not Ds Is Nothing Then
    '            Ds.Dispose()
    '            Ds = Nothing
    '        End If
    '    End Try
    'End Function

    'Public Function ExecScalar(ByVal SQLQuery As String) As Object
    '    Dim SqlConn As SqlConnection = Nothing
    '    Dim SqlCmd As SqlCommand = Nothing
    '    Try
    '        SqlConn = New SqlConnection(My.Settings.CTRConnectionString)
    '        SqlConn.Open()
    '        SqlCmd = New SqlCommand
    '        SqlCmd.Connection = SqlConn
    '        SqlCmd.CommandTimeout = My.Settings.SQLCommandTimeout
    '        SqlCmd.CommandText = SQLQuery
    '        Return SqlCmd.ExecuteScalar()
    '    Catch tex As Threading.ThreadAbortException
    '        Throw tex
    '    Catch ex As Exception
    '        Throw ex
    '    Finally
    '        If Not SqlConn Is Nothing Then
    '            SqlConn.Close()
    '            SqlConn.Dispose()
    '            SqlConn = Nothing
    '        End If
    '        If Not SqlCmd Is Nothing Then
    '            SqlCmd.Dispose()
    '            SqlCmd = Nothing
    '        End If
    '    End Try
    'End Function

    'Public Function ExecNonQuery(ByVal SQLQuery As String) As Integer
    '    Dim SqlConn As SqlConnection = Nothing
    '    Dim SqlCmd As SqlCommand = Nothing
    '    Try
    '        SqlConn = New SqlConnection(My.Settings.CTRConnectionString)
    '        SqlConn.Open()
    '        SqlCmd = New SqlCommand
    '        SqlCmd.Connection = SqlConn
    '        SqlCmd.CommandTimeout = My.Settings.SQLCommandTimeout
    '        SqlCmd.CommandText = SQLQuery
    '        Return SqlCmd.ExecuteNonQuery()
    '    Catch tex As Threading.ThreadAbortException
    '        Throw tex
    '    Catch ex As Exception
    '        Throw ex
    '    Finally
    '        If Not SqlConn Is Nothing Then
    '            SqlConn.Close()
    '            SqlConn.Dispose()
    '            SqlConn = Nothing
    '        End If
    '        If Not SqlCmd Is Nothing Then
    '            SqlCmd.Dispose()
    '            SqlCmd = Nothing
    '        End If
    '    End Try
    'End Function

    'Public Function ClearTable(ByVal TableName As String) As Boolean
    '    Dim SqlConn As SqlConnection = Nothing
    '    Dim SqlCmd As SqlCommand = Nothing
    '    Try
    '        SqlConn = New SqlConnection(My.Settings.CTRConnectionString)
    '        SqlConn.Open()
    '        SqlCmd = New SqlCommand("IF EXISTS(SELECT name FROM sysobjects WHERE  name = N'" + TableName + "' AND type = 'U') TRUNCATE TABLE " + TableName, SqlConn)
    '        SqlCmd.CommandTimeout = My.Settings.SQLCommandTimeout
    '        Return SqlCmd.ExecuteNonQuery > 0
    '    Catch tex As Threading.ThreadAbortException
    '        Throw tex
    '    Catch ex As Exception
    '        Throw ex
    '    Finally
    '        If Not SqlConn Is Nothing Then
    '            SqlConn.Close()
    '            SqlConn.Dispose()
    '            SqlConn = Nothing
    '        End If
    '        If Not SqlCmd Is Nothing Then
    '            SqlCmd.Dispose()
    '            SqlCmd = Nothing
    '        End If
    '    End Try
    'End Function

    Public Function DisplayError(ByVal Ex As Exception, ByVal HeaderErrorMessage As String) As Boolean
        Dim ErrorMessage As String
        ErrorMessage = HeaderErrorMessage & vbCrLf
        ErrorMessage += "Error Detail: " & vbCrLf
        ErrorMessage += "Source: " & Ex.Source & vbCrLf
        ErrorMessage += "Message: " & Ex.Message & vbCrLf
        ErrorMessage += "Stack Trace: " & Ex.StackTrace
    End Function

    'Public Function IsConnectionExists(ByRef strServerName As String, ByRef strDatabaseName As String, ByRef strErrorMessage As String) As Boolean
    '    Dim sqlconn As SqlConnection = Nothing
    '    Try
    '        sqlconn = New SqlConnection(My.Settings.CTRConnectionString)
    '        strServerName = sqlconn.DataSource
    '        strDatabaseName = sqlconn.Database
    '        sqlconn.Open()
    '        Return True
    '    Catch ex As Exception
    '        strErrorMessage = ex.Message
    '    Finally
    '        If Not sqlconn Is Nothing Then
    '            If sqlconn.State = ConnectionState.Open Then
    '                sqlconn.Close()
    '            End If
    '        End If
    '        sqlconn = Nothing
    '    End Try
    'End Function

End Module

Public Enum EnumProcessStatus
    Idle
    Starting
    Running
    Finished
    [Error]
    Canceled
    Waiting
    Unknown
End Enum

Public Enum EnumProcessResult
    Finished
    [Error]
    Canceled
End Enum

Public Enum EnumProgressValueType
    Number
    Percent
End Enum

Public Enum EnumProgressListViewColumn
    Source = 0
    Starttime = 1
    Endtime = 2
    ExecutionTime = 3
    Status = 4
    Progress = 5
    Total = 6
    ErrorCode = 7
    Description = 8

End Enum