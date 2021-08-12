Imports System.Net.Mail

Public Class EmailTemplateBLL
    Private mylog As New NawaConsoleLog
    Sub run()

        'looping di emailtemplate yg aktif dan status monitoringnya bukan 1(none)
        While True
            Try

                ''Sementara dicomment dulu, jadi langsung send email aja
                'Using objDb As NawaDataEntities = New NawaDataEntities
                '    objDb.Database.ExecuteSqlCommand("exec usp_SchedulerEmail ")
                'End Using

                ''insert detailnya
                'Dim dt As Data.DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT * FROM EmailTemplateScheduler ets INNER JOIN EmailTemplate et ON ets.PK_EmailTemplate_ID=et.PK_EmailTemplate_ID WHERE ets.FK_EmailStatus_ID=1")
                'For Each item As DataRow In dt.Rows
                '    Dim dtTablePrimary As Data.DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT eta.NamaTable, eta.QueryData, eta.FieldUnikPrimaryTable  FROM EmailTemplateAdditional eta WHERE eta.FK_EmailTableType_ID=1 AND eta.FK_EmailTemplate_ID=" & item("PK_EmailTemplate_ID"), Nothing)

                '    For Each item1 As DataRow In dtTablePrimary.Rows
                '        Using objDb As NawaDataEntities = New NawaDataEntities
                '            Dim objparamtable As New Data.SqlClient.SqlParameter("@tablename", item1("NamaTable"))
                '            Dim objparamquerydata As New Data.SqlClient.SqlParameter("@querydata", System.Net.WebUtility.HtmlDecode(item1("QueryData")))
                '            objDb.Database.ExecuteSqlCommand("exec usp_CreateTableEmailPrimary @tablename,@querydata", objparamtable, objparamquerydata)
                '        End Using

                '        Dim sql As String
                '        sql = "INSERT INTO EmailTemplateSchedulerDetail "
                '        sql += " ( "
                '        sql += " 	 "
                '        sql += " 	FK_EmailTEmplateScheduler_ID, "
                '        sql += " 	UnikFieldTablePrimary, "
                '        sql += " 	EmailTo, "
                '        sql += " 	EmailCC, "
                '        sql += " 	EmailBCC, "
                '        sql += " 	EmailSubject, "
                '        sql += " 	EmailBody, "
                '        sql += " 	ProcessDate, "
                '        sql += " 	SendEmailDate, "
                '        sql += " 	FK_EmailStatus_ID, "
                '        sql += " 	ErrorMessage, "
                '        sql += " 	retrycount "
                '        sql += " ) "
                '        sql += " select  "
                '        sql += " '" & item("PK_EmailTemplateScheduler_ID") & "' ,"
                '        sql += " " & item1("FieldUnikPrimaryTable") & " ,"
                '        sql += " '" & item("EmailTo") & "' ,"
                '        sql += " '" & item("EmailCC") & "' ,"
                '        sql += " '" & item("EmailBCC") & "' ,"
                '        sql += " '" & item("EmailSubject") & "' ,"
                '        sql += " '" & item("EmailBody") & "' ,"
                '        sql += " '" & CDate(item("ProcessDate")).ToString("yyyy-MM-dd HH:mm:ss") & "' ,"
                '        sql += " null ,"
                '        sql += " '1' ,"
                '        sql += " '' ,"
                '        sql += " '0' "
                '        sql += " from __" & item1("NamaTable")

                '        SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, sql, Nothing)

                '        'buat table detail dulu untuk setiap master
                '        Dim dtschedulerdetail As DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT * FROM EmailTemplateSchedulerDetail WHERE FK_EmailTEmplateScheduler_ID=" & item("PK_EmailTemplateScheduler_ID"), Nothing)
                '        Dim dtemaildetail As DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT etd.Replacer , SUBSTRING(etd.FieldReplacer,1,1)+'__'+SUBSTRING(etd.FieldReplacer,2,LEN(etd.FieldReplacer)-1) AS FieldReplacer  FROM EmailTemplateDetail etd WHERE etd.FK_EmailTemplate_ID=" & item("PK_EmailTemplate_ID"), Nothing)

                '        For Each rowschedulerdetail As DataRow In dtschedulerdetail.Rows
                '            Dim dtTableAdditional As DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT eta.NamaTable, eta.QueryData, eta.FieldUnikPrimaryTable  FROM EmailTemplateAdditional eta WHERE eta.FK_EmailTableType_ID=2 AND eta.FK_EmailTemplate_ID=" & item("PK_EmailTemplate_ID"), Nothing)

                '            For Each item2 As DataRow In dtTableAdditional.Rows
                '                'generate table additional detail
                '                Using objDb As NawaDataEntities = New NawaDataEntities
                '                    Dim strquery As String = System.Net.WebUtility.HtmlDecode(item2("QueryData"))
                '                    strquery = strquery.Replace("@ID", "'" & rowschedulerdetail("UnikFieldTablePrimary") & "'")

                '                    Dim objparamtable As New Data.SqlClient.SqlParameter("@tablename", item2("NamaTable"))
                '                    Dim objparamquerydata As New Data.SqlClient.SqlParameter("@querydata", strquery)
                '                    objDb.Database.ExecuteSqlCommand("exec usp_CreateTableEmailPrimary @tablename,@querydata", objparamtable, objparamquerydata)
                '                End Using
                '            Next

                '            'replace isi tabledetail
                '            For Each rowreplacer As DataRow In dtemaildetail.Rows
                '                Using objDb As NawaDataEntities = New NawaDataEntities
                '                    Dim objparampk As New Data.SqlClient.SqlParameter("@PK_EmailTemplateSchedulerDetail_ID", rowschedulerdetail("PK_EmailTemplateSchedulerDetail_ID"))
                '                    Dim objparamreplacer As New Data.SqlClient.SqlParameter("@Replacer", "'" & rowreplacer("Replacer") & "'")
                '                    Dim objparamfieldreplacer As New Data.SqlClient.SqlParameter("@FieldReplacer", rowreplacer("FieldReplacer"))
                '                    Dim objparampkemailtemplate As New Data.SqlClient.SqlParameter("@pk_emailitemplate_id", item("PK_EmailTemplate_ID"))

                '                    objDb.Database.ExecuteSqlCommand("exec usp_replaceEmailSchedulerDetail @PK_EmailTemplateSchedulerDetail_ID,@Replacer,@FieldReplacer,@pk_emailitemplate_id", objparampk, objparamreplacer, objparamfieldreplacer, objparampkemailtemplate)
                '                End Using
                '            Next

                '            'update status jadi queue untuk EmailTemplateScheduler dan EmailTemplateSchedulerdetail
                '        Next
                '    Next

                '    Using objDb As NawaDataEntities = New NawaDataEntities
                '        Dim objparampk As New Data.SqlClient.SqlParameter("@PK_EmailTemplateScheduler_ID", item("PK_EmailTemplateScheduler_ID"))
                '        objDb.Database.ExecuteSqlCommand("exec usp_updateEmailStatusScheduler @PK_EmailTemplateScheduler_ID", objparampk)
                '    End Using
                'Next

                UpdateEmailScheduler()

                'send email
                Dim intretrycounttosend = 0
                Using objDb As NawaDataEntities = New NawaDataEntities
                    Dim objsysparam As SystemParameter = objDb.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = 2012).FirstOrDefault
                    If Not objsysparam Is Nothing Then
                        intretrycounttosend = objsysparam.SettingValue
                    End If
                End Using

                Dim dtEmail As DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT * FROM EmailTemplateScheduler a INNER JOIN EmailTemplateSchedulerDetail b ON a.PK_EmailTemplateScheduler_ID=b.FK_EmailTEmplateScheduler_ID WHERE (b.FK_EmailStatus_ID=2 OR b.FK_EmailStatus_ID=3 or b.FK_EmailStatus_ID=5 ) and b.retrycount<" & intretrycounttosend, Nothing)

                Dim strsender As String = ""
                Using objDb As NawaDataEntities = New NawaDataEntities
                    Dim objsysparam As SystemParameter = objDb.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = 2005).FirstOrDefault
                    If Not objsysparam Is Nothing Then
                        strsender = objsysparam.SettingValue
                    End If
                End Using

                For Each item As DataRow In dtEmail.Rows
                    Try
                        ''update status 3 (inprogress)
                        Using objDb As NawaDataEntities = New NawaDataEntities

                            Dim objparampk As New Data.SqlClient.SqlParameter("@PK_EmailTemplateSchedulerDetail_ID", item("PK_EmailTemplateSchedulerDetail_ID"))
                            Dim objparamfk As New Data.SqlClient.SqlParameter("@FkStatusemailid", 3)
                            Dim objparamerr As New Data.SqlClient.SqlParameter("@errorMessage", "")
                            objDb.Database.ExecuteSqlCommand("exec usp_updateEmailStatusSchedulerProcess @PK_EmailTemplateSchedulerDetail_ID,@FkStatusemailid,@errorMessage", objparampk, objparamfk, objparamerr)
                        End Using
                        SendEmail(strsender, item("EmailTo"), item("EmailCC"), item("EmailBCC"), item("EmailSubject"), item("EmailBody"))

                        ''update status 4 (done/sucesssend)
                        Using objDb As NawaDataEntities = New NawaDataEntities

                            Dim objparampk As New Data.SqlClient.SqlParameter("@PK_EmailTemplateSchedulerDetail_ID", item("PK_EmailTemplateSchedulerDetail_ID"))
                            Dim objparamfk As New Data.SqlClient.SqlParameter("@FkStatusemailid", 4)
                            Dim objparamerr As New Data.SqlClient.SqlParameter("@errorMessage", "")
                            objDb.Database.ExecuteSqlCommand("exec usp_updateEmailStatusSchedulerProcess @PK_EmailTemplateSchedulerDetail_ID,@FkStatusemailid,@errorMessage", objparampk, objparamfk, objparamerr)
                        End Using

                    Catch ex As Exception
                        ''update status 5 (failtosend)
                        Using objDb As NawaDataEntities = New NawaDataEntities
                            Dim objparampk As New Data.SqlClient.SqlParameter("@PK_EmailTemplateSchedulerDetail_ID", item("PK_EmailTemplateSchedulerDetail_ID"))
                            Dim objparamerr As New Data.SqlClient.SqlParameter("@errorMessage", ex.Message)
                            objDb.Database.ExecuteSqlCommand("exec usp_updateLogErrorEmail @PK_EmailTemplateSchedulerDetail_ID,@errorMessage", objparampk, objparamerr)
                        End Using
                    End Try
                Next
            Catch ex As Exception
                mylog.LogError(ex.Message, ex)
            End Try
        End While
    End Sub

    Sub UpdateEmailScheduler()
        Dim dtTable As Data.DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT Pk_EmailTemplateSystem_Id, RecordID, emailTemplateID FROM EmailTemplateSystem WHERE ISNULL(IsEmailSend, 0) = 0")
        For Each DtRow As Data.DataRow In dtTable.Rows
            Dim Pk_EmailTemplateSystem_Id As Integer = DtRow.Item("Pk_EmailTemplateSystem_Id").ToString
            Dim inttemplateid As Integer = DtRow.Item("emailTemplateID").ToString
            Dim RecordID As Integer = DtRow.Item("RecordID").ToString

            Dim objparam(1) As System.Data.SqlClient.SqlParameter

            objparam(0) = New SqlClient.SqlParameter
            objparam(0).ParameterName = "@PkmoduleApprovalid"
            objparam(0).SqlDbType = SqlDbType.BigInt
            objparam(0).Value = RecordID

            objparam(1) = New SqlClient.SqlParameter
            objparam(1).ParameterName = "@inttemplateid"
            objparam(1).SqlDbType = SqlDbType.Int
            objparam(1).Value = inttemplateid

            Dim dt As Data.DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.StoredProcedure, "usp_InserEmailSchedulerModuleApproval", objparam)
            For Each item As DataRow In dt.Rows
                Dim dtTablePrimary As Data.DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT eta.NamaTable, eta.QueryData, eta.FieldUnikPrimaryTable  FROM EmailTemplateAdditional eta WHERE eta.FK_EmailTableType_ID=1 AND eta.FK_EmailTemplate_ID=" & inttemplateid, Nothing)

                For Each item1 As DataRow In dtTablePrimary.Rows
                    Dim strqueryx As String = System.Net.WebUtility.HtmlDecode(item1("QueryData"))
                    strqueryx = strqueryx.Replace("@ID", RecordID)
                    Using objDb As NawaDataEntities = New NawaDataEntities

                        Dim objparamtable As New Data.SqlClient.SqlParameter("@tablename", item1("NamaTable"))
                        Dim objparamquerydata As New Data.SqlClient.SqlParameter("@querydata", strqueryx)

                        objDb.Database.ExecuteSqlCommand("exec usp_CreateTableEmailPrimary @tablename,@querydata", objparamtable, objparamquerydata)
                    End Using

                    Dim sql As String
                    sql = "INSERT INTO EmailTemplateSchedulerDetail "
                    sql += " ( "
                    sql += " 	 "
                    sql += " 	FK_EmailTEmplateScheduler_ID, "
                    sql += " 	UnikFieldTablePrimary, "
                    sql += " 	EmailTo, "
                    sql += " 	EmailCC, "
                    sql += " 	EmailBCC, "
                    sql += " 	EmailSubject, "
                    sql += " 	EmailBody, "
                    sql += " 	ProcessDate, "
                    sql += " 	SendEmailDate, "
                    sql += " 	FK_EmailStatus_ID, "
                    sql += " 	ErrorMessage, "
                    sql += " 	retrycount "
                    sql += " ) "
                    sql += " select  "
                    sql += " '" & item("PK_EmailTemplateScheduler_ID") & "' ,"
                    sql += " " & item1("FieldUnikPrimaryTable") & " ,"
                    sql += " '" & item("EmailTo") & "' ,"
                    sql += " '" & item("EmailCC") & "' ,"
                    sql += " '" & item("EmailBCC") & "' ,"
                    sql += " '" & item("EmailSubject") & "' ,"
                    sql += " '" & item("EmailBody") & "' ,"
                    sql += " '" & CDate(item("ProcessDate")).ToString("yyyy-MM-dd HH:mm:ss") & "' ,"
                    sql += " null ,"
                    sql += " '1' ,"
                    sql += " '' ,"
                    sql += " '0' "
                    sql += " from __" & item1("NamaTable")

                    SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, sql, Nothing)

                    'buat table detail dulu untuk setiap master
                    Dim dtschedulerdetail As DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT * FROM EmailTemplateSchedulerDetail WHERE FK_EmailTEmplateScheduler_ID=" & item("PK_EmailTemplateScheduler_ID"), Nothing)
                    Dim dtemaildetail As DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT etd.Replacer , SUBSTRING(etd.FieldReplacer,1,1)+'__'+SUBSTRING(etd.FieldReplacer,2,LEN(etd.FieldReplacer)-1) AS FieldReplacer  FROM EmailTemplateDetail etd WHERE etd.FK_EmailTemplate_ID=" & item("PK_EmailTemplate_ID"), Nothing)

                    For Each rowschedulerdetail As DataRow In dtschedulerdetail.Rows
                        Dim dtTableAdditional As DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, "SELECT eta.NamaTable, eta.QueryData, eta.FieldUnikPrimaryTable  FROM EmailTemplateAdditional eta WHERE eta.FK_EmailTableType_ID=2 AND eta.FK_EmailTemplate_ID=" & item("PK_EmailTemplate_ID"), Nothing)
                        For Each item2 As DataRow In dtTableAdditional.Rows
                            'generate table additional detail
                            Using objDb As NawaDataEntities = New NawaDataEntities
                                Dim strquery As String = System.Net.WebUtility.HtmlDecode(item2("QueryData"))
                                strquery = strquery.Replace("@ID", "'" & rowschedulerdetail("UnikFieldTablePrimary") & "'")

                                Dim objparamtable As New Data.SqlClient.SqlParameter("@tablename", item2("NamaTable"))
                                Dim objparamquerydata As New Data.SqlClient.SqlParameter("@querydata", strquery)

                                objDb.Database.ExecuteSqlCommand("exec usp_CreateTableEmailPrimary @tablename,@querydata", objparamtable, objparamquerydata)
                            End Using
                        Next

                        'replace isi tabledetail
                        For Each rowreplacer As DataRow In dtemaildetail.Rows
                            Using objDb As NawaDataEntities = New NawaDataEntities
                                Dim objparampk As New Data.SqlClient.SqlParameter("@PK_EmailTemplateSchedulerDetail_ID", rowschedulerdetail("PK_EmailTemplateSchedulerDetail_ID"))
                                Dim objparamreplacer As New Data.SqlClient.SqlParameter("@Replacer", "'" & rowreplacer("Replacer") & "'")
                                Dim objparamfieldreplacer As New Data.SqlClient.SqlParameter("@FieldReplacer", rowreplacer("FieldReplacer"))
                                Dim objparampkemailtemplate As New Data.SqlClient.SqlParameter("@pk_emailitemplate_id", item("PK_EmailTemplate_ID"))

                                objDb.Database.ExecuteSqlCommand("exec usp_replaceEmailSchedulerDetail @PK_EmailTemplateSchedulerDetail_ID,@Replacer,@FieldReplacer,@pk_emailitemplate_id", objparampk, objparamreplacer, objparamfieldreplacer, objparampkemailtemplate)
                            End Using
                        Next
                        'update status jadi queue untuk EmailTemplateScheduler dan EmailTemplateSchedulerdetail
                    Next
                Next

                Using objDb As NawaDataEntities = New NawaDataEntities
                    Dim objparampk As New Data.SqlClient.SqlParameter("@PK_EmailTemplateScheduler_ID", item("PK_EmailTemplateScheduler_ID"))
                    objDb.Database.ExecuteSqlCommand("exec usp_updateEmailStatusScheduler @PK_EmailTemplateScheduler_ID", objparampk)
                End Using
            Next
            'Delete FROM EmailTemplateSystem
            SQLHelper.ExecuteDataSet(SQLHelper.strConnectionString, CommandType.Text, "UPDATE EmailTemplateSystem SET IsEmailSend = 1 WHERE Pk_EmailTemplateSystem_Id = " & Pk_EmailTemplateSystem_Id)
        Next
    End Sub

    Public Function SendEmail(ByVal sender As String, ByVal StrRecipientTo As String, ByVal StrRecipientCC As String, ByVal StrRecipientBCC As String, ByVal StrSubject As String, ByVal strbody As String) As Boolean
        Dim oEmail As EMail
        Try
            'oEmail = New EMail
            'oEmail.Sender = sender
            'oEmail.Recipient = StrRecipientTo
            'oEmail.RecipientCC = StrRecipientCC
            'oEmail.RecipientBCC = StrRecipientBCC
            'oEmail.Subject = StrSubject
            'oEmail.Body = strbody.Replace(vbLf, "<br>")
            'If oEmail.SendEmail() Then
            '    Return True
            'Else
            '    Return False
            'End If

            Dim querystring As String = ""
            Dim strApplicationName As String = ""
            Dim strEmailFrom As String = ""
            Dim strSMTPServer As String = ""
            Dim strEMailUser As String = ""
            Dim strEmailPassword As String = ""

            querystring = "SELECT SettingValue FROM SystemParameter WHERE PK_SystemParameter_ID = 1"
            strApplicationName = SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, querystring, Nothing)

            querystring = "SELECT SettingValue FROM SystemParameter WHERE PK_SystemParameter_ID = 2005"
            strEmailFrom = SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, querystring, Nothing)

            querystring = "SELECT SettingValue FROM SystemParameter WHERE PK_SystemParameter_ID = 2000"
            strSMTPServer = SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, querystring, Nothing)

            querystring = "SELECT SettingValue FROM SystemParameter WHERE PK_SystemParameter_ID = 2002"
            strEMailUser = SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, querystring, Nothing)

            querystring = "SELECT SettingValue FROM SystemParameter WHERE PK_SystemParameter_ID = 2003"
            strEmailPassword = SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, querystring, Nothing)

            'Start Send Email
            Dim EmailMessage As New MailMessage
            Dim EmailClient As New SmtpClient(strSMTPServer)
            EmailMessage.Subject = StrSubject
            EmailMessage.From = New MailAddress(strEmailFrom, strApplicationName)

            For Each strEmailTo As String In StrRecipientTo.Split(";")
                If strEmailTo.Trim <> "" Then
                    EmailMessage.To.Add(strEmailTo)
                End If
            Next
            For Each strEmailCc As String In StrRecipientCC.Split(";")
                If strEmailCc.Trim <> "" Then
                    EmailMessage.CC.Add(strEmailCc)
                End If
            Next
            For Each strEmailBcc As String In StrRecipientBCC.Split(";")
                If strEmailBcc.Trim <> "" Then
                    EmailMessage.Bcc.Add(strEmailBcc)
                End If
            Next
            EmailMessage.Body = strbody.Replace(vbLf, "<br>")
            EmailMessage.IsBodyHtml = True

            Try
                EmailClient.Credentials = New Net.NetworkCredential(strEMailUser, strEmailPassword)
                EmailClient.Send(EmailMessage)
                EmailMessage.Dispose()

                Return True
            Catch ex As Exception
                'Dim strErrorMessage As String = ex.Message
                Throw ex
            End Try


        Catch ex As Exception
            Throw ex
        End Try
    End Function
End Class
