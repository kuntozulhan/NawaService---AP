
Public Class NawaConsoleLog
    Private Shared ReadOnly myLog As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Sub LogInfo(objmessage As Object)
        Try
            Using objDb As NawaDataEntities = New NawaDataEntities
                Dim objlog As New LogConsoleService
                With objlog
                    .LogCreatedDate = DateTime.Now
                    .LogInfo = objmessage.ToString
                    .LogDescription = objmessage.ToString
                    .LogStatus = "INFO"
                End With
                objDb.LogConsoleServices.Add(objlog)
                objDb.SaveChanges()
            End Using
        Catch ex As Exception
            myLog.Error("An error has been occured in Process LogInfo", ex)
        End Try
    End Sub
    Sub LogError(ByVal objmessage As Object, ByVal objexception As System.Exception)
        Try
            Using objDb As NawaDataEntities = New NawaDataEntities
                Dim objlog As New LogConsoleService
                With objlog
                    .LogCreatedDate = DateTime.Now
                    .LogInfo = objmessage.ToString
                    .LogDescription = objexception.ToString
                    .LogStatus = "ERROR"
                End With
                objDb.LogConsoleServices.Add(objlog)
                objDb.SaveChanges()
            End Using
        Catch ex As Exception
            myLog.Error("An error has been occured in Process LogInfo", ex)
        End Try
    End Sub
End Class
